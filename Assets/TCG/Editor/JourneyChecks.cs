using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
namespace TCG.Editor
{
    public static class JourneyChecks
    {
        static int count;
        static void Check(bool condition,string message){if(!condition)throw new Exception("JOURNEY TEST FAILED: "+message);count++;}
        static void Reject(Action action,string message){bool failed=false;try{action();}catch(InvalidOperationException){failed=true;}catch(InvalidDataException){failed=true;}Check(failed,message);}
        static Card Card(string id)=>new Card(Catalog.Get(id),Guid.NewGuid().ToString());
        static Game Main(MatchMode mode=MatchMode.Duel)
        {var g=new Game(mode:mode);g.AdvancePhase();g.AdvancePhase();g.Current.Offer[0]=Card("land-6");g.Place(0,16);g.AdvancePhase();foreach(var p in g.Players)for(int e=0;e<7;e++)p.Mana[e]=20;return g;}
        static Unit Put(Game g,int tile,int owner=0,string id="unit-0-0")
        {var u=new Unit(owner,Card(id));g.Board[tile].Unit=u;return u;}
        static void Resolve(Game g){g.PassPriority();g.PassPriority();}
        static void Cast(Game g,int owner,string id,int target=-1,int stack=-1)
        {var p=g.Players[owner];p.Hand.Add(Card(id));g.PlayCard(owner,p.Hand.Count-1,target,stack);}
        static void Refresh(Game g)=>typeof(Game).GetMethod("BeginTurn",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(g,null);
        public static void Run()
        {
            count=0;
            Check(Catalog.All.Where(c=>c.Id.StartsWith("kingdom-")).All(c=>CardAuthoring.Validate(c).Count==0),"kingdom definitions including vehicle and automatic effects");
            var g=Main();var a=Put(g,16);var b=Put(g,17);a.Equipment.Add(Card("kingdom-sword"));int mana=g.Current.Mana.Sum();
            g.ChangeEquipment(16,0,17);Check(a.Attack==4&&b.Attack==2&&g.Current.Mana.Sum()==mana-1,"transfer pays before resolving");Resolve(g);
            Check(a.Attack==2&&b.Attack==4&&b.Equipment.Count==1,"transfer applies bonuses to new holder");
            g.ChangeEquipment(17,0);Resolve(g);Check(b.Equipment.Count==0&&g.Current.Hand.Any(c=>c.Id=="kingdom-sword"),"unequip returns to hand");
            a.Equipment.Add(Card("kingdom-boots"));a.Movement=3;g.ChangeEquipment(16,0);Resolve(g);Check(a.Movement==2,"unequip clamps remaining movement");
            a.Equipment.Add(Card("kingdom-sword"));g.ChangeEquipment(16,0,17);int pending=g.Stack[0].Id;g.PassPriority();Cast(g,1,"counter-0",-1,pending);Resolve(g);Check(a.Equipment.Count==1&&b.Equipment.Count==0,"counter transfer preserves attachment");
            g.ChangeEquipment(16,0,17);g.Board[17].Unit=null;Resolve(g);Check(a.Equipment.Count==1,"lost transfer target preserves source gear");
            Reject(()=>g.ChangeEquipment(16,0,20),"nonadjacent transfer rejected");
            g=Main();a=Put(g,16);var building=new Building(Card("kingdom-infirmary"),0);g.Board[16].Buildings.Add(building);
            Check(building.Life==5,"buildings begin at max life");Reject(()=>g.Repair(16,0),"repair full building rejected");building.Life=2;g.Repair(16,0);Resolve(g);Check(building.Life==4,"repair heals two");
            var inhabitant=g.Board[16].Unit;g.Board[16].Unit=new Unit(0,Card("kingdom-carriage"));
            Check(g.AbilityBlockReason(16,0)!="","empty vehicle does not inhabit a building");g.Board[16].Unit=inhabitant;
            Reject(()=>g.Repair(16,0),"repair once per turn");g.Turn+=2;g.Repair(16,0);Resolve(g);Check(building.Life==5,"repair capped at max life");
            building.Life=1;g.Turn+=2;g.Repair(16,0);g.PassPriority();Cast(g,1,"core-return",16);Resolve(g);Resolve(g);Check(building.Life==1,"repair fails after inhabitant leaves");
            g=Main();a=Put(g,16);g.Board[17].Owner=1;g.Board[17].Terrain=Card("land-6");building=new Building(Card("kingdom-infirmary"),1);g.Board[17].Buildings.Add(building);
            for(int i=0;i<3;i++){a.Actions=1;g.Attack(16,17);Resolve(g);}
            Check(g.Board[17].Buildings.Count==0&&g.Players[1].Graveyard.Contains(building.Card),"repeated attacks destroy building at zero life");
            g=Main();g.Board[16].Terrain=Card("kingdom-sanctuary");g.Current.Life=40;Refresh(g);
            Check(g.Phase==Phase.Start&&g.Stack.Count==1&&g.Current.Life==40,"start trigger waits on stack");Reject(g.AdvancePhase,"start cannot advance over trigger");Resolve(g);
            Check(g.Current.Life==42&&g.Phase==Phase.Start,"automatic heal resolves at start");g.AdvancePhase();Check(g.Phase==Phase.Draw,"normal phase flow resumes");
            g=Main();g.Board[16].Terrain=Card("kingdom-sanctuary");g.Current.Life=40;Refresh(g);pending=g.Stack[0].Id;g.PassPriority();Cast(g,1,"counter-0",-1,pending);Resolve(g);Check(g.Players[0].Life==40&&g.Stack.Count==0,"counter automatic trigger during start");
            g=Main();g.Board[16].Terrain=Card("kingdom-gate");a=Put(g,5);g.Move(5,16);Check(g.Stack.Count==1&&a.TemporaryShield==0,"move creates enter trigger");Resolve(g);Check(a.TemporaryShield==1,"enter shield resolves");
            g=Main();g.Board[16].Terrain=Card("kingdom-gate");Cast(g,0,"unit-0-0",16);Resolve(g);Check(g.Stack.Count==1,"summon creates terrain trigger");Resolve(g);Check(g.Board[16].Unit.TemporaryShield==1,"summon trigger applied");
            g=Main();g.Board[16].Terrain=Card("kingdom-gate");a=Put(g,5);g.Move(5,16);g.Board[16].Terrain=Card("land-6");Resolve(g);Check(a.TemporaryShield==0,"terrain source removal cancels automatic trigger");
            g=Main();Cast(g,0,"kingdom-carriage",16);Resolve(g);var vehicle=g.Board[16].Unit;
            Check(vehicle.IsVehicle&&vehicle.Life==6&&!g.CanMove(16,17),"empty vehicle cannot move");a=Put(g,5);b=Put(g,15);
            g.Embark(5,16);g.Embark(15,16);Check(vehicle.Passengers.Count==2&&g.Board[5].Unit==null&&g.Board[15].Unit==null,"multiple creatures embark");
            Check(!g.CanTarget(Catalog.Get("kingdom-sword"),0,16),"equipment targets creatures rather than vehicle");
            g.Move(16,17);Check(g.Board[17].Unit==vehicle&&vehicle.Passengers.Contains(a)&&g.Position(a)==-1,"vehicle transports passengers outside board targeting");
            Check(g.CanDisembark(17,0,16),"disembark into owned adjacent tile");g.Disembark(17,0,16);
            Check(g.Board[16].Unit==a&&a.Actions==0&&a.Movement==0&&vehicle.Passengers.Count==1,"disembark costs remaining turn actions");
            g=Main();vehicle=Put(g,16,0,"kingdom-siege");a=Put(g,5);g.Embark(5,16);Check(!g.CanMove(16,17),"siege requires two crew");b=Put(g,15);g.Embark(15,16);Check(g.CanMove(16,17),"two crew activate siege");
            var extra=Put(g,27);Reject(()=>g.Embark(27,16),"seat capacity enforced");a.StunnedThroughTurn=g.Turn+2;Check(!g.CanMove(16,17),"stunned crew cannot operate vehicle");
            a.StunnedThroughTurn=0;g.PassPriority();Cast(g,1,"core-return",16);Resolve(g);
            Check(vehicle.Passengers.Count==0&&g.Players[0].Hand.Contains(vehicle.Card)&&g.Position(a)>=0&&g.Position(b)>=0,"vehicle return evacuates crew");
            Check(a.Actions==0&&b.Actions==0,"evacuated crew exhausted");
            g=Main();vehicle=Put(g,16,0,"kingdom-carriage");a=Put(g,5);a.TemporaryShield=2;a.BonusAttack=2;g.Embark(5,16);g.AdvancePhase();Resolve(g);
            Check(a.TemporaryShield==0&&a.BonusAttack==0,"passengers lose temporary effects at end");g.Active=0;g.Turn++;Refresh(g);Check(a.Actions==1&&a.Movement==2,"passenger state refreshes on own turn");
            g=Main();vehicle=Put(g,16,0,"kingdom-carriage");a=new Unit(0,Card("unit-0-0"));var commander=new Unit(0,g.Current.Commander);g.Current.CommanderAvailable=false;
            vehicle.Passengers.Add(a);vehicle.Passengers.Add(commander);foreach(int pos in new[]{5,15,17,27})g.Board[pos].Owner=1;
            g.PassPriority();Cast(g,1,"core-return",16);Resolve(g);
            Check(g.Position(a)==16&&g.Players[0].CommanderAvailable&&g.Players[0].CommanderDeaths==1,"evacuation preserves first passenger and handles trapped commander death");
            var duel=new Game();var quick=new Game(mode:MatchMode.Quick);var practice=new Game(mode:MatchMode.Practice);
            Check(duel.Current.Life==50&&duel.Current.Mana.Sum()==1,"duel baseline");
            Check(quick.Current.Life==30&&quick.Current.Mana.Sum()==2,"quick mode rules differ");
            Check(practice.Current.Mana.Sum()==71&&practice.Current.Mana.All(m=>m>=10),"practice funds all elements");
            Reject(()=>new Game(mode:(MatchMode)99),"unknown mode rejected");
            PackageChecks();MenuChecks();Debug.Log("TCG_JOURNEY_VALIDATION_OK: "+count+" assertions covering equipment management, repair, triggers, vehicles, packages, slots and menu modes.");
        }
        static void PackageChecks()
        {
            var before=Catalog.All.Select(c=>c.Id).ToHashSet();string root=Path.Combine(Path.GetTempPath(),"tcg-package-"+Guid.NewGuid().ToString("N"));
            try
            {
                var token=CardAuthoring.Copy(Catalog.Get("token-clay"));token.Id="custom-"+Guid.NewGuid().ToString("N");Catalog.RegisterCustom(token);
                var custom=CardAuthoring.Copy(Catalog.Get("core-clay"));custom.Id="custom-"+Guid.NewGuid().ToString("N");custom.Effects[0].TokenId=token.Id;Catalog.RegisterCustom(custom);
                var deck=Catalog.DefaultDeck();deck.main[99]=custom.Id;string json=DeckPackages.Export(deck);
                var package=JsonUtility.FromJson<DeckPackage>(json);Check(package.cards.Count==2&&package.cards.Any(c=>c.Id==token.Id),"export includes token dependencies");
                Remove(custom.Id);Remove(token.Id);var imported=DeckPackages.Import(json,root);Check(Catalog.Validate(imported).Count==0&&Catalog.Get(token.Id)!=null,"portable import installs missing definitions");
                Remove(custom.Id);Remove(token.Id);DeckPackages.LoadInstalled(root);Check(Catalog.Get(custom.Id)!=null&&Catalog.Get(token.Id)!=null,"installed definitions reload before deck library");
                var revision=CardAuthoring.Copy(Catalog.Get(custom.Id));revision.Name="Imported revision";Check(DeckPackages.UpdateInstalled(revision,root),"editor can update imported definitions");
                Remove(custom.Id);DeckPackages.LoadInstalled(root);Check(Catalog.Get(custom.Id).Name=="Imported revision","imported edit persists across reload");
                var conflict=CardAuthoring.Copy(Catalog.Get(custom.Id));conflict.Name="Local edited card";Catalog.RegisterCustom(conflict);
                imported=DeckPackages.Import(json,root);Check(imported.main[99]!=custom.Id&&Catalog.Get(custom.Id).Name=="Local edited card","ID conflict remapped without overwriting local definition");
                string prior=File.ReadAllText(Path.Combine(root,"installed-v1.json"));int size=Catalog.All.Count;
                package.cards[0].Cost=-1;Reject(()=>DeckPackages.Import(JsonUtility.ToJson(package),root),"invalid bundle rejected");
                Check(Catalog.All.Count==size&&File.ReadAllText(Path.Combine(root,"installed-v1.json"))==prior,"invalid import leaves catalog and disk unchanged");
                var old=DeckPackages.Import(DeckLibraryStore.Export(Catalog.DefaultDeck()),root);Check(Catalog.Validate(old).Count==0,"legacy deck JSON accepted");
                var legacy=CardAuthoring.Copy(Catalog.Get("kingdom-sword"));legacy.Id="custom-legacy-fixture";
                string legacyJson=CustomCardStore.Serialize(legacy);
                foreach(string field in new[]{"BuildingLife","Seats","CrewRequired","AutomaticTrigger"})
                    legacyJson=System.Text.RegularExpressions.Regex.Replace(legacyJson,",?\\s*\""+field+"\"\\s*:\\s*\\d+","");
                legacyJson=System.Text.RegularExpressions.Regex.Replace(legacyJson,",?\\s*\"AutomaticEffects\"\\s*:\\s*\\[\\s*\\]","");
                Check(CustomCardStore.Parse(legacyJson).BuildingLife==5,"v0.4 card files retain defaults for new fields");
                string file=DeckPackages.ExportFile(deck,Path.Combine(root,"exports"));Check(File.Exists(file)&&JsonUtility.FromJson<DeckPackage>(File.ReadAllText(file)).format=="tcg-deck-package","export writes portable file");
                var lib=DeckLibrary.Create();lib.Upsert(Guid.NewGuid().ToString("N"),Catalog.DefaultDeck());lib.Upsert(Guid.NewGuid().ToString("N"),Catalog.DefaultDeck(1));DeckLibraryStore.Save(Path.Combine(root,"decks"),lib);
                Check(DeckLibraryStore.Load(Path.Combine(root,"decks"),out _).entries.Count==4,"multiple saved slots persist");
            }
            finally{foreach(var c in Catalog.All.Where(c=>!before.Contains(c.Id)).ToArray())Remove(c.Id);if(Directory.Exists(root))Directory.Delete(root,true);}
        }
        static void Remove(string id)
        {Catalog.All.RemoveAll(c=>c.Id==id);((Dictionary<string,CardDefinition>)typeof(Catalog).GetField("ById",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null)).Remove(id);}
        static void MenuChecks()
        {
            var go=new GameObject("Menu rules check");const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
            try
            {
                var view=go.AddComponent<GameView>();var type=typeof(GameView);
                object Read(string field)=>type.GetField(field,flags).GetValue(view);
                type.GetMethod("Awake",flags).Invoke(view,null);
                Check((bool)Read("mainMenu")&&!(bool)Read("hasStarted"),"startup at home menu");
                type.GetMethod("OpenPlaySetup",flags).Invoke(view,null);Check((bool)Read("playSetup"),"play opens mode and deck selection");
                type.GetField("selectedMode",flags).SetValue(view,MatchMode.Quick);type.GetMethod("NewGame",flags).Invoke(view,null);
                Check(!(bool)Read("mainMenu")&&((Game)Read("game")).Mode==MatchMode.Quick,"chosen mode launches game");
                type.GetField("selectedMode",flags).SetValue(view,MatchMode.Practice);type.GetMethod("NewGame",flags).Invoke(view,null);
                var g=(Game)Read("game");g.Phase=Phase.Main;
                type.GetMethod("Execute",flags).Invoke(view,new object[]{(Action)g.PassPriority});Check(!(bool)Read("handoff")&&g.Priority==1,"practice switches sides without privacy modal");
            }
            finally{UnityEngine.Object.DestroyImmediate(go);}
        }
    }
}
