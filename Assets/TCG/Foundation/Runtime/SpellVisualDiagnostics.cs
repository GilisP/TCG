using System;
using System.Collections;
using System.IO;
using System.Linq;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
 public sealed partial class TableView
 {
  IEnumerator VerifySpellVisuals()
  {
   var directory=Path.GetFullPath(Path.Combine(Application.dataPath,"..","SpellVisualVerification"));Directory.CreateDirectory(directory);
   int checks=0,errors=0;Application.LogCallback listen=(m,s,t)=>{if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors++;};Application.logMessageReceived+=listen;
   void Check(bool ok,string why){checks++;if(!ok)throw new Exception("SPELL PLAYER: "+why);}
   enabled=false;
   Check(Resources.Load<Shader>("SpellMote")?.isSupported==true,"soft-mote shader included and supported");
   Check(Resources.Load<Shader>("ArcaneGlow")?.isSupported==true,"ring shader included and supported");
   var source=ContentLoader.TestTable(catalog,effects,2,false,1414);var view=source.ViewFor(0);var remote=Match.FromView(view,catalog,effects);world.Bind(remote);yield return null;
   var precon=PreconstructedDeckLoader.Load(catalog).Decks.First(d=>d.id=="COL001-D02");
   var spells=precon.main.Select(catalog.Get).Where(c=>c.Kind==CardType.Spell||c.Kind==CardType.Instant).GroupBy(SpellPresentation.Family).Select(g=>g.First()).ToArray();
   foreach(var card in spells){view.visuals=new[]{new NetVisual{kind="impact",from=55,to=60,card=NetworkCards.Pack(card)}};view.revision++;remote.ApplyView(view,catalog);}
   yield return null;
   Check(world.SpellPresentationCount==spells.Length,"projected D02 motifs instantiate");
   foreach(var visual in world.GetComponentsInChildren<SpellPresentationEffect>()){
    var particles=visual.GetComponentInChildren<ParticleSystem>();Check(particles!=null&&particles.particleCount==24,"bounded luminous motes");
    Check(visual.GetComponentsInChildren<Collider>().Length==0,"visuals never intercept tile selection");
    Check(visual.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterial.shader.isSupported),"supported materials");
   }
   yield return new WaitForSecondsRealtime(1.8f);Check(world.SpellPresentationCount==0,"effects expire in player");
   for(int i=0;i<30;i++)world.PreviewSpellPresentation(spells[0],60,SpellVisualStage.Announcement);
   Check(world.SpellPresentationCount==24,"same-frame cap in player");
   world.Bind(source);yield return new WaitForSecondsRealtime(.1f);Check(world.SpellPresentationCount==0,"player rebind clears visuals");
   Check(errors==0,"no runtime errors");Application.logMessageReceived-=listen;
   File.WriteAllText(Path.Combine(directory,"result.txt"),"PASS: "+checks+" checks; D02 motifs="+spells.Length+"; errors="+errors);Application.Quit(0);
  }
 }
}
