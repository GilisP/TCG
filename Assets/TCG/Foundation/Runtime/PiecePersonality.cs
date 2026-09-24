using UnityEngine;
namespace TCG.Table
{
    // Pure presentation layout: no occupancy limit or game rules are changed.
    public static class PieceFormation
    {
        public const int VisibleLimit=9;
        public static Vector3 Offset(int index,int count)
        {
            int visible=Mathf.Clamp(count,1,VisibleLimit),cols=Mathf.CeilToInt(Mathf.Sqrt(visible)),rows=Mathf.CeilToInt(visible/(float)cols);
            int row=index/cols,inRow=Mathf.Min(cols,visible-row*cols);
            float step=.72f/cols;
            return new Vector3((index%cols-(inRow-1)*.5f)*step,.16f,(row-(rows-1)*.5f)*step);
        }
        public static float Scale(int count)=>count<=1?.85f:.85f/Mathf.CeilToInt(Mathf.Sqrt(Mathf.Min(count,VisibleLimit)));
    }
    // Limb animation is local to the figure; the board root remains authoritative for picking/routes.
    public sealed class FigureMotion:MonoBehaviour
    {
        Transform[] parts; Vector3[] positions; Quaternion[] rotations; string shape; float seed; Vector3 previous;
        public void Initialize(string profile,int id)
        {
            shape=profile;seed=id*.73f;parts=GetComponentsInChildren<Transform>();positions=new Vector3[parts.Length];rotations=new Quaternion[parts.Length];
            for(int i=0;i<parts.Length;i++){positions[i]=parts[i].localPosition;rotations[i]=parts[i].localRotation;}previous=transform.position;
        }
        void LateUpdate()
        {
            if(parts==null)return;
            float speed=Mathf.Clamp01(Vector3.Distance(transform.position,previous)/Mathf.Max(.001f,Time.unscaledDeltaTime)*.5f);previous=transform.position;
            float time=Time.unscaledTime,breath=Mathf.Sin(time*1.8f+seed),stride=Mathf.Sin(time*13+seed)*speed;
            for(int i=1;i<parts.Length;i++)
            {
                var t=parts[i];string n=t.name;var p=positions[i];var r=rotations[i];
                if(n=="Bota"||n=="Pata") {p.y+=Mathf.Max(0,stride*Mathf.Sign(p.x+p.z*.2f))*.045f;r*=Quaternion.Euler(stride*Mathf.Sign(p.x)*22,0,0);}
                else if(n=="Asa")r*=Quaternion.Euler(0,0,Mathf.Sin(time*4+seed)*Mathf.Sign(p.x)*14);
                else if(n.Contains("Cabeça")||n=="Cabeça"||n=="Focinho")r*=Quaternion.Euler(breath*2,Mathf.Sin(time*.8f+seed)*4,0);
                else if(n=="Capa"||n=="Faixa")r*=Quaternion.Euler(breath*4+speed*8,0,breath*2);
                else if(n=="Cajado"||n=="Arco"||n=="Espada"||n=="Braço")r*=Quaternion.Euler(stride*9,0,breath*2);
                else if(n=="Tronco"||n=="Corpo"||n=="Corpo alado")p.y+=breath*.008f;
                t.localPosition=p;t.localRotation=r;
            }
        }
    }
}
