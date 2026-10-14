using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using TCG.Table;

namespace TCG.Foundation.Editor
{
    public static class TroopPresentationChecks
    {
        [MenuItem("TCG/Arte/Verificar tropas suaves")]
        public static void Run()
        {
            int checks=0;void Check(bool ok,string label){checks++;if(!ok)throw new Exception("TROOP PRESENTATION: "+label);}
            var catalog=ContentLoader.Load(new EffectRegistry());
            var candidates=catalog.Cards.Where(c=>c.Kind==CardType.Creature&&!c.IsCommander&&!c.IsVehicle&&c.Art!="beast"&&c.Art!="serpent"&&c.Art!="winged").ToArray();
            Check(candidates.Length>20,"catalog troop profiles");
            var selection=candidates.GroupBy(c=>c.Subtypes.Contains("Goblin")?"goblin":c.Subtypes.Contains("Esqueleto")?"skeleton":c.Art).Select(g=>g.First()).Take(10)
                .Concat(candidates.Where(c=>new[]{"ED261007-03-C001","ED261007-03-C002","ED261007-03-C003","ED261007-03-C004","ED261007-03-C005"}.Contains(c.Id))).Distinct().ToArray();
            var holder=new GameObject("Troop verification");var world=holder.AddComponent<TableWorld>();
            var figure=typeof(TableWorld).GetMethod("Figure",BindingFlags.Instance|BindingFlags.NonPublic);
            Check(figure!=null,"shared figure entry");
            string folder=Path.GetFullPath("Logs/ExpansionVisuals");Directory.CreateDirectory(folder);
            try
            {
                foreach(var card in selection)
                {
                    var root=(Transform)figure.Invoke(world,new object[]{card,TableWorld.Elements[card.Color],null});
                    try
                    {
                        Check(root.name.StartsWith("Troop "),"smooth entry "+card.Id);
                        var body=root.Find("Body");Check(body!=null,"body pivot");
                        Check(body.Find("LeftArm")!=null&&body.Find("RightLeg")!=null,"articulated limbs");
                        string prop=card.Id=="ED261007-03-C001"?"Trumpet bell":card.Id=="ED261007-03-C002"?"Carried firewood":card.Id=="ED261007-03-C003"?"Iron cooking pan":card.Id=="ED261007-03-C004"?"Oversized helmet":card.Id=="ED261007-03-C005"?"Caravan meat sack":null;
                        if(prop!=null)Check(root.GetComponentsInChildren<Transform>().Any(t=>t.name==prop),"individual approved card prop");
                        Check(root.GetComponentsInChildren<Collider>().Length==0,"no new picking colliders");
                        Check(root.GetComponentsInChildren<MeshFilter>().All(m=>m.sharedMesh!=null&&m.sharedMesh.vertexCount>100),"smooth reusable meshes");
                        var motion=root.gameObject.AddComponent<FigureMotion>();motion.Initialize(card.Art,23);
                        Vector3 position=root.position;Quaternion rotation=root.rotation;
                        var hand=body.Find("RightArm");Quaternion neutral=hand.localRotation;float neutralHandZ=hand.Find("Hand").position.z;
                        foreach(var state in new[]{MotionState.Idle,MotionState.Walking,MotionState.Attacking,MotionState.Dying,MotionState.Celebrating})
                        {
                            motion.EvaluatePose(2.2f,state==MotionState.Walking?1:0,state,.35f);
                            Check(root.position==position&&root.rotation==rotation,"pose preserves root "+state);
                            var pose=hand.localRotation;motion.EvaluatePose(2.2f,state==MotionState.Walking?1:0,state,.35f);
                            Check(Quaternion.Angle(pose,hand.localRotation)<.001f,"absolute repeatable pose "+state);
                        }
                        motion.EvaluatePose(2.2f,0,MotionState.Attacking,.35f);Check(Quaternion.Angle(neutral,hand.localRotation)>5,"attack has visible motion");
                        Check(hand.Find("Hand").position.z<neutralHandZ,"attack reaches facing -Z");
                        motion.EvaluatePose(2.2f,0,MotionState.Idle,0);
                        Render(root,Path.Combine(folder,card.Id+".png"));Check(File.Exists(Path.Combine(folder,card.Id+".png")),"render artifact");
                    }
                    finally{UnityEngine.Object.DestroyImmediate(root.gameObject);}
                }
                File.WriteAllText(Path.Combine(folder,"result.txt"),"PASS "+checks+" checks; "+selection.Length+" profiles. Rendered from Unity; manual play/FPS not covered.");
                Debug.Log("TROOP PRESENTATION PASSED: "+checks+" checks, "+selection.Length+" profiles");
            }
            finally{UnityEngine.Object.DestroyImmediate(holder);}
        }
        static void Render(Transform root,string output)
        {
            foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=29;
            var cameraObject=new GameObject("Troop render camera");var cam=cameraObject.AddComponent<Camera>();
            var lightObject=new GameObject("Troop render key");var light=lightObject.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.4f;light.cullingMask=1<<29;light.transform.rotation=Quaternion.Euler(35,-30,0);
            var texture=new RenderTexture(384,512,24);var image=new Texture2D(384,512,TextureFormat.RGB24,false);var previous=RenderTexture.active;
            try
            {
                cam.cullingMask=1<<29;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.15f,.18f,.22f);
                cam.orthographic=true;cam.orthographicSize=.52f;cam.transform.position=new Vector3(1.15f,.9f,-1.8f);cam.transform.LookAt(new Vector3(0,.41f,0));
                cam.targetTexture=texture;cam.Render();RenderTexture.active=texture;image.ReadPixels(new Rect(0,0,384,512),0,0);image.Apply();File.WriteAllBytes(output,image.EncodeToPNG());
            }
            finally{RenderTexture.active=previous;cam.targetTexture=null;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(cameraObject);UnityEngine.Object.DestroyImmediate(lightObject);}
        }
    }
}
