using System;
using System.Linq;
using System.Reflection;
using TCG.Table;
using UnityEngine;
namespace TCG.Foundation.Editor
{
    public static class ComputerOpponentChecks
    {
        static string Key(Command c)=>c==null?"null":c.kind+":"+c.player+":"+c.target+":"+c.unit+":"+c.pile+":"+c.card+":"+string.Join(",",c.units)+":"+string.Join(",",c.blockers);
        public static void Run()
        {
            int checks=0;void Check(bool ok,string label){checks++;if(!ok)throw new Exception("COMPUTER: "+label);}
            var effects=new EffectRegistry();var catalog=ContentLoader.LoadFixtures(effects);
            var official=PreconstructedDeckLoader.Load(catalog).Decks.OrderBy(d=>d.id,StringComparer.Ordinal).ToArray();
            Check(official.Length==10,"all ten official precons loaded");
            for(int pair=0;pair<5;pair++)
            {
                var decks=official.Skip(pair*2).Take(2).ToArray();int seed=310+pair;
                string scope=string.Join(" vs ",decks.Select(d=>d.id))+" seed="+seed;
                Check(decks.All(d=>d.main.Length==100&&d.terrains.Length==50&&!string.IsNullOrEmpty(d.commander)),scope+" full lists and commanders");
                var match=new Match(catalog,effects,2,false,seed,decks.Select(d=>d.main).ToArray(),decks.Select(d=>d.terrains).ToArray(),decks.Select(d=>d.commander).ToArray()){AutomaticResponses=true,AutoAdvanceAfterTerrain=true};
                match.ResolveInitialEffects();var opponents=new[]{new ComputerOpponent(seed+50),new ComputerOpponent(seed+51)};
                int sampled=0;
                while(!match.Over&&sampled++<100)
                {
                    int owner=match.Controller;var legal=match.DecisionCommandsFor(owner);var projection=match.ViewFor(owner);
                    string context=scope+" step="+sampled+" turn="+match.Turn+" choice="+(projection.prompt??"");
                    Check(legal.Count>0,context+" candidates");var action=opponents[owner].Choose(projection,legal,catalog);
                    Check(legal.Contains(action),context+" selection "+Key(action));
                    Check(match.Try(action,out var error),context+" action="+Key(action)+" error="+error);
                }
                Check(sampled>0,scope+" sampled official gameplay");
            }
            foreach(int count in new[]{2,3,4})foreach(bool teams in count==4?new[]{false,true}:new[]{false})
            {
                var m=ContentLoader.TestTable(catalog,effects,count,teams,17);
                var bots=Enumerable.Range(0,count).Select(i=>new ComputerOpponent(71+i)).ToArray();
                // Reduced-library fixture: normal exhaustion is a rule victory, never a
                // timeout concession. The loop cap fails the test rather than ending play.
                foreach(var seat in m.Seats)seat.main.RemoveRange(0,Math.Max(0,seat.main.Count-8));
                int steps=0;
                while(!m.Over&&steps++<1800)
                {
                    int owner=m.Controller;var legal=m.DecisionCommandsFor(owner);var view=m.ViewFor(owner);
                    Check(legal.Count>0,"controller has candidates "+count+"/"+teams);
                    Check(legal.All(c=>c.player==owner&&c.revision==m.Revision&&c.kind!=ActionKind.Concede),"authority and revision");
                    Check(m.DecisionCommandsFor((owner+1)%count).Count==0,"inactive has no candidates");
                    var action=bots[owner].Choose(view,legal,catalog);Check(legal.Contains(action),"selection belongs to legal list");
                    Check(m.Try(action,out var error),"selected action accepted: "+error);
                }
                Check(m.Over,"sustained fixture completed by game rules "+count+"/"+teams);
                Check(m.DecisionCommandsFor(0).Count==0,"terminal has no candidates");
            }
            var privacy=ContentLoader.TestTable(catalog,effects,2,false,41);int me=privacy.Controller,enemy=1-me;
            var before=privacy.ViewFor(me);var commands=privacy.DecisionCommandsFor(me);
            privacy.Seats[enemy].main.Reverse();privacy.Seats[enemy].hand.Reverse();privacy.Seats[enemy].terrainDeck.Reverse();
            var after=privacy.ViewFor(me);
            Check(JsonUtility.ToJson(before)==JsonUtility.ToJson(after),"opponent private permutations invisible");
            for(int seed=0;seed<20;seed++)Check(Key(new ComputerOpponent(seed).Choose(before,commands,catalog))==Key(new ComputerOpponent(seed).Choose(after,privacy.DecisionCommandsFor(me),catalog)),"privacy equivalence and deterministic seed");
            var manaFixture=ContentLoader.TestTable(catalog,effects,2,false,11);int manaOwner=manaFixture.Active;
            Check(manaFixture.Try(new Command{player=manaOwner,revision=manaFixture.Revision,kind=ActionKind.DrawMain,pile=0},out _),"mana fixture draw");
            manaFixture.Seats[manaOwner].hand.Clear();manaFixture.Seats[manaOwner].hand.Add(catalog.Cards.First(c=>c.Playable&&c.ColoredCost[0]>0&&c.ColoredCost[1]==0));
            for(int color=0;color<7;color++)manaFixture.Seats[manaOwner].mana[color]=0;
            for(int pile=0;pile<2;pile++){manaFixture.Seats[manaOwner].piles[pile].Clear();manaFixture.Seats[manaOwner].piles[pile].Add(new TerrainCard(catalog.Get("test-land-"+pile),manaOwner));}
            var manaCandidates=manaFixture.DecisionCommandsFor(manaOwner).Where(c=>c.kind==ActionKind.Place&&c.target==Match.Capital(manaOwner,2)&&c.pile<2).ToArray();
            Check(manaCandidates.Length==2,"two equally placed public terrain alternatives");
            Check(new ComputerOpponent(3).Choose(manaFixture.ViewFor(manaOwner),manaCandidates,catalog).pile==0,"terrain color fills own hand colored deficit");
            var choice=new RuleChoice(me,"fixture",new[]{new ChoiceOption(7,"A"),new ChoiceOption(9,"B")},key=>{});
            typeof(Match).GetProperty("Choice").SetValue(privacy,choice);
            var options=privacy.DecisionCommandsFor(me);Check(options.Count==2&&options.All(c=>c.kind==ActionKind.Choose),"choice only");
            Check(privacy.Try(new ComputerOpponent(3).Choose(privacy.ViewFor(me),options,catalog),out var choiceError),"choice accepted "+choiceError);
            // Real combat at the opponent capital, both capital and unit assignments.
            var combat=ContentLoader.TestTable(catalog,effects,2,false,6);int attacker=combat.Active,defender=1-attacker;
            bool Do(Command c){c.player=combat.Controller;c.revision=combat.Revision;return combat.Try(c,out _);}
            Check(Do(new Command{kind=ActionKind.DrawMain,pile=0}),"combat draw");Check(Do(new Command{kind=ActionKind.Place,pile=0,target=Match.Capital(attacker,2)}),"combat terrain");
            if(combat.Phase==Stage.Terrain)Check(Do(new Command{kind=ActionKind.NextPhase}),"combat main");
            var creature=catalog.Cards.First(c=>c.Kind==CardType.Creature&&c.Playable&&c.Actions>0&&c.Movement>0&&c.Rule==""&&c.Abilities.Count==0);
            int capital=Match.Capital(defender,2),origin=Match.Neighbors(capital).First();combat.Board[origin].Terrain=Match.Ruins;
            combat.Board[origin].pieces.Add(new Piece(900001,attacker,creature));combat.Board[capital].pieces.Add(new Piece(900002,defender,creature));
            var attacks=combat.DecisionCommandsFor(attacker).Where(c=>c.kind==ActionKind.Attack&&c.target==capital).ToArray();Check(attacks.Length>0,"attack generated");Check(Do(attacks[0]),"attack accepted");
            int guard=30;while(combat.Defense==null&&combat.Stack.Count>0&&guard-->0)Check(Do(new Command{kind=ActionKind.Pass}),"pre-defense responses");
            Check(combat.Defense!=null,"defense reached");var defenses=combat.DecisionCommandsFor(combat.Controller);Check(defenses.Count>=2&&defenses.All(c=>c.kind==ActionKind.Defend),"legal defense alternatives");
            Check(Do(new ComputerOpponent(9).Choose(combat.ViewFor(combat.Controller),defenses,catalog)),"defense accepted");
            bool rejected=false;try{Match.FromView(privacy.ViewFor(me),catalog,effects).DecisionCommandsFor(me);}catch(InvalidOperationException){rejected=true;}Check(rejected,"projection cannot generate authority commands");
            Debug.Log("COMPUTER OPPONENT CHECKS PASSED: "+checks);
        }
    }
}
