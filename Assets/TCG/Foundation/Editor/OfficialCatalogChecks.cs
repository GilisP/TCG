using System;
using System.Linq;
using TCG.Table;
using UnityEngine;
namespace TCG.Foundation.Editor
{
 public static class OfficialCatalogChecks
 {
  public static void Run()
  {
   int checks=0;Action<bool,string> check=(ok,message)=>{checks++;if(!ok)throw new Exception("OFFICIAL CATALOG: "+message);};
   var effects=new EffectRegistry();var active=ContentLoader.Load(effects);var history=ContentLoader.LoadFixtures(effects);
   var precons=PreconstructedDeckLoader.Load(active);var expected=precons.Decks.SelectMany(d=>d.main.Concat(d.terrains).Concat(new[]{d.commander})).Select(active.Identity).Concat(history.Cards.Where(c=>c.IsCommander).Select(c=>c.Id)).Distinct().OrderBy(x=>x).ToArray();
   check(active.Cards.Select(c=>c.Id).OrderBy(x=>x).SequenceEqual(expected),"exact official union plus every commander");
   check(history.Cards.Count>active.Cards.Count,"historical corpus remains separate");
   check(PlannedDeckLoader.Load(active).Decks.Count==0,"old planned templates are not offered");
   check(precons.Decks.Count==10&&precons.Decks.All(precons.Ready),"all ten official lists playable");
   var removed=history.Cards.First(c=>!active.IsActive(c.Id));var data=new CollectionData{coins=317};data.owned.Add(removed.Id);var saved=new DeckData{id="old",name="Historical draft",commander=expected.First(id=>active.Get(id).IsCommander),experimental=true,main=Enumerable.Repeat(removed.Id,5).ToList(),terrains=precons.Decks[0].terrains.Take(12).ToList()};data.decks.Add(saved);var library=new CollectionLibrary(active,data);
   string before=JsonUtility.ToJson(data);bool blocked=false;try{library.BuyCard(removed.Id);}catch(InvalidOperationException){blocked=true;}check(blocked&&JsonUtility.ToJson(data)==before,"removed acquisition blocked atomically");
   blocked=false;try{library.Collect(removed.Id);}catch(InvalidOperationException){blocked=true;}check(blocked&&JsonUtility.ToJson(data)==before,"free acquisition blocked without deleting profile");
   check(library.Validate(saved,true).Any(e=>e.Contains("catálogo ativo")),"old deck rejected for play");
   check(active.Get(removed.Id).Id==removed.Id&&library.Owns(removed.Id),"historic identities still readable and owned");
   var booster=new BoosterDefinition();check(booster.Pool(active).All(c=>active.IsActive(c.Id))&&booster.Pool(active).Length==active.Cards.Count(c=>c.Playable),"gameplay booster uses complete active pool only");
   check(new BoosterDefinition{expansions=new[]{removed.Expansion}}.Pool(active).All(c=>active.IsActive(c.Id)&&c.Expansion==removed.Expansion),"expansion scope remains configurable");
   foreach(int players in new[]{2,4})check(ContentLoader.TestTable(active,effects,players,false,81)!=null,"official default table "+players);
   Debug.Log("OFFICIAL CATALOG CHECKS PASSED: "+checks+"; active="+active.Cards.Count+", commanders="+active.Cards.Count(c=>c.IsCommander)+", historic="+history.Cards.Count);
  }
 }
}
