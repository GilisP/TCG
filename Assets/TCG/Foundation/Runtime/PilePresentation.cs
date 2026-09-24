using System.Collections.Generic;
using System.Linq;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
    public sealed partial class TableView
    {
        Definition pileHover; string pileHoverCaption; int stackPage;
        Match visualMatch;
        readonly List<StackEcho> stackEchoes=new List<StackEcho>();
        sealed class StackEcho { public Definition Card; public bool Burn; public float Born; }
        void BeginPilePresentation()
        {
            pileHover=null;pileFramePointer=Event.current.mousePosition;
            if(visualMatch==match)return;
            if(visualMatch!=null)visualMatch.Visual-=RememberStackResult;
            visualMatch=match;stackEchoes.Clear();world.TerrainIllustration=c=>CardIllustration(c,"standard");
            if(match!=null)match.Visual+=RememberStackResult;
        }
        void RememberStackResult(MatchEvent e)
        {
            if(e.Card==null||(e.Kind!="stack-canceled"&&e.Kind!="stack-resolved"))return;
            if(stackEchoes.Count>=12)stackEchoes.RemoveAt(0);
            stackEchoes.Add(new StackEcho{Card=e.Card,Burn=e.Kind=="stack-canceled",Born=Time.unscaledTime});
        }
        void HoverPileCard(Rect rect,Definition card,string caption)
        {
            if(card==null||!rect.Contains(PilePointer))return;
            pileHover=card;pileHoverCaption=caption;
        }
        void DrawTerrainPiles()
        {
            // Handle input before drawing card faces, whose nested GUI matrices transform events.
            var input=Event.current;
            if(GUI.enabled&&input.type==EventType.MouseDown&&input.button==0)
            {
                var point=input.mousePosition;
                for(int p=0;p<4;p++)if(new Rect(1220+(p%2)*180,477+(p/2)*201,120,180).Contains(point))
                {selectedPile=p;selectedCard=null;input.Use();break;}
            }
            Text(1215,427,355,28,"TERRENOS COMPARTILHADOS",cardName,Gold);
            var seat=match.Seats[match.Active];
            for(int p=0;p<4;p++)
            {
                var r=new Rect(1220+(p%2)*180,477+(p/2)*201,120,180);
                var card=seat.Top(p);
                Text(r.x,r.y-23,165,23,"P"+(p+1)+" · "+seat.PileCount(p)+" carta(s)",small,p==selectedPile?Gold:Muted);
                if(card!=null)RenderCardFace(r,card,"standard",p==selectedPile);
                else {Fill(r,Dark);Text(r.x+8,r.y+45,r.width-16,90,"Pilha vazia\nSelecione para receber a compra",small,Muted);}
                HoverPileCard(r,card,"Pilha "+(p+1)+" · dono: "+(seat.TopOwner(p)<0?"—":match.Seats[seat.TopOwner(p)].Name)+"\nReposição: deck de "+seat.Name);
            }
        }
        void DrawStackCards()
        {
            Text(1215,375,345,28,"PILHA · "+match.Stack.Count+" ação(ões)",cardName,Gold);
            stackPage=Mathf.Clamp(stackPage,0,(match.Stack.Count-1)/4);
            for(int i=0;i<Mathf.Min(4,match.Stack.Count-stackPage*4);i++)
            {
                var pending=match.Stack[match.Stack.Count-1-i-stackPage*4];
                var r=new Rect(1225+(i%2)*175,432+(i/2)*149,88,132);
                Text(r.x,r.y-23,165,23,(i+stackPage*4==0?"TOPO":(i+stackPage*4+1)+"º")+" · "+match.Seats[pending.Owner].Name,small,i==0?Gold:Muted);
                if(pending.Card!=null){RenderCardFace(r,pending.Card,"standard",i==0);HoverPileCard(r,pending.Card,"Pilha de ações · "+match.Seats[pending.Owner].Name);}
                else{Fill(r,Dark);Text(r.x+5,r.y+20,r.width-10,95,pending.Description,small,Gold);}
            }
            if(match.Stack.Count>4){if(Button(1215,733,165,32,"↑ anteriores",stackPage>0))stackPage--;if(Button(1390,733,165,32,"próximas ↓",stackPage<(match.Stack.Count-1)/4))stackPage++;}
            Text(1215,772,350,27,"Passe para resolver ou use uma resposta.",small,Muted);
            int response=0;foreach(var monk in match.Board.SelectMany(c=>c.Pieces).Where(p=>match.CanActivate(p.Id)).Take(2)){if(Button(1215,806+response*43,350,38,"Responder · "+monk.Card.Name))Submit(ActionKind.Activate,unit:monk.Id);response++;}
        }
        void DrawPileOverlay(bool modal)
        {
            if(modal)return;
            if(pileHover==null&&BoardArea.Contains(PilePointer)&&!pointerHeld)
            {
                var pt=PilePointer;
                if(Physics.Raycast(world.View.ScreenPointToRay(new Vector3(pt.x*scale,Screen.height-pt.y*scale,0)),out var hit,70)&&hit.collider.TryGetComponent<PileHit>(out var pile))
                {var seat=match.Seats[match.Active];pileHover=seat.Top(pile.Pile);pileHoverCaption="Terreno compartilhado · pilha "+(pile.Pile+1);}
            }
            if(pileHover!=null)
            {
                // Fixed inspection area never covers the hovered pile or intercepts a board click.
                var r=new Rect(887,232,260,390);RenderCardFace(r,pileHover,"standard",true);
                Fill(new Rect(875,626,285,71),Dark);Text(887,634,262,58,pileHoverCaption,small,Gold);
            }
            stackEchoes.RemoveAll(x=>Time.unscaledTime-x.Born>1.65f);
            for(int i=0;i<stackEchoes.Count;i++)
            {
                var echo=stackEchoes[i];float t=(Time.unscaledTime-echo.Born)/1.65f;
                var r=new Rect(690+(i%3)*50,310+(i%3)*24,150,225);
                var old=GUI.color;GUI.color=new Color(1,1,1,Mathf.Clamp01((1-t)*3));
                RenderCardFace(r,echo.Card,"standard",!echo.Burn);
                if(echo.Burn)
                {
                    float edge=r.yMax-r.height*t;
                    Fill(new Rect(r.x,edge,r.width,r.yMax-edge),new Color(.08f,.025f,.01f,.97f));
                    for(int n=0;n<15;n++)
                    {float flame=12+9*Mathf.Sin(n*2.3f+Time.unscaledTime*13);Fill(new Rect(r.x+n*10,edge-flame,10,flame),new Color(1,.28f+.3f*(n%2),.05f,.8f));}
                }
                else
                {
                    float a=Mathf.Sin(t*Mathf.PI)*.32f;Fill(r,new Color(1,.85f,.4f,a));
                    OrnateBorder(new Rect(r.x-5,r.y-5,r.width+10,r.height+10),new Color(1,.91f,.55f,1-t));
                }
                Text(r.x-20,r.yMax+8,r.width+80,30,echo.Burn?"ANULADA":"RESOLVEU",cardName,echo.Burn?new Color(1,.55f,.22f):Gold);
                GUI.color=old;
            }
        }
    }
}
