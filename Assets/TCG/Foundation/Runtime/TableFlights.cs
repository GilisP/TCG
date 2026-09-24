using System.Linq;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
    public sealed partial class TableWorld
    {
        public int FlightCount=>GetComponentsInChildren<TableFlight>().Length;
        void ClearFlights(){foreach(var f in GetComponentsInChildren<TableFlight>())Destroy(f.gameObject);}
        void ResourceCombatVisual(MatchEvent e)
        {
            if(e.Kind!="mana"&&e.Kind!="attack")return;
            if(e.From<0||e.To<0||e.From>=121||e.To>=121)return;
            var active=GetComponentsInChildren<TableFlight>();
            if(active.Length>=48){active[0].gameObject.SetActive(false);Destroy(active[0].gameObject);}
            string form="mana";int color=e.Color;
            if(e.Kind=="attack")
            {
                color=e.Card?.Color??6;
                form=e.Card?.Art=="mage"||e.Card?.Subtypes.Any(s=>s.ToLowerInvariant().Contains("mago"))==true?"fireball":e.Card?.Range>1?"arrow":"slash";
            }
            var obj=new GameObject(form=="mana"?"Mana → capital":form=="arrow"?"Flecha":form=="slash"?"Corte":"Projétil mágico");obj.transform.SetParent(transform,false);
            obj.AddComponent<TableFlight>().Initialize(Position(e.From)+Vector3.up*.55f,Position(e.To)+Vector3.up*.6f,Elements[Mathf.Clamp(color,0,6)],form);
        }
    }
    // Presentation only: damage and mana are already authoritative when this runs.
    public sealed class TableFlight:MonoBehaviour
    {
        Material material; LineRenderer trail,shape; Light lamp; Transform orb;
        Vector3 from,to; Color tint; string form; float born,duration;
        public void Initialize(Vector3 start,Vector3 end,Color color,string kind)
        {
            from=start;to=end;tint=color;form=kind;born=Time.unscaledTime;duration=kind=="mana"?1.1f:kind=="slash"?.65f:.8f;
            material=new Material(Resources.Load<Shader>("ArcaneGlow")){color=Color.white};
            trail=Line("Rastro luminoso",24,.045f);shape=Line("Traço",kind=="slash"?25:5,kind=="slash"?.09f:.035f);
            if(kind=="mana"||kind=="fireball")
            {
                var sphere=GameObject.CreatePrimitive(PrimitiveType.Sphere);sphere.name="Núcleo brilhante";sphere.transform.SetParent(transform,false);Destroy(sphere.GetComponent<Collider>());orb=sphere.transform;orb.localScale=Vector3.one*(kind=="mana"?.17f:.24f);sphere.GetComponent<Renderer>().sharedMaterial=material;
                material.color=color;
            }
            lamp=gameObject.AddComponent<Light>();lamp.color=color;lamp.range=1.6f;lamp.shadows=LightShadows.None;Tick(0);
        }
        LineRenderer Line(string label,int count,float width)
        {
            var go=new GameObject(label);go.transform.SetParent(transform,false);var line=go.AddComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=true;line.positionCount=count;line.widthMultiplier=width;line.numCapVertices=4;return line;
        }
        Vector3 Point(float t)=>Vector3.Lerp(from,to,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*(form=="mana"?1.5f:.55f);
        void Update(){float t=(Time.unscaledTime-born)/duration;if(t>=1){Destroy(gameObject);return;}Tick(t);}
        void Tick(float t)
        {
            float fade=Mathf.Min(1,(1-t)*5);Vector3 head=Point(t);transform.position=head;lamp.intensity=fade*1.8f;
            Color bright=Color.Lerp(tint,Color.white,.55f);bright.a=fade;trail.startColor=new Color(tint.r,tint.g,tint.b,0);trail.endColor=bright;shape.startColor=shape.endColor=bright;
            for(int n=0;n<24;n++)trail.SetPosition(n,Point(Mathf.Max(0,t-.24f+n*.24f/23)));
            if(form=="slash")
            {
                trail.enabled=false;Vector3 axis=(to-from).normalized,side=Vector3.Cross(Vector3.up,axis);if(side.sqrMagnitude<.01f)side=Vector3.right;
                for(int n=0;n<25;n++){float a=Mathf.Lerp(-1.3f,1.3f,n/24f)+t*2;shape.SetPosition(n,to+side*Mathf.Sin(a)*.65f+Vector3.up*Mathf.Cos(a)*.6f);}
            }
            else if(form=="arrow")
            {
                Vector3 dir=(Point(Mathf.Min(1,t+.02f))-Point(Mathf.Max(0,t-.02f))).normalized,side=Vector3.Cross(dir,Vector3.up).normalized;
                shape.SetPositions(new[]{head-dir*.5f,head,head-dir*.18f+side*.1f,head,head-dir*.18f-side*.1f});
            }
            else
            {
                for(int n=0;n<5;n++){float a=n*Mathf.PI*.5f+t*10;shape.SetPosition(n,head+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*.19f);}
                if(orb!=null)orb.localScale=Vector3.one*(form=="mana"?.17f:.24f)*(1+.15f*Mathf.Sin(t*22));
            }
        }
        void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
