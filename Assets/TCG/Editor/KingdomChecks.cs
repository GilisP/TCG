using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace TCG.Editor
{
    public static class KingdomChecks
    {
        static int count;
        static void Check(bool ok,string message) { if(!ok) throw new Exception("KINGDOM TEST FAILED: "+message); count++; }
        static void Reject(Action action,string message)
        { bool rejected=false;try{action();}catch(InvalidOperationException){rejected=true;}catch(InvalidDataException){rejected=true;}Check(rejected,message); }
        static Card Card(string id)=>new Card(Catalog.Get(id),Guid.NewGuid().ToString());
        static Game Main()
        {var g=new Game();g.AdvancePhase();g.AdvancePhase();g.Current.Offer[0]=Card("land-6");g.Place(0,16);g.AdvancePhase();foreach(var p in g.Players)for(int e=0;e<7;e++)p.Mana[e]=20;return g;}
        static Unit Put(Game g,int owner,int pos)
        {var u=new Unit(owner,Card("unit-0-0"));g.Board[pos].Unit=u;return u;}
        static void Cast(Game g,int owner,string id,int tile=-1,int target=-1)
        {var p=g.Players[owner];p.Hand.Add(Card(id));g.PlayCard(owner,p.Hand.Count-1,tile,target);}
        static void Resolve(Game g){g.PassPriority();g.PassPriority();}
        static void Refresh(Game g)
        {typeof(Game).GetMethod("BeginTurn",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(g,null);}
        public static void Run()
        {
            count=0; Check(Catalog.All.Where(c=>c.Id.StartsWith("kingdom-")).All(c=>CardAuthoring.Validate(c).Count==0),"eight new definitions valid");
            Check(Catalog.DefaultDeck().main.Contains("kingdom-sword") && Catalog.DefaultDeck().terrains.Contains("kingdom-well"),"new defaults include equipment and lands");
            for(int i=0;i<3;i++)Check(Expansion.Starter(i).main.Contains("kingdom-infirmary"),"suggested deck includes buildings "+i);
            var g=Main();var u=Put(g,0,16);var sword=Catalog.Get("kingdom-sword");
            Array.Clear(g.Current.Mana,0,7);g.Current.Mana[6]=20;
            Check(!g.CanPay(0,sword),"incolor cannot replace Sol");
            var snapshot=(int[])g.Current.Mana.Clone();int hand=g.Current.Hand.Count;g.Current.Hand.Add(Card(sword.Id));
            Reject(()=>g.PlayCard(0,hand,16),"colored cost rejected");
            Check(g.Current.Mana.SequenceEqual(snapshot)&&g.Current.Hand.Count==hand+1&&g.Stack.Count==0,"failed payment atomic");
            g.Current.Mana[0]=1;g.PlayCard(0,hand,16);
            Check(g.Current.Mana[0]==0&&g.Current.Mana[6]==19&&u.Attack==2,"colored first then generic; bonus waits for resolution");
            Resolve(g);Check(u.Attack==4&&u.Equipment.Count==1,"equipment attaches");
            g.Current.Mana[5]=1;Cast(g,0,"kingdom-mail",16);Resolve(g);Check(u.Armor==1,"equipment armor");
            Check(!g.CanTarget(Catalog.Get("kingdom-boots"),0,16),"two equipment limit");
            g.PassPriority();Cast(g,1,"core-shard",16);Resolve(g);Check(u.Life==3,"equipment reduces damage");
            g.Move(16,17);Check(u.Equipment.Count==2&&u.Attack==4,"equipment follows movement");
            g.PassPriority();Cast(g,1,"core-return",17);Resolve(g);
            Check(g.Players[0].Graveyard.Count(c=>c.Definition.Kind==CardKind.Equipment)==2&&g.Players[0].Hand.Contains(u.Card),"equipment goes to grave on return");
            g=Main();u=Put(g,0,16);Cast(g,0,"kingdom-boots",16);Resolve(g);Check(u.Movement==2&&u.MaxMovement==3,"boots do not grant immediate move");Refresh(g);Check(u.Movement==3,"boots refresh movement");
            g=Main();u=Put(g,0,16);Cast(g,0,"kingdom-sword",16);int pending=g.Stack[0].Id;g.PassPriority();Cast(g,1,"counter-0",-1,pending);Resolve(g);
            Check(u.Equipment.Count==0&&g.Players[0].Graveyard.Any(c=>c.Id=="kingdom-sword"),"counter equipment");
            g=Main();u=Put(g,0,16);Cast(g,0,"kingdom-sword",16);g.Board[16].Unit=null;Resolve(g);Check(g.Current.Graveyard.Any(c=>c.Id=="kingdom-sword"),"equipment target lost");
            g=Main();Check(!g.CanTarget(Catalog.Get("kingdom-infirmary"),0,5),"no building on capital without terrain");
            Cast(g,0,"kingdom-infirmary",16);Check(g.Board[16].Buildings.Count==0,"construction waits on stack");Resolve(g);
            Check(g.Board[16].Buildings.Count==1&&g.Board[16].HousingUsed==1,"construction consumes housing");
            Check(!g.CanTarget(Catalog.Get("kingdom-observatory"),0,16),"housing limit");
            Reject(()=>g.ActivatePermanent(16,0),"uninhabited construction blocked");
            u=Put(g,0,16);u.Life=1;int mana=g.Current.Mana.Sum();g.ActivatePermanent(16,0);
            Check(u.Life==1&&g.Current.Mana.Sum()==mana-1,"activation pays and waits");Resolve(g);Check(u.Life==3,"infirmary heals occupant");
            Reject(()=>g.ActivatePermanent(16,0),"once per turn");
            g.Turn+=2;g.ActivatePermanent(16,0);g.PassPriority();Cast(g,1,"core-return",16);Resolve(g);Resolve(g);Check(g.Log.Any(s=>s.Contains("desabitada")),"response removes inhabitant");
            g=Main();Cast(g,0,"kingdom-observatory",16);Resolve(g);Put(g,0,16);hand=g.Current.Hand.Count;g.ActivatePermanent(16,0);Resolve(g);Check(g.Current.Hand.Count==hand+1,"observatory draw");
            g.Phase=Phase.Terrain;g.Placed=false;g.Current.Offer[0]=Card("kingdom-grove");Reject(()=>g.Place(0,16),"replacement cannot evict buildings");Check(g.Board[16].HousingUsed==2,"rejected terrain replacement preserves building");
            g=Main();Cast(g,0,"kingdom-infirmary",16);g.Board[16].Owner=1;Resolve(g);Check(g.Board[16].Buildings.Count==0&&g.Current.Graveyard.Any(c=>c.Id=="kingdom-infirmary"),"building loses territory before resolution");
            g=Main();g.Board[17].Owner=1;g.Board[17].Terrain=Card("land-6");var b=new Building(Card("kingdom-infirmary"),1);g.Board[17].Buildings.Add(b);u=Put(g,0,16);
            g.Attack(16,17);Check(g.Stack[0].Kind==StackKind.StructureAttack,"demolition on stack");Resolve(g);Check(g.Board[17].Buildings.Count==1&&b.Life==3&&!g.Players[1].Graveyard.Contains(b.Card),"attack damages building life");
            g=Main();g.Board[17].Owner=1;g.Board[17].Terrain=Card("land-6");b=new Building(Card("kingdom-infirmary"),1);g.Board[17].Buildings.Add(b);u=Put(g,0,16);g.Move(16,17);
            Check(g.Board[17].Owner==0&&g.Board[17].Buildings.Count==0&&g.Players[1].Graveyard.Contains(b.Card),"conquest clears enemy structures");
            g=Main();g.Board[16].Terrain=Card("kingdom-well");Refresh(g);Check(g.Current.Mana[0]==2&&g.Current.Mana[6]==1,"well generates two Sol plus capital");
            g=Main();g.Board[16].Terrain=Card("kingdom-grove");Reject(()=>g.ActivatePermanent(16),"grove needs occupant");u=Put(g,0,16);g.ActivatePermanent(16);Resolve(g);Check(u.TemporaryShield==2,"terrain shield");
            Reject(()=>g.ActivatePermanent(16),"terrain once per turn");g.AdvancePhase();Resolve(g);Check(u.TemporaryShield==0,"terrain shield expires");
            g=Main();g.Board[16].Terrain=Card("kingdom-spring");g.Current.Life=40;g.ActivatePermanent(16);Resolve(g);Check(g.Current.Life==42,"spring heals player");
            g=Main();g.Board[16].Terrain=Card("kingdom-spring");g.Current.Life=40;g.ActivatePermanent(16);pending=g.Stack[0].Id;g.PassPriority();Cast(g,1,"counter-0",-1,pending);Resolve(g);Check(g.Players[0].Life==40&&g.Board[16].TerrainUsedTurn==g.Turn,"counter ability consumes activation");
            Authoring(); Debug.Log("TCG_KINGDOM_VALIDATION_OK: "+count+" assertions covering equipment, structures, terrains, colored mana and authoring.");
        }
        static void Authoring()
        {
            var card=CardAuthoring.Copy(Catalog.Get("kingdom-sword"));card.Id="custom-"+Guid.NewGuid().ToString("N");
            var copy=CustomCardStore.Parse(CustomCardStore.Serialize(card));Check(copy.EquipmentAttack==2&&copy.ColoredCost[0]==1&&copy.Kind==CardKind.Equipment,"custom JSON round trip");
            copy.ColoredCost[0]=9;Check(card.ColoredCost[0]==1,"deep cost clone");
            var invalid=CardAuthoring.Copy(card);invalid.ColoredCost=new[]{1};Reject(()=>CustomCardStore.Serialize(invalid),"bad mana vector");
            invalid=CardAuthoring.Copy(card);invalid.Id="../escape";Reject(()=>CustomCardStore.Serialize(invalid),"unsafe filename rejected");
            invalid=CardAuthoring.Copy(card);invalid.Effects=new[]{new EffectStep(Effect.Draw,EffectTarget.Owner,1)};Reject(()=>CustomCardStore.Serialize(invalid),"unsupported equipment effects rejected");
            invalid=CardAuthoring.Copy(Catalog.Get("core-spark"));invalid.Id=card.Id;invalid.Effects[1].Target=EffectTarget.AllyUnit;Reject(()=>CustomCardStore.Serialize(invalid),"bad operation target rejected");
            invalid=CardAuthoring.Copy(Catalog.Get("core-rally"));invalid.Id=card.Id;invalid.Effects[0].Duration=EffectDuration.Permanent;Reject(()=>CustomCardStore.Serialize(invalid),"unsupported permanent buff rejected");
            var root=Path.Combine(Path.GetTempPath(),"tcg-card-check-"+Guid.NewGuid().ToString("N"));
            try
            {
                CustomCardStore.Save(root,card);card.Name="Lâmina revisada";CustomCardStore.Save(root,card);
                Check(CustomCardStore.Parse(File.ReadAllText(Path.Combine(root,card.Id+".json"))).Name==card.Name,"atomic update saved card");
            }
            finally {if(Directory.Exists(root)) Directory.Delete(root,true);}
            try
            {
                Catalog.RegisterCustom(card);var oldInstance=new Card(Catalog.Get(card.Id),"old");copy=CardAuthoring.Copy(card);copy.EquipmentAttack=5;Catalog.RegisterCustom(copy);
                Check(oldInstance.Definition.EquipmentAttack==2&&Catalog.Get(card.Id).EquipmentAttack==5,"editing keeps existing match instances unchanged");
                var deck=Catalog.DefaultDeck();deck.main[99]=card.Id;Check(Catalog.Validate(deck).Count==0,"custom equipment eligible in decks");
                var game=new Game(42,deck);Check(game.Players[0].Hand.Concat(game.Players[0].Deck).Any(c=>c.Id==card.Id),"new match contains custom card");
            }
            finally
            {
                Catalog.All.RemoveAll(c=>c.Id==card.Id);
                var lookup=(System.Collections.Generic.Dictionary<string,CardDefinition>)typeof(Catalog).GetField("ById",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
                lookup.Remove(card.Id);
            }
            var legacy=CardAuthoring.Copy(Catalog.Get("fire-0"));Check(legacy.Effects.Length==1&&legacy.Effects[0].Operation==Effect.Damage,"legacy effects editable without losing behavior");
        }
    }
}
