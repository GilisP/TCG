using System;
using System.Linq;
using System.Globalization;
using TCG.Foundation;
using UnityEngine;

namespace TCG.Table
{
 public sealed partial class TableView
 {
  bool storyPage,deckSelectionStory;string storyHeroId="",storySelectedNode="";StoryCampaign storyCampaign;Vector2 storyMapScroll;
  string storyMarkerHero="";int storyMarkerFrom,storyMarkerTo;float storyMarkerAt;
  void AdvanceStoryMarker()
  {
   int target=0;while(target<storyCampaign.Encounters.Count-1&&StoryCompleted(storyCampaign.Encounters[target]))target++;
   storyMarkerFrom=storyMarkerHero==storyHeroId?storyMarkerTo:target;storyMarkerTo=target;storyMarkerHero=storyHeroId;storyMarkerAt=Time.unscaledTime;
  }
  PlannedDeckData StoryTemplate(string id)=>PreconstructedDeckLoader.Load(catalog).Decks.FirstOrDefault(d=>d.id==id);
  void OpenStoryMap()
  {
   SaveFinishedProgress();if(network!=null&&network.Running){hubNotice="Saia da sala online para jogar História.";return;}
   storyPage=true;menu=true;libraryOpen=networkPage=false;handoff=false;ResetDrag();
   storyHeroId=library.Data.story.selectedHeroPreconId??"";
   if(StoryTemplate(storyHeroId)==null){storyCampaign=null;OpenStoryPreconSelection();return;}
   storyCampaign=StoryCampaign.BuildDefault(storyHeroId,PreconstructedDeckLoader.Load(catalog).Decks.Select(d=>d.id));
   storySelectedNode=storyCampaign.Encounters.FirstOrDefault(n=>storyCampaign.CanEnter(library.Data.story,storyHeroId,n.id)&&!StoryCompleted(n))?.id??storyCampaign.Encounters.Last().id;
   AdvanceStoryMarker();storyMapScroll=new Vector2(0,Mathf.Max(0,storyMarkerTo*100-200));
  }
  void OpenStoryPreconSelection()
  {
   deckSelectionEntries.Clear();var precons=PreconstructedDeckLoader.Load(catalog);
   foreach(var d in precons.Decks.Where(precons.Ready))deckSelectionEntries.Add(new DeckSelectionEntry{Deck=PreconstructedDeckLibrary.Create(d)});
   deckSelectionStory=true;deckSelectionOnline=false;deckSelectionSeat=0;deckSelectionSearch="";deckSelectionScroll=Vector2.zero;deckSelectionOpen=true;
  }
  bool ChooseStoryPrecon(string deckId)
  {
   if(network!=null&&network.Running)return false;
   var precons=PreconstructedDeckLoader.Load(catalog);var template=precons.Decks.FirstOrDefault(d=>"precon-"+d.id==deckId);
   if(template==null||!precons.Ready(template))return false;
   if(!ProgressTransaction(()=>library.Data.story.selectedHeroPreconId=template.id))return false;
   storyHeroId=template.id;storyCampaign=StoryCampaign.BuildDefault(storyHeroId,precons.Decks.Select(d=>d.id));storySelectedNode=storyCampaign.Encounters.FirstOrDefault(n=>storyCampaign.CanEnter(library.Data.story,storyHeroId,n.id)&&!StoryCompleted(n))?.id??storyCampaign.Encounters.Last().id;AdvanceStoryMarker();
   deckSelectionOpen=false;storyPage=true;return true;
  }
  bool StoryCompleted(StoryEncounter n)=>library.Data.story.heroes.FirstOrDefault(h=>h.heroPreconId==storyHeroId)?.completed.Contains(n.id)==true;
  static string StoryFormat(StoryEncounter n)=>n.format=="teams"?"2 × 2 · parceiro IA":n.format=="ffa3"?"3 · todos contra todos":n.format=="ffa4"?"4 · todos contra todos":"1 × 1";
  void DrawStoryStatus(){if(StoryBattleActive&&Button(1210,900,365,44,"História · voltar ao mapa"))OpenStoryMap();}
  void DrawStoryPath(Vector2 from,Vector2 to,Color color)
  {
   var matrix=GUI.matrix;GUIUtility.RotateAroundPivot(Mathf.Atan2(to.y-from.y,to.x-from.x)*Mathf.Rad2Deg,from);
   Solid(new Rect(from.x,from.y-3,Vector2.Distance(from,to),6),color);GUI.matrix=matrix;
  }
  void DrawStoryMap()
  {
   GUI.enabled=true;Fill(new Rect(0,0,1600,1000),Dark);Text(45,30,1120,70,"HISTÓRIA · CAMINHO DOS REINOS",menuTitle,Gold);
   if(Button(1320,35,230,50,"Voltar")){storyPage=false;NavigateHub(HubPage.Play);}
   Text(45,115,1500,65,"Escolha um precon, avance pelo mapa e vença outros exércitos. Duelo é a batalha principal; encontros especiais mudam a mesa.",body,Ink);
   var hero=StoryTemplate(storyHeroId);Text(45,200,950,50,hero==null?"Escolha seu exército":"Seu precon · "+hero.name,heading,Gold);
   if(Button(1130,196,420,52,"Escolher outro precon"))OpenStoryPreconSelection();
   if(storyCampaign==null){Text(45,340,1460,100,"Selecione um precon para começar sua jornada.",heading,Ink);return;}
   Fill(new Rect(40,280,1000,630),Panel);
   storyMapScroll=GUI.BeginScrollView(new Rect(55,295,965,595),storyMapScroll,new Rect(0,0,925,1480));
   PaperGrain(new Rect(0,0,925,1480),.12f);
   Vector2 Point(StoryEncounter n)=>new Vector2(n.x*925,70+n.y*1360);
   for(int i=1;i<storyCampaign.Encounters.Count;i++)DrawStoryPath(Point(storyCampaign.Encounters[i-1]),Point(storyCampaign.Encounters[i]),StoryCompleted(storyCampaign.Encounters[i-1])?Gold:TableWorld.Hex("60503C"));
   for(int i=0;i<storyCampaign.Encounters.Count;i++)
   {
    var node=storyCampaign.Encounters[i];var p=Point(node);bool enter=storyCampaign.CanEnter(library.Data.story,storyHeroId,node.id),done=StoryCompleted(node);
    Jewel(p,34,done?TableWorld.Seats[1]:enter?Gold:Muted*.35f);
    if(Button(p.x-110,p.y-26,220,56,(i+1)+" · "+StoryFormat(node)+(done?" ✓":enter?"":" · bloqueado"),true,storySelectedNode==node.id))storySelectedNode=node.id;
   }
   var marker=Vector2.Lerp(Point(storyCampaign.Encounters[storyMarkerFrom]),Point(storyCampaign.Encounters[storyMarkerTo]),Mathf.SmoothStep(0,1,Mathf.Clamp01((Time.unscaledTime-storyMarkerAt)/1.2f)));
   marker.y-=50;Jewel(marker,18,TableWorld.Seats[0]);Text(marker.x-65,marker.y-46,130,24,"SEU AVANÇO",label,Gold);
   GUI.EndScrollView();Fill(new Rect(1070,280,490,630),Panel);
   var selected=storyCampaign.Encounters.FirstOrDefault(n=>n.id==storySelectedNode)??storyCampaign.Encounters[0];
   Text(1100,305,430,40,selected.title,heading,Gold);Text(1100,365,430,50,StoryFormat(selected),body,Ink);
   int y=435;foreach(string enemy in selected.opponents){var d=StoryTemplate(enemy);Text(1100,y,430,60,"Inimigo · "+d.name,body,Ink);y+=65;}
   if(selected.format=="teams"){Text(1100,y,430,60,"Aliado IA · "+StoryTemplate(selected.allyPreconId).name,body,TableWorld.Seats[1]);y+=65;}
   string day=DateTime.Now.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture);int available=selected.opponents.Count(enemy=>!library.Data.story.dailyClaims.Any(c=>c.enemyPreconId==enemy&&c.day==day));
   Text(1100,710,430,65,"Recompensa hoje: "+available+" booster(s). Um por inimigo, uma vez por dia.",small,Gold);
   if(Button(1100,788,430,56,StoryCompleted(selected)?"JOGAR NOVAMENTE":"ENFRENTAR",storyCampaign.CanEnter(library.Data.story,storyHeroId,selected.id),true))StartStoryEncounter(selected.id);
   if(Button(1100,854,430,38,"Retomar batalha em andamento",StoryBattleActive&&!match.Over)){storyPage=menu=libraryOpen=handoff=false;}
   Text(45,930,1510,50,string.IsNullOrEmpty(hubNotice)?"Progresso salvo por precon. Recompensas do dia ficam disponíveis para abrir na Loja.":hubNotice,small,Muted);
  }
 }
}
