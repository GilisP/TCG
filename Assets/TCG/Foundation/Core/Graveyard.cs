using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
    // Per-entry provenance survives equal definitions, removals and later re-entry.
    public sealed class GraveEntry
    {
        public int Id {get;} public Definition Card {get;} public bool FromLibrary {get;}
        internal GraveEntry(int id,Definition card,bool milled){Id=id;Card=card;FromLibrary=milled;}
    }
    public sealed class Graveyard : IEnumerable<Definition>
    {
        readonly List<GraveEntry> entries=new List<GraveEntry>(); int next;
        public int Count=>entries.Count;
        public IEnumerable<GraveEntry> Milled=>entries.Where(x=>x.FromLibrary);
        public void Add(Definition card)=>entries.Add(new GraveEntry(next++,card,false));
        public GraveEntry AddMilled(Definition card){var entry=new GraveEntry(next++,card,true);entries.Add(entry);return entry;}
        public bool ContainsEntry(int id)=>entries.Any(x=>x.Id==id);
        public void AddRange(IEnumerable<Definition> cards){foreach(var c in cards)Add(c);}
        public bool Contains(Definition card)=>entries.Any(x=>x.Card==card);
        public bool Remove(Definition card){int at=entries.FindIndex(x=>x.Card==card);if(at<0)return false;entries.RemoveAt(at);return true;}
        public bool RemoveEntry(int id){int at=entries.FindIndex(x=>x.Id==id);if(at<0)return false;entries.RemoveAt(at);return true;}
        public void Clear()=>entries.Clear();
        public IReadOnlyList<Definition> AsReadOnly()=>entries.Select(x=>x.Card).ToArray();
        public IEnumerator<Definition> GetEnumerator()=>entries.Select(x=>x.Card).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
    }
}
