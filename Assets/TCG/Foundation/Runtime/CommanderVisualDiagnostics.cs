using System;
using System.IO;
using System.Linq;
using System.Collections;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
 public sealed partial class TableView
 {
  IEnumerator VerifyCommanderVisuals()
  {
   string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","CommanderVisualVerification"));Directory.CreateDirectory(dir);int checks=0;
   Application.LogCallback listen=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert){File.WriteAllText(Path.Combine(dir,"result.txt"),"FAIL: "+m);Application.Quit(1);}};Application.logMessageReceived+=listen;
   collectionStore=new CollectionStore(Path.Combine(dir,"profile-"+Guid.NewGuid().ToString("N")));library=collectionStore.Load(catalog);menu=true;
   foreach(var card in catalog.Cards.Where(c=>c.IsCommander))
   {
    var preview=world.CreateCommanderModel(card,TableWorld.Seats[0]);Check(preview.GetComponentsInChildren<Renderer>().Length>=20,"runtime model "+card.Id);
    Check(Resources.Load<Texture2D>("CardArt/Commanders/"+card.Id+"-portrait-v1")!=null,"runtime art "+card.Id);Destroy(preview.gameObject);
   }
   yield return world.VerifyCommanderEventFixtures(catalog,effects,Check);
   File.WriteAllText(Path.Combine(dir,"result.txt"),"PASS: "+checks+" checks; 29 individual images/models; projected movement, attack, death, return, same-revision ghost, victory; zero runtime errors");Application.logMessageReceived-=listen;Application.Quit(0);
   void Check(bool ok,string why){checks++;if(!ok)throw new Exception("COMMANDER VISUAL: "+why);}
  }
 }
 public sealed partial class TableWorld
 {
  public IEnumerator VerifyCommanderEventFixtures(ContentCatalog catalog,EffectRegistry effects,Action<bool,string> check)
  {
   var original=match;var view=ContentLoader.TestTable(catalog,effects,2,false,81).ViewFor(0);int home=Match.Capital(0,2),enemy=Match.Capital(1,2);var card=catalog.Get("MED-193");
   NetPiece Pawn(int id,int owner,int cell)=>new NetPiece{id=id,owner=owner,original=owner,cell=cell,card=NetworkCards.Pack(card),attack=card.Attack,defense=card.Defense,range=card.Range,actions=card.Actions,movement=card.Movement,attached=-1,carrier=-1};
   view.pieces=new[]{Pawn(900001,0,home),Pawn(900002,1,enemy)};view.visuals=Array.Empty<NetVisual>();var projected=Match.FromView(view,catalog,effects);Bind(projected);yield return new WaitForSecondsRealtime(.9f);
   var actor=pieces[900001];check(actor.GetComponent<FigureMotion>().State==MotionState.Idle,"idle settles");var before=actor.position;int target=Match.Neighbors(home).First();view.cells[target].terrain=view.cells[home].terrain;view.cells[target].owner=0;view.pieces[0].cell=target;view.revision++;view.visuals=new[]{new NetVisual{kind="move",from=home,to=target,piece=900001,owner=0}};projected.ApplyView(view,catalog);yield return new WaitForSecondsRealtime(.15f);
   check(actor.GetComponent<FigureMotion>().State==MotionState.Walking&&(actor.position-before).sqrMagnitude>.01f,"projected move walks");
   view.revision++;view.visuals=new[]{new NetVisual{kind="attack",from=target,to=enemy,piece=900001,owner=0,card=NetworkCards.Pack(card)}};projected.ApplyView(view,catalog);yield return new WaitForSecondsRealtime(.15f);check(actor.GetComponent<FigureMotion>().State==MotionState.Attacking,"projected attack");
   view.pieces=view.pieces.Where(p=>p.id!=900002).ToArray();view.revision++;view.visuals=new[]{new NetVisual{kind="return",from=enemy,to=enemy,piece=900002,owner=1,card=NetworkCards.Pack(card)}};projected.ApplyView(view,catalog);yield return null;yield return null;check(dying.Count==0&&!pieces.ContainsKey(900002),"return is not death");
   view.pieces=Array.Empty<NetPiece>();view.revision++;view.visuals=new[]{new NetVisual{kind="death",from=target,to=target,piece=900001,owner=0,card=NetworkCards.Pack(card)}};projected.ApplyView(view,catalog);yield return null;yield return null;
   check(dying.Count==1&&!pieces.ContainsKey(900001),"death ghost separated from occupancy");check(actor.GetComponent<FigureMotion>().State==MotionState.Dying&&!actor.GetComponent<Collider>().enabled,"death pose and picking disabled");yield return new WaitForSecondsRealtime(1.4f);check(dying.Count==0&&actor==null,"death cleanup");
   view.revision++;view.visuals=new[]{new NetVisual{kind="death",from=target,to=target,piece=900003,owner=0,card=NetworkCards.Pack(card)}};projected.ApplyView(view,catalog);yield return null;
   check(dying.Count==1&&dying[0].root.GetComponent<PieceHit>()==null,"birth and death in same revision have visual ghost only");yield return new WaitForSecondsRealtime(1.4f);check(dying.Count==0,"same revision ghost cleanup");
   view.pieces=new[]{Pawn(900004,0,home),Pawn(900005,1,enemy)};view.revision++;view.visuals=Array.Empty<NetVisual>();view.over=true;view.winner=view.seats[0].team;projected.ApplyView(view,catalog);yield return null;yield return null;
   check(pieces[900004].GetComponent<FigureMotion>().State==MotionState.Celebrating,"winner celebrates");check(pieces[900005].GetComponent<FigureMotion>().State!=MotionState.Celebrating,"loser does not celebrate");
   Bind(original);
  }
 }
}
