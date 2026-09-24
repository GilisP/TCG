using System.Collections.Generic;
using System.Linq;

namespace TCG
{
    public static class Expansion
    {
        static EffectStep E(Effect op,EffectTarget target,int value = 0,EffectDuration duration = EffectDuration.Instant) => new EffectStep(op,target,value,duration);
        public static void AddTo(List<CardDefinition> list)
        {
            list.AddRange(new[] {
                Spell("spark","Fagulha Sagaz",2,3,CardKind.Spell,"Causa 2 de dano a uma criatura inimiga; depois compra 1 carta.",E(Effect.Damage,EffectTarget.EnemyUnit,2),E(Effect.Draw,EffectTarget.Owner,1)),
                Spell("return","Retorno das Marés",2,2,CardKind.Trick,"Devolve uma criatura inimiga à mão do dono. Comandantes voltam à zona de comando sem aumento de custo.",E(Effect.Return,EffectTarget.EnemyUnit)),
                Spell("eclipse","Eclipse do Destino",4,1,CardKind.Spell,"Exila uma criatura inimiga. Um comandante exilado não pode ser invocado novamente nesta versão.",E(Effect.Exile,EffectTarget.EnemyUnit)),
                Spell("rupture","Ruptura das Raízes",3,5,CardKind.Spell,"Destrói uma criatura inimiga. Armadura e escudos não impedem destruição.",E(Effect.Destroy,EffectTarget.EnemyUnit)),
                Spell("rally","Brado da Aurora",1,0,CardKind.Trick,"Uma criatura sua recebe +2 de ataque até o final deste turno.",E(Effect.Buff,EffectTarget.AllyUnit,2,EffectDuration.EndOfTurn)),
                Spell("shield","Escudo Solar",1,0,CardKind.Trick,"Dá 3 de escudo a uma criatura sua até o final deste turno. Escudo absorve dano e é consumido.",E(Effect.Shield,EffectTarget.AllyUnit,3,EffectDuration.EndOfTurn)),
                Spell("frost","Prisão de Geada",2,2,CardKind.Trick,"Congela uma criatura inimiga: zera movimento e ações até terminar o próximo turno do controlador. Não anula ataques já anunciados.",E(Effect.Stun,EffectTarget.EnemyUnit,1,EffectDuration.ThroughNextControllerTurn)),
                Spell("reserve","Reserva dos Ventos",1,4,CardKind.Spell,"Adiciona 3 mana incolor à sua reserva.",E(Effect.GainMana,EffectTarget.Owner,3)),
                Spell("clay","Vigia de Argila",1,5,CardKind.Spell,"Cria uma ficha Vigia 1/2, com 1 movimento, numa casa sua desocupada. Fichas desaparecem ao sair do campo.",new EffectStep(Effect.SummonToken,EffectTarget.EmptyOwnedTile) { TokenId = "token-clay" }),
                Spell("refuge","Clarão do Refúgio",3,0,CardKind.Spell,"Recupera até 2 de vida de cada criatura sua; depois compra 1 carta. Não exige alvo.",E(Effect.Heal,EffectTarget.AllAllies,2),E(Effect.Draw,EffectTarget.Owner,1)),
                Spell("hunger","Chama Voraz",3,3,CardKind.Spell,"Causa 4 de dano a uma criatura inimiga; depois recupera 2 de vida do seu jogador.",E(Effect.Damage,EffectTarget.EnemyUnit,4),E(Effect.Heal,EffectTarget.Owner,2)),
                Spell("shard","Estilhaço Instantâneo",1,4,CardKind.Trick,"Causa 1 de dano a uma criatura inimiga. Pode responder a uma ação na pilha.",E(Effect.Damage,EffectTarget.EnemyUnit,1)),
                Creature("sentinel","Sentinela da Aurora",2,0,1,4,2,"Ao entrar: recebe 2 de escudo permanente, consumido ao absorver dano.",E(Effect.Shield,EffectTarget.Source,2,EffectDuration.Permanent)),
                Creature("sower","Semeadora da Vida",3,5,2,4,2,"Ao entrar: seu jogador recupera 3 de vida e compra 1 carta.",E(Effect.Heal,EffectTarget.Owner,3),E(Effect.Draw,EffectTarget.Owner,1)),
                Creature("cartographer","Cartógrafo dos Ventos",2,4,1,3,3,"Ao entrar: gera 1 mana incolor. Possui 3 movimentos.",E(Effect.GainMana,EffectTarget.Owner,1)),
                new CardDefinition { Id = "token-clay",Name = "Vigia de Argila",Kind = CardKind.Token,Element = 5,Attack = 1,Life = 2,Movement = 1,Art = "2",Text = "Ficha 1/2. Desaparece ao sair do campo; não pode entrar em decks." }
            });
        }
        static CardDefinition Spell(string id,string name,int cost,int element,CardKind kind,string text,params EffectStep[] steps) =>
            new CardDefinition { Id = "core-"+id,Name = name,Cost = cost,Element = element,Kind = kind,Text = text,Effects = steps,Art = steps[0].Operation.ToString().ToLowerInvariant() };
        static CardDefinition Creature(string id,string name,int cost,int element,int attack,int life,int movement,string text,params EffectStep[] steps) =>
            new CardDefinition { Id = "core-"+id,Name = name,Cost = cost,Element = element,Kind = CardKind.Creature,Attack = attack,Life = life,Movement = movement,Text = text,Effects = steps,Art = id == "sentinel" ? "2" : "5" };
        public static DeckList Starter(int style)
        {
            var deck = Catalog.DefaultDeck();
            deck.name = new[] { "Aliança dos Reinos", "Marcha das Brasas", "Conselho das Marés" }[style%3];
            deck.commander = style == 2 ? "commander-lua" : "commander-sol";
            var cards = Catalog.All.Where(c => c.MainDeckEligible);
            if (style == 1) cards = cards.OrderBy(c => c.Id.StartsWith("confluence-") ? 0 : c.Id.StartsWith("kingdom-") ? 1 : c.Id.StartsWith("core-") ? 2 : 3).ThenByDescending(c => c.IsUnit ? c.Attack : c.Steps.Any(s => s.Operation == Effect.Damage) ? 5 : 0);
            else if (style == 2) cards = cards.OrderBy(c => c.Id.StartsWith("confluence-") ? 0 : c.Id.StartsWith("kingdom-") ? 1 : c.Id.StartsWith("core-") ? 2 : 3).ThenBy(c => c.Kind == CardKind.Trick ? 0 : c.Kind == CardKind.Spell ? 1 : 2);
            else cards = cards.OrderBy(c => c.Id.StartsWith("confluence-") ? 0 : c.Id.StartsWith("kingdom-") ? 1 : c.Id.StartsWith("core-") ? 2 : 3);
            deck.main = cards.Take(100).Select(c => c.Id).ToList(); return deck;
        }
    }
}
