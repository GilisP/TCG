using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TCG.Table;
using UnityEngine;
using UnityEditor;
namespace TCG.Foundation.Editor
{
 public sealed class CardArtLayoutCanvas:MonoBehaviour
 {
  public TableView Table;public Definition[] Cards;public static int Frames;
  const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
  void OnGUI()
  {
   if(Table==null||Cards==null)return;
   var type=typeof(TableView);type.GetMethod("Styles",Flags).Invoke(Table,null);var original=GUI.matrix;GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/1600f,Screen.height/1000f,1));
   GUI.Box(new Rect(0,0,1600,1000),GUIContent.none);
   var label=new GUIStyle(GUI.skin.label){fontSize=19,normal={textColor=Color.white}};
   GUI.Label(new Rect(25,10,1550,40),"TCG — arte e custo: S Sol / L Lua / G Água / F Fogo / A Ar / T Terra / N Incolor / gen. genérico",label);
   var render=type.GetMethod("RenderCardFace",Flags);
   for(int i=0;i<Cards.Length;i++)
   {
    float x=25+i*310;type.GetField("inspected",Flags).SetValue(Table,Cards[i]);type.GetField("inspectionFoil",Flags).SetValue(Table,i==2);render.Invoke(Table,new object[]{new Rect(x,65,286,429),Cards[i],"standard",false});GUI.Label(new Rect(x,505,295,70),Cards[i].Name+"\n"+TableView.PrintedManaCost(Cards[i]),label);
    render.Invoke(Table,new object[]{new Rect(x,605,120,180),Cards[i],"standard",false});render.Invoke(Table,new object[]{new Rect(x+150,605,82,123),Cards[i],"nocturne",false});GUI.Label(new Rect(x,800,285,70),"Mão 120px / noturna 82px\nAmplie para ler a ficha.",label);
   }
   GUI.matrix=original;Frames++;
  }
 }
 [InitializeOnLoad]
 public static class CardArtLayoutPreview
 {
  const string Key="TCG.ArtLayout.";static double next;
  static CardArtLayoutPreview(){if(SessionState.GetBool(Key+"running",false))Register();}
  static void Register(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;}
  public static void Run(){ProjectEntry.Open();SessionState.SetBool(Key+"running",true);SessionState.SetInt(Key+"stage",0);SessionState.SetFloat(Key+"deadline",(float)EditorApplication.timeSinceStartup+90);Register();EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView")).Show();EditorApplication.isPlaying=true;}
  static void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)Finish(false,message+"\n"+trace);}
  static void Tick()
  {
   if(!SessionState.GetBool(Key+"running",false))return;
   if(EditorApplication.timeSinceStartup>SessionState.GetFloat(Key+"deadline",0)){Finish(false,"Play timeout");return;}
   if(!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.timeSinceStartup<next)return;
   var table=UnityEngine.Object.FindFirstObjectByType<TableView>();if(table==null)return;const BindingFlags flags=BindingFlags.NonPublic|BindingFlags.Instance;
   int stage=SessionState.GetInt(Key+"stage",0);
   if(stage==0)
   {
    var catalog=(ContentCatalog)typeof(TableView).GetField("catalog",flags).GetValue(table);var canvas=table.gameObject.AddComponent<CardArtLayoutCanvas>();canvas.Table=table;canvas.Cards=new[]{"AUTOR-20261007-CARTA-001","AUTOR-20261007-TERRENO-001","MED-242","MED-230","MED-240"}.Select(catalog.Get).ToArray();var world=(TableWorld)typeof(TableView).GetField("world",flags).GetValue(table);foreach(var renderer in world.GetComponentsInChildren<Renderer>())renderer.enabled=false;world.enabled=false;world.View.clearFlags=CameraClearFlags.SolidColor;world.View.backgroundColor=new Color(.04f,.06f,.07f);table.enabled=false;SessionState.SetInt(Key+"stage",1);next=EditorApplication.timeSinceStartup+2;return;
   }
   if(stage==1){if(CardArtLayoutCanvas.Frames<3){EditorApplication.QueuePlayerLoopUpdate();return;}Directory.CreateDirectory("Documentacao/Arte/Precon-D01-20261014/Layouts");ScreenCapture.CaptureScreenshot(Path.GetFullPath("Documentacao/Arte/Precon-D01-20261014/Layouts/preview.png"));SessionState.SetInt(Key+"stage",2);next=EditorApplication.timeSinceStartup+2;return;}
   Finish(true,"PASS: native Editor Play layout render; five card faces at full/hand/thumbnail sizes; no GUI exceptions");
  }
  static void Finish(bool ok,string message){SessionState.SetBool(Key+"running",false);Directory.CreateDirectory("Logs/CardArtLayoutVerification");File.WriteAllText("Logs/CardArtLayoutVerification/result.txt",(ok?"":"FAIL: ")+message);EditorApplication.Exit(ok?0:1);}
 }
}
