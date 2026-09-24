using System;
using System.Linq;
using System.Collections.Generic;
namespace TCG
{
    public sealed partial class Game
    {
        public void ChooseTerrainMana(int index,int element)
        {
            Main();Require(Valid(index)&&Board[index].Owner==Active&&Board[index].Terrain!=null,"Escolha um terreno seu.");
            Require(Board[index].Terrain.Definition.ProductionColors.Contains(element),"Este terreno não produz esse elemento.");
            Board[index].ManaChoice=element;Log.Add(Board[index].Terrain.Name+": próxima geração em "+Elements[element]+". A reserva atual não muda.");
        }
        bool ResolveConfluence(StackItem item,EffectStep step)
        {
            int player=step.Target==EffectTarget.EnemyPlayer?1-item.Owner:item.Owner;
            if(step.Operation==Effect.Damage&&step.Target==EffectTarget.EnemyPlayer){DamagePlayer(player,step.Value);return true;}
            if(step.Operation==Effect.GainPopularity){Players[item.Owner].Popularity+=step.Value;Log.Add("Ganhou "+step.Value+" popularidade.");return true;}
            if(step.Operation==Effect.Discard)
            {
                var hand=Players[player].Hand;int count=Math.Min(step.Value,hand.Count);
                for(int i=0;i<count;i++){var card=hand[hand.Count-1];hand.RemoveAt(hand.Count-1);Players[player].Graveyard.Add(card);}
                Log.Add(Players[player].Name+" descartou "+count+" carta(s) do final da mão.");return true;
            }
            if(step.Operation==Effect.Recover)
            {
                var p=Players[item.Owner];int count=Math.Min(step.Value,p.Graveyard.Count);
                for(int i=0;i<count;i++){var card=p.Graveyard[p.Graveyard.Count-1];p.Graveyard.RemoveAt(p.Graveyard.Count-1);p.Hand.Add(card);}
                Log.Add("Recuperou "+count+" carta(s) recentes do cemitério para a mão.");return true;
            }
            if(step.Operation!=Effect.Ready&&step.Operation!=Effect.Weaken&&step.Operation!=Effect.Cleanse)return false;
            var targets=step.Target==EffectTarget.AllAllies||step.Target==EffectTarget.AllEnemies?
                Board.Where(t=>t.Unit!=null&&(step.Target==EffectTarget.AllAllies?t.Unit.Owner==item.Owner:t.Unit.Owner!=item.Owner)).Select(t=>t.Unit).ToArray():
                new[]{step.Target==EffectTarget.Source?item.Source:item.Target};
            foreach(var u in targets)
            {
                int pos=Position(u);if(pos<0)continue;
                if(step.Operation==Effect.Weaken)u.AttackPenalty+=step.Value;
                else if(step.Operation==Effect.Cleanse){u.StunnedThroughTurn=0;u.AttackPenalty=0;u.BonusAttack=Math.Max(0,u.BonusAttack);}
                else if(u.StunnedThroughTurn<Turn){u.Actions+=step.Value;u.Movement+=step.Value;}
                Emit("buff",pos,pos,u.Owner,u.Card.Definition,step.Value);
            }
            return true;
        }
    }
    public static class ConfluenceCards
    {
        static EffectStep E(Effect op,EffectTarget target,int value=0,EffectDuration duration=EffectDuration.Instant)=>new EffectStep(op,target,value,duration);
        static CardDefinition Creature(string id,string name,int generic,int[] colors,int attack,int life,string illustration,string text,params EffectStep[] steps)
        {var c=new CardDefinition{Id="confluence-"+id,Name=name,Kind=CardKind.Creature,Element=colors[0],Cost=generic,Attack=attack,Life=life,Illustration=illustration,Art="2",Text=text,Effects=steps};foreach(int e in colors)c.ColoredCost[e]++;return c;}
        static CardDefinition Spell(string id,string name,int generic,int[] colors,bool trick,string text,params EffectStep[] steps)
        {var c=new CardDefinition{Id="confluence-"+id,Name=name,Kind=trick?CardKind.Trick:CardKind.Spell,Element=colors[0],Cost=generic,Illustration="arcane-clash",Art="draw",Text=text,Effects=steps};foreach(int e in colors)c.ColoredCost[e]++;return c;}
        public static void AddTo(List<CardDefinition> cards)
        {
            string[] names={"Claustro do Eclipse","Estuário da Aurora","Forja do Meio-dia","Ponte dos Alísios","Bosque da Coroa","Lago das Duas Luas","Caldeira do Crepúsculo","Torre da Névoa","Raízes do Anoitecer","Fontes de Obsidiana","Arquipélago Suspenso","Jardim das Marés","Desfiladeiro do Trovão","Vulcão das Raízes","Planície dos Sussurros"};
            int n=0;
            for(int a=0;a<6;a++)for(int b=a+1;b<6;b++)
                cards.Add(new CardDefinition{Id=$"confluence-land-{a}-{b}",Name=names[n++],Kind=CardKind.Terrain,Element=a,ManaColors=new[]{a,b},Housing=1,Illustration="confluence",Art="land",Text="Gera 1 mana de "+Catalog.Elements[a]+" OU "+Catalog.Elements[b]+" no seu início. Escolha a próxima cor em Domínios. Capacidade 1."});
            cards.Add(new CardDefinition{Id="confluence-land-dawn",Name="Encontro da Aurora",Kind=CardKind.Terrain,Element=0,ManaColors=new[]{0,2,5},Housing=0,Illustration="realm-reference",Art="land",Text="Gera 1 Sol, Água OU Terra. Escolha a próxima cor em Domínios. Não comporta construções."});
            cards.Add(new CardDefinition{Id="confluence-land-storm",Name="Fenda da Tempestade",Kind=CardKind.Terrain,Element=1,ManaColors=new[]{1,3,4},Housing=0,Illustration="confluence",Art="land",Text="Gera 1 Lua, Fogo OU Ar. Escolha a próxima cor em Domínios. Não comporta construções."});
            cards.Add(Creature("dawn-guard","Guardiã dos Juramentos",1,new[]{0},2,4,"guardian","Ao entrar: recebe escudo 1 permanente e ganha 1 popularidade.",E(Effect.Shield,EffectTarget.Source,1,EffectDuration.Permanent),E(Effect.GainPopularity,EffectTarget.Owner,1)));
            cards.Add(Creature("moon-seer","Vidente do Lago Lunar",2,new[]{1},1,4,"moon-mage","Ao entrar: compra 1 carta; o oponente descarta a última carta da mão.",E(Effect.Draw,EffectTarget.Owner,1),E(Effect.Discard,EffectTarget.EnemyPlayer,1)));
            cards.Add(Creature("tide-healer","Curadora das Fontes",1,new[]{2},1,4,"moon-mage","Ao entrar: remove congelamento e redução de ataque dos aliados; cura 2 de cada um. Limpar não renova ações.",E(Effect.Cleanse,EffectTarget.AllAllies),E(Effect.Heal,EffectTarget.AllAllies,2)));
            cards.Add(Creature("ember-dragon","Dragão da Caldeira Antiga",3,new[]{3,3},5,5,"dragon","Ao entrar: causa 1 de dano a todas as unidades inimigas.",E(Effect.Damage,EffectTarget.AllEnemies,1)));
            cards.Add(Creature("wind-captain","Capitã dos Alísios",2,new[]{4},2,3,"guardian","Ao entrar: recebe 1 ação e 1 movimento adicionais.",E(Effect.Ready,EffectTarget.Source,1)));
            var root=Creature("root-warden","Sentinela do Carvalho",2,new[]{5},3,5,"guardian","Armadura 1. Ao entrar: cura 2 do jogador.",E(Effect.Heal,EffectTarget.Owner,2));root.Trait=Trait.Armor;cards.Add(root);
            cards.Add(Creature("sunroot","Cavaleira do Bosque Dourado",1,new[]{0,5},3,4,"guardian","Ao entrar: cura 1 de cada aliado e recebe escudo 2 permanente.",E(Effect.Heal,EffectTarget.AllAllies,1),E(Effect.Shield,EffectTarget.Source,2,EffectDuration.Permanent)));
            cards.Add(Creature("moonwater","Oráculo das Duas Margens",1,new[]{1,2},2,3,"moon-mage","Ao entrar: compra 1 e recupera a carta mais recente do cemitério para a mão.",E(Effect.Draw,EffectTarget.Owner,1),E(Effect.Recover,EffectTarget.Owner,1)));
            cards.Add(Creature("storm-dragon","Draco do Trovão Rubro",2,new[]{3,4},4,4,"dragon","Ao entrar: causa 2 de dano direto ao jogador inimigo.",E(Effect.Damage,EffectTarget.EnemyPlayer,2)));
            cards.Add(Creature("ash-muse","Arauto do Eclipse Ardente",1,new[]{1,3},3,3,"dragon","Ao entrar: oponente descarta a última carta; você ganha 1 popularidade.",E(Effect.Discard,EffectTarget.EnemyPlayer,1),E(Effect.GainPopularity,EffectTarget.Owner,1)));
            cards.Add(Creature("river-sage","Sábia das Raízes Submersas",1,new[]{2,5},2,4,"moon-mage","Ao entrar: recupera 1 carta recente do cemitério e cura 2 do jogador.",E(Effect.Recover,EffectTarget.Owner,1),E(Effect.Heal,EffectTarget.Owner,2)));
            cards.Add(Creature("dawn-herald","Porta-estandarte da Aurora",1,new[]{0,4},2,3,"guardian","Ao entrar: todos os aliados recebem +1 ataque e escudo 1 até o final.",E(Effect.Buff,EffectTarget.AllAllies,1,EffectDuration.EndOfTurn),E(Effect.Shield,EffectTarget.AllAllies,1,EffectDuration.EndOfTurn)));
            cards.Add(Spell("meteor","Chuva de Brasas",2,new[]{3,3},false,"Causa 2 de dano a todas as unidades inimigas.",E(Effect.Damage,EffectTarget.AllEnemies,2)));
            cards.Add(Spell("mind-rift","Fenda da Memória",1,new[]{1},false,"O oponente descarta até 2 cartas do final da mão.",E(Effect.Discard,EffectTarget.EnemyPlayer,2)));
            cards.Add(Spell("reclaim","Memória das Raízes",1,new[]{5},false,"Recupera até 2 cartas recentes do seu cemitério para a mão. Esta magia ainda está na pilha.",E(Effect.Recover,EffectTarget.Owner,2)));
            cards.Add(Spell("rally","Chamado dos Estandartes",1,new[]{0},false,"Ganha 2 popularidade e compra 1 carta.",E(Effect.GainPopularity,EffectTarget.Owner,2),E(Effect.Draw,EffectTarget.Owner,1)));
            cards.Add(Spell("haste","Segundo Fôlego",0,new[]{4},true,"Unidade sua não congelada recebe 1 ação e 1 movimento extras.",E(Effect.Ready,EffectTarget.AllyUnit,1)));
            cards.Add(Spell("wither","Peso do Crepúsculo",0,new[]{1},true,"Unidade inimiga recebe -2 ataque até o final. Ataque mínimo zero.",E(Effect.Weaken,EffectTarget.EnemyUnit,2,EffectDuration.EndOfTurn)));
            cards.Add(Spell("purify","Águas da Libertação",0,new[]{2},true,"Remove congelamento e redução de ataque de uma aliada; cura 3. Não renova ações gastas.",E(Effect.Cleanse,EffectTarget.AllyUnit),E(Effect.Heal,EffectTarget.AllyUnit,3)));
            cards.Add(Spell("eclipse-drain","Tributo do Eclipse",1,new[]{1,3},false,"Causa 3 de dano direto ao jogador inimigo e cura 3 do seu jogador.",E(Effect.Damage,EffectTarget.EnemyPlayer,3),E(Effect.Heal,EffectTarget.Owner,3)));
            cards.Add(Spell("shieldwall","Muralha dos Juramentos",1,new[]{0,2},true,"Todos os aliados recebem escudo 2 até o final; compre 1 carta.",E(Effect.Shield,EffectTarget.AllAllies,2,EffectDuration.EndOfTurn),E(Effect.Draw,EffectTarget.Owner,1)));
            cards.Add(Spell("winter","Inverno sem Voz",2,new[]{1,2},false,"Congela todas as unidades inimigas até o final do próximo turno delas.",E(Effect.Stun,EffectTarget.AllEnemies,1,EffectDuration.ThroughNextControllerTurn)));
            cards.Add(Spell("sun-token","Juramento Materializado",1,new[]{0},false,"Cria um Escudeiro Solar 2/3 em casa sua vazia.",new EffectStep(Effect.SummonToken,EffectTarget.EmptyOwnedTile){TokenId="confluence-token-sun"}));
            cards.Add(Spell("mist-token","Forma da Névoa",0,new[]{1,2},false,"Cria uma Aparição das Marés 1/2, movimento 3, em casa sua vazia; compra 1.",new EffectStep(Effect.SummonToken,EffectTarget.EmptyOwnedTile){TokenId="confluence-token-mist"},E(Effect.Draw,EffectTarget.Owner,1)));
            cards.Add(new CardDefinition{Id="confluence-token-sun",Name="Escudeiro Solar",Kind=CardKind.Token,Element=0,Attack=2,Life=3,Illustration="guardian",Art="2",Text="Ficha 2/3. Desaparece ao sair do campo."});
            cards.Add(new CardDefinition{Id="confluence-token-mist",Name="Aparição das Marés",Kind=CardKind.Token,Element=2,Attack=1,Life=2,Movement=3,Illustration="moon-mage",Art="5",Text="Ficha 1/2, movimento 3. Desaparece ao sair do campo."});
        }
    }
    public static class EffectRecipes
    {
        public static readonly string[] Names={"Dano em inimigo","Cura de aliado","Comprar cartas","Anular ação","Devolver inimigo","Exilar inimigo","Destruir inimigo","Fortalecer aliado","Escudo temporário","Congelar inimigo","Gerar mana","Invocar ficha","Descartar do oponente","Recuperar do cemitério","Ganhar popularidade","Ação e movimento extras","Enfraquecer inimigo","Limpar estados negativos","Dano em todos os inimigos","Dano direto ao jogador","Dano direto + cura","Dano + compra","Fortalecer exército","Limpar + curar","Entrada: escudo e compra"};
        public static EffectStep[] Create(int index)
        {
            EffectStep E(Effect op,EffectTarget target,int n=1,EffectDuration d=EffectDuration.Instant)=>new EffectStep(op,target,n,d);
            switch(index)
            {
                case 0:return new[]{E(Effect.Damage,EffectTarget.EnemyUnit,3)};
                case 1:return new[]{E(Effect.Heal,EffectTarget.AllyUnit,3)};
                case 2:return new[]{E(Effect.Draw,EffectTarget.Owner,2)};
                case 3:return new[]{E(Effect.Counter,EffectTarget.Stack)};
                case 4:return new[]{E(Effect.Return,EffectTarget.EnemyUnit)};
                case 5:return new[]{E(Effect.Exile,EffectTarget.EnemyUnit)};
                case 6:return new[]{E(Effect.Destroy,EffectTarget.EnemyUnit)};
                case 7:return new[]{E(Effect.Buff,EffectTarget.AllyUnit,2,EffectDuration.EndOfTurn)};
                case 8:return new[]{E(Effect.Shield,EffectTarget.AllyUnit,3,EffectDuration.EndOfTurn)};
                case 9:return new[]{E(Effect.Stun,EffectTarget.EnemyUnit,1,EffectDuration.ThroughNextControllerTurn)};
                case 10:return new[]{E(Effect.GainMana,EffectTarget.Owner,3)};
                case 11:return new[]{new EffectStep(Effect.SummonToken,EffectTarget.EmptyOwnedTile){TokenId="token-clay"}};
                case 12:return new[]{E(Effect.Discard,EffectTarget.EnemyPlayer,1)};
                case 13:return new[]{E(Effect.Recover,EffectTarget.Owner,1)};
                case 14:return new[]{E(Effect.GainPopularity,EffectTarget.Owner,2)};
                case 15:return new[]{E(Effect.Ready,EffectTarget.AllyUnit,1)};
                case 16:return new[]{E(Effect.Weaken,EffectTarget.EnemyUnit,2,EffectDuration.EndOfTurn)};
                case 17:return new[]{E(Effect.Cleanse,EffectTarget.AllyUnit)};
                case 18:return new[]{E(Effect.Damage,EffectTarget.AllEnemies,2)};
                case 19:return new[]{E(Effect.Damage,EffectTarget.EnemyPlayer,3)};
                case 20:return new[]{E(Effect.Damage,EffectTarget.EnemyPlayer,3),E(Effect.Heal,EffectTarget.Owner,3)};
                case 21:return new[]{E(Effect.Damage,EffectTarget.EnemyUnit,3),E(Effect.Draw,EffectTarget.Owner,1)};
                case 22:return new[]{E(Effect.Buff,EffectTarget.AllAllies,1,EffectDuration.EndOfTurn)};
                case 23:return new[]{E(Effect.Cleanse,EffectTarget.AllyUnit),E(Effect.Heal,EffectTarget.AllyUnit,3)};
                case 24:return new[]{E(Effect.Shield,EffectTarget.Source,2,EffectDuration.Permanent),E(Effect.Draw,EffectTarget.Owner,1)};
                default:throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }
}
