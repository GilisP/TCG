using System;
using System.Collections.Generic;
using System.Linq;

namespace TCG
{
    public enum CardKind { Creature, Spell, Trick, Terrain, Commander, Token, Equipment, Building, Vehicle }
    public enum Effect { None, Damage, Heal, Draw, Counter, Return, Exile, Destroy, Buff, Shield, Stun, GainMana, SummonToken, Discard, Recover, GainPopularity, Ready, Weaken, Cleanse }
    public enum EffectTarget { Owner, EnemyUnit, AllyUnit, AnyUnit, Source, AllAllies, EmptyOwnedTile, Stack, AllEnemies, EnemyPlayer }
    public enum EffectDuration { Instant, EndOfTurn, Permanent, ThroughNextControllerTurn }
    [Serializable]
    public sealed class EffectStep
    {
        public Effect Operation;
        public EffectTarget Target;
        public int Value, Element = 6;
        public EffectDuration Duration;
        public string TokenId;
        public EffectStep(Effect operation,EffectTarget target,int value = 0,EffectDuration duration = EffectDuration.Instant)
        { Operation = operation; Target = target; Value = value; Duration = duration; }
    }
    public enum TerrainTrigger { None, TurnStart, Enter }
    public enum Trait { None, Armor, Lifesteal }
    [Serializable]
    public sealed class CardDefinition
    {
        public string Id, Name, Text, Art;
        public string Illustration;
        public int[] ManaColors = Array.Empty<int>();
        public int[] ProductionColors => ManaColors != null && ManaColors.Length > 0 ? ManaColors : new[]{Element};
        public string ColorLabel => Kind == CardKind.Terrain ? string.Join(" / ",ProductionColors.Select(e => Catalog.Elements[e])) : Catalog.Elements[Element];
        public CardKind Kind;
        public Effect Effect;
        public EffectStep[] Effects;
        public Trait Trait;
        public int Element = 6, Cost, Attack, Life, Movement = 2, Range = 1, Value;
        public int[] ColoredCost = new int[6];
        public int EquipmentAttack, EquipmentArmor, EquipmentMovement;
        public int Housing = 2, HousingUse = 1, ExtraMana, ActivationCost;
        public bool Autonomous;
        public int BuildingLife = 5, Seats = 2, CrewRequired = 1;
        public TerrainTrigger AutomaticTrigger;
        public EffectStep[] AutomaticEffects = Array.Empty<EffectStep>();
        public int TotalCost => Cost+(ColoredCost?.Sum() ?? 0);
        public string CostLabel => string.Join(" + ",new[] { Cost > 0 ? Cost+" genérica" : null }.Concat(
            Enumerable.Range(0,6).Where(e => ColoredCost != null && ColoredCost[e] > 0).Select(e => ColoredCost[e]+" "+Catalog.Elements[e])).Where(s => s != null).DefaultIfEmpty("0"));
        public bool IsUnit => Kind == CardKind.Creature || Kind == CardKind.Commander || Kind == CardKind.Token || Kind == CardKind.Vehicle;
        public bool MainDeckEligible => Kind == CardKind.Creature || Kind == CardKind.Spell || Kind == CardKind.Trick || Kind == CardKind.Equipment || Kind == CardKind.Building || Kind == CardKind.Vehicle;
        EffectStep[] legacySteps;
        public EffectStep[] Steps => Effects ?? (legacySteps ?? (legacySteps = Effect == Effect.None ? Array.Empty<EffectStep>() : new[] {
            new EffectStep(Effect,Effect == Effect.Counter ? EffectTarget.Stack : Effect == Effect.Draw || IsUnit ? EffectTarget.Owner : Effect == Effect.Damage ? EffectTarget.EnemyUnit : EffectTarget.AllyUnit,Value) }));
        public EffectTarget? ChosenTarget => Steps.Where(s => s.Target == EffectTarget.EnemyUnit || s.Target == EffectTarget.AllyUnit || s.Target == EffectTarget.AnyUnit || s.Target == EffectTarget.EmptyOwnedTile).Select(s => (EffectTarget?)s.Target).FirstOrDefault();
        public bool NeedsBoardTarget => IsUnit || Kind == CardKind.Equipment || Kind == CardKind.Building || ChosenTarget != null;
        public bool NeedsStackTarget => Steps.Any(s => s.Target == EffectTarget.Stack);
        public string TypeName => Kind == CardKind.Vehicle ? "Veículo" : Kind == CardKind.Equipment ? "Equipamento" : Kind == CardKind.Building ? "Construção" : Kind == CardKind.Creature ? "Criatura" : Kind == CardKind.Spell ? "Feitiço" : Kind == CardKind.Trick ? "Truque" : Kind == CardKind.Terrain ? "Terreno" : Kind == CardKind.Token ? "Ficha" : "Comandante";
    }
    [Serializable]
    public sealed class DeckList
    {
        public int version = 1;
        public string name = "Meu deck", commander;
        public List<string> main = new List<string>(), terrains = new List<string>();
        public DeckList Copy() => new DeckList { version = version, name = name, commander = commander, main = new List<string>(main), terrains = new List<string>(terrains) };
    }
    public static class Catalog
    {
        public static readonly string[] Elements = { "Sol", "Lua", "Água", "Fogo", "Ar", "Terra", "Incolor" };
        public static readonly List<CardDefinition> All = Build();
        static readonly Dictionary<string,CardDefinition> ById = All.ToDictionary(c => c.Id);
        public static CardDefinition Get(string id) => id != null && ById.TryGetValue(id,out var card) ? card : null;
        public static void RegisterCustom(CardDefinition card)
        {
            var errors = CardAuthoring.Validate(card);
            if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n",errors));
            if (!card.Id.StartsWith("custom-")) throw new InvalidOperationException("ID reservado ao catálogo original.");
            var old = Get(card.Id); if (old != null) All.Remove(old);
            All.Add(card); ById[card.Id] = card;
        }
        public static List<string> ValidateEffectDefinitions()
        {
            return All.SelectMany(c => CardAuthoring.Validate(c)).ToList();
        }
        static List<CardDefinition> Build()
        {
            var list = new List<CardDefinition>();
            string[] names = { "Batedor", "Explorador", "Guardião", "Arqueiro", "Curandeira", "Sábio", "Colosso", "Ceifador" };
            string[] lands = { "Aurora", "Crepúsculo", "Marés", "Brasas", "Ventos", "Raízes", "Horizonte", "Estrelas", "Vale", "Ruínas", "Montanhas", "Fronteira" };
            int[] costs = {1,1,2,2,2,3,3,3}, attacks = {2,1,2,2,1,1,4,3}, lives = {3,2,5,2,3,3,5,3};
            for (int n = 0; n < 12; n++)
            {
                for (int a = 0; a < 8; a++)
                {
                    var c = new CardDefinition { Id = $"unit-{a}-{n}", Name = names[a]+" de "+lands[n], Kind = CardKind.Creature,
                        Element = a%6, Cost = costs[a], Attack = attacks[a], Life = lives[a], Movement = a == 1 ? 3 : a == 6 ? 1 : 2,
                        Range = a == 3 ? 2 : 1, Art = a.ToString(), Trait = a == 2 ? Trait.Armor : a == 7 ? Trait.Lifesteal : Trait.None,
                        Effect = a == 4 ? Effect.Heal : a == 5 ? Effect.Draw : Effect.None, Value = a == 4 ? 2 : a == 5 ? 1 : 0 };
                    c.Text = a == 0 ? "Infantaria. Sem habilidade especial." : a == 1 ? "Exploração: anda até 3 casas por turno." :
                        a == 2 ? "Armadura: reduz cada dano recebido em 1." : a == 3 ? "Alcance 2: ataca a até 2 casas de distância." :
                        a == 4 ? "Ao entrar: recupera 2 de vida do seu jogador." : a == 5 ? "Ao entrar: compra 1 carta." :
                        a == 6 ? "Força bruta: ataque 4; apenas 1 movimento." : "Drenar: seu jogador recupera o dano causado por esta criatura.";
                    list.Add(c);
                }
                if (n < 8)
                {
                    list.Add(new CardDefinition { Id = "fire-"+n, Name = "Chama de "+lands[n], Kind = CardKind.Spell, Cost = 2, Element = 3, Effect = Effect.Damage, Value = 3, Art = "fire", Text = "Causa 3 de dano a uma criatura inimiga. Só na sua fase principal." });
                    list.Add(new CardDefinition { Id = "heal-"+n, Name = "Amparo de "+lands[n], Kind = CardKind.Trick, Cost = 1, Element = 2, Effect = Effect.Heal, Value = 3, Art = "heal", Text = "Recupera até 3 de vida de uma criatura sua. Pode responder na pilha." });
                    list.Add(new CardDefinition { Id = "draw-"+n, Name = "Estudo de "+lands[n], Kind = CardKind.Spell, Cost = 2, Element = 1, Effect = Effect.Draw, Value = 2, Art = "draw", Text = "Compra 2 cartas. Não exige alvo. Só na sua fase principal." });
                    list.Add(new CardDefinition { Id = "counter-"+n, Name = "Negação de "+lands[n], Kind = CardKind.Trick, Cost = 1, Element = 4, Effect = Effect.Counter, Value = 1, Art = "counter", Text = "Anula uma ação escolhida na pilha. Custos pagos não são devolvidos." });
                }
            }
            for (int i = 0; i < 56; i++) list.Add(new CardDefinition { Id = "land-"+i, Name = Catalog.Elements[i%7]+" / Domínio "+(i/7+1), Kind = CardKind.Terrain, Element = i%7, Art = "land", Text = "Gera 1 mana deste elemento no início do seu turno." });
            list.Add(new CardDefinition { Id = "commander-sol", Name = "Aurel, Voz do Sol", Kind = CardKind.Commander, Element = 0, Cost = 3, Attack = 3, Life = 6, Art = "c", Text = "Comando: +1 popularidade e cura 2 do jogador; ou gasta 2 popularidade para dar +1 ataque aos aliados até o fim do turno. Uma ativação por turno, custa 1 ação." });
            list.Add(new CardDefinition { Id = "commander-lua", Name = "Selene, Voz da Lua", Kind = CardKind.Commander, Element = 1, Cost = 3, Attack = 2, Life = 7, Range = 2, Art = "c", Text = "Comando: +1 popularidade e cura 2 do jogador; ou gasta 2 popularidade para dar +1 ataque aos aliados até o fim do turno. Uma ativação por turno, custa 1 ação." });
            Expansion.AddTo(list);
            KingdomCards.AddTo(list);
            JourneyCards.AddTo(list);
            ConfluenceCards.AddTo(list);
            return list;
        }
        public static DeckList DefaultDeck(int owner = 0) => new DeckList {
            name = owner == 0 ? "Exército do Sol" : "Exército da Lua", commander = owner == 0 ? "commander-sol" : "commander-lua",
            main = All.Where(c => c.MainDeckEligible).OrderBy(c => c.Id.StartsWith("confluence-") ? 0 : c.Id.StartsWith("kingdom-") ? 1 : c.Id.StartsWith("core-") ? 2 : 3).Take(100).Select(c => c.Id).ToList(),
            terrains = All.Where(c => c.Kind == CardKind.Terrain).OrderBy(c => c.Id.StartsWith("confluence-") ? 0 : c.Id.StartsWith("kingdom-") ? 1 : 2).Take(50).Select(c => c.Id).ToList() };
        public static List<string> Validate(DeckList deck,bool requireComplete = true,Func<string,CardDefinition> resolve = null)
        {
            var errors = new List<string>();
            if (deck == null) { errors.Add("Deck ausente."); return errors; }
            if (deck.version != 1) errors.Add("Versão de deck não suportada.");
            if (deck.main == null || (requireComplete && deck.main.Count != 100)) errors.Add("O deck principal precisa de exatamente 100 cartas.");
            if (deck.terrains == null || (requireComplete && deck.terrains.Count != 50)) errors.Add("O deck de terrenos precisa de exatamente 50 cartas.");
            if ((resolve ?? Get)(deck.commander)?.Kind != CardKind.Commander) errors.Add("Escolha 1 comandante válido.");
            foreach (var pair in new[] { Tuple.Create(deck.main,false),Tuple.Create(deck.terrains,true) })
            {
                if (pair.Item1 == null) continue;
                if (pair.Item1.Distinct().Count() != pair.Item1.Count) errors.Add("Não é permitido repetir cartas no mesmo deck.");
                foreach (var id in pair.Item1)
                {
                    var card = (resolve ?? Get)(id);
                    if (card == null) { errors.Add("Carta desconhecida: "+(id ?? "sem ID")); continue; }
                    bool terrain = card.Kind == CardKind.Terrain;
                    if (pair.Item2 ? !terrain : !card.MainDeckEligible) errors.Add("Carta na zona errada: "+card.Name);
                }
            }
            return errors.Distinct().ToList();
        }
    }
}
