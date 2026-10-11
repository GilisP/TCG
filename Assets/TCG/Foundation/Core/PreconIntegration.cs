using System;
using System.Linq;
namespace TCG.Foundation
{
    public sealed partial class Match
    {
        readonly System.Collections.Generic.Dictionary<int,int> preconEffectOwners=new System.Collections.Generic.Dictionary<int,int>();
        int PreconDeathOwner(Piece p)
        {
            if(p.Defense<=0)
            {
                if(p.Modifiers.Any(m=>m.Defense<0)&&preconEffectOwners.TryGetValue(p.Id,out int caster))return caster;
                int at=Position(p.Id);if(Valid(at)&&seats[p.Owner].grave.Any(c=>c.Kind==CardType.Creature))
                {var tileAura=cells[at].pieces.LastOrDefault(e=>e.Card.Traits.Contains("pca-tile-enchantment"));if(tileAura!=null)return tileAura.Owner;}
            }
            return lastDamageSource.TryGetValue(p.Id,out var source)?source.Owner:-1;
        }
        bool PreconNoAttack(Piece p)
        {
            foreach(var source in All)foreach(var a in source.Card.Abilities)
                if(a.trigger=="aura"&&a.ops.Any(o=>o.kind=="noAttack")&&AbilityTarget(a,new Pending{Owner=source.Owner,Source=source,SourceCell=Position(source.Id)},p,true)&&!(source==p&&PreconUnitCanAttack(p)))return true;
            return false;
        }
        bool? PreconEnchantmentPlacement(Definition card,int owner,int tile,int unit)
        {
            var author=PreconAuthorEnchantmentPlacement(card,owner,tile,unit);
            if(author.HasValue)return author;
            if(!card.Traits.Contains("pcu-global"))return null;
            return unit<0&&Valid(tile)&&cells[tile].Terrain!=null&&cells[tile].Owner==owner;
        }
    }
}
