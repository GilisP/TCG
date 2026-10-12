using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TCG.Foundation;
using UnityEngine;

namespace TCG.Table
{
 public sealed partial class TableView
 {
  IEnumerator VerifyStory()
  {
   string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","StoryVerification"));Directory.CreateDirectory(dir);int checks=0;
   var errors=new List<string>();Application.LogCallback listen=(message,trace,kind)=>{if(kind==LogType.Error||kind==LogType.Exception||kind==LogType.Assert){errors.Add(message);File.WriteAllText(Path.Combine(dir,"result.txt"),"FAIL: "+message);Application.Quit(1);}};Application.logMessageReceived+=listen;
   collectionStore=new CollectionStore(Path.Combine(dir,"profile-"+Guid.NewGuid().ToString("N")));library=collectionStore.Load(catalog);int coins=library.Data.coins;
   OpenStoryMap();Check(deckSelectionStory&&deckSelectionEntries.Count==10,"official precon picker");Check(ChooseStoryPrecon("precon-COL001-D01"),"choose hero");
   Check(storyPage&&storyCampaign.Encounters.Count==12&&storyCampaign.Encounters.Count(n=>n.format=="duel")==9,"map and duel majority");yield return Capture("01-map.png");
   Check(!StartStoryEncounter(storyCampaign.Encounters[1].id),"locked node rejected");
   string first=storyCampaign.Encounters[0].id;Check(StartStoryEncounter(first,83),"start full duel");
   Check(players==2&&!teams&&Viewer==0&&match.Seats.All(s=>s.Commander!=null&&s.MainCount+s.HandCount+s.Graveyard.Count==100),"full main decks, fixed human view");
   var humanBot=new ComputerOpponent(91);int beforeRevision=match.Revision;
   for(int step=0;step<50&&!match.Over;step++)
   {
    int seat=match.Controller;var action=(seat==0?humanBot:storyBots[seat]).Choose(match.ViewFor(seat),match.DecisionCommandsFor(seat),catalog);
    Check(action!=null&&match.Try(action,out _),"legal story action "+step);Check(Viewer==0&&!handoff,"human camera/hand never handed to bot");yield return null;
   }
   Check(match.Revision>beforeRevision,"AI and human decision integration");yield return Capture("02-duel.png");
   // Controlled concession fixtures verify the result/reward boundary; bots never concede.
   if(!match.Over)Check(match.Try(new Command{kind=ActionKind.Concede,player=0,revision=match.Revision},out _),"defeat fixture");
   if(!match.NumbersFor(0).won){SaveStoryBattleResult();Check(library.Data.progress.boosters==0&&!storyCampaign.CanEnter(library.Data.story,storyHeroId,storyCampaign.Encounters[1].id),"loss grants nothing");}
   OpenStoryMap();Check(StartStoryEncounter(first,87),"replay duel");Check(match.Try(new Command{kind=ActionKind.Concede,player=1,revision=match.Revision},out _),"victory fixture");SaveStoryBattleResult();int rewards=library.Data.progress.boosters;
   Check(rewards==1&&storyReward.boosters==1,"first daily reward persisted");SaveStoryBattleResult();Check(library.Data.progress.boosters==rewards,"duplicate callback gives no reward");
   library=collectionStore.Load(catalog);Check(library.Data.progress.boosters==1&&library.Data.story.heroes[0].completed.Contains(first),"reload campaign and reward");
   OpenStoryMap();yield return Capture("03-progress.png");Check(StartStoryEncounter(first,89),"same day replay");Check(match.Try(new Command{kind=ActionKind.Concede,player=1,revision=match.Revision},out _),"repeat fixture");SaveStoryBattleResult();Check(storyReward.boosters==0&&library.Data.progress.boosters==1,"same enemy no second daily booster");
   // Unlock fixture for constructing each special format, not evidence of played victories.
   var progress=library.Data.story.heroes.Single(h=>h.heroPreconId==storyHeroId);foreach(var node in storyCampaign.Encounters)if(!progress.completed.Contains(node.id))progress.completed.Add(node.id);
   foreach(string format in new[]{"teams","ffa3","ffa4"})
   {
    var node=storyCampaign.Encounters.Single(n=>n.format==format);Check(StartStoryEncounter(node.id,97),"special "+format);
    Check(players==(format=="ffa3"?3:4)&&teams==(format=="teams")&&(!teams||match.Seats[0].Team==match.Seats[2].Team&&match.Seats[1].Team==match.Seats[3].Team),"format seating "+format);
    Check(storyBots.Skip(1).All(b=>b!=null)&&storyBattleDecks.Skip(1).All(d=>d.id!=storyBattleDecks[0].id),"AI opponents/ally "+format);yield return Capture("04-"+format+".png");
   }
   Check(library.Data.coins==coins&&library.Data.owned.Count==0&&library.Data.decks.Count==0,"temporary decks preserve inventory and balance");
   File.WriteAllText(Path.Combine(dir,"result.txt"),"PASS: "+checks+" checks; picker/map/full-deck construction; sampled AI/human actions; concession reward fixtures and persistence; special seating; runtime errors="+errors.Count);
   Application.logMessageReceived-=listen;Application.Quit(0);
   void Check(bool ok,string why){checks++;if(!ok)throw new Exception("STORY RUNTIME: "+why);}
   IEnumerator Capture(string name){yield return new WaitForSecondsRealtime(.15f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name));yield return null;}
  }
 }
}
