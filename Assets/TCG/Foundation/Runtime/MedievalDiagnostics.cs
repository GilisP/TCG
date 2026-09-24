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
        IEnumerator VerifyMedievalPresentation()
        {
            var dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","VerificationMedieval"));Directory.CreateDirectory(dir);
            var errors=new List<string>();Application.LogCallback listener=(message,trace,type)=>{if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message+"\n"+trace);};
            Application.logMessageReceived+=listener;
            yield return new WaitForSecondsRealtime(2);
            menu=false;collection=true;expansion="EXP-001-base";
            for(int page=0;page<6;page++){catalogPage=page;yield return Capture("catalogo-"+(page+1)+".png");}
            inspected=catalog.Get("MED-156");yield return Capture("ficha-leviata.png");inspected=null;collection=false;
            var main=Enumerable.Range(0,2).Select(_=>Enumerable.Range(0,40).Select(n=>n%2==0?"MED-121":"MED-123").ToArray()).ToArray();
            var lands=Enumerable.Range(0,2).Select(_=>Enumerable.Repeat("test-land-3",50).ToArray()).ToArray();
            match=new Match(catalog,effects,2,false,45,main,lands);world.Bind(match);handoff=false;
            int castAt=-1,impactAt=-1;match.Visual+=e=>{if(e.Kind=="cast")castAt=e.From;if(e.Kind=="impact")impactAt=e.To;};
            Require(Submit(ActionKind.DrawMain),"compra");int home=Match.Capital(match.Active,2);
            Require(Submit(ActionKind.Place,home),"terreno");Require(Submit(ActionKind.NextPhase),"fase");
            Require(Submit(ActionKind.Play,home,card:"MED-121"),"invocar Saqueador");
            while(match.Stack.Count>0){Require(Submit(ActionKind.Pass),"resolver criatura");handoff=false;}
            selectedCell=home;yield return new WaitForSecondsRealtime(.8f);
            Require(Submit(ActionKind.Play,card:"MED-123"),"anunciar truque");handoff=false;
            Require(castAt==home,"animação na capital de quem jogou");yield return Capture("anuncio-capital.png",.08f);
            while(match.Stack.Count>0){Require(Submit(ActionKind.Pass),"resolver truque");handoff=false;}
            Require(match.Choice!=null,"escolha de alvo");yield return Capture("escolha-alvo.png");
            Require(Submit(ActionKind.Choose,match.Choice.Options.First(o=>o.Key>=0).Key),"confirmar alvo");handoff=false;
            Require(impactAt==home,"efeito no alvo após resolver");yield return Capture("resolucao-alvo.png",.08f);
            var previews=new List<GameObject>();int n=0;
            foreach(string id in new[]{"MED-085","MED-101","MED-097","MED-130","MED-118","MED-119","MED-134","MED-090"}){
                previews.Add(world.PreviewModel(catalog.Get(id),new Vector3(-3+(n%4)*2,0,-1+(n/4)*2)));n++;
            }
            world.Yaw=15;world.Distance=13;yield return Capture("modelos-medievais.png");
            foreach(var go in previews)Destroy(go);
            double elapsed=0;int frames=0;while(elapsed<3){yield return null;elapsed+=Time.unscaledDeltaTime;frames++;}
            File.WriteAllText(Path.Combine(dir,"result.txt"),"Medieval presentation completed\nCatalog cards: "+catalog.Cards.Count(c=>c.Expansion=="EXP-001-base")+"\nCast capital: "+castAt+"\nImpact target: "+impactAt+"\nAverage FPS: "+(frames/elapsed).ToString("F1")+"\nRuntime errors: "+errors.Count+"\n"+string.Join("\n",errors));
            Application.logMessageReceived-=listener;Application.Quit(errors.Count==0?0:1);
            IEnumerator Capture(string name,float delay=.5f){yield return new WaitForSecondsRealtime(delay);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name));yield return new WaitForSecondsRealtime(.7f);}
            void Require(bool ok,string step){if(!ok)throw new InvalidOperationException("Medieval presentation failed: "+step+" / "+notice);}
        }
    }
    public sealed partial class TableWorld
    {
        public GameObject PreviewModel(Definition card,Vector3 position)
        {
            var root=new GameObject("Prévia visual · "+card.Name).transform;root.SetParent(transform);root.position=position;
            Block("Expositor",new Vector3(0,0,0),new Vector3(1.5f,.2f,1.4f),Hex("31474A"),root);
            var figure=Figure(card,Elements[card.Color],root);figure.localScale=Vector3.one*1.65f;figure.localPosition=Vector3.up*.12f;
            return root.gameObject;
        }
    }
}

