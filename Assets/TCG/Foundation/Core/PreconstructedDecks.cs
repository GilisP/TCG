using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
 public sealed class PreconstructedDeckLibrary
 {
  readonly Dictionary<PlannedDeckData,string[]> unavailable=new Dictionary<PlannedDeckData,string[]>();
  public IReadOnlyList<PlannedDeckData> Decks {get;}
  public IReadOnlyList<string> Errors(PlannedDeckData template)=>unavailable.TryGetValue(template,out var errors)?Array.AsReadOnly(errors):throw new InvalidOperationException("Precon não pertence ao manifesto validado.");
  public bool Ready(PlannedDeckData template)=>Errors(template).Count==0;
  public PreconstructedDeckLibrary(ContentCatalog catalog,PlannedDeckFile file)
  {
   if(catalog==null)throw new ArgumentNullException(nameof(catalog));
   if(file==null||file.version!=1||file.decks==null||file.decks.Length!=10)throw new InvalidOperationException("São necessários dez precons oficiais, versão 1.");
   var expected=Enumerable.Range(1,10).Select(i=>"COL001-D"+i.ToString("00")).ToArray();
   if(!file.decks.Select(d=>d?.id).OrderBy(x=>x,StringComparer.Ordinal).SequenceEqual(expected))throw new InvalidOperationException("IDs de precons oficiais inválidos ou repetidos.");
   var validator=new CollectionLibrary(catalog,new CollectionData{owned=catalog.Cards.Select(c=>c.Id).ToList()});
   foreach(var template in file.decks)
   {
    if(string.IsNullOrWhiteSpace(template.name)||template.name.Length>80||template.main==null||template.terrains==null||template.main.Length!=100||template.terrains.Length!=50||string.IsNullOrWhiteSpace(template.commander))throw new InvalidOperationException("Precon incompleto: "+template.id);
    // Resolve every identity before considering availability. Unknown cards and malformed lists remain fatal.
    var cards=template.main.Concat(template.terrains).Append(template.commander).Select(id=>catalog.Get(catalog.Identity(id))).ToArray();
    if(cards.Select(c=>catalog.Identity(c.Id)).Distinct().Count()!=151)throw new InvalidOperationException(template.name+": precon exige 100 + 50 sem repetições e comandante separado.");
    var blocked=cards.Where(c=>!c.Playable).Select(c=>c.Name+": "+c.UnavailableReason).ToArray();
    var permitted=new HashSet<string>(cards.Where(c=>!c.Playable).Select(c=>c.Name+": "+c.UnavailableReason));
    var commander=catalog.Get(catalog.Identity(template.commander));if(!commander.Playable)permitted.Add("Comandante indisponível: "+commander.UnavailableReason);
    var structural=validator.Validate(Create(template),true).Where(e=>!permitted.Contains(e)).ToArray();
    if(structural.Length>0)throw new InvalidOperationException(template.name+": "+string.Join("; ",structural));
    unavailable.Add(template,blocked);
   }
   Decks=Array.AsReadOnly(file.decks);
  }
  public static DeckData Create(PlannedDeckData template){var deck=template.Create();deck.id="precon-"+template.id;return deck;}
  public DeckData Import(CollectionLibrary library,PlannedDeckData template)
  {
   if(!Ready(template))throw new InvalidOperationException(template.name+": "+string.Join("; ",Errors(template)));
   var candidate=Create(template);var existing=library.Data.decks.FirstOrDefault(d=>d.id==candidate.id);if(existing!=null)return existing.Copy();
   foreach(var id in candidate.main.Concat(candidate.terrains).Append(candidate.commander))library.Collect(id);
   library.SaveDeck(candidate);return candidate.Copy();
  }
 }
}
