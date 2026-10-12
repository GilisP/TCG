using System;
using System.Linq;
using TCG.Foundation;
using UnityEngine;

namespace TCG.Table
{
 public sealed partial class TableView
 {
  Match storyMatch;StoryCampaign storyBattleCampaign;StoryEncounter storyBattleEncounter;
  string storyBattleHeroId;DeckData[] storyBattleDecks;ComputerOpponent[] storyBots;
  StoryReward storyReward;float nextStoryAction,nextStorySave;bool storyAiBlocked;
  bool StoryBattleActive=>storyMatch!=null&&ReferenceEquals(match,storyMatch);
  bool StartStoryEncounter(string id,int? seed=null)
  {
   if(network!=null&&network.Running){hubNotice="Saia da sala online para jogar História.";return false;}
   if(storyCampaign==null||!storyCampaign.CanEnter(library.Data.story,storyHeroId,id)){hubNotice="Vença o encontro anterior para avançar.";return false;}
   var encounter=storyCampaign.Encounters.Single(n=>n.id==id);
   var ids=encounter.format=="teams"?new[]{storyHeroId,encounter.opponents[0],encounter.allyPreconId,encounter.opponents[1]}:new[]{storyHeroId}.Concat(encounter.opponents).ToArray();
   var templates=ids.Select(StoryTemplate).ToArray();var precons=PreconstructedDeckLoader.Load(catalog);
   if(templates.Any(d=>d==null||!precons.Ready(d))){hubNotice="Este encontro precisa de um precon ainda indisponível.";return false;}
   SaveFinishedProgress();
   var decks=templates.Select(PreconstructedDeckLibrary.Create).ToArray();int gameSeed=seed??Guid.NewGuid().GetHashCode();
   var battle=new Match(catalog,effects,decks.Length,encounter.format=="teams",gameSeed,decks.Select(d=>d.main.ToArray()).ToArray(),decks.Select(d=>d.terrains.ToArray()).ToArray(),decks.Select(d=>d.commander).ToArray()){AutomaticResponses=true,AutoAdvanceAfterTerrain=true};
   storyBattleCampaign=storyCampaign;storyBattleEncounter=encounter;storyBattleHeroId=storyHeroId;storyBattleDecks=decks;
   match=storyMatch=battle;players=decks.Length;teams=encounter.format=="teams";storyReward=null;storyAiBlocked=false;nextStorySave=0;
   storyBots=Enumerable.Range(0,players).Select(i=>i==0?null:new ComputerOpponent(unchecked(gameSeed+i*7919))).ToArray();
   useSavedDecks=false;storyPage=menu=libraryOpen=handoff=networkPage=false;sessionAvailable=true;
   selectedCell=selectedUnit=-1;selectedCard=null;selectedCommander=false;blockers=null;lastDefense=null;inspected=null;collection=help=confirmQuit=exileOpen=false;deckSelectionOpen=false;
   ResetDrag();CloseReactionMenu();reactions=new ReactionChannel();world.Bind(match);
   for(int p=0;p<players;p++){matchBacks[p]=decks[p].cardBack;world.SetCardBack(p,matchBacks[p]);}
   match.ResolveInitialEffects();nextStoryAction=Time.unscaledTime+.65f;notice="História · você é Âmbar. Os demais assentos são controlados por IA.";return true;
  }
  void UpdateStoryOpponent()
  {
   if(!StoryBattleActive||match.Over||menu||storyPage||libraryOpen||deckSelectionOpen||help||collection||confirmQuit||inspected!=null||storyAiBlocked||match.Controller==0||Time.unscaledTime<nextStoryAction)return;
   nextStoryAction=Time.unscaledTime+.65f;int seat=match.Controller;
   try
   {
    var commands=match.DecisionCommandsFor(seat);var view=match.ViewFor(seat);var action=storyBots[seat].Choose(view,commands,catalog);
    string error="";if(action==null||!match.Try(action,out error)){storyAiBlocked=true;notice="IA aguardando: "+(action==null?"nenhuma ação disponível":error);Debug.LogError("Story opponent could not apply an official action: "+error);return;}
    handoff=false;selectedCard=null;selectedCommander=false;blockers=null;lastDefense=null;
    notice="IA · "+match.Seats[seat].Name+" "+StoryActionText(action.kind)+"."+(match.Controller==0?" Sua vez.":"");
    if(match.Over)SaveFinishedProgress();
   }
   catch(Exception e){storyAiBlocked=true;notice="IA interrompida: "+e.Message;Debug.LogException(e);}
  }
  static string StoryActionText(ActionKind kind)
  {
   switch(kind)
   {
    case ActionKind.DrawMain:case ActionKind.DrawTerrain:return "comprou cartas";
    case ActionKind.Place:return "colocou um terreno";
    case ActionKind.Play:case ActionKind.CastExiled:return "jogou uma carta";
    case ActionKind.SummonCommander:return "conjurou seu comandante";
    case ActionKind.Move:return "moveu uma tropa";
    case ActionKind.Attack:return "declarou um ataque";
    case ActionKind.Defend:return "escolheu a defesa";
    case ActionKind.Equip:return "equipou uma criatura";
    case ActionKind.Activate:case ActionKind.ActivateTerrain:return "ativou uma habilidade";
    case ActionKind.BoardVehicle:return "embarcou uma tropa";
    case ActionKind.DisembarkVehicle:return "desembarcou uma tropa";
    case ActionKind.Choose:return "concluiu uma escolha";
    default:return "passou a prioridade";
   }
  }
  void SaveStoryBattleResult()
  {
   if(!StoryBattleActive||!match.Over||library==null||!collectionStore.CanWrite||library.Data.story.recordedMatches.Contains(match.MatchId)||Time.unscaledTime<nextStorySave)return;
   nextStorySave=Time.unscaledTime+3;var numbers=match.NumbersFor(0);StoryReward reward=null;
   if(ProgressTransaction(()=>{reward=library.CompleteStoryEncounter(storyBattleCampaign,storyBattleHeroId,storyBattleEncounter.id,numbers,DateTime.Now.Date);library.RecordMatch(numbers);}))storyReward=reward;
  }
  string StorySeatLabel(int seat)=>StoryBattleActive?(seat==0?"Você":teams&&seat==2?"IA aliada":"IA inimiga")+" · "+match.Seats[seat].Name:match.Seats[seat].Name;
  void DrawStoryResult()
  {
   SaveStoryBattleResult();Veil();bool won=match.NumbersFor(0).won;
   Text(300,240,1000,75,won?"VITÓRIA NA HISTÓRIA":"ENCONTRO ENCERRADO",title,Gold);
   Text(300,335,1000,55,storyBattleEncounter.title+" · "+StoryFormat(storyBattleEncounter),heading,Ink);
   string reward=storyReward==null?"Salvando resultado… "+hubNotice:won?(storyReward.boosters>0?"+"+storyReward.boosters+" booster(s) para abrir na Loja.":"Você já recebeu as recompensas destes inimigos hoje."):"Sem recompensa nesta batalha. Você pode tentar novamente.";
   Text(300,405,1000,100,reward,body,Ink);
   if(Button(300,545,1000,60,"VOLTAR AO MAPA",true,true))OpenStoryMap();
   if(Button(300,630,1000,52,"Abrir boosters na Loja",storyReward!=null&&library.Data.progress.boosters>0)){storyPage=false;shopBoosters=true;NavigateHub(HubPage.Shop);}
  }
 }
}
