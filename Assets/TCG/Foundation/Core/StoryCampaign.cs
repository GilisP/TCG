using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
namespace TCG.Foundation
{
    [Serializable] public sealed class StoryHeroProgress
    {
        public string heroPreconId;
        public List<string> completed=new List<string>();
    }
    [Serializable] public sealed class StoryDailyClaim
    {
        public string enemyPreconId,day;
    }
    [Serializable] public sealed class StoryProgress
    {
        public string selectedHeroPreconId="";
        public List<StoryHeroProgress> heroes=new List<StoryHeroProgress>();
        public List<StoryDailyClaim> dailyClaims=new List<StoryDailyClaim>();
        public List<string> recordedMatches=new List<string>();
    }
    [Serializable] public sealed class StoryEncounter
    {
        public string id,title,format,allyPreconId;
        public string[] opponents=Array.Empty<string>(),prerequisites=Array.Empty<string>();
        public float x,y;
    }
    public sealed class StoryReward
    {
        public int boosters; public bool newlyCompleted;
        public string[] rewardedEnemies=Array.Empty<string>();
    }
    public sealed class StoryCampaign
    {
        public string heroPreconId {get;private set;}
        public IReadOnlyList<StoryEncounter> Encounters {get;private set;}
        // Configurable encounter data; this factory provides the provisional initial route.
        public static StoryCampaign BuildDefault(string heroPreconId,IEnumerable<string> allPreconIds)
        {
            if(string.IsNullOrWhiteSpace(heroPreconId)||allPreconIds==null)throw new ArgumentException("Precons da campanha inválidos.");
            var ids=allPreconIds.ToArray();
            if(ids.Any(string.IsNullOrWhiteSpace)||ids.Distinct(StringComparer.Ordinal).Count()!=ids.Length||!ids.Contains(heroPreconId))throw new ArgumentException("Identidades de precons inválidas.");
            var foes=ids.Where(id=>id!=heroPreconId).ToArray();
            if(foes.Length<3)throw new ArgumentException("A campanha exige pelo menos quatro precons.");
            var nodes=new List<StoryEncounter>();
            foreach(string enemy in foes)nodes.Add(new StoryEncounter{id="duel-"+enemy,title="Duelo "+(nodes.Count+1),format="duel",opponents=new[]{enemy}});
            nodes.Add(new StoryEncounter{id="teams",title="Confronto em duplas",format="teams",opponents=new[]{foes[0],foes[1]},allyPreconId=foes[2]});
            nodes.Add(new StoryEncounter{id="ffa3",title="Confronto de três",format="ffa3",opponents=new[]{foes[1],foes[2]}});
            nodes.Add(new StoryEncounter{id="ffa4",title="Confronto de quatro",format="ffa4",opponents=new[]{foes[0],foes[1],foes[2]}});
            for(int i=0;i<nodes.Count;i++)
            {
                nodes[i].prerequisites=i==0?Array.Empty<string>():new[]{nodes[i-1].id};
                nodes[i].x=.15f+.7f*(i%3)/2f;nodes[i].y=.1f+.8f*i/(nodes.Count-1);
            }
            return new StoryCampaign{heroPreconId=heroPreconId,Encounters=nodes.AsReadOnly()};
        }
        public bool CanEnter(StoryProgress progress,string hero,string encounterId)
        {
            var node=Encounters.FirstOrDefault(n=>n.id==encounterId);
            if(hero!=heroPreconId||node==null)return false;
            var completed=progress?.heroes?.FirstOrDefault(h=>h!=null&&h.heroPreconId==hero)?.completed;
            return node.prerequisites.All(id=>completed!=null&&completed.Contains(id));
        }
    }
    public sealed partial class CollectionLibrary
    {
        public void NormalizeStoryProgress()
        {
            Data.story=Data.story??new StoryProgress();
            var story=Data.story;
            story.heroes=story.heroes??new List<StoryHeroProgress>();
            story.dailyClaims=story.dailyClaims??new List<StoryDailyClaim>();
            story.recordedMatches=story.recordedMatches??new List<string>();
            foreach(var hero in story.heroes)
            {
                if(hero==null||string.IsNullOrWhiteSpace(hero.heroPreconId))throw new InvalidOperationException("Progresso de história inválido.");
                hero.completed=hero.completed??new List<string>();
                if(hero.completed.Any(string.IsNullOrWhiteSpace))throw new InvalidOperationException("Encontro concluído inválido.");
            }
            if(story.heroes.Select(h=>h.heroPreconId).Distinct().Count()!=story.heroes.Count||story.recordedMatches.Any(string.IsNullOrWhiteSpace)||story.dailyClaims.Any(c=>c==null||string.IsNullOrWhiteSpace(c.enemyPreconId)||!DateTime.TryParseExact(c.day,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out _)))throw new InvalidOperationException("Histórico de campanha inválido.");
        }
        public StoryReward CompleteStoryEncounter(StoryCampaign campaign,string hero,string encounterId,MatchNumbers result,DateTime calendarDay)
        {
            if(campaign==null||result==null||string.IsNullOrWhiteSpace(result.id)||result.cards<0||result.terrains<0||result.turns<0)throw new InvalidOperationException("Resultado de história inválido.");
            var story=Data.story;
            if(story.recordedMatches.Contains(result.id))return new StoryReward();
            if(!campaign.CanEnter(story,hero,encounterId))throw new InvalidOperationException("Encontro ainda indisponível.");
            var node=campaign.Encounters.First(n=>n.id==encounterId);
            var heroProgress=story.heroes.FirstOrDefault(h=>h.heroPreconId==hero);
            string day=calendarDay.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture);
            string[] enemies=result.won?node.opponents.Distinct().Where(enemy=>!story.dailyClaims.Any(c=>c.enemyPreconId==enemy&&c.day==day)).ToArray():Array.Empty<string>();
            int boosterTotal=checked(Data.progress.boosters+enemies.Length);
            bool newlyCompleted=result.won&&(heroProgress==null||!heroProgress.completed.Contains(encounterId));
            // All fallible calculations finish before changes. Persistence/rollback is owned by the caller.
            if(newlyCompleted)
            {
                if(heroProgress==null){heroProgress=new StoryHeroProgress{heroPreconId=hero};story.heroes.Add(heroProgress);}
                heroProgress.completed.Add(encounterId);
            }
            foreach(string enemy in enemies)story.dailyClaims.Add(new StoryDailyClaim{enemyPreconId=enemy,day=day});
            story.recordedMatches.Add(result.id);Data.progress.boosters=boosterTotal;
            return new StoryReward{boosters=enemies.Length,newlyCompleted=newlyCompleted,rewardedEnemies=enemies};
        }
    }
}
