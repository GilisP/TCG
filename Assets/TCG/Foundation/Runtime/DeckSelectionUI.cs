using System;
using System.Collections.Generic;
using System.Linq;
using TCG.Foundation;
using UnityEngine;

namespace TCG.Table
{
 public sealed partial class TableView
 {
  sealed class DeckSelectionEntry {public DeckData Deck;public int NetworkIndex;public bool Preset;}
  bool deckSelectionOpen,deckSelectionOnline;int deckSelectionSeat;string deckSelectionSearch="";Vector2 deckSelectionScroll;
  readonly List<DeckSelectionEntry> deckSelectionEntries=new List<DeckSelectionEntry>();
  void OpenDeckSelection(int seat,bool online=false)
  {
   deckSelectionEntries.Clear();var choices=TableDeckChoices();
   deckSelectionStory=false;
   if(online)deckSelectionEntries.Add(new DeckSelectionEntry{Deck=NetworkPresetDeck(),NetworkIndex=0,Preset=true});
   for(int n=0;n<choices.Count;n++)if(ValidateTableDeck(choices[n]).Count==0)deckSelectionEntries.Add(new DeckSelectionEntry{Deck=choices[n],NetworkIndex=n+1});
   deckSelectionSeat=seat;deckSelectionOnline=online;deckSelectionSearch="";deckSelectionScroll=Vector2.zero;deckSelectionOpen=true;
  }
  bool ChooseDeckEntry(DeckSelectionEntry entry)
  {
   if(deckSelectionStory)return ChooseStoryPrecon(entry.Deck.id);
   // Recheck the live list: an open picker must not select a removed/invalid deck.
   var choices=TableDeckChoices();var deck=entry.Preset?NetworkPresetDeck():choices.FirstOrDefault(d=>d.id==entry.Deck.id);
   if(deck==null||!entry.Preset&&ValidateTableDeck(deck).Count!=0)return false;
   if(deckSelectionOnline)
   {
    if(network==null||!network.Connected||network.Busy||network.State==null||network.State.started)return false;
    int index=entry.Preset?0:choices.FindIndex(d=>d.id==deck.id)+1;
    if(networkDeck!=index){networkDeck=index;networkDeckConfirmed=false;if(network.State.members.Any(m=>m.seat==network.State.seat&&m.ready))network.Send(new RoomRequest{type="ready",ready=false});}
   }
   else {if(deckSelectionSeat<0||deckSelectionSeat>=players)return false;seatDecks[deckSelectionSeat]=deck.id;}
   deckSelectionOpen=false;return true;
  }
  void DrawDeckSelection()
  {
   if(deckSelectionOnline&&network?.State?.started==true){deckSelectionOpen=false;return;}
   if(Event.current.type==EventType.KeyDown&&Event.current.keyCode==KeyCode.Escape){deckSelectionOpen=false;Event.current.Use();return;}
   GUI.enabled=true;Fill(new Rect(0,0,1600,1000),Dark);Fill(new Rect(120,90,1360,830),Panel);
   Text(155,120,1200,55,deckSelectionStory?"ESCOLHA SEU PRECON":"ESCOLHA SEU DECK",menuTitle,Gold);
   Text(155,185,1230,40,deckSelectionStory?"Este precon será seu exército na História":deckSelectionOnline?"Seu deck para esta sala":"Deck do jogador "+(deckSelectionSeat+1),body,Ink);
   Solid(new Rect(155,224,1280,1),new Color(Gold.r,Gold.g,Gold.b,.25f));Text(155,245,160,35,"Buscar",body,Gold);deckSelectionSearch=GUI.TextField(new Rect(310,240,1125,42),deckSelectionSearch,100);
   var entries=deckSelectionEntries.Where(e=>string.IsNullOrWhiteSpace(deckSelectionSearch)||(e.Deck.name+" "+catalog.Get(e.Deck.commander).Name+" "+TableDeckLabel(e.Deck)).IndexOf(deckSelectionSearch.Trim(),StringComparison.OrdinalIgnoreCase)>=0).ToArray();
   deckSelectionScroll=GUI.BeginScrollView(new Rect(150,305,1290,490),deckSelectionScroll,new Rect(0,0,1250,Math.Max(470,entries.Length*132)));
   bool enabled=!deckSelectionOnline||network!=null&&network.Connected&&!network.Busy&&network.State!=null&&!network.State.started;
   int first=Math.Max(0,(int)(deckSelectionScroll.y/132)),last=Math.Min(entries.Length,first+5);
   for(int n=first;n<last;n++)
   {
    var entry=entries[n];var d=entry.Deck;var commander=catalog.Get(d.commander);float y=n*132;
    bool selected=deckSelectionStory?d.id=="precon-"+storyHeroId:deckSelectionOnline?networkDeck==entry.NetworkIndex:seatDecks[deckSelectionSeat]==d.id;
    RenderCardFace(new Rect(10,y+7,72,108),commander,library.SelectedStyle(commander.Id,d));
    bool choose=Button(100,y+5,1130,116,"",enabled,selected);
    Solid(new Rect(112,y+20,3,86),selected?Gold:new Color(Gold.r,Gold.g,Gold.b,.22f));
    CardText(new Rect(132,y+23,890,34),entry.Preset?"Oficial · "+d.name:TableDeckLabel(d),23,selected?Gold:Ink,true);
    CardText(new Rect(132,y+65,890,26),commander.Name,17,Ink);
    CardText(new Rect(132,y+94,890,18),d.main.Count+" cartas  ·  "+d.terrains.Count+" terrenos",12,Muted);
    CardText(new Rect(1025,y+46,184,32),selected?"SELECIONADO":"ESCOLHER",13,selected?Gold:Muted,true,TextAnchor.MiddleCenter);
    if(choose)ChooseDeckEntry(entry);
   }
   if(entries.Length==0)Text(20,20,1200,100,deckSelectionEntries.Count==0?"Nenhum deck válido disponível. Monte um deck com comandante na coleção.":"Nenhum deck corresponde à busca.",body,Muted);
   GUI.EndScrollView();Text(155,810,1230,35,entries.Length+(deckSelectionStory?" precons oficiais disponíveis":" decks disponíveis · salvos e precons oficiais"),small,Muted);
   if(Button(155,860,1280,45,"Cancelar / voltar"))deckSelectionOpen=false;
  }
 }
}
