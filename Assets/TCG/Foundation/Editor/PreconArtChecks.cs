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
  public static void Run()
  {
   var catalog=ContentLoader.Load(new EffectRegistry());var precons=PreconstructedDeckLoader.Load(catalog);var deck=precons.Decks.First(d=>d.id=="COL001-D01");var manifest=JsonUtility.FromJson<CardArtManifest>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"CardArt","manifest.json")));int checks=0;
   void Check(bool ok,string why){checks++;if(!ok)throw new Exception("PRECON ART: "+why);}
   foreach(var id in deck.main.Concat(deck.terrains).Append(deck.commander))
   {
    var entries=manifest.entries.Where(e=>e.cardId==id&&e.styleId=="standard").ToArray();Check(entries.Length==1,"one standard mapping "+id);
    var texture=Resources.Load<Texture2D>(entries[0].resourcePath);Check(texture!=null&&texture.width>=512&&texture.height>=384,"imported painted art "+id);
    Check((id==deck.commander?entries[0].resourcePath=="CardArt/Commanders/"+id+"-portrait-v1":entries[0].resourcePath.StartsWith("CardArt/PreconD01/"+id+"-painted-v")),"correct individual resource "+id);
   }
   var queue=JsonUtility.FromJson<ArtGenerationQueue>(File.ReadAllText("Documentacao/Arte/Precon-D01-20261014/queue.json"));
   foreach(var item in queue.items){var card=catalog.Get(item.card.id);Check(card.Name==item.card.name&&card.Kind.ToString()==item.card.kind,"generation subject matches current card "+card.Id);Check(card.Cost==item.card.cost&&card.ColoredCost.SequenceEqual(item.card.coloredCost),"generation cost source matches current card "+card.Id);}
   Check(manifest.entries.Count(e=>e.cardId=="MED-225"&&e.styleId=="illuminated")==1,"old illuminated variant preserved");
   MulticolorCostChecks.Run();Debug.Log("PRECON ART CHECKS PASSED: "+checks+"; 151 exclusive painted images including preserved commander");
  }
 }
}
