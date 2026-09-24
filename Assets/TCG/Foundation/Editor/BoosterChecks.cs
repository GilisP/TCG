using System;
using System.IO;
using System.Linq;
using TCG.Foundation;
using TCG.Table;
using UnityEngine;
public static class BoosterChecks
{
 static int checks;
 static void Check(bool value,string name){checks++;if(!value)throw new Exception("BOOSTER: "+name);}
 sealed class ZeroRandom: System.Random {public override int Next(int max)=>0;}
 sealed class BrokenRandom: System.Random {public override int Next(int max)=>throw new InvalidOperationException("Random failure");}
 static bool Reject(Action a){try{a();return false;}catch(InvalidOperationException){return true;}}
 public static void Run()
 {
  checks=0;var catalog=ContentLoader.Load(new EffectRegistry());
  var pack=JsonUtility.FromJson<BoosterDefinition>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"Economy","booster-medieval.json")));
  Check(pack.price==100&&pack.cards==5&&pack.duplicateRefund==10,"approved provisional economy");
  var pool=pack.Pool(catalog);Check(pool.Length>70&&pool.All(c=>c.Playable&&pack.expansions.Contains(c.Expansion)),"eligible expansion pool");
  var lib=new CollectionLibrary(catalog,new CollectionData());var receipt=lib.OpenBooster(pack,new ZeroRandom());
  Check(receipt.rewards.Count==5&&receipt.rewards.Count(r=>r.duplicate)==4&&receipt.refund==40&&lib.Data.coins==440,"duplicates within opening and refund");
  Check(lib.Data.owned.Count==1&&lib.Data.lastBooster==receipt,"grant identity and receipt");
  lib.OpenBooster(pack,new ZeroRandom());Check(lib.Data.coins==390,"all duplicates net fifty");
  pack.standardWeight=0;pack.illuminatedWeight=100;pack.nocturneWeight=0;
  receipt=lib.OpenBooster(pack,new ZeroRandom());var id=receipt.rewards[0].cardId;
  Check(lib.OwnsVariant(id,"illuminated")&&receipt.refund==40&&lib.Data.owned.Count==1,"alternate distinct from base");
  lib.SelectStyle(id,"illuminated");var deck=new DeckData();lib.SelectStyle(id,"standard",deck);lib.SaveDeck(deck);
  Check(lib.SelectedStyle(id)=="illuminated"&&lib.SelectedStyle(id,deck)=="standard","per deck override");
  deck.cardLooks[0].styleId="nocturne";Check(lib.Data.decks[0].cardLooks[0].styleId=="standard","deep copy");
  var dir=Path.Combine(Path.GetTempPath(),"tcg-booster-check-"+Guid.NewGuid().ToString("N"));var store=new CollectionStore(dir);store.Save(lib);var restored=store.Load(catalog);
  Check(restored.Data.coins==330&&restored.SelectedStyle(id)=="illuminated"&&restored.Data.lastBooster.rewards.Count==5,"persist wallet style receipt");
  var fresh=new CollectionLibrary(catalog,new CollectionData());fresh.OpenBooster(pack,new ZeroRandom());
  Check(fresh.Owns(id)&&fresh.OwnsVariant(id,"illuminated"),"alternate grants base");
  string before=JsonUtility.ToJson(fresh.Data);Check(Reject(()=>fresh.OpenBooster(pack,new BrokenRandom()))&&before==JsonUtility.ToJson(fresh.Data),"random failure atomic");
  fresh.Data.coins=99;before=JsonUtility.ToJson(fresh.Data);Check(Reject(()=>fresh.OpenBooster(pack,new ZeroRandom()))&&before==JsonUtility.ToJson(fresh.Data),"insufficient wallet atomic");
  fresh.Data.coins=500;pack.expansions=new[]{"missing"};before=JsonUtility.ToJson(fresh.Data);Check(Reject(()=>fresh.OpenBooster(pack,new ZeroRandom()))&&before==JsonUtility.ToJson(fresh.Data),"missing expansion atomic");
  pack.standardWeight=-1;Check(Reject(()=>pack.Validate()),"invalid chances rejected");
  var old=new CollectionData{schemaVersion=2,coins=17};old.owned.Add(id);var migrated=new CollectionLibrary(catalog,old);
  Check(migrated.Data.schemaVersion==3&&migrated.Data.coins==17&&migrated.Owns(id),"migration no starter regrant");
  Check(Reject(()=>new CollectionLibrary(catalog,new CollectionData{lastBooster=new BoosterReceipt{rewards=null}})),"malformed receipt rejected");
  Debug.Log("BOOSTER CHECKS PASSED: "+checks);
 }
}
