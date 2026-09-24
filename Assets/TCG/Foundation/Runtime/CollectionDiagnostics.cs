using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using TCG.Foundation;
namespace TCG.Table
{
    public sealed partial class TableView
    {
        IEnumerator VerifyCollection()
        {
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","CollectionVerification"));Directory.CreateDirectory(dir);var errors=new System.Collections.Generic.List<string>();Application.LogCallback listener=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors.Add(m);};Application.logMessageReceived+=listener;
            collectionStore=new CollectionStore(Path.Combine(dir,"isolated-profile-"+Guid.NewGuid().ToString("N")));library=collectionStore.Load(catalog);OpenLibrary();
            CollectCard("MED-121");CollectCard("test-land-3");CollectCard("MED-194");NewDraft(true);for(int n=0;n<5;n++)AddToDraft(catalog.Get("MED-121"));for(int n=0;n<12;n++)AddToDraft(catalog.Get("test-land-3"));draft.commander="MED-194";draft.name="Mesa de verificação";SaveDraft();
            if(library.Validate(draft,true).Count!=0)throw new Exception("Deck de teste inválido.");librarySearch="MED-1";yield return Capture("01-construtor.png");
            CollectCard("MED-193");inspected=catalog.Get("MED-193");yield return Capture("02-comandante-detalhado.png");inspected=null;
            var loaded=collectionStore.Load(catalog);if(loaded.Data.decks.Count!=1||!loaded.Owns("MED-193"))throw new Exception("Persistência não restaurou coleção/deck.");library=loaded;
            players=4;for(int p=0;p<4;p++)seatDecks[p]=draft.id;useSavedDecks=true;libraryOpen=false;StartMatch();handoff=false;world.Distance=23;world.Yaw=0;yield return Capture("03-spawns-centrais.png");
            for(int p=0;p<4;p++){var pos=TableWorld.Position(Match.Capital(p,4));if(pos.x!=0&&pos.z!=0)throw new Exception("Spawn fora do eixo central.");}
            File.WriteAllText(Path.Combine(dir,"result.txt"),"Collect, draft save/reload, deck validation and four-seat launch passed\nCentered spawns passed\nRuntime errors: "+errors.Count+"\n"+string.Join("\n",errors));Application.logMessageReceived-=listener;Application.Quit(errors.Count==0?0:1);
            IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(1);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name));yield return new WaitForSecondsRealtime(.8f);}
        }
    }
}
