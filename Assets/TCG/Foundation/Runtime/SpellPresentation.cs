using System;
using System.Linq;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
    public enum SpellVisualStage { Announcement, Resolution, Cancellation }
    public enum SpellVisualFamily { Radiance, Void, Tide, Flame, Gale, Growth, Arcane }
    // A presentation plan consumes public events only. It does not predict targets or execute effects.
    public static class SpellPresentation
    {
        public const int MaximumInstances=24;
        public static bool TryPlan(MatchEvent e,out SpellVisualStage stage,out int tile)
        {
            stage=SpellVisualStage.Announcement;tile=-1;
            if(e?.Card==null||(e.Card.Kind!=CardType.Instant&&e.Card.Kind!=CardType.Spell))return false;
            switch(e.Kind){case "cast":case "trigger":tile=e.From;break;
                case "resolve":case "impact":stage=SpellVisualStage.Resolution;tile=e.To;break;
                case "stack-canceled":stage=SpellVisualStage.Cancellation;tile=e.From;break;
                default:return false;}
            return tile>=0&&tile<121;
        }
        public static int[] Colors(Definition card)
        {
            var printed=Enumerable.Range(0,7).Where(c=>card.ColoredCost[c]>0).ToArray();
            return printed.Length>0?printed:CardFilter.CardColors(card).Where(c=>c>=0&&c<=6).DefaultIfEmpty(6).Distinct().Take(7).ToArray();
        }
        public static SpellVisualFamily Family(Definition card)
        {
            // Public operation identifiers select artwork only; no printed text or rules are evaluated.
            var abilities=NetworkCards.Pack(card).abilities;
            var operations=card.Effects.Select(e=>e.Operation).Concat(abilities.SelectMany(a=>a.ops).Select(o=>o.kind)).ToArray();
            bool Has(params string[] names)=>operations.Any(names.Contains);
            if(Has("heal","gainLife"))return SpellVisualFamily.Growth;
            if(Has("damage","damageSelf"))return SpellVisualFamily.Flame;
            if(Has("draw","scry","selectTop","orderTop","loot","mana","pcl-scry-other-spell","pcl-select-top","pcl-top-bottom"))return SpellVisualFamily.Arcane;
            if(Has("pcl-counter","grave","pcl-grave","noMove","noAttack")||abilities.SelectMany(a=>a.ops).Any(o=>o.kind=="buff"&&(o.a<0||o.b<0)))return SpellVisualFamily.Void;
            if(Has("shield","immune","buff"))return SpellVisualFamily.Radiance;
            if(Has("bounce","pcl-bounce","move","groupMove","pull"))return SpellVisualFamily.Tide;
            return (SpellVisualFamily)Colors(card)[0];
        }
        public static float Duration(SpellVisualStage stage)=>stage==SpellVisualStage.Announcement?1.05f:stage==SpellVisualStage.Cancellation?.75f:1.6f;
    }
    public sealed partial class TableWorld
    {
        Match spellPresentationMatch;
        public int SpellPresentationCount=>GetComponentsInChildren<SpellPresentationEffect>().Count(v=>v.gameObject.activeSelf);
        void TickSpellPresentation()
        {
            if(ReferenceEquals(spellPresentationMatch,match))return;
            foreach(var v in GetComponentsInChildren<SpellPresentationEffect>()){v.gameObject.SetActive(false);Destroy(v.gameObject);}
            spellPresentationMatch=match;
        }
        void PresentSpell(MatchEvent e)
        {
            TickSpellPresentation();
            if(!SpellPresentation.TryPlan(e,out var stage,out int tile))return;
            var active=GetComponentsInChildren<SpellPresentationEffect>();
            if(active.Length>=SpellPresentation.MaximumInstances){active[0].gameObject.SetActive(false);Destroy(active[0].gameObject);}
            var root=new GameObject("Magia · "+stage+" · "+e.Card.Name);root.transform.SetParent(transform,false);root.transform.position=Position(tile)+Vector3.up*.17f;
            root.AddComponent<SpellPresentationEffect>().Initialize(e.Card,stage);
        }
        public void PreviewSpellPresentation(Definition card,int tile,SpellVisualStage stage)
        {
            PresentSpell(new MatchEvent(stage==SpellVisualStage.Announcement?"cast":stage==SpellVisualStage.Cancellation?"stack-canceled":"impact",tile,tile,card:card));
        }
    }
    public sealed class SpellPresentationEffect:MonoBehaviour
    {
        const int RingPoints=65,Strands=18,StrandPoints=9;
        Material glow,moteMaterial;LineRenderer[] rings,strands;Light lamp;int[] colors;SpellVisualFamily family;SpellVisualStage stage;float born,duration;
        ParticleSystem motes;readonly ParticleSystem.Particle[] particles=new ParticleSystem.Particle[24];
        public void Initialize(Definition card,SpellVisualStage visualStage)
        {
            colors=SpellPresentation.Colors(card);family=SpellPresentation.Family(card);stage=visualStage;born=Time.unscaledTime;duration=SpellPresentation.Duration(stage);
            var shader=Resources.Load<Shader>("ArcaneGlow");if(shader==null){Destroy(gameObject);return;}
            glow=new Material(shader){color=Color.white};rings=new LineRenderer[3];strands=new LineRenderer[Strands];
            for(int i=0;i<rings.Length;i++)rings[i]=Line("Selo arcano",RingPoints,.03f+i*.008f);
            for(int i=0;i<strands.Length;i++)strands[i]=Line("Filamento · "+family,StrandPoints,.04f);
            var moteShader=Resources.Load<Shader>("SpellMote");
            if(moteShader!=null){moteMaterial=new Material(moteShader);var obj=new GameObject("Centelhas e brilho suave");obj.transform.SetParent(transform,false);motes=obj.AddComponent<ParticleSystem>();motes.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=motes.main;main.playOnAwake=false;main.loop=false;main.simulationSpace=ParticleSystemSimulationSpace.Local;main.maxParticles=24;var emission=motes.emission;emission.enabled=false;var shape=motes.shape;shape.enabled=false;var renderer=motes.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=moteMaterial;renderer.renderMode=ParticleSystemRenderMode.Billboard;motes.Play();}
            lamp=gameObject.AddComponent<Light>();lamp.range=2;lamp.shadows=LightShadows.None;Tick(0);
        }
        LineRenderer Line(string label,int count,float width)
        {
            var go=new GameObject(label);go.transform.SetParent(transform,false);var line=go.AddComponent<LineRenderer>();line.sharedMaterial=glow;line.useWorldSpace=false;line.positionCount=count;line.widthMultiplier=width;line.numCapVertices=3;return line;
        }
        void Update(){if(glow==null)return;float age=Time.unscaledTime-born;if(age>=duration){gameObject.SetActive(false);Destroy(gameObject);return;}Tick(age/duration);}
        void Tint(LineRenderer line,int index,float opacity)
        {
            var color=Color.Lerp(TableWorld.Elements[colors[index%colors.Length]],Color.white,.22f);color.a=opacity;line.startColor=color;color.a*=.25f;line.endColor=color;
        }
        void Tick(float t)
        {
            float fade=Mathf.Sin(Mathf.PI*t),rotation=t*duration;bool announce=stage==SpellVisualStage.Announcement,cancel=stage==SpellVisualStage.Cancellation;
            float radius=cancel?.52f*(1-t):announce?.22f+.24f*t:.12f+.65f*Mathf.Sqrt(t);
            lamp.color=TableWorld.Elements[colors[0]];lamp.intensity=fade*(announce?.6f:cancel?.35f:1.5f);
            if(motes!=null){for(int i=0;i<particles.Length;i++){
                float phase=Mathf.Repeat(t+i*.071f,1),a=i*2.39996f+rotation*(family==SpellVisualFamily.Void?-2:2);
                float r=radius*(.2f+phase),height=cancel?-.08f:phase*(announce?.65f:1.05f);
                particles[i].position=new Vector3(Mathf.Cos(a)*r,height,Mathf.Sin(a)*r);
                var c=Color.Lerp(TableWorld.Elements[colors[i%colors.Length]],Color.white,.2f);c.a=fade*(1-phase)*(cancel?.45f:.95f);
                particles[i].startColor=c;particles[i].startSize=(i%6==0?.35f:.12f)*(1-phase*.5f);particles[i].startLifetime=duration+1;particles[i].remainingLifetime=duration+1;
            }motes.SetParticles(particles,particles.Length);}
            for(int i=0;i<rings.Length;i++){
                Tint(rings[i],i,fade*.8f);
                for(int k=0;k<RingPoints;k++){
                    float a=k*Mathf.PI*2/(RingPoints-1)+rotation*(i%2==0?1:-1);
                    float r=radius*(1-i*.19f),y=.04f+i*.065f;
                    if(family==SpellVisualFamily.Tide)y+=Mathf.Sin(a*4+rotation*5)*.055f;
                    if(family==SpellVisualFamily.Gale){r*=.94f+.06f*Mathf.Sin(a*2+rotation*5);y+=Mathf.Sin(a*2+rotation*5)*.045f;}
                    if(cancel)y+=t*.22f;
                    rings[i].SetPosition(k,new Vector3(Mathf.Cos(a)*r,y,Mathf.Sin(a)*r));
                }
            }
            for(int i=0;i<strands.Length;i++){
                Tint(strands[i],i,fade*(cancel?.6f:.9f));float a=i*Mathf.PI*2/strands.Length;
                for(int k=0;k<StrandPoints;k++){
                    float s=k/(float)(StrandPoints-1),r=radius*(.3f+s),height=(announce?.5f:.95f)*s*(1-t*.45f);
                    float twist=family==SpellVisualFamily.Void?-s*3:family==SpellVisualFamily.Tide?s*2:family==SpellVisualFamily.Growth?Mathf.Sin(s*4+t*5)*.22f:0;
                    if(family==SpellVisualFamily.Flame){r+=Mathf.Sin(s*12+i+t*12)*.08f;height*=1.25f;}
                    if(family==SpellVisualFamily.Gale){height*=.65f;twist=s*5-rotation*4;r*=.65f+.35f*s;}
                    if(family==SpellVisualFamily.Radiance){r*=.7f;height*=1.45f;}
                    if(family==SpellVisualFamily.Arcane){twist=s*4+rotation;height*=.8f;}
                    if(cancel){height=-s*.13f;r*=1-t;}
                    strands[i].SetPosition(k,new Vector3(Mathf.Cos(a+twist)*r,height,Mathf.Sin(a+twist)*r));
                }
            }
        }
        void OnDestroy(){if(glow!=null)Destroy(glow);if(moteMaterial!=null)Destroy(moteMaterial);}
    }
}
