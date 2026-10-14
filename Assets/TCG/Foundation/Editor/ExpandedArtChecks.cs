using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TCG.Table;

namespace TCG.Foundation.Editor
{
    public static class ExpandedArtChecks
    {
        [Serializable]class Manifest { public Entry[] entries; }
        [Serializable]class Entry { public string cardId,styleId,resourcePath; }
        public static void Run()
        {
            int checks=0;void Check(bool ok,string label){checks++;if(!ok)throw new Exception("D03 ART: "+label);}
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"CardArt/manifest.json")));
            var catalog=ContentLoader.Load(new EffectRegistry());
            var labeled=new Definition(new CardData{id="keyword-label-fixture",name="Keyword label fixture",kind="Creature",text="Texto de teste",keywords=new[]{"magic-resistance","anchored","armor:2","tenacity:1","barrier:3"}},"isolated-presentation");
            foreach(string label in new[]{"Resistência à magia","Ancorado","Armadura 2","Tenacidade 1","Barreira 3"})
                Check(KeywordPresentation.CardRules(labeled).Contains(label),"Portuguese face label "+label);
            foreach(string id in new[]{"PD26-T-010","PD26-T-011","PD26-T-012"})
                Check(catalog.Get(id).Keywords.Contains(KeywordRules.Anchored),"existing rooted escort names its approved keyword");
            var paintings=manifest.entries.Where(e=>e.resourcePath.StartsWith("CardArt/PreconD03/")).ToArray();
            Check(paintings.Length==10,"ten exclusive bindings");
            foreach(var entry in paintings)
            {
                Check(catalog.Cards.Any(c=>c.Id==entry.cardId),"active card "+entry.cardId);
                Check(manifest.entries.Count(e=>e.cardId==entry.cardId&&e.styleId==entry.styleId)==1,"unique identity/style");
                var texture=Resources.Load<Texture2D>(entry.resourcePath);Check(texture!=null,"resource import");
                var path="Assets/TCG/Resources/"+entry.resourcePath+".png";
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                Check(importer!=null&&!importer.mipmapEnabled&&importer.maxTextureSize==1024,"shared painted importer");
                Check(texture.width>=512&&texture.height>=512,"illustration resolution");
                Check(File.Exists(path+".meta"),"preserved Unity identity");
            }
            Debug.Log("EXPANDED D03 ART PASSED: "+checks+" checks");
        }
    }
}
