using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TCG.Foundation.Editor
{
    [InitializeOnLoad]
    public static class ProjectEntry
    {
        const string ScenePath = "Assets/TCG/Scenes/Mesa.unity";
        static ProjectEntry() { EditorApplication.delayCall += ConfigurePlay; EditorApplication.playModeStateChanged += ObservePlay; }
        static void ObservePlay(PlayModeStateChange state) { if(state == PlayModeStateChange.EnteredPlayMode) Debug.Log("TCG EDITOR PLAY: " + Application.dataPath + " | scene=" + SceneManager.GetActiveScene().path + " | table=" + (UnityEngine.Object.FindFirstObjectByType<TCG.Table.TableWorld>() != null)); }
        static void ConfigurePlay()
        {
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (scene != null) EditorSceneManager.playModeStartScene = scene;
        }
        [MenuItem("TCG/Abrir jogo neste projeto", priority = 0)]
        public static void Open()
        {
            ConfigurePlay();
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (SceneManager.GetActiveScene().path != ScenePath)
            {
                for (int i=0;i<SceneManager.sceneCount;i++)
                    if (SceneManager.GetSceneAt(i).isDirty)
                        throw new InvalidOperationException("Salve a cena em edição antes de abrir Mesa.");
                EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            }
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log("TCG: cena principal aberta em " + Application.dataPath + "/TCG/Scenes/Mesa.unity. Use Play para jogar; a geometria é criada em execução.");
        }
        [MenuItem("TCG/Jogar no Unity", priority = 1)]
        public static void Play()
        {
            Open();
            EditorApplication.delayCall += () => { EditorApplication.isPlaying = true; EditorApplication.ExecuteMenuItem("Window/General/Game"); };
        }
        public static void Validate()
        {
            ConfigurePlay();
            if (EditorSceneManager.playModeStartScene == null || AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene) != ScenePath)
                throw new InvalidOperationException("A cena inicial do Play não foi configurada.");
            FoundationChecks.Run();
            Debug.Log("PROJECT ENTRY VERIFIED: " + Application.dataPath);
        }
    }
}