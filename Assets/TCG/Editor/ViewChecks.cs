using System;
using System.Reflection;
using UnityEngine;

namespace TCG.Editor
{
    public static class ViewChecks
    {
        const BindingFlags Flags = BindingFlags.Instance|BindingFlags.NonPublic;
        static void Check(bool ok,string message) { if (!ok) throw new Exception("TCG VIEW TEST FAILED: "+message); }
        public static void Run()
        {
            var go = new GameObject("TCG UI logic checks");
            try
            {
                var view = go.AddComponent<GameView>(); var type = typeof(GameView);
                Func<string,object> get = name => type.GetField(name,Flags).GetValue(view);
                Action<string,object> set = (name,value) => type.GetField(name,Flags).SetValue(view,value);
                Func<string,object[],object> call = (name,args) => type.GetMethod(name,Flags).Invoke(view,args);
                call("Awake",Array.Empty<object>());
                var game = (Game)get("game");
                game.AdvancePhase(); game.AdvancePhase(); game.Place(0,16); game.AdvancePhase();
                game.Current.Mana[6] = 20; game.Current.Hand.Clear(); game.Current.Hand.Add(new Card(Catalog.Get("unit-0-0"),"view-unit"));
                call("Choose",new object[] { 1 }); set("selection",0);
                Check((int)call("TargetKind",new object[] { 5 }) == 2,"summon highlight");
                call("ClickTile",new object[] { 5 });
                Check(game.Stack.Count == 1 && (int)get("selection") == -1,"cast from click clears selection");
                call("Execute",new object[] { (Action)game.PassPriority });
                Check((bool)get("handoff") && game.Priority == 1,"priority handoff");
                call("Execute",new object[] { (Action)game.PassPriority });
                Check(game.Board[5].Unit != null,"cast resolves through view controller");
                call("Choose",new object[] { 2 }); call("ClickTile",new object[] { 5 });
                Check((int)get("source") == 5 && (int)call("TargetKind",new object[] { 4 }) == 3,"select unit / movement highlight");
                call("ClickTile",new object[] { 4 });
                Check((int)get("source") == 4 && game.Board[4].Unit != null,"selection follows moving unit");
                var queue = (System.Collections.Generic.Queue<VisualEvent>)get("animations");
                Check(queue.Count >= 3,"terrain, summon and move animation events queued");
                game.Board[15].Unit = new Unit(1,new Card(Catalog.Get("unit-0-0"),"enemy"));
                Check((int)call("TargetKind",new object[] { 15 }) == 4,"attack highlight");
                call("ClickTile",new object[] { 15 }); Check(game.Stack[0].Kind == StackKind.Attack,"attack click uses stack");
                var deck = Catalog.DefaultDeck(); var card = Catalog.Get(deck.main[0]);
                call("ToggleCard",new object[] { deck,card }); Check(deck.main.Count == 99,"deck removal");
                call("ToggleCard",new object[] { deck,card }); Check(Catalog.Validate(deck).Count == 0,"deck addition preserves validity");
                call("ToggleCard",new object[] { deck,Catalog.Get("commander-lua") }); Check(deck.commander == "commander-lua","commander selection");
                call("Choose",new object[] { 4 }); call("ClickTile",new object[] { 16 });
                Check((int)get("source") == 16 && (int)get("mode") == 4,"domain inspection keeps selected tile");
                set("kindFilter",4);
                var collection=(System.Collections.Generic.List<CardDefinition>)call("FilterCollection",new object[]{deck});
                Check(collection.Count>0 && collection.TrueForAll(c=>c.Kind==CardKind.Equipment),"equipment filter in deck collection");
                set("kindFilter",5);
                collection=(System.Collections.Generic.List<CardDefinition>)call("FilterCollection",new object[]{deck});
                Check(collection.Count>0 && collection.TrueForAll(c=>c.Kind==CardKind.Building),"building filter in deck collection");
                Debug.Log("TCG_VIEW_VALIDATION_OK: 15 assertions; click flow, highlights, handoff, animation queue, deck editing and domains. Does not validate pixel rendering.");
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
    }
}
