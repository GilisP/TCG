using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
    [Serializable] public sealed class MatchNumbers
    {
        public string id; public bool won; public int cards, terrains, turns;
        public MatchNumbers Copy()=> (MatchNumbers)MemberwiseClone();
    }
    [Serializable] public sealed class PlayerProgress
    {
        public int completed,wins,cards,terrains,recordCards,recordTerrains,recordTurns;
        public List<MatchNumbers> history=new List<MatchNumbers>();
        public List<string> recorded=new List<string>(),claimed=new List<string>();
        public int boosters;
    }
    [Serializable] public sealed class MissionDefinition
    {
        public string id,title,metric; public int target,coins,boosters;
    }
    [Serializable] public sealed class MissionCatalog { public MissionDefinition[] missions=Array.Empty<MissionDefinition>(); }
    public sealed partial class CollectionLibrary
    {
        public bool RecordMatch(MatchNumbers result)
        {
            if(result==null||string.IsNullOrEmpty(result.id)||result.cards<0||result.terrains<0||result.turns<0)throw new InvalidOperationException("Resultado inválido.");
            var p=Data.progress;if(p.recorded.Contains(result.id))return false;
            p.completed++;if(result.won)p.wins++;p.cards+=result.cards;p.terrains+=result.terrains;
            p.recordCards=Math.Max(p.recordCards,result.cards);p.recordTerrains=Math.Max(p.recordTerrains,result.terrains);p.recordTurns=Math.Max(p.recordTurns,result.turns);
            p.recorded.Add(result.id);p.history.Insert(0,result.Copy());if(p.history.Count>50)p.history.RemoveAt(50);return true;
        }
        public int MissionValue(MissionDefinition m)
        {
            var p=Data.progress;switch(m.metric){case "completed":return p.completed;case "wins":return p.wins;case "cards":return p.cards;case "terrains":return p.terrains;default:throw new InvalidOperationException("Métrica desconhecida.");}
        }
        public void ClaimMission(MissionDefinition m)
        {
            if(m==null||string.IsNullOrEmpty(m.id)||m.target<1||m.coins<0||m.boosters<0)throw new InvalidOperationException("Missão inválida.");
            if(Data.progress.claimed.Contains(m.id)||MissionValue(m)<m.target)throw new InvalidOperationException("Missão indisponível para resgate.");
            int coins=checked(Data.coins+m.coins),boosters=checked(Data.progress.boosters+m.boosters);
            Data.coins=coins;Data.progress.boosters=boosters;Data.progress.claimed.Add(m.id);
        }
        public BoosterReceipt OpenRewardBooster(BoosterDefinition pack,Random random)
        {
            if(Data.progress.boosters<1)throw new InvalidOperationException("Nenhum booster de recompensa.");
            var free=new BoosterDefinition{id=pack.id,name=pack.name,price=0,cards=pack.cards,duplicateRefund=pack.duplicateRefund,expansions=pack.expansions,standardWeight=pack.standardWeight,illuminatedWeight=pack.illuminatedWeight,nocturneWeight=pack.nocturneWeight};
            var result=OpenBooster(free,random);Data.progress.boosters--;return result;
        }
    }
    public sealed partial class Match
    {
        public static readonly Definition Ruins=new Definition(new CardData{id="system-ruins",name="Ruínas",kind="Terrain",rule="ruins",unavailableReason="Terreno de cenário; não colecionável.",text="Terreno inicial sem efeitos e sem geração de mana.",art="ruins"},"system");
        public string MatchId {get;private set;}=Guid.NewGuid().ToString("N");
        readonly int[] playedCards=new int[4],placedTerrains=new int[4];
        public MatchNumbers NumbersFor(int seat)=>IsRemoteView?remote.numbers?.Copy():new MatchNumbers{id=MatchId,won=Over&&WinningTeam==seats[seat].Team,cards=playedCards[seat],terrains=placedTerrains[seat],turns=Turn};
        void CountAction(Command c){if(c.kind==ActionKind.Place)placedTerrains[c.player]++;if(c.kind==ActionKind.Play||c.kind==ActionKind.SummonCommander||c.kind==ActionKind.CastExiled)playedCards[c.player]++;}
    }
}
