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
        // Explicit development-build smoke test, never active during an ordinary player session.
        void StartDiagnosticsIfRequested()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(Environment.GetCommandLineArgs().Contains("-tcg-net-role")) StartCoroutine(VerifyNetwork());
            else if(Environment.GetCommandLineArgs().Contains("-tcg-foil-commanders-verify")) StartCoroutine(VerifyFoilCommanders());
            else if(Environment.GetCommandLineArgs().Contains("-tcg-booster-verify")) StartCoroutine(VerifyBoosters());
            else if(Environment.GetCommandLineArgs().Contains("-tcg-commanders-verify")) StartCoroutine(VerifyCommanders());
            else if(Environment.GetCommandLineArgs().Contains("-tcg-hub-verify")) StartCoroutine(VerifyHub());
            else if(Environment.GetCommandLineArgs().Contains("-tcg-filter-verify")) StartCoroutine(VerifyFilters());
            else if(Environment.GetCommandLineArgs().Contains("-tcg-collection-verify")) StartCoroutine(VerifyCollection());
            else if(Environment.GetCommandLineArgs().Contains("-tcg-personality-verify")) StartCoroutine(VerifyPersonality());
            else if(Environment.GetCommandLineArgs().Contains("-tcg-reaction-verify")) StartCoroutine(VerifyReactionsAndPlacement());
            else if(Environment.GetCommandLineArgs().Contains("-tcg-drag-verify")) StartCoroutine(VerifyDragMovement());
            else if(Environment.GetCommandLineArgs().Contains("-tcg-movement-verify")) StartCoroutine(VerifyMovementLight());
            else if(Environment.GetCommandLineArgs().Contains("-tcg-qol-verify")) StartCoroutine(VerifyQualityOfLife());
            else if(Environment.GetCommandLineArgs().Contains("-tcg-medieval-verify")) StartCoroutine(VerifyMedievalPresentation());
            else if(Environment.GetCommandLineArgs().Contains("-tcg-verify")) StartCoroutine(VerifyPresentation());
#endif
        }
        IEnumerator VerifyPresentation()
        {
            var directory=Path.GetFullPath(Path.Combine(Application.dataPath,"..","Verification")); Directory.CreateDirectory(directory);
            var errors=new List<string>(); Application.LogCallback listener=(message,trace,type)=>{ if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert) errors.Add(message+"\n"+trace); };
            Application.logMessageReceived+=listener;
            yield return new WaitForSecondsRealtime(2);
            yield return Capture("01-menu.png");
            medieval=false; StartMatch(); handoff=false;
            yield return new WaitForSecondsRealtime(1);
            foreach(int tile in Enumerable.Range(0,121).Where(i=>match.Board[i].Terrain!=null))
            {
                var ray=world.View.ScreenPointToRay(world.View.WorldToScreenPoint(TableWorld.Position(tile)));
                Require(Physics.Raycast(ray,out var hit,70)&&hit.collider.TryGetComponent<CellHit>(out var cell)&&cell.Index==tile,"seleção espacial da casa "+tile);
            }
            yield return Capture("02-preparacao.png");
            Require(Submit(ActionKind.DrawMain),"compra");
            int home=Match.Capital(match.Active,match.Seats.Count);
            int placement=Enumerable.Range(0,121).Where(match.CanPlace).OrderBy(i=>Math.Abs(i%11-5)+Math.Abs(i/11-5)).First();
            Require(Submit(ActionKind.Place,placement),"terreno"); if(match.Phase==Stage.Terrain)Require(Submit(ActionKind.NextPhase),"fase principal");
            for(int attempt=0;attempt<4;attempt++)
            {
                var card=match.HandFor(match.Controller).FirstOrDefault(c=>c.Kind==CardType.Creature&&match.CanPlay(c,home)); if(card==null) break;
                Require(Submit(ActionKind.Play,home,card:card.Id),"invocação");
                while(match.Stack.Count>0) { Require(Submit(ActionKind.Pass),"prioridade"); handoff=false; }
            }
            selectedCell=home; selectedUnit=match.Board[home].Pieces.FirstOrDefault()?.Id??-1;
            yield return new WaitForSecondsRealtime(2);
            yield return Capture("03-partida.png");
            if(selectedUnit>=0)
            {
                int destination=Match.Neighbors(home).FirstOrDefault(i=>match.CanMove(selectedUnit,i));
                if(match.CanMove(selectedUnit,destination)) Require(Submit(ActionKind.Move,destination,selectedUnit),"movimento");
            }
            double elapsed=0; int frames=0; while(elapsed<3) { yield return null; elapsed+=Time.unscaledDeltaTime; frames++; }
            collection=true; yield return null; yield return Capture("04-expansoes.png"); collection=false;
            Require(Submit(ActionKind.NextPhase),"fase final");
            int previous=match.Active; while(match.Phase==Stage.End&&!match.Over) { Require(Submit(ActionKind.Pass),"fim de turno"); handoff=false; }
            Require(match.Phase==Stage.Draw,"troca de turno");
            handoff=true; yield return null; yield return Capture("05-privacidade.png");
            File.WriteAllText(Path.Combine(directory,"result.txt"),"Presentation smoke completed\nRevision: "+match.Revision+"\nAverage FPS over "+elapsed.ToString("F2")+" seconds: "+(frames/elapsed).ToString("F1")+"\nRuntime errors: "+errors.Count+"\n"+string.Join("\n",errors));
            Application.logMessageReceived-=listener; Application.Quit(errors.Count==0?0:1);
            IEnumerator Capture(string file) { yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(directory,file)); yield return new WaitForSecondsRealtime(.8f); }
            void Require(bool value,string step) { if(!value) throw new InvalidOperationException("Presentation smoke failed: "+step+" / "+notice); }
        }
    }
}



