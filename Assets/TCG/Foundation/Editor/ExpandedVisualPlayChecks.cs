using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TCG.Table;

namespace TCG.Foundation.Editor
{
    // Isolated Play verification: no TableView/profile/business state is created.
    [InitializeOnLoad]
    public static class ExpandedVisualPlayChecks
    {
        const string Key="TCG.ExpandedVisualPlayChecks";
        static GameObject holder;static Transform actor;static FigureMotion motion;static int frames,lastFrame=-1;
        static ExpandedVisualPlayChecks(){EditorApplication.playModeStateChanged+=Changed;}
        public static void OpenMain()
        {
            SessionState.SetBool(Key,false);
            EditorSceneManager.OpenScene("Assets/TCG/Scenes/Mesa.unity");
            EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/TCG/Scenes/Mesa.unity");
        }
        public static void Begin()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            SessionState.SetBool(Key,true);EditorSceneManager.playModeStartScene=null;EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Key,false))return;
            SessionState.SetBool(Key,false);
            EditorApplication.delayCall+=()=>
            {
                try{if(Object.FindObjectsByType<TableView>(FindObjectsSortMode.None).Length!=0)throw new System.Exception("Play fixture unexpectedly loaded profile-backed table");TroopPresentationChecks.Run();BeginFrames();}
                catch(System.Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
            };
        }
        static void BeginFrames()
        {
            holder=new GameObject("Isolated animated troop");var world=holder.AddComponent<TableWorld>();world.enabled=false;
            var card=ContentLoader.Load(new EffectRegistry()).Get("ED261007-03-C001");
            actor=(Transform)typeof(TableWorld).GetMethod("Figure",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(world,new object[]{card,TableWorld.Elements[3],holder.transform});
            motion=actor.gameObject.AddComponent<FigureMotion>();motion.Initialize(card.Art,101);frames=0;lastFrame=-1;EditorApplication.update+=Frame;
        }
        static void Require(bool ok,string message){if(!ok)throw new System.Exception("PLAY LIFECYCLE: "+message);}
        static void Frame()
        {
            if(Time.frameCount==lastFrame)return;lastFrame=Time.frameCount;
            try
            {
                if(frames<16)actor.position+=Vector3.right*.02f;
                if(frames==15)Require(motion.State==MotionState.Walking,"position updates select walking");
                if(frames==24)Require(motion.State==MotionState.Idle,"stopped position selects idle");
                if(frames==25)motion.PlayAttack();
                if(frames==26)Require(motion.State==MotionState.Attacking,"attack event state");
                if(frames==29)motion.PlayCelebrate();
                if(frames==30){Require(motion.State==MotionState.Celebrating,"celebration event state");motion.PlayAttack();Require(motion.State==MotionState.Celebrating,"celebration ignores attack");}
                if(frames==37)motion.PlayDeath();
                if(frames==38){Require(motion.State==MotionState.Dying,"death event state");motion.PlayCelebrate();Require(motion.State==MotionState.Dying,"death ignores celebration");}
                if(++frames<50)return;
                Require(Mathf.Abs(actor.position.x-.32f)<.0001f,"poses preserve gameplay root position");
                EditorApplication.update-=Frame;Object.DestroyImmediate(holder);Debug.Log("EXPANDED VISUAL PLAY PASSED: 8 lifecycle checks, isolated figures; no profile-backed TableView");EditorApplication.Exit(0);
            }
            catch(System.Exception e){EditorApplication.update-=Frame;if(holder!=null)Object.DestroyImmediate(holder);Debug.LogException(e);EditorApplication.Exit(1);}
        }
    }
}
