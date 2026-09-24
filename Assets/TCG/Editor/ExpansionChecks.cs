using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TCG.Editor
{
    public static class ExpansionChecks
    {
        static int count;
        static void Check(bool ok,string name) { if (!ok) throw new Exception("EXPANSION TEST FAILED: "+name); count++; }
        static void Reject(Action action,string name)
        { bool failed = false; try { action(); } catch (InvalidOperationException) { failed = true; } catch (InvalidDataException) { failed = true; } catch (ArgumentException) { failed = true; } Check(failed,name); }
        static Game Main()
        { var g = new Game(); g.AdvancePhase(); g.AdvancePhase(); g.Place(0,16); g.AdvancePhase(); g.Players[0].Mana[6] = g.Players[1].Mana[6] = 30; return g; }
        static Unit Put(Game g,int owner,string id,int pos)
        { var unit = new Unit(owner,new Card(Catalog.Get(id),Guid.NewGuid().ToString())); g.Board[pos].Unit = unit; return unit; }
        static void Resolve(Game g) { g.PassPriority(); g.PassPriority(); }
        static void Cast(Game g,int owner,string id,int target = -1)
        { var hand = g.Players[owner].Hand; hand.Add(new Card(Catalog.Get(id),Guid.NewGuid().ToString())); g.PlayCard(owner,hand.Count-1,target); }
        static void NextTurn(Game g)
        { if (g.Phase == Phase.Main) g.AdvancePhase(); Resolve(g); }
        static void ReachMain(Game g,int tile)
        { g.AdvancePhase(); g.AdvancePhase(); g.Place(0,tile); g.AdvancePhase(); }
        public static void Run()
        {
            count = 0;
            Check(Catalog.ValidateEffectDefinitions().Count == 0,"effect metadata targets and durations");
            Check(Catalog.All.Count(c => c.Id.StartsWith("core-")) == 15,"15 bespoke cards");
            for (int n = 0; n < 3; n++) Check(Catalog.Validate(Expansion.Starter(n)).Count == 0,"starter deck "+n);
            var g = Main(); var enemy = Put(g,1,"unit-0-0",17); int before = g.Current.Hand.Count;
            Cast(g,0,"core-spark",17); Resolve(g); Check(enemy.Life == 1 && g.Current.Hand.Count == before+1,"damage then draw chain");
            g = Main(); enemy = Put(g,1,"unit-0-0",17); before = g.Current.Hand.Count;
            Cast(g,0,"core-spark",17); g.Board[17].Unit = null; Resolve(g);
            Check(g.Current.Hand.Count == before,"invalid chosen target cancels whole chain");
            g = Main(); enemy = Put(g,1,"unit-0-0",17); enemy.Life = 1; before = g.Current.Hand.Count;
            Cast(g,0,"core-spark",17); Resolve(g);
            Check(g.Board[17].Unit == null && g.Current.Hand.Count == before+1,"chain continues after its damage kills target");
            g = Main(); var ally = Put(g,0,"unit-0-0",16);
            Cast(g,0,"core-shield",16); Resolve(g); Check(ally.TemporaryShield == 3,"temporary shield");
            g.PassPriority(); Cast(g,1,"core-shard",16); Resolve(g);
            Check(ally.Life == 3 && ally.TemporaryShield == 2,"shield consumed before life");
            NextTurn(g); Check(ally.TemporaryShield == 0,"shield expires on final");
            g = Main(); Cast(g,0,"core-sentinel",16); Resolve(g); Resolve(g); ally = g.Board[16].Unit;
            Check(ally.Shield == 2,"entry shields source"); NextTurn(g); Check(ally.Shield == 2,"permanent shield persists");
            g = Main(); ally = Put(g,0,"unit-0-0",16); Cast(g,0,"core-rally",16); Resolve(g);
            Check(ally.Attack == 4,"temporary attack buff"); NextTurn(g); Check(ally.Attack == 2,"attack buff expires");
            g = Main(); enemy = Put(g,1,"unit-0-0",17); enemy.BonusAttack = 4;
            Cast(g,0,"core-return",17); Resolve(g); Check(g.Board[17].Unit == null && g.Players[1].Hand.Contains(enemy.Card),"return to owner hand");
            Check(new Unit(1,enemy.Card).BonusAttack == 0,"returned unit loses counters on replay");
            g = Main(); enemy = Put(g,1,"unit-2-0",17); enemy.Shield = 99;
            Cast(g,0,"core-rupture",17); Resolve(g); Check(g.Board[17].Unit == null && g.Players[1].Graveyard.Contains(enemy.Card),"destroy bypasses armor and shield");
            g = Main(); enemy = Put(g,1,"unit-0-0",17); Cast(g,0,"core-eclipse",17); Resolve(g);
            Check(g.Players[1].Exile.Contains(enemy.Card) && !g.Players[1].Graveyard.Contains(enemy.Card),"exile zone");
            g = Main(); var commander = new Unit(1,g.Players[1].Commander); g.Players[1].CommanderAvailable = false; g.Board[115].Unit = commander;
            Cast(g,0,"core-eclipse",115); Resolve(g); Check(!g.Players[1].CommanderAvailable && g.Players[1].Exile.Contains(commander.Card),"exiled commander unavailable");
            g = Main(); commander = new Unit(1,g.Players[1].Commander); g.Players[1].CommanderAvailable = false; g.Board[115].Unit = commander;
            Cast(g,0,"core-return",115); Resolve(g); Check(g.Players[1].CommanderAvailable && g.Players[1].CommanderDeaths == 0,"returned commander has no death tax");
            g = Main(); ally = Put(g,0,"unit-0-0",16); enemy = Put(g,1,"unit-0-0",17);
            g.Attack(16,17); g.PassPriority(); Cast(g,1,"core-return",16); Resolve(g); Resolve(g);
            Check(enemy.Life == 3 && g.Board[16].Unit == null,"return attacker cancels pending attack");
            g = Main(); enemy = Put(g,1,"unit-0-0",17); Cast(g,0,"core-frost",17); Resolve(g);
            Check(enemy.Movement == 0 && enemy.Actions == 0 && enemy.StunnedThroughTurn == 2,"freeze applied");
            NextTurn(g); Check(enemy.Actions == 0 && enemy.Movement == 0,"freeze blocks next own start refresh");
            ReachMain(g,104); NextTurn(g); Check(enemy.StunnedThroughTurn == 0,"freeze expires after own final");
            ReachMain(g,27); NextTurn(g); Check(enemy.Movement == 2 && enemy.Actions == 1,"frozen unit recovers on following turn");
            g = Main(); int mana = g.Current.Mana.Sum(); Cast(g,0,"core-reserve"); Resolve(g); Check(g.Current.Mana.Sum() == mana+2,"mana gain pays cost first");
            g = Main(); Cast(g,0,"core-clay",16); Resolve(g); var token = g.Board[16].Unit;
            Check(token != null && token.Card.Definition.Kind == CardKind.Token && token.Life == 2,"token created at chosen tile");
            g.PassPriority(); Cast(g,1,"core-return",16); Resolve(g);
            Check(g.Board[16].Unit == null && !g.Players[0].Hand.Contains(token.Card),"token vanishes when returned");
            var bad = Catalog.DefaultDeck(); bad.main[0] = "token-clay"; Check(Catalog.Validate(bad).Count > 0,"tokens excluded from decks");
            g = Main(); Cast(g,0,"core-clay",16); ally = Put(g,0,"unit-0-0",16); Resolve(g); Check(g.Board[16].Unit == ally,"token does not overwrite occupied tile");
            g = Main(); ally = Put(g,0,"unit-0-0",5); var other = Put(g,0,"unit-0-0",16); enemy = Put(g,1,"unit-0-0",17);
            ally.Life = other.Life = enemy.Life = 1; before = g.Current.Hand.Count; Cast(g,0,"core-refuge"); Resolve(g);
            Check(ally.Life == 3 && other.Life == 3 && enemy.Life == 1 && g.Current.Hand.Count == before+1,"allied area heal then draw");
            g = Main(); g.Current.Life = 40; before = g.Current.Hand.Count;
            Cast(g,0,"core-sower",16); Resolve(g); Resolve(g); Check(g.Current.Life == 43 && g.Current.Hand.Count == before+1,"compound entry effects");
            Storage(); Debug.Log("TCG_EXPANSION_VALIDATION_OK: "+count+" assertions covering new effects and library persistence.");
        }
        static void Storage()
        {
            string root = Path.Combine(Path.GetTempPath(),"TCG-library-tests-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
            try
            {
                var legacy = Catalog.DefaultDeck(); legacy.name = "Meu deck antigo";
                File.WriteAllText(Path.Combine(root,"player-1.json"),JsonUtility.ToJson(legacy));
                var library = DeckLibraryStore.Load(root,out string notice);
                Check(library.solDeck.name == legacy.name && notice == "","legacy migration");
                string id = Guid.NewGuid().ToString("N"); var draft = Expansion.Starter(2); library.Upsert(id,draft); library.Equip(id,0);
                DeckLibraryStore.Save(root,library); var loaded = DeckLibraryStore.Load(root,out _);
                Check(loaded.entries.Count == 3 && loaded.solId == id && loaded.solDeck.main.SequenceEqual(draft.main),"save/load equipped library");
                draft.main.RemoveAt(0); loaded.Upsert(id,draft); DeckLibraryStore.Save(root,loaded);
                loaded = DeckLibraryStore.Load(root,out _);
                Check(loaded.entries.First(e => e.id == id).deck.main.Count == 99 && loaded.solDeck.main.Count == 100,"incomplete draft does not corrupt equipped snapshot");
                Reject(() => loaded.Equip(id,1),"invalid deck cannot equip");
                loaded.Remove(id); DeckLibraryStore.Save(root,loaded); loaded = DeckLibraryStore.Load(root,out _);
                Check(loaded.entries.Count == 2 && loaded.solDeck.main.Count == 100 && string.IsNullOrEmpty(loaded.solId),"delete keeps equipped snapshot / null id JSON");
                var imported = DeckLibraryStore.Import(DeckLibraryStore.Export(draft));
                Check(imported.main.SequenceEqual(draft.main) && imported.name == draft.name,"draft import/export round trip");
                Reject(() => DeckLibraryStore.Import("not-json"),"malformed import");
                var unknown = draft.Copy(); unknown.main[0] = "nonexistent";
                Reject(() => DeckLibraryStore.Import(JsonUtility.ToJson(unknown)),"unknown card import");
                var duplicate = draft.Copy(); duplicate.main[0] = duplicate.main[1];
                Reject(() => DeckLibraryStore.Import(JsonUtility.ToJson(duplicate)),"duplicate card import");
                string contents = File.ReadAllText(DeckLibraryStore.FilePath(root)); var invalid = loaded.Copy(); invalid.solDeck.main.Clear();
                Reject(() => DeckLibraryStore.Save(root,invalid),"invalid equipped snapshot cannot save");
                Check(File.ReadAllText(DeckLibraryStore.FilePath(root)) == contents,"failed save preserves file");
                File.WriteAllText(DeckLibraryStore.FilePath(root),"{}");
                Reject(() => DeckLibraryStore.Load(root,out _),"invalid library rejected");
                Check(File.ReadAllText(DeckLibraryStore.FilePath(root)) == "{}","corrupt library not overwritten by load");
                var clone = library.Copy(); clone.solDeck.main.Clear(); Check(library.solDeck.main.Count == 100,"library snapshots deep copied");
            }
            finally
            {
                foreach (string file in Directory.GetFiles(root)) File.Delete(file);
                Directory.Delete(root);
            }
        }
    }
}
