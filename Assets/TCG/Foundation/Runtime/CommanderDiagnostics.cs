using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
 public sealed partial class TableView
 {
  IEnumerator VerifyCommanders()
  {
   string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","CommanderVerification"));Directory.CreateDirectory(dir);var errors=new List<string>();Application.LogCallback listener=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors.Add(m);};Application.logMessageReceived+=listener;
   collectionStore=new CollectionStore(Path.Combine(dir,"profile-"+Guid.NewGuid().ToString("N")));library=collectionStore.Load(catalog);int coins=library.Data.coins;players=4;useSavedDecks=false;commanderDemo=true;demoCommanders[0]="MED-229";demoCommanders[1]="MED-230";demoCommanders[2]="MED-231";demoCommanders[3]="MED-225";NavigateHub(HubPage.Play);yield return Capture("01-escolher-comandantes.png");StartMatch();handoff=false;Require(match.Seats[2].Commander.Id=="MED-231","Valeria selectable");yield return Capture("02-zona-comandante.png");
   var main=Enumerable.Range(0,48).Select(i=>i%2==0?"test-vehicle-cart":"test-vehicle-scout").ToArray();var lands=Enumerable.Range(0,50).Select(i=>"test-land-"+(i%2==0?2:4)).ToArray();match=new Match(catalog,effects,2,false,5,new[]{main,main},new[]{lands,lands},new[]{"MED-231","MED-231"});match.AutomaticResponses=true;match.AutoAdvanceAfterTerrain=true;world.Bind(match);int owner=match.Active,home=Match.Capital(owner,2);Piece cart=null,scout=null;
   for(int guard=0;guard<30;guard++)
   {
    if(match.Phase==Stage.Draw)Do(ActionKind.DrawMain);
    if(match.Phase==Stage.Terrain){int tile=Enumerable.Range(0,121).First(n=>match.Board[n].Terrain==null&&match.CanPlace(n));Do(ActionKind.Place,tile);}
    if(match.Active==owner)
    {
     if(match.CanSummonCommander(home))Do(ActionKind.SummonCommander,home);
     foreach(string id in new[]{"test-vehicle-cart","test-vehicle-scout"})if(!match.Board.SelectMany(c=>c.Pieces).Any(p=>p.Owner==owner&&p.Card.Id==id)){var card=match.HandFor(owner).FirstOrDefault(c=>c.Id==id);if(card!=null&&match.CanPlay(card,home))Do(ActionKind.Play,home,card:id);}
     cart=match.Board[home].Pieces.FirstOrDefault(p=>p.Card.Id=="test-vehicle-cart");scout=match.Board[home].Pieces.FirstOrDefault(p=>p.Card.Id=="test-vehicle-scout");if(cart!=null&&scout!=null&&match.Board[home].Pieces.Any(p=>p.Card.Id=="MED-231"))break;
    }
    Do(ActionKind.NextPhase);
   }
   Require(cart!=null&&scout!=null,"normal mana/deck commands summon fixture");selectedCell=home;selectedUnit=scout.Id;handoff=false;yield return Capture("03-embarcar.png");Do(ActionKind.BoardVehicle,cart.Id,scout.Id);Require(match.Passengers(cart.Id).Count==1&&match.VehicleCapacity(cart.Id)==3&&match.EffectiveKeywords(cart.Id).Contains("flying"),"Valeria capacity and keyword");yield return Capture("04-passageiro.png");selectedUnit=cart.Id;yield return Capture("04b-palavras-chave.png");int next=Match.Neighbors(home).First(n=>match.CanMove(cart.Id,n));Do(ActionKind.Move,next,cart.Id);selectedCell=next;selectedUnit=scout.Id;Require(match.Position(scout.Id)==next,"transport synchronizes");yield return Capture("05-transporte.png");Do(ActionKind.DisembarkVehicle,unit:scout.Id);Require(scout.CarrierId<0&&!match.EffectiveKeywords(cart.Id).Contains("flying"),"disembark removes inherited keyword");inspected=cart.Card;yield return Capture("06-veiculo.png");inspected=null;Require(library.Data.coins==coins&&library.Data.decks.Count==0&&library.Data.owned.Count==0,"demo leaves profile untouched");
   File.WriteAllText(Path.Combine(dir,"result.txt"),"Commander selection, normal-cost summon/play, boarding, transport, Valeria dynamic capacity/keyword, disembark and isolated profile passed.\nRuntime errors: "+errors.Count+"\n"+string.Join("\n",errors));Application.logMessageReceived-=listener;Application.Quit(errors.Count==0?0:1);
   void Require(bool value,string label){if(!value)throw new Exception("COMMANDER UI: "+label);}
   void Do(ActionKind kind,int target=-1,int unit=-1,string card=null){if(!match.Try(new Command{player=match.Controller,revision=match.Revision,kind=kind,target=target,unit=unit,card=card,pile=0},out var error))throw new Exception("COMMANDER UI "+kind+": "+error);}
   IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.7f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name));yield return new WaitForSecondsRealtime(.6f);}
  }
 }
}

