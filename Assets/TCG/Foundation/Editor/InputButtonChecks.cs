using System;
using System.IO;
using System.Reflection;
using TCG.Table;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace TCG.Foundation.Editor
{
 public sealed class InputButtonProbe:MonoBehaviour
 {
  public static int Calls;
  void OnGUI()
  {
   var table=UnityEngine.Object.FindFirstObjectByType<TableView>();if(table==null)return;
   var saved=Event.current.mousePosition;
   try{Event.current.mousePosition=new Vector2(5,5);typeof(TableView).GetMethod("Button",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(table,new object[]{0f,0f,20f,20f,"Input check",true,false});Calls++;}
   finally{Event.current.mousePosition=saved;}
  }
 }
 [InitializeOnLoad]
 public static class InputButtonChecks
 {
  const string Key="TCG.InputCheck.";static double next;
  static InputButtonChecks(){if(SessionState.GetBool(Key+"running",false))Register();}
  static void Register(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;}
  public static void Run(){ProjectEntry.Open();SessionState.SetBool(Key+"running",true);SessionState.SetInt(Key+"stage",0);SessionState.SetFloat(Key+"deadline",(float)EditorApplication.timeSinceStartup+90);Register();EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView")).Show();EditorApplication.isPlaying=true;}
  static void Log(string message,string trace,LogType type){if(type==LogType.Exception||type==LogType.Error||type==LogType.Assert)Finish(false,message);}
  static void Tick()
  {
   if(!SessionState.GetBool(Key+"running",false))return;
   if(EditorApplication.timeSinceStartup>SessionState.GetFloat(Key+"deadline",0)){Finish(false,"Play timeout");return;}
   if(!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.timeSinceStartup<next)return;
   var table=UnityEngine.Object.FindFirstObjectByType<TableView>();if(table==null)return;
   if(UnityEngine.Object.FindFirstObjectByType<InputButtonProbe>()==null)table.gameObject.AddComponent<InputButtonProbe>();
   if(InputButtonProbe.Calls<12){EditorApplication.QueuePlayerLoopUpdate();return;}
   int stage=SessionState.GetInt(Key+"stage",0);
   if(stage>=4){Finish(true,"PASS: Editor Play with Input System-only; forced hovered Button calls="+InputButtonProbe.Calls+"; four hub pages; no exceptions");return;}
   var type=typeof(TableView);var page=type.GetNestedType("HubPage",BindingFlags.NonPublic);type.GetMethod("NavigateHub",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(table,new[]{Enum.ToObject(page,stage)});SessionState.SetInt(Key+"stage",stage+1);next=EditorApplication.timeSinceStartup+.5;EditorApplication.QueuePlayerLoopUpdate();
  }
  static void Finish(bool ok,string message){SessionState.SetBool(Key+"running",false);Directory.CreateDirectory("Logs/InputButtonVerification");File.WriteAllText("Logs/InputButtonVerification/result.txt",message);EditorApplication.Exit(ok?0:1);}
 }
}
