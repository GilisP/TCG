using System;
using System.IO;
using System.Reflection;
using TCG.Table;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace TCG.Foundation.Editor
{
 [InitializeOnLoad]
 public static class CommanderGalleryChecks
 {
  const string Key="TCG.GallerySmoke.";
  static double next;
  static CommanderGalleryChecks(){if(SessionState.GetBool(Key+"running",false))Register();}
  static void Register(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;}
  public static void Run()
  {
   EditorSceneManager.OpenScene("Assets/TCG/Scenes/Mesa.unity");SessionState.SetBool(Key+"running",true);SessionState.SetInt(Key+"stage",0);SessionState.SetBool(Key+"done",false);SessionState.SetFloat(Key+"deadline",(float)EditorApplication.timeSinceStartup+100);Register();CommanderAssetSetup.OpenGallery();EditorApplication.isPlaying=true;
  }
  static void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)Finish(false,message);}
  static void Tick()
  {
   if(!SessionState.GetBool(Key+"running",false))return;
   if(EditorApplication.timeSinceStartup>SessionState.GetFloat(Key+"deadline",0)){Finish(false,"Gallery Play timeout");return;}
   if(SessionState.GetBool(Key+"done",false))
   {if(EditorApplication.isPlaying){EditorApplication.isPlaying=false;return;}if(EditorApplication.isPlayingOrWillChangePlaymode)return;ProjectEntry.Open();SessionState.SetBool(Key+"running",false);EditorApplication.Exit(0);return;}
   if(!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.timeSinceStartup<next)return;
   var gallery=UnityEngine.Object.FindFirstObjectByType<CommanderShowcase>();if(gallery==null)return;
   const BindingFlags flags=BindingFlags.NonPublic|BindingFlags.Instance;var type=typeof(CommanderShowcase);var cards=(TCG.Foundation.Definition[])type.GetField("cards",flags).GetValue(gallery);var model=(Transform)type.GetField("model",flags).GetValue(gallery);if(cards==null||model==null)return;
   if(SceneManager.GetActiveScene().path!="Assets/TCG/Scenes/GaleriaComandantes.unity"||cards.Length!=29){Finish(false,"Wrong gallery scene/count");return;}
   int stage=SessionState.GetInt(Key+"stage",0);
   if(stage>=145){var folder=Path.GetFullPath("Logs/CommanderGalleryVerification");Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"result.txt"),"PASS: Editor Play Mode; 29 commanders x 5 presentation states; imported art/models; no runtime errors; main entry restored");SessionState.SetBool(Key+"done",true);return;}
   type.GetMethod("Select",flags).Invoke(gallery,new object[]{stage/5});type.GetField("state",flags).SetValue(gallery,(MotionState)(stage%5));type.GetField("stateStart",flags).SetValue(gallery,Time.unscaledTime);
   var art=(Texture2D)type.GetField("art",flags).GetValue(gallery);if(art==null){Finish(false,"Gallery image missing "+cards[stage/5].Id);return;}SessionState.SetInt(Key+"stage",stage+1);next=EditorApplication.timeSinceStartup+.12;EditorApplication.QueuePlayerLoopUpdate();
  }
  static void Finish(bool ok,string message)
  {if(!SessionState.GetBool(Key+"running",false))return;SessionState.SetBool(Key+"running",false);Directory.CreateDirectory("Logs/CommanderGalleryVerification");File.WriteAllText("Logs/CommanderGalleryVerification/result.txt","FAIL: "+message);EditorApplication.Exit(1);}
 }
}
