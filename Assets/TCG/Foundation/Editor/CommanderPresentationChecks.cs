using System;
using System.IO;
using System.Linq;
using TCG.Table;
using UnityEngine;
using UnityEditor;
namespace TCG.Foundation.Editor
{
 public static class CommanderPresentationChecks
 {
  public static void Run()
  {
   int checks=0;void Check(bool ok,string message){checks++;if(!ok)throw new Exception("COMMANDER PRESENTATION: "+message);}
   var catalog=ContentLoader.Load(new EffectRegistry());var cards=catalog.Cards.Where(c=>c.IsCommander).ToArray();Check(cards.Length==29,"all commanders retained");
   var manifest=JsonUtility.FromJson<CardArtManifest>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"CardArt","manifest.json")));
   foreach(var card in cards)
   {
    var prefab=Resources.Load<GameObject>("CommanderModels/"+card.Id+"-v1");Check(prefab!=null,"prefab "+card.Id);
    Check(prefab.GetComponentsInChildren<Collider>().Length==0,"art has no gameplay collider "+card.Id);
    Check(prefab.GetComponentsInChildren<Renderer>().Length>=20,"model detail "+card.Id);
    foreach(var mesh in prefab.GetComponentsInChildren<MeshFilter>().Select(x=>x.sharedMesh).Distinct())
    {Check(mesh!=null&&mesh.vertexCount>=600,"smooth tessellation "+card.Id);Check(mesh.normals.Length==mesh.vertexCount&&mesh.normals.All(n=>!float.IsNaN(n.x)&&n.sqrMagnitude>.5f),"finite surface normals "+card.Id);}
    var texture=Resources.Load<Texture2D>("CardArt/Commanders/"+card.Id+"-portrait-v1");Check(texture!=null&&texture.width>=256&&texture.height>=256,"individual imported image "+card.Id);
    Check(manifest.entries.Count(x=>x.cardId==card.Id&&x.styleId=="standard"&&x.resourcePath=="CardArt/Commanders/"+card.Id+"-portrait-v1")==1,"individual standard art mapping "+card.Id);
    foreach(MotionState state in Enum.GetValues(typeof(MotionState)))
    {var clip=Resources.Load<AnimationClip>("CommanderModels/Animations/"+card.Id+"-"+state);Check(clip!=null&&clip.length>0&&AnimationUtility.GetCurveBindings(clip).Length>0,"saved clip "+card.Id+" "+state);}
    var instance=UnityEngine.Object.Instantiate(prefab);var root=instance.transform;var motion=instance.AddComponent<FigureMotion>();motion.Initialize(card.Art,11);var rootPosition=root.position;var rootRotation=root.rotation;var rootScale=root.localScale;var bones=root.GetComponentsInChildren<Transform>();
    foreach(MotionState state in Enum.GetValues(typeof(MotionState)))
    {
     motion.EvaluatePose(.41f,state==MotionState.Walking?1:0,state,state==MotionState.Dying?1.15f:.35f);var positions=bones.Select(t=>t.localPosition).ToArray();var rotations=bones.Select(t=>t.localRotation).ToArray();
     motion.EvaluatePose(.41f,state==MotionState.Walking?1:0,state,state==MotionState.Dying?1.15f:.35f);
     Check(bones.Select((t,i)=>(t.localPosition-positions[i]).sqrMagnitude<.0000001f&&Quaternion.Angle(t.localRotation,rotations[i])<.01f).All(x=>x),"deterministic pose "+card.Id+" "+state);
     if(state==MotionState.Dying)Check(Quaternion.Angle(bones.First(t=>t.name=="SeatEmblem").localRotation,Quaternion.identity)<.01f,"death keeps pedestal level "+card.Id);
     Check(root.position==rootPosition&&root.rotation==rootRotation&&root.localScale==rootScale,"pose preserves picking root "+card.Id+" "+state);
    }
    var attackArm=bones.FirstOrDefault(t=>t.name=="RightArm"||t.name=="RiderRightArm");
    if(attackArm!=null){motion.EvaluatePose(0,0,MotionState.Idle,0);float resting=attackArm.TransformDirection(Vector3.down).z;motion.EvaluatePose(0,0,MotionState.Attacking,.35f);Check(attackArm.TransformDirection(Vector3.down).z<resting-.2f,"attack points toward facing -Z "+card.Id);}
    var marker=bones.First(t=>t.name=="SeatEmblem");Check(Quaternion.Angle(marker.localRotation,Quaternion.identity)<.01f,"base stays level during death "+card.Id);
    motion.PlayAttack();Check(motion.State==MotionState.Attacking,"attack enters state");motion.PlayDeath();motion.PlayAttack();Check(motion.State==MotionState.Dying,"death prevents attack");UnityEngine.Object.DestroyImmediate(instance);
   }
   Debug.Log("COMMANDER PRESENTATION CHECKS PASSED: "+checks);
  }
 }
}
