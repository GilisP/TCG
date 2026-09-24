using System.Collections.Generic;
using UnityEngine;
namespace TCG
{
    public static class GeneratedArt
    {
        public static readonly string[] Keys={"realm-reference","guardian","moon-mage","dragon","arcane-clash","confluence"};
        static readonly Dictionary<string,Texture2D> cache=new Dictionary<string,Texture2D>();
        public static Texture2D Get(string key)
        {
            if(string.IsNullOrEmpty(key))return null;
            if(!cache.TryGetValue(key,out var texture)){texture=Resources.Load<Texture2D>("TCGArt/"+key);cache[key]=texture;}
            return texture;
        }
    }
}
