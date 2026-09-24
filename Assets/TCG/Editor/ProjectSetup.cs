using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TCG.Editor
{
    public static class ProjectSetup
    {
        [MenuItem("TCG/Preparar cena e validar regras")]
        public static void Prepare()
        {
            const string path = "Assets/TCG/Scenes/Partida.unity";
            if (!AssetDatabase.IsValidFolder("Assets/TCG")) AssetDatabase.CreateFolder("Assets","TCG");
            if (!AssetDatabase.IsValidFolder("Assets/TCG/Scenes")) AssetDatabase.CreateFolder("Assets/TCG","Scenes");
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var camera = new GameObject("Camera").AddComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.047f,.071f,.09f);
                new GameObject("TCG Game").AddComponent<GameView>(); EditorSceneManager.SaveScene(scene,path);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path,true) }.Concat(EditorBuildSettings.scenes.Where(s => s.path != path)).ToArray();
            PlayerSettings.defaultScreenWidth = 1440; PlayerSettings.defaultScreenHeight = 960;
            AssetDatabase.SaveAssets(); Validate();
        }
        [MenuItem("TCG/Validar regras")]
        public static void Validate() { RuleChecks.Run(); ViewChecks.Run(); ExpansionChecks.Run(); KingdomChecks.Run(); JourneyChecks.Run(); ConfluenceChecks.Run(); }
    }
}
