using System;
using UnityEngine;
namespace TCG.Table
{
    public sealed partial class TableView
    {
        int handPage,hoveredHand=-1,handOwner=-1;
        const int CardsPerFan=9;
        Rect FanRect(int index,int count)
        {
            float step=Mathf.Min(120,900f/Mathf.Max(1,count-1)),offset=index-(count-1)*.5f;
            return new Rect(590+offset*step-60,761+Mathf.Abs(offset),120,180);
        }
        float FanAngle(int index,int count)=>(index-(count-1)*.5f)*3;
        Rect LiftedCard(int index,int count){var r=FanRect(index,count);return new Rect(Mathf.Clamp(r.center.x-104,20,960),634,208,312);}
        bool FanContains(Vector2 point,int index,int count)
        {
            var r=FanRect(index,count);var pivot=new Vector2(r.center.x,r.yMax);var p=(Vector2)(Quaternion.Euler(0,0,-FanAngle(index,count))*(point-pivot))+pivot;return r.Contains(p);
        }
        void DrawHand()
        {
            Fill(new Rect(0,728,1190,237),Dark);
            Text(24,732,820,27,"MÃO DE "+match.Seats[Viewer].Name.ToUpperInvariant()+" · clique para jogar · botão direito: ficha",small,Gold);
            if(handoff){hoveredHand=-1;return;}
            var hand=match.HandFor(Viewer);
            if(handOwner!=Viewer){handPage=0;hoveredHand=-1;handOwner=Viewer;}
            int pages=Math.Max(1,(hand.Count+CardsPerFan-1)/CardsPerFan);handPage=Mathf.Clamp(handPage,0,pages-1);
            if(pages>1){if(Button(900,732,44,28,"‹")){handPage=Math.Max(0,handPage-1);hoveredHand=-1;} Text(949,732,155,28,(handPage+1)+" / "+pages+" · "+hand.Count+" cartas",small,Ink);if(Button(1120,732,44,28,"›")){handPage=Math.Min(pages-1,handPage+1);hoveredHand=-1;}}
            int start=handPage*CardsPerFan,count=Math.Min(CardsPerFan,hand.Count-start);var e=Event.current;var mouse=presentationPointer??e.mousePosition;
            if(hoveredHand>=count)hoveredHand=-1;
            int hit=-1;
            if(GUI.enabled)
            {
                // Resolve the foremost exposed card first; lifted area retains hover while reading.
                if(hoveredHand>=0&&LiftedCard(hoveredHand,count).Contains(mouse))hit=hoveredHand;
                else for(int i=count-1;i>=0;i--)if(FanContains(mouse,i,count)){hit=i;break;}
            }
            hoveredHand=hit;
            var matrix=GUI.matrix;
            for(int i=0;i<count;i++)
            {
                if(i==hoveredHand)continue;var r=FanRect(i,count);GUIUtility.RotateAroundPivot(FanAngle(i,count),new Vector2(r.center.x,r.yMax));
                Fill(new Rect(r.x+5,r.y+6,r.width,r.height),new Color(0,0,0,.4f));DrawCard(r,hand[start+i],hand[start+i].Id==selectedCard,false);GUI.matrix=matrix;
            }
            if(hoveredHand>=0)
            {
                var r=LiftedCard(hoveredHand,count);Fill(new Rect(r.x-5,r.y-5,r.width+10,r.height+10),Gold);DrawCard(r,hand[start+hoveredHand],true,false);
                if(e.type==EventType.MouseDown&&(e.button==0||e.button==1))
                {
                    var card=hand[start+hoveredHand];if(e.button==1){inspected=card;detailScroll=Vector2.zero;}
                    else {selectedCommander=false;selectedCard=card.Id;selectedUnit=-1;if(!match.NeedsTarget(card))Submit(TCG.Foundation.ActionKind.Play,card:selectedCard);else notice="Escolha o terreno ou alvo para "+card.Name+".";}
                    ResetDrag();e.Use();
                }
                // A lifted card overlaps the board: never let its mouse event reach a tile.
                else if(e.isMouse)e.Use();
            }
        }
    }
}

