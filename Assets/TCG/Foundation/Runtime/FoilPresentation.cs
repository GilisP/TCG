using System;
using System.Linq;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
 public sealed partial class TableWorld
 {
  public Func<int,Definition,bool> FoilAppearance;
  public int FoilBurstCount {get;private set;}
  public int ArcaneBurstCount=>GetComponentsInChildren<ArcaneBurst>().Length;
  void FoilEntry(MatchEvent e)
  {
   if(e.Kind!="summon"&&e.Kind!="terrain"&&e.Kind!="impact"&&e.Kind!="resolve")return;
   var piece=e.Piece>=0?match?.Find(e.Piece):null;var card=e.Card??piece?.Card??(e.Kind=="terrain"?match?.Board[e.To].Terrain:null);
   int owner=e.Owner>=0?e.Owner:piece!=null?piece.Owner:e.Kind=="terrain"?match.Board[e.To].TerrainOwner:-1;
   if(card==null||owner<0||!(FoilAppearance?.Invoke(owner,card)??card.Foil))return;
   if((e.Kind=="impact"||e.Kind=="resolve")&&card.Permanent)return;
   Burst(e.To,card,true,false);FoilBurstCount++;
  }
  void Burst(int at,Definition card,bool foil,bool announcement)
  {
   if(at<0||at>=121)return;
   var active=GetComponentsInChildren<ArcaneBurst>();if(active.Length>=32){active[0].gameObject.SetActive(false);Destroy(active[0].gameObject);}
   var go=new GameObject(foil?"Entrada foil · "+card.Name:"Magia · "+card.Name);go.transform.SetParent(transform);go.transform.position=Position(at)+Vector3.up*.13f;
   go.AddComponent<ArcaneBurst>().Initialize(foil?Hex("FFE6A0"):Elements[card.Color],card.Color,foil,announcement);
  }
  public void PreviewBurst(Definition card,int at,bool foil)=>Burst(at,card,foil,false);
  void ClearBursts(){foreach(var v in GetComponentsInChildren<ArcaneBurst>())Destroy(v.gameObject);}
 }
}
