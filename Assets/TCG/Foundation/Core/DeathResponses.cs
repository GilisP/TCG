using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
    public sealed partial class Match
    {
        readonly HashSet<int> pendingDeaths=new HashSet<int>();readonly Dictionary<int,int> deathKillers=new Dictionary<int,int>();
        bool OfferDeathResponse(Piece p,int killer)
        {
            if(p==null||Position(p.Id)<0||p.Card.Kind!=CardType.Creature)return false;if(pendingDeaths.Contains(p.Id))return true;
            var card=seats[p.Owner].hand.FirstOrDefault(c=>c.Rule=="destiny"&&c.Playable&&CanPay(p.Owner,c));
            var permission=card==null?exilePermissions.FirstOrDefault(x=>x.Owner==p.Owner&&x.Card.Rule=="destiny"&&(x.AnyMana?seats[p.Owner].mana.Sum()>=x.Card.TotalCost:CanPay(p.Owner,x.Card))):null;
            if(card==null&&permission!=null)card=permission.Card;
            if(card==null||!All.Any(q=>q!=p&&q.Owner==p.Owner&&q.Card.Kind==CardType.Creature&&q.Health>0&&Adjacent(Position(q.Id),Position(p.Id))))return false;
            pendingDeaths.Add(p.Id);deathKillers[p.Id]=killer;
            Ask(p.Owner,p.Card.Name+" morreria. Jogar Troca de Destino?",new[]{new ChoiceOption(1,"Jogar Troca de Destino"),new ChoiceOption(0,"Não usar")},key=>{
                if(key!=1||(permission==null?!seats[p.Owner].hand.Contains(card):!exilePermissions.Contains(permission))||!(permission!=null&&permission.AnyMana?seats[p.Owner].mana.Sum()>=card.TotalCost:CanPay(p.Owner,card))){FinishPendingDeath(p);return;}
                if(permission!=null&&permission.AnyMana)SpendGeneric(p.Owner,card.TotalCost);else Pay(p.Owner,card);if(permission==null)seats[p.Owner].hand.Remove(card);else{seats[permission.CardOwner].exile.Remove(card);exilePermissions.Remove(permission);}stack.Add(new Pending{Owner=p.Owner,CardOwner=permission?.CardOwner??p.Owner,Card=card,Rule="destiny",Subject=p,SourceCell=Position(p.Id)});passes=0;
                Visual?.Invoke(new MatchEvent("cast",Capital(p.Owner,seats.Length),Capital(p.Owner,seats.Length),-1,card,p.Owner));
            });return true;
        }
        void FinishPendingDeath(Piece p)
        {pendingDeaths.Remove(p.Id);int killer=deathKillers.TryGetValue(p.Id,out var value)?value:-1;deathKillers.Remove(p.Id);CommitKill(p,killer);}
        void ResolveDeathResponse(Pending item)
        {
            var protectedPiece=item.Subject;if(protectedPiece==null||Find(protectedPiece.Id)==null){if(protectedPiece!=null){pendingDeaths.Remove(protectedPiece.Id);deathKillers.Remove(protectedPiece.Id);}FinishSpell(item);return;}
            work.Enqueue(()=>{
                int at=Position(protectedPiece.Id);var options=All.Where(q=>q!=protectedPiece&&q.Owner==item.Owner&&q.Card.Kind==CardType.Creature&&q.Health>0&&Adjacent(Position(q.Id),at)).ToArray();
                if(options.Length==0){FinishPendingDeath(protectedPiece);FinishSpell(item);return;}
                Choice=new RuleChoice(item.Owner,"Criatura que morrerá no lugar",options.Select(q=>new ChoiceOption(q.Id,q.Card.Name)),key=>{var substitute=Find(key);if(substitute==null){FinishPendingDeath(protectedPiece);FinishSpell(item);return;}if(protectedPiece.Health<=0)protectedPiece.Damage=protectedPiece.BeforeLethalDamage;protectedPiece.CombatLethal=false;pendingDeaths.Remove(protectedPiece.Id);deathKillers.Remove(protectedPiece.Id);CommitKill(substitute);FinishSpell(item);Preview(item,at);});
            });
        }
        bool PortalLink(int from,int to)=>from!=to&&TerrainRule(from,"portals")&&TerrainRule(to,"portals");
        IEnumerable<int> MovementNeighbors(Piece p,int from)=>Neighbors(from).Concat(TerrainRule(from,"portals")?Enumerable.Range(0,121).Where(n=>PortalLink(from,n)):Enumerable.Empty<int>()).Distinct();
    }
}
