using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
 public static class PreconstructedDeckLoader
 {
  static ContentCatalog cachedCatalog;static PreconstructedDeckLibrary cached;
  public static PreconstructedDeckLibrary Load(ContentCatalog catalog){if(cachedCatalog!=catalog){cached=new PreconstructedDeckLibrary(catalog,JsonUtility.FromJson<PlannedDeckFile>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"Decks","official-precons.json"))));cachedCatalog=catalog;}return cached;}
 }
 public sealed partial class TableView
 {
  bool preconsOpen;Vector2 preconsScroll;int preconPreview=-1;Vector2 preconCardsScroll;
  List<DeckData> TableDeckChoices(){var precons=PreconstructedDeckLoader.Load(catalog);var blocked=new HashSet<string>(precons.Decks.Where(d=>!precons.Ready(d)).Select(d=>"precon-"+d.id));return library.Data.decks.Where(d=>!blocked.Contains(d.id)).Select(d=>d.Copy()).Concat(precons.Decks.Where(precons.Ready).Select(PreconstructedDeckLibrary.Create)).Concat(PlannedDeckLoader.Load(catalog).Decks.Select(d=>d.Create())).GroupBy(d=>d.id).Select(g=>g.First()).ToList();}
  bool TemporaryDeck(string id)=>!library.Data.decks.Any(d=>d.id==id)&&(id.StartsWith("precon-",StringComparison.Ordinal)||id.StartsWith("planned-",StringComparison.Ordinal));
  List<string> ValidateTableDeck(DeckData d)=>d!=null&&TemporaryDeck(d.id)?new CollectionLibrary(catalog,new CollectionData{owned=catalog.Cards.Select(c=>c.Id).ToList()}).Validate(d,true):library.Validate(d,true);
  string TableDeckLabel(DeckData d)=>d==null?"Escolher deck":(d.id.StartsWith("precon-",StringComparison.Ordinal)?"Precon · ":d.id.StartsWith("planned-",StringComparison.Ordinal)?"Planejado · ":"Salvo · ")+d.name;
  void DrawPreconstructedDecks()
  {
   var precons=PreconstructedDeckLoader.Load(catalog);Fill(new Rect(20,82,1560,838),Panel);Text(50,100,1490,45,"PRECONS · 10 · 100 + 50 + COMANDANTE",heading,Gold);
   Text(50,156,1490,70,"Importação gratuita explícita inclui as cartas necessárias. Reimportar preserva decks editados, moedas e cosméticos. Para experimentar na mesa, não é necessário importar.",body,Muted);
   if(preconPreview>=0){var d=precons.Decks[preconPreview];Text(50,236,1470,35,d.name,heading,Gold);var ids=new[]{d.commander}.Concat(d.main).Concat(d.terrains).ToArray();preconCardsScroll=GUI.BeginScrollView(new Rect(45,285,1500,530),preconCardsScroll,new Rect(0,0,1450,ids.Length*32));for(int i=0;i<ids.Length;i++){var c=catalog.Get(ids[i]);Text(10,i*32,1430,30,(i==0?"Comandante · ":i<=100?"Principal · ":"Terreno · ")+c.Name+" · "+c.Id,body,Ink);}GUI.EndScrollView();if(Button(50,845,1490,52,"Voltar aos precons"))preconPreview=-1;return;}
   preconsScroll=GUI.BeginScrollView(new Rect(40,248,1520,574),preconsScroll,new Rect(0,0,1485,precons.Decks.Count*112));
   for(int i=0;i<precons.Decks.Count;i++){var d=precons.Decks[i];float y=i*112;Text(12,y,840,33,d.name+" · "+catalog.Get(d.commander).Name,cardName,Ink);bool ready=precons.Ready(d);Text(12,y+39,840,65,ready?d.description:"Bloqueado · "+string.Join("; ",precons.Errors(d)),small,ready?Muted:Gold);if(Button(860,y+14,190,55,"Ver lista"))preconPreview=i;bool exists=library.Data.decks.Any(x=>x.id=="precon-"+d.id);if(Button(1070,y+14,385,55,!ready?"Aguardando decisão":exists?"Abrir meu precon":"Importar grátis + cartas",ready&&collectionStore.CanWrite)){try{draft=precons.Import(library,d);collectionStore.Save(library);libraryMessage="Precons salvos neste computador.";preconsOpen=false;SelectDeckSection(false);}catch(Exception e){library=collectionStore.Load(catalog);libraryMessage=e.Message;}}}
   GUI.EndScrollView();if(Button(50,845,470,52,"Voltar à coleção"))preconsOpen=false;
   if(Button(540,845,1000,52,"Importar grátis os "+precons.Decks.Count(precons.Ready)+" precons disponíveis",collectionStore.CanWrite&&precons.Decks.Any(precons.Ready))){try{foreach(var d in precons.Decks.Where(precons.Ready))precons.Import(library,d);collectionStore.Save(library);libraryMessage="Precons salvos neste computador.";preconsOpen=false;}catch(Exception e){library=collectionStore.Load(catalog);libraryMessage=e.Message;}}
  }
 }
}
