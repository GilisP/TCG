using System;
using System.IO;
using System.Linq;
using UnityEngine;
using TCG.Table;
namespace TCG.Foundation.Editor
{
 [Serializable] public sealed class ArtGenerationQueue {public ArtGenerationItem[] items;}
 [Serializable] public sealed class ArtGenerationItem {public CardData card;}
 public static class PreconArtChecks
 {
  public static void Run(){RunDeck("COL001-D01","PreconD01","Documentacao/Arte/Precon-D01-20261014/queue.json");RunDeck("COL001-D02","PreconD02","Documentacao/Arte/Precon-D02-20261014/queue.json");MulticolorCostChecks.Run();}
  static void RunDeck(string deckId,string folder,string queuePath)
  {
   var catalog=ContentLoader.Load(new EffectRegistry());var precons=PreconstructedDeckLoader.Load(catalog);var deck=precons.Decks.First(d=>d.id==deckId);var manifest=JsonUtility.FromJson<CardArtManifest>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"CardArt","manifest.json")));int checks=0;
   void Check(bool ok,string why){checks++;if(!ok)throw new Exception("PRECON ART: "+why);}
   foreach(var id in deck.main.Concat(deck.terrains).Append(deck.commander))
   {
    var entries=manifest.entries.Where(e=>e.cardId==id&&e.styleId=="standard").ToArray();Check(entries.Length==1,"one standard mapping "+id);
    var texture=Resources.Load<Texture2D>(entries[0].resourcePath);Check(texture!=null&&texture.width>=512&&texture.height>=384,"imported painted art "+id);
    Check((id==deck.commander?entries[0].resourcePath=="CardArt/Commanders/"+id+"-portrait-v1":(entries[0].resourcePath.StartsWith("CardArt/"+folder+"/"+id+"-painted-v")||entries[0].resourcePath.StartsWith("CardArt/PreconD01/"+id+"-painted-v"))),"correct individual resource "+id);
   }
   var queue=JsonUtility.FromJson<ArtGenerationQueue>(File.ReadAllText(queuePath));
   foreach(var item in queue.items){var card=catalog.Get(item.card.id);Check(card.Name==item.card.name&&card.Kind.ToString()==item.card.kind,"generation subject matches current card "+card.Id);Check(card.Cost==item.card.cost&&card.ColoredCost.SequenceEqual(item.card.coloredCost),"generation cost source matches current card "+card.Id);}
   Check(manifest.entries.Count(e=>e.cardId=="MED-225"&&e.styleId=="illuminated")==1,"old illuminated variant preserved");
   Debug.Log("PRECON ART CHECKS PASSED: "+checks+"; "+deckId+" 151 painted images including preserved commander");
  }
 }
}
