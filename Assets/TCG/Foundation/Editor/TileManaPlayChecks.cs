using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TCG.Table;
using UnityEngine;
using UnityEditor;
namespace TCG.Foundation.Editor
{
 [InitializeOnLoad]
 public static class TileManaPlayChecks
 {
  const string Key="TCG.TileManaSmoke.";const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;static double next;
  static TileManaPlayChecks(){if(SessionState.GetBool(Key+"running",false))Register();}
  static void Register(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;}
  public static void Run(){MulticolorCostChecks.Run();ProjectEntry.Open();SessionState.SetBool(Key+"running",true);SessionState.SetInt(Key+"stage",0);SessionState.SetFloat(Key+"deadline",(float)EditorApplication.timeSinceStartup+100);Register();EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView")).Show();EditorApplication.isPlaying=true;}
  static void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)Finish(false,message);}
  static void Tick()
  {
   if(!SessionState.GetBool(Key+"running",false))return;
   if(EditorApplication.timeSinceStartup>SessionState.GetFloat(Key+"deadline",0)){Finish(false,"Play timeout");return;}
   if(!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.timeSinceStartup<next)return;
   var table=UnityEngine.Object.FindFirstObjectByType<TableView>();if(table==null)return;var type=typeof(TableView);int stage=SessionState.GetInt(Key+"stage",0);
   if(stage==0)
   {
    var catalog=(ContentCatalog)type.GetField("catalog",Flags).GetValue(table);var effects=new EffectRegistry();var view=ContentLoader.TestTable(catalog,effects,2,false,114).ViewFor(0);int home=Match.Capital(0,2);var card=catalog.Cards.First(c=>c.Kind==CardType.Creature&&!c.IsCommander);
    view.pieces=Enumerable.Range(0,12).Select(n=>new NetPiece{id=8000+n,owner=n%2,original=n%2,cell=home,card=NetworkCards.Pack(card),attack=2,defense=3,actions=1,movement=2,attached=-1,carrier=-1}).ToArray();
    var match=Match.FromView(view,catalog,effects);type.GetField("match",Flags).SetValue(table,match);((TableWorld)type.GetField("world",Flags).GetValue(table)).Bind(match);type.GetField("menu",Flags).SetValue(table,false);type.GetField("handoff",Flags).SetValue(table,false);type.GetField("deckSelectionOpen",Flags).SetValue(table,false);type.GetField("selectedUnit",Flags).SetValue(table,8000);type.GetField("selectedCard",Flags).SetValue(table,card.Id);
    int revision=match.Revision;type.GetMethod("SelectTileForInspection",Flags).Invoke(table,new object[]{home});
    if(match.Revision!=revision||(int)type.GetField("selectedUnit",Flags).GetValue(table)!=8000||(string)type.GetField("selectedCard",Flags).GetValue(table)!=card.Id||match.Board[home].Pieces.Count!=12)throw new Exception("Inspection changed match/action selection or lost occupants");
    SessionState.SetInt(Key+"stage",1);next=EditorApplication.timeSinceStartup+2;return;
   }
   if(stage==1){type.GetMethod("InspectTileCard",Flags).Invoke(table,new object[]{((Match)type.GetField("match",Flags).GetValue(table)).Board[Match.Capital(0,2)].Terrain});SessionState.SetInt(Key+"stage",2);next=EditorApplication.timeSinceStartup+1;return;}
   Finish(true,"PASS: Editor Play; multicolor payment audit; tile window with terrain +12 occupants; selection preserved; revision unchanged; card inspection renders; zero exceptions");
  }
  static void Finish(bool ok,string message){SessionState.SetBool(Key+"running",false);Directory.CreateDirectory("Logs/TileManaVerification");File.WriteAllText("Logs/TileManaVerification/result.txt",(ok?"":"FAIL: ")+message);EditorApplication.Exit(ok?0:1);}
 }
}
