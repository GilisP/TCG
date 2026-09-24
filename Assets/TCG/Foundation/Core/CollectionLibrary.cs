using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
    [Serializable] public sealed class DeckData
    {
        public string id=Guid.NewGuid().ToString("N"),name="Novo deck",commander="";
        public bool experimental; public string cardBack="classic"; public List<CardLook> cardLooks=new List<CardLook>();
        public List<string> main=new List<string>(),terrains=new List<string>();
        public DeckData Copy()=>new DeckData{id=id,name=name,commander=commander,experimental=experimental,cardBack=cardBack,cardLooks=cardLooks.Select(x=>x.Copy()).ToList(),main=new List<string>(main),terrains=new List<string>(terrains)};
    }
    [Serializable] public sealed class CollectionData
    {
        public PlayerProgress progress=new PlayerProgress(); public int schemaVersion=3; public int coins=500; public List<string> cosmetics=new List<string>{"classic"};
        public List<string> variants=new List<string>(); public List<CardLook> appearances=new List<CardLook>(); public BoosterReceipt lastBooster;
        public List<string> owned=new List<string>();
        public List<DeckData> decks=new List<DeckData>();
    }
    public sealed partial class CollectionLibrary
    {
        public CollectionData Data {get;}
        readonly ContentCatalog catalog;
        public CollectionLibrary(ContentCatalog catalog,CollectionData data)
        {
            this.catalog=catalog??throw new ArgumentNullException(nameof(catalog));Data=data??new CollectionData();
            if(Data.schemaVersion==1){Data.schemaVersion=2;Data.coins=500;Data.cosmetics=new List<string>{"classic"};foreach(var deck in Data.decks??new List<DeckData>())if(deck!=null&&string.IsNullOrEmpty(deck.cardBack))deck.cardBack="classic";}
            if(Data.schemaVersion==2){Data.schemaVersion=3;Data.variants=Data.variants??new List<string>();Data.appearances=Data.appearances??new List<CardLook>();}
            if(Data.schemaVersion!=3||Data.owned==null||Data.decks==null||Data.cosmetics==null||Data.variants==null||Data.appearances==null||Data.coins<0)throw new InvalidOperationException("Formato da coleção não suportado ou incompleto.");
            if(Data.owned.Any(string.IsNullOrWhiteSpace)||Data.decks.Any(d=>d==null||string.IsNullOrWhiteSpace(d.name)||d.name.Length>80||string.IsNullOrWhiteSpace(d.id)||d.main==null||d.terrains==null||d.main.Any(string.IsNullOrWhiteSpace)||d.terrains.Any(string.IsNullOrWhiteSpace)))throw new InvalidOperationException("Coleção possui entradas inválidas.");
            if(Data.decks.Select(d=>d.id).Distinct().Count()!=Data.decks.Count)throw new InvalidOperationException("IDs de decks repetidos.");
            foreach(var deck in Data.decks)deck.cardLooks=deck.cardLooks??new List<CardLook>();
            if(Data.appearances.Any(x=>x==null||string.IsNullOrEmpty(x.cardId)||string.IsNullOrEmpty(x.styleId))||Data.decks.Any(d=>d.cardLooks.Any(x=>x==null||string.IsNullOrEmpty(x.cardId)||string.IsNullOrEmpty(x.styleId))))throw new InvalidOperationException("Aparências inválidas.");
            // Unity JsonUtility writes a null inline serializable object as an empty object.
            if(Data.lastBooster!=null&&string.IsNullOrEmpty(Data.lastBooster.id)&&string.IsNullOrEmpty(Data.lastBooster.boosterId)&&Data.lastBooster.price==0&&Data.lastBooster.refund==0&&Data.lastBooster.rewards!=null&&Data.lastBooster.rewards.Count==0)Data.lastBooster=null;
            if(Data.variants.Any(string.IsNullOrWhiteSpace)||Data.lastBooster!=null&&(Data.lastBooster.rewards==null||Data.lastBooster.rewards.Count<1||Data.lastBooster.rewards.Count>20||Data.lastBooster.rewards.Any(r=>r==null||string.IsNullOrWhiteSpace(r.cardId)||!CardStyles.Valid(r.styleId)||r.refund<0)))throw new InvalidOperationException("Histórico de boosters inválido.");
            Data.progress=Data.progress??new PlayerProgress(); Data.progress.history=Data.progress.history??new List<MatchNumbers>(); Data.progress.recorded=Data.progress.recorded??new List<string>(); Data.progress.claimed=Data.progress.claimed??new List<string>();
            // Preserve unknown identities to survive a temporarily missing expansion.
        }
        public bool OwnsCosmetic(string id)=>CosmeticCatalog.All.Any(c=>c.Id==id)&&Data.cosmetics.Contains(id);
        public const int TestCardPrice=50;
        public void BuyCard(string id)
        {
            var card=catalog.Get(catalog.Identity(id));
            if(!card.Playable)throw new InvalidOperationException("Esta carta ainda não está disponível para jogar.");
            if(Owns(id))throw new InvalidOperationException("Esta carta já está na coleção.");
            if(Data.coins<TestCardPrice)throw new InvalidOperationException("Moedas insuficientes.");
            Data.coins-=TestCardPrice;Collect(id);
        }
        public void BuyCosmetic(string id)
        {
            var cosmetic=CosmeticCatalog.All.FirstOrDefault(c=>c.Id==id);
            if(cosmetic==null)throw new InvalidOperationException("Cosmético não encontrado.");
            if(OwnsCosmetic(id))throw new InvalidOperationException("Este cosmético já está na coleção.");
            if(Data.coins<cosmetic.Price)throw new InvalidOperationException("Moedas insuficientes.");
            Data.coins-=cosmetic.Price;Data.cosmetics.Add(id);
        }
        public bool Owns(string id)=>Data.owned.Any(x=>IdentityOrOriginal(x)==IdentityOrOriginal(id));
        string IdentityOrOriginal(string id){try{return catalog.Identity(id);}catch(InvalidOperationException){return id;}}
        public bool Collect(string id)
        {
            string canonical=catalog.Identity(id);if(Owns(canonical))return false;Data.owned.Add(canonical);return true;
        }
        public void SaveDeck(DeckData deck)
        {
            if(deck==null||string.IsNullOrWhiteSpace(deck.id)||string.IsNullOrWhiteSpace(deck.name))throw new InvalidOperationException("Dê um nome ao deck.");
            if(deck.name.Length>80)throw new InvalidOperationException("Nome limitado a 80 caracteres.");
            var copy=deck.Copy();int at=Data.decks.FindIndex(d=>d.id==deck.id);if(at<0)Data.decks.Add(copy);else Data.decks[at]=copy;
        }
        public List<string> Validate(DeckData deck,bool forPlay)
        {
            var errors=new List<string>();if(deck==null){errors.Add("Escolha um deck.");return errors;}
            if(!OwnsCosmetic(string.IsNullOrEmpty(deck.cardBack)?"classic":deck.cardBack))errors.Add("Verso não disponível na coleção.");
            errors.AddRange(catalog.ValidateDeck(deck.main,deck.terrains,!deck.experimental));
            foreach(string id in deck.main.Concat(deck.terrains).Distinct())
            {
                try{var c=catalog.Get(catalog.Identity(id));if(!Owns(id))errors.Add("Falta na coleção: "+c.Name);if(c.IsCommander)errors.Add("Comandante deve ficar na zona separada: "+c.Name);if(!c.Playable)errors.Add(c.Name+": "+c.UnavailableReason);}
                catch(InvalidOperationException){/* Catalog validator reports missing IDs; keep the original draft intact. */}
            }
            if(!string.IsNullOrWhiteSpace(deck.commander))
            {
                try{var cmd=catalog.Get(catalog.Identity(deck.commander));if(!cmd.IsCommander)errors.Add("A carta escolhida não é comandante.");
                    foreach(var id in deck.main.Concat(deck.terrains).Distinct()){try{var c=catalog.Get(catalog.Identity(id));var colors=c.IdentityColors.Count>0?c.IdentityColors:c.ColoredCost.Select((v,n)=>v>0?n:-1).Where(n=>n>=0&&n<6).Concat(c.Color<6?new[]{c.Color}:Array.Empty<int>()).Distinct().ToArray();if(colors.Any(color=>!cmd.IdentityColors.Contains(color)))errors.Add("Cor fora da identidade: "+c.Name);}catch(InvalidOperationException){}}
                    if(!Owns(deck.commander))errors.Add("Comandante ausente da coleção.");if(!cmd.Playable)errors.Add("Comandante indisponível: "+cmd.UnavailableReason);}
                catch(InvalidOperationException){errors.Add("Comandante não encontrado no catálogo.");}

            }
            else if(!deck.experimental)errors.Add("Deck padrão exige um comandante separado.");

            foreach(var look in deck.cardLooks){try{if(!OwnsVariant(look.cardId,look.styleId))errors.Add("Arte não disponível: "+look.cardId);}catch(InvalidOperationException){errors.Add("Arte ausente: "+look.cardId);}}
            return errors.Distinct().ToList();
        }
    }
}

