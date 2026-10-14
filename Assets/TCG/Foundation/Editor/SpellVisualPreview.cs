using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using TCG.Foundation;
using TCG.Table;
using UnityEditor;
using UnityEngine;
namespace TCG.Foundation.Editor
{
 public sealed class SpellVisualProbe:MonoBehaviour
 {
  public TableWorld World; public ContentCatalog Catalog; int checks; string caption;
  void Check(bool ok,string why){checks++;if(!ok)throw new Exception("SPELL PLAY: "+why);}
  void OnGUI(){var previous=GUI.color;GUI.color=Color.black;GUI.DrawTexture(new Rect(0,0,Screen.width,85),Texture2D.whiteTexture);GUI.color=previous;GUI.Label(new Rect(20,20,Screen.width-40,60),caption,new GUIStyle(GUI.skin.label){fontSize=22,normal={textColor=Color.white}});}
  IEnumerator Start()
  {
   const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
   var effects=new EffectRegistry();var local=ContentLoader.TestTable(Catalog,effects,2,false,1414);World.Bind(local);
   yield return null;
   var card=Catalog.Get("MED-148");var view=local.ViewFor(0);var remote=Match.FromView(view,Catalog,effects);World.Bind(remote);yield return null;
   view.visuals=new[]{new NetVisual{kind="cast",from=55,to=65,card=NetworkCards.Pack(card)}};view.revision++;
   remote.ApplyView(view,Catalog);yield return null;
   Check(World.SpellPresentationCount==1,"projected cast creates one effect");
   Check((World.GetComponentInChildren<SpellPresentationEffect>().transform.position-(TableWorld.Position(55)+Vector3.up*.17f)).sqrMagnitude<.001f,"announcement at caster capital only");
   yield return new WaitForSecondsRealtime(1.2f);Check(World.SpellPresentationCount==0,"announcement expires");
   view.visuals=new[]{new NetVisual{kind="impact",from=55,to=65,card=NetworkCards.Pack(card)}};view.revision++;
   remote.ApplyView(view,Catalog);yield return null;
   Check(World.SpellPresentationCount==1,"projected impact creates one effect");
   Check((World.GetComponentInChildren<SpellPresentationEffect>().transform.position-(TableWorld.Position(65)+Vector3.up*.17f)).sqrMagnitude<.001f,"impact at resolved target");
   yield return new WaitForSecondsRealtime(1.8f);Check(World.SpellPresentationCount==0,"resolution expires");
   for(int i=0;i<30;i++)World.PreviewSpellPresentation(card,60,SpellVisualStage.Resolution);
   Check(World.SpellPresentationCount==24,"burst capped immediately");yield return null;
   foreach(var line in World.GetComponentsInChildren<SpellPresentationEffect>().SelectMany(x=>x.GetComponentsInChildren<LineRenderer>()))for(int i=0;i<line.positionCount;i++){var p=line.GetPosition(i);Check(!float.IsNaN(p.x)&&!float.IsInfinity(p.x)&&!float.IsNaN(p.y)&&!float.IsInfinity(p.y)&&!float.IsNaN(p.z)&&!float.IsInfinity(p.z),"finite geometry");}
   World.Bind(local);yield return new WaitForSecondsRealtime(.1f);Check(World.SpellPresentationCount==0,"rebind clears effects");
   World.Distance=13;World.Elevation=65;World.Yaw=0;
   Directory.CreateDirectory("Documentacao/Arte/Precon-D02-20261014/VFX");
   foreach(SpellVisualStage stage in Enum.GetValues(typeof(SpellVisualStage)))
   {
    caption="Magias — "+stage+" — Sol / Lua / Água / Fogo / Ar / Terra / Incolor";
    for(int color=0;color<7;color++){
     var fixture=NetworkCards.Unpack(new CardData{id="vfx-color-"+color,name="Visual",kind="Instant",color=color,identityColors=new[]{color}});
     World.PreviewSpellPresentation(fixture,57+color,stage);
    }
    yield return new WaitForSecondsRealtime(stage==SpellVisualStage.Cancellation?.2f:.4f);
    ScreenCapture.CaptureScreenshot(Path.GetFullPath("Documentacao/Arte/Precon-D02-20261014/VFX/"+stage+".png"));
    yield return new WaitForSecondsRealtime(1.9f);Check(World.SpellPresentationCount==0,"all seven expire "+stage);
   }
   var precon=PreconstructedDeckLoader.Load(Catalog).Decks.First(d=>d.id=="COL001-D02");
   var samples=precon.main.Select(Catalog.Get).Where(c=>c.Kind==CardType.Spell||c.Kind==CardType.Instant).GroupBy(SpellPresentation.Family).Select(g=>g.First()).ToArray();
   caption="Arquivo das marés — magias de compra, controle, proteção, movimento e cura";
   for(int i=0;i<samples.Length;i++)World.PreviewSpellPresentation(samples[i],56+i*2,SpellVisualStage.Resolution);
   yield return new WaitForSecondsRealtime(.4f);
   foreach(var visual in World.GetComponentsInChildren<SpellPresentationEffect>()){
    Check(visual.GetComponentInChildren<ParticleSystem>()?.particleCount==24,"24 bounded soft motes");
    Check(visual.GetComponentsInChildren<Collider>().Length==0,"no input collider");
    Check(visual.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterial.shader.isSupported),"supported shaders");
   }
   ScreenCapture.CaptureScreenshot(Path.GetFullPath("Documentacao/Arte/Precon-D02-20261014/VFX/D02-motifs.png"));yield return new WaitForSecondsRealtime(1.8f);Check(World.SpellPresentationCount==0,"D02 motifs expire");
   SpellVisualPreview.Finish(true,"PASS: "+checks+" Play checks; projected origin/target, cap, finite geometry, lifetime, rebind and seven-color stage captures.");
  }
 }
 [InitializeOnLoad] public static class SpellVisualPreview
 {
  const string Key="TCG.SpellPreview";static double deadline;
  static SpellVisualPreview(){if(SessionState.GetBool(Key,false))Register();}
  static void Register(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;deadline=EditorApplication.timeSinceStartup+120;}
  public static void Run(){ProjectEntry.Open();SessionState.SetBool(Key,true);Register();EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView")).Show();EditorApplication.isPlaying=true;}
  static void Log(string m,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)Finish(false,m+"\n"+trace);}
  static void Tick(){if(!SessionState.GetBool(Key,false))return;if(EditorApplication.timeSinceStartup>deadline){Finish(false,"Play timeout");return;}if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;var table=UnityEngine.Object.FindFirstObjectByType<TableView>();if(table==null||table.GetComponent<SpellVisualProbe>()!=null)return;const BindingFlags f=BindingFlags.Instance|BindingFlags.NonPublic;var probe=table.gameObject.AddComponent<SpellVisualProbe>();probe.World=(TableWorld)typeof(TableView).GetField("world",f).GetValue(table);probe.Catalog=(ContentCatalog)typeof(TableView).GetField("catalog",f).GetValue(table);table.enabled=false;}
  public static void Finish(bool ok,string message){SessionState.SetBool(Key,false);Directory.CreateDirectory("Logs/SpellVisualVerification");File.WriteAllText("Logs/SpellVisualVerification/result.txt",(ok?"":"FAIL: ")+message);EditorApplication.Exit(ok?0:1);}
 }
}
