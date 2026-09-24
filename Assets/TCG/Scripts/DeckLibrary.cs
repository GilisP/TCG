using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TCG
{
    [Serializable]
    public sealed class SavedDeck
    {
        public string id;
        public DeckList deck;
    }
    [Serializable]
    public sealed class DeckLibrary
    {
        public int version = 1;
        public List<SavedDeck> entries = new List<SavedDeck>();
        public DeckList solDeck, luaDeck;
        public string solId, luaId;
        public DeckLibrary Copy() => new DeckLibrary {
            version = version,solDeck = solDeck.Copy(),luaDeck = luaDeck.Copy(),solId = solId,luaId = luaId,
            entries = entries.Select(e => new SavedDeck { id = e.id,deck = e.deck.Copy() }).ToList() };
        public static DeckLibrary Create(DeckList sol = null,DeckList lua = null)
        {
            var library = new DeckLibrary { solDeck = (sol ?? Catalog.DefaultDeck(0)).Copy(),luaDeck = (lua ?? Catalog.DefaultDeck(1)).Copy() };
            library.solId = Guid.NewGuid().ToString("N"); library.luaId = Guid.NewGuid().ToString("N");
            library.entries.Add(new SavedDeck { id = library.solId,deck = library.solDeck.Copy() });
            library.entries.Add(new SavedDeck { id = library.luaId,deck = library.luaDeck.Copy() }); return library;
        }
        public void Upsert(string id,DeckList deck)
        {
            if (!Guid.TryParse(id,out _)) throw new InvalidDataException("Identificador de deck inválido.");
            var errors = Catalog.Validate(deck,false); if (errors.Count > 0) throw new InvalidDataException(string.Join(" ",errors));
            var entry = entries.FirstOrDefault(e => e.id == id);
            if (entry == null) entries.Add(new SavedDeck { id = id,deck = deck.Copy() }); else entry.deck = deck.Copy();
        }
        public void Equip(string id,int player)
        {
            var entry = entries.FirstOrDefault(e => e.id == id) ?? throw new InvalidDataException("Salve este deck antes de equipá-lo.");
            var errors = Catalog.Validate(entry.deck); if (errors.Count > 0) throw new InvalidDataException(string.Join(" ",errors));
            if (player == 0) { solId = id; solDeck = entry.deck.Copy(); }
            else if (player == 1) { luaId = id; luaDeck = entry.deck.Copy(); }
            else throw new InvalidDataException("Jogador inválido.");
        }
        public void Remove(string id)
        {
            entries.RemoveAll(e => e.id == id);
            // Equipped snapshots survive deleting or editing the saved list.
            if (solId == id) solId = null; if (luaId == id) luaId = null;
        }
    }
    public static class DeckLibraryStore
    {
        public static string Root => Path.Combine(Application.persistentDataPath,"decks");
        public static string FilePath(string root) => Path.Combine(root,"library-v1.json");
        public static DeckLibrary Load(string root,out string message)
        {
            message = ""; string path = FilePath(root);
            if (File.Exists(path))
            {
                var library = JsonUtility.FromJson<DeckLibrary>(File.ReadAllText(path)); ValidateLibrary(library); return library;
            }
            var decks = new[] { Catalog.DefaultDeck(0),Catalog.DefaultDeck(1) };
            for (int p = 0; p < 2; p++)
            {
                string legacy = Path.Combine(root,"player-"+(p+1)+".json");
                if (!File.Exists(legacy)) continue;
                try { var deck = Import(File.ReadAllText(legacy)); var errors = Catalog.Validate(deck); if (errors.Count > 0) throw new InvalidDataException(string.Join(" ",errors)); decks[p] = deck; }
                catch (Exception e) { message += "Deck antigo do jogador "+(p+1)+" não carregado: "+e.Message+" "; }
            }
            return DeckLibrary.Create(decks[0],decks[1]);
        }
        public static void Save(string root,DeckLibrary library)
        {
            ValidateLibrary(library); Directory.CreateDirectory(root);
            string path = FilePath(root), temp = path+".tmp";
            File.WriteAllText(temp,JsonUtility.ToJson(library,true));
            if (File.Exists(path)) File.Replace(temp,path,null); else File.Move(temp,path);
        }
        public static string Export(DeckList deck)
        { var errors = Catalog.Validate(deck,false); if (errors.Count > 0) throw new InvalidDataException(string.Join(" ",errors)); return JsonUtility.ToJson(deck,true); }
        public static DeckList Import(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || json.Length > 200000) throw new InvalidDataException("JSON vazio ou grande demais.");
            var deck = JsonUtility.FromJson<DeckList>(json);
            var errors = Catalog.Validate(deck,false);
            if (errors.Count > 0) throw new InvalidDataException(string.Join(" ",errors));
            if (deck.main.Count > 500 || deck.terrains.Count > 500) throw new InvalidDataException("Lista de cartas muito grande.");
            deck.name = string.IsNullOrWhiteSpace(deck.name) ? "Deck importado" : deck.name.Substring(0,Math.Min(64,deck.name.Length));
            return deck;
        }
        static void ValidateLibrary(DeckLibrary library)
        {
            if (library == null || library.version != 1 || library.entries == null) throw new InvalidDataException("Biblioteca de decks inválida.");
            if (library.entries.Any(e => e == null || !Guid.TryParse(e.id,out _)) || library.entries.Select(e => e.id).Distinct().Count() != library.entries.Count)
                throw new InvalidDataException("Biblioteca possui identificadores inválidos ou repetidos.");
            foreach (var entry in library.entries)
            { var errors = Catalog.Validate(entry.deck,false); if (errors.Count > 0) throw new InvalidDataException(string.Join(" ",errors)); }
            if (Catalog.Validate(library.solDeck).Count > 0 || Catalog.Validate(library.luaDeck).Count > 0) throw new InvalidDataException("Os decks equipados precisam estar completos e válidos.");
            if ((!string.IsNullOrEmpty(library.solId) && !library.entries.Any(e => e.id == library.solId)) || (!string.IsNullOrEmpty(library.luaId) && !library.entries.Any(e => e.id == library.luaId)))
                throw new InvalidDataException("Referência de deck equipado inválida.");
        }
    }
}
