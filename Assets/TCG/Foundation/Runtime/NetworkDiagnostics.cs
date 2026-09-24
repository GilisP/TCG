using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
 public sealed partial class TableView
 {
  IEnumerator VerifyNetwork()
  {
   yield return null; // Netcode registers its message types after scene load.
   var args=Environment.GetCommandLineArgs();string Arg(string key,string fallback){int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:fallback;}
   bool host=Arg("-tcg-net-role","host")=="host";int count=int.Parse(Arg("-tcg-net-count","2"));ushort port=ushort.Parse(Arg("-tcg-net-port","7788"));string label=Arg("-tcg-net-label",host?"host":"guest");
   string dir=Path.Combine(Application.dataPath,"..","NetworkVerification",port.ToString());Directory.CreateDirectory(dir);var errors=new List<string>();Application.LogCallback listener=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors.Add(m);};Application.logMessageReceived+=listener;
   EnsureNetwork();networkPage=true;menu=false;var task=host?network.Host(false,false,"Diagnóstico",count,count==4,label,port):network.Join(false,"127.0.0.1",label,false,port);while(!task.IsCompleted)yield return null;if(task.IsFaulted)throw task.Exception;
   yield return new WaitForSecondsRealtime(1);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,label+"-lobby.png"));
   float deadline=Time.unscaledTime+150;bool deckSent=false,ready=false,started=false,played=false,moved=false,lost=false,resumed=false,sawPause=false;float reconnectAt=0;int privateChecks=0,movesSent=0;float nextAction=0;
   while(Time.unscaledTime<deadline){
    var state=network.State;if(state==null){yield return null;continue;}sawPause|=state.paused;
    if(!network.Connected){if(lost&&Time.unscaledTime>=reconnectAt&&!network.Busy){var reconnect=network.Reconnect();while(!reconnect.IsCompleted)yield return null;if(reconnect.IsFaulted)throw reconnect.Exception;reconnectAt=Time.unscaledTime+5;}yield return null;continue;}
    if(lost&&!resumed&&state.started&&!state.paused){resumed=true;}
    if(!state.started){
     if(!network.Busy&&!deckSent){network.Send(new RoomRequest{type="deck",deck=NetworkDeck()});deckSent=true;}
     else if(!network.Busy&&!ready){network.Send(new RoomRequest{type="ready",ready=true});ready=true;}
     else if(host&&!network.Busy&&!started&&state.members.Length==count&&state.members.All(m=>m.ready)){network.Send(new RoomRequest{type="start"});started=true;}
     yield return new WaitForSecondsRealtime(.15f);continue;
    }
    int seat=state.seat;Require(match.IsRemoteView,"filtered client view");Require(Enumerable.Range(0,count).Where(i=>i!=seat).All(i=>match.HandFor(i).Count==0),"private hands absent");privateChecks++;
    if(!host&&label=="guest1"&&!lost&&match.Revision>=7){lost=true;reconnectAt=Time.unscaledTime+3;network.SimulateConnectionLoss();yield return null;continue;}
    if(match.Revision>=26&&(host||label!="guest1"||resumed))break;
    if(!network.Busy&&!state.paused&&match.Controller==seat&&Time.unscaledTime>=nextAction){
     nextAction=Time.unscaledTime+.35f;
     if(match.Choice!=null)Submit(ActionKind.Choose,match.Choice.Options[0].Key);
     else if(match.Defense!=null)Submit(ActionKind.Defend,defense:Enumerable.Repeat(-1,match.Defense.Attackers.Length).ToArray());
     else if(match.Phase==Stage.Draw){played=false;moved=false;Submit(ActionKind.DrawMain);}
     else if(match.Phase==Stage.Terrain)Submit(ActionKind.Place,Enumerable.Range(0,121).First(match.CanPlace));
     else if(match.Stack.Count>0||match.Phase==Stage.End)Submit(ActionKind.Pass);
     else if(!played){int home=Match.Capital(seat,count);var card=match.HandFor(seat).FirstOrDefault(c=>match.CanPlay(c,home));if(card!=null){Submit(ActionKind.Play,home,card:card.Id);played=true;}else Submit(ActionKind.NextPhase);}
     else if(!moved){var piece=match.Board.SelectMany(c=>c.Pieces).FirstOrDefault(p=>p.Owner==seat&&Enumerable.Range(0,121).Any(at=>match.CanMove(p.Id,at)));if(piece!=null){if(Submit(ActionKind.Move,Enumerable.Range(0,121).First(at=>match.CanMove(piece.Id,at)),piece.Id))movesSent++;moved=true;}else Submit(ActionKind.NextPhase);}
     else Submit(ActionKind.NextPhase);
    }
    yield return null;
   }
   Require(match.IsRemoteView&&match.Revision>=26,"multi-process progression");Require(movesSent>0,"movement transmitted");Require(host||label!="guest1"||resumed,"guest reconnected");Require(!host||sawPause,"host paused for disconnect");
   yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,label+".png"));yield return new WaitForSecondsRealtime(.5f);
   File.WriteAllText(Path.Combine(dir,label+".txt"),"PASS · seats="+count+" · revision="+match.Revision+" · privacy checks="+privateChecks+" · moves="+movesSent+" · reconnect="+resumed+" · paused="+sawPause+" · runtime errors="+errors.Count+Environment.NewLine+string.Join(Environment.NewLine,errors));
   // Give every peer time to record evidence before the host leaves.
   yield return new WaitForSecondsRealtime(host?8:3);awaitStop();Application.logMessageReceived-=listener;Application.Quit(errors.Count==0?0:1);
   void Require(bool yes,string why){if(!yes)throw new Exception("NETWORK RUNTIME: "+why+" / "+network.Status);}
   async void awaitStop(){await network.Stop();}
  }
 }
}
