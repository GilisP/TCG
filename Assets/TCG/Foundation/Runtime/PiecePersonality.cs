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
    public enum MotionState { Idle, Walking, Attacking, Dying, Celebrating }
    public sealed class FigureMotion:MonoBehaviour
    {
        public const float AttackDuration=.7f, DeathDuration=1.15f;
        Transform[] parts; Vector3[] positions; Quaternion[] rotations; float seed,started; bool quadruped; Vector3 previous;
        public MotionState State { get; private set; }
        public void Initialize(string profile,int id)
        {
            quadruped=false;State=MotionState.Idle;started=Time.unscaledTime;seed=id*.73f;parts=GetComponentsInChildren<Transform>();positions=new Vector3[parts.Length];rotations=new Quaternion[parts.Length];
            for(int i=0;i<parts.Length;i++){if(parts[i].name=="FrontLeftLeg")quadruped=true;positions[i]=parts[i].localPosition;rotations[i]=parts[i].localRotation;}previous=transform.position;
        }
        public void PlayAttack(){if(State==MotionState.Dying||State==MotionState.Celebrating)return;State=MotionState.Attacking;started=Time.unscaledTime;}
        public void PlayDeath(){if(State==MotionState.Dying)return;State=MotionState.Dying;started=Time.unscaledTime;}
        public void PlayCelebrate(){if(State==MotionState.Dying||State==MotionState.Celebrating)return;State=MotionState.Celebrating;started=Time.unscaledTime;}
        void LateUpdate()
        {
            float speed=Mathf.Clamp01(Vector3.Distance(transform.position,previous)/Mathf.Max(.001f,Time.unscaledDeltaTime)*.5f);previous=transform.position;
            float age=Time.unscaledTime-started;
            if(State==MotionState.Attacking&&age>=AttackDuration)State=MotionState.Idle;
            if(State==MotionState.Idle||State==MotionState.Walking)State=speed>.03f?MotionState.Walking:MotionState.Idle;
            EvaluatePose(Time.unscaledTime,speed,State,age);
        }
        // Absolute evaluation supports deterministic checks; the picking root is untouched.
        public void EvaluatePose(float time,float speed,MotionState state,float age)
        {
            if(parts==null)return;
            float breath=Mathf.Sin(time*1.8f+seed),stride=Mathf.Sin(time*13+seed)*Mathf.Clamp01(speed);
            float strike=state==MotionState.Attacking?Mathf.Sin(Mathf.Clamp01(age/AttackDuration)*Mathf.PI):0;
            float fall=state==MotionState.Dying?Mathf.SmoothStep(0,1,Mathf.Clamp01(age/DeathDuration)):0;
            if(state==MotionState.Dying){breath*=1-fall;stride=0;speed=0;} bool celebrate=state==MotionState.Celebrating;
            for(int i=1;i<parts.Length;i++)
            {
                var t=parts[i];if(t==null)continue;string n=t.name;var p=positions[i];var r=rotations[i];
                bool body=n=="Body"||n=="RiderBody"||n=="Tronco"||n=="Corpo"||n=="Corpo alado";
                bool head=n=="Head"||n=="RiderHead"||n.Contains("Cabeça")||n=="Focinho";
                bool leg=n.EndsWith("Leg")||n=="Bota"||n=="Pata";
                bool arm=n=="LeftArm"||n=="RightArm"||n=="RiderLeftArm"||n=="RiderRightArm"||n=="Braço"||n=="Cajado"||n=="Arco"||n=="Espada";
                float side=n.Contains("Left")?-1:n.Contains("Right")?1:Mathf.Sign(p.x);
                if(leg){float gait=stride*side*(n.StartsWith("Back")?-1:1);p.y+=Mathf.Max(0,gait)*.045f;r*=Quaternion.Euler(gait*25+(celebrate&&quadruped&&n.StartsWith("Front")?110:0),0,0);}
                else if(n=="Asa")r*=Quaternion.Euler(0,0,Mathf.Sin(time*4+seed)*side*14);
                else if(head)r*=Quaternion.Euler(breath*2-strike*9,Mathf.Sin(time*.8f+seed)*5,0);
                else if(n=="Cape"||n=="RiderCape"||n=="Capa"||n=="Faixa")r*=Quaternion.Euler(breath*4+speed*12,0,breath*2);
                else if(n=="Tail")r*=Quaternion.Euler(0,Mathf.Sin(time*2.1f+seed)*12,0);
                else if(arm)r*=Quaternion.Euler(-stride*side*14+strike*85+(celebrate?150+breath*8:0),strike*side*18,breath*3);
                else if(body){p.y+=breath*.009f+(celebrate?Mathf.Max(0,Mathf.Sin(time*4+seed))*.045f:0);p.z-=strike*.055f;r*=Quaternion.Euler(-strike*16+(celebrate&&quadruped&&n=="Body"?18:0),breath*2,0);}
                if(fall>0&&t.parent==transform&&n!="SeatEmblem"&&n!="OwnerPlate"&&n!="SeatMarker"&&n!="Base"&&n!="Owner plinth"&&!(n=="Forma low-poly"&&p.y<=.07f)){p.y-=fall*.11f;p.z+=fall*.12f;r*=quadruped?Quaternion.Euler(0,0,fall*88):Quaternion.Euler(fall*78,0,fall*12);}
                t.localPosition=p;t.localRotation=r;
            }
        }
    }
}