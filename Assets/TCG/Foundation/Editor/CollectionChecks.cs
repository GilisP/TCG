using System;
using System.IO;
using System.Linq;
using TCG.Foundation;
using TCG.Table;
using UnityEngine;
public static class CollectionChecks
{
    static int checks;static void Check(bool ok,string label){checks++;if(!ok)throw new Exception("COLLECTION: "+label);}
    public static void Run()
    {
        checks=0;var catalog=ContentLoader.Load(new EffectRegistry());var lib=new CollectionLibrary(catalog,new CollectionData());
        Check(lib.Collect("MED-085"),"collect once");Check(!lib.Collect("MED-085"),"idempotent ownership");lib.Collect("test-land-0");
        var d=new DeckData{name="Teste persistente",experimental=true,main=Enumerable.Repeat("MED-085",5).ToList(),terrains=Enumerable.Repeat("test-land-0",12).ToList()};
        Check(lib.Validate(d,true).Count==0,"experimental legal");lib.SaveDeck(d);d.main.Clear();Check(lib.Data.decks[0].main.Count==5,"save copies draft");
        string dir=Path.Combine(Path.GetTempPath(),"tcg-library-tests-"+Guid.NewGuid().ToString("N"));var store=new CollectionStore(dir);store.Save(lib);
        var loaded=store.Load(catalog);Check(loaded.Owns("MED-085")&&loaded.Data.decks[0].main.Count==5,"restart restores");store.Save(loaded);Check(File.Exists(store.PathName+".bak"),"backup exists");
        File.WriteAllText(store.PathName,"broken");var recovery=store.Load(catalog);Check(!store.CanWrite&&recovery.Owns("MED-085"),"corrupt primary preserves backup and blocks overwrite");
        bool blocked=false;try{store.Save(recovery);}catch(InvalidOperationException){blocked=true;}Check(blocked&&File.ReadAllText(store.PathName)=="broken","no destructive recovery");
        var unknown=new CollectionLibrary(catalog,new CollectionData{owned={"expansion-offline"},decks={new DeckData{name="Ausente",main={"unknown-card"}}}});Check(unknown.Data.owned.Contains("expansion-offline")&&unknown.Validate(unknown.Data.decks[0],true).Count>0,"unknown ids retained and invalidated");
        var standard=new DeckData{name="Padrão",main=Enumerable.Repeat("MED-085",100).ToList(),terrains=Enumerable.Repeat("MED-208",50).ToList(),commander="MED-200"};Check(lib.Validate(standard,true).Any(x=>x.Contains("repetida")),"singleton enforced");
        Check(catalog.ValidateDeck(Enumerable.Repeat("MED-085",100),new[]{"MED-208","MED-208"},true).All(x=>!x.Contains("repetida: MED-208")),"two portals allowed");Check(catalog.ValidateDeck(Enumerable.Repeat("MED-085",100),new[]{"MED-208","MED-208","MED-208"},true).Any(x=>x.Contains("repetida: MED-208")),"third portal forbidden");
        Check(Match.Capital(0,4)==55&&Match.Capital(1,4)==5&&Match.Capital(2,4)==65&&Match.Capital(3,4)==115,"four centered spawns");Check(Match.Capital(0,2)==55&&Match.Capital(1,2)==65,"opposite centered duel");
        Check(catalog.Cards.Count(c=>c.Expansion=="EXP-001-author-20260919")==32,"approved batch 32");Check(catalog.Cards.Count(c=>c.IsCommander&&c.Expansion=="EXP-001-author-20260919")==10,"ten commanders");Check(catalog.Cards.Where(c=>c.Kind==CardType.Creature).All(c=>c.Subtypes.Count>0),"explicit creature subtypes");
        Debug.Log("COLLECTION CHECKS PASSED: "+checks+"; persistence evidence: "+dir);
    }
}
