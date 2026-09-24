using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TCG.Foundation;
namespace TCG.Table
{
    public sealed partial class TableView
    {
        Vector2? presentationPointer;
        readonly Queue<Event> presentationEvents=new Queue<Event>();
        void ProcessPresentationVerification()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(Event.current.type!=EventType.Repaint||presentationEvents.Count==0)return;
            var saved=Event.current;try{Event.current=presentationEvents.Dequeue();DrawHand();}finally{Event.current=saved;}
#endif
        }
        IEnumerator VerifyPersonality()
        {
            var dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","PersonalityVerification"));Directory.CreateDirectory(dir);
            var errors=new List<string>();Application.LogCallback listener=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors.Add(m);};Application.logMessageReceived+=listener;
            var mains=Enumerable.Range(0,2).Select(_=>Enumerable.Repeat("MED-085",100).ToArray()).ToArray();var lands=Enumerable.Range(0,2).Select(_=>Enumerable.Repeat("test-land-0",50).ToArray()).ToArray();
            match=new Match(catalog,effects,2,false,5,mains,lands){AutomaticResponses=true,AutoAdvanceAfterTerrain=true};world.Bind(match);menu=false;handoff=false;world.Distance=20;world.Yaw=0;
            Require(Submit(ActionKind.DrawMain),"draw");int home=Match.Capital(match.Active,2);Require(Submit(ActionKind.Place,home),"terrain");yield return new WaitForSecondsRealtime(1);
            presentationPointer=new Vector2(20,790);yield return Capture("01-mao-em-leque.png");
            int count=match.HandFor(match.Controller).Count;presentationPointer=FanRect(0,count).center;yield return null;Require(hoveredHand==0,"hover foremost exposed card");yield return Capture("02-carta-elevada.png");
            presentationPointer=new Vector2(LiftedCard(0,count).xMax-8,870);yield return null;Require(hoveredHand==0,"lifted card wins overlap");
            presentationEvents.Enqueue(new Event{type=EventType.MouseDown,button=0,mousePosition=presentationPointer.Value});while(presentationEvents.Count>0)yield return null;Require(selectedCard=="MED-085","hand click selects");
            presentationPointer=new Vector2(20,790);yield return ClickTile(home);Require(match.Board[home].Pieces.Count==1,"play from fan");
            selectedCard="MED-085";yield return ClickTile(home);Require(match.Board[home].Pieces.Count==2,"occupied placement");yield return Capture("03-duas-pecas.png");
            float pose=world.FigurePoseSample();yield return new WaitForSecondsRealtime(.4f);Require(Mathf.Abs(world.FigurePoseSample()-pose)>.000001f,"creature breathing animates");
            var previews=new List<GameObject>();int n=0;foreach(var id in new[]{"MED-085","MED-101","MED-097","MED-130","MED-118","MED-119"}){previews.Add(world.PreviewModel(catalog.Get(id),new Vector3(-2.5f+n%3*2.5f,0,-1.5f+n/3*2.5f)));n++;}
            world.Distance=12;world.Yaw=12;yield return Capture("04-personalidades.png");foreach(var go in previews)Destroy(go);
            var group=world.PreviewFormation(catalog.Get("MED-085"),9);yield return Capture("05-formacao-nove.png");Destroy(group);
            presentationPointer=null;File.WriteAllText(Path.Combine(dir,"result.txt"),"Fan hover/select and placement passed\nTwo occupants placed through mouse events\nFormation and models captured\nRuntime errors: "+errors.Count+"\n"+string.Join("\n",errors));Application.logMessageReceived-=listener;Application.Quit(errors.Count==0?0:1);
            void Require(bool ok,string message){if(!ok)throw new Exception("PERSONALITY: "+message+" / "+notice);}
            IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.5f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name));yield return new WaitForSecondsRealtime(.8f);}
            IEnumerator ClickTile(int tile){var p=world.View.WorldToScreenPoint(TableWorld.Position(tile));var at=new Vector2(p.x/scale,(Screen.height-p.y)/scale);dragVerificationEvents.Enqueue(new Event{type=EventType.MouseDown,button=0,mousePosition=at});dragVerificationEvents.Enqueue(new Event{type=EventType.MouseUp,button=0,mousePosition=at});while(dragVerificationEvents.Count>0)yield return null;yield return new WaitForSecondsRealtime(.7f);}
        }
    }
    public sealed partial class TableWorld
    {
        public float FigurePoseSample()=>pieces.Values.SelectMany(p=>p.GetComponentsInChildren<Transform>()).Where(t=>t.name=="Tronco").Sum(t=>t.localPosition.y);
        public GameObject PreviewFormation(Definition card,int count)
        {
            var root=Group("Prévia de formação",transform);root.localScale=Vector3.one*3;
            Block("Terreno de demonstração",Vector3.zero,new Vector3(.94f,.1f,.94f),Hex("526058"),root);
            for(int i=0;i<count;i++){var figure=Figure(card,Seats[i%4],root);figure.localPosition=PieceFormation.Offset(i,count);figure.localScale=Vector3.one*PieceFormation.Scale(count);figure.gameObject.AddComponent<FigureMotion>().Initialize(card.Art,i);}
            return root.gameObject;
        }
    }
}
