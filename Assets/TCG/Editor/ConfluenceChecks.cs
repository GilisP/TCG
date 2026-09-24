using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace TCG.Editor
{
    public static class ConfluenceChecks
    {
        static int count;
        static void Check(bool ok,string name){if(!ok)throw new Exception("CONFLUENCE TEST FAILED: "+name);count++;}
        static Card Card(string id)=>new Card(Catalog.Get(id),Guid.NewGuid().ToString());
        static Game Main(){var g=new Game();g.AdvancePhase();g.AdvancePhase();g.Current.Offer[0]=Card("land-6");g.Place(0,16);g.AdvancePhase();foreach(var p in g.Players)for(int e=0;e<7;e++)p.Mana[e]=30;return g;}
        static Unit Put(Game g,int pos,int owner=0){var u=new Unit(owner,Card("unit-0-0"));g.Board[pos].Unit=u;return u;}
        static void Cast(Game g,string id,int target=-1){g.Current.Hand.Add(Card(id));g.PlayCard(g.Active,g.Current.Hand.Count-1,target);}
        static void Resolve(Game g){g.PassPriority();g.PassPriority();}
        static void Refresh(Game g)=>typeof(Game).GetMethod("BeginTurn",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(g,null);
        public static void Run()
        {
            count=0;var added=Catalog.All.Where(c=>c.Id.StartsWith("confluence-")).ToArray();
            Check(added.Length==43,"43 new definitions");Check(added.Count(c=>c.Kind==CardKind.Terrain)==17,"17 multicolor lands");
            Check(added.Count(c=>c.Kind==CardKind.Creature)==12,"12 creatures");Check(added.Count(c=>c.Kind==CardKind.Spell||c.Kind==CardKind.Trick)==12,"12 spells and tricks");
            Check(added.All(c=>CardAuthoring.Validate(c).Count==0),"every new card validates");
            var g=Main();g.Board[16].Terrain=Card("confluence-land-0-2");var before=(int[])g.Current.Mana.Clone();
            g.ChooseTerrainMana(16,2);Check(g.Current.Mana.SequenceEqual(before),"choosing next color does not convert current mana");Refresh(g);
            Check(g.Current.Mana[2]==1&&g.Current.Mana[0]==0&&g.Current.Mana[6]==1,"dual land generates chosen color only");
            g.Phase=Phase.Main;g.ChooseTerrainMana(16,0);Refresh(g);Check(g.Current.Mana[0]==1&&g.Current.Mana[2]==0,"switch next generation");
            g.Phase=Phase.Main;bool rejected=false;try{g.ChooseTerrainMana(16,3);}catch(InvalidOperationException){rejected=true;}Check(rejected,"unsupported mana color rejected");
            g.Board[16].Terrain=Card("confluence-land-storm");g.Board[16].ManaChoice=0;Refresh(g);Check(g.Current.Mana[1]==1,"stale selection falls back to new terrain first color");
            var invalid=CardAuthoring.Copy(Catalog.Get("confluence-land-0-2"));invalid.ManaColors=new[]{0,0};Check(CardAuthoring.Validate(invalid).Count>0,"duplicate production colors rejected");
            g=Main();var a=Put(g,16);var e=Put(g,17,1);var other=Put(g,18,1);Cast(g,"confluence-meteor");Resolve(g);Check(e.Life==1&&other.Life==1&&a.Life==3,"enemy area damage leaves allies alone");
            g=Main();e=Put(g,17,1);Cast(g,"confluence-wither",17);Resolve(g);Check(e.Attack==0,"weaken floors attack at zero");g.AdvancePhase();Resolve(g);Check(e.Attack==2,"weaken expires");
            g=Main();a=Put(g,16);a.Actions=a.Movement=0;Cast(g,"confluence-haste",16);Resolve(g);Check(a.Actions==1&&a.Movement==1,"ready grants action and movement");
            g=Main();a=Put(g,16);a.Actions=a.Movement=0;a.StunnedThroughTurn=g.Turn+2;Cast(g,"confluence-haste",16);Resolve(g);Check(a.Actions==0,"ready does not bypass freeze");
            a.Life=1;a.AttackPenalty=2;Cast(g,"confluence-purify",16);Resolve(g);Check(a.StunnedThroughTurn==0&&a.Attack==2&&a.Life==3&&a.Actions==0,"cleanse and heal without restoring spent actions");
            g=Main();a=Put(g,16);a.BonusAttack=2;a.AttackPenalty=3;Cast(g,"confluence-purify",16);Resolve(g);Check(a.Attack==4&&a.BonusAttack==2,"cleanse removes debuff while preserving positive buff");
            g=Main();int discarded=g.Players[1].Hand.Count;Cast(g,"confluence-mind-rift");Resolve(g);Check(g.Players[1].Hand.Count==discarded-2&&g.Players[1].Graveyard.Count==2,"discard moves last cards to graveyard");
            g=Main();var first=Card("unit-0-0");var last=Card("fire-0");g.Current.Graveyard.Add(first);g.Current.Graveyard.Add(last);Cast(g,"confluence-reclaim");Resolve(g);
            Check(g.Current.Hand.Contains(first)&&g.Current.Hand.Contains(last)&&g.Current.Graveyard.Count==1&&g.Current.Graveyard[0].Id=="confluence-reclaim","recover before resolving spell enters graveyard");
            g=Main();int pop=g.Current.Popularity,hand=g.Current.Hand.Count;Cast(g,"confluence-rally");Resolve(g);Check(g.Current.Popularity==pop+2&&g.Current.Hand.Count==hand+1,"popularity plus draw chain");
            g=Main();g.Current.Life=40;Cast(g,"confluence-eclipse-drain");Resolve(g);Check(g.Players[1].Life==47&&g.Current.Life==43,"direct damage then heal");
            g=Main();e=Put(g,17,1);other=Put(g,18,1);a=Put(g,16);Cast(g,"confluence-winter");Resolve(g);Check(e.Actions==0&&other.Actions==0&&a.Actions==1,"freeze all enemies only");
            g=Main();Cast(g,"confluence-sun-token",16);Resolve(g);Check(g.Board[16].Unit.Card.Id=="confluence-token-sun"&&g.Board[16].Unit.Attack==2,"new token summon");
            g=Main();Cast(g,"confluence-moon-seer",16);Resolve(g);Check(g.Stack.Count==1,"new creature entry waits on stack");Resolve(g);Check(g.Players[1].Hand.Count==4,"creature compound entry effect");
            for(int i=0;i<EffectRecipes.Names.Length;i++)
            {
                var model=new CardDefinition{Id="recipe",Name="Recipe",Kind=i==24?CardKind.Creature:CardKind.Spell,Life=3,Text="Teste",Effects=EffectRecipes.Create(i)};
                Check(CardAuthoring.Validate(model).Count==0,"valid recipe "+i);
            }
            var clone=CardAuthoring.Copy(Catalog.Get("confluence-land-0-2"));clone.Id="custom-color-art-check";
            var restored=CustomCardStore.Parse(CustomCardStore.Serialize(clone));Check(restored.ManaColors.SequenceEqual(new[]{0,2})&&restored.Illustration=="confluence","art and production colors serialize");
            foreach(var key in GeneratedArt.Keys){var texture=GeneratedArt.Get(key);Check(texture!=null&&texture.width>=1024&&texture.height>=1024,"generated illustration imports: "+key);}
            using(var art=new DisposableArt())Check(art.Value.Landscape(Catalog.Get("confluence-meteor"))==GeneratedArt.Get("arcane-clash"),"card renderer resolves generated illustration");
            Debug.Log("TCG_CONFLUENCE_VALIDATION_OK: "+count+" assertions covering multicolor mana, new cards, reusable recipes and imported illustrations.");
        }
        sealed class DisposableArt:IDisposable{public readonly BoardArt Value=new BoardArt();public void Dispose()=>Value.Dispose();}
    }
}
