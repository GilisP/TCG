using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
    [Serializable] public sealed class PlannedDeckData
    {
        public string id,name,commander,description;public string[] main,terrains;
        public DeckData Create()=>new DeckData{id="planned-"+id,name=name,commander=commander,experimental=false,main=main.ToList(),terrains=terrains.ToList()};
    }
    [Serializable] public sealed class PlannedDeckFile {public int version=1;public PlannedDeckData[] decks=Array.Empty<PlannedDeckData>();}
    public sealed class PlannedDeckLibrary
    {
        public IReadOnlyList<PlannedDeckData> Decks {get;}
        public PlannedDeckLibrary(ContentCatalog catalog,PlannedDeckFile file)
        {
            if(file==null||file.version!=1||file.decks==null)throw new InvalidOperationException("Formato de decks planejados inválido.");
            if(file.decks.Any(d=>d==null||string.IsNullOrWhiteSpace(d.id)||string.IsNullOrWhiteSpace(d.name)||d.main==null||d.terrains==null)||file.decks.Select(d=>d.id).Distinct().Count()!=file.decks.Length)throw new InvalidOperationException("Deck planejado incompleto ou duplicado.");
            var validator=new CollectionLibrary(catalog,new CollectionData{owned=catalog.Cards.Select(c=>c.Id).ToList()});
            foreach(var d in file.decks){var errors=validator.Validate(d.Create(),true);if(d.main.Concat(d.terrains).Select(catalog.Identity).Distinct().Count()!=150)errors.Add("Lista planejada repetiu uma identidade.");if(errors.Count>0)throw new InvalidOperationException(d.name+": "+string.Join("; ",errors));}
            Decks=Array.AsReadOnly(file.decks);
        }
    }
    public sealed partial class CollectionLibrary
    {
        // Explicit test import only. Existing deck edits and all economy/cosmetics are preserved.
        public DeckData ImportPlannedDeck(PlannedDeckData template)
        {
            if(template==null)throw new ArgumentNullException(nameof(template));
            var candidate=template.Create();var existing=Data.decks.FirstOrDefault(d=>d.id==candidate.id);
            if(existing!=null)return existing.Copy();
            var owned=new HashSet<string>(Data.owned);foreach(var id in candidate.main.Concat(candidate.terrains).Concat(new[]{candidate.commander}))owned.Add(catalog.Identity(id));
            var validation=new CollectionLibrary(catalog,new CollectionData{owned=owned.ToList()}).Validate(candidate,true);
            if(validation.Count>0)throw new InvalidOperationException(string.Join("; ",validation));
            foreach(var id in candidate.main.Concat(candidate.terrains).Concat(new[]{candidate.commander}))Collect(id);
            SaveDeck(candidate);return candidate.Copy();
        }
    }
}
