using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TCG.Table
{
    public sealed partial class TableWorld
    {
        sealed class MovementRay
        {
            public LineRenderer Beam, Pulse, Ring;
            public TextMesh Number;
            public Vector3 From, To;
        }
        readonly List<MovementRay> movementRays=new List<MovementRay>();
        Material movementMaterial;
        int lightPiece=-1,lightRevision=-1;
        public int MovementLightCount {get;private set;}

        public void ShowMovement(int piece,bool visible)
        {
            if(!visible||match==null)piece=-1;
            if(lightPiece==piece&&lightRevision==(match?.Revision??-1))return;
            lightPiece=piece;lightRevision=match?.Revision??-1;MovementLightCount=0;
            if(piece>=0&&match.Find(piece)!=null)
            {
                var from=Position(match.Position(piece))+Vector3.up*.18f;
                for(int tile=0;tile<121;tile++)
                {
                    if(!match.CanMove(piece,tile))continue;
                    if(MovementLightCount==movementRays.Count)movementRays.Add(CreateMovementRay());
                    var ray=movementRays[MovementLightCount++];ConfigureMovementRay(ray,from,Position(tile)+Vector3.up*.18f);
                    SetMovementRayVisible(ray,true);ray.Number.text="1";
                }
            }
            for(int n=MovementLightCount;n<movementRays.Count;n++)
                SetMovementRayVisible(movementRays[n],false);
        }
        static void SetMovementRayVisible(MovementRay ray,bool visible)
        {ray.Beam.gameObject.SetActive(visible);ray.Pulse.gameObject.SetActive(visible);ray.Ring.gameObject.SetActive(visible);ray.Number.gameObject.SetActive(visible);}
        static void ConfigureMovementRay(MovementRay ray,Vector3 from,Vector3 to)
        {
            ray.From=from;ray.To=to;
            for(int n=0;n<25;n++)ray.Beam.SetPosition(n,MovementArc(ray,n/24f));
            for(int n=0;n<33;n++){float angle=n*Mathf.PI*2/32;ray.Ring.SetPosition(n,to+new Vector3(Mathf.Cos(angle)*.32f,.01f,Mathf.Sin(angle)*.32f));}
            ray.Number.transform.position=to+Vector3.up*.5f;
        }
        static Vector3 MovementArc(MovementRay ray,float t)
        {return Vector3.Lerp(ray.From,ray.To,t)+Vector3.up*(Mathf.Sin(t*Mathf.PI)*.28f);}
        MovementRay CreateMovementRay()
        {
            if(movementMaterial==null)
            {
                movementMaterial=new Material(Resources.Load<Shader>("MovementGlow"));movementMaterial.color=new Color(.2f,.95f,1,1);
                effectMaterials.Add(movementMaterial);
            }
            LineRenderer Line(string name,int count,float width)
            {
                var go=new GameObject(name);go.transform.SetParent(transform,false);var line=go.AddComponent<LineRenderer>();
                line.sharedMaterial=movementMaterial;line.useWorldSpace=true;line.positionCount=count;line.widthMultiplier=width;
                line.numCapVertices=4;line.numCornerVertices=3;line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;
                return line;
            }
            var label=new GameObject("Movimento • número da parada");label.transform.SetParent(transform,false);
            var number=label.AddComponent<TextMesh>();number.fontSize=48;number.characterSize=.12f;number.anchor=TextAnchor.MiddleCenter;number.alignment=TextAlignment.Center;number.fontStyle=FontStyle.Bold;number.color=Color.white;
            return new MovementRay{Beam=Line("Movimento • feixe",25,.09f),Pulse=Line("Movimento • luz viajante",5,.15f),Ring=Line("Movimento • destino",33,.09f),Number=number};
        }
        void TickMovementLights()
        {
            for(int n=0;n<MovementLightCount;n++)
            {
                AnimateMovementRay(movementRays[n],Mathf.Repeat(Time.unscaledTime*.7f,1));
            }
            TickOrderLights();
        }
        void AnimateMovementRay(MovementRay ray,float t)
        {
            for(int k=0;k<5;k++)ray.Pulse.SetPosition(k,MovementArc(ray,Mathf.Clamp01(t-k*.025f)));
            ray.Ring.widthMultiplier=.075f+.025f*(.5f+.5f*Mathf.Sin(Time.unscaledTime*4));
            ray.Number.transform.rotation=View.transform.rotation;
        }
    }
}
