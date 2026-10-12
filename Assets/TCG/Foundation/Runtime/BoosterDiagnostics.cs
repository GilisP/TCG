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
  IEnumerator VerifyBoosterAnimation()
  {
   string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","BoosterAnimationVerification"));Directory.CreateDirectory(dir);
   int checks=0;var errors=new List<string>();Application.LogCallback listener=(message,trace,kind)=>{if(kind==LogType.Error||kind==LogType.Exception||kind==LogType.Assert){errors.Add(message);File.WriteAllText(Path.Combine(dir,"result.txt"),"FAIL: "+message);Application.Quit(1);}};Application.logMessageReceived+=listener;
   collectionStore=new CollectionStore(Path.Combine(dir,"profile-"+Guid.NewGuid().ToString("N")));library=collectionStore.Load(catalog);
   library.Data.coins=100000;collectionStore.Save(library);NavigateHub(HubPage.Shop);shopBoosters=true;
   Check(PurchaseBooster(),"purchase");string saved=JsonUtility.ToJson(library.Data);float start=boosterOpenedAt;
   Check(openedBooster.rewards.Count==CurrentBooster().cards,"reward count");
   Check(collectionStore.Load(catalog).Data.lastBooster.id==openedBooster.id,"receipt saved before animation");
   foreach(float time in new[]{.35f,1.05f,1.8f,2.5f,BoosterOpeningDuration+.1f})
   {
    boosterOpenedAt=Time.unscaledTime-time;
    yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,"phase-"+time.ToString("0.00",System.Globalization.CultureInfo.InvariantCulture)+".png"));yield return null;
    Check(saved==JsonUtility.ToJson(library.Data),"animation does not change rewards or balance");
   }
   boosterOpenedAt=start;Check(BoosterOpeningDuration>BoosterSealDuration,"timeline includes all card flips");
   // Same time transition used by Skip and Last opening: it never invokes a purchase.
   boosterOpenedAt=Time.unscaledTime-BoosterOpeningDuration;yield return null;
   Check(saved==JsonUtility.ToJson(library.Data),"skip preserves receipt");openedBooster=null;
   openedBooster=library.Data.lastBooster;boosterOpenedAt=Time.unscaledTime-20;yield return null;
   Check(saved==JsonUtility.ToJson(library.Data),"replay does not charge or draw again");openedBooster=null;
   var store=collectionStore;string blocker=Path.Combine(dir,"blocked-"+Guid.NewGuid().ToString("N"));File.WriteAllText(blocker,"test");collectionStore=new CollectionStore(blocker);
   Check(!PurchaseBooster()&&saved==JsonUtility.ToJson(library.Data)&&openedBooster==null,"failed save cannot start animation or charge");collectionStore=store;
   File.WriteAllText(Path.Combine(dir,"result.txt"),"PASS: "+checks+" checks; seal/split/flip/reveal rendered; saved before animation; skip/replay and failed-save rollback; runtime errors="+errors.Count);
   Application.logMessageReceived-=listener;Application.Quit(0);
   void Check(bool ok,string why){checks++;if(!ok)throw new Exception("BOOSTER ANIMATION: "+why);}
  }
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
