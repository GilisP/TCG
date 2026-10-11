using System;
using System.Linq;
using System.Reflection;
using TCG.Table;
using UnityEngine;
namespace TCG.Foundation.Editor
{
 public static class PreconLibraryChecks
 {
  static int checks;static ContentCatalog catalog;static EffectRegistry effects;
  static void Check(bool value,string label){checks++;if(!value)throw new Exception("PRECON LIBRARY: "+label);}
  static object Invoke(Match game,string name,params object[] args){try{return typeof(Match).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,args);}catch(TargetInvocationException e){throw e.InnerException;}}
  static Match Game()=>ContentLoader.TestTable(catalog,effects,2,false,73);
  static void Choose(Match game,int key){if(!game.Try(new Command{player=game.Choice.Owner,revision=game.Revision,kind=ActionKind.Choose,target=key},out var error))throw new Exception(error);}
  static void Op(Match game,int owner,AbilityOp op,Action done){Check((bool)Invoke(game,"ResolvePreconLibraryOp",new Pending{Owner=owner},new CardAbility(),null,op,owner,done),"operation dispatched");Invoke(game,"Drain");}
  public static void Run()
  {
   checks=0;effects=new EffectRegistry();catalog=ContentLoader.Load(effects);
   {var g=Game();var a=new CardAbility{ops=new[]{new AbilityOp{kind="pcl-counter",a=3,b=1}}};Check((bool)Invoke(g,"PreconLibraryHasTargets",a,new Pending{Owner=g.Active})==false,"counter unavailable without eligible stack target");}
   {
    var g=Game();int o=g.Active;g.Seats[o].main.Clear();var instant=catalog.Get("ED261008-D02-F01-1");var creature=catalog.Get("MED-085");g.Seats[o].main.Add(instant);g.Seats[o].main.Add(creature);bool done=false;Op(g,o,new AbilityOp{kind="pcl-select-top",a=2,value="magic"},()=>done=true);
    Check(g.Choice.Options.Any(x=>x.Label==instant.Name)&&!g.Choice.Options.Any(x=>x.Label==creature.Name),"selection filters spells");Choose(g,0);while(g.Choice!=null)Choose(g,g.Choice.Options.First(x=>x.Key>=0).Key);Check(done&&g.Seats[o].hand.Contains(instant)&&g.Seats[o].main.Single()==creature,"selection and bottom ordering finish");
   }
   {
    var g=Game();int o=g.Active;var cheap=catalog.Get("MED-085");g.Seats[o].grave.Add(cheap);bool done=false;Op(g,o,new AbilityOp{kind="pcl-grave",a=999,b=1,value="creature:bottom"},()=>done=true);Choose(g,0);Check(done&&g.Seats[o].main.First()==cheap&&!g.Seats[o].grave.Contains(cheap),"grave to bottom atomic move");
   }
   {
    var g=Game();int o=g.Active,v=1-o;var c=catalog.Get("MED-085");g.Seats[v].grave.Add(c);bool done=false;Op(g,o,new AbilityOp{kind="pcl-exile-grave",a=999,b=1,c=1},()=>done=true);Choose(g,0);var permit=g.ExiledFor(o).Single();Check(done&&permit.AnyMana&&permit.CardOwner==v&&g.Seats[v].exile.Contains(c),"foreign exile preserves owner and payment");Invoke(g,"PreconLibraryEndTurn",o);Check(g.ExiledFor(o).Count==1,"next-turn permission survives current end");
   }
   {
    var g=Game();int o=g.Active,v=1-o;var c=catalog.Get("MED-085");g.Seats[v].grave.Add(c);Op(g,o,new AbilityOp{kind="pcl-exile-grave",a=999,b=1},()=>{});Choose(g,0);Invoke(g,"PreconLibraryEndTurn",o);Check(g.ExiledFor(o).Count==0&&g.Seats[v].exile.Contains(c),"current-turn permission expires without returning exiled card");
   }
   {
    var g=Game();int o=g.Active;g.Seats[o].main.Clear();bool done=false;Op(g,o,new AbilityOp{kind="pcl-top-hand-life",a=2},()=>done=true);Check(done&&g.Choice==null,"empty library choice terminates");
   }
   Debug.Log("PRECON LIBRARY CHECKS PASSED: "+checks);
  }
 }
}
