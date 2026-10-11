using System;
using System.Linq;
using UnityEngine;

namespace TCG.Foundation.Editor
{
    public static class MovementProjectionChecks
    {
        static int checks;
        static ContentCatalog catalog;
        static EffectRegistry effects;

        public static void Run()
        {
            checks=0;effects=new EffectRegistry();catalog=new ContentCatalog();
            catalog.Add(new ExpansionData { id="route-fixture",title="Route fixture",cards=new[] {
                new CardData {id="route-unit",name="Route unit",kind="Creature",movement=2},
                new CardData {id="route-land",name="Route land",kind="Terrain"}
            }},Array.Empty<string>());
            foreach(int count in new[]{2,4})
            {
                foreach(bool sparse in new[]{false,true})
                {
                    var m=Game(count);
                    for(int at=0;at<121;at++)if(!sparse||at%3!=1)Land(m,at);
                    // Portals, mountains, enemy occupancy/capital, powered blocker and
                    // enemy equipment deliberately exercise the original edge predicates.
                    m.Board[60].Terrain=Terrain("route-portal","portals");
                    m.Board[101].Terrain=Terrain("route-portal","portals");
                    m.Board[74].Terrain=Terrain("route-mountain","mountain-crossing");
                    m.Board[72].CapitalOwner=(m.Active+1)%count;
                    Add(m,59,(m.Active+1)%count,Card("route-enemy"));
                    Add(m,69,(m.Active+1)%count,Card("route-enchantment","Enchantment"));
                    Add(m,73,(m.Active+1)%count,Card("route-equipment","Equipment"));
                    Add(m,49,m.Active,Card("route-blocker","Creature",new[]{"block-all"}));
                    var walkers=new[] {
                        Add(m,60,m.Active,Card("route-orthogonal")),
                        Add(m,54,m.Active,Card("route-diagonal","Creature",new[]{"pca-diagonal-only"})),
                        Add(m,66,m.Active,Card("route-flying","Creature",new[]{"flying"}))
                    };
                    if(sparse)m.Board[54].Terrain=null; // A source without terrain still seeds the search.
                    foreach(var p in walkers)Equivalent(m,p.Id,"count="+count+" sparse="+sparse);
                    Equivalent(m,-900,"missing piece");
                    int revision=m.Revision;
                    var view=m.ViewFor(m.Active);
                    foreach(var p in walkers)
                    {
                        Ok(m.CanOrderMovement(p.Id),"fixture piece can receive projected routes");
                        var projected=view.routes.Where(r=>r.key.StartsWith(p.Id+":",StringComparison.Ordinal)).ToArray();
                        Ok(projected.Length==121,"all route keys preserved in projection");
                        foreach(var r in projected)
                        {
                            int target=int.Parse(r.key.Substring(r.key.IndexOf(':')+1));
                            Ok(r.path.SequenceEqual(m.MovementRoute(p.Id,target)),"projected oracle route "+r.key);
                        }
                    }
                    Ok(m.Revision==revision&&m.Position(walkers[0].Id)==60&&m.Position(walkers[1].Id)==54,
                        "route projection does not mutate match");
                    Ok(m.MovementRoute(walkers[0].Id,101).SequenceEqual(new[]{101}),"portal edge keeps neighbor ordering");
                    Ok(m.MovementRoute(walkers[0].Id,72).Count==0&&m.MovementRoute(walkers[0].Id,59).Count==0,
                        "enemy capital and enemy creature remain unreachable");
                    Ok(m.MovementRoute(walkers[0].Id,49).Count==0&&m.MovementRoute(walkers[0].Id,74).Count==0,
                        "powered blocker and mountain exclude walking piece");
                    Ok(m.MovementRoute(walkers[0].Id,73).Count>0,"enemy equipment alone does not block route");
                    Ok(m.MovementRoute(walkers[2].Id,74).Count>0,"flying piece crosses mountain");
                    Ok(m.MovementRoute(walkers[1].Id,42).SequenceEqual(new[]{42}),"diagonal-only piece keeps direct diagonal neighbor");
                }
                var preference=Game(count);
                foreach(int at in new[]{60,49,50,51,62})Land(preference,at);
                var walker=Add(preference,60,preference.Active,Card("route-preference"));
                Equivalent(preference,walker.Id,"ground preferred over shorter hole path");
                Ok(preference.MovementRoutesForProjection(walker.Id)[62].SequenceEqual(new[]{49,50,51,62}),
                    "longer all-ground path precedes shorter path through hole");
                preference.Board[50].Terrain=null;
                Equivalent(preference,walker.Id,"ground path removed");
                Ok(preference.MovementRoutesForProjection(walker.Id)[62].SequenceEqual(new[]{61,62}),
                    "hole fallback used when no ground path remains");
            }
            Debug.Log("MOVEMENT PROJECTION CHECKS PASSED: "+checks);
        }

        static void Equivalent(Match m,int id,string fixture)
        {
            var batch=m.MovementRoutesForProjection(id);
            Ok(batch.Length==121,"batch destination count");
            for(int target=0;target<121;target++)
                Ok(batch[target].SequenceEqual(m.MovementRoute(id,target)),fixture+" piece="+id+" target="+target);
        }
        static Match Game(int count)
        {
            var main=Enumerable.Range(0,count).Select(_=>Enumerable.Repeat("route-unit",20).ToArray()).ToArray();
            var terrains=Enumerable.Range(0,count).Select(_=>Enumerable.Repeat("route-land",20).ToArray()).ToArray();
            var m=new Match(catalog,effects,count,false,17,main,terrains){AutomaticResponses=true,AutoAdvanceAfterTerrain=true};
            Ok(m.Try(new Command {player=m.Controller,revision=m.Revision,kind=ActionKind.DrawMain,pile=0},out var drawError),drawError);
            Ok(m.Try(new Command {player=m.Controller,revision=m.Revision,kind=ActionKind.Place,pile=0,target=Match.Capital(m.Active,count)},out var placeError),placeError);
            foreach(var cell in m.Board){cell.Terrain=null;cell.Owner=-1;cell.CapitalOwner=-1;cell.pieces.Clear();}
            return m;
        }
        static void Land(Match m,int at){m.Board[at].Terrain=catalog.Get("route-land");m.Board[at].Owner=m.Active;}
        static Definition Terrain(string id,string rule)=>new Definition(new CardData {id=id,name=id,kind="Terrain",rule=rule},"fixture");
        static Definition Card(string id,string kind="Creature",string[] traits=null)=>new Definition(new CardData {
            id=id,name=id,kind=kind,traits=traits??Array.Empty<string>(),movement=2
        },"fixture");
        static Piece Add(Match m,int at,int owner,Definition card)
        {
            var p=new Piece(1000+m.Board.Sum(c=>c.Pieces.Count),owner,card){Game=m};m.Board[at].pieces.Add(p);return p;
        }
        static void Ok(bool value,string message){if(!value)throw new Exception("MOVEMENT PROJECTION CHECK: "+message);checks++;}
    }
}
