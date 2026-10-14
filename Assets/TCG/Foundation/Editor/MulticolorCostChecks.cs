using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using TCG.Table;
namespace TCG.Foundation.Editor
{
 public static class MulticolorCostChecks
 {
  public static void Run()
  {
   var effects=new EffectRegistry();var catalog=ContentLoader.Load(effects);var table=ContentLoader.TestTable(catalog,effects,2,false,14);int owner=table.Active,checks=0;
   var mana=(int[])typeof(Seat).GetField("mana",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(table.Seats[owner]);
   var can=typeof(Match).GetMethod("CanPay",BindingFlags.NonPublic|BindingFlags.Instance);var pay=typeof(Match).GetMethod("Pay",BindingFlags.NonPublic|BindingFlags.Instance);
   void Check(bool ok,string why){checks++;if(!ok)throw new Exception("MULTICOLOR: "+why);}
   var cards=catalog.Cards.Where(c=>c.ColoredCost.Count(x=>x>0)>1).ToArray();Check(cards.Length>0,"multicolor catalog");
   foreach(var card in cards)
   {
    void Fill(){Array.Clear(mana,0,7);for(int i=0;i<7;i++)mana[i]=card.ColoredCost[i];mana[6]+=card.Cost;}
    Fill();Check((bool)can.Invoke(table,new object[]{owner,card}),"exact cost available "+card.Id);
    foreach(int color in Enumerable.Range(0,7).Where(i=>card.ColoredCost[i]>0))
    {Fill();mana[color]--;mana[(color+1)%7]+=50;Check(!(bool)can.Invoke(table,new object[]{owner,card}),"wrong color excess cannot replace "+color+" "+card.Id);}
    Fill();pay.Invoke(table,new object[]{owner,card});Check(mana.All(x=>x==0),"all colored/generic mana spent "+card.Id);
    Check(TableView.PrintedManaCost(card).Contains(" + "),"front prints complete cost "+card.Id);
    var roundtrip=NetworkCards.Pack(card);Check(roundtrip.coloredCost.SequenceEqual(card.ColoredCost)&&roundtrip.cost==card.Cost,"network preserves cost "+card.Id);
   }
   Debug.Log("MULTICOLOR COST CHECKS PASSED: "+checks+"; cards="+cards.Length);
  }
 }
}
