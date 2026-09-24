using System;
using System.Linq;
using UnityEngine;
using TCG.Table;
namespace TCG.Foundation.Editor
{
 public static class NetworkChecks
 {
  static int checks;
  public static void Run()
  {
   checks=0;var effects=new EffectRegistry();var catalog=ContentLoader.Load(effects);double time=0;
   foreach(int count in new[]{2,4}){
    var room=new NetworkRoom(catalog,effects,"test",count,count==4,()=>time);var seq=new int[count];var tokens=new string[count];
    for(int i=0;i<count;i++){var reply=room.Receive((ulong)i,new RoomRequest{type="hello",content="test",name="Seat "+i});Ok(reply.seat==i&&reply.token.Length>=40,"assigned seat and secret");tokens[i]=reply.token;Send(i,new RoomRequest{type="deck",deck=Deck(i)});Send(i,new RoomRequest{type="ready",ready=true});}
    Ok(room.Receive(90,new RoomRequest{type="hello",content="test"}).error.Length>0,"capacity enforced");
    Send(0,new RoomRequest{type="start"});var m=room.Game;Ok(m!=null,"match started");Ok(count!=4||m.Seats[0].Team==m.Seats[2].Team&&m.Seats[1].Team==m.Seats[3].Team,"teams preserved");
    for(int i=0;i<count;i++){
     var view=room.Reply(i).view;var wire=JsonUtility.ToJson(view);var copy=Match.FromView(JsonUtility.FromJson<MatchView>(wire),catalog,effects);
     Ok(copy.HandFor(i).Count==5,"own hand present");Ok(Enumerable.Range(0,count).Where(x=>x!=i).All(x=>copy.HandFor(x).Count==0),"other hands absent");Ok(copy.Seats.All(s=>s.MainCount==43&&s.HandCount==5),"private counts preserved");Ok(!wire.Contains("\"main\":[")&&!wire.Contains("seed"),"no deck order or seed");Ok(!copy.Try(new Command{player=i,revision=0,kind=ActionKind.Concede},out _),"projection cannot execute rules");
    }
    int active=m.Controller;int revision=m.Revision;
    var wrong=room.Receive((ulong)((active+1)%count),new RoomRequest{type="action",sequence=++seq[(active+1)%count],command=new Command{player=active,revision=revision,kind=ActionKind.DrawMain,pile=0}});Ok(wrong.error.Length>0&&m.Revision==revision,"forged seat rejected");
    var cmd=new RoomRequest{type="action",command=new Command{player=active,revision=revision,kind=ActionKind.DrawMain,pile=0}};Send(active,cmd);Ok(m.Phase==Stage.Terrain,"draw synchronized");var duplicate=room.Receive((ulong)active,cmd);Ok(duplicate.error.Length==0&&m.Revision==revision+1,"duplicate command does not draw twice");
    var stale=room.Receive((ulong)active,new RoomRequest{type="action",sequence=++seq[active],command=new Command{player=active,revision=revision,kind=ActionKind.DrawMain,pile=0}});Ok(stale.error.Length>0&&m.Revision==revision+1,"stale revision rejected");
    int tile=Enumerable.Range(0,121).First(m.CanPlace);var projection=Match.FromView(m.ViewFor(active),catalog,effects);Ok(projection.CanPlace(tile),"legal placement replicated");Send(active,new RoomRequest{type="action",command=new Command{player=active,revision=m.Revision,kind=ActionKind.Place,pile=0,target=tile}});Ok(m.Phase==Stage.Main,"terrain auto phase synchronized");
    int enemy=(active+1)%count;foreach(int at in new[]{56,57,58}){m.Board[at].Terrain=catalog.Get("test-land-0");m.Board[at].Owner=at==58?enemy:active;}
    var attacker=new Piece(900,active,catalog.Get("MED-085")){Game=m};var defender=new Piece(901,enemy,catalog.Get("MED-085")){Game=m,Damage=2};m.Board[56].pieces.Add(attacker);m.Board[58].pieces.Add(defender);
    var battleView=Match.FromView(m.ViewFor(active),catalog,effects);Ok(battleView.CanMove(900,57),"movement permission replicated");Send(active,new RoomRequest{type="action",command=new Command{player=active,revision=m.Revision,kind=ActionKind.Move,unit=900,target=57}});Ok(m.Position(900)==57,"movement accepted by host");
    battleView=Match.FromView(m.ViewFor(active),catalog,effects);Ok(battleView.CanAttack(900,58),"attack permission replicated");Send(active,new RoomRequest{type="action",command=new Command{player=active,revision=m.Revision,kind=ActionKind.Attack,units=new[]{900},target=58}});
    for(int n=0;n<12&&(m.Stack.Count>0||m.Choice!=null);n++){int who=m.Controller;Send(who,new RoomRequest{type="action",command=new Command{player=who,revision=m.Revision,kind=m.Choice!=null?ActionKind.Choose:m.Defense!=null?ActionKind.Defend:ActionKind.Pass,target=m.Choice?.Options[0].Key??-1,blockers=new[]{901}}});}
    Ok(m.Find(901)==null,"combat and defense resolved authoritatively");
    room.Disconnect(1);Ok(room.Paused,"disconnect pauses");Ok(room.Receive(99,new RoomRequest{type="hello",token="wrong",content="test"}).error.Length>0,"reconnection secret checked");time+=60;var resumed=room.Receive(101,new RoomRequest{type="hello",token=tokens[1],content="test"});Ok(resumed.seat==1&&!room.Paused&&resumed.view.revision==m.Revision,"same seat and fresh state restored");
    typeof(Match).GetMethod("Ask",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(m,new object[]{1,"Private choice",new[]{new ChoiceOption(8,"PRIVATE-NETWORK-TARGET")},(Action<int>)(_=>{}),false});
    typeof(Match).GetMethod("Drain",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(m,null);
    Ok(m.Choice?.Owner==1,"private choice fixture");Ok(!JsonUtility.ToJson(m.ViewFor(0)).Contains("PRIVATE-NETWORK-TARGET")&&JsonUtility.ToJson(m.ViewFor(1)).Contains("PRIVATE-NETWORK-TARGET"),"private choice only reaches its owner");
    room.Disconnect(101);time+=121;room.Tick();Ok(m.Seats[1].Eliminated,"timeout forfeits after two minutes");Ok(m.Choice==null||m.Choice.Owner!=1,"abandoned choice cannot lock the match");Ok(room.Receive(102,new RoomRequest{type="hello",token=tokens[1],content="test"}).error.Length>0,"expired reconnect rejected");
    room.Disconnect(0);Ok(room.Closed,"host disconnect closes room");
    void Send(int seat,RoomRequest r){r.sequence=++seq[seat];var reply=room.Receive((ulong)seat,r);Ok(string.IsNullOrEmpty(reply.error),r.type+": "+reply.error);}
   }
   var mismatch=new NetworkRoom(catalog,effects,"A",2,false,()=>time);Ok(mismatch.Receive(0,new RoomRequest{type="hello",content="B"}).error.Length>0,"catalog mismatch rejected");
   Debug.Log("NETWORK CHECKS PASSED: "+checks);
  }
  static DeckData Deck(int i)=>new DeckData{experimental=true,main=Enumerable.Repeat(i%2==0?"MED-085":"MED-086",48).ToList(),terrains=Enumerable.Repeat("test-land-0",50).ToList()};
  static void Ok(bool value,string message){if(!value)throw new Exception("NETWORK CHECK: "+message);checks++;}
 }
}
