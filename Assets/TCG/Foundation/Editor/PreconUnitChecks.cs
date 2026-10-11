using System;
using System.Linq;
using System.Reflection;
using TCG.Foundation;
using TCG.Table;
using UnityEngine;
public static class PreconUnitChecks
{
 static int checks,id=980000;
 static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Assert(bool condition,string message){checks++;if(!condition)throw new Exception("PRECON UNIT: "+message);}
 static object Call(Match m,string method,params object[] args)=>typeof(Match).GetMethod(method,Private).Invoke(m,args);
 static Definition Card(string name,int attack=1,int defense=4,CardAbility[] abilities=null,int seats=0)=>new Definition(new CardData{id="pcu-check-"+name,name=name,kind="Creature",attack=attack,defense=defense,vehicleSeats=seats,vehicleCrew=seats>0?1:0,abilities=abilities??Array.Empty<CardAbility>(),subtypes=new[]{"Goblin"}},"test");
 static Piece Add(Match m,int owner,int tile,Definition card){var p=new Piece(id++,owner,card){Game=m};m.Board[tile].pieces.Add(p);return p;}
 static void Op(Match m,Pending pending,AbilityOp op,Piece target=null,Action done=null)
 {Assert((bool)Call(m,"ResolvePreconUnitOp",pending,new CardAbility(),target,op,pending.Owner,done??(()=>{})),"operation is handled: "+op.kind);m.ResolveInitialEffects();}
 static void Choose(Match m,int key)
 {Assert(m.Try(new Command{kind=ActionKind.Choose,player=m.Controller,revision=m.Revision,target=key},out var error),"choice: "+error);}
 public static void Run()
 {
  checks=0;var registry=new EffectRegistry();var catalog=ContentLoader.Load(registry);
  Match Game(){var mains=Enumerable.Range(0,2).Select(_=>Enumerable.Repeat("MED-085",100).ToArray()).ToArray();var lands=Enumerable.Range(0,2).Select(_=>Enumerable.Repeat("test-land-0",50).ToArray()).ToArray();var m=new Match(catalog,registry,2,false,17,mains,lands);foreach(var cell in m.Board){cell.pieces.Clear();}m.Board[60].Terrain=catalog.Get("test-land-0");m.Board[60].Owner=m.Active;return m;}
  {
   var m=Game();int owner=m.Active;bool continued=false;var pending=new Pending{Owner=owner,SourceCell=60};
   Op(m,pending,new AbilityOp{kind="pcu-token",a=2,b=1,c=2,value="Goblin|1|2|1|here"},done:()=>continued=true);
   Assert(continued&&m.Board[60].Pieces.Count==2,"two tokens and continuation");Assert(m.Board[60].Pieces.All(p=>p.Token&&p.Card.Subtypes.Contains("Goblin")&&p.Card.Defense==2),"explicit token statistics and subtype");
   var token=m.Board[60].Pieces[0];int grave=m.Seats[owner].Graveyard.Count;Call(m,"CommitKill",token,-1);Assert(m.Seats[owner].Graveyard.Count==grave,"tokens never become inventory grave entries");
  }
  {
   var m=Game();int owner=m.Active;var aura=new CardAbility{trigger="aura",target="self",ops=new[]{new AbilityOp{kind="pcu-passenger-bonus",a=2,value="attack"}}};var v=Add(m,owner,60,Card("vehicle",abilities:new[]{aura},seats:3));var p=Add(m,owner,60,Card("passenger"));p.CarrierId=v.Id;
   Assert((int)Call(m,"PreconUnitBonus",v,0)==2,"one passenger gives configured attack");p.CarrierId=-1;Assert((int)Call(m,"PreconUnitBonus",v,0)==0,"unboarding removes dynamic attack");
   p.CarrierId=v.Id;Op(m,new Pending{Owner=owner,SourceCell=60},new AbilityOp{kind="pcu-passenger-temp",b=3},v);Assert((int)Call(m,"PreconUnitBonus",v,1)==3,"temporary bonus counts current passengers");p.CarrierId=-1;Assert((int)Call(m,"PreconUnitBonus",v,1)==0,"temporary passenger bonus updates dynamically");
  }
  {
   var m=Game();var p=Add(m,m.Active,60,Card("base-zero",0,5));Op(m,new Pending{Owner=m.Active},new AbilityOp{kind="pcu-allow-attack"},p);Assert((bool)Call(m,"PreconUnitCanAttack",p),"zero base attack exception explicitly authorized");var normal=Add(m,m.Active,60,Card("normal"));Op(m,new Pending{Owner=m.Active},new AbilityOp{kind="pcu-allow-attack"},normal);Assert(!(bool)Call(m,"PreconUnitCanAttack",normal),"exception cannot bypass restrictions of other attackers");
  }
  {
   var m=Game();int owner=m.Active;var p=Add(m,owner,60,Card("wounded"));var q=Add(m,1-owner,60,Card("enemy-wounded"));p.Damage=3;q.Damage=2;Op(m,new Pending{Owner=owner},new AbilityOp{kind="pcu-heal-all"});Assert(p.Damage==0&&q.Damage==2,"global healing affects own bodies only");
   Assert((bool)Call(m,"PreconUnitCondition","pcu-token",p,owner,60,-1)==false,"regular bodies are not tokens");p.Token=true;Assert((bool)Call(m,"PreconUnitCondition","pcu-token",p,owner,60,-1),"token condition uses game state");
   Assert(Call(m,"PreconUnitCondition","foreign-condition",p,owner,60,-1)==null,"foreign condition falls through");
  }
  {
   var m=Game();bool next=false;Assert(!(bool)Call(m,"ResolvePreconUnitOp",new Pending(),new CardAbility(),null,new AbilityOp{kind="foreign-operation"},0,(Action)(()=>next=true))&&!next,"foreign op leaves continuation to owner module");
   Assert(PreconUnitSchema.Operations.Contains("pcu-declared-volley")&&PreconUnitSchema.Conditions.Contains("pcu-token-equipped"),"new declarative operations and predicates registered");
  }
  {
   var m=Game();int owner=m.Active;var archerCard=new Definition(new CardData{id="pcu-check-archer",name="Arqueiro",kind="Creature",attack=1,defense=5,range=2,subtypes=new[]{"Arqueiro"}},"test");var p=Add(m,owner,60,archerCard);var q=Add(m,owner,60,archerCard);var victim=Add(m,1-owner,60,Card("volley-victim",1,10));
   var volleyCard=Card("volley",abilities:new[]{new CardAbility{target="owner",ops=new[]{new AbilityOp{kind="pcu-declared-volley",a=2,b=2}}}});var item=new Pending{Owner=owner,Card=volleyCard};bool committed=false;
   Assert((bool)Call(m,"PreconUnitDeclare",item,(Action)(()=>committed=true)),"volley intercepts declaration");m.ResolveInitialEffects();Choose(m,p.Id);Choose(m,victim.Id);Choose(m,q.Id);Choose(m,victim.Id);
   Assert(committed&&victim.Damage==0,"declaration chooses all targets before damage");Op(m,item,new AbilityOp{kind="pcu-declared-volley",a=2,b=2});Assert(victim.Damage==4,"declared volley resolves both legal shots");
  }
  {
   var m=Game();var p=Add(m,m.Active,60,Card("defender",1,5));var equipment=new Definition(new CardData{id="pcu-check-shield",name="Escudo",kind="Equipment",abilities=new[]{new CardAbility{trigger="pcu-host-defend",target="host",ops=new[]{new AbilityOp{kind="buff",a=2,value="combat"}}}}},"test");var e=Add(m,m.Active,60,equipment);e.AttachedTo=p.Id;
   int before=p.Attack;Call(m,"PreconUnitCombatEntered",p,true);Assert(p.Attack==before+2,"defending equipment applies before damage snapshot");Assert(m.Stack.Count==0,"defending equipment buff does not wait in new stack");
  }
  {
   var m=Game();int owner=m.Active;var sourceCard=new Definition(new CardData{id="pcu-check-shotter",name="Arqueiro de teste",kind="Creature",defense=5,range=2,subtypes=new[]{"Arqueiro"}},"test");var shooter=Add(m,owner,60,sourceCard);var enemy=Add(m,1-owner,60,Card("shielded-shot-target"));enemy.Modifiers.Add(new Modifier{Prevent=5,AccumulatePrevention=true,EndTurn=m.Turn});var observer=Add(m,owner,60,Card("shot-observer",abilities:new[]{new CardAbility{trigger="pcu-archer-shot",target="subject",ops=new[]{new AbilityOp{kind="pcu-free-move"}}}}));
   var item=new Pending{Owner=owner,Source=shooter,SourceCell=60};var activated=new CardAbility{trigger="activate"};Assert((bool)Call(m,"ResolvePreconUnitOp",item,activated,null,new AbilityOp{kind="pcu-shot",a=1},owner,(Action)(()=>{})),"activated shot accepted");m.ResolveInitialEffects();Choose(m,enemy.Id);Assert(enemy.Damage==0&&m.Stack.Count==0,"fully prevented shot does not trigger archer movement");
   enemy.Modifiers.Clear();Assert((bool)Call(m,"ResolvePreconUnitOp",item,activated,null,new AbilityOp{kind="pcu-shot",a=1},owner,(Action)(()=>{})),"unprevented shot accepted");m.ResolveInitialEffects();Choose(m,enemy.Id);Assert(enemy.Damage==1&&m.Stack.Any(p=>p.Card==observer.Card),"actual activated shot triggers archer movement");
  }
  {
   var m=Game();int owner=m.Active;var shooter=Add(m,owner,60,new Definition(new CardData{id="pcu-check-eligibility-archer",name="Atirador",kind="Creature",defense=5,range=2,subtypes=new[]{"Arqueiro"}},"test"));var instantShot=new CardAbility{target="own",condition="range2",ops=new[]{new AbilityOp{kind="pcu-shot",a=1}}};var item=new Pending{Owner=owner,SourceCell=60};
   Assert(!(bool)Call(m,"PreconUnitHasTargets",instantShot,item),"shot response requires an enemy within shooter range");var enemy=Add(m,1-owner,60,Card("eligible-enemy"));Assert((bool)Call(m,"PreconUnitHasTargets",instantShot,item),"shot response appears for a reachable enemy");m.Board[60].pieces.Remove(enemy);m.Board[100].pieces.Add(enemy);Assert(!(bool)Call(m,"PreconUnitHasTargets",instantShot,item),"distant enemies do not open shot response window");
   var vehicle=Add(m,owner,60,Card("eligibility-vehicle",seats:3));var unload=new CardAbility{target="ownVehicle",ops=new[]{new AbilityOp{kind="pcu-disembark",a=2}}};Assert(!(bool)Call(m,"PreconUnitHasTargets",unload,item),"empty vehicle does not open disembark response");shooter.CarrierId=vehicle.Id;Assert((bool)Call(m,"PreconUnitHasTargets",unload,item),"vehicle with own passenger enables disembark response");
  }
  {
   var m=Game();int owner=m.Active;var p=Add(m,owner,60,Card("fight-source",2,5));var q=Add(m,1-owner,60,Card("fight-shielded",0,5,new[]{new CardAbility{trigger="hurt",target="owner",ops=new[]{new AbilityOp{kind="gainLife",a=1}}}}));q.Modifiers.Add(new Modifier{Prevent=2,AccumulatePrevention=true,EndTurn=m.Turn});
   Op(m,new Pending{Owner=owner},new AbilityOp{kind="pcu-fight",a=1});Choose(m,p.Id);Choose(m,q.Id);Assert(p.Damage==0&&q.Damage==0&&m.Stack.Count==0,"prevented fight damage does not trigger hurt using raw attack amount");
  }
  {
   var m=Game();int owner=m.Active,original=1-owner;var archerCard=new Definition(new CardData{id="pcu-check-exiled-archer",name="Arqueiro do exílio",kind="Creature",attack=1,defense=5,range=2,subtypes=new[]{"Arqueiro"}},"test");var shooter=Add(m,owner,60,archerCard);var enemy=Add(m,original,60,Card("foreign-volley-victim",1,10));var volley=new Definition(new CardData{id="pcu-check-foreign-volley",name="Salva do exílio",kind="Spell",cost=2,rule="abilities",abilities=new[]{new CardAbility{target="owner",ops=new[]{new AbilityOp{kind="pcu-declared-volley",a=2,b=1}}}}},"test");m.Seats[original].exile.Add(volley);m.Seats[owner].mana[6]=5;bool paid=false;
   Call(m,"QueueForeignCast",owner,original,volley,true,(Action)(()=>{paid=true;m.Seats[owner].mana[6]-=2;m.Seats[original].exile.Remove(volley);}));m.ResolveInitialEffects();
   Assert(!paid&&m.Stack.Count==0&&m.Seats[owner].mana[6]==5&&m.Seats[original].Exile.Contains(volley),"exile payment and announcement wait for volley declaration");Choose(m,shooter.Id);Assert(!paid&&m.Stack.Count==0,"foreign volley still waits for victim selection");Choose(m,enemy.Id);
   Assert(paid&&m.Seats[owner].mana[6]==3&&!m.Seats[original].Exile.Contains(volley)&&m.Stack.Count==1,"foreign cast commits exactly after all targets are declared");var pending=m.Stack.Single();Assert(pending.CardOwner==original&&pending.ExileAfter,"foreign cast preserves original ownership and exile-on-resolution flag");Assert(enemy.Damage==0,"foreign volley declaration does not deal premature damage");Op(m,pending,new AbilityOp{kind="pcu-declared-volley",a=2,b=1});Assert(enemy.Damage==2,"foreign cast reuses preserved pending for declared volley resolution");
  }
  {
   var m=Game();int owner=m.Active;var cost=Add(m,owner,60,Card("foreign-sacrifice-cost"));var spell=new Definition(new CardData{id="pcu-check-exiled-sacrifice",name="Oferta do exílio",kind="Spell",rule="abilities",abilities=new[]{new CardAbility{target="owner",sacrifice="own",ops=new[]{new AbilityOp{kind="gainLife",a=1}}}}},"test");bool committed=false;Call(m,"QueueForeignCast",owner,owner,spell,false,(Action)(()=>committed=true));m.ResolveInitialEffects();Assert(!committed&&m.Stack.Count==0,"foreign additional sacrifice is selected before commit");Choose(m,cost.Id);Assert(committed&&m.Stack.Single().Subject==cost&&m.Find(cost.Id)==null,"foreign pending preserves sacrificed subject after paying cost");
  }
  Debug.Log("PRECON UNIT CHECKS PASSED: "+checks);
 }
}
