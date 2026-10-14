using System;
using System.Globalization;
using System.Linq;

namespace TCG.Foundation
{
    public static class NumericKeywordRules
    {
        public static bool TryParse(string keyword, out string name, out int amount)
        {
            name = ""; amount = 0;
            if (string.IsNullOrEmpty(keyword)) return false;
            int split = keyword.IndexOf(':');
            if (split <= 0 || split == keyword.Length - 1) return false;
            string prefix = keyword.Substring(0, split), number = keyword.Substring(split + 1);
            if (prefix != KeywordRules.Armor && prefix != KeywordRules.Tenacity && prefix != KeywordRules.Barrier) return false;
            if (number.Any(c => c < '0' || c > '9') || !int.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out amount) || amount < 1 || amount > 1000) return false;
            name = prefix; return true;
        }
        public static bool Valid(string keyword) => TryParse(keyword, out _, out _);
    }

    public sealed partial class Match
    {
        int NumericKeyword(Piece p, string name)
        {
            int result = 0;
            foreach (string keyword in p.Card.Traits.Concat(EffectiveKeywords(p.Id)))
                if (NumericKeywordRules.TryParse(keyword, out string kind, out int amount) && kind == name) result = Math.Max(result, amount);
            return result;
        }
        internal int RemainingBarrier(Piece p) => Math.Max(0, NumericKeyword(p, KeywordRules.Barrier) - p.BarrierSpent);
        int ApplyNumericDefenses(Piece p, int amount, bool combat)
        {
            int reduction = NumericKeyword(p, KeywordRules.Tenacity) + (combat ? NumericKeyword(p, KeywordRules.Armor) : 0);
            int remaining = Math.Max(0, amount - reduction);
            int prevented = Math.Min(remaining, RemainingBarrier(p));
            p.BarrierSpent += prevented;
            return remaining - prevented;
        }
    }
}
