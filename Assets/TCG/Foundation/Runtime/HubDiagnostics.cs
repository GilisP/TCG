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
        IEnumerator VerifyHub()
        {
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","HubVerification"));Directory.CreateDirectory(dir);
            var errors=new List<string>();Application.LogCallback listener=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors.Add(m);};Application.logMessageReceived+=listener;
            collectionStore=new CollectionStore(Path.Combine(dir,"profile-"+Guid.NewGuid().ToString("N")));library=collectionStore.Load(catalog);
            NavigateHub(HubPage.Play);yield return Capture("01-jogar.png");
            NavigateHub(HubPage.Shop);shopSearch="Mago Colecionador";yield return Capture("02-loja-cartas.png");
            pendingPurchase="MED-193";purchaseIsCosmetic=false;yield return Capture("03-confirmacao.png");pendingPurchase=null;
            Require(library.Data.coins==500,"cancel leaves wallet intact");
            foreach(string id in new[]{"MED-121","test-land-3","MED-194"}){pendingPurchase=id;purchaseIsCosmetic=false;Require(CompletePurchase(),"buy card");}
            pendingPurchase="ember";purchaseIsCosmetic=true;Require(CompletePurchase(),"buy back");
            Require(library.Data.coins==200,"four purchases exact balance");
            pendingPurchase="ember";Require(!CompletePurchase()&&library.Data.coins==200,"duplicate UI purchase leaves balance");
            var store=collectionStore;string blocker=Path.Combine(dir,"not-a-directory");File.WriteAllText(blocker,"test");
            collectionStore=new CollectionStore(blocker);pendingPurchase="MED-086";purchaseIsCosmetic=false;
            Require(!CompletePurchase()&&library.Data.coins==200&&!library.Owns("MED-086"),"failed write rolls back purchase");collectionStore=store;hubNotice="";
            OpenLibrary();NewDraft(true);for(int n=0;n<100;n++)AddToDraft(catalog.Get("MED-121"));for(int n=0;n<50;n++)AddToDraft(catalog.Get("test-land-3"));
            draft.commander="MED-194";draft.cardBack="ember";draft.name="Guarda da Brasa";SaveDraft();librarySearch="MED-194";
            Require(library.Validate(draft,true).Count==0,"equipped deck valid");
            yield return Capture("04-colecao-decks.png");
            cosmeticsTab=true;yield return Capture("05-cosmeticos.png");cosmeticsTab=false;
            NavigateHub(HubPage.Shop);shopCosmetics=true;yield return Capture("06-loja-cosmeticos.png");
            library=collectionStore.Load(catalog);Require(library.Data.coins==200&&library.Owns("MED-194")&&library.Data.decks[0].cardBack=="ember","purchase and equipped deck persist");
            NavigateHub(HubPage.Settings);yield return Capture("07-menu.png");
            int revision=match.Revision;NavigateHub(HubPage.Collection);NavigateHub(HubPage.Shop);NavigateHub(HubPage.Play);Require(match.Revision==revision,"navigation does not change match");
            players=4;useSavedDecks=true;for(int p=0;p<4;p++)seatDecks[p]=draft.id;yield return Capture("08-selecao-decks.png");StartMatch();handoff=false;
            Require(!menu&&sessionAvailable&&match.Seats.Count==4,"launch from saved decks");
            Require(world.GetComponentsInChildren<Renderer>(true).Where(r=>r.name=="Verso").All(r=>Vector4.Distance(r.sharedMaterial.color,TableWorld.Hex("873F2C"))<.01f),"backs applied to held cards and piles");
            yield return Capture("09-versos-na-mesa.png");
            File.WriteAllText(Path.Combine(dir,"result.txt"),"Four-screen navigation, purchase/cancel/duplicate/write-failure rollback, save/reload, deck launch and applied backs passed\nRuntime errors: "+errors.Count+"\n"+string.Join("\n",errors));Application.logMessageReceived-=listener;Application.Quit(errors.Count==0?0:1);
            void Require(bool value,string name){if(!value)throw new Exception("HUB UI: "+name);}
            IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.6f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name));yield return new WaitForSecondsRealtime(.7f);}
        }
    }
}
