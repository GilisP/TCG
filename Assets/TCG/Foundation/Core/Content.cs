using System;
using System.Collections.Generic;
using System.Linq;

namespace TCG.Foundation
{
    // Serializable transport documents are copied into immutable definitions at the catalog boundary.
    [Serializable] public sealed class EffectData { public string operation; public int amount; }
    [Serializable] public sealed class CardData
    {
        public bool foil; public string id, name, kind, text, rarity, art; public string printedStats="";
        public string unavailableReason = "";
        public string rule = ""; public string[] traits = Array.Empty<string>(); public string[] keywords=Array.Empty<string>(); public int vehicleSeats,vehicleCrew;
        public string[] subtypes=Array.Empty<string>(); public bool commander; public int[] identityColors=Array.Empty<int>(); public int maxCopies=1;
        public int actions=1, range=1, equipCost=1;
        public int color = 6, cost, attack, defense = 1, movement = 2;
        public int[] coloredCost = new int[7];
        public EffectData[] effects = Array.Empty<EffectData>();
    }
    [Serializable] public sealed class PrintingData { public string id, cardId, art; }
    [Serializable] public sealed class ExpansionData
    {
        public int schemaVersion = 1;
        public string id, title, version = "0.1.0", status = "prototype";
        public CardData[] cards = Array.Empty<CardData>();
        public PrintingData[] printings = Array.Empty<PrintingData>();
    }
    public enum CardType { Terrain, Creature, Spell, Instant, Construction, Equipment, Artifact, Enchantment }
    public sealed class Definition
    {
        public bool Foil {get;} public string Id { get; } public string Name { get; } public string Text { get; } public string PrintedStats {get;}
        public string Expansion { get; } public string Art { get; } public string Rarity { get; }
        public CardType Kind { get; } public int Color { get; } public int Cost { get; }
        public int Attack { get; } public int Defense { get; } public int Movement { get; }
        public IReadOnlyList<string> Subtypes {get;} public bool IsCommander {get;} public IReadOnlyList<int> IdentityColors {get;} public int MaxCopies {get;}
        public string UnavailableReason { get; } public bool Playable=>string.IsNullOrEmpty(UnavailableReason);
        public string Rule { get; } public IReadOnlyList<string> Traits { get; } public IReadOnlyList<string> Keywords {get;} public int VehicleSeats {get;} public int VehicleCrew {get;} public bool IsVehicle=>VehicleSeats>0; public bool IsMeka=>IsVehicle&&Subtypes.Contains("Meka");
        public int Actions { get; } public int Range { get; } public int EquipCost { get; }
        public bool Permanent => Kind==CardType.Creature||Kind==CardType.Construction||Kind==CardType.Equipment||Kind==CardType.Artifact||Kind==CardType.Enchantment;
        public int TotalCost => Cost+ColoredCost.Sum();
        public IReadOnlyList<int> ColoredCost { get; }
        public IReadOnlyList<EffectSpec> Effects { get; }
        internal Definition(CardData d, string expansion)
        {
            Foil=d.foil; Id=d.id; Name=d.name; Text=d.text ?? ""; Expansion=expansion; Art=d.art ?? ""; Rarity=d.rarity ?? "common";
            Kind=(CardType)Enum.Parse(typeof(CardType),d.kind,true); Color=d.color; Cost=d.cost;
            Attack=d.attack; Defense=d.defense; Movement=d.movement; PrintedStats=string.IsNullOrWhiteSpace(d.printedStats)?d.attack+"/"+d.defense:d.printedStats;
            Subtypes=Array.AsReadOnly((d.subtypes??Array.Empty<string>()).ToArray());IsCommander=d.commander;IdentityColors=Array.AsReadOnly((d.identityColors??Array.Empty<int>()).ToArray());MaxCopies=d.maxCopies;
            Keywords=Array.AsReadOnly((d.keywords??Array.Empty<string>()).Distinct().ToArray());VehicleSeats=d.vehicleSeats;VehicleCrew=d.vehicleCrew;
            UnavailableReason=d.unavailableReason??""; Rule=d.rule??""; Traits=Array.AsReadOnly((string[])(d.traits??Array.Empty<string>()).Clone()); Actions=d.actions; Range=d.range; EquipCost=d.equipCost;
            ColoredCost=Array.AsReadOnly((int[])d.coloredCost.Clone());
            Effects=Array.AsReadOnly(d.effects.Select(e=>new EffectSpec(e.operation,e.amount)).ToArray());
        }
    }
    public sealed class EffectSpec
    {
        public string Operation { get; } public int Amount { get; }
        public EffectSpec(string operation,int amount) { Operation=operation; Amount=amount; }
    }
    public sealed class ContentCatalog
    {
        readonly Dictionary<string,Definition> cards = new Dictionary<string,Definition>(StringComparer.Ordinal);
        readonly Dictionary<string,string> printings = new Dictionary<string,string>(StringComparer.Ordinal);
        readonly Dictionary<string,string> expansions = new Dictionary<string,string>(StringComparer.Ordinal);
        public IReadOnlyCollection<Definition> Cards => cards.Values;
        public IReadOnlyDictionary<string,string> Expansions => expansions;
        public Definition Get(string id) => cards.TryGetValue(id ?? "",out var card) ? card : throw new InvalidOperationException("Carta desconhecida: "+id);
        public string Identity(string id) => printings.TryGetValue(id ?? "",out var card) ? card : Get(id).Id;
        public void AddAll(IEnumerable<ExpansionData> sources,IEnumerable<string> supportedEffects)
        {
            var data=sources?.ToArray() ?? throw new ArgumentNullException(nameof(sources));
            var supported=supportedEffects.ToArray(); var staged=new ContentCatalog();
            foreach(var entry in cards) staged.cards.Add(entry.Key,entry.Value);
            foreach(var entry in printings) staged.printings.Add(entry.Key,entry.Value);
            foreach(var entry in expansions) staged.expansions.Add(entry.Key,entry.Value);
            // Resolve definitions before printings across the entire import. Filenames and
            // mutual reprint references cannot determine the meaning of a catalog.
            foreach(var pack in data)
            {
                if(pack==null||pack.printings==null) throw new InvalidOperationException("Expansão ou lista de reimpressões ausente.");
                staged.Add(new ExpansionData{schemaVersion=pack.schemaVersion,id=pack.id,title=pack.title,version=pack.version,status=pack.status,cards=pack.cards},supported);
            }
            foreach(var pack in data) foreach(var p in pack.printings)
            {
                if(p==null||!ValidId(p.id)||staged.cards.ContainsKey(p.id)||staged.printings.ContainsKey(p.id)||!staged.cards.ContainsKey(p.cardId??""))
                    throw new InvalidOperationException("Reimpressão inválida na expansão "+pack.id);
                staged.printings.Add(p.id,p.cardId);
            }
            cards.Clear(); foreach(var entry in staged.cards) cards.Add(entry.Key,entry.Value);
            printings.Clear(); foreach(var entry in staged.printings) printings.Add(entry.Key,entry.Value);
            expansions.Clear(); foreach(var entry in staged.expansions) expansions.Add(entry.Key,entry.Value);
        }
        public void Add(ExpansionData data, IEnumerable<string> supportedEffects)
        {
            var supported=new HashSet<string>(supportedEffects,StringComparer.Ordinal);
            var errors=new List<string>();
            if(data==null || data.schemaVersion!=1) throw new InvalidOperationException("Formato de expansão não suportado.");
            if(!ValidId(data.id) || expansions.ContainsKey(data.id)) errors.Add("ID de expansão inválido ou repetido.");
            if(string.IsNullOrWhiteSpace(data.title)) errors.Add("Título da expansão ausente.");
            if(data.cards==null || data.printings==null) errors.Add("Listas de cartas/edições ausentes.");
            var ids=new HashSet<string>(cards.Keys,StringComparer.Ordinal);
            var names=new HashSet<string>(cards.Values.Select(c=>c.Name),StringComparer.OrdinalIgnoreCase);
            foreach(var c in data.cards ?? Array.Empty<CardData>())
            {
                if(c==null) { errors.Add("Carta ausente."); continue; }
                if(!ValidId(c.id) || !ids.Add(c.id) || printings.ContainsKey(c.id)) errors.Add("ID inválido/repetido: "+c.id);
                if(string.IsNullOrWhiteSpace(c.name) || !names.Add(c.name.Trim())) errors.Add("Nome repetido: use uma reimpressão. "+c.name);
                if(!Enum.TryParse<CardType>(c.kind,true,out var kind) || !Enum.IsDefined(typeof(CardType),kind)) errors.Add("Tipo não implementado: "+c.kind);
                if(c.color<0 || c.color>6 || c.cost<0 || c.cost>100 || c.attack<0 || c.attack>1000 || c.defense<1 || c.defense>1000 || c.movement<0 || c.movement>20) errors.Add("Atributos inválidos: "+c.id);
                if(c.coloredCost==null || c.coloredCost.Length!=7 || c.coloredCost.Any(v=>v<0 || v>100)) errors.Add("Custo colorido inválido: "+c.id);
                if(c.effects==null || c.effects.Any(e=>e==null || !supported.Contains(e.operation) || e.amount<1 || e.amount>100)) errors.Add("Efeito não suportado ou inválido: "+c.id);
                if(kind==CardType.Terrain && (c.cost!=0 || (c.coloredCost?.Sum() ?? 0)!=0 || (c.effects?.Length ?? 0)!=0)) errors.Add("Terreno deve ser gratuito e sem efeitos nesta versão: "+c.id);
                if(c.maxCopies<1||c.maxCopies>100||c.identityColors==null||c.identityColors.Any(x=>x<0||x>5)||c.commander&&kind!=CardType.Creature)errors.Add("Metadados de deck inválidos: "+c.id);
                if(c.vehicleSeats<0||c.vehicleSeats>20||c.vehicleCrew<0||c.vehicleCrew>c.vehicleSeats||c.vehicleSeats>0&&(kind!=CardType.Artifact||c.vehicleCrew<1)||!MedievalRules.Valid("",c.keywords))errors.Add("Veículo/palavras-chave inválidos: "+c.id);
                if(c.actions<0||c.actions>10||c.range<1||c.range>11||c.equipCost<0||c.equipCost>100||!MedievalRules.Valid(c.rule,c.traits)) errors.Add("Regra/atributo medieval inválido: "+c.id);
                if(kind==CardType.Creature && (c.effects?.Length ?? 0)!=0) errors.Add("Gatilhos de criatura ainda não suportados: "+c.id);
                if((kind==CardType.Spell||kind==CardType.Instant)&&(c.effects?.Length??0)==0&&string.IsNullOrEmpty(c.rule)) errors.Add("Feitiço sem efeitos implementados: "+c.id);
            }
            var printingIds=new HashSet<string>(printings.Keys,StringComparer.Ordinal);
            foreach(var p in data.printings ?? Array.Empty<PrintingData>())
                if(p==null || !ValidId(p.id) || !printingIds.Add(p.id) || ids.Contains(p.id) || !ids.Contains(p.cardId ?? "")) errors.Add("Reimpressão inválida.");
            if(errors.Count>0) throw new InvalidOperationException(string.Join("\n",errors));
            // Commit only after validating the entire pack: a malformed file cannot half-register cards.
            foreach(var c in data.cards) cards.Add(c.id,new Definition(c,data.id));
            foreach(var p in data.printings) printings.Add(p.id,p.cardId);
            expansions.Add(data.id,data.title);
        }
        public List<string> ValidateDeck(IEnumerable<string> main,IEnumerable<string> terrains,bool standard)
        {
            var errors=new List<string>(); var m=main?.ToArray() ?? Array.Empty<string>(); var t=terrains?.ToArray() ?? Array.Empty<string>();
            if(standard && (m.Length!=100 || t.Length!=50)) errors.Add("Deck padrão: 100 cartas principais e 50 terrenos.");
            if(m.Length<5 || t.Length<12) errors.Add("A mesa de teste precisa de pelo menos 5 cartas e 12 terrenos.");
            var seen=new Dictionary<string,int>();
            foreach(var id in m.Concat(t))
            {
                try { var identity=Identity(id); if(!seen.ContainsKey(identity))seen[identity]=0;seen[identity]++;if(standard && seen[identity]>Get(identity).MaxCopies) errors.Add("Carta/reimpressão repetida: "+identity); }
                catch(InvalidOperationException e) { errors.Add(e.Message); }
            }
            foreach(var id in m) if(cards.ContainsKey(ResolveKnown(id)) && Get(ResolveKnown(id)).Kind==CardType.Terrain) errors.Add("Terreno no principal.");
            foreach(var id in t) if(cards.ContainsKey(ResolveKnown(id)) && Get(ResolveKnown(id)).Kind!=CardType.Terrain) errors.Add("Carta não terreno no deck de terrenos.");
            return errors;
        }
        string ResolveKnown(string id) => id!=null && printings.TryGetValue(id,out var identity) ? identity : id ?? "";
        static bool ValidId(string id) => !string.IsNullOrWhiteSpace(id) && id.Length<=100 && id.All(c=>char.IsLetterOrDigit(c)||c=='-'||c=='_');
    }
}

