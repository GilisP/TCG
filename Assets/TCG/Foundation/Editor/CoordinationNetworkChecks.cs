using System;
using System.Linq;
using UnityEngine;

namespace TCG.Foundation.Editor
{
    // In-memory protocol checks: no transport, disk profile, or timing dependency.
    public static class CoordinationNetworkChecks
    {
        static int checks;
        const string Content = "coordination-network-fixture";

        public static void Run()
        {
            checks = 0;
            foreach (int count in new[] { 2, 3, 4 }) CheckRoom(count);
            Debug.Log("COORDINATION NETWORK CHECKS PASSED: " + checks);
        }

        static void CheckRoom(int count)
        {
            double now = 1000;
            var catalog = new ContentCatalog();
            var cards = Enumerable.Range(0, count).Select(i => new CardData {
                id = "AG-N-private-" + i, name = "Private fixture " + i,
                kind = "Creature", attack = 1, defense = 2
            }).Concat(new[] {
                new CardData { id = "AG-N-commander", name = "Commander fixture", kind = "Creature", commander = true },
                new CardData { id = "AG-N-land", name = "Terrain fixture", kind = "Terrain" }
            }).ToArray();
            catalog.Add(new ExpansionData { id = "AG-N", title = "In-memory network fixture", cards = cards }, Array.Empty<string>());
            var effects = new EffectRegistry();
            var room = new NetworkRoom(catalog, effects, Content, count, false, () => now);
            var seq = new int[count];
            var connections = Enumerable.Range(0, count).Select(i => (ulong)(10 + i)).ToArray();
            var tokens = new string[count];

            Ok(room.Receive(900, new RoomRequest { type = "ready", sequence = 1, ready = true }).error.Length > 0,
                "unjoined connection cannot ready");
            Ok(room.Receive(900, new RoomRequest { type = "hello", content = "different" }).error.Length > 0,
                "content mismatch cannot join");
            for (int i = 0; i < count; i++)
            {
                var joined = room.Receive(connections[i], new RoomRequest { type = "hello", content = Content, name = "Fixture " + i });
                Ok(joined.seat == i && joined.ack == 0 && joined.token.Length >= 40, "initial seat and reconnect secret");
                tokens[i] = joined.token;
                var readyWithoutDeck = Send(i, new RoomRequest { type = "ready", ready = true });
                Ok(readyWithoutDeck.error.Length > 0 && readyWithoutDeck.ack == seq[i] && !room.Reply(i).members[i].ready,
                    "failed ready consumes sequence without readiness");
                var missing = Deck(i); missing.commander = "";
                Ok(Send(i, new RoomRequest { type = "deck", deck = missing }).error.Length > 0, "commanderless deck refused online");
                var valid = Deck(i);
                Ok(Send(i, new RoomRequest { type = "deck", deck = valid }).error.Length == 0, "valid experimental deck accepted");
                // Mutation of the sender's object after acceptance must not alter host storage.
                valid.commander = ""; valid.main.Clear(); valid.terrains.Clear(); valid.cardLooks.Clear();
                Ok(Send(i, new RoomRequest { type = "ready", ready = true }).error.Length == 0, "host retained independent deck copy");
            }
            Ok(tokens.Distinct().Count() == count, "each seat owns a distinct secret");
            Ok(Send(1, new RoomRequest { type = "start" }).error.Length > 0 && room.Game == null, "only host starts");
            var switchRequest = new RoomRequest { type = "deck", deck = Deck(1) };
            Ok(Send(1, switchRequest).error.Length == 0 && !room.Reply(0).members[1].ready, "deck selection clears readiness");
            Ok(Send(0, new RoomRequest { type = "start" }).error.Length > 0 && room.Game == null, "unready seat blocks start");
            Ok(Send(1, new RoomRequest { type = "ready", ready = true }).error.Length == 0, "new selection can be confirmed");
            var replayDeck = room.Receive(connections[1], switchRequest);
            Ok(replayDeck.error.Length == 0 && replayDeck.ack == seq[1] && replayDeck.members[1].ready,
                "old deck replay cannot undo subsequent readiness");
            int acknowledged = seq[1];
            var gap = room.Receive(connections[1], new RoomRequest { type = "ready", sequence = acknowledged + 2, ready = false });
            Ok(gap.error.Length > 0 && gap.ack == acknowledged && room.Reply(1).members[1].ready, "sequence gap has no lobby effects");
            Ok(Send(0, new RoomRequest { type = "start" }).error.Length == 0 && room.Game != null, "host starts confirmed copied decks");
            var game = room.Game;
            int revision = game.Revision;
            Ok(Send(1, new RoomRequest { type = "deck", deck = Deck(1) }).error.Length > 0 && game.Revision == revision,
                "deck cannot change during match");
            CheckViews();

            int active = game.Controller;
            int other = (active + 1) % count;
            Ok(Send(other, new RoomRequest { type = "action", command = new Command { player = active, revision = revision, kind = ActionKind.DrawMain, pile = 0 } }).error.Length > 0
                && game.Revision == revision, "connection cannot impersonate active seat");
            var draw = new RoomRequest { type = "action", command = new Command { player = active, revision = revision, kind = ActionKind.DrawMain, pile = 0 } };
            Ok(Send(active, draw).error.Length == 0 && game.Revision == revision + 1, "host executes legal draw once");
            Ok(room.Receive(connections[active], draw).error.Length == 0 && game.Revision == revision + 1,
                "identical sequence replay does not execute twice");
            Ok(Send(active, new RoomRequest { type = "action", command = new Command { player = active, revision = revision, kind = ActionKind.DrawMain, pile = 0 } }).error.Length > 0
                && game.Revision == revision + 1, "fresh sequence with stale revision rejected");
            Ok(Send(other, new RoomRequest { type = "action", command = new Command { player = other, revision = game.Revision, kind = ActionKind.DrawMain, pile = 0 } }).error.Length > 0 && game.Revision == revision + 1, "own seat still obeys host priority");
            CheckViews();

            room.Disconnect(connections[1]);
            Ok(room.Paused && room.SeatFor(connections[1]) == -1, "disconnected seat pauses match");
            int frozen = game.Revision;
            Ok(Send(0, new RoomRequest { type = "action", command = new Command { player = 0, revision = frozen, kind = ActionKind.Concede } }).error.Length > 0
                && game.Revision == frozen, "paused room refuses state mutation");
            now += 119;
            room.Tick();
            Ok(!game.Seats[1].Eliminated, "no abandonment before injected deadline");
            Ok(room.Receive(901, new RoomRequest { type = "hello", content = Content, token = "wrong" }).error.Length > 0,
                "invalid reconnect token rejected");
            connections[1] = 101;
            var resumed = room.Receive(connections[1], new RoomRequest { type = "hello", content = Content, token = tokens[1] });
            Ok(resumed.seat == 1 && resumed.ack == seq[1] && resumed.token == tokens[1] && !room.Paused && resumed.view.revision == frozen,
                "reconnect preserves seat secret sequence and current state");
            Ok(room.Receive(902, new RoomRequest { type = "hello", content = Content, token = tokens[1] }).error.Length > 0,
                "connected token cannot steal seat");
            Ok(room.Receive(11, new RoomRequest { type = "action", sequence = seq[1] + 1, command = new Command { player = 1, revision = frozen, kind = ActionKind.Concede } }).error.Length > 0,
                "previous connection loses authority after reconnect");
            var oldAction = room.Receive(connections[1], new RoomRequest { type = "action", sequence = seq[1], command = new Command { player = 1, revision = frozen, kind = ActionKind.Concede } });
            Ok(oldAction.error.Length == 0 && game.Revision == frozen && !game.Seats[1].Eliminated,
                "pre-reconnect sequence cannot concede restored seat");
            Ok(Send(1, new RoomRequest { type = "action", command = new Command { player = 0, revision = frozen, kind = ActionKind.Concede } }).error.Length > 0
                && game.Revision == frozen, "next sequence accepted but forged seat remains rejected");
            room.Disconnect(connections[1]); now += 120; room.Tick();
            Ok(game.Seats[1].Eliminated, "abandonment at exact injected deadline");
            int afterTimeout = game.Revision; room.Tick();
            Ok(game.Revision == afterTimeout, "timeout elimination is idempotent");
            Ok(room.Receive(903, new RoomRequest { type = "hello", content = Content, token = tokens[1] }).error.Length > 0,
                "expired reconnect rejected");
            // Finish through authenticated commands; abandoned seat 1 stays eliminated.
            for (int seat = 2; seat < count && !game.Over; seat++)
            {
                int beforeConcede = game.Revision;
                Ok(Send(seat, new RoomRequest { type = "action", command = new Command {
                    player = seat, revision = beforeConcede, kind = ActionKind.Concede
                } }).error.Length == 0 && game.Seats[seat].Eliminated && game.Revision == beforeConcede + 1,
                    "connected remaining seat concedes through host with current revision");
            }
            Ok(game.Over && game.WinningTeam == game.Seats[0].Team, "last surviving host team wins completed match");
            for (int seat = 0; seat < count; seat++)
            {
                var finalView = room.Reply(seat).view;
                var finalProjection = Match.FromView(JsonUtility.FromJson<MatchView>(JsonUtility.ToJson(finalView)), catalog, effects);
                Ok(finalView.over && finalView.winner == game.WinningTeam && finalView.revision == game.Revision
                    && finalProjection.Over && finalProjection.WinningTeam == game.WinningTeam && finalProjection.Revision == game.Revision,
                    "every seat including abandoned seat receives same final winner and revision");
            }
            room.Disconnect(connections[0]);
            Ok(room.Closed && room.Receive(904, new RoomRequest { type = "hello", content = Content }).error.Length > 0,
                "host departure closes authority");

            RoomReply Send(int seat, RoomRequest request)
            {
                request.sequence = ++seq[seat];
                return room.Receive(connections[seat], request);
            }
            void CheckViews()
            {
                var views = Enumerable.Range(0, count).Select(i => room.Reply(i).view).ToArray();
                string publicSeats = JsonUtility.ToJson(new PublicSeats { seats = views[0].seats });
                string publicBoard = JsonUtility.ToJson(new PublicBoard { cells = views[0].cells });
                for (int i = 0; i < count; i++)
                {
                    var view = views[i];
                    Ok(view.viewer == i && view.revision == game.Revision && view.active == game.Active && view.controller == game.Controller,
                        "projection follows authoritative public revision");
                    Ok(JsonUtility.ToJson(new PublicSeats { seats = view.seats }) == publicSeats && JsonUtility.ToJson(new PublicBoard { cells = view.cells }) == publicBoard,
                        "public seats board and shared terrain agree across viewers");
                    Ok(view.hand.Length == game.Seats[i].HandCount && view.hand.All(id => id == "AG-N-private-" + i), "only own hand definitions transmitted");
                    Ok(view.looks.Any(l => l.owner == i && l.card == "AG-N-private-" + i && l.foil) && view.looks.All(l => l.owner == i), "copied own foil remains visible while hidden opponent looks are omitted");
                    string wire = JsonUtility.ToJson(room.Reply(i));
                    Ok(Enumerable.Range(0, count).Where(j => j != i).All(j => !wire.Contains("AG-N-private-" + j) && !wire.Contains(tokens[j])),
                        "other hand arts and reconnect secrets absent from reply");
                    var projected = Match.FromView(JsonUtility.FromJson<MatchView>(JsonUtility.ToJson(view)), catalog, effects);
                    Ok(Enumerable.Range(0, count).Where(j => j != i).All(j => projected.HandFor(j).Count == 0)
                        && projected.Seats.Select(s => s.HandCount).SequenceEqual(game.Seats.Select(s => s.HandCount)), "projection hides other hands while preserving counts");
                    Ok(!projected.Try(new Command { player = i, revision = projected.Revision, kind = ActionKind.Concede }, out _)
                        && !projected.Seats[i].Eliminated && projected.Revision == game.Revision, "projection cannot act as match authority");
                }
            }
        }

        static DeckData Deck(int seat) => new DeckData {
            commander = "AG-N-commander", experimental = true,
            main = Enumerable.Repeat("AG-N-private-" + seat, 20).ToList(),
            terrains = Enumerable.Repeat("AG-N-land", 20).ToList(),
            cardLooks = new System.Collections.Generic.List<CardLook> { new CardLook { cardId = "AG-N-private-" + seat, styleId = "standard", foil = true } }
        };
        [Serializable] sealed class PublicSeats { public NetSeat[] seats; }
        [Serializable] sealed class PublicBoard { public NetCell[] cells; }
        static void Ok(bool value, string message)
        {
            if (!value) throw new Exception("COORDINATION NETWORK CHECK: " + message);
            checks++;
        }
    }
}
