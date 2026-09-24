using System.Linq;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
 public sealed partial class TableView
 {
  bool exileOpen;Vector2 exileScroll;
  void DrawExiledCards()
  {
   if(menu||handoff)return;var cards=match.ExiledFor(Viewer);
   if(cards.Count>0&&Button(995,383,177,40,"Exílio · "+cards.Count))exileOpen=!exileOpen;
   if(!exileOpen)return;GUI.enabled=true;Fill(new Rect(0,0,1600,1000),new Color(0,0,0,.8f));
   Fill(new Rect(250,180,900,610),Panel);Text(280,206,830,40,"CARTAS QUE VOCÊ PODE CONJURAR",heading,Gold);
   exileScroll=GUI.BeginScrollView(new Rect(280,270,830,430),exileScroll,new Rect(0,0,800,Mathf.Max(410,cards.Count*70)));
   for(int n=0;n<cards.Count;n++){var x=cards[n];if(Button(0,n*70,790,58,x.Card.Name+" · custo "+x.Card.TotalCost+(x.AnyMana?" · qualquer cor":"")+(x.Card.Rule=="destiny"?" · ao responder a uma morte":""),match.CanCastExiled(x.Id))){Submit(ActionKind.CastExiled,target:x.Id);exileOpen=false;}}
   GUI.EndScrollView();if(Button(280,724,830,40,"Fechar"))exileOpen=false;
  }
 }
}
