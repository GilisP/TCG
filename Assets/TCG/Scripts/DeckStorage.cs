using System;
using System.IO;
using UnityEngine;

namespace TCG
{
    public static class DeckStorage
    {
        public static string PathFor(int player) => Path.Combine(Application.persistentDataPath,"decks","player-"+(player+1)+".json");
        public static DeckList Load(int player)
        {
            string path = PathFor(player);
            if (!File.Exists(path)) return Catalog.DefaultDeck(player);
            var deck = JsonUtility.FromJson<DeckList>(File.ReadAllText(path));
            var errors = Catalog.Validate(deck);
            if (errors.Count > 0) throw new InvalidDataException(string.Join(" ",errors));
            return deck;
        }
        public static void Save(int player,DeckList deck)
        {
            var errors = Catalog.Validate(deck);
            if (errors.Count > 0) throw new InvalidDataException(string.Join(" ",errors));
            string path = PathFor(player); Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temp = path+".tmp";
            File.WriteAllText(temp,JsonUtility.ToJson(deck,true));
            if (File.Exists(path)) File.Replace(temp,path,null); else File.Move(temp,path);
        }
    }
}
