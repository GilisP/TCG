using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TCG.Foundation;
namespace TCG.Table
{
    public sealed partial class TableView
    {
        readonly CardFilter libraryFilters=new CardFilter(),deckFilters=new CardFilter();
        bool filtersOpen,filteringDeck,editingTerrains,librarySectionOnly;
        string deckSearch="";
        Vector2 filtersScroll;
        GUIStyle checkStyle;
        Definition DraftCommander(){try{return string.IsNullOrEmpty(draft?.commander)?null:catalog.Get(catalog.Identity(draft.commander));}catch(InvalidOperationException){return null;}}
        Definition[] FilteredLibrary()=>catalog.Cards.Where(c=>libraryFilters.Matches(c,library.Owns(c.Id),librarySearch,DraftCommander())&&(!librarySectionOnly||draft==null||(editingTerrains?c.Kind==CardType.Terrain:c.Kind!=CardType.Terrain))).OrderBy(c=>c.Name).ToArray();
        IEnumerable<string> FilteredDeck()
        {
            if(draft==null)return Array.Empty<string>();
            return (editingTerrains?draft.terrains:draft.main).Distinct().Where(id=>{
                try{return deckFilters.Matches(catalog.Get(catalog.Identity(id)),library.Owns(id),deckSearch,DraftCommander());}
                catch(InvalidOperationException){return string.IsNullOrWhiteSpace(deckSearch)||CardName(id).IndexOf(deckSearch,StringComparison.OrdinalIgnoreCase)>=0;}
            });
        }
        void SelectDeckSection(bool terrains){editingTerrains=terrains;deckScroll=Vector2.zero;libraryPage=0;}
        void OpenFilters(bool forDeck){filteringDeck=forDeck;filtersOpen=true;filtersScroll=Vector2.zero;ResetDrag();}
        bool CheckBox(float x,float y,float w,string name,bool value)
        {
            if(checkStyle==null)checkStyle=new GUIStyle(GUI.skin.toggle){fontSize=16,wordWrap=true};
            return GUI.Toggle(new Rect(x,y,w,34),value,name,checkStyle);
        }
        void FilterCheck<T>(float x,float y,string label,HashSet<T> values,T key,float width=345)
        {
            bool before=values.Contains(key),after=CheckBox(x,y,width,label,before);
            if(before==after)return;if(after)values.Add(key);else values.Remove(key);libraryPage=0;deckScroll=Vector2.zero;
        }
        void DrawFilterDialog()
        {
            GUI.enabled=true;Fill(new Rect(0,0,1600,1000),new Color(0,0,0,.85f));Fill(new Rect(175,125,1250,785),Panel);
            var f=filteringDeck?deckFilters:libraryFilters;
            Text(210,148,1170,45,filteringDeck?"FILTRAR CARTAS DO DECK":"FILTRAR BIBLIOTECA E COLEÇÃO",heading,Gold);
            Text(210,202,1170,53,"Marque várias opções. Dentro do mesmo grupo vale qualquer uma; grupos diferentes se combinam. Nenhuma opção marcada inclui todas.",body,Muted);
            float contentHeight=Math.Max(665,200+catalog.Cards.Select(c=>c.Rarity).Distinct().Count()*38+catalog.Expansions.Count*55);
            filtersScroll=GUI.BeginScrollView(new Rect(202,276,1200,525),filtersScroll,new Rect(0,0,1165,contentHeight));
            Text(8,0,345,34,"Tipos",heading,Gold);
            var types=new[]{CardType.Creature,CardType.Terrain,CardType.Spell,CardType.Instant,CardType.Construction,CardType.Equipment,CardType.Artifact,CardType.Enchantment};
            var names=new[]{"Criaturas","Terrenos","Feitiços","Truques","Construções","Equipamentos","Artefatos","Encantamentos"};
            for(int i=0;i<types.Length;i++)FilterCheck(8,44+i*38,names[i],f.Types,types[i]);
            Text(8,353,345,25,"Disponibilidade",heading,Gold);
            bool owned=CheckBox(8,380,345,"Só cartas que possuo",f.OwnedOnly);
            bool playable=CheckBox(8,422,345,"Só cartas prontas para jogar",f.PlayableOnly);
            bool commanders=CheckBox(8,464,345,"Só comandantes",f.CommandersOnly);
            bool identity=CheckBox(8,506,350,"Cores do comandante do deck",f.CommanderIdentityOnly);
            if(owned!=f.OwnedOnly||playable!=f.PlayableOnly||commanders!=f.CommandersOnly||identity!=f.CommanderIdentityOnly){libraryPage=0;deckScroll=Vector2.zero;}
            f.OwnedOnly=owned;f.PlayableOnly=playable;f.CommandersOnly=commanders;f.CommanderIdentityOnly=identity;
            if(DraftCommander()==null)Text(8,550,340,82,"O filtro de identidade será aplicado quando um comandante for escolhido.",small,Muted);
            Text(390,0,345,34,"Cores",heading,Gold);
            var colors=new[]{"Sol","Lua","Água","Fogo","Ar","Terra","Incolor"};
            for(int i=0;i<7;i++)FilterCheck(390,44+i*38,colors[i],f.Colors,i);
            Text(390,333,345,35,"Custo total de mana",heading,Gold);
            for(int i=0;i<7;i++)FilterCheck(390,380+i*38,i==6?"6 ou mais":i.ToString(),f.Costs,i);
            Text(780,0,345,34,"Raridades",heading,Gold);float y=44;
            foreach(var rarity in catalog.Cards.Select(c=>c.Rarity).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x=>x)){FilterCheck(780,y,rarity=="AUTOR"?"Autoral · raridade a definir":rarity,f.Rarities,rarity);y+=38;}
            y+=23;Text(780,y,345,34,"Expansões",heading,Gold);y+=47;
            foreach(var expansion in catalog.Expansions){FilterCheck(780,y,expansion.Value,f.Expansions,expansion.Key,350);y+=55;}
            GUI.EndScrollView();
            if(Button(210,835,280,48,"Limpar filtros")){f.Clear();libraryPage=0;deckScroll=Vector2.zero;}
            Text(520,845,480,35,f.SelectedCount+" opções marcadas",body,Ink);
            if(Button(1040,835,345,48,"Ver resultados",true,true))filtersOpen=false;
        }
    }
}
