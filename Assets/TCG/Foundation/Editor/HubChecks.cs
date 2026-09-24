using System;
using System.IO;
using System.Linq;
using TCG.Foundation;
using TCG.Table;
using UnityEngine;
public static class HubChecks
{
    static int checks;
    static void Check(bool value,string name){checks++;if(!value)throw new Exception("HUB: "+name);}
    static bool Reject(Action action){try{action();return false;}catch(InvalidOperationException){return true;}}
    public static void Run()
    {
        checks=0;var catalog=ContentLoader.Load(new EffectRegistry());var lib=new CollectionLibrary(catalog,new CollectionData());
        Check(lib.Data.coins==500&&lib.OwnsCosmetic("classic"),"prototype starting balance and default back");
        lib.BuyCard("MED-085");Check(lib.Owns("MED-085")&&lib.Data.coins==450,"purchase charges and grants");
        Check(Reject(()=>lib.BuyCard("MED-085"))&&lib.Data.coins==450,"duplicate does not charge");
        Check(Reject(()=>lib.BuyCard("MED-147"))&&lib.Data.coins==450,"unavailable card does not charge");
        Check(Reject(()=>lib.BuyCard("absent"))&&lib.Data.coins==450,"unknown card does not charge");
        lib.BuyCosmetic("ember");Check(lib.OwnsCosmetic("ember")&&lib.Data.coins==300,"cosmetic charge and grant");
        Check(Reject(()=>lib.BuyCosmetic("ember"))&&lib.Data.coins==300,"duplicate cosmetic rejected");
        Check(Reject(()=>lib.BuyCosmetic("bad"))&&lib.Data.coins==300,"unknown cosmetic rejected");
        lib.Data.coins=49;Check(Reject(()=>lib.BuyCard("MED-086"))&&!lib.Owns("MED-086")&&lib.Data.coins==49,"insufficient funds atomic");
        Check(Reject(()=>lib.BuyCosmetic("tide"))&&!lib.OwnsCosmetic("tide")&&lib.Data.coins==49,"cosmetic insufficient funds atomic");
        lib.Collect("test-land-0");var deck=new DeckData{name="Verso persistente",experimental=true,cardBack="ember",main=Enumerable.Repeat("MED-085",12).ToList(),terrains=Enumerable.Repeat("test-land-0",12).ToList()};
        lib.SaveDeck(deck);Check(lib.Validate(deck,true).Count==0,"owned back valid");
        deck.cardBack="tide";Check(lib.Data.decks[0].cardBack=="ember","deck copy preserves independent cosmetic");
        Check(lib.Validate(deck,true).Any(s=>s.Contains("Verso")),"unowned back invalid");
        string dir=Path.Combine(Path.GetTempPath(),"tcg-hub-"+Guid.NewGuid().ToString("N"));var store=new CollectionStore(dir);store.Save(lib);var restored=store.Load(catalog);
        Check(restored.Data.coins==49&&restored.OwnsCosmetic("ember")&&restored.Data.decks[0].cardBack=="ember","restart preserves wallet inventory cosmetic");
        string old="{\"schemaVersion\":1,\"owned\":[\"MED-085\"],\"decks\":[{\"id\":\"old-deck\",\"name\":\"Antigo\",\"main\":[],\"terrains\":[],\"experimental\":true}]}";
        File.WriteAllText(store.PathName,old);var migrated=store.Load(catalog);Check(migrated.Data.schemaVersion==3&&migrated.Owns("MED-085")&&migrated.Data.decks[0].id=="old-deck","v1 migration preserves existing");
        Check(migrated.Data.coins==500&&migrated.Data.decks[0].cardBack=="classic","migration grants once and defaults back");
        migrated.BuyCosmetic("grove");store.Save(migrated);Check(store.Load(catalog).Data.coins==350,"reload does not regrant starter coins");
        Check(Reject(()=>new CollectionLibrary(catalog,new CollectionData{coins=-1})),"negative balance rejected");
        Check(Reject(()=>new CollectionLibrary(catalog,new CollectionData{schemaVersion=99})),"unknown schema rejected");
        Debug.Log("HUB CHECKS PASSED: "+checks);
    }
}

