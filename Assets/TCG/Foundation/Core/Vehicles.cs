using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
 public sealed partial class Match
 {
  public IReadOnlyList<Piece> Passengers(int vehicle)=>All.Where(p=>p.CarrierId==vehicle).ToArray();
  public int VehicleCapacity(int vehicle){var p=Find(vehicle);return p==null||!p.Card.IsVehicle?0:p.Card.VehicleSeats+All.Count(q=>q.Owner==p.Owner&&q.Card.Rule=="valeria");}
  public IReadOnlyList<string> EffectiveKeywords(int id)
  {
   var p=Find(id);if(p==null)return Array.Empty<string>();
   var traits=p.Card.Keywords.AsEnumerable();
   if(p.Card.IsVehicle&&All.Any(q=>q.Owner==p.Owner&&q.Card.Rule=="valeria"))traits=traits.Concat(Passengers(id).SelectMany(q=>q.Card.Keywords));
   return traits.Distinct().ToArray();
  }
  bool Crewed(Piece p)=>!p.Card.IsVehicle||Passengers(p.Id).Count>=p.Card.VehicleCrew;
  bool CanAct(Piece p)=>p!=null&&Crewed(p)&&(p.CarrierId<0||Find(p.CarrierId)!=null&&!Find(p.CarrierId).Card.IsMeka&&Crewed(Find(p.CarrierId)));
  bool Mobile(Piece p)=>p!=null&&(p.Card.Kind==CardType.Creature||p.Card.IsVehicle)&&p.CarrierId<0&&CanAct(p);
  bool CargoWindow=>!Over&&Choice==null&&Defense==null&&stack.Count==0&&Priority==Active&&Phase==Stage.Main;
  public bool CanBoard(int troop,int vehicle)
  {if(IsRemoteView)return NetCan("board",troop,vehicle);
   var p=Find(troop);var v=Find(vehicle);
   return CargoWindow&&p!=null&&v!=null&&p.Card.Kind==CardType.Creature&&p.Owner==Active&&v.Owner==Active&&v.Card.IsVehicle&&p.CarrierId<0&&p.Actions>=1&&Position(troop)==Position(vehicle)&&Passengers(vehicle).Count<VehicleCapacity(vehicle);
  }
  public bool CanDisembark(int troop){if(IsRemoteView)return NetCan("disembark",troop);var p=Find(troop);return CargoWindow&&p!=null&&p.Owner==Active&&p.CarrierId>=0&&Find(p.CarrierId)!=null&&p.Actions>=1;}
  void BoardVehicle(int troop,int vehicle)
  {
   MainOnly();if(vehicle<0){var options=All.Where(v=>CanBoard(troop,v.Id)).Select(v=>new ChoiceOption(v.Id,v.Card.Name+" · "+Passengers(v.Id).Count+"/"+VehicleCapacity(v.Id))).ToArray();Check(options.Length>0,"Nenhum veículo com vaga neste tile.");Ask(Active,"Embarcar: escolha o veículo (1 PA)",options,n=>{if(n>=0)BoardVehicle(troop,n);},true);return;}Check(CanBoard(troop,vehicle),"Embarque exige tropa sua e vaga no mesmo tile, pagando 1 PA da tropa.");
   var p=Find(troop);p.Actions--;p.CarrierId=vehicle;movementOrders.Remove(troop);Note(p.Card.Name+" embarcou em "+Find(vehicle).Card.Name+" (1 PA).");
  }
  void DisembarkVehicle(int troop)
  {
   MainOnly();Check(CanDisembark(troop),"Desembarque exige 1 PA da tropa.");var p=Find(troop);p.Actions--;p.CarrierId=-1;Note(p.Card.Name+" desembarcou no mesmo tile (1 PA).");
  }
  void CarryPassengers(Piece vehicle,int from,int to)
  {
   // Board occupants stay individually targetable; their relation follows the vehicle.
   if(!vehicle.Card.IsVehicle)return;
   foreach(var p in Passengers(vehicle.Id).ToArray())
   {
    int origin=Position(p.Id);if(origin==to)continue;cells[origin].pieces.Remove(p);cells[to].pieces.Add(p);p.LastMovedTurn=Turn;
    foreach(var e in All.Where(e=>e.AttachedTo==p.Id).ToArray()){cells[Position(e.Id)].pieces.Remove(e);cells[to].pieces.Add(e);}
    Visual?.Invoke(new MatchEvent("move",origin,to,p.Id,p.Card,p.Owner));
   }
  }
  void VehicleDestroyed(Piece vehicle,int killer)
  {
   foreach(var p in Passengers(vehicle.Id).ToArray()){p.CarrierId=-1;Kill(p,killer);}
  }
  void ReleasePassenger(Piece p)
  {
   p.CarrierId=-1;
   foreach(var passenger in Passengers(p.Id).ToArray())passenger.CarrierId=-1;
  }
 }
}

