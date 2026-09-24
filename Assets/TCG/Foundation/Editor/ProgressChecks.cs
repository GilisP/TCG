using System;
using System.Linq;
using TCG.Table;
using UnityEngine;
namespace TCG.Foundation.Editor
{
    public static class ProgressChecks
    {
        public static void Run()
        {
            int checks=0;void Check(bool ok,string name){checks++;if(!ok)throw new Exception("PROGRESS: "+name);}
            var effects=new EffectRegistry();var catalog=ContentLoader.Load(effects);var m=ContentLoader.TestTable(catalog,effects,2,false,10);
            Check(m.Board.Count(c=>c.Terrain==Match.Ruins)==6,"three ruins per seat");
            Check(m.Seats[m.Active].Mana.Sum()==1,"ruins produce no mana");
            Check(m.Board[Match.Capital(0,2)].Terrain!=Match.Ruins,"capital uses deck card");
            var copy=Match.FromView(JsonUtility.FromJson<MatchView>(JsonUtility.ToJson(m.ViewFor(0))),catalog,effects);
            Check(copy.Board.Count(c=>c.Terrain==Match.Ruins)==6,"ruins replicated without catalog card");
            int owner=m.Active;Check(m.Try(new Command{player=owner,revision=m.Revision,kind=ActionKind.DrawMain,pile=0},out _),"draw");
            int tile=Match.Neighbors(Match.Capital(owner,2)).First();Check(m.Try(new Command{player=owner,revision=m.Revision,kind=ActionKind.Place,target=tile,pile=0},out _),"replace ruins");
            Check(m.Seats[owner].terrainGrave.Count==0,"ruins do not enter cemetery");Check(m.NumbersFor(owner).terrains==1,"placement recorded");
            Check(!m.Try(new Command{player=owner,revision=0,kind=ActionKind.Place,target=tile},out _)&&m.NumbersFor(owner).terrains==1,"rejected command does not count");
            var library=new CollectionLibrary(catalog,new CollectionData{coins=217});var result=new MatchNumbers{id="test",won=true,cards=12,terrains=3,turns=8};
            Check(library.RecordMatch(result)&&!library.RecordMatch(result),"result idempotent");
            Check(library.Data.progress.completed==1&&library.Data.progress.wins==1&&library.Data.progress.recordCards==12,"totals and records");
            var mission=new MissionDefinition{id="first",metric="completed",target=1,coins=50,boosters=1};library.ClaimMission(mission);
            Check(library.Data.coins==267&&library.Data.progress.boosters==1,"rewards");
            bool rejected=false;try{library.ClaimMission(mission);}catch(InvalidOperationException){rejected=true;}Check(rejected&&library.Data.coins==267,"duplicate claim blocked");
            library=new CollectionLibrary(catalog,JsonUtility.FromJson<CollectionData>(JsonUtility.ToJson(library.Data)));
            Check(!library.RecordMatch(result)&&library.Data.progress.claimed.Contains("first"),"receipt survives reload");
            var receipt=library.OpenRewardBooster(new BoosterDefinition(),new System.Random(4));Check(receipt.price==0&&receipt.rewards.Count==5&&library.Data.progress.boosters==0,"free booster opens");
            Check(library.Data.coins==267+receipt.refund,"no charge for reward booster");
            var old=JsonUtility.FromJson<CollectionData>("{\"schemaVersion\":3,\"coins\":23,\"owned\":[],\"decks\":[],\"variants\":[],\"appearances\":[],\"cosmetics\":[\"classic\"]}");
            Check(new CollectionLibrary(catalog,old).Data.coins==23&&old.progress.completed==0,"old profile retains coins");
            foreach(int count in new[]{2,3,4})for(int seat=0;seat<count;seat++){var home=TableWorld.Position(Match.Capital(seat,count));var camera=Quaternion.Euler(57,TableWorld.CapitalYaw(seat,count),0)*Vector3.back;camera.y=0;Check(Vector3.Dot(home.normalized,camera.normalized)>.999f,"camera behind own capital "+count+"/"+seat);}
            Debug.Log("PROGRESS CHECKS PASSED: "+checks);
        }
    }
}
