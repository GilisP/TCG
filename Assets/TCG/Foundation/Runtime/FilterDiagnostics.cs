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
  IEnumerator VerifyFilters()
  {
   string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","FilterVerification"));Directory.CreateDirectory(dir);
   var errors=new List<string>();Application.LogCallback listener=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors.Add(m);};Application.logMessageReceived+=listener;
   collectionStore=new CollectionStore(Path.Combine(dir,"profile-"+Guid.NewGuid().ToString("N")));library=collectionStore.Load(catalog);foreach(var c in catalog.Cards)library.Collect(c.Id);
   OpenLibrary();NewDraft(true);draft.name="Vigília do Eclipse";draft.commander="MED-225";for(int n=0;n<5;n++)AddToDraft(catalog.Get("MED-085"));for(int n=0;n<12;n++)AddToDraft(catalog.Get("test-land-0"));SaveDraft();string first=draft.id;
   SelectDeckSection(false);librarySearch="";yield return Capture("01-principal.png");
   libraryFilters.Colors.Add(0);libraryFilters.Colors.Add(1);libraryFilters.CommandersOnly=true;OpenFilters(false);yield return Capture("02-filtros-biblioteca.png");filtersOpen=false;
   Require(FilteredLibrary().Any(c=>c.Id=="MED-225")&&!FilteredLibrary().Any(c=>c.Id=="MED-230"),"bicolor OR filters");yield return Capture("03-resultados.png");
   libraryFilters.Clear();SelectDeckSection(true);librarySectionOnly=true;Require(FilteredLibrary().All(c=>c.Kind==CardType.Terrain),"terrain library only");yield return Capture("04-terrenos.png");
   deckFilters.Colors.Add(1);Require(!FilteredDeck().Any()&&draft.terrains.Count==12,"filters preserve hidden deck counts");OpenFilters(true);yield return Capture("05-filtros-deck.png");filtersOpen=false;deckFilters.Clear();
   NewDraft(true);draft.name="Guarda da Floresta";draft.commander="MED-226";for(int n=0;n<5;n++)AddToDraft(catalog.Get("MED-097"));for(int n=0;n<12;n++)AddToDraft(catalog.Get("test-land-5"));SaveDraft();string second=draft.id;
   library=collectionStore.Load(catalog);Require(library.Data.decks.Single(d=>d.id==first).terrains.All(x=>x=="test-land-0")&&library.Data.decks.Single(d=>d.id==second).terrains.All(x=>x=="test-land-5"),"independent terrain decks persist");
   draft=library.Data.decks.Single(d=>d.id==first).Copy();SelectDeckSection(true);libraryFilters.Clear();librarySectionOnly=false;librarySearch="MED-225";yield return Capture("06-deck-recarregado.png");
   players=2;useSavedDecks=true;seatDecks[0]=first;seatDecks[1]=second;NavigateHub(HubPage.Play);yield return Capture("07-decks-vinculados.png");
   Require(library.Validate(library.Data.decks.Single(d=>d.id==first),true).Count==0,"first deck legal");Require(library.Validate(library.Data.decks.Single(d=>d.id==second),true).Count==0,"second deck legal");StartMatch();handoff=false;Require(!menu,"paired decks launch");
   Require(Enumerable.Range(0,4).All(i=>match.Seats[0].Top(i)?.Id=="test-land-0"&&match.Seats[1].Top(i)?.Id=="test-land-5"),"each seat uses assigned terrain deck");yield return Capture("08-partida.png");
   File.WriteAllText(Path.Combine(dir,"result.txt"),"Filters, independent deck sections, paired persistence and match launch passed.\nRuntime errors: "+errors.Count+"\n"+string.Join("\n",errors));Application.logMessageReceived-=listener;Application.Quit(errors.Count==0?0:1);
   void Require(bool v,string n){if(!v)throw new Exception("FILTER UI: "+n);}
   IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.5f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name));yield return new WaitForSecondsRealtime(.5f);}
  }
 }
}

