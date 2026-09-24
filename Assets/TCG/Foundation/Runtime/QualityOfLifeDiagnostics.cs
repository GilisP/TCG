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
        IEnumerator VerifyQualityOfLife()
        {
            string directory=Path.GetFullPath(Path.Combine(Application.dataPath,"..","QualityVerification"));Directory.CreateDirectory(directory);
            var errors=new List<string>();Application.LogCallback listener=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors.Add(m);};Application.logMessageReceived+=listener;
            yield return new WaitForSecondsRealtime(2);
            players=4;StartMatch();menu=false;handoff=false;world.Distance=23;world.Elevation=57;
            yield return new WaitForSecondsRealtime(1);
            Require(match.AutomaticResponses,"passagem automática habilitada na mesa real");
            Require(world.GetComponentsInChildren<PileHit>().Length==16,"quatro pilhas por jogador");
            foreach(var pile in world.GetComponentsInChildren<PileHit>())
            {
                var point=pile.transform.position+Vector3.up*.55f;var ray=world.View.ScreenPointToRay(world.View.WorldToScreenPoint(point));
                Require(Physics.Raycast(ray,out var hit,70)&&hit.collider.GetComponent<PileHit>()==pile,"raycast da pilha "+pile.Player+"/"+pile.Pile);
            }
            yield return Capture("01-salao.png");
            world.SetRoom(1);yield return Capture("02-taverna.png");
            int before=match.Seats[match.Active].MainCount;Require(Submit(ActionKind.DrawMain),"compra");Require(match.Seats[match.Active].MainCount==before-1,"contagem real do deck");
            yield return Capture("03-pilhas.png");
            players=2;StartMatch();handoff=false;world.Distance=23;world.Yaw=45;
            yield return new WaitForSecondsRealtime(1);
            Require(world.GetComponentsInChildren<PileHit>().Length==8,"dois jogadores em lados opostos");yield return Capture("04-duelo.png");
            handoff=true;yield return Capture("05-privacidade.png");
            double elapsed=0;int frames=0;while(elapsed<3){yield return null;elapsed+=Time.unscaledDeltaTime;frames++;}
            File.WriteAllText(Path.Combine(directory,"result.txt"),"QOL presentation completed\nRuntime errors: "+errors.Count+"\nAverage FPS: "+(frames/elapsed).ToString("F1")+"\n"+string.Join("\n",errors));
            Application.logMessageReceived-=listener;Application.Quit(errors.Count==0?0:1);
            void Require(bool value,string label){if(!value)throw new Exception("QOL presentation: "+label);}
            IEnumerator Capture(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,name));yield return new WaitForSecondsRealtime(.8f);}
        }
    }
}
