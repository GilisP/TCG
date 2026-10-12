using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TCG.Foundation;
using UnityEngine;

namespace TCG.Table
{
 public sealed partial class TableView
 {
  IEnumerator VerifyDeckSelection()
  {
   var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-tcg-deck-picker-role");bool host=args[at+1]=="host";string label=host?"host":"guest";
   string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","DeckSelectionVerification"));Directory.CreateDirectory(dir);
   int checks=0;var errors=new List<string>();Application.LogCallback listen=(message,trace,kind)=>{if(kind==LogType.Error||kind==LogType.Exception||kind==LogType.Assert){errors.Add(message);File.WriteAllText(Path.Combine(dir,label+".txt"),"FAIL: "+message);Application.Quit(1);}};Application.logMessageReceived+=listen;
   collectionStore=new CollectionStore(Path.Combine(dir,"profile-"+Guid.NewGuid().ToString("N")));library=collectionStore.Load(catalog);
   var precons=PreconstructedDeckLoader.Load(catalog);precons.Import(library,precons.Decks[0]);var saved=library.Data.decks[0].Copy();saved.id="saved-picker";saved.name="Deck salvo do diagnóstico";library.Data.decks.Add(saved);
   library.Data.decks.Add(new DeckData{id="invalid-picker",name="Rascunho sem comandante"});string before=JsonUtility.ToJson(library.Data);
   PrepareLocalTable();OpenDeckSelection(1);
   Check(deckSelectionEntries.All(e=>e.Deck.id!="invalid-picker")&&deckSelectionEntries.Any(e=>e.Deck.id==saved.id),"valid saved deck offered, invalid draft omitted");
   Check(deckSelectionEntries.Count(e=>e.Deck.id.StartsWith("precon-",StringComparison.Ordinal))==10,"all ready precons available");
   string other=seatDecks[0];Check(ChooseDeckEntry(deckSelectionEntries.First(e=>e.Deck.id==saved.id))&&seatDecks[1]==saved.id&&seatDecks[0]==other,"chosen seat only");
   OpenDeckSelection(0);deckSelectionSearch="marés";yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,label+"-list.png"));deckSelectionOpen=false;
   Check(seatDecks[0]==other&&JsonUtility.ToJson(library.Data)==before,"cancel/search do not alter selection or collection");
   EnsureNetwork();networkPage=true;var task=host?network.Host(false,false,"Picker verification",2,false,label,7940):network.Join(false,"127.0.0.1",label,false,7940);
   while(!task.IsCompleted)yield return null;if(task.IsFaulted)throw task.Exception;
   yield return Wait(()=>network.State!=null&&!network.Busy,"hello");OpenDeckSelection(network.State.seat,true);
   Check(ChooseDeckEntry(deckSelectionEntries.First(e=>e.Deck.id=="precon-COL001-D01")),"online deck choice");
   networkDeckConfirmed=network.Send(new RoomRequest{type="deck",deck=NetworkDeck()});yield return Wait(()=>!network.Busy,"confirm initial deck");
   Check(networkDeckConfirmed&&string.IsNullOrEmpty(network.LastError),"host accepted initial deck");network.Send(new RoomRequest{type="ready",ready=true});
   yield return Wait(()=>!network.Busy&&network.State.members.Any(m=>m.seat==network.State.seat&&m.ready),"ready");
   OpenDeckSelection(network.State.seat,true);deckSelectionOpen=false;
   Check(networkDeckConfirmed&&network.State.members.First(m=>m.seat==network.State.seat).ready,"cancel online preserves confirmation/readiness");
   OpenDeckSelection(network.State.seat,true);Check(ChooseDeckEntry(deckSelectionEntries.First(e=>e.Deck.id=="precon-COL001-D02"))&&!networkDeckConfirmed,"changing deck clears confirmation");
   yield return Wait(()=>!network.Busy&&!network.State.members.First(m=>m.seat==network.State.seat).ready,"unready after change");
   Check(NetworkDeck().commander=="MED-193","correct chosen deck payload");networkDeckConfirmed=network.Send(new RoomRequest{type="deck",deck=NetworkDeck()});yield return Wait(()=>!network.Busy,"confirm changed deck");
   Check(string.IsNullOrEmpty(network.LastError)&&network.State.members.First(m=>m.seat==network.State.seat).commander=="MED-193","host accepts changed commander");
   Check(JsonUtility.ToJson(library.Data)==before,"online selection does not grant inventory or change balance");
   File.WriteAllText(Path.Combine(dir,label+".txt"),"PASS: "+checks+" checks; local selection/filter/cancel; online choice, readiness reset and authoritative deck confirmation; runtime errors="+errors.Count);
   yield return new WaitForSecondsRealtime(host?10:2);task=network.Stop();while(!task.IsCompleted)yield return null;Application.logMessageReceived-=listen;Application.Quit(0);
   void Check(bool ok,string why){checks++;if(!ok)throw new Exception("DECK PICKER: "+why);}
   IEnumerator Wait(Func<bool> done,string why){float deadline=Time.unscaledTime+30;while(!done()&&Time.unscaledTime<deadline)yield return null;Check(done(),why);}
  }
 }
}
