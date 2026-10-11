using System;
using System.Linq;
using TCG.Table;
using UnityEngine;
namespace TCG.Foundation.Editor
{
 public static class PreconIntegrationChecks
 {
  public static void Run()
  {
   int checks=0;void Check(bool ok,string message){checks++;if(!ok)throw new Exception("PRECON INTEGRATION: "+message);}
   var catalog=ContentLoader.Load(new EffectRegistry());var library=PreconstructedDeckLoader.Load(catalog);var ready=library.Decks.Where(library.Ready).ToArray();
   foreach(int count in new[]{2,4})
   {
    double time=0;var room=new NetworkRoom(catalog,new EffectRegistry(),"precon-integration",count,false,()=>time);var sequence=new int[count];var connections=Enumerable.Range(0,count).Select(n=>(ulong)n).ToArray();var tokens=new string[count];
    RoomReply Send(int seat,RoomRequest request){request.sequence=++sequence[seat];return room.Receive(connections[seat],request);}
    for(int seat=0;seat<count;seat++){var hello=room.Receive(connections[seat],new RoomRequest{type="hello",content="precon-integration"});tokens[seat]=hello.token;Check(hello.error==""&&hello.seat==seat,"seat reservation");var blocked=library.Decks.FirstOrDefault(d=>!library.Ready(d));if(blocked!=null)Check(Send(seat,new RoomRequest{type="deck",deck=PreconstructedDeckLibrary.Create(blocked)}).error!="","host refuses unsupported precon");Check(Send(seat,new RoomRequest{type="deck",deck=PreconstructedDeckLibrary.Create(ready[seat])}).error=="","host accepts full approved list");Check(Send(seat,new RoomRequest{type="ready",ready=true}).error=="","confirmed list ready");}
    Check(Send(0,new RoomRequest{type="start"}).error=="","start full precon table");var game=room.Game;Check(game.Seats.All(s=>s.Commander!=null),"all commanders in own zones");
    int active=game.Controller,revision=game.Revision;var draw=new RoomRequest{type="action",command=new Command{player=active,revision=revision,kind=ActionKind.DrawMain,pile=0}};Check(Send(active,draw).error==""&&game.Revision==revision+1,"draw applied once");int hand=game.HandFor(active).Count;Check(room.Receive(connections[active],draw).error==""&&game.Revision==revision+1&&game.HandFor(active).Count==hand,"duplicate packet does not draw again");
    Check(Send(active,new RoomRequest{type="action",command=new Command{player=active,revision=revision,kind=ActionKind.DrawMain,pile=0}}).error!=""&&game.Revision==revision+1,"stale revision refused");
    for(int seat=0;seat<count;seat++){var view=room.Reply(seat).view;var projected=Match.FromView(view,catalog,new EffectRegistry());Check(projected.HandFor(seat).Count==game.HandFor(seat).Count&&Enumerable.Range(0,count).Where(n=>n!=seat).All(n=>projected.HandFor(n).Count==0),"private hands and own projection");Check(projected.Revision==game.Revision&&projected.Seats.Select(s=>s.Life).SequenceEqual(game.Seats.Select(s=>s.Life)),"public state agrees");}
    room.Disconnect(connections[1]);Check(room.Paused,"pause on loss");int frozen=game.Revision;time=119;room.Tick();Check(!game.Seats[1].Eliminated,"reconnect window respected");connections[1]=101;var reconnect=room.Receive(101,new RoomRequest{type="hello",content="precon-integration",token=tokens[1]});Check(reconnect.error==""&&reconnect.seat==1&&reconnect.ack==sequence[1]&&!room.Paused&&game.Revision==frozen,"reconnect retains precon state and sequence");Check(room.Receive(1,new RoomRequest{type="action",sequence=sequence[1]+1,command=new Command{player=1,revision=frozen,kind=ActionKind.Concede}}).error!="","old connection loses authority");
    for(int seat=1;seat<count;seat++)Check(Send(seat,new RoomRequest{type="action",command=new Command{player=seat,revision=game.Revision,kind=ActionKind.Concede}}).error=="","precon match finishes through valid action");Check(game.Over&&game.WinningTeam==game.Seats[0].Team,"whole match reaches victory");
   }
   Debug.Log("PRECON INTEGRATION CHECKS PASSED: "+checks);
  }
 }
}
