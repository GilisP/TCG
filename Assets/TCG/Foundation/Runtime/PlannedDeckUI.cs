using System;
using System.IO;
using System.Linq;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
    public static class PlannedDeckLoader
    {
        static ContentCatalog cachedCatalog;static PlannedDeckLibrary cached;
        public static PlannedDeckLibrary Load(ContentCatalog catalog){if(cachedCatalog!=catalog){cached=new PlannedDeckLibrary(catalog,JsonUtility.FromJson<PlannedDeckFile>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"Decks","planned-decks.json"))));cachedCatalog=catalog;}return cached;}
    }
    public sealed partial class TableView
    {
        PlannedDeckLibrary plannedDecks;bool plannedDecksOpen;Vector2 plannedScroll;
        void DrawPlannedDecks()
        {
            if(plannedDecks==null)plannedDecks=PlannedDeckLoader.Load(catalog);
            Fill(new Rect(20,82,1560,838),Panel);Text(50,100,1490,45,"DECKS PLANEJADOS · 100 + 50 + COMANDANTE",heading,Gold);
            Text(50,156,1490,75,"Importar adiciona gratuitamente as cartas de teste necessárias. Seus decks existentes, moedas e cosméticos são preservados. As listas são protótipos para avaliar o equilíbrio.",body,Muted);
            plannedScroll=GUI.BeginScrollView(new Rect(40,248,1520,574),plannedScroll,new Rect(0,0,1485,plannedDecks.Decks.Count*112));
            for(int i=0;i<plannedDecks.Decks.Count;i++){
                var d=plannedDecks.Decks[i];float y=i*112;Text(12,y,1030,33,d.name+" · "+catalog.Get(d.commander).Name,cardName,Ink);Text(12,y+39,1030,65,d.description,small,Muted);
                bool exists=library.Data.decks.Any(x=>x.id=="planned-"+d.id);
                if(Button(1070,y+14,385,55,exists?"Abrir meu deck":"Importar deck + cartas de teste",collectionStore.CanWrite)){
                    try{draft=library.ImportPlannedDeck(d);PersistLibrary();plannedDecksOpen=false;SelectDeckSection(false);}catch(Exception e){libraryMessage=e.Message;}
                }
            }
            GUI.EndScrollView();
            if(Button(50,845,470,52,"Voltar à coleção"))plannedDecksOpen=false;
            if(Button(540,845,1000,52,"Importar os 29 decks e suas cartas de teste",collectionStore.CanWrite)){
                try{foreach(var d in plannedDecks.Decks)library.ImportPlannedDeck(d);PersistLibrary();plannedDecksOpen=false;}catch(Exception e){library=collectionStore.Load(catalog);libraryMessage=e.Message;}
            }
        }
    }
}
