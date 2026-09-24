using System.Collections.Generic;
using System.Linq;
using TCG.Foundation;
using UnityEngine;

namespace TCG.Table
{
    public sealed partial class TableWorld
    {
        Transform dragGhost;int ghostPiece=-1;
        readonly List<MovementRay> orderRays=new List<MovementRay>();int orderRayCount;
        int previewPiece=-1,previewTarget=-1,orderPiece=-1,orderTarget=-1,orderRevision=-1;
        public int MovementStopCount {get;private set;}
        public void PreviewMovementDrag(int piece,int target)
        {
            previewPiece=piece;previewTarget=target;
            if(piece<0||target<0||match?.Find(piece)==null)
            {if(dragGhost!=null)Release(dragGhost);dragGhost=null;ghostPiece=-1;return;}
            if(ghostPiece!=piece||dragGhost==null)
            {
                if(dragGhost!=null)Release(dragGhost);var p=match.Find(piece);dragGhost=Figure(p.Card,Hex("8FDADD"),transform);ghostPiece=piece;
                foreach(var t in dragGhost.GetComponentsInChildren<Transform>())t.gameObject.layer=2;
            }
            dragGhost.position=Position(target)+Vector3.up*.28f;
        }
        public void ShowMovementOrder(int selected,bool visible)
        {
            int piece=previewPiece>=0?previewPiece:selected;
            int target=previewPiece>=0?previewTarget:match?.OrderFor(selected)?.Destination??-1;
            if(!visible||piece<0||target<0||match?.Find(piece)==null)
            {
                foreach(var ray in orderRays)SetMovementRayVisible(ray,false);
                orderRayCount=MovementStopCount=0;orderRevision=-1;return;
            }
            if(orderPiece==piece&&orderTarget==target&&orderRevision==match.Revision)return;
            orderPiece=piece;orderTarget=target;orderRevision=match.Revision;
            var p=match.Find(piece);var route=match.MovementRoute(piece,target).ToArray();
            if(route.Length==0&&match.Position(piece)!=target)route=new[]{target};
            orderRayCount=route.Length;MovementStopCount=0;
            int perTurn=Mathf.Max(1,p.Card.Movement);var order=match.OrderFor(piece);
            bool waiting=order!=null&&(order.State==MovementOrderState.WaitingForTerrain||order.State==MovementOrderState.WaitingForPath||order.State==MovementOrderState.WaitingForMovement);
            int budget=match.Phase==Stage.Main&&!waiting&&p.Movement>0?p.Movement:perTurn;
            int stopAt=budget;var from=Position(match.Position(piece))+Vector3.up*.18f;
            for(int n=0;n<route.Length;n++)
            {
                if(n==orderRays.Count)orderRays.Add(CreateMovementRay());
                var ray=orderRays[n];var to=Position(route[n])+Vector3.up*.18f;
                ConfigureMovementRay(ray,from,to);SetMovementRayVisible(ray,true);
                bool stop=n+1>=stopAt||n==route.Length-1;
                ray.Ring.gameObject.SetActive(stop);ray.Number.gameObject.SetActive(stop);
                if(stop){ray.Number.text=(++MovementStopCount).ToString();stopAt=n+1+perTurn;}
                from=to;
            }
            for(int n=route.Length;n<orderRays.Count;n++)SetMovementRayVisible(orderRays[n],false);
        }
        void TickOrderLights()
        {
            if(orderRayCount==0)return;
            float progress=Mathf.Repeat(Time.unscaledTime*.7f,orderRayCount);int segment=Mathf.FloorToInt(progress);
            for(int n=0;n<orderRayCount;n++)
            {
                var ray=orderRays[n];ray.Pulse.gameObject.SetActive(n==segment);
                AnimateMovementRay(ray,n==segment?progress-segment:0);
            }
        }
    }
}