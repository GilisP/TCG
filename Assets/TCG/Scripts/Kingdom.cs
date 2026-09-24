using System;
using System.Linq;
using System.Collections.Generic;
namespace TCG
{
    public sealed class Building
    {
        public Card Card;
        public int Owner, Life, UsedTurn = -1, RepairTurn = -1;
        public Building(Card card,int owner) { Card = card; Owner = owner; Life = card.Definition.BuildingLife; }
    }
    public sealed partial class Game
    {
        public bool CanPay(int owner,CardDefinition card,int tax = 0)
        {
            if (card == null || tax < 0) return false;
            var pool = Players[owner].Mana;
            for (int e = 0; e < 6; e++) if (pool[e] < card.ColoredCost[e]) return false;
            return pool.Sum() >= card.TotalCost+tax;
        }
        void PayCard(int owner,CardDefinition card,int tax)
        {
            Require(CanPay(owner,card,tax),"Falta mana dos elementos exigidos.");
            for (int e = 0; e < 6; e++) Players[owner].Mana[e] -= card.ColoredCost[e];
            Pay(owner,card.Cost+tax);
        }
        public string AbilityBlockReason(int index,int buildingIndex = -1)
        {
            if (!OpenMain) return "Exige sua Principal e pilha vazia.";
            if (!Valid(index) || Board[index].Owner != Active) return "Escolha um domínio seu.";
            var tile = Board[index];
            if (buildingIndex < -1 || buildingIndex >= tile.Buildings.Count) return "Construção ausente.";
            var building = buildingIndex < 0 ? null : tile.Buildings[buildingIndex];
            var card = building == null ? tile.Terrain : building.Card;
            if (card == null || card.Definition.Steps.Length == 0) return "Sem habilidade ativada.";
            if ((building == null ? tile.TerrainUsedTurn : building.UsedTurn) == Turn) return "Já ativada neste turno.";
            bool inhabited = tile.Unit != null && tile.Unit.Owner == Active && (building == null || !tile.Unit.IsVehicle);
            if (building != null && !card.Definition.Autonomous && !inhabited) return "Construção desabitada: coloque uma criatura sua aqui.";
            if (card.Definition.Steps.Any(s => s.Target == EffectTarget.Source) && !inhabited) return "A habilidade exige uma criatura sua neste terreno.";
            if (Current.Mana.Sum() < card.Definition.ActivationCost) return "Mana insuficiente para ativar.";
            return "";
        }
        public void ActivatePermanent(int index,int buildingIndex = -1)
        {
            string reason = AbilityBlockReason(index,buildingIndex); Require(reason == "",reason);
            var tile = Board[index]; var building = buildingIndex < 0 ? null : tile.Buildings[buildingIndex];
            var card = building == null ? tile.Terrain : building.Card;
            Pay(Active,card.Definition.ActivationCost);
            if (building == null) tile.TerrainUsedTurn = Turn; else building.UsedTurn = Turn;
            Push(new StackItem { Kind = StackKind.PermanentAbility, Owner = Active, Permanent = card, Building = building,
                Source = tile.Unit, TargetTile = index, Steps = card.Definition.Steps, Description = card.Name+" / habilidade" });
        }
        void DestroyBuilding(int index,Building building)
        {
            if (!Board[index].Buildings.Remove(building)) return;
            Players[building.Owner].Graveyard.Add(building.Card);
            Emit("leave",index,index,building.Owner,building.Card.Definition); Log.Add(building.Card.Name+" foi demolida.");
        }
        bool ResolveKingdom(StackItem item)
        {
            if (item.Kind == StackKind.StructureAttack)
            {
                int from = Position(item.Source); var tile = Board[item.TargetTile];
                if (from >= 0 && item.Source.Owner == item.Owner && item.Source.Crewed(Turn) && Distance(from,item.TargetTile) <= item.Source.Range &&
                    tile.Owner != item.Owner && tile.Unit == null && tile.Buildings.Contains(item.Building))
                { Emit("attack",from,item.TargetTile,item.Owner,item.Source.Card.Definition); item.Building.Life -= Math.Max(0,item.Source.Attack);
                    Emit("damage",item.TargetTile,item.TargetTile,item.Owner,item.Building.Card.Definition,item.Source.Attack);
                    if (item.Building.Life <= 0) DestroyBuilding(item.TargetTile,item.Building); }
                else Log.Add("Demolição cancelada: alvo protegido, removido ou fora do alcance.");
                return true;
            }
            if (item.Kind == StackKind.PermanentAbility)
            {
                var tile = Board[item.TargetTile];
                bool exists = tile.Owner == item.Owner && (item.Building == null ? tile.Terrain == item.Permanent : tile.Buildings.Contains(item.Building));
                bool inhabited = tile.Unit != null && tile.Unit.Owner == item.Owner && (item.Building == null || !tile.Unit.IsVehicle);
                if (!exists || (item.Building != null && !item.Permanent.Definition.Autonomous && !inhabited))
                { Log.Add("Habilidade cancelada: fonte removida, conquistada ou construção desabitada."); return true; }
                // Source means the occupant selected at activation, not a replacement creature.
                foreach (var step in item.Steps)
                {
                    if (Winner >= 0) break;
                    if (step.Target == EffectTarget.Source && (tile.Unit != item.Source || !inhabited)) continue;
                    ResolveStep(item,step);
                }
                return true;
            }
            if (item.Kind != StackKind.Card || item.Card == null) return false;
            var def = item.Card.Definition;
            if (def.Kind == CardKind.Equipment)
            {
                if (item.Target != null && Position(item.Target) >= 0 && item.Target.Owner == item.Owner && !item.Target.IsVehicle && item.Target.Equipment.Count < 2)
                { item.Target.Equipment.Add(item.Card); Emit("buff",Position(item.Target),Position(item.Target),item.Owner,def); Log.Add(def.Name+" equipado em "+item.Target.Card.Name+"."); }
                else { FinishCard(item); Log.Add("Equipamento perdeu o alvo ou não há espaço."); }
                return true;
            }
            if (def.Kind == CardKind.Building)
            {
                var tile = Board[item.TargetTile];
                if (tile.Owner == item.Owner && tile.Terrain != null && tile.HousingUsed+def.HousingUse <= tile.Terrain.Definition.Housing)
                { tile.Buildings.Add(new Building(item.Card,item.Owner)); Emit("summon",item.TargetTile,item.TargetTile,item.Owner,def); }
                else { FinishCard(item); Log.Add("Construção cancelada: perdeu o terreno ou falta habitação."); }
                return true;
            }
            return false;
        }
    }
    public static class KingdomCards
    {
        public static void AddTo(List<CardDefinition> cards)
        {
            cards.Add(new CardDefinition { Id="kingdom-sword", Name="Lâmina da Forja Solar", Kind=CardKind.Equipment, Element=0, Cost=1, ColoredCost=new[]{1,0,0,0,0,0}, EquipmentAttack=2, Art="buff", Text="Equipar uma criatura sua: +2 ataque. Ocupa 1 dos 2 espaços. Ao sair a criatura, vai ao cemitério." });
            cards.Add(new CardDefinition { Id="kingdom-mail", Name="Cota das Montanhas", Kind=CardKind.Equipment, Element=5, Cost=1, ColoredCost=new[]{0,0,0,0,0,1}, EquipmentArmor=1, Art="shield", Text="Equipar uma criatura sua: +1 armadura. Reduz cada dano em 1. Dois equipamentos por criatura." });
            cards.Add(new CardDefinition { Id="kingdom-boots", Name="Botas do Peregrino", Kind=CardKind.Equipment, Element=4, ColoredCost=new[]{0,0,0,0,1,0}, EquipmentMovement=1, Art="return", Text="Equipar: +1 movimento na renovação dos próximos turnos. Não concede movimento imediato." });
            cards.Add(new CardDefinition { Id="kingdom-infirmary", Name="Enfermaria da Abadia", Kind=CardKind.Building, Element=2, Cost=1, ColoredCost=new[]{0,0,1,0,0,0}, HousingUse=1, ActivationCost=1, Art="heal", Effects=new[]{new EffectStep(Effect.Heal,EffectTarget.Source,3)}, Text="Ocupa 1 habitação. Habitada: pague 1 mana para curar 3 da criatura neste terreno. Uma vez por turno; usa a pilha." });
            cards.Add(new CardDefinition { Id="kingdom-observatory", Name="Observatório Lunar", Kind=CardKind.Building, Element=1, Cost=1, ColoredCost=new[]{0,1,0,0,0,0}, HousingUse=2, ActivationCost=2, Art="draw", Effects=new[]{new EffectStep(Effect.Draw,EffectTarget.Owner,1)}, Text="Ocupa 2 habitações. Habitado: pague 2 mana, compre 1 carta. Uma vez por turno; usa a pilha." });
            cards.Add(new CardDefinition { Id="kingdom-well", Name="Poço da Aurora", Kind=CardKind.Terrain, Element=0, Housing=2, ExtraMana=1, Art="land", Text="Gera 2 mana Sol no início do seu turno. Capacidade: 2 habitações." });
            cards.Add(new CardDefinition { Id="kingdom-grove", Name="Bosque dos Juramentos", Kind=CardKind.Terrain, Element=5, Housing=1, ActivationCost=1, Art="land", Effects=new[]{new EffectStep(Effect.Shield,EffectTarget.Source,2,EffectDuration.EndOfTurn)}, Text="Gera 1 Terra. Capacidade 1. Pague 1: criatura sua aqui recebe escudo 2 até o final. Uma vez por turno; usa a pilha." });
            cards.Add(new CardDefinition { Id="kingdom-spring", Name="Fonte das Marés", Kind=CardKind.Terrain, Element=2, Housing=3, ActivationCost=1, Art="land", Effects=new[]{new EffectStep(Effect.Heal,EffectTarget.Owner,2)}, Text="Gera 1 Água. Capacidade 3. Pague 1: cure 2 do seu jogador. Uma vez por turno; usa a pilha." });
        }
    }
}
