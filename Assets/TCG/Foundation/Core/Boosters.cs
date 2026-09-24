using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
 [Serializable] public sealed class CardLook {public bool foil; public string cardId,styleId="standard";public CardLook Copy()=>new CardLook{cardId=cardId,styleId=styleId,foil=foil};}
 public static class CardStyles
 {
  public static readonly string[] All={"standard","illuminated","nocturne"};
  public static bool Valid(string id)=>All.Contains(id);
  public static string Name(string id)=>id=="illuminated"?"Iluminura":id=="nocturne"?"Nocturna":"Clássica";
  public static string Key(string card,string style)=>card+"|"+style;
 }
 [Serializable] public sealed class BoosterDefinition
 {
  public string id="medieval",name="Crônicas dos Reinos";public int price=100,cards=5,duplicateRefund=10;
  public string[] expansions=Array.Empty<string>();public int standardWeight=80,illuminatedWeight=15,nocturneWeight=5;
  public void Validate(){if(string.IsNullOrWhiteSpace(id)||price<0||cards<1||cards>20||duplicateRefund<0||standardWeight<0||illuminatedWeight<0||nocturneWeight<0||(long)standardWeight+illuminatedWeight+nocturneWeight<=0||(long)standardWeight+illuminatedWeight+nocturneWeight>100000||expansions==null)throw new InvalidOperationException("Configuração de booster inválida.");}
  public Definition[] Pool(ContentCatalog catalog){Validate();if(expansions.Any(x=>!catalog.Expansions.ContainsKey(x)))throw new InvalidOperationException("Expansão do booster ausente.");return catalog.Cards.Where(c=>c.Playable&&(expansions.Length==0||expansions.Contains(c.Expansion))).OrderBy(c=>c.Id,StringComparer.Ordinal).ToArray();}
 }
 [Serializable] public sealed class BoosterReward {public string cardId,styleId;public bool duplicate;public int refund;}
 [Serializable] public sealed class BoosterReceipt {public string id,boosterId;public int price,refund;public List<BoosterReward> rewards=new List<BoosterReward>();}
 public sealed partial class CollectionLibrary
 {
  public bool OwnsVariant(string card,string style)=>CardStyles.Valid(style)&&Owns(card)&&(style=="standard"?true:Data.variants.Contains(CardStyles.Key(catalog.Identity(card),style)));
  public bool SelectedFoil(string card,DeckData deck=null)
  {
   string id=catalog.Identity(card);var look=deck?.cardLooks?.LastOrDefault(x=>x.cardId==id)??Data.appearances.LastOrDefault(x=>x.cardId==id);
   return look?.foil??catalog.Get(id).Foil;
  }
  public void SelectFoil(string card,bool foil,DeckData deck=null)
  {
   if(!Owns(card))throw new InvalidOperationException("Carta ausente da coleção.");string id=catalog.Identity(card),style=SelectedStyle(card,deck);
   var list=deck?.cardLooks??Data.appearances;list.RemoveAll(x=>x.cardId==id);list.Add(new CardLook{cardId=id,styleId=style,foil=foil});
  }
  public string SelectedStyle(string card,DeckData deck=null)
  {
   string id=catalog.Identity(card);var look=deck?.cardLooks?.LastOrDefault(x=>x.cardId==id)??Data.appearances.LastOrDefault(x=>x.cardId==id);
   return look!=null&&OwnsVariant(id,look.styleId)?look.styleId:"standard";
  }
  public void SelectStyle(string card,string style,DeckData deck=null)
  {
   string id=catalog.Identity(card);if(!OwnsVariant(id,style))throw new InvalidOperationException("Esta variante ainda não está na coleção.");
   bool shine=SelectedFoil(card,deck);var list=deck?.cardLooks??Data.appearances;list.RemoveAll(x=>x.cardId==id);list.Add(new CardLook{cardId=id,styleId=style,foil=shine});
  }
  public BoosterReceipt OpenBooster(BoosterDefinition pack,Random random)
  {
   if(pack==null||random==null)throw new ArgumentNullException();var pool=pack.Pool(catalog);if(pool.Length==0)throw new InvalidOperationException("Booster sem cartas disponíveis.");if(Data.coins<pack.price)throw new InvalidOperationException("Moedas insuficientes.");
   var owned=new HashSet<string>(Data.owned.Select(IdentityOrOriginal));var variants=new HashSet<string>(Data.variants);var looks=Data.appearances.Select(x=>x.Copy()).ToList();
   var result=new BoosterReceipt{id=Guid.NewGuid().ToString("N"),boosterId=pack.id,price=pack.price};
   int total=pack.standardWeight+pack.illuminatedWeight+pack.nocturneWeight;
   for(int n=0;n<pack.cards;n++)
   {
    string card=catalog.Identity(pool[random.Next(pool.Length)].Id);int roll=random.Next(total);string style=roll<pack.standardWeight?"standard":roll<pack.standardWeight+pack.illuminatedWeight?"illuminated":"nocturne";
    string key=CardStyles.Key(card,style);bool duplicate=style=="standard"?owned.Contains(card):variants.Contains(key);bool newCard=owned.Add(card);if(style!="standard")variants.Add(key);
    int refund=duplicate?pack.duplicateRefund:0;result.refund=checked(result.refund+refund);result.rewards.Add(new BoosterReward{cardId=card,styleId=style,duplicate=duplicate,refund=refund});
    if(newCard&&!looks.Any(x=>x.cardId==card))looks.Add(new CardLook{cardId=card,styleId=style});
   }
   int balance=checked(Data.coins-pack.price+result.refund);
   // Commit after all validation/random choices succeed. Persistence is handled atomically by the UI/store boundary.
   Data.coins=balance;foreach(string id in owned)if(!Owns(id))Data.owned.Add(id);Data.variants=variants.OrderBy(x=>x,StringComparer.Ordinal).ToList();Data.appearances=looks;Data.lastBooster=result;return result;
  }
 }
}
