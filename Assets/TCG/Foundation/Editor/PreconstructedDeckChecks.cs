using System;
using System.Linq;
using System.IO;
using TCG.Table;
using UnityEngine;
namespace TCG.Foundation.Editor
{
 public static class PreconstructedDeckChecks
 {
  public static void Run()
  {
   int checks=0;Action<bool,string> check=(ok,message)=>{checks++;if(!ok)throw new Exception("PRECONS: "+message);};
   var catalog=ContentLoader.Load(new EffectRegistry());var precons=PreconstructedDeckLoader.Load(catalog);var ready=precons.Decks.Where(precons.Ready).ToArray();var blocked=precons.Decks.Where(d=>!precons.Ready(d)).ToArray();int expected=catalog.Get("COL001-R016").Playable?10:9;
   check(precons.Decks.Count==10&&ready.Length==expected&&blocked.Length==10-expected,"all ten lists preserved; only the unanswered R016 may block one");check(blocked.All(d=>d.main.Contains("COL001-R016")&&precons.Errors(d).Count>0),"blocked list has explicit reason");check(PlannedDeckLoader.Load(catalog).Decks.Count==29,"old planned templates retained");
   var data=new CollectionData{coins=317};data.cosmetics.Add("test-preserved");var library=new CollectionLibrary(catalog,data);var unrelated=new DeckData{name="Rascunho anterior",experimental=true};library.SaveDeck(unrelated);
   foreach(var template in ready){var temporary=PreconstructedDeckLibrary.Create(template);check(!temporary.experimental&&temporary.id=="precon-"+template.id,"stable separate identity");check(data.decks.Count<12,"temporary creation does not persist");var deck=precons.Import(library,template);check(deck.main.Count==100&&deck.terrains.Count==50,"complete lists");check(library.Validate(deck,true).Count==0,"legal imported "+template.id);}
   foreach(var template in blocked){string snapshot=JsonUtility.ToJson(data);bool rejectedBlocked=false;try{precons.Import(library,template);}catch(InvalidOperationException){rejectedBlocked=true;}check(rejectedBlocked&&JsonUtility.ToJson(data)==snapshot,"blocked import cannot mutate any profile field");}
   check(data.coins==317&&data.cosmetics.Contains("test-preserved")&&data.schemaVersion==3&&data.decks.Count==expected+1,"economy cosmetics schema and unrelated deck retained");
   var edited=data.decks[1];edited.name="Editado pelo jogador";edited.main.RemoveAt(0);edited.cardBack="test-preserved";var count=data.owned.Count;var again=precons.Import(library,ready[0]);check(again.name==edited.name&&again.main.Count==99&&again.cardBack==edited.cardBack&&data.owned.Count==count,"reimport preserves edits and inventory");
   var dir=Path.Combine(Path.GetTempPath(),"tcg-precons-"+Guid.NewGuid().ToString("N"));var store=new CollectionStore(dir);store.Save(library);var loaded=store.Load(catalog);check(loaded.Data.coins==317&&loaded.Data.decks.Count==expected+1&&loaded.Data.decks[1].main.Count==99,"isolated persistence roundtrip");
   bool rejected=false;try{new PreconstructedDeckLibrary(catalog,new PlannedDeckFile{decks=precons.Decks.Take(9).ToArray()});}catch(InvalidOperationException){rejected=true;}check(rejected,"missing precon rejected");
   PlannedDeckFile CloneFile()=>JsonUtility.FromJson<PlannedDeckFile>(JsonUtility.ToJson(new PlannedDeckFile{decks=precons.Decks.ToArray()}));
   foreach(int corruption in new[]{0,1,2}){var malformed=CloneFile();if(corruption==0)malformed.decks[0].main[0]="unknown-card-for-check";else if(corruption==1)malformed.decks[0].main[1]=malformed.decks[0].main[0];else malformed.decks[0].main=malformed.decks[0].main.Take(99).ToArray();bool fatal=false;try{new PreconstructedDeckLibrary(catalog,malformed);}catch(InvalidOperationException){fatal=true;}check(fatal,"availability never masks structural corruption "+corruption);}
   foreach(int players in new[]{2,4}){var room=new NetworkRoom(catalog,new EffectRegistry(),"precon-check",players,false,()=>0);for(int seat=0;seat<players;seat++){check(room.Receive((ulong)seat,new RoomRequest{type="hello",content="precon-check"}).error=="","join");check(room.Receive((ulong)seat,new RoomRequest{type="deck",sequence=1,deck=PreconstructedDeckLibrary.Create(ready[seat])}).error=="","host accepts ready temporary precon");check(room.Receive((ulong)seat,new RoomRequest{type="ready",sequence=2,ready=true}).error=="","ready after deck");}check(room.Receive(0,new RoomRequest{type="start",sequence=3}).error=="","start ready precon table");}
   Debug.Log("PRECON CHECKS PASSED: "+checks+"; "+ready.Length+" ready, "+blocked.Length+" explicitly blocked");
  }
 }
}
