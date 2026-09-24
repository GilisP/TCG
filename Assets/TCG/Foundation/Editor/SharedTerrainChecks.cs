using System;
using System.Collections.Generic;
using System.Linq;
using TCG.Table;
using UnityEngine;
namespace TCG.Foundation.Editor
{
    public static class SharedTerrainChecks
    {
        public static void Run()
        {
            int checks=0;void Check(bool ok,string name){checks++;if(!ok)throw new Exception("SHARED TERRAIN: "+name);}
            var effects=new EffectRegistry();var catalog=ContentLoader.Load(effects);
            void Do(Match m,ActionKind kind,int target=-1,int pile=0,int player=-1,string card=null)
            {if(!m.Try(new Command{player=player<0?m.Controller:player,revision=m.Revision,kind=kind,target=target,pile=pile,card=card},out var error))throw new Exception("SHARED action: "+error);}
            for(int count=2;count<=4;count++)for(int seed=0;seed<8;seed++)
            {
                var m=ContentLoader.TestTable(catalog,effects,count,false,seed);
                var shared=m.Seats[0].piles.SelectMany(p=>p).ToArray();
                Check(shared.Length==8&&m.Seats.All(s=>ReferenceEquals(s.piles,m.Seats[0].piles)),"one shared set");
                var amounts=Enumerable.Range(0,count).Select(p=>shared.Count(c=>c.Owner==p)).ToArray();
                Check(amounts.Max()-amounts.Min()<=1&&amounts.Sum()==8,"balanced 4/4, 3/3/2 or 2/2/2/2");
                Check(Enumerable.Range(0,count).All(p=>m.Board[Match.Capital(p,count)].TerrainOwner==p),"own capital source");
                var same=ContentLoader.TestTable(catalog,effects,count,false,seed);
                Check(shared.Select(x=>x.Owner+":"+x.Card.Id).SequenceEqual(same.Seats[0].piles.SelectMany(p=>p).Select(x=>x.Owner+":"+x.Card.Id)),"seed reproducible");
                int owner=m.Active,other=(owner+1)%count,before=m.Seats[owner].TerrainCount;
                Do(m,ActionKind.DrawTerrain);
                Check(m.Seats[other].TopOwner(0)==owner&&m.Seats[owner].TerrainCount==before-1,"purchase from active deck, public to everyone");
                var seat=m.Seats[owner];var foreign=shared.First(c=>c.Owner!=owner);seat.piles[0].Clear();seat.piles[0].Add(foreign);
                int at=Match.Neighbors(Match.Capital(owner,count)).First();before=seat.TerrainCount;
                Do(m,ActionKind.Place,at);
                Check(m.Board[at].Owner==owner&&m.Board[at].TerrainOwner==foreign.Owner,"realm separate from original owner");
                Check(seat.TopOwner(0)==owner&&seat.TerrainCount==before-1,"empty replenishment from active player");
                var view=Match.FromView(JsonUtility.FromJson<MatchView>(JsonUtility.ToJson(m.ViewFor(other))),catalog,effects);
                Check(view.Board[at].TerrainOwner==foreign.Owner&&view.Board[at].Owner==owner,"ownership replicated");
                Check(view.Seats.All(s=>Enumerable.Range(0,4).All(p=>s.Top(p)?.Id==seat.Top(p)?.Id&&s.TopOwner(p)==seat.TopOwner(p)&&s.PileCount(p)==seat.PileCount(p))),"same public tops/counts/owners for all");
                // Move to the same player's next turn and replace its borrowed terrain.
                int guard=40;Do(m,ActionKind.NextPhase);Do(m,ActionKind.NextPhase);
                while((m.Active==owner||m.Phase!=Stage.Draw)&&guard-->0)Do(m,ActionKind.Pass);
                while(m.Active!=owner&&guard-->0){Do(m,ActionKind.DrawMain);Do(m,ActionKind.Place,Match.Capital(m.Active,count));Do(m,ActionKind.NextPhase);Do(m,ActionKind.NextPhase);for(int n=0;n<count;n++)Do(m,ActionKind.Pass);}
                Check(guard>0,"turn rotation");Do(m,ActionKind.DrawMain);int grave=m.Seats[foreign.Owner].TerrainGraveyard.Count;
                Do(m,ActionKind.Place,at);
                Check(m.Seats[foreign.Owner].TerrainGraveyard.Count==grave+1&&m.Seats[foreign.Owner].TerrainGraveyard.Last()==foreign.Card,"borrowed terrain returns to owner's cemetery");
            }
            var game=ContentLoader.TestTable(catalog,effects,4,false,35);int live=game.Active,loser=(live+1)%4;
            int[] counts=Enumerable.Range(0,4).Select(game.Seats[0].PileCount).ToArray();Do(game,ActionKind.Concede,player:loser);
            Check(counts.SequenceEqual(Enumerable.Range(0,4).Select(game.Seats[0].PileCount)),"elimination preserves shared piles");
            Do(game,ActionKind.DrawMain);game.Seats[live].terrainDeck.Clear();game.Seats[live].piles[0].RemoveAt(0);Do(game,ActionKind.Place,Match.Capital(live,4));
            Check(game.Seats[live].Top(0)==null&&!game.Seats[live].Eliminated,"empty own deck does not borrow or lose");
            var events=new List<MatchEvent>();game.Visual+=events.Add;game.AddMana(live,3,2,60);
            Check(events.Count==1&&events[0].Kind=="mana"&&events[0].From==60&&events[0].Color==3&&events[0].Amount==2&&events[0].To==Match.Capital(live,4),"mana event source and destination");
            game.AddMana(live,3,0);Check(events.Count==1,"no zero-gain effect");
            var spells=ContentLoader.TestTable(catalog,effects,2,false,82);Do(spells,ActionKind.DrawMain);Do(spells,ActionKind.Place,Match.Capital(spells.Active,2));Do(spells,ActionKind.NextPhase);
            foreach(var seat in spells.Seats){seat.hand.Clear();for(int n=0;n<7;n++)seat.mana[n]=30;}
            var spellEvents=new List<MatchEvent>();spells.Visual+=spellEvents.Add;
            spells.Seats[spells.Controller].hand.Add(catalog.Get("test-mill"));Do(spells,ActionKind.Play,card:"test-mill");string canceled=spells.Stack.Last().VisualId;
            spells.Seats[spells.Controller].hand.Add(catalog.Get("test-counter"));Do(spells,ActionKind.Play,card:"test-counter");string resolved=spells.Stack.Last().VisualId;
            var net=Match.FromView(spells.ViewFor(spells.Active),catalog,effects);Check(net.Stack.Last().VisualId==resolved,"stack identity replicated");
            int safety=30;while((spells.Stack.Count>0||spells.Choice!=null)&&safety-->0){if(spells.Choice!=null)Do(spells,ActionKind.Choose,spells.Choice.Options.First().Key);else Do(spells,ActionKind.Pass);}
            Check(safety>0,"counter resolves");
            Check(spellEvents.Count(e=>e.Kind=="stack-canceled"&&e.StackId==canceled)==1,"one burn for canceled instance");
            Check(!spellEvents.Any(e=>e.Kind=="stack-resolved"&&e.StackId==canceled),"canceled instance never glows");
            Check(spellEvents.Count(e=>e.Kind=="stack-resolved"&&e.StackId==resolved)==1,"counter itself resolves once");
            Debug.Log("SHARED TERRAIN CHECKS PASSED: "+checks);
        }
    }
}
