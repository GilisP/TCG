using System;
using System.Linq;
using UnityEngine;

namespace TCG.Editor
{
    public static class RuleChecks
    {
        static int count;
        static void Check(bool ok,string name) { if (!ok) throw new Exception("TCG TEST FAILED: "+name); count++; }
        static void Reject(Action action,string name)
        { bool rejected = false; try { action(); } catch (InvalidOperationException) { rejected = true; } Check(rejected,name); }
        static void Resolve(Game g) { g.PassPriority(); g.PassPriority(); }
        static Game MainGame()
        { var g = new Game(); g.AdvancePhase(); g.AdvancePhase(); g.Place(0,16); g.AdvancePhase(); return g; }
        static int Give(Game g,int owner,string id)
        { g.Players[owner].Hand.Add(new Card(Catalog.Get(id),"test-"+Guid.NewGuid())); return g.Players[owner].Hand.Count-1; }
        static Unit Put(Game g,int owner,string id,int tile)
        { var unit = new Unit(owner,new Card(Catalog.Get(id),"test-"+Guid.NewGuid())); g.Board[tile].Unit = unit; return unit; }
        static void Fund(Game g,int owner = 0) { g.Players[owner].Mana[6] = 20; }
        public static void Run()
        {
            count = 0;
            Check(Catalog.All.Count(c => !c.Id.StartsWith("custom-")) == 257,"catalog count");
            Check(Catalog.All.Select(c => c.Id).Distinct().Count() == Catalog.All.Count,"catalog unique ids");
            var deck = Catalog.DefaultDeck(); Check(Catalog.Validate(deck).Count == 0,"default deck valid");
            var bad = deck.Copy(); bad.main[0] = bad.main[1]; Check(Catalog.Validate(bad).Any(e => e.Contains("repetir")),"duplicate detection");
            bad = deck.Copy(); bad.main[0] = "land-0"; Check(Catalog.Validate(bad).Count > 0,"wrong card type");
            bad = deck.Copy(); bad.terrains.RemoveAt(0); Check(Catalog.Validate(bad).Count > 0,"terrain size");
            bad = deck.Copy(); bad.commander = "unit-0-0"; Check(Catalog.Validate(bad).Count > 0,"commander validation");
            bad = deck.Copy(); bad.main[0] = "missing"; Check(Catalog.Validate(bad).Count > 0,"unknown definition");
            Reject(() => new Game(42,bad),"invalid decks rejected at match start");
            var restored = JsonUtility.FromJson<DeckList>(JsonUtility.ToJson(deck));
            Check(Catalog.Validate(restored).Count == 0 && restored.main.SequenceEqual(deck.main),"deck JSON round trip");
            var g = new Game();
            Check(g.Phase == Phase.Start && g.Current.Hand.Count == 5 && g.Current.Deck.Count == 95,"initial state");
            Check(g.Board.Length == 121 && g.Current.Offer.Count == 5 && g.Current.Terrains.Count == 45,"board and terrain setup");
            Reject(() => g.Place(0,16),"terrain outside phase");
            Reject(g.PassPriority,"priority before main"); g.AdvancePhase();
            Check(g.Phase == Phase.Draw && g.Current.Hand.Count == 7,"draw phase"); g.AdvancePhase();
            Reject(g.AdvancePhase,"mandatory terrain"); Reject(() => g.Place(0,120),"nonadjacent terrain");
            g.Current.Offer[0] = new Card(Catalog.Get("land-6"),"rule-land"); g.Place(0,16); Check(g.Current.Offer.Count == 5 && g.Current.Terrains.Count == 44,"offer refill");
            Reject(() => g.Place(0,17),"one terrain per turn"); g.AdvancePhase(); Check(g.OpenMain,"main unlocked");
            int hand = Give(g,0,"unit-0-0"); g.PlayCard(0,hand,5);
            Check(g.Board[5].Unit == null && g.Stack.Count == 1 && g.Current.Mana.Sum() == 0,"cost paid before resolution");
            Reject(() => g.Move(5,4),"movement blocked with stack"); g.PassPriority();
            Check(g.Priority == 1 && g.Stack.Count == 1,"one pass does not resolve");
            Reject(() => g.PlayCard(0,0,16),"wrong priority blocked"); g.PassPriority();
            Check(g.Board[5].Unit != null && g.Stack.Count == 0,"unit resolves on two passes");
            g.Move(5,4); g.Move(4,3); Reject(() => g.Move(3,2),"movement cap");
            g.AdvancePhase(); Resolve(g);
            Check(g.Active == 1 && g.Phase == Phase.Start,"end turn priority window");
            g.AdvancePhase(); g.AdvancePhase(); g.Place(0,104); g.AdvancePhase(); g.AdvancePhase(); Resolve(g);
            Check(g.Active == 0 && g.Current.Mana.Sum() == 2 && g.Board[3].Unit.Movement == 2,"resource refresh");

            g = MainGame(); Fund(g); Fund(g,1); var victim = Put(g,1,"unit-0-0",17);
            g.PlayCard(0,Give(g,0,"fire-0"),17); int original = g.Stack.Last().Id; g.PassPriority();
            Reject(() => g.PlayCard(1,Give(g,1,"draw-0")),"opponent cannot cast sorcery");
            g.PlayCard(1,Give(g,1,"counter-0"),-1,original);
            Check(g.Passes == 0 && g.Priority == 1 && g.Stack.Count == 2,"response resets passes");
            Resolve(g); Check(g.Stack.Count == 0 && victim.Life == 3,"counter cancels before damage");
            Check(g.Players[0].Graveyard.Any(c => c.Id == "fire-0") && g.Players[1].Graveyard.Any(c => c.Id == "counter-0"),"counter zones");
            g.PlayCard(0,Give(g,0,"fire-1"),17); original = g.Stack.Last().Id; g.PassPriority();
            g.PlayCard(1,Give(g,1,"counter-1"),-1,original); int negate = g.Stack.Last().Id; g.PassPriority();
            g.PlayCard(0,Give(g,0,"counter-2"),-1,negate); Resolve(g);
            Check(g.Stack.Count == 1 && g.Stack[0].Id == original,"counter of counter / LIFO"); Resolve(g);
            Check(g.Board[17].Unit == null,"original spell survives nested counter");

            g = MainGame(); Fund(g); victim = Put(g,1,"unit-0-0",17); g.PlayCard(0,Give(g,0,"fire-0"),17);
            var replacement = Put(g,1,"unit-2-0",17); Resolve(g);
            Check(replacement.Life == 5,"target identity not retargeted to replacement");
            g = MainGame(); Fund(g); var armored = Put(g,1,"unit-2-0",17); g.PlayCard(0,Give(g,0,"fire-0"),17); Resolve(g);
            Check(armored.Life == 3,"armor reduces spell damage");
            var archer = Put(g,0,"unit-3-0",15); g.Attack(15,17); Resolve(g); Check(armored.Life == 2,"range 2 and armor");
            Reject(() => g.Attack(15,17),"one attack action");
            g = MainGame(); Fund(g); g.Current.Life = 40; var drainer = Put(g,0,"unit-7-0",16); victim = Put(g,1,"unit-0-0",17);
            g.Attack(16,17); Resolve(g); Check(g.Current.Life == 43 && g.Board[17].Unit == null,"lifesteal");

            g = MainGame(); Fund(g); g.Current.Life = 40;
            g.PlayCard(0,Give(g,0,"unit-4-0"),16); Resolve(g);
            Check(g.Current.Life == 40 && g.Stack.Count == 1 && g.Stack[0].Kind == StackKind.Trigger,"entry trigger gets own stack item");
            Resolve(g); Check(g.Current.Life == 42,"entry heal");
            g = MainGame(); Fund(g); int before = g.Current.Hand.Count;
            g.PlayCard(0,Give(g,0,"unit-5-0"),16); Resolve(g); Resolve(g);
            Check(g.Current.Hand.Count == before+1,"sage draws on entry");
            g = MainGame(); Fund(g); var ally = Put(g,0,"unit-0-0",16); ally.Life = 1;
            g.PlayCard(0,Give(g,0,"heal-0"),16); Resolve(g); Check(ally.Life == 3,"heal limited to maximum");

            g = MainGame(); Fund(g); g.DeployCommander(0,5);
            Check(!g.Current.CommanderAvailable && g.Board[5].Unit == null,"commander pending zone"); Resolve(g);
            Check(g.Board[5].Unit.Commander,"commander deployed"); g.Current.Life = 40;
            g.ActivateCommand(0,0); Resolve(g); Check(g.Current.Popularity == 3 && g.Current.Life == 42,"reunir ability");
            Reject(() => g.ActivateCommand(0,1),"command once per turn and shared action");
            g = MainGame(); Fund(g); g.DeployCommander(0,5); Resolve(g);
            int baseAttack = g.Board[5].Unit.Attack; g.ActivateCommand(0,1); Resolve(g);
            Check(g.Current.Popularity == 0 && g.Board[5].Unit.Attack == baseAttack+1,"inspire cost and buff");
            g.AdvancePhase(); Resolve(g); Check(g.Board[5].Unit.Attack == baseAttack,"buff expires at end");

            g = MainGame(); Fund(g); Fund(g,1); g.DeployCommander(0,5); int summonId = g.Stack.Last().Id; g.PassPriority();
            g.PlayCard(1,Give(g,1,"counter-0"),-1,summonId); Resolve(g);
            Check(g.Players[0].CommanderAvailable && g.CommanderCost(0) == 3,"counter commander returns without death tax");
            g = MainGame(); Fund(g); g.DeployCommander(0,5); Resolve(g); g.Board[5].Unit.Life = 1;
            g.AdvancePhase(); Resolve(g); g.AdvancePhase(); g.AdvancePhase(); g.Place(0,104); g.AdvancePhase(); Fund(g,1);
            g.PlayCard(1,Give(g,1,"fire-0"),5); Resolve(g);
            Check(g.Players[0].CommanderAvailable && g.CommanderCost(0) == 5,"commander death and tax");

            g = MainGame(); Put(g,0,"unit-0-0",104); g.Players[1].Life = 2; g.Attack(104,115); Resolve(g);
            Check(g.Winner == 0,"capital victory"); Reject(g.PassPriority,"post-victory lock");
            g = new Game(); g.Current.Deck.Clear(); g.AdvancePhase(); Check(g.Winner == 1,"deck-out during draw phase");
            Debug.Log("TCG_VALIDATION_OK: "+count+" assertions; phases, decks, targeting, LIFO, counters, triggers, traits, commander, victory.");
        }
    }
}
