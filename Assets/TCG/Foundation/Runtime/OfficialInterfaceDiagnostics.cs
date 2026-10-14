using System;
using System.Collections;
using System.IO;
using System.Linq;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
 public sealed partial class TableView
 {
  IEnumerator VerifyOfficialInterface()
  {
   string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","OfficialInterfaceVerification"));Directory.CreateDirectory(dir);
   int checks=0;var errors=new System.Collections.Generic.List<string>();
   Application.LogCallback listen=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors.Add(m);};Application.logMessageReceived+=listen;
   collectionStore=new CollectionStore(Path.Combine(dir,"profile-"+Guid.NewGuid().ToString("N")));library=collectionStore.Load(catalog);
   var precons=PreconstructedDeckLoader.Load(catalog);precons.Import(library,precons.Decks[0]);
   Check(catalog.IsGameplayCatalog&&PlannedDeckLoader.Load(catalog).Decks.Count==0,"active catalog and old templates disabled");
   foreach(HubPage page in Enum.GetValues(typeof(HubPage))){NavigateHub(page);yield return new WaitForSecondsRealtime(.2f);Check(errors.Count==0,"render hub "+page);}
   NavigateHub(HubPage.Play);PrepareLocalTable();OpenDeckSelection(0);yield return new WaitForSecondsRealtime(.2f);
   Check(deckSelectionEntries.Count>=10&&deckSelectionEntries.All(e=>e.Deck.main.Concat(e.Deck.terrains).All(catalog.IsActive)),"only valid active deck choices");
   var entry=deckSelectionEntries.First(e=>e.Deck.id=="precon-COL001-D02");Check(ChooseDeckEntry(entry),"choose official deck");
   Check(seatDecks[0]==entry.Deck.id,"selection by identity");
   for(int seat=1;seat<players;seat++)seatDecks[seat]="precon-"+precons.Decks[seat].id;
   StartMatch();handoff=false;yield return new WaitForSecondsRealtime(.2f);Check(match.Seats[0].Commander.Id==entry.Deck.commander,"match commander selected");
   NavigateHub(HubPage.Shop);shopBoosters=true;Check(CurrentBooster().Pool(catalog).All(c=>catalog.IsActive(c.Id)),"booster active pool");
   Check(PurchaseBooster(),"booster purchase");Check(openedBooster.rewards.All(x=>catalog.IsActive(x.cardId)),"active rewards");yield return new WaitForSecondsRealtime(4);
   openedBooster=null;NavigateHub(HubPage.Play);OpenStoryMap();yield return new WaitForSecondsRealtime(.2f);Check(errors.Count==0,"story map and card faces render");
   storyPage=false;NavigateHub(HubPage.Play);
   File.WriteAllText(Path.Combine(dir,"result.txt"),"PASS: "+checks+" checks; active="+catalog.Cards.Count+"; runtime errors="+errors.Count);
   Application.logMessageReceived-=listen;
   if(!Environment.GetCommandLineArgs().Contains("-tcg-ui-review"))Application.Quit(errors.Count==0?0:1);
   void Check(bool ok,string why){checks++;if(!ok)throw new Exception("OFFICIAL UI: "+why);}
  }
 }
}
