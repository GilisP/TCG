using System;
using System.Linq;
using System.Collections.Generic;
namespace TCG
{
    public enum MatchMode { Duel, Quick, Practice }
    public sealed partial class Game
    {
        public bool CanChangeEquipment(int source,int equipment,int target = -1)
        {
            if (!OpenMain || !Valid(source) || Current.Mana.Sum() < 1) return false;
            var unit = Board[source].Unit;
            if (unit == null || unit.Owner != Active || equipment < 0 || equipment >= unit.Equipment.Count) return false;
            if (target == -1) return true;
            if (!Valid(target) || !Adjacent(source,target)) return false;
            var other = Board[target].Unit;
            return other != null && other.Owner == Active && !other.IsVehicle && other.Equipment.Count < 2;
        }
        public void ChangeEquipment(int source,int equipment,int target = -1)
        {
            Main(); Require(CanChangeEquipment(source,equipment,target),"Exige 1 mana, equipamento seu e, para transferir, criatura aliada adjacente com espaço.");
            var unit=Board[source].Unit; var card=unit.Equipment[equipment]; Pay(Active,1);
            Push(new StackItem {Kind=StackKind.EquipmentChange,Owner=Active,Source=unit,Target=target < 0 ? null : Board[target].Unit,
                Permanent=card,Description=(target < 0 ? "Desequipar: " : "Transferir: ")+card.Name});
        }
        public bool CanRepair(int index,int building)
        {
            if(!OpenMain || !Valid(index) || Board[index].Owner!=Active || building<0 || building>=Board[index].Buildings.Count || Current.Mana.Sum()<1) return false;
            var b=Board[index].Buildings[building]; var u=Board[index].Unit;
            return b.Life<b.Card.Definition.BuildingLife && b.RepairTurn!=Turn && u!=null && u.Owner==Active && !u.IsVehicle;
        }
        public void Repair(int index,int building)
        {
            Main();Require(CanRepair(index,building),"Reparo exige habitante, construção ferida, 1 mana e no máximo uma vez por turno.");
            var b=Board[index].Buildings[building];Pay(Active,1);b.RepairTurn=Turn;
            Push(new StackItem{Kind=StackKind.Repair,Owner=Active,TargetTile=index,Building=b,Source=Board[index].Unit,Description="Reparar 2: "+b.Card.Name});
        }
        public bool CanEmbark(int creature,int vehicle)
        {
            if(!OpenMain || !Valid(creature) || !Valid(vehicle) || !Adjacent(creature,vehicle)) return false;
            var unit=Board[creature].Unit;var transport=Board[vehicle].Unit;
            return unit!=null && !unit.IsVehicle && unit.Owner==Active && unit.Actions>0 && transport!=null && transport.IsVehicle && transport.Owner==Active && transport.Passengers.Count<transport.Card.Definition.Seats;
        }
        public void Embark(int creature,int vehicle)
        {
            Main();Require(CanEmbark(creature,vehicle),"Embarque exige criatura sua adjacente com uma ação e assento livre.");
            var unit=Board[creature].Unit;unit.Actions--;unit.Movement=0;Board[creature].Unit=null;Board[vehicle].Unit.Passengers.Add(unit);
            Emit("move",creature,vehicle,Active,unit.Card.Definition);Log.Add(unit.Card.Name+" embarcou.");
        }
        public bool CanDisembark(int vehicle,int passenger,int target)
        {
            if(!OpenMain || !Valid(vehicle) || !Valid(target) || !Adjacent(vehicle,target)) return false;
            var transport=Board[vehicle].Unit;
            return transport!=null && transport.Owner==Active && transport.IsVehicle && passenger>=0 && passenger<transport.Passengers.Count &&
                Board[target].Unit==null && (Board[target].Owner==Active || Board[target].Owner==-1);
        }
        public void Disembark(int vehicle,int passenger,int target)
        {
            Main();Require(CanDisembark(vehicle,passenger,target),"Desembarque exige casa adjacente vazia, sua ou neutra.");
            var transport=Board[vehicle].Unit;var unit=transport.Passengers[passenger];transport.Passengers.RemoveAt(passenger);
            unit.Actions=unit.Movement=0;Board[target].Unit=unit;Emit("move",vehicle,target,Active,unit.Card.Definition);QueueTerrain(target,TerrainTrigger.Enter);
            Log.Add(unit.Card.Name+" desembarcou sem ações ou movimento até o próximo turno.");
        }
        void Evacuate(Unit vehicle,int position)
        {
            foreach(var unit in vehicle.Passengers.ToArray())
            {
                int target=Enumerable.Range(0,121).Where(i=>i==position || Adjacent(position,i)).OrderBy(i=>i==position?0:1)
                    .Where(i=>Board[i].Unit==null && (Board[i].Owner==unit.Owner || Board[i].Owner==-1)).DefaultIfEmpty(-1).First();
                unit.Actions=unit.Movement=0;
                if(target>=0) {Board[target].Unit=unit;Emit("summon",target,target,unit.Owner,unit.Card.Definition);QueueTerrain(target,TerrainTrigger.Enter);}
                else
                {
                    Players[unit.Owner].Graveyard.AddRange(unit.Equipment);unit.Equipment.Clear();
                    if(unit.Commander){Players[unit.Owner].CommanderAvailable=true;Players[unit.Owner].CommanderDeaths++;}
                    else if(unit.Card.Definition.Kind!=CardKind.Token)Players[unit.Owner].Graveyard.Add(unit.Card);
                    Log.Add(unit.Card.Name+" perdeu-se na evacuação: nenhuma casa segura disponível.");
                }
            }
            vehicle.Passengers.Clear();
        }
        void QueueTerrain(int index,TerrainTrigger trigger)
        {
            var tile=Board[index];var def=tile.Terrain?.Definition;
            if(def==null || def.AutomaticTrigger!=trigger || def.AutomaticEffects==null || def.AutomaticEffects.Length==0 || tile.Owner<0) return;
            if(trigger==TerrainTrigger.TurnStart && tile.Owner!=Active) return;
            if(trigger==TerrainTrigger.Enter && (tile.Unit==null || tile.Unit.Owner!=tile.Owner)) return;
            Push(new StackItem {Kind=StackKind.PermanentAbility,Owner=tile.Owner,TargetTile=index,Permanent=tile.Terrain,Source=tile.Unit,
                Steps=def.AutomaticEffects,Automatic=true,Description=def.Name+(trigger==TerrainTrigger.TurnStart ? " / início do turno" : " / entrada no terreno")});
        }
        bool ResolveJourney(StackItem item)
        {
            if(item.Kind==StackKind.EquipmentChange)
            {
                int from=Position(item.Source),to=Position(item.Target);
                bool valid=from>=0 && item.Source.Owner==item.Owner && item.Source.Equipment.Contains(item.Permanent) &&
                    (item.Target==null || (to>=0 && Adjacent(from,to) && item.Target.Owner==item.Owner && !item.Target.IsVehicle && item.Target.Equipment.Count<2));
                if(!valid){Log.Add("Transferência/desequipar perdeu a fonte, o alvo ou o espaço.");return true;}
                item.Source.Equipment.Remove(item.Permanent);item.Source.Movement=Math.Min(item.Source.Movement,item.Source.MaxMovement);
                if(item.Target==null)Players[item.Owner].Hand.Add(item.Permanent);else item.Target.Equipment.Add(item.Permanent);
                Emit("buff",from,to>=0?to:from,item.Owner,item.Permanent.Definition);return true;
            }
            if(item.Kind==StackKind.Repair)
            {
                var tile=Board[item.TargetTile];
                if(tile.Owner==item.Owner && tile.Buildings.Contains(item.Building) && tile.Unit==item.Source && item.Source.Owner==item.Owner)
                {int healed=Math.Min(2,item.Building.Card.Definition.BuildingLife-item.Building.Life);item.Building.Life+=healed;Emit("heal",item.TargetTile,item.TargetTile,item.Owner,item.Building.Card.Definition,healed);}
                else Log.Add("Reparo cancelado: perdeu a construção ou o habitante.");
                return true;
            }
            return false;
        }
    }
    public static class JourneyCards
    {
        public static void AddTo(List<CardDefinition> cards)
        {
            cards.Add(new CardDefinition{Id="kingdom-carriage",Name="Carruagem da Fronteira",Kind=CardKind.Vehicle,Element=5,Cost=2,ColoredCost=new[]{0,0,0,0,0,1},Attack=1,Life=6,Movement=3,Seats=3,CrewRequired=1,Art="c",Text="Transporte: 3 assentos; exige 1 tripulante para mover/atacar. Embarque adjacente custa uma ação da criatura. Desembarque sem ações neste turno."});
            cards.Add(new CardDefinition{Id="kingdom-siege",Name="Carro de Cerco Solar",Kind=CardKind.Vehicle,Element=0,Cost=3,ColoredCost=new[]{1,0,0,0,0,0},Attack=4,Life=8,Movement=1,Range=2,Seats=2,CrewRequired=2,Art="c",Text="2 assentos e 2 tripulantes para operar. Ataque 4, alcance 2. Passageiros são evacuados quando o veículo sai do campo."});
            cards.Add(new CardDefinition{Id="kingdom-sanctuary",Name="Santuário da Primeira Luz",Kind=CardKind.Terrain,Element=0,Housing=2,Art="land",AutomaticTrigger=TerrainTrigger.TurnStart,AutomaticEffects=new[]{new EffectStep(Effect.Heal,EffectTarget.Owner,2)},Text="Gera 1 Sol. No início do seu turno: cure 2 do jogador. Habilidade automática usa a pilha."});
            cards.Add(new CardDefinition{Id="kingdom-gate",Name="Portão dos Ventos",Kind=CardKind.Terrain,Element=4,Housing=2,Art="land",AutomaticTrigger=TerrainTrigger.Enter,AutomaticEffects=new[]{new EffectStep(Effect.Shield,EffectTarget.Source,1,EffectDuration.EndOfTurn)},Text="Gera 1 Ar. Quando uma unidade sua entra aqui, recebe escudo 1 até o final do turno. Usa a pilha."});
        }
    }
}
