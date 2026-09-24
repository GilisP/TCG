using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
 [Serializable] public sealed class NetSeat { public string name,commander; public int team,life,hand,main,terrains,casts,death,lastTerrain; public bool eliminated,ready; public int[] mana,pileCounts; public string[] tops,grave,terrainGrave,exile; }
 [Serializable] public sealed class NetPiece { public int id,owner,original,cell,attack,defense,range,damage,actions,movement,attached,carrier,lastMoved; public CardData card; }
 [Serializable] public sealed class NetCell { public int owner,capital,terrainOwner; public string terrain; }
 [Serializable] public sealed class NetPending { public int owner,target,unit,defender,chooser; public bool defense; public CardData card; public int[] attackers,blockers; }
 [Serializable] public sealed class NetOption { public int key; public string label; }
 [Serializable] public sealed class NetOrder { public int piece,owner,destination,state; }
 [Serializable] public sealed class NetRoute { public string key; public int[] path; }
 [Serializable] public sealed class NetExile { public int id,owner,original; public string card; public bool any; }
 [Serializable] public sealed class NetVisual { public string kind; public int from,to,piece,owner; public CardData card; }
 [Serializable] public sealed class NetLook { public int owner; public string card,style; public bool foil; }
 [Serializable] public sealed class MatchView
 {
  public MatchNumbers numbers; public int viewer,active,priority,controller,revision,turn,phase,winner,choiceOwner=-1; public bool placed,over; public string prompt;
  public NetLook[] looks=Array.Empty<NetLook>(); public NetSeat[] seats; public NetCell[] cells; public NetPiece[] pieces; public NetPending[] stack; public string[] hand,legal; public NetOption[] options;
  public NetOrder[] orders; public NetRoute[] routes; public NetExile[] exile; public NetVisual[] visuals=Array.Empty<NetVisual>();
 }
 public static class NetworkCards
 {
  public static CardData Pack(Definition c)=>c==null?null:new CardData{id=c.Id,name=c.Name,kind=c.Kind.ToString(),text=c.Text,rarity=c.Rarity,art=c.Art,rule=c.Rule,traits=c.Traits.ToArray(),keywords=c.Keywords.ToArray(),subtypes=c.Subtypes.ToArray(),commander=c.IsCommander,identityColors=c.IdentityColors.ToArray(),maxCopies=c.MaxCopies,vehicleSeats=c.VehicleSeats,vehicleCrew=c.VehicleCrew,actions=c.Actions,range=c.Range,equipCost=c.EquipCost,color=c.Color,cost=c.Cost,attack=c.Attack,defense=c.Defense,movement=c.Movement,coloredCost=c.ColoredCost.ToArray(),effects=c.Effects.Select(e=>new EffectData{operation=e.Operation,amount=e.Amount}).ToArray(),foil=c.Foil};
  public static Definition Unpack(CardData d)=>d==null||string.IsNullOrEmpty(d.id)?null:new Definition(d,"network-public");
 }
 public sealed partial class Match
 {
  MatchView remote; HashSet<string> remoteLegal; Dictionary<string,int[]> remoteRoutes;
  public bool IsRemoteView=>remote!=null;
  static string NetKey(string action,int a=-1,int b=-1,string card="")=>action+":"+a+":"+b+":"+card;
  bool NetCan(string action,int a=-1,int b=-1,string card="")=>remoteLegal.Contains(NetKey(action,a,b,card));
  Match(int count,EffectRegistry registry){seats=new Seat[count];effects=registry;}
  public static Match FromView(MatchView view,ContentCatalog catalog,EffectRegistry effects){var m=new Match(view.seats.Length,effects);m.ApplyView(view,catalog);return m;}
  public void ApplyView(MatchView view,ContentCatalog catalog)
  {
   if(view==null||view.cells.Length!=121||view.seats.Length!=seats.Length)throw new InvalidOperationException("Visão de rede inválida.");
   remote=view;remoteLegal=new HashSet<string>(view.legal??Array.Empty<string>());remoteRoutes=(view.routes??Array.Empty<NetRoute>()).ToDictionary(x=>x.key,x=>x.path);
   Active=view.active;Priority=view.priority;Revision=view.revision;Turn=view.turn;Phase=(Stage)view.phase;Placed=view.placed;Over=view.over;WinningTeam=view.winner;
   Definition Card(string id)=>string.IsNullOrEmpty(id)?null:id==Ruins.Id?Ruins:catalog.Get(id);
   for(int i=0;i<seats.Length;i++){var s=view.seats[i];var seat=new Seat(s.name,s.team){Life=s.life,Eliminated=s.eliminated,Commander=Card(s.commander),CommanderReady=s.ready,CommanderCasts=s.casts,DeathCounters=s.death,LastCreatedTerrain=s.lastTerrain,NetHand=s.hand,NetMain=s.main,NetTerrain=s.terrains,NetPiles=s.pileCounts};seats[i]=seat;Array.Copy(s.mana,seat.mana,7);seat.grave.AddRange(s.grave.Select(Card));seat.terrainGrave.AddRange(s.terrainGrave.Select(Card));seat.exile.AddRange(s.exile.Select(Card));for(int n=0;n<4;n++)if(!string.IsNullOrEmpty(s.tops[n]))seat.piles[n].Add(Card(s.tops[n]));}
   seats[view.viewer].hand.AddRange(view.hand.Select(Card));
   for(int i=0;i<121;i++){var c=view.cells[i];cells[i].Owner=c.owner;cells[i].CapitalOwner=c.capital;cells[i].TerrainOwner=c.terrainOwner;cells[i].Terrain=Card(c.terrain);cells[i].pieces.Clear();}
   foreach(var p in view.pieces){var u=new Piece(p.id,p.owner,NetworkCards.Unpack(p.card)){OriginalOwner=p.original,Damage=p.damage,Movement=p.movement,Actions=p.actions,AttachedTo=p.attached,CarrierId=p.carrier,LastMovedTurn=p.lastMoved,NetAttack=p.attack,NetDefense=p.defense,NetRange=p.range};cells[p.cell].pieces.Add(u);}
   stack.Clear();foreach(var s in view.stack)stack.Add(new Pending{Owner=s.owner,Target=s.target,TargetUnit=s.unit,Defender=s.defender,Chooser=s.chooser,NeedsDefense=s.defense,Card=NetworkCards.Unpack(s.card),Attackers=s.attackers,Blockers=s.blockers});
   Choice=view.choiceOwner<0?null:new RuleChoice(view.choiceOwner,view.prompt,(view.options??Array.Empty<NetOption>()).Select(x=>new ChoiceOption(x.key,x.label)),null);
   movementOrders.Clear();foreach(var o in view.orders)movementOrders[o.piece]=new MovementOrder(o.piece,o.owner,o.destination){State=(MovementOrderState)o.state};
   exilePermissions.Clear();foreach(var x in view.exile)exilePermissions.Add(new ExilePermission{Id=x.id,Owner=x.owner,CardOwner=x.original,Card=Card(x.card),AnyMana=x.any});
   log.Clear();log.Add("Estado recebido do host · revisão "+Revision);
   foreach(var v in view.visuals??Array.Empty<NetVisual>())Visual?.Invoke(new MatchEvent(v.kind,v.from,v.to,v.piece,NetworkCards.Unpack(v.card),v.owner));
  }
  public MatchView ViewFor(int viewer)
  {
   Check(!IsRemoteView&&viewer>=0&&viewer<seats.Length,"Assento inválido.");
   var legal=new List<string>();var routes=new List<NetRoute>();void Add(bool yes,string k,int a=-1,int b=-1,string c=""){if(yes)legal.Add(NetKey(k,a,b,c));}
   var pieces=All.ToArray();
   if(Controller==viewer&&!Over){
    for(int at=0;at<121;at++){Add(CanPlace(at),"place",at);Add(CanSummonCommander(at),"summon",at);foreach(var card in seats[viewer].hand.Distinct()){Add(CanPlay(card,at),"play",at,-1,card.Id);foreach(var p in cells[at].pieces)Add(CanPlay(card,at,p.Id),"play",at,p.Id,card.Id);}}
    foreach(var c in seats[viewer].hand.Distinct())Add(CanPlay(c,-1),"play",-1,-1,c.Id);
    foreach(var p in pieces.Where(p=>p.Owner==viewer)){Add(CanActivate(p.Id),"activate",p.Id);Add(CanOrderMovement(p.Id),"order",p.Id);Add(CanDisembark(p.Id),"disembark",p.Id);for(int at=0;at<121;at++){Add(CanMove(p.Id,at),"move",p.Id,at);Add(CanAttack(p.Id,at),"attack",p.Id,at);if(CanOrderMovement(p.Id))routes.Add(new NetRoute{key=p.Id+":"+at,path=MovementRoute(p.Id,at).ToArray()});}foreach(var q in pieces){Add(CanEquip(p.Id,q.Id),"equip",p.Id,q.Id);Add(CanBoard(p.Id,q.Id),"board",p.Id,q.Id);}}
    foreach(var x in ExiledFor(viewer))Add(CanCastExiled(x.Id),"exile",x.Id);
   }
   return new MatchView{numbers=NumbersFor(viewer),viewer=viewer,active=Active,priority=Priority,controller=Controller,revision=Revision,turn=Turn,phase=(int)Phase,placed=Placed,over=Over,winner=WinningTeam,
    hand=seats[viewer].hand.Select(c=>c.Id).ToArray(),legal=legal.ToArray(),routes=routes.ToArray(),
    seats=seats.Select(s=>new NetSeat{name=s.Name,team=s.Team,life=s.Life,eliminated=s.Eliminated,commander=s.Commander?.Id,ready=s.CommanderReady,casts=s.CommanderCasts,death=s.DeathCounters,lastTerrain=s.LastCreatedTerrain,hand=s.HandCount,main=s.MainCount,terrains=s.TerrainCount,mana=s.mana.ToArray(),tops=Enumerable.Range(0,4).Select(n=>s.Top(n)?.Id).ToArray(),pileCounts=Enumerable.Range(0,4).Select(s.PileCount).ToArray(),grave=s.grave.Select(c=>c.Id).ToArray(),terrainGrave=s.terrainGrave.Select(c=>c.Id).ToArray(),exile=s.exile.Select(c=>c.Id).ToArray()}).ToArray(),
    cells=cells.Select(c=>new NetCell{owner=c.Owner,capital=c.CapitalOwner,terrainOwner=c.TerrainOwner,terrain=c.Terrain?.Id}).ToArray(),
    pieces=pieces.Select(p=>new NetPiece{id=p.Id,owner=p.Owner,original=p.OriginalOwner,cell=Position(p.Id),card=NetworkCards.Pack(p.Card),attack=p.Attack,defense=p.Defense,range=p.Range,damage=p.Damage,actions=p.Actions,movement=p.Movement,attached=p.AttachedTo,carrier=p.CarrierId,lastMoved=p.LastMovedTurn}).ToArray(),
    stack=stack.Select(s=>new NetPending{owner=s.Owner,target=s.Target,unit=s.TargetUnit,defender=s.Defender,chooser=s.Chooser,defense=s.NeedsDefense,card=NetworkCards.Pack(s.Card),attackers=s.Attackers,blockers=s.Blockers}).ToArray(),
    choiceOwner=Choice?.Owner??-1,prompt=Choice==null?"":Choice.Owner==viewer?Choice.Prompt:"Aguardando escolha de outro jogador",options=Choice!=null&&Choice.Owner==viewer?Choice.Options.Select(o=>new NetOption{key=o.Key,label=o.Label}).ToArray():Array.Empty<NetOption>(),
    orders=OrdersFor(viewer).Select(o=>new NetOrder{piece=o.Piece,owner=o.Owner,destination=o.Destination,state=(int)o.State}).ToArray(),exile=ExiledFor(viewer).Select(x=>new NetExile{id=x.Id,owner=x.Owner,original=x.CardOwner,card=x.Card.Id,any=x.AnyMana}).ToArray()};
  }
 }
}
