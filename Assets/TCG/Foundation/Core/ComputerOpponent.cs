using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
    // Consumes the same private projection as a human seat, never the authoritative Match.
    public sealed class ComputerOpponent
    {
        readonly Random random;readonly HashSet<string> used=new HashSet<string>();int turn=-1,lastRevision=-1;Command cached;
        public ComputerOpponent(int seed){random=new Random(seed);}
        static int Distance(int a,int b)=>Math.Abs(a%11-b%11)+Math.Abs(a/11-b/11);
        public Command Choose(MatchView view,IReadOnlyList<Command> legal,ContentCatalog catalog)
        {
            if(view==null||legal==null||catalog==null)throw new ArgumentNullException();
            if(view.over||view.controller!=view.viewer||legal.Count==0)return null;
            if(lastRevision==view.revision&&cached!=null&&legal.Contains(cached))return cached;
            if(turn!=view.turn){turn=view.turn;used.Clear();}
            int me=view.viewer;
            bool Enemy(int owner)=>owner>=0&&owner<view.seats.Length&&!view.seats[owner].eliminated&&view.seats[owner].team!=view.seats[me].team;
            int Goal(int at)=>Enumerable.Range(0,view.cells.Length).Where(i=>Enemy(view.cells[i].capital)).Select(i=>Distance(at,i)).DefaultIfEmpty(0).Min();
            NetPiece Piece(int id)=>view.pieces.FirstOrDefault(p=>p.id==id);
            string Once(Command c)=>c.kind+":"+c.unit+":"+c.target;
            double TerrainManaValue(int pile)
            {
                string top=view.seats[me].tops[pile];if(string.IsNullOrEmpty(top))return 0;
                var terrain=catalog.Get(top);var available=view.seats[me].mana;
                var wanted=view.hand.Select(catalog.Get).ToList();
                if(view.seats[me].ready&&!string.IsNullOrEmpty(view.seats[me].commander))wanted.Add(catalog.Get(view.seats[me].commander));
                double Need(int color)=>wanted.Select(card=>Math.Max(0,card.ColoredCost[color]-available[color])).DefaultIfEmpty(0).Max();
                // Random-mana terrain is valued by its expected usefulness, without
                // predicting the authority's random generator or future deck cards.
                double need=terrain.Rule=="portals"||terrain.Traits.Contains("precon-random-mana")?Enumerable.Range(0,6).Average(Need):Need(terrain.Color);
                return Math.Min(6,need*3);
            }
            double Score(Command c)
            {
                var p=Piece(c.unit);var target=Piece(c.target);
                switch(c.kind)
                {
                    case ActionKind.Choose:return 20;
                    case ActionKind.DrawMain:return 60-view.seats[me].pileCounts[c.pile];
                    case ActionKind.DrawTerrain:return view.turn==1&&view.hand.Length<8?40:60-view.seats[me].pileCounts[c.pile];
                    case ActionKind.Place:
                        return 70-Goal(c.target)*2+TerrainManaValue(c.pile)+(view.cells[c.target].terrain==null?10:view.cells[c.target].terrain==Match.Ruins.Id?8:-20)-view.pieces.Where(q=>q.cell==c.target).Sum(q=>q.owner==me?40:0);
                    case ActionKind.SummonCommander:return 55-Goal(c.target);
                    case ActionKind.Play:
                        var card=catalog.Get(c.card);var victim=Piece(c.unit);
                        if(card.Permanent)return 40+card.Attack+card.Defense*.5-(c.target>=0?Goal(c.target):0);
                        return 25+(victim!=null&&Enemy(victim.owner)?victim.attack+Math.Max(0,victim.defense-victim.damage):0);
                    case ActionKind.CastExiled:return 25;
                    case ActionKind.Move:return p==null?-100:Goal(p.cell)-Goal(c.target)>0?15+Goal(p.cell)-Goal(c.target):-30;
                    case ActionKind.Attack:
                        int power=c.units.Sum(id=>Piece(id)?.attack??0);int guard=view.pieces.Where(q=>q.cell==c.target&&Enemy(q.owner)).Sum(q=>Math.Max(0,q.defense-q.damage));
                        return Enemy(view.cells[c.target].capital)?80+power:power>=guard?45+power-guard:2+power-guard;
                    case ActionKind.Defend:
                        var battle=view.stack.Last();double value=0;var damage=new Dictionary<int,int>();
                        for(int i=0;i<c.blockers.Length;i++){int id=c.blockers[i];int hit=Piece(battle.attackers[i])?.attack??0;if(id<0){value-=hit*3;continue;}var blocker=Piece(id);if(blocker==null)continue;damage[id]=(damage.TryGetValue(id,out int before)?before:0)+hit;value+=Math.Min(blocker.attack,Piece(battle.attackers[i])?.defense??0);}
                        foreach(var pair in damage){var blocker=Piece(pair.Key);if(pair.Value>=blocker.defense-blocker.damage)value-=blocker.attack+blocker.defense;}
                        return battle.chooser==battle.defender?value:-value;
                    case ActionKind.Equip:return 30;
                    case ActionKind.BoardVehicle:return used.Contains(Once(c))||used.Contains(ActionKind.DisembarkVehicle+":"+c.unit+":-1")?-40:target!=null&&p!=null&&view.pieces.Count(q=>q.carrier==target.id)<target.card.vehicleCrew?12:-20;
                    case ActionKind.DisembarkVehicle:
                        var carrier=p==null?null:Piece(p.carrier);
                        return used.Contains(Once(c))||carrier==null?-40:Goal(carrier.cell)<=1&&view.pieces.Count(q=>q.carrier==carrier.id)>carrier.card.vehicleCrew?12:-15;
                    case ActionKind.Activate:case ActionKind.ActivateTerrain:return used.Contains(Once(c))?-40:18;
                    case ActionKind.NextPhase:return -5;
                    case ActionKind.Pass:return -4;
                    default:return -100;
                }
            }
            double best=double.NegativeInfinity;var candidates=new List<Command>();
            foreach(var c in legal){double score=Score(c);if(score>best){best=score;candidates.Clear();}if(score==best)candidates.Add(c);}
            cached=candidates[random.Next(candidates.Count)];lastRevision=view.revision;
            if(cached.kind==ActionKind.Activate||cached.kind==ActionKind.ActivateTerrain||cached.kind==ActionKind.BoardVehicle||cached.kind==ActionKind.DisembarkVehicle)used.Add(Once(cached));
            return cached;
        }
    }
}
