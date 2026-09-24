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
        Vector2? pileVerificationPointer;
        readonly Queue<Event> pileVerificationEvents=new Queue<Event>();
        Vector2 pileFramePointer;
        Vector2 PilePointer=>pileVerificationPointer??pileFramePointer;
        void ProcessPileVerification()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(Event.current.type!=EventType.Repaint||pileVerificationEvents.Count==0)return;
            var saved=Event.current;try{Event.current=pileVerificationEvents.Dequeue();Debug.Log("PILE input: "+Event.current.mousePosition+" / "+Event.current.type+" / enabled="+GUI.enabled);DrawTerrainPiles();}finally{Event.current=saved;}
#endif
        }
        IEnumerator VerifyPiles()
        {
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","PileVerification"));Directory.CreateDirectory(dir);
            var errors=new List<string>();Application.LogCallback listener=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors.Add(m);};Application.logMessageReceived+=listener;
            var source=ContentLoader.TestTable(catalog,effects,4,false,217);var view=source.ViewFor(source.Active);
            match=Match.FromView(view,catalog,effects);world.Bind(match);menu=false;handoff=false;libraryOpen=false;networkPage=false;selectedPile=0;
            yield return new WaitForSecondsRealtime(1);
            Require(world.GetComponentsInChildren<PileHit>().Length==4,"only four shared physical piles");
            Require(world.GetComponentsInChildren<Renderer>().Count(r=>r.name=="Ilustração do topo"&&r.sharedMaterial.mainTexture!=null)==4,"four illustrated tops");
            pileVerificationPointer=new Vector2(1280,567);yield return Capture("01-terrain-hover.png",.2f);
            Require(pileHover==match.Seats[0].Top(0),"terrain hover reveals full card");
            int revision=match.Revision;pileVerificationEvents.Enqueue(new Event{type=EventType.MouseDown,button=0,mousePosition=new Vector2(1450,567)});while(pileVerificationEvents.Count>0)yield return null;
            Require(selectedPile==1&&match.Revision==revision,"pile selection does not play a board action: selected="+selectedPile+" revision="+match.Revision);
            pileVerificationPointer=null;
            var pile=world.GetComponentsInChildren<PileHit>()[0];var screen=world.View.WorldToScreenPoint(pile.transform.position+Vector3.up*.4f);pileVerificationPointer=new Vector2(screen.x/scale,(Screen.height-screen.y)/scale);
            yield return Capture("02-physical-pile-hover.png",.2f);Require(pileHover==match.Seats[0].Top(pile.Pile),"physical pile hover");
            var spell=catalog.Get("test-mill");view.phase=(int)Stage.Main;view.stack=new[]{new NetPending{visualId="one",owner=0,card=NetworkCards.Pack(spell),attackers=Array.Empty<int>(),blockers=Array.Empty<int>()},new NetPending{visualId="two",owner=1,card=NetworkCards.Pack(catalog.Get("test-counter")),attackers=Array.Empty<int>(),blockers=Array.Empty<int>()}};view.revision++;match.ApplyView(view,catalog);
            pileVerificationPointer=new Vector2(1265,498);yield return Capture("03-stack-hover.png",.2f);Require(pileHover?.Id=="test-counter","stack hover");pileVerificationPointer=new Vector2(20,200);
            view.stack=Array.Empty<NetPending>();view.visuals=new[]{new NetVisual{kind="stack-canceled",stackId="one",card=NetworkCards.Pack(spell),owner=0}};view.revision++;match.ApplyView(view,catalog);
            yield return Capture("04-canceled-burn.png",.55f);Require(stackEchoes.Any(e=>e.Burn),"cancel burn displayed");yield return new WaitForSecondsRealtime(1.7f);
            view.visuals=new[]{new NetVisual{kind="stack-resolved",stackId="two",card=NetworkCards.Pack(catalog.Get("test-counter")),owner=1}};view.revision++;match.ApplyView(view,catalog);
            yield return Capture("05-resolved-glow.png",.35f);Require(stackEchoes.Any(e=>!e.Burn),"resolve glow displayed");yield return new WaitForSecondsRealtime(1.7f);
            view.visuals=Enumerable.Range(0,7).Select(c=>new NetVisual{kind="mana",from=50+c,to=Match.Capital(source.Active,4),owner=source.Active,color=c,amount=1}).ToArray();view.revision++;match.ApplyView(view,catalog);
            yield return Capture("06-mana-flights.png",.35f);Require(world.FlightCount==7,"seven mana colors animated from network events");yield return new WaitForSecondsRealtime(1.3f);Require(world.FlightCount==0,"mana effects cleaned up");
            var melee=catalog.Cards.First(c=>c.Kind==CardType.Creature&&c.Range==1&&c.Art!="mage");var archer=catalog.Cards.First(c=>c.Kind==CardType.Creature&&c.Range>1&&c.Art!="mage");var mage=catalog.Cards.First(c=>c.Kind==CardType.Creature&&c.Art=="mage");
            view.visuals=new[]{new NetVisual{kind="attack",from=58,to=59,piece=-1,card=NetworkCards.Pack(melee)},new NetVisual{kind="attack",from=60,to=62,piece=-1,card=NetworkCards.Pack(archer)},new NetVisual{kind="attack",from=69,to=73,piece=-1,card=NetworkCards.Pack(mage)}};view.revision++;match.ApplyView(view,catalog);
            yield return Capture("07-attacks.png",.25f);Require(world.FlightCount==3,"slash arrow spell animations");yield return new WaitForSecondsRealtime(1);Require(world.FlightCount==0,"attack effects cleaned up");
            pileVerificationPointer=null;File.WriteAllText(Path.Combine(dir,"result.txt"),"PASS: shared physical tops/art, HUD and physical hover, click isolation, stack hover, cancel/resolve, seven mana colors, three attack styles, effect cleanup. Runtime errors: "+errors.Count+"\n"+string.Join("\n",errors));Application.logMessageReceived-=listener;Application.Quit(errors.Count==0?0:1);
            void Require(bool ok,string name){if(!ok)throw new Exception("PILE UI: "+name);}
            IEnumerator Capture(string name,float delay){yield return new WaitForSecondsRealtime(delay);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name));yield return new WaitForSecondsRealtime(.15f);}
        }
    }
}
