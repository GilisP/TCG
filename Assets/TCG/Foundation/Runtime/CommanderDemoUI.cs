using System.Linq;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
 public sealed partial class TableView
 {
  static string KeywordLabel(string key)=>key=="flying"?"Voar":key=="pass-units"?"Atravessar tropas":key=="immune-all"?"Imunidade":key=="immune-enemy"?"Imunidade a inimigos":key.Replace("-"," ");
  bool commanderDemo;
  readonly string[] demoCommanders={"MED-225","MED-226","MED-227","MED-228"};
  void DrawCommanderDemoSetup()
  {
   Fill(new Rect(45,280,990,645),Panel);Text(75,310,900,45,"EXPERIMENTAR COMANDANTES",heading,Gold);
   Text(75,369,900,70,"Escolha um comandante por jogador. Cada um recebe um deck de teste com cartas e terrenos das suas cores.",body,Muted);
   var choices=catalog.Cards.Where(c=>c.IsCommander&&c.Playable).OrderBy(c=>c.Id).ToArray();
   for(int seat=0;seat<players;seat++)
   {
    float x=75+(seat%2)*465,y=465+(seat/2)*145;var c=catalog.Get(demoCommanders[seat]);
    Text(x,y,435,27,new[]{"Âmbar","Jade","Safira","Rubi"}[seat]+" · "+string.Join(" / ",c.IdentityColors.Select(n=>new[]{"Sol","Lua","Água","Fogo","Ar","Terra"}[n])),cardName,TableWorld.Seats[seat]);
    if(Button(x,y+37,365,61,c.Name)){int index=System.Array.FindIndex(choices,item=>item.Id==c.Id);demoCommanders[seat]=choices[(index+1)%choices.Length].Id;}
    if(Button(x+375,y+37,60,61,"Ver"))inspected=c;
   }
   Text(75,807,900,90,"Decks experimentais com repetições. A coleção, as moedas e os decks salvos permanecem intactos. Os custos e as regras normais da partida continuam valendo.",small,Muted);
  }
 }
}

