using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
    // OR inside each selection group; AND across groups. An empty group means all.
    public sealed class CardFilter
    {
        public readonly HashSet<CardType> Types=new HashSet<CardType>();
        public readonly HashSet<int> Colors=new HashSet<int>(),Costs=new HashSet<int>();
        public readonly HashSet<string> Rarities=new HashSet<string>(StringComparer.OrdinalIgnoreCase),Expansions=new HashSet<string>(StringComparer.Ordinal);
        public bool OwnedOnly,PlayableOnly,CommandersOnly,CommanderIdentityOnly;
        public int SelectedCount=>Types.Count+Colors.Count+Costs.Count+Rarities.Count+Expansions.Count+(OwnedOnly?1:0)+(PlayableOnly?1:0)+(CommandersOnly?1:0)+(CommanderIdentityOnly?1:0);
        public void Clear(){Types.Clear();Colors.Clear();Costs.Clear();Rarities.Clear();Expansions.Clear();OwnedOnly=PlayableOnly=CommandersOnly=CommanderIdentityOnly=false;}
        public static int[] CardColors(Definition c)
        {
            var colors=c.IdentityColors.Count>0?c.IdentityColors.ToArray():c.ColoredCost.Select((v,i)=>v>0&&i<6?i:-1).Where(i=>i>=0).Concat(c.Color<6?new[]{c.Color}:Array.Empty<int>()).Distinct().ToArray();
            return colors.Length==0?new[]{6}:colors;
        }
        public bool Matches(Definition c,bool owned,string search="",Definition commander=null)
        {
            if(OwnedOnly&&!owned||PlayableOnly&&!c.Playable||CommandersOnly&&!c.IsCommander)return false;
            if(Types.Count>0&&!Types.Contains(c.Kind)||Colors.Count>0&&!CardColors(c).Any(Colors.Contains))return false;
            if(Costs.Count>0&&!Costs.Contains(Math.Min(6,c.TotalCost))||Rarities.Count>0&&!Rarities.Contains(c.Rarity)||Expansions.Count>0&&!Expansions.Contains(c.Expansion))return false;
            if(CommanderIdentityOnly&&commander!=null&&CardColors(c).Any(color=>color<6&&!commander.IdentityColors.Contains(color)))return false;
            return string.IsNullOrWhiteSpace(search)||(c.Name+" "+c.Id+" "+c.Text+" "+c.Expansion+" "+string.Join(" ",c.Subtypes)).IndexOf(search.Trim(),StringComparison.OrdinalIgnoreCase)>=0;
        }
    }
}
