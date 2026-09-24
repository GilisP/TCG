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
  IEnumerator VerifyFoilCommanders()
  {
   string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","FoilCommanderVerification"));Directory.CreateDirectory(dir);
   var errors=new List<string>();Application.LogCallback listener=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors.Add(m);};Application.logMessageReceived+=listener;
   collectionStore=new CollectionStore(Path.Combine(dir,"profile-"+Guid.NewGuid().ToString("N")));library=collectionStore.Load(catalog);
   library.Collect("MED-232");library.SelectFoil("MED-232",true);collectionStore.Save(library);
   OpenLibrary();libraryFilters.Clear();libraryFilters.CommandersOnly=true;Require(FilteredLibrary().All(c=>c.IsCommander)&&FilteredLibrary().Length>=28,"commander filter");
   yield return Capture("01-so-comandantes.png");librarySearch="MED-232";inspected=catalog.Get("MED-232");yield return Capture("02-carta-foil.png");inspected=null;
   var main=Enumerable.Repeat("test-enchantment-blessing",48).ToArray();var lands=Enumerable.Repeat("test-land-6",50).ToArray();
   match=new Match(catalog,effects,2,false,22,new[]{main,main},new[]{lands,lands},new[]{"MED-232","MED-232"}){AutomaticResponses=true,AutoAdvanceAfterTerrain=true};
   menu=false;libraryOpen=false;handoff=false;useSavedDecks=false;world.Bind(match);int owner=match.Active,home=Match.Capital(owner,2);
   world.FoilAppearance=(who,card)=>who==owner&&card.Id=="MED-232";int before=world.FoilBurstCount;bool summoned=false;
   for(int n=0;n<35&&!summoned;n++){
    if(match.Phase==Stage.Draw)Do(ActionKind.DrawMain);
    if(match.Phase==Stage.Terrain){int at=Enumerable.Range(0,121).First(x=>match.CanPlace(x)&&match.Board[x].Terrain==null);Do(ActionKind.Place,at);}
    if(match.Active==owner&&match.CanSummonCommander(home)){Do(ActionKind.SummonCommander,home);summoned=true;break;}
    Do(ActionKind.NextPhase);
   }
   Require(summoned&&world.FoilBurstCount==before+1,"foil emitted by real summon exactly once");
   selectedCell=home;world.Distance=20;yield return Capture("03-entrada-foil.png",.12f);
   yield return new WaitForSecondsRealtime(2);Require(world.ArcaneBurstCount==0,"effects cleaned");
   var commander=match.Board[home].Pieces.Single(p=>p.Card.Id=="MED-232");int to=Match.Neighbors(home).Where(x=>match.CanMove(commander.Id,x)).DefaultIfEmpty(-1).First();
   if(to>=0){Do(ActionKind.Move,to,commander.Id);Require(world.FoilBurstCount==before+1,"movement does not replay entry");}
   var previews=new List<GameObject>();int index=0;foreach(string id in new[]{"MED-232","MED-234","MED-235","MED-238","MED-239","MED-242"}){previews.Add(world.PreviewModel(catalog.Get(id),new Vector3(-2.5f+(index%3)*2.5f,0,-1.5f+(index/3)*2.5f)));index++;}
   world.Distance=12;world.Yaw=8;yield return Capture("04-modelos-detalhados.png");foreach(var go in previews)Destroy(go);
   world.PreviewBurst(catalog.Get("MED-234"),59,false);world.PreviewBurst(catalog.Get("MED-236"),61,false);world.PreviewBurst(catalog.Get("MED-232"),60,true);yield return Capture("05-magias-e-foil.png",.3f);
   yield return new WaitForSecondsRealtime(2);Require(world.ArcaneBurstCount==0,"preview cleanup");
   library=collectionStore.Load(catalog);Require(library.SelectedFoil("MED-232"),"foil persists");
   File.WriteAllText(Path.Combine(dir,"result.txt"),"Commander filter, foil persistence, real commander summon and single entry burst, no repeat on movement, detailed model and spell captures, VFX cleanup passed. Runtime errors: "+errors.Count+Environment.NewLine+string.Join(Environment.NewLine,errors));
   Application.logMessageReceived-=listener;Application.Quit(errors.Count==0?0:1);
   void Require(bool ok,string label){if(!ok)throw new Exception("FOIL COMMANDER UI: "+label);}
   void Do(ActionKind kind,int target=-1,int unit=-1){if(!match.Try(new Command{player=match.Controller,revision=match.Revision,kind=kind,target=target,unit=unit,pile=0},out var error))throw new Exception("FOIL COMMANDER UI: "+error);}
   IEnumerator Capture(string name,float delay=.6f){yield return new WaitForSecondsRealtime(delay);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,name));yield return new WaitForSecondsRealtime(.5f);}
  }
 }
}
