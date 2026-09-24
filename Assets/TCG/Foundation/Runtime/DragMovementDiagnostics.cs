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
        readonly Queue<Event> dragVerificationEvents=new Queue<Event>();
        void ProcessDragVerification()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(Event.current.type!=EventType.Repaint||dragVerificationEvents.Count==0)return;
            var saved=Event.current;try{Event.current=dragVerificationEvents.Dequeue();BoardInput();}finally{Event.current=saved;}
#endif
        }
        IEnumerator VerifyDragMovement()
        {
            string directory=Path.GetFullPath(Path.Combine(Application.dataPath,"..","DragVerification"));Directory.CreateDirectory(directory);
            var errors=new List<string>();Application.LogCallback listener=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors.Add(m);};Application.logMessageReceived+=listener;
            var mains=Enumerable.Range(0,2).Select(_=>Enumerable.Repeat("MED-085",100).ToArray()).ToArray();
            var lands=Enumerable.Range(0,2).Select(_=>Enumerable.Repeat("test-land-0",50).ToArray()).ToArray();
            match=new Match(catalog,effects,2,false,5,mains,lands){AutomaticResponses=true,AutoAdvanceAfterTerrain=true};world.Bind(match);menu=false;handoff=false;world.Distance=20;world.Yaw=0;
            Require(Submit(ActionKind.DrawMain),"compra");int home=Match.Capital(match.Active,2);
            Require(Submit(ActionKind.Place,home),"colocação");Require(match.Phase==Stage.Main,"terreno pula confirmação da fase");
            Require(Submit(ActionKind.Play,home,card:"MED-085"),"criatura");int id=match.Board[home].Pieces.First().Id;
            yield return new WaitForSecondsRealtime(1);int direction=home%11==0?1:-1;int neighbor=home+direction;
            yield return Drag(id,neighbor);
            Require(match.Position(id)==neighbor&&match.Find(id).Movement==1,"arrastar modelo executa movimento");
            int goal=home+direction*4;
            yield return Drag(id,goal);
            Require(match.OrderFor(id)?.Destination==goal&&match.Position(id)==neighbor,"rota distante espera antes do buraco");
            Require(world.MovementStopCount==2,"duas paradas numeradas em três espaços com movimento dois");
            Require(world.MovementLightCount==0,"rota substitui bolinhas de destinos alternativos");
            yield return Capture("01-rota-aguardando.png");
            Require(Submit(ActionKind.CancelMove,unit:id),"cancelar rota");Require(match.OrderFor(id)==null,"cancelamento remove destino");
            yield return new WaitForSecondsRealtime(.3f);
            Require(world.MovementLightCount==0&&world.MovementStopCount==0,"oculta luz e paradas após cancelar e desmarcar");
            yield return Capture("02-rota-cancelada.png");
            File.WriteAllText(Path.Combine(directory,"result.txt"),"Drag gesture checks passed\nAuto terrain phase: passed\nHole waiting and cancellation: passed\nRuntime errors: "+errors.Count+"\n"+string.Join("\n",errors));
            Application.logMessageReceived-=listener;Application.Quit(errors.Count==0?0:1);
            void Require(bool value,string label){if(!value)throw new Exception("DRAG: "+label+" / "+notice);}
            Vector2 Point(Vector3 at){var s=world.View.WorldToScreenPoint(at);return new Vector2(s.x/scale,(Screen.height-s.y)/scale);}
            IEnumerator Drag(int piece,int to)
            {
                var from=Point(TableWorld.Position(match.Position(piece))+Vector3.up*.5f);var destination=Point(TableWorld.Position(to));
                dragVerificationEvents.Enqueue(new Event{type=EventType.MouseDown,button=0,mousePosition=from});
                dragVerificationEvents.Enqueue(new Event{type=EventType.MouseDrag,button=0,mousePosition=destination});
                dragVerificationEvents.Enqueue(new Event{type=EventType.MouseUp,button=0,mousePosition=destination});
                while(dragVerificationEvents.Count>0)yield return null;yield return new WaitForSecondsRealtime(1);
            }
            IEnumerator Capture(string file){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,file));yield return new WaitForSecondsRealtime(.8f);}
        }
    }
}
