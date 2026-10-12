using System;
using System.Linq;
using UnityEngine;
using TCG.Table;
namespace TCG.Foundation.Editor
{
    public static class StoryCampaignChecks
    {
        public static void Run()
        {
            int checks=0;void Check(bool ok,string name){checks++;if(!ok)throw new Exception("STORY: "+name);}
            var catalog=ContentLoader.Load(new EffectRegistry());
            var ids=Enumerable.Range(1,10).Select(i=>"COL001-D"+i.ToString("00")).ToArray();
            var campaign=StoryCampaign.BuildDefault(ids[0],ids);
            var data=new CollectionData{coins=217};var library=new CollectionLibrary(catalog,data);
            var day=new DateTime(2026,10,12);var first=campaign.Encounters[0];
            MatchNumbers Result(string id,bool won=true)=>new MatchNumbers{id=id,won=won};
            Check(campaign.Encounters.Count==12&&campaign.Encounters.Count(n=>n.format=="duel")==9,"duel majority");
            Check(campaign.Encounters.All(n=>!n.opponents.Contains(ids[0])&&n.opponents.Distinct().Count()==n.opponents.Length&&!n.opponents.Contains(n.allyPreconId)),"unique foes and ally");
            Check(campaign.Encounters.All(n=>n.x>=0&&n.x<=1&&n.y>=0&&n.y<=1),"map coordinates");
            Check(campaign.CanEnter(data.story,ids[0],first.id)&&!campaign.CanEnter(data.story,ids[0],campaign.Encounters[1].id),"initial gate");
            var defeat=library.CompleteStoryEncounter(campaign,ids[0],first.id,Result("loss",false),day);
            Check(defeat.boosters==0&&!defeat.newlyCompleted&&!campaign.CanEnter(data.story,ids[0],campaign.Encounters[1].id),"defeat never unlocks or rewards");
            var reward=library.CompleteStoryEncounter(campaign,ids[0],first.id,Result("win"),day);
            Check(reward.boosters==1&&reward.newlyCompleted&&data.progress.boosters==1&&data.coins==217,"win gives booster without coins");
            Check(campaign.CanEnter(data.story,ids[0],campaign.Encounters[1].id)&&campaign.CanEnter(data.story,ids[0],first.id),"unlock and replay");
            var duplicate=library.CompleteStoryEncounter(campaign,ids[0],first.id,Result("win"),day.AddDays(1));
            Check(duplicate.boosters==0&&!duplicate.newlyCompleted&&data.progress.boosters==1,"match id survives day rollover");
            reward=library.CompleteStoryEncounter(campaign,ids[0],first.id,Result("repeat"),day);
            Check(reward.boosters==0&&!reward.newlyCompleted,"same daily foe once");
            reward=library.CompleteStoryEncounter(campaign,ids[0],first.id,Result("tomorrow"),day.AddDays(1));
            Check(reward.boosters==1&&!reward.newlyCompleted,"calendar next day reward");
            for(int i=1;i<9;i++)library.CompleteStoryEncounter(campaign,ids[0],campaign.Encounters[i].id,Result("duel"+i),day);
            var teams=campaign.Encounters.First(n=>n.format=="teams");
            reward=library.CompleteStoryEncounter(campaign,ids[0],teams.id,Result("teams",true),day);
            Check(reward.newlyCompleted&&reward.boosters==0,"team victory unlock and overlapping foes no reward");
            reward=library.CompleteStoryEncounter(campaign,ids[0],teams.id,Result("teams-next"),day.AddDays(2));
            Check(reward.boosters==2&&reward.rewardedEnemies.SequenceEqual(teams.opponents),"two defeated enemy decks give two boosters");
            var other=StoryCampaign.BuildDefault(ids[2],ids);var otherFirst=other.Encounters.First();
            Check(!other.CanEnter(data.story,ids[2],other.Encounters[1].id),"hero progress separate");
            library.CompleteStoryEncounter(other,ids[2],otherFirst.id,Result("other-first"),day);
            reward=library.CompleteStoryEncounter(other,ids[2],other.Encounters[1].id,Result("other-overlap"),day);
            Check(reward.boosters==0&&reward.newlyCompleted,"daily enemy shared across heroes");
            library=new CollectionLibrary(catalog,JsonUtility.FromJson<CollectionData>(JsonUtility.ToJson(data)));
            Check(library.CompleteStoryEncounter(campaign,ids[0],first.id,Result("win"),day).boosters==0&&library.Data.story.heroes.Count==2,"serialization preserves claims and heroes");
            var old=JsonUtility.FromJson<CollectionData>("{\"schemaVersion\":3,\"coins\":23,\"owned\":[],\"decks\":[],\"variants\":[],\"appearances\":[],\"cosmetics\":[\"classic\"]}");
            library=new CollectionLibrary(catalog,old);
            Check(old.story!=null&&old.story.heroes.Count==0&&old.coins==23&&old.schemaVersion==3,"old profile additive normalization");
            old.progress.boosters=int.MaxValue;string before=JsonUtility.ToJson(old);bool overflow=false;
            try{library.CompleteStoryEncounter(campaign,ids[0],first.id,Result("overflow"),day);}catch(OverflowException){overflow=true;}
            Check(overflow&&JsonUtility.ToJson(old)==before,"overflow leaves all progress untouched");
            bool locked=false;try{library.CompleteStoryEncounter(campaign,ids[0],campaign.Encounters[1].id,Result("locked"),day);}catch(InvalidOperationException){locked=true;}
            Check(locked&&old.story.recordedMatches.Count==0,"locked encounter rejects result");
            Debug.Log("STORY CAMPAIGN CHECKS PASSED: "+checks);
        }
    }
}
