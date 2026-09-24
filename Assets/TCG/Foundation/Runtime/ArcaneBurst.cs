using UnityEngine;
namespace TCG.Table
{
 public sealed class ArcaneBurst:MonoBehaviour
 {
  LineRenderer[] rings,motes;Material glow;Light lamp;float born,duration;Color tint;bool foil;int element;
  public void Initialize(Color color,int colorIndex,bool isFoil,bool announcement)
  {
   tint=color;element=colorIndex;foil=isFoil;born=Time.unscaledTime;duration=foil?1.8f:announcement?1.3f:1.65f;
   glow=new Material(Resources.Load<Shader>("ArcaneGlow"));glow.color=Color.white;
   rings=new LineRenderer[3];for(int n=0;n<3;n++)rings[n]=Line("Círculo rúnico",65,.014f+n*.006f);
   motes=new LineRenderer[foil?24:16];for(int n=0;n<motes.Length;n++)motes[n]=Line("Centelha arcana",3,.024f);
   lamp=gameObject.AddComponent<Light>();lamp.color=color;lamp.range=foil?2.4f:1.8f;lamp.shadows=LightShadows.None;
   Update();
  }
  LineRenderer Line(string name,int count,float width)
  {
   var obj=new GameObject(name);obj.transform.SetParent(transform,false);var line=obj.AddComponent<LineRenderer>();line.sharedMaterial=glow;line.useWorldSpace=false;line.positionCount=count;line.widthMultiplier=width;line.numCapVertices=3;return line;
  }
  void Update()
  {
   if(glow==null)return;float age=Time.unscaledTime-born,t=age/duration;if(t>=1){Destroy(gameObject);return;}
   float fade=Mathf.Sin(Mathf.PI*t),expand=.25f+.65f*Mathf.Sin(t*Mathf.PI*.7f);lamp.intensity=fade*(foil?2.3f:1.4f);
   for(int n=0;n<rings.Length;n++){
    Color c=foil?Color.Lerp(new Color(1,.83f,.35f),Color.HSVToRGB(Mathf.Repeat(t+n*.22f,1),.42f,1),.55f):Color.Lerp(tint,Color.white,n*.18f);
    c.a=fade*.85f;rings[n].startColor=rings[n].endColor=c;
    for(int k=0;k<65;k++){float a=k*Mathf.PI*2/64+age*(n%2==0?1:-1),r=expand*(1-n*.2f);float y=.04f+n*.07f+(foil?.035f*Mathf.Sin(a*6+age*4):0);rings[n].SetPosition(k,new Vector3(Mathf.Cos(a)*r,y,Mathf.Sin(a)*r));}
   }
   for(int n=0;n<motes.Length;n++){
    float a=n*Mathf.PI*2/motes.Length+age*(element==2||element==4?2:.6f),phase=Mathf.Repeat(t+n*.071f,1);
    float radius=foil?.15f+phase*.6f:element==3?.5f*(1-phase):.12f+phase*.72f;
    float height=foil?phase*1.4f:element==5?.1f+Mathf.Sin(phase*Mathf.PI)*.45f:phase*.95f;
    var p=new Vector3(Mathf.Cos(a)*radius,height,Mathf.Sin(a)*radius);
    motes[n].SetPosition(0,p);motes[n].SetPosition(1,p-Vector3.up*.045f);motes[n].SetPosition(2,p-Vector3.up*.12f);
    Color c=foil?Color.HSVToRGB(Mathf.Repeat(n*.075f+t*.2f,1),.3f,1):Color.Lerp(tint,Color.white,.5f);c.a=fade*(1-phase);motes[n].startColor=c;c.a=0;motes[n].endColor=c;
   }
  }
  void OnDestroy(){if(glow!=null)Destroy(glow);}
 }
}
