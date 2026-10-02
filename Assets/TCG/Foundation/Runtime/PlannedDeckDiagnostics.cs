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
        IEnumerator VerifyPlannedDecks()
        {
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","PlannedDeckVerification"));Directory.CreateDirectory(dir);
            var errors=new List<string>();Application.LogCallback listener=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors.Add(m);};Application.logMessageReceived+=listener;
            collectionStore=new CollectionStore(Path.Combine(dir,"profile-"+Guid.NewGuid().ToString("N")));library=collectionStore.Load(catalog);
            OpenLibrary();plannedDecksOpen=true;plannedDecks=PlannedDeckLoader.Load(catalog);yield return Capture("01-modelos.png");
            foreach(var plan in plannedDecks.Decks)library.ImportPlannedDeck(plan);PersistLibrary();library=collectionStore.Load(catalog);
            Require(library.Data.decks.Count==29&&library.Data.coins==500,"imports persist without spending coins");
            draft=library.Data.decks.First(d=>d.commander=="MED-240").Copy();plannedDecksOpen=false;SelectDeckSection(false);yield return Capture("02-deck-principal.png");
            SelectDeckSection(true);librarySectionOnly=true;yield return Capture("03-terrenos.png");
            players=2;useSavedDecks=true;seatDecks[0]=draft.id;seatDecks[1]=library.Data.decks.First(d=>d.commander=="IDEIA-004").id;NavigateHub(HubPage.Play);yield return Capture("04-selecao.png");
            StartMatch();handoff=false;Require(match.Seats.All(s=>s.MainCount==95),"planned match starts with complete decks");yield return Capture("05-mesa.png");
            Submit(ActionKind.DrawMain);handoff=false;Submit(ActionKind.Place,Match.Capital(match.Active,2));handoff=false;
            int at=Match.Capital(match.Active,2);typeof(Cell).GetProperty("Terrain").SetValue(match.Board[at],catalog.Get("PD26-LS-022"));((int[])typeof(Seat).GetField("mana",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(match.Seats[match.Active]))[6]=10;selectedCell=at;yield return Capture("06-habilidade-terreno.png");
            Require(match.CanActivateTerrain(at),"terrain action available");Submit(ActionKind.ActivateTerrain,at);
            for(int i=0;i<80&&(match.Stack.Count>0||match.Choice!=null);i++){handoff=false;if(match.Choice!=null)Submit(ActionKind.Choose,match.Choice.Options.First(x=>x.Key!=-1).Key);else Submit(ActionKind.Pass);yield return null;}
            Require(!match.CanActivateTerrain(at),"terrain once per turn");yield return Capture("07-efeito-resolvido.png");
            File.WriteAllText(Path.Combine(dir,"result.txt"),"PASS: 29 templates; import and reload; linked 100/50 decks; match start; terrain activation. Runtime errors: "+errors.Count+"\n"+string.Join("\n",errors));Application.logMessageReceived-=listener;Application.Quit(errors.Count==0?0:1);
            void Require(bool ok,string message){if(!ok)throw new Exception("PLANNED UI: "+message);}
            IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.6f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name));yield return new WaitForSecondsRealtime(.3f);}
        }
    }
}
