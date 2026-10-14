using System;
using System.Linq;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
 public sealed class CommanderShowcase:MonoBehaviour
 {
  TableWorld world;Definition[] cards;Transform model;FigureMotion motion;int index;MotionState state=MotionState.Idle;float stateStart;Texture2D art;string error;
  void Start()
  {
   try
   {
    var catalog=ContentLoader.Load(new EffectRegistry());cards=catalog.Cards.Where(c=>c.IsCommander).OrderBy(c=>c.Id).ToArray();
    world=new GameObject("Commander showroom presentation").AddComponent<TableWorld>();world.BeginCommanderPreview();
    RenderSettings.fog=false;RenderSettings.ambientLight=TableWorld.Hex("78818A");
    var camera=new GameObject("Showroom camera").AddComponent<Camera>();camera.transform.SetParent(transform);camera.transform.position=new Vector3(-1.7f,1.35f,-2.8f);camera.transform.LookAt(new Vector3(0,.42f,0));camera.orthographic=true;camera.orthographicSize=.8f;camera.rect=new Rect(0,.04f,.64f,.82f);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=TableWorld.Hex("15252C");
    var lamp=new GameObject("Studio key").AddComponent<Light>();lamp.transform.SetParent(transform);lamp.type=LightType.Directional;lamp.color=TableWorld.Hex("FFE8CB");lamp.intensity=1.2f;lamp.transform.rotation=Quaternion.Euler(45,-40,0);lamp.shadows=LightShadows.Soft;
    Select(0);
   }
   catch(Exception e){error=e.Message;Debug.LogException(e);}
  }
  void Select(int next)
  {
   if(model!=null)Destroy(model.gameObject);index=(next+cards.Length)%cards.Length;model=world.CreateCommanderModel(cards[index],TableWorld.Seats[0],world.transform);motion=model.gameObject.AddComponent<FigureMotion>();motion.Initialize(cards[index].Art,index+1);motion.enabled=false;
   art=Resources.Load<Texture2D>("CardArt/Commanders/"+cards[index].Id+"-portrait-v1");stateStart=Time.unscaledTime;
  }
  void Update()
  {
   if(motion==null)return;float age=Time.unscaledTime-stateStart;float cycle=state==MotionState.Attacking?Mathf.Repeat(age,FigureMotion.AttackDuration+1):state==MotionState.Dying?Mathf.Repeat(age,FigureMotion.DeathDuration+1.5f):age;
   motion.EvaluatePose(age,state==MotionState.Walking?1:0,state,cycle);model.rotation=Quaternion.Euler(0,Mathf.Sin(age*.25f)*15,0);
  }
  void OnGUI()
  {
   float scale=Mathf.Min(Screen.width/1280f,Screen.height/800f);var old=GUI.matrix;GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
   GUI.Box(new Rect(15,15,1250,95),"");var title=new GUIStyle(GUI.skin.label){fontSize=23,fontStyle=FontStyle.Bold,wordWrap=true};GUI.Label(new Rect(35,25,1200,35),"GALERIA DOS COMANDANTES",title);
   if(error!=null){GUI.Label(new Rect(35,70,1200,100),error);GUI.matrix=old;return;}
   if(cards!=null)
   {
    if(GUI.Button(new Rect(35,68,100,32),"Anterior"))Select(index-1);if(GUI.Button(new Rect(145,68,100,32),"Próximo"))Select(index+1);GUI.Label(new Rect(265,70,900,32),(index+1)+" / "+cards.Length+" — "+cards[index].Name);
    string[] names={"Esperar","Caminhar","Atacar","Morrer","Comemorar"};for(int n=0;n<names.Length;n++)if(GUI.Button(new Rect(35+n*150,725,140,42),(int)state==n?"● "+names[n]:names[n])){state=(MotionState)n;stateStart=Time.unscaledTime;}
    GUI.Box(new Rect(835,140,415,560),"");GUI.Label(new Rect(855,158,375,65),cards[index].Name,title);
    if(art!=null)GUI.DrawTexture(new Rect(855,235,375,280),art,ScaleMode.ScaleToFit);GUI.Label(new Rect(855,535,375,125),@"Modelo e ilustração individuais.

Use os botões para comparar as cinco animações. Esta galeria não altera a coleção nem inicia uma partida.");
   }
   GUI.matrix=old;
  }
 }
}
