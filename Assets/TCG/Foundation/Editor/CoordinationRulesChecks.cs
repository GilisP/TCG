using System;
using System.Collections.Generic;
using System.Linq;
using TCG.Table;
using UnityEngine;

namespace TCG.Foundation.Editor
{
    // In-memory fixtures exercise the existing collection contract without touching a profile.
    public static class CoordinationRulesChecks
    {
        static int checks;
        static void Check(bool ok, string label)
        {
            checks++;
            if (!ok) throw new InvalidOperationException("COORDINATION RULES: " + label);
        }
        static bool Has(CollectionLibrary library, DeckData deck, string message) =>
            library.Validate(deck, true).Any(error => error.Contains(message));
        static void Rejects(Action action, string label)
        {
            bool rejected = false;
            try { action(); } catch (InvalidOperationException) { rejected = true; }
            Check(rejected, label);
        }
        static CardData Card(string id, string kind, int color = 6) =>
            new CardData { id = id, name = "Coordination " + id, kind = kind, color = color };

        public static void Run()
        {
            checks = 0;
            var effects = new EffectRegistry();
            var catalog = ContentLoader.Load(effects);
            var cards = new List<CardData>();
            for (int i = 0; i < 100; i++) cards.Add(Card("coord-main-" + i, "Creature"));
            for (int i = 0; i < 50; i++) cards.Add(Card("coord-land-" + i, "Terrain"));
            var commander = Card("coord-commander", "Creature");
            commander.commander = true; commander.identityColors = new[] { 0 };
            cards.Add(commander);
            cards.Add(Card("coord-outside", "Creature", 1));
            var unavailable = Card("coord-unavailable", "Creature");
            unavailable.unavailableReason = "Coordination fixture unavailable"; cards.Add(unavailable);
            var unavailableCommander = Card("coord-unavailable-commander", "Creature");
            unavailableCommander.commander = true; unavailableCommander.identityColors = new[] { 0 };
            unavailableCommander.unavailableReason = "Coordination commander unavailable"; cards.Add(unavailableCommander);
            catalog.Add(new ExpansionData {
                id = "coordination-rules-fixtures", title = "Coordination in-memory fixtures",
                cards = cards.ToArray(), printings = new[] {
                    new PrintingData { id = "coord-main-reprint", cardId = "coord-main-0" },
                    new PrintingData { id = "coord-commander-reprint", cardId = "coord-commander" }
                }
            }, effects.Keys);
            var data = new CollectionData();
            var library = new CollectionLibrary(catalog, data);
            foreach (var card in cards) library.Collect(card.id);
            var standard = new DeckData {
                id = "coord-standard", name = "Coordination standard", commander = commander.id,
                main = Enumerable.Range(0,100).Select(i => "coord-main-" + i).ToList(),
                terrains = Enumerable.Range(0,50).Select(i => "coord-land-" + i).ToList()
            };
            Check(library.Validate(standard, true).Count == 0, "valid standard fixture");
            Check(catalog.Identity("coord-main-reprint") == "coord-main-0", "printing canonical identity");
            Check(library.Owns("coord-main-reprint"), "canonical ownership accepts printing");
            int owned = data.owned.Count;
            Check(!library.Collect("coord-main-reprint") && data.owned.Count == owned, "collecting printing never grants duplicate identity");
            var experimental = standard.Copy(); experimental.experimental = true;
            experimental.main = standard.main.Take(5).ToList(); experimental.terrains = standard.terrains.Take(12).ToList();
            Check(library.Validate(experimental, true).Count == 0, "valid minimum experimental fixture");
            experimental.commander = "coord-commander-reprint";
            Check(library.Validate(experimental, true).Count == 0, "printing usable in commander zone");
            foreach (bool testMode in new[] { false, true }) {
                var missing = standard.Copy(); missing.experimental = testMode; missing.commander = "";
                Check(Has(library, missing, "comandante separado"), "commander required in mode " + testMode);
                missing.commander = "   ";
                Check(Has(library, missing, "comandante separado"), "blank commander rejected in mode " + testMode);
            }
            var invalid = standard.Copy(); invalid.commander = "coord-main-0";
            Check(Has(library, invalid, "não é comandante"), "ordinary creature rejected as commander");
            invalid.commander = "coord-missing-commander";
            Check(Has(library, invalid, "Comandante não encontrado"), "unknown commander rejected");
            invalid.commander = "coord-unavailable-commander";
            Check(Has(library, invalid, "Comandante indisponível"), "unavailable commander rejected");
            var unowned = new CollectionLibrary(catalog, new CollectionData { owned = standard.main.Concat(standard.terrains).ToList() });
            Check(Has(unowned, standard, "Comandante ausente da coleção"), "commander ownership required");
            invalid = standard.Copy(); invalid.main[0] = commander.id;
            Check(Has(library, invalid, "Comandante deve ficar na zona separada"), "commander rejected in main");
            invalid = standard.Copy(); invalid.terrains[0] = commander.id;
            Check(Has(library, invalid, "Comandante deve ficar na zona separada") && Has(library, invalid, "Carta não terreno"), "commander rejected in terrain deck");
            invalid = standard.Copy(); invalid.main[0] = "coord-outside";
            Check(Has(library, invalid, "Cor fora da identidade"), "outside commander identity rejected");
            invalid.main[0] = "coord-unavailable";
            Check(Has(library, invalid, "Coordination fixture unavailable"), "unavailable main card rejected");
            invalid = standard.Copy(); invalid.main[0] = "coord-land-0";
            Check(Has(library, invalid, "Terreno no principal"), "terrain rejected in main");
            invalid = standard.Copy(); invalid.terrains[0] = "coord-main-0";
            Check(Has(library, invalid, "Carta não terreno"), "main card rejected in terrains");
            invalid = standard.Copy(); invalid.main[1] = "coord-main-reprint";
            Check(Has(library, invalid, "repetida: coord-main-0"), "printing shares standard copy limit");
            invalid.experimental = true;
            Check(library.Validate(invalid, true).Count == 0, "experimental repeats allowed by existing contract");
            invalid = experimental.Copy(); invalid.main.RemoveAt(0);
            Check(Has(library, invalid, "pelo menos 5 cartas e 12 terrenos"), "experimental main minimum enforced");
            invalid = experimental.Copy(); invalid.terrains.RemoveAt(0);
            Check(Has(library, invalid, "pelo menos 5 cartas e 12 terrenos"), "experimental terrain minimum enforced");
            invalid = standard.Copy(); invalid.main.RemoveAt(0);
            Check(Has(library, invalid, "100 cartas principais e 50 terrenos"), "standard exact size enforced");
            invalid = standard.Copy(); invalid.cardBack = "coord-unowned-back";
            Check(Has(library, invalid, "Verso não disponível"), "unowned cosmetic rejected");
            Check(library.Validate(null, true).Count == 1, "null selection rejected");

            var draft = experimental.Copy(); draft.id = "coord-draft"; draft.commander = "";
            draft.main[0] = "coord-missing-card";
            library.SaveDeck(draft);
            var saved = data.decks.Single(d => d.id == draft.id);
            Check(saved.commander == "" && saved.main[0] == "coord-missing-card", "saving draft preserves unknown identity and absent commander");
            var before = string.Join("|", saved.main.Concat(saved.terrains));
            Check(Has(library, saved, "Carta desconhecida") && Has(library, saved, "comandante separado"), "preserved draft is invalid for play");
            Check(before == string.Join("|", saved.main.Concat(saved.terrains)) && saved.commander == "", "validation does not repair or mutate draft");
            draft.main.Clear(); draft.terrains.Clear(); draft.name = "mutated caller";
            Check(saved.main.Count == 5 && saved.terrains.Count == 12 && saved.name != draft.name, "saved draft detached from caller lists and fields");
            var decorated = standard.Copy(); decorated.cardLooks.Add(new CardLook { cardId = "coord-main-0", styleId = "classic" });
            var detached = decorated.Copy(); detached.main.Clear(); detached.terrains.Clear(); detached.cardLooks[0].cardId = "coord-main-1";
            Check(decorated.main.Count == 100 && decorated.terrains.Count == 50 && decorated.cardLooks[0].cardId == "coord-main-0", "copy deeply detaches lists and appearance entries");
            Check(detached.id == decorated.id && detached.commander == decorated.commander && detached.cardBack == decorated.cardBack && detached.experimental == decorated.experimental, "copy preserves deck identity and selection metadata");
            library.SaveDeck(standard); var replacement = standard.Copy(); replacement.name = "Updated same identity"; library.SaveDeck(replacement);
            Check(data.decks.Count(d => d.id == standard.id) == 1 && data.decks.Single(d => d.id == standard.id).name == replacement.name, "saving same identity replaces rather than duplicates");
            Rejects(() => new CollectionLibrary(catalog, new CollectionData { decks = new List<DeckData> { standard.Copy(), standard.Copy() } }), "duplicate persisted deck identities rejected");
            var offline = new CollectionLibrary(catalog, new CollectionData { owned = new List<string> { "coord-missing-card" }, decks = new List<DeckData> { saved.Copy() } });
            Check(offline.Owns("coord-missing-card") && offline.Data.decks[0].main[0] == "coord-missing-card", "temporarily absent catalog content preserved on load");
            Debug.Log("COORDINATION RULES CHECKS PASSED: " + checks);
        }
    }
}
