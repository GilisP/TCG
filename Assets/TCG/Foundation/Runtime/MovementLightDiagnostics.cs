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
        IEnumerator VerifyMovementLight()
        {
            string directory=Path.GetFullPath(Path.Combine(Application.dataPath,"..","MovementVerification"));Directory.CreateDirectory(directory);
            var errors=new List<string>();Application.LogCallback listener=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors.Add(m);};Application.logMessageReceived+=listener;
            var mains=Enumerable.Range(0,2).Select(_=>Enumerable.Repeat("MED-085",100).ToArray()).ToArray();
            var lands=Enumerable.Range(0,2).Select(_=>Enumerable.Repeat("test-land-0",50).ToArray()).ToArray();
            match=new Match(catalog,effects,2,false,5,mains,lands){AutomaticResponses=true};world.Bind(match);menu=false;handoff=false;world.Distance=16;world.Yaw=45;
            Require(Submit(ActionKind.DrawMain),"compra");int home=Match.Capital(match.Active,2);
            Require(Submit(ActionKind.Place,home),"terreno");Require(Submit(ActionKind.NextPhase),"fase principal");Require(Submit(ActionKind.Play,home,card:"MED-085"),"criatura");
            selectedCell=home;selectedUnit=match.Board[home].Pieces.First().Id;
            yield return new WaitForSecondsRealtime(1);
            int expected=Enumerable.Range(0,121).Count(i=>match.CanMove(selectedUnit,i));
            Require(expected>0&&world.MovementLightCount==expected,"feixes apenas nos destinos legais");
            yield return Capture("01-movimento.png");
            handoff=true;yield return new WaitForSecondsRealtime(.2f);Require(world.MovementLightCount==0,"oculta durante troca de jogador");
            handoff=false;yield return new WaitForSecondsRealtime(.2f);Require(world.MovementLightCount==expected,"restaura seleção");
            int target=Enumerable.Range(0,121).First(i=>match.CanMove(selectedUnit,i));Require(Submit(ActionKind.Move,target,selectedUnit),"mover");
            yield return new WaitForSecondsRealtime(.3f);Require(world.MovementLightCount==0,"limpa ao mover e desmarcar");
            yield return Capture("02-apos-mover.png");
            File.WriteAllText(Path.Combine(directory,"result.txt"),"Movement light checks passed\nLegal destinations: "+expected+"\nRuntime errors: "+errors.Count+"\n"+string.Join("\n",errors));
            Application.logMessageReceived-=listener;Application.Quit(errors.Count==0?0:1);
            void Require(bool value,string label){if(!value)throw new Exception("Movement light: "+label);}
            IEnumerator Capture(string file){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,file));yield return new WaitForSecondsRealtime(.8f);}
        }
    }
}
