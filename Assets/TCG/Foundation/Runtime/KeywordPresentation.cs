using System;
using System.Linq;
using TCG.Foundation;

namespace TCG.Table
{
    // Labels only; validation and all numerical rules remain in the host engine.
    public static class KeywordPresentation
    {
        public static string Label(string key)
        {
            if(key=="magic-resistance")return "Resistência à magia";
            if(key=="anchored")return "Ancorado";
            var parts=(key??"").Split(':');if(parts.Length!=2)return "";
            string name=parts[0]=="armor"?"Armadura":parts[0]=="tenacity"?"Tenacidade":parts[0]=="barrier"?"Barreira":"";
            return name.Length==0?"":name+" "+parts[1];
        }
        public static string CardRules(Definition card)
        {
            string text=card.Text??"";
            var labels=card.Keywords.Select(Label).Where(s=>s.Length>0&&text.IndexOf(s,StringComparison.OrdinalIgnoreCase)<0).Distinct().ToArray();
            return labels.Length==0?text:string.Join(" · ",labels)+(text.Length>0?"\n"+text:"");
        }
    }
}
