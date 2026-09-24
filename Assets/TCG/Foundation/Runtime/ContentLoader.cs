using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TCG.Foundation;

namespace TCG.Table
{
    public static class ContentLoader
    {
        public static ContentCatalog Load(EffectRegistry effects)
        {
            var catalog=new ContentCatalog();
            var directory=Path.Combine(Application.streamingAssetsPath,"Expansions");
            if(!Directory.Exists(directory)) throw new InvalidOperationException("Pasta de expansões ausente: "+directory);
            var packs=new List<ExpansionData>();
            foreach(var path in Directory.GetFiles(directory,"*.json").OrderBy(p=>p,StringComparer.Ordinal))
            {
                try { packs.Add(JsonUtility.FromJson<ExpansionData>(File.ReadAllText(path))); }
                catch(Exception e) { throw new InvalidOperationException(Path.GetFileName(path)+": "+e.Message,e); }
            }
            catalog.AddAll(packs,effects.Keys);
            if(catalog.Cards.Count==0) throw new InvalidOperationException("Nenhuma carta disponível.");
            return catalog;
        }
        public static Match MedievalTable(ContentCatalog catalog,EffectRegistry effects,int players,bool teams,int seed,int firstColor=0)
        {
            var main=new List<string[]>();var lands=new List<string[]>();
            for(int p=0;p<players;p++) {
                int color=(firstColor+p)%6;
                var ids=catalog.Cards.Where(c=>c.Expansion=="EXP-001-base"&&c.Color==color&&c.Playable).OrderBy(c=>c.TotalCost).Select(c=>c.Id).ToArray();
                if(ids.Length==0)throw new InvalidOperationException("Cor medieval sem cartas jogáveis.");
                main.Add(Enumerable.Range(0,48).Select(i=>ids[i%ids.Length]).ToArray());
                lands.Add(Enumerable.Repeat("test-land-"+color,50).ToArray());
            }
            return new Match(catalog,effects,players,teams,seed,main,lands);
        }
        public static Match CommanderTable(ContentCatalog catalog,EffectRegistry effects,int players,bool teams,int seed,string[] commanders)
        {
            if(commanders==null||commanders.Length<players)throw new ArgumentException("Escolha um comandante para cada jogador.");
            var main=new List<string[]>();var terrains=new List<string[]>();
            for(int seat=0;seat<players;seat++)
            {
                var commander=catalog.Get(commanders[seat]);if(!commander.IsCommander||!commander.Playable)throw new InvalidOperationException("Comandante indisponível.");
                var colors=commander.IdentityColors.Count==0?new[]{6}:commander.IdentityColors.ToArray();
                var cards=catalog.Cards.Where(c=>c.Playable&&!c.IsCommander&&c.Kind!=CardType.Terrain&&CardFilter.CardColors(c).All(color=>color==6||colors.Contains(color))).OrderBy(c=>c.TotalCost).ThenBy(c=>c.Id).ToArray();
                if(cards.Length==0||colors.Length==0)throw new InvalidOperationException("Comandante sem base de cartas/terrenos para demonstração.");
                // Experimental repeated lists are session fixtures, never granted to the collection.
                main.Add(Enumerable.Range(0,48).Select(i=>cards[i%cards.Length].Id).ToArray());
                terrains.Add(Enumerable.Range(0,50).Select(i=>"test-land-"+colors[i%colors.Length]).ToArray());
            }
            return new Match(catalog,effects,players,teams,seed,main,terrains,commanders.Take(players).ToArray());
        }
        public static Match TestTable(ContentCatalog catalog,EffectRegistry effects,int players,bool teams,int seed)
        {
            var expansions=catalog.Expansions.Keys.Where(x=>x.StartsWith("demo-",StringComparison.Ordinal)).OrderBy(x=>x,StringComparer.Ordinal).ToArray();
            if(expansions.Length==0) throw new InvalidOperationException("Os pacotes demo- da mesa de teste estão ausentes.");
            var main=new List<string[]>(); var terrains=new List<string[]>();
            for(int p=0;p<players;p++)
            {
                string set=expansions[p%expansions.Length];
                var units=catalog.Cards.Where(c=>c.Expansion==set&&c.Kind!=CardType.Terrain).Select(c=>c.Id).ToArray();
                var lands=catalog.Cards.Where(c=>c.Expansion==set&&c.Kind==CardType.Terrain).Select(c=>c.Id).ToArray();
                if(units.Length==0||lands.Length==0) throw new InvalidOperationException("A expansão de teste precisa de criaturas/efeitos e terrenos: "+set);
                main.Add(Enumerable.Range(0,40).Select(i=>units[i%units.Length]).ToArray());
                terrains.Add(Enumerable.Range(0,50).Select(i=>lands[i%lands.Length]).ToArray());
            }
            return new Match(catalog,effects,players,teams,seed,main,terrains);
        }
    }
}

