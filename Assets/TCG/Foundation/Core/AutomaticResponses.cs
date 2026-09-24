using System.Linq;

namespace TCG.Foundation
{
    public sealed partial class Match
    {
        // Presentation opts in; explicit-pass fixtures can still exercise the underlying protocol.
        public bool AutomaticResponses { get; set; }

        public bool HasResponse()
        {
            if(Over||Choice!=null||Defense!=null)return false;
            return All.Any(MonkResponse)||ExiledFor(Priority).Any(x=>x.Card.Kind==CardType.Instant&&CanCastExiled(x.Id))||seats[Priority].hand.Any(card=>card.Kind==CardType.Instant && ResponseTargets(card,Priority) &&
                (NeedsTarget(card) ? All.Any(p=>CanPlay(card,Position(p.Id),p.Id)) : CanPlay(card,-1)));
        }

        void AdvanceAutomaticResponses()
        {
            while(AutomaticResponses&&!Over&&Choice==null&&Defense==null)
            {
                // The active player keeps their main phase, movement and normal actions.
                if(stack.Count==0&&(Phase!=Stage.End&&(Phase!=Stage.Main||Priority==Active)))return;
                if(HasResponse())return;
                Pass(); Drain(); StateCheck(); FinishCheck();
            }
        }

        bool ResponseTargets(Definition card,int owner)
        {
            var allies=All.Where(p=>p.Card.Kind==CardType.Creature&&Allied(owner,p.Owner));
            switch(card.Rule)
            {
                case "test-counter":return stack.Any(x=>x.Rule==null&&x.Card!=null&&(x.Card.Kind==CardType.Spell||x.Card.Kind==CardType.Instant));
                case "guard":case "hold":case "attack-buff":case "range-buff":
                case "prevent-three":case "formation-prevent":return allies.Any();
                case "blood-price":return allies.Any(p=>p.Owner==owner);
                case "reduce-attack":return All.Any(p=>p.Card.Kind==CardType.Creature&&Enemies(owner,p.Owner));
                case "move-ally":return allies.Any(p=>!Immune(p,owner)&&Neighbors(Position(p.Id)).Any(n=>cells[n].Terrain!=null&&!Blocked(p,n)));
                case "swap":return allies.Any(p=>!Immune(p,owner)&&allies.Any(q=>q!=p&&!Immune(q,owner)&&Distance(Position(p.Id),Position(q.Id))<=3));
                case "death-burst":
                    if(!enemyDeaths.Any(d=>d.owner==owner&&d.turn==Turn))return false;
                    var death=enemyDeaths.Last(d=>d.owner==owner&&d.turn==Turn);
                    return All.Any(p=>p.Card.Kind==CardType.Creature&&Enemies(owner,p.Owner)&&Adjacent(death.cell,Position(p.Id)));
                // Unknown future programs remain available: never silently discard an unclassified response.
                default:return true;
            }
        }
    }
}
