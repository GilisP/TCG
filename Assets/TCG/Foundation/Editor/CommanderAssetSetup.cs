using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TCG.Table;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace TCG.Foundation.Editor
{
 public static class CommanderAssetSetup
 {
  const string DirectoryName="Assets/TCG/Resources/CommanderModels";
  [MenuItem("TCG/Arte/Gerar modelos e animações dos comandantes")]
  public static void Generate()
  {
   if(Application.isBatchMode&&string.IsNullOrEmpty(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path))EditorSceneManager.OpenScene("Assets/TCG/Scenes/Mesa.unity");
   var pipeline=GraphicsSettings.defaultRenderPipeline;var quality=QualitySettings.renderPipeline;var ambient=RenderSettings.ambientLight;
   var host=new GameObject("Commander asset baker");var world=host.AddComponent<TableWorld>();
   var catalog=ContentLoader.Load(new EffectRegistry());var cards=catalog.Cards.Where(c=>c.IsCommander).OrderBy(c=>c.Id).ToArray();
   var folder=Path.GetFullPath("Documentacao/Arte/Comandantes-20261014");System.IO.Directory.CreateDirectory(folder);
   System.IO.Directory.CreateDirectory(DirectoryName+"/Geometry");System.IO.Directory.CreateDirectory(DirectoryName+"/Materials");System.IO.Directory.CreateDirectory(DirectoryName+"/Animations");
   AssetDatabase.Refresh();var meshes=new Dictionary<int,Mesh>();var materials=new Dictionary<int,Material>();
   var atlas=new Texture2D(6*256,5*288,TextureFormat.RGB24,false);var blank=new Color[atlas.width*atlas.height];for(int i=0;i<blank.Length;i++)blank[i]=new Color(.07f,.1f,.12f);atlas.SetPixels(blank);
   try
   {
    GraphicsSettings.defaultRenderPipeline=null;QualitySettings.renderPipeline=null;RenderSettings.ambientLight=new Color(.4f,.43f,.48f);
    for(int index=0;index<cards.Length;index++)
    {
     var card=cards[index];var model=world.CreateCommanderModel(card,TableWorld.Seats[0],null,false);
     foreach(var filter in model.GetComponentsInChildren<MeshFilter>())
     {
      var source=filter.sharedMesh;int id=source.GetInstanceID();if(!meshes.TryGetValue(id,out var mesh))
      {string path=DirectoryName+"/Geometry/"+source.name.Replace(" ","-")+".asset";mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh==null){mesh=UnityEngine.Object.Instantiate(source);AssetDatabase.CreateAsset(mesh,path);}else EditorUtility.CopySerialized(source,mesh);meshes.Add(id,mesh);}filter.sharedMesh=mesh;
     }
     foreach(var renderer in model.GetComponentsInChildren<Renderer>())
     {
      var source=renderer.sharedMaterial;int id=source.GetInstanceID();if(!materials.TryGetValue(id,out var material))
      {Color32 c=source.color;string path=DirectoryName+"/Materials/"+c.r.ToString("X2")+c.g.ToString("X2")+c.b.ToString("X2")+c.a.ToString("X2")+".mat";material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material==null){material=UnityEngine.Object.Instantiate(source);AssetDatabase.CreateAsset(material,path);}else EditorUtility.CopySerialized(source,material);materials.Add(id,material);}material.enableInstancing=true;EditorUtility.SetDirty(material);renderer.sharedMaterial=material;
     }
     PrefabUtility.SaveAsPrefabAsset(model.gameObject,DirectoryName+"/"+card.Id+"-v1.prefab");
     var preview=Render(model);atlas.SetPixels((index%6)*256,(4-index/6)*288,256,288,preview.GetPixels());System.IO.File.WriteAllBytes(Path.Combine(folder,card.Id+"-model-preview.png"),preview.EncodeToPNG());UnityEngine.Object.DestroyImmediate(preview);
     BakeAnimations(model,card.Id);UnityEngine.Object.DestroyImmediate(model.gameObject);
    }
    atlas.Apply();System.IO.File.WriteAllBytes(Path.Combine(folder,"commander-models-atlas.png"),atlas.EncodeToPNG());
    System.IO.File.WriteAllText(Path.Combine(folder,"model-atlas-order.txt"),string.Join(Environment.NewLine,cards.Select(c=>c.Id+" — "+c.Name)));
    RenderAnimationSamples(cards,folder);
    var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);var gallery=new GameObject("Galeria de comandantes");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gallery,scene);gallery.AddComponent<CommanderShowcase>();
    EditorSceneManager.SaveScene(scene,"Assets/TCG/Scenes/GaleriaComandantes.unity");EditorSceneManager.CloseScene(scene,true);
    AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("COMMANDER ASSETS GENERATED: "+cards.Length+" prefabs; "+(cards.Length*5)+" animation clips");
   }
   finally
   {
    UnityEngine.Object.DestroyImmediate(atlas);world.ReleasePreviewMaterials();UnityEngine.Object.DestroyImmediate(host);
    GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=quality;RenderSettings.ambientLight=ambient;
   }
  }
  static Texture2D Render(Transform root,float framing=.68f)
  {
   root.position=new Vector3(1000,1000,1000);foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=29;
   var cameraGo=new GameObject("Commander render camera");var cam=cameraGo.AddComponent<Camera>();cam.cullingMask=1<<29;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.07f,.1f,.12f);cam.orthographic=true;cam.orthographicSize=framing;cam.nearClipPlane=.01f;cam.farClipPlane=12;
   cam.transform.position=root.position+new Vector3(-1.3f,1.05f,-2);cam.transform.LookAt(root.position+Vector3.up*.4f);
   var key=new GameObject("Commander key light");var light=key.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.3f;light.color=new Color(1,.91f,.8f);light.cullingMask=1<<29;key.transform.rotation=Quaternion.Euler(38,-36,0);
   var rt=RenderTexture.GetTemporary(256,288,24);var previous=RenderTexture.active;
   try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var texture=new Texture2D(256,288,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,256,288),0,0);texture.Apply();return texture;}
   finally{RenderTexture.active=previous;cam.targetTexture=null;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(cameraGo);UnityEngine.Object.DestroyImmediate(key);root.position=Vector3.zero;}
  }
  static void RenderAnimationSamples(Definition[] cards,string folder)
  {
   string[] ids={"MED-193","MED-195","MED-201","MED-229","MED-232"};var sheet=new Texture2D(1280,1440,TextureFormat.RGB24,false);
   for(int row=0;row<ids.Length;row++)
   {
    var card=cards.First(c=>c.Id==ids[row]);var model=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("CommanderModels/"+card.Id+"-v1")).transform;var motion=model.gameObject.AddComponent<FigureMotion>();motion.Initialize(card.Art,11);
    foreach(MotionState state in Enum.GetValues(typeof(MotionState)))
    {motion.EvaluatePose(.3f,state==MotionState.Walking?1:0,state,state==MotionState.Dying?FigureMotion.DeathDuration:.35f);var shot=Render(model,.8f);sheet.SetPixels((int)state*256,(4-row)*288,256,288,shot.GetPixels());UnityEngine.Object.DestroyImmediate(shot);}
    UnityEngine.Object.DestroyImmediate(model.gameObject);
   }
   sheet.Apply();File.WriteAllBytes(Path.Combine(folder,"commander-animation-poses.png"),sheet.EncodeToPNG());UnityEngine.Object.DestroyImmediate(sheet);
  }
  public static void ValidateGalleryEntry()
  {
   if(Application.isBatchMode&&string.IsNullOrEmpty(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path))EditorSceneManager.OpenScene("Assets/TCG/Scenes/Mesa.unity");
   OpenGallery();if(AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene)!="Assets/TCG/Scenes/GaleriaComandantes.unity")throw new Exception("Gallery Play entry missing");
   ProjectEntry.Open();if(AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene)!="Assets/TCG/Scenes/Mesa.unity")throw new Exception("Main Play entry not restored");Debug.Log("COMMANDER GALLERY ENTRY PASSED");
  }
  static void BakeAnimations(Transform root,string cardId)
  {
   var motion=root.gameObject.AddComponent<FigureMotion>();motion.Initialize("commander",17);
   var bones=root.GetComponentsInChildren<Transform>().Where(t=>t!=root&&(t.name.EndsWith("Arm")||t.name.EndsWith("Leg")||t.name.EndsWith("Head")||t.name.EndsWith("Body")||t.name.EndsWith("Cape")||t.name=="Tail")).ToArray();
   foreach(MotionState state in Enum.GetValues(typeof(MotionState)))
   {
    float duration=state==MotionState.Attacking?FigureMotion.AttackDuration:state==MotionState.Dying?FigureMotion.DeathDuration:state==MotionState.Walking?4*Mathf.PI/13:2;
    var curves=new AnimationCurve[bones.Length,7];for(int b=0;b<bones.Length;b++)for(int channel=0;channel<7;channel++)curves[b,channel]=new AnimationCurve();
    int frames=Mathf.CeilToInt(duration*30);
    for(int frame=0;frame<=frames;frame++)
    {
     float time=duration*frame/frames;float poseTime=frame==frames&&(state==MotionState.Idle||state==MotionState.Walking||state==MotionState.Celebrating)?0:time;
     motion.EvaluatePose(poseTime,state==MotionState.Walking?1:0,state,poseTime);
     for(int b=0;b<bones.Length;b++){var p=bones[b].localPosition;var q=bones[b].localRotation;float[] values={p.x,p.y,p.z,q.x,q.y,q.z,q.w};for(int channel=0;channel<7;channel++)curves[b,channel].AddKey(time,values[channel]);}
    }
    var clip=new AnimationClip{name=cardId+"-"+state,frameRate=30};string[] properties={"m_LocalPosition.x","m_LocalPosition.y","m_LocalPosition.z","m_LocalRotation.x","m_LocalRotation.y","m_LocalRotation.z","m_LocalRotation.w"};
    for(int b=0;b<bones.Length;b++)for(int channel=0;channel<7;channel++)AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(AnimationUtility.CalculateTransformPath(bones[b],root),typeof(Transform),properties[channel]),curves[b,channel]);
    clip.EnsureQuaternionContinuity();var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=state==MotionState.Idle||state==MotionState.Walking||state==MotionState.Celebrating;AnimationUtility.SetAnimationClipSettings(clip,settings);
    string path=DirectoryName+"/Animations/"+clip.name+".anim";var existing=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);if(existing==null)AssetDatabase.CreateAsset(clip,path);else{EditorUtility.CopySerialized(clip,existing);UnityEngine.Object.DestroyImmediate(clip);}
   }
  }
  [MenuItem("TCG/Arte/Abrir galeria dos comandantes")]
  public static void OpenGallery()
  {
   if(EditorApplication.isPlayingOrWillChangePlaymode)return;
   if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
   {SessionState.SetBool(ProjectEntry.GalleryPreviewKey,true);EditorSceneManager.OpenScene("Assets/TCG/Scenes/GaleriaComandantes.unity");EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/TCG/Scenes/GaleriaComandantes.unity");}
  }
  [MenuItem("TCG/Arte/Visualizar comandantes no Play")]
  public static void PlayGallery(){OpenGallery();if(!EditorApplication.isPlayingOrWillChangePlaymode&&UnityEngine.SceneManagement.SceneManager.GetActiveScene().path=="Assets/TCG/Scenes/GaleriaComandantes.unity")EditorApplication.delayCall+=()=>{EditorApplication.isPlaying=true;EditorApplication.ExecuteMenuItem("Window/General/Game");};}
 }
}
