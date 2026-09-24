using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
 public sealed partial class TableView
 {
  IEnumerator VerifyBoosters()
  {
   string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","BoosterVerification"));Directory.CreateDirectory(dir);
   var errors=new List<string>();Application.LogCallback listener=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors.Add(m);};Application.logMessageReceived+=listener;
   collectionStore=new CollectionStore(Path.Combine(dir,"profile-"+Guid.NewGuid().ToString("N")));library=collectionStore.Load(catalog);
   NavigateHub(HubPage.Play);yield return Capture("01-menu.png");
   NavigateHub(HubPage.Shop);shopBoosters=true;yield return Capture("02-booster-shop.png");
   Require(PurchaseBooster(),"purchase");yield return new WaitForSecondsRealtime(2);yield return Capture("03-opening.png");
   int coins=library.Data.coins;string receipt=openedBooster.id;library=collectionStore.Load(catalog);
   Require(library.Data.coins==coins&&library.Data.lastBooster.id==receipt,"saved before reveal");openedBooster=null;
   var store=collectionStore;string blocker=Path.Combine(dir,"blocked");File.WriteAllText(blocker,"test");collectionStore=new CollectionStore(blocker);
   string before=JsonUtility.ToJson(library.Data);Require(!PurchaseBooster()&&before==JsonUtility.ToJson(library.Data)&&openedBooster==null,"write failure rollback");collectionStore=store;
   library.Collect("MED-225");library.Data.variants.Add(CardStyles.Key("MED-225","illuminated"));library.SelectStyle("MED-225","illuminated");collectionStore.Save(library);
   OpenLibrary();librarySearch="";yield return Capture("04-collection.png");
   inspected=catalog.Get("MED-225");inspectionIdentity=inspected.Id;inspectionStyle="illuminated";
   Require(ApplyCardStyle(false),"apply collection appearance");
   draft=new DeckData{commander="MED-225",experimental=true};Require(ApplyCardStyle(true),"apply deck appearance");
   library=collectionStore.Load(catalog);Require(library.SelectedStyle("MED-225",library.Data.decks[0])=="illuminated","deck appearance persisted");
   collectionStore=new CollectionStore(blocker);before=JsonUtility.ToJson(library.Data);inspectionStyle="standard";Require(!ApplyCardStyle(true)&&before==JsonUtility.ToJson(library.Data)&&draft.cardLooks[0].styleId=="illuminated","appearance failure rollback");collectionStore=store;inspectionStyle="illuminated";
   yield return Capture("05-illustrated-card.png");inspected=null;
   Require(Resources.Load<Texture2D>("CardArt/elo-iluminura-v1")!=null,"illustration imported");
   NavigateHub(HubPage.Shop);shopBoosters=false;shopCosmetics=false;shopSearch="";yield return Capture("06-card-shop.png");
   NavigateHub(HubPage.Play);useSavedDecks=false;StartMatch();handoff=false;yield return Capture("07-table.png");
   File.WriteAllText(Path.Combine(dir,"result.txt"),"Booster purchase, persisted receipt, write rollback, collection/deck appearance persistence and rollback, art import and presentation captured. Runtime errors: "+errors.Count+"\n"+string.Join("\n",errors));Application.logMessageReceived-=listener;Application.Quit(errors.Count==0?0:1);
   void Require(bool condition,string message){if(!condition)throw new Exception("BOOSTER UI: "+message);}
   IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.6f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name));yield return new WaitForSecondsRealtime(.8f);}
  }
 }
}
