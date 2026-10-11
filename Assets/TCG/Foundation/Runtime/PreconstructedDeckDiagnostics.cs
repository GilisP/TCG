using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
 public sealed partial class TableView
 {
  IEnumerator VerifyPrecons()
  {
   string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","PreconVerification"));Directory.CreateDirectory(dir);
   int checks=0;var errors=new List<string>();Application.LogCallback listener=(message,trace,kind)=>{if(kind==LogType.Error||kind==LogType.Exception||kind==LogType.Assert){errors.Add(message);File.WriteAllText(Path.Combine(dir,"result.txt"),"FAIL: "+string.Join("\n",errors));Application.Quit(1);}};Application.logMessageReceived+=listener;
   collectionStore=new CollectionStore(Path.Combine(dir,"profile-"+Guid.NewGuid().ToString("N")));library=collectionStore.Load(catalog);
   var precons=PreconstructedDeckLoader.Load(catalog);var ready=precons.Decks.Where(precons.Ready).ToArray();int coins=library.Data.coins;
   Check(precons.Decks.Count==10&&ready.Length>=9,"ten approved lists and nine or more ready");
   OpenLibrary();preconsOpen=true;yield return Capture("01-precons.png");
   foreach(var deck in ready)precons.Import(library,deck);collectionStore.Save(library);library=collectionStore.Load(catalog);
   Check(library.Data.decks.Count==ready.Length&&library.Data.coins==coins,"explicit free import persists without coin changes");
   foreach(var deck in ready){precons.Import(library,deck);Check(library.Validate(library.Data.decks.Single(d=>d.id=="precon-"+deck.id),true).Count==0,"imported list legal "+deck.id);}
   Check(library.Data.decks.Count==ready.Length,"reimport does not duplicate");
   var selected=ready.First(d=>d.commander=="MED-240");draft=library.Data.decks.Single(d=>d.id=="precon-"+selected.id).Copy();preconsOpen=false;SelectDeckSection(false);yield return Capture("02-main.png");SelectDeckSection(true);yield return Capture("03-terrains.png");
   players=4;teams=false;useSavedDecks=true;for(int seat=0;seat<players;seat++)seatDecks[seat]="precon-"+ready[seat].id;NavigateHub(HubPage.Play);yield return Capture("04-deck-choice.png");StartMatch();handoff=false;
   Check(match.Seats.Count==4&&match.Seats.All(s=>s.Commander!=null&&s.MainCount==95),"four complete linked decks start");yield return Capture("05-table.png");
   File.WriteAllText(Path.Combine(dir,"result.txt"),"PASS: "+checks+" checks; "+ready.Length+" ready precons; explicit import/persistence/selection/table; runtime errors="+errors.Count);Application.logMessageReceived-=listener;Application.Quit(0);
   void Check(bool ok,string message){checks++;if(!ok)throw new Exception("PRECONS UI: "+message);}
   IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.6f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name));yield return new WaitForSecondsRealtime(.3f);}
  }
 }
}
