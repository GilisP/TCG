using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace TCG
{
    [Serializable] public sealed class DeckPackage
    { public string format; public int version; public DeckList deck; public List<CardDefinition> cards; }
    [Serializable] public sealed class InstalledCards
    { public int version=1; public List<CardDefinition> cards=new List<CardDefinition>(); }
    public static class DeckPackages
    {
        public static string Root=>Path.Combine(Application.persistentDataPath,"custom-cards");
        static string PathFor(string root)=>Path.Combine(root,"installed-v1.json");
        static bool Custom(string id)=>id!=null && id.StartsWith("custom-",StringComparison.Ordinal);
        static IEnumerable<string> References(CardDefinition c)=>c.Steps.Concat(c.AutomaticEffects ?? Array.Empty<EffectStep>()).Where(s=>s.Operation==Effect.SummonToken).Select(s=>s.TokenId);
        public static string Export(DeckList deck)
        {
            var errors=Catalog.Validate(deck,false);if(errors.Count>0)throw new InvalidDataException(string.Join(" ",errors));
            var included=new Dictionary<string,CardDefinition>();
            var pending=new Queue<string>(deck.main.Concat(deck.terrains).Concat(new[]{deck.commander}));
            while(pending.Count>0)
            {
                string id=pending.Dequeue();if(!Custom(id)||included.ContainsKey(id))continue;
                var card=Catalog.Get(id)??throw new InvalidDataException("Carta ausente: "+id);
                included[id]=CardAuthoring.Copy(card);foreach(var reference in References(card))pending.Enqueue(reference);
            }
            return JsonUtility.ToJson(new DeckPackage{format="tcg-deck-package",version=1,deck=deck.Copy(),cards=included.Values.ToList()},true);
        }
        public static string ExportFile(DeckList deck,string root=null)
        {
            string json=Export(deck),directory=root??Path.Combine(DeckLibraryStore.Root,"exports");Directory.CreateDirectory(directory);
            string path=Path.Combine(directory,"deck-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N").Substring(0,6)+".json");
            File.WriteAllText(path,json);return path;
        }
        static Dictionary<string,CardDefinition> ValidateCards(List<CardDefinition> cards,bool selfContained)
        {
            if(cards==null||cards.Count>600||cards.Any(c=>c==null||!Custom(c.Id))||cards.Select(c=>c.Id).Distinct().Count()!=cards.Count)throw new InvalidDataException("Lista de cartas personalizadas inválida.");
            var lookup=cards.ToDictionary(c=>c.Id);
            CardDefinition Resolve(string id)=>id!=null&&lookup.TryGetValue(id,out var card)?card:selfContained&&Custom(id)?null:Catalog.Get(id);
            foreach(var card in cards)CustomCardStore.Serialize(card,Resolve);
            return lookup;
        }
        public static DeckList Import(string json,string root=null)
        {
            if(string.IsNullOrWhiteSpace(json)||json.Length>2000000)throw new InvalidDataException("Pacote vazio ou maior que 2 MB.");
            var package=JsonUtility.FromJson<DeckPackage>(json);
            if(package==null||string.IsNullOrEmpty(package.format))return DeckLibraryStore.Import(json);
            if(package.format!="tcg-deck-package"||package.version!=1)throw new InvalidDataException("Formato ou versão de pacote não suportado.");
            var lookup=ValidateCards(package.cards,true);
            CardDefinition Resolve(string id)=>id!=null&&lookup.TryGetValue(id,out var c)?c:Custom(id)?null:Catalog.Get(id);
            var errors=Catalog.Validate(package.deck,false,Resolve);if(errors.Count>0)throw new InvalidDataException(string.Join(" ",errors));
            if(package.deck.main.Count>500||package.deck.terrains.Count>500)throw new InvalidDataException("Deck muito grande.");
            // Allocate new IDs for conflicting definitions, then rewrite every deck and token reference.
            // Reuse existing IDs only when the complete definition matches.
            var mapping=new Dictionary<string,string>();
            foreach(var c in package.cards)
            {
                var existing=Catalog.Get(c.Id);
                mapping[c.Id]=existing==null||JsonUtility.ToJson(CardAuthoring.Copy(existing))==JsonUtility.ToJson(CardAuthoring.Copy(c))?c.Id:"custom-"+Guid.NewGuid().ToString("N");
            }
            // A dependent card also needs a fresh ID if one of its token IDs has changed.
            foreach(var c in package.cards.Where(c=>References(c).Any(id=>mapping.TryGetValue(id,out var mapped)&&mapped!=id)))
                if(Catalog.Get(c.Id)!=null && mapping[c.Id]==c.Id)mapping[c.Id]="custom-"+Guid.NewGuid().ToString("N");
            string Map(string id)=>id!=null&&mapping.TryGetValue(id,out var mapped)?mapped:id;
            var incoming=package.cards.Select(CardAuthoring.Copy).ToList();
            foreach(var c in incoming){c.Id=Map(c.Id);foreach(var step in c.Steps.Concat(c.AutomaticEffects))if(step.Operation==Effect.SummonToken)step.TokenId=Map(step.TokenId);}
            var deck=package.deck.Copy();deck.main=deck.main.Select(Map).ToList();deck.terrains=deck.terrains.Select(Map).ToList();deck.commander=Map(deck.commander);
            deck.name=string.IsNullOrWhiteSpace(deck.name)?"Deck importado":deck.name.Substring(0,Math.Min(64,deck.name.Length));
            string directory=root??Root;
            var installed=ReadInstalled(directory);
            foreach(var c in incoming.Where(c=>Catalog.Get(c.Id)==null))
            {installed.cards.RemoveAll(old=>old.Id==c.Id);installed.cards.Add(c);}
            var combined=ValidateCards(installed.cards,false);
            CardDefinition FinalResolve(string id)=>id!=null&&combined.TryGetValue(id,out var c)?c:Catalog.Get(id);
            errors=Catalog.Validate(deck,false,FinalResolve);if(errors.Count>0)throw new InvalidDataException(string.Join(" ",errors));
            // Persist one atomic manifest only after the entire package validates. No partial imports.
            Directory.CreateDirectory(directory);string path=PathFor(directory),temp=path+".tmp";
            File.WriteAllText(temp,JsonUtility.ToJson(installed,true));
            if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);
            foreach(var c in incoming.OrderBy(c=>c.Kind==CardKind.Token?0:1))if(Catalog.Get(c.Id)==null)Catalog.RegisterCustom(c);
            return deck;
        }
        static InstalledCards ReadInstalled(string root)
        {
            string path=PathFor(root);if(!File.Exists(path))return new InstalledCards();
            var installed=JsonUtility.FromJson<InstalledCards>(File.ReadAllText(path));
            if(installed==null||installed.version!=1)throw new InvalidDataException("Biblioteca de cartas importadas inválida.");
            ValidateCards(installed.cards,false);return installed;
        }
        public static bool UpdateInstalled(CardDefinition card,string root=null)
        {
            string directory=root??Root;var installed=ReadInstalled(directory);
            int index=installed.cards.FindIndex(c=>c.Id==card.Id);if(index<0)return false;
            installed.cards[index]=CardAuthoring.Copy(card);ValidateCards(installed.cards,false);
            string path=PathFor(directory),temp=path+".tmp";
            File.WriteAllText(temp,JsonUtility.ToJson(installed,true));File.Replace(temp,path,null);return true;
        }
        public static void LoadInstalled(string root=null)
        {
            try
            {
                var installed=ReadInstalled(root??Root);
                foreach(var c in installed.cards.OrderBy(c=>c.Kind==CardKind.Token?0:1))Catalog.RegisterCustom(c);
            }
            catch(Exception e){Debug.LogError("Cartas importadas não carregadas; arquivo preservado: "+e.Message);}
        }
    }
}
