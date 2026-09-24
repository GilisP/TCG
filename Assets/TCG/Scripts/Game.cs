using System;
using System.Collections.Generic;
using System.Linq;

namespace TCG
{
    public enum Phase { Start, Draw, Terrain, Main, End }
    public enum StackKind { Card, Attack, Command, Trigger, PermanentAbility, StructureAttack, EquipmentChange, Repair }
    public sealed class Card
    {
        public readonly CardDefinition Definition;
        public readonly string InstanceId;
        public string Id => Definition.Id;
        public string Name => Definition.Name;
        public int Element => Definition.Element;
        public Card(CardDefinition definition,string instanceId) { Definition = definition; InstanceId = instanceId; }
    }
    public sealed class Unit
    {
        public int Owner, Life, Movement, Actions = 1, BonusAttack, AttackPenalty, Shield, TemporaryShield, StunnedThroughTurn;
        public readonly Card Card;
        public bool IsVehicle => Card.Definition.Kind == CardKind.Vehicle;
        public readonly List<Unit> Passengers = new List<Unit>();
        public bool Crewed(int turn) => !IsVehicle || Passengers.Count(p => p.StunnedThroughTurn < turn) >= Card.Definition.CrewRequired;
        public readonly List<Card> Equipment = new List<Card>();
        public int Attack => Math.Max(0,Card.Definition.Attack+BonusAttack+Equipment.Sum(c => c.Definition.EquipmentAttack)-AttackPenalty);
        public int Armor => (Card.Definition.Trait == Trait.Armor ? 1 : 0)+Equipment.Sum(c => c.Definition.EquipmentArmor);
        public int MaxMovement => Card.Definition.Movement+Equipment.Sum(c => c.Definition.EquipmentMovement);
        public int Range => Card.Definition.Range;
        public bool Commander => Card.Definition.Kind == CardKind.Commander;
        public Unit(int owner,Card card) { Owner = owner; Card = card; Life = card.Definition.Life; Movement = card.Definition.Movement; }
    }
    public sealed class Tile
    {
        public int Owner = -1;
        public bool Capital;
        public Card Terrain;
        public int TerrainUsedTurn = -1, ManaChoice = -1;
        public int ProducedElement => Terrain == null ? 6 : Terrain.Definition.ProductionColors.Contains(ManaChoice) ? ManaChoice : Terrain.Definition.ProductionColors[0];
        public readonly List<Building> Buildings = new List<Building>();
        public int HousingUsed => Buildings.Sum(b => b.Card.Definition.HousingUse);
        public Unit Unit;
    }
    public sealed class Player
    {
        public string Name;
        public int Life = 50, Popularity = 2, CommanderDeaths;
        public bool CommanderAvailable = true, CommandUsed;
        public Card Commander;
        public int[] Mana = new int[7];
        public readonly List<Card> Deck = new List<Card>(), Terrains = new List<Card>(), Offer = new List<Card>(), Hand = new List<Card>(),
            Graveyard = new List<Card>(), TerrainGraveyard = new List<Card>(), Exile = new List<Card>();
        public Player(string name) { Name = name; }
    }
    public sealed class StackItem
    {
        public int Id, Owner, TargetTile = -1, CounterId = -1, Command;
        public StackKind Kind;
        public Card Card;
        public Unit Source, Target;
        public Card Permanent;
        public Building Building;
        public bool Automatic;
        public Effect Effect;
        public int Value;
        public EffectStep[] Steps = Array.Empty<EffectStep>();
        public EffectTarget? TargetMode;
        public string Description;
    }
    public sealed class VisualEvent
    {
        public string Kind;
        public int From, To, Value, Owner;
        public CardDefinition Card;
        public VisualEvent(string kind,int from,int to,int owner,CardDefinition card = null,int value = 0)
        { Kind = kind; From = from; To = to; Owner = owner; Card = card; Value = value; }
    }
    public sealed partial class Game
    {
        public static readonly string[] Elements = Catalog.Elements;
        public readonly Tile[] Board = Enumerable.Range(0,121).Select(_ => new Tile()).ToArray();
        public readonly Player[] Players = { new Player("Reino do Sol"),new Player("Reino da Lua") };
        public readonly List<string> Log = new List<string>();
        public readonly List<StackItem> Stack = new List<StackItem>();
        public event Action<VisualEvent> Visual;
        public int Active, Priority, Turn = 1, Winner = -1, Passes;
        public bool Placed;
        public Phase Phase = Phase.Start;
        int nextStackId = 1;
        public Player Current => Players[Active];
        public Player Acting => Players[Priority];
        public string PhaseName => new[] { "Início", "Compra", "Terreno", "Principal", "Final" }[(int)Phase];
        public readonly MatchMode Mode;
        public int LifeLimit => Mode == MatchMode.Quick ? 30 : 50;
        public Game(int seed = 42,DeckList first = null,DeckList second = null,MatchMode mode = MatchMode.Duel)
        {
            Mode = mode; Require(Enum.IsDefined(typeof(MatchMode),mode),"Modo inválido.");
            foreach (var player in Players) player.Life = LifeLimit;
            var decks = new[] { first ?? Catalog.DefaultDeck(0),second ?? Catalog.DefaultDeck(1) };
            var random = new Random(seed);
            for (int p = 0; p < 2; p++)
            {
                var errors = Catalog.Validate(decks[p]); Require(errors.Count == 0,string.Join(" ",errors));
                var player = Players[p]; int serial = 0;
                foreach (var id in decks[p].main) player.Deck.Add(new Card(Catalog.Get(id),$"p{p}-{serial++}"));
                foreach (var id in decks[p].terrains) player.Terrains.Add(new Card(Catalog.Get(id),$"p{p}-{serial++}"));
                player.Commander = new Card(Catalog.Get(decks[p].commander),$"p{p}-commander");
                Shuffle(player.Deck,random); Shuffle(player.Terrains,random);
                for (int i = 0; i < 5; i++) { player.Hand.Add(Pop(player.Deck)); player.Offer.Add(Pop(player.Terrains)); }
                Board[p == 0 ? 5 : 115].Owner = p; Board[p == 0 ? 5 : 115].Capital = true;
            }
            BeginTurn();
        }
        static void Shuffle(List<Card> list,Random random)
        { for (int i = list.Count-1; i > 0; i--) { int j = random.Next(i+1); var card = list[i]; list[i] = list[j]; list[j] = card; } }
        static Card Pop(List<Card> list) { var card = list[list.Count-1]; list.RemoveAt(list.Count-1); return card; }
        static void Require(bool condition,string message) { if (!condition) throw new InvalidOperationException(message); }
        static bool Valid(int i) => i >= 0 && i < 121;
        public static int Distance(int a,int b) => Math.Abs(a%11-b%11)+Math.Abs(a/11-b/11);
        public static bool Adjacent(int a,int b) => Distance(a,b) == 1;
        public int Position(Unit unit) => unit == null ? -1 : Array.FindIndex(Board,t => t.Unit == unit);
        public bool OpenMain => Winner < 0 && Phase == Phase.Main && Stack.Count == 0 && Priority == Active;
        void Live() { Require(Winner < 0,"A partida terminou."); }
        void Main() { Live(); Require(OpenMain,"Esta ação exige sua fase principal, pilha vazia e sua prioridade."); }
        void Emit(string kind,int from,int to,int owner,CardDefinition card = null,int value = 0) => Visual?.Invoke(new VisualEvent(kind,from,to,owner,card,value));
        void BeginTurn()
        {
            Phase = Phase.Start; Priority = Active; Passes = 0; Placed = false; Current.CommandUsed = false;
            Array.Clear(Current.Mana,0,7);
            foreach (var tile in Board)
            {
                if (tile.Owner == Active) Current.Mana[tile.ProducedElement] += 1+(tile.Terrain?.Definition.ExtraMana ?? 0);
                if (tile.Unit != null && tile.Unit.Owner == Active)
                {
                    bool frozen = tile.Unit.StunnedThroughTurn >= Turn;
                    tile.Unit.Movement = frozen ? 0 : tile.Unit.MaxMovement; tile.Unit.Actions = frozen ? 0 : 1;
                }
            }
            foreach (var unit in Board.Where(t => t.Unit != null).SelectMany(t => t.Unit.Passengers).Where(u => u.Owner == Active))
            { bool frozen = unit.StunnedThroughTurn >= Turn; unit.Actions = frozen ? 0 : 1; unit.Movement = frozen ? 0 : unit.MaxMovement; }
            if (Mode == MatchMode.Quick) Current.Mana[6]++;
            if (Mode == MatchMode.Practice) for (int e = 0; e < 7; e++) Current.Mana[e] += 10;
            for (int i = Board.Length-1; i >= 0; i--) QueueTerrain(i,TerrainTrigger.TurnStart);
            Log.Add($"Turno {Turn} / Início: {Current.Name} renova mana, movimentos e ações.");
        }
        void Draw(int owner,int count)
        {
            for (int n = 0; n < count; n++)
            {
                if (Players[owner].Deck.Count == 0) { Winner = 1-owner; Log.Add(Players[owner].Name+" perdeu por compra sem cartas."); return; }
                Players[owner].Hand.Add(Pop(Players[owner].Deck));
            }
            Log.Add($"{Players[owner].Name} comprou {count} carta(s).");
        }
        public bool MustPlace => !Placed && Current.Offer.Count > 0 && Enumerable.Range(0,121).Any(CanPlace);
        public void AdvancePhase()
        {
            Live(); Require(Stack.Count == 0 && Priority == Active,"Resolva a pilha e recupere a prioridade antes de avançar.");
            if (Phase == Phase.Start) { Phase = Phase.Draw; Draw(Active,2); }
            else if (Phase == Phase.Draw) Phase = Phase.Terrain;
            else if (Phase == Phase.Terrain) { Require(!MustPlace,"Coloque um terreno antes da fase principal."); Phase = Phase.Main; }
            else if (Phase == Phase.Main) Phase = Phase.End;
            else { PassPriority(); return; }
            Passes = 0; Log.Add("Fase: "+PhaseName+".");
        }
        public bool CanPlace(int index)
        {
            if (!Valid(index) || Placed || Phase != Phase.Terrain || Priority != Active || Winner >= 0 || Stack.Count > 0 || Board[index].Capital) return false;
            return Board[index].Owner == Active || (Board[index].Owner == -1 && Enumerable.Range(0,121).Any(i => Board[i].Owner == Active && Adjacent(i,index)));
        }
        public void Place(int offer,int index)
        {
            Live(); Require(offer >= 0 && offer < Current.Offer.Count,"Selecione um terreno da oferta."); Require(CanPlace(index),"Coloque um terreno na fase Terreno, em uma casa marcada.");
            var tile = Board[index]; Require(tile.HousingUsed <= Current.Offer[offer].Definition.Housing,"Este terreno não comporta as construções existentes."); if (tile.Terrain != null) Current.TerrainGraveyard.Add(tile.Terrain);
            tile.ManaChoice = -1; tile.TerrainUsedTurn = -1; tile.Terrain = Current.Offer[offer]; Current.Offer.RemoveAt(offer); tile.Owner = Active;
            if (Current.Terrains.Count > 0) Current.Offer.Add(Pop(Current.Terrains));
            Placed = true; Log.Add("Terreno colocado: "+tile.Terrain.Name); Emit("terrain",index,index,Active,tile.Terrain.Definition);
        }
        void Pay(int owner,int cost)
        {
            Require(Players[owner].Mana.Sum() >= cost,"Mana insuficiente.");
            for (int e = 6; e >= 0; e--) { int amount = Math.Min(cost,Players[owner].Mana[e]); Players[owner].Mana[e] -= amount; cost -= amount; }
        }
        public string CardBlockReason(CardDefinition card,int owner)
        {
            if (Winner >= 0) return "A partida terminou.";
            if (owner != Priority) return "Aguarde sua prioridade.";
            if (card == null || card.Kind == CardKind.Terrain || card.Kind == CardKind.Token) return "Selecione uma carta jogável.";
            if (card.Kind == CardKind.Trick)
            { if (Phase != Phase.Main && Phase != Phase.End && Stack.Count == 0) return "Truques só na fase principal ou final."; }
            else if (!OpenMain || owner != Active) return "Só na sua fase principal, com pilha vazia.";
            int cost = card.Kind == CardKind.Commander ? CommanderCost(owner) : card.Cost;
            if (!CanPay(owner,card,cost-card.Cost)) return "Mana insuficiente: "+card.CostLabel+(cost > card.Cost ? " + "+(cost-card.Cost)+" de taxa" : "")+".";
            if (card.Kind == CardKind.Commander && !Players[owner].CommanderAvailable) return "Comandante indisponível: no campo, na pilha ou exilado.";
            if (card.NeedsStackTarget && Stack.Count == 0) return "Não há ação na pilha para anular.";
            return "";
        }
        public bool CanTarget(CardDefinition card,int owner,int index)
        {
            if (CardBlockReason(card,owner) != "" || !Valid(index)) return false;
            var tile = Board[index];
            if (card.IsUnit) return tile.Owner == owner && tile.Unit == null;
            if (card.Kind == CardKind.Equipment) return tile.Unit != null && tile.Unit.Owner == owner && !tile.Unit.IsVehicle && tile.Unit.Equipment.Count < 2;
            if (card.Kind == CardKind.Building) return tile.Owner == owner && tile.Terrain != null && tile.HousingUsed+card.HousingUse <= tile.Terrain.Definition.Housing;
            if (card.ChosenTarget == EffectTarget.EmptyOwnedTile) return tile.Owner == owner && tile.Unit == null;
            if (card.ChosenTarget == EffectTarget.EnemyUnit) return tile.Unit != null && tile.Unit.Owner != owner;
            if (card.ChosenTarget == EffectTarget.AnyUnit) return tile.Unit != null;
            if (card.ChosenTarget == EffectTarget.AllyUnit) return tile.Unit != null && tile.Unit.Owner == owner &&
                (!card.Steps.Where(s => s.Target == EffectTarget.AllyUnit).All(s => s.Operation == Effect.Heal) || tile.Unit.Life < tile.Unit.Card.Definition.Life);
            return false;
        }
        public void PlayCard(int owner,int handIndex,int target = -1,int counterId = -1)
        {
            Live(); Require(owner == Priority,"Você não tem prioridade."); var player = Players[owner];
            Require(handIndex >= 0 && handIndex < player.Hand.Count,"Selecione uma carta da mão.");
            var card = player.Hand[handIndex]; AnnounceCard(owner,card,target,counterId);
            player.Hand.RemoveAt(handIndex);
        }
        void AnnounceCard(int owner,Card card,int target,int counterId)
        {
            var def = card.Definition; string reason = CardBlockReason(def,owner); Require(reason == "",reason);
            if (def.NeedsBoardTarget) Require(CanTarget(def,owner,target),"Selecione um alvo válido destacado no campo.");
            if (def.NeedsStackTarget) Require(Stack.Any(s => s.Id == counterId),"Selecione uma ação da pilha para anular.");
            PayCard(owner,def,def.Kind == CardKind.Commander ? 2*Players[owner].CommanderDeaths : 0);
            Push(new StackItem { Kind = StackKind.Card,Owner = owner,Card = card,TargetTile = target,CounterId = counterId,
                Target = Valid(target) && !def.IsUnit ? Board[target].Unit : null,Effect = def.Effect,Value = def.Value,
                Steps = def.Steps,TargetMode = def.IsUnit ? null : def.ChosenTarget,Description = def.Name });
        }
        public int CommanderCost(int owner) => Players[owner].Commander.Definition.Cost+2*Players[owner].CommanderDeaths;
        public void DeployCommander(int owner,int target)
        {
            Live(); Require(owner == Priority,"Você não tem prioridade.");
            AnnounceCard(owner,Players[owner].Commander,target,-1); Players[owner].CommanderAvailable = false;
        }
        void Push(StackItem item)
        { item.Id = nextStackId++; Stack.Add(item); Priority = item.Owner; Passes = 0; Log.Add("Na pilha: "+item.Description); }
        public bool CanMove(int source,int target)
        {
            if (!OpenMain || !Valid(source) || !Valid(target)) return false;
            var unit = Board[source].Unit;
            return unit != null && unit.Owner == Active && unit.Crewed(Turn) && unit.Movement > 0 && Adjacent(source,target) && Board[target].Unit == null && (!Board[target].Capital || Board[target].Owner == Active);
        }
        public bool CanAttack(int source,int target)
        {
            if (!OpenMain || !Valid(source) || !Valid(target)) return false;
            var unit = Board[source].Unit; var tile = Board[target];
            return unit != null && unit.Owner == Active && unit.Crewed(Turn) && unit.Actions > 0 && source != target && Distance(source,target) <= unit.Range &&
                ((tile.Unit != null && tile.Unit.Owner != Active) || (tile.Capital && tile.Owner != Active) || (tile.Buildings.Count > 0 && tile.Owner != Active));
        }
        public void Move(int source,int target)
        {
            Main(); Require(CanMove(source,target),"Movimento inválido: escolha uma casa marcada MOVER.");
            var unit = Board[source].Unit; unit.Movement--; Board[source].Unit = null; Board[target].Unit = unit;
            if (Board[target].Terrain != null)
            {
                if (Board[target].Owner != Active) foreach (var building in Board[target].Buildings.ToArray()) DestroyBuilding(target,building);
                Board[target].Owner = Active;
            }
            Emit("move",source,target,Active,unit.Card.Definition); Log.Add(unit.Card.Name+" moveu uma casa."); QueueTerrain(target,TerrainTrigger.Enter);
        }
        public void Attack(int source,int target)
        {
            Main(); Require(CanAttack(source,target),"Ataque inválido: verifique alcance e ações restantes.");
            var unit = Board[source].Unit; unit.Actions--;
            if (Board[target].Unit == null && Board[target].Buildings.Count > 0)
            {
                Push(new StackItem { Kind = StackKind.StructureAttack, Owner = Active, Source = unit, TargetTile = target,
                    Building = Board[target].Buildings[0], Description = unit.Card.Name+" → atacar construção "+Board[target].Buildings[0].Card.Name }); return;
            }
            Push(new StackItem { Kind = StackKind.Attack,Owner = Active,Source = unit,Target = Board[target].Unit,TargetTile = target,
                Description = unit.Card.Name+" → ataque" });
        }
        public void ActivateCommand(int owner,int ability)
        {
            Main(); Require(owner == Active,"Comando só no seu turno.");
            Require(ability == 0 || ability == 1,"Comando desconhecido.");
            var commander = Board.Select(t => t.Unit).FirstOrDefault(u => u != null && u.Owner == owner && u.Commander);
            Require(commander != null && commander.Actions > 0 && !Players[owner].CommandUsed,"Comandante precisa estar no campo, ter uma ação e não ter usado comando neste turno.");
            Require(ability == 0 || Players[owner].Popularity >= 2,"Inspirar custa 2 de popularidade.");
            if (ability == 1) Players[owner].Popularity -= 2;
            commander.Actions--; Players[owner].CommandUsed = true;
            Push(new StackItem { Kind = StackKind.Command,Owner = owner,Source = commander,Command = ability,Description = ability == 0 ? "Comando: Reunir (+1 popularidade e cura 2)" : "Comando: Inspirar (+1 ataque até o final)" });
        }
        public void PassPriority()
        {
            Live(); Require(Phase == Phase.Main || Phase == Phase.End || Stack.Count > 0,"Avance as fases até a Principal para usar prioridade.");
            Log.Add(Players[Priority].Name+" passou prioridade."); Passes++;
            if (Passes < 2) { Priority = 1-Priority; return; }
            Passes = 0;
            if (Stack.Count > 0)
            {
                var item = Stack[Stack.Count-1]; Stack.RemoveAt(Stack.Count-1); Resolve(item); Priority = Active;
            }
            else if (Phase == Phase.Main) { Phase = Phase.End; Priority = Active; Log.Add("Ambos passaram: fase Final."); }
            else
            {
                foreach (var unit in Board.Where(t => t.Unit != null).SelectMany(t => new[]{t.Unit}.Concat(t.Unit.Passengers)))
                {
                    unit.BonusAttack = 0; unit.AttackPenalty = 0; unit.TemporaryShield = 0;
                    if (unit.StunnedThroughTurn <= Turn) unit.StunnedThroughTurn = 0;
                }
                Active = 1-Active; Turn++; BeginTurn();
            }
        }
        void FinishCard(StackItem item)
        {
            if (item.Kind != StackKind.Card || item.Card == null) return;
            if (item.Card.Definition.Kind == CardKind.Commander) Players[item.Owner].CommanderAvailable = true;
            else Players[item.Owner].Graveyard.Add(item.Card);
        }
        void Resolve(StackItem item)
        {
            Log.Add("Resolvendo: "+item.Description);
            if (ResolveJourney(item) || ResolveKingdom(item)) return;
            if (item.Kind == StackKind.Card && item.Card.Definition.IsUnit)
            {
                if (Valid(item.TargetTile) && Board[item.TargetTile].Unit == null && Board[item.TargetTile].Owner == item.Owner)
                {
                    var unit = new Unit(item.Owner,item.Card); Board[item.TargetTile].Unit = unit;
                    Emit("summon",item.TargetTile,item.TargetTile,item.Owner,item.Card.Definition);
                    QueueTerrain(item.TargetTile,TerrainTrigger.Enter);
                    if (item.Steps.Length > 0)
                        Push(new StackItem { Kind = StackKind.Trigger,Owner = item.Owner,Source = unit,Steps = item.Steps,Effect = item.Effect,Value = item.Value,Description = item.Card.Name+" / ao entrar" });
                }
                else { FinishCard(item); Log.Add("Entrada anulada: território ocupado ou perdeu o controle."); }
                return;
            }
            if (item.Kind == StackKind.Attack)
            {
                int from = Position(item.Source), to = item.Target != null ? Position(item.Target) : item.TargetTile;
                bool valid = from >= 0 && Valid(to) && item.Source.Owner == item.Owner && item.Source.Crewed(Turn) && Distance(from,to) <= item.Source.Range &&
                    (item.Target != null ? item.Target.Owner != item.Owner : Board[to].Capital && Board[to].Owner != item.Owner && Board[to].Unit == null);
                if (!valid) { Log.Add("Ataque perdeu o alvo ou a fonte."); return; }
                Emit("attack",from,to,item.Owner,item.Source.Card.Definition);
                int damage = item.Target != null ? Damage(item.Target,item.Source.Attack) : DamagePlayer(Board[to].Owner,item.Source.Attack);
                if (item.Source.Card.Definition.Trait == Trait.Lifesteal) HealPlayer(item.Owner,damage);
                return;
            }
            if (item.Kind == StackKind.Command)
            {
                if (item.Command == 0) { Players[item.Owner].Popularity++; HealPlayer(item.Owner,2); }
                else foreach (var tile in Board) if (tile.Unit != null && tile.Unit.Owner == item.Owner) tile.Unit.BonusAttack++;
                return;
            }
            if (!TargetStillValid(item)) Log.Add("O alvo escolhido ficou inválido: todos os efeitos da carta foram cancelados.");
            else foreach (var step in item.Steps)
            {
                if (Winner >= 0) break;
                ResolveStep(item,step);
            }
            FinishCard(item);
        }
        bool TargetStillValid(StackItem item)
        {
            if (item.TargetMode == null) return true;
            if (item.TargetMode == EffectTarget.EmptyOwnedTile)
                return Valid(item.TargetTile) && Board[item.TargetTile].Owner == item.Owner && Board[item.TargetTile].Unit == null;
            if (item.Target == null || Position(item.Target) < 0) return false;
            return item.TargetMode == EffectTarget.AnyUnit || (item.TargetMode == EffectTarget.AllyUnit ? item.Target.Owner == item.Owner : item.Target.Owner != item.Owner);
        }
        void ResolveStep(StackItem item,EffectStep step)
        {
            if (ResolveConfluence(item,step)) return;
            if (step.Operation == Effect.Counter)
            {
                var target = Stack.FirstOrDefault(s => s.Id == item.CounterId);
                if (target != null) { Stack.Remove(target); FinishCard(target); Log.Add("Anulado: "+target.Description); }
                else Log.Add("Anulação perdeu o alvo.");
                return;
            }
            if (step.Operation == Effect.Draw) { Draw(item.Owner,step.Value); return; }
            if (step.Operation == Effect.GainMana) { Players[item.Owner].Mana[step.Element] += step.Value; Log.Add("Ganhou "+step.Value+" mana "+Elements[step.Element]+"."); return; }
            if (step.Target == EffectTarget.Owner && step.Operation == Effect.Heal) { HealPlayer(item.Owner,step.Value); return; }
            if (step.Operation == Effect.SummonToken)
            {
                var definition = Catalog.Get(step.TokenId);
                if (definition != null && definition.Kind == CardKind.Token && Valid(item.TargetTile) && Board[item.TargetTile].Unit == null && Board[item.TargetTile].Owner == item.Owner)
                {
                    Board[item.TargetTile].Unit = new Unit(item.Owner,new Card(definition,"token-"+(nextStackId++)));
                    Emit("summon",item.TargetTile,item.TargetTile,item.Owner,definition);
                    QueueTerrain(item.TargetTile,TerrainTrigger.Enter);
                }
                return;
            }
            var targets = step.Target == EffectTarget.AllAllies || step.Target == EffectTarget.AllEnemies ? Board.Where(t => t.Unit != null && (step.Target == EffectTarget.AllAllies ? t.Unit.Owner == item.Owner : t.Unit.Owner != item.Owner)).Select(t => t.Unit).ToArray() :
                new[] { step.Target == EffectTarget.Source ? item.Source : item.Target };
            foreach (var unit in targets)
            {
                int pos = Position(unit); if (unit == null || pos < 0) continue;
                switch (step.Operation)
                {
                    case Effect.Damage: Damage(unit,step.Value); break;
                    case Effect.Heal:
                        int healed = Math.Min(step.Value,Math.Max(0,unit.Card.Definition.Life-unit.Life)); unit.Life += healed;
                        Emit("heal",pos,pos,unit.Owner,unit.Card.Definition,healed); break;
                    case Effect.Buff: unit.BonusAttack += step.Value; Emit("buff",pos,pos,unit.Owner,unit.Card.Definition,step.Value); break;
                    case Effect.Shield:
                        if (step.Duration == EffectDuration.EndOfTurn) unit.TemporaryShield += step.Value; else unit.Shield += step.Value;
                        Emit("shield",pos,pos,unit.Owner,unit.Card.Definition,step.Value); break;
                    case Effect.Stun:
                        unit.Movement = unit.Actions = 0; unit.StunnedThroughTurn = Math.Max(unit.StunnedThroughTurn,Turn+(unit.Owner == Active ? 2 : 1));
                        Emit("stun",pos,pos,unit.Owner,unit.Card.Definition); break;
                    case Effect.Return: case Effect.Exile: case Effect.Destroy: RemoveUnit(unit,step.Operation); break;
                }
            }
        }
        void RemoveUnit(Unit unit,Effect reason)
        {
            int pos = Position(unit); if (pos < 0) return;
            Board[pos].Unit = null;
            Evacuate(unit,pos);
            Players[unit.Owner].Graveyard.AddRange(unit.Equipment); unit.Equipment.Clear();
            if (unit.Card.Definition.Kind != CardKind.Token)
            {
                if (reason == Effect.Exile) { Players[unit.Owner].Exile.Add(unit.Card); if (unit.Commander) Players[unit.Owner].CommanderAvailable = false; }
                else if (unit.Commander)
                { Players[unit.Owner].CommanderAvailable = true; if (reason == Effect.Destroy) Players[unit.Owner].CommanderDeaths++; }
                else if (reason == Effect.Return) Players[unit.Owner].Hand.Add(unit.Card);
                else Players[unit.Owner].Graveyard.Add(unit.Card);
            }
            Emit("leave",pos,pos,unit.Owner,unit.Card.Definition);
            Log.Add(unit.Card.Name+(unit.Card.Definition.Kind == CardKind.Token ? " desapareceu do campo." : reason == Effect.Return ? " voltou à mão/zona de comando." : reason == Effect.Exile ? " foi exilado." : " foi destruído."));
        }
        int Damage(Unit unit,int amount)
        {
            int pos = Position(unit); amount = Math.Max(0,amount-unit.Armor);
            int absorb = Math.Min(unit.TemporaryShield,amount); unit.TemporaryShield -= absorb; amount -= absorb;
            int permanent = Math.Min(unit.Shield,amount); unit.Shield -= permanent; amount -= permanent;
            if (absorb+permanent > 0) Log.Add("Escudo absorveu "+(absorb+permanent)+" de dano.");
            int actual = Math.Min(unit.Life,amount); unit.Life -= amount; Emit("damage",pos,pos,unit.Owner,unit.Card.Definition,amount);
            if (unit.Life <= 0)
            {
                RemoveUnit(unit,Effect.Destroy);
            }
            return actual;
        }
        int DamagePlayer(int owner,int amount)
        {
            int actual = Math.Min(Players[owner].Life,amount); Players[owner].Life -= amount;
            Emit("damage",owner == 0 ? 5 : 115,owner == 0 ? 5 : 115,owner,null,amount);
            if (Players[owner].Life <= 0) Winner = 1-owner; return actual;
        }
        void HealPlayer(int owner,int value)
        {
            int healed = Math.Min(value,Math.Max(0,LifeLimit-Players[owner].Life)); Players[owner].Life += healed;
            Emit("heal",owner == 0 ? 5 : 115,owner == 0 ? 5 : 115,owner,null,healed);
        }
    }
}
