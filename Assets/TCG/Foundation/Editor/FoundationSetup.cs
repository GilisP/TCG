using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using TCG.Table;

namespace TCG.Foundation.Editor
{
    public static class FoundationSetup
    {
        [MenuItem("TCG/Base/Preparar mesa 3D")]
        public static void Prepare()
        {
            const string path="Assets/TCG/Scenes/Mesa.unity";
            if(!Directory.Exists("Assets/TCG/Resources")) Directory.CreateDirectory("Assets/TCG/Resources");
            AssetDatabase.Refresh();
            if(AssetDatabase.LoadAssetAtPath<Material>("Assets/TCG/Resources/TableSurface.mat")==null)
            {
                var material=new Material(Shader.Find("Standard")); material.SetFloat("_Glossiness",.12f);
                AssetDatabase.CreateAsset(material,"Assets/TCG/Resources/TableSurface.mat");
            }
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null)
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                new GameObject("Fronteiras • nova base").AddComponent<TableView>(); EditorSceneManager.SaveScene(scene,path);
            }
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(path,true)}.Concat(EditorBuildSettings.scenes.Where(s=>s.path!=path)).ToArray();
            PlayerSettings.defaultScreenWidth=1600; PlayerSettings.defaultScreenHeight=1000; PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            AssetDatabase.SaveAssets(); FoundationChecks.Run(); MedievalChecks.Run(); QualityOfLifeChecks.Run(); MovementOrderChecks.Run(); ReactionChecks.Run(); PresentationChecks.Run(); CollectionChecks.Run(); AuthorCardChecks.Run(); HubChecks.Run(); BicolorChecks.Run(); BoosterChecks.Run(); NewCommanderChecks.Run(); NetworkChecks.Run();
            Debug.Log("FOUNDATION PREPARE PASSED");
        }
        public static void Build()
        {
            Prepare();
            var output=Path.GetFullPath("Builds/BaseJogavel/Fronteiras.exe"); Directory.CreateDirectory(Path.GetDirectoryName(output));
            var quality=QualitySettings.GetQualityLevel(); var pipeline=GraphicsSettings.defaultRenderPipeline;
            var profiles=Enumerable.Range(0,QualitySettings.names.Length).Select(QualitySettings.GetRenderPipelineAssetAt).ToArray();
            try
            {
                // Package a 3D built-in profile without changing the legacy editor quality assets.
                for(int i=0;i<profiles.Length;i++) { QualitySettings.SetQualityLevel(i,false); QualitySettings.renderPipeline=null; }
                QualitySettings.SetQualityLevel(quality,false); GraphicsSettings.defaultRenderPipeline=null;
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/TCG/Scenes/Mesa.unity"},locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
                if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Build falhou: "+report.summary.result);
            }
            finally
            {
                GraphicsSettings.defaultRenderPipeline=pipeline;
                for(int i=0;i<profiles.Length;i++) { QualitySettings.SetQualityLevel(i,false); QualitySettings.renderPipeline=profiles[i]; }
                QualitySettings.SetQualityLevel(quality,false);
            }
            Debug.Log("FOUNDATION BUILD PASSED: "+output);
        }
    }
}

