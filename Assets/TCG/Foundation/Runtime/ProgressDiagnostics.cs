using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TCG.Foundation;
namespace TCG.Table
{
    public sealed partial class TableView
    {
        IEnumerator VerifyProgress()
        {
            var dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","ProgressVerification"));Directory.CreateDirectory(dir);
            var errors=new List<string>();Application.LogCallback listen=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors.Add(m);};Application.logMessageReceived+=listen;
            collectionStore=new CollectionStore(Path.Combine(dir,"profile-"+Guid.NewGuid().ToString("N")));library=collectionStore.Load(catalog);
            Require(ProgressTransaction(()=>library.RecordMatch(new MatchNumbers{id="fixture",won=true,cards=12,terrains=10,turns=8})),"record");
            NavigateHub(HubPage.Settings);progressPage=true;yield return Capture("01-statistics.png");
            Require(ProgressTransaction(()=>library.ClaimMission(missionCatalog.missions[0])),"claim");
            library=collectionStore.Load(catalog);Require(library.Data.progress.boosters==1&&library.Data.coins==550,"reward persisted");
            string before=JsonUtility.ToJson(library.Data);Require(!ProgressTransaction(()=>library.ClaimMission(missionCatalog.missions[0]))&&before==JsonUtility.ToJson(library.Data),"no double claim");
            var savedStore=collectionStore;string blocked=Path.Combine(dir,"blocked");File.WriteAllText(blocked,"fixture");collectionStore=new CollectionStore(blocked);
            Require(!ProgressTransaction(()=>library.ClaimMission(missionCatalog.missions[1]))&&before==JsonUtility.ToJson(library.Data),"claim write rollback");
            Require(!PurchaseBooster(true)&&before==JsonUtility.ToJson(library.Data),"reward opening rollback");collectionStore=savedStore;
            NavigateHub(HubPage.Shop);shopBoosters=true;yield return Capture("02-reward-booster.png");Require(PurchaseBooster(true),"open reward");yield return Capture("03-free-opening.png");
            library=collectionStore.Load(catalog);Require(library.Data.progress.boosters==0&&library.Data.lastBooster.price==0,"free receipt persisted");openedBooster=null;
            StartMatch();handoff=false;yield return Capture("04-ruins.png");inspected=Match.Ruins;yield return Capture("05-ruins-inspection.png");
            File.WriteAllText(Path.Combine(dir,"result.txt"),"PASS: progress, unique claim, reload, claim/open rollback, free booster, ruins presentation. Runtime errors: "+errors.Count+"\n"+string.Join("\n",errors));Application.logMessageReceived-=listen;Application.Quit(errors.Count==0?0:1);
            void Require(bool ok,string label){if(!ok)throw new Exception("PROGRESS UI: "+label);}
            IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.7f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name));yield return new WaitForSecondsRealtime(.8f);}
        }
    }
}
