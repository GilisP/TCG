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
        IEnumerator VerifyReactionsAndPlacement()
        {
            var directory=Path.GetFullPath(Path.Combine(Application.dataPath,"..","ReactionVerification"));Directory.CreateDirectory(directory);
            var errors=new List<string>();Application.LogCallback listener=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors.Add(m);};Application.logMessageReceived+=listener;
            var mains=Enumerable.Range(0,2).Select(_=>Enumerable.Repeat("MED-085",100).ToArray()).ToArray();var lands=Enumerable.Range(0,2).Select(_=>Enumerable.Repeat("test-land-0",50).ToArray()).ToArray();
            match=new Match(catalog,effects,2,false,5,mains,lands){AutomaticResponses=true,AutoAdvanceAfterTerrain=true};world.Bind(match);menu=false;handoff=false;world.Distance=23;world.Yaw=0;reactions=new ReactionChannel();
            Require(Submit(ActionKind.DrawMain),"compra");int home=Match.Capital(match.Active,2);Require(Submit(ActionKind.Place,home),"terreno");
            yield return new WaitForSecondsRealtime(1);
            selectedCard="MED-085";
            yield return Click(Point(TableWorld.Position(home)));
            Require(match.Board[home].Pieces.Count==1,"carta selecionada entra por clique completo no terreno");
            selectedCard="MED-085";
            yield return Click(Point(TableWorld.Position(home)+Vector3.up*.5f));
            Require(match.Board[home].Pieces.Count==2,"segunda carta entra mesmo clicando em miniatura do terreno");
            yield return Capture("01-cartas-colocadas.png");
            int other=1-match.Controller;
            yield return Click(Point(world.AvatarPosition(other)));
            Require(reactionMenu==-1,"avatar adversário não abre menu próprio");
            yield return Click(Point(world.AvatarPosition(match.Controller)),true);
            Require(reactionMenu==match.Controller,"clicar no avatar abre reações");
            yield return Capture("02-menu-reacoes.png");
            reactionTargetPicker=ReactionKind.ThumbsUp;yield return Capture("03-escolher-cor.png");
            int revision=match.Revision;Require(SendReaction(ReactionKind.ThumbsUp,other),"joia direcionado");
            Require(reactions.Visible(Time.unscaledTimeAsDouble).Single().Target==other&&match.Revision==revision,"alvo certo, sem alterar turno");
            yield return Capture("04-balao-direcionado.png");
            yield return new WaitForSecondsRealtime(1.1f);reactionMenu=match.Controller;Require(SendReaction(ReactionKind.Celebrate,-1),"comemoração pública");yield return Capture("05-comemoracao.png");
            yield return new WaitForSecondsRealtime(5.1f);Require(!reactions.Visible(Time.unscaledTimeAsDouble).Any(),"balão desaparece");
            File.WriteAllText(Path.Combine(directory,"result.txt"),"Avatar click and card-placement regression passed\nFive reactions, target selection and expiry passed\nRuntime errors: "+errors.Count+"\n"+string.Join("\n",errors));Application.logMessageReceived-=listener;Application.Quit(errors.Count==0?0:1);
            Vector2 Point(Vector3 at){var p=world.View.WorldToScreenPoint(at);return new Vector2(p.x/scale,(Screen.height-p.y)/scale);}
            IEnumerator Click(Vector2 at,bool downOnly=false)
            {
                dragVerificationEvents.Enqueue(new Event{type=EventType.MouseDown,button=0,mousePosition=at});
                if(!downOnly)dragVerificationEvents.Enqueue(new Event{type=EventType.MouseUp,button=0,mousePosition=at});
                while(dragVerificationEvents.Count>0)yield return null;yield return new WaitForSecondsRealtime(.5f);
            }
            IEnumerator Capture(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,name));yield return new WaitForSecondsRealtime(.8f);}
            void Require(bool ok,string label){if(!ok)throw new Exception("REACTION PRESENTATION: "+label+" / "+notice);}
        }
    }
}
