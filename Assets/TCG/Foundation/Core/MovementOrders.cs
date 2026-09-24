using System;
using System.Collections.Generic;
using System.Linq;

namespace TCG.Foundation
{
    public enum MovementOrderState { Scheduled, Moving, WaitingForTerrain, WaitingForPath, WaitingForMovement }
    public sealed class MovementOrder
    {
        public int Piece {get;} public int Owner {get;} public int Destination {get;}
        public MovementOrderState State {get;internal set;}=MovementOrderState.Scheduled;
        internal MovementOrder(int piece,int owner,int destination){Piece=piece;Owner=owner;Destination=destination;}
    }
    public sealed partial class Match
    {
        readonly Dictionary<int,MovementOrder> movementOrders=new Dictionary<int,MovementOrder>();
        readonly Queue<int> movementWork=new Queue<int>();
        public bool AutoAdvanceAfterTerrain {get;set;}
        public MovementOrder OrderFor(int piece)=>movementOrders.TryGetValue(piece,out var order)&&Find(piece)!=null?order:null;
        public IReadOnlyList<MovementOrder> OrdersFor(int owner)=>movementOrders.Values.Where(o=>o.Owner==owner&&Find(o.Piece)!=null).OrderBy(o=>o.Piece).ToArray();
        public bool CanOrderMovement(int id)
        {if(IsRemoteView)return NetCan("order",id);
            var p=Find(id);
            return !Over&&Choice==null&&Defense==null&&stack.Count==0&&Controller==Active&&p!=null&&p.Owner==Active&&Mobile(p)&&p.Card.Movement>0&&Phase!=Stage.End;
        }
        public IReadOnlyList<int> MovementRoute(int id,int target)
        {if(IsRemoteView)return remoteRoutes.TryGetValue(id+":"+target,out var routeView)?routeView:Array.Empty<int>();
            var p=Find(id);if(p==null||!Valid(target))return Array.Empty<int>();
            var route=FindMovementRoute(p,target,false);
            return route.Length>0?route:FindMovementRoute(p,target,true);
        }
        int[] FindMovementRoute(Piece p,int target,bool allowHoles)
        {
            int start=Position(p.Id);if(start==target)return Array.Empty<int>();
            var parent=new Dictionary<int,int>{{start,-1}};var pending=new Queue<int>();pending.Enqueue(start);
            while(pending.Count>0)
            {
                int at=pending.Dequeue();
                foreach(int next in MovementNeighbors(p,at))
                {
                    if(parent.ContainsKey(next)||(!allowHoles&&cells[next].Terrain==null)||Blocked(p,next)||Enemies(p.Owner,cells[next].CapitalOwner)||cells[next].pieces.Any(q=>q.Card.Kind!=CardType.Equipment&&Enemies(p.Owner,q.Owner)))continue;
                    parent.Add(next,at);
                    if(next==target)
                    {
                        var route=new List<int>();for(int n=target;n!=start;n=parent[n])route.Add(n);route.Reverse();return route.ToArray();
                    }
                    pending.Enqueue(next);
                }
            }
            return Array.Empty<int>();
        }
        void PlanMovement(int id,int target)
        {
            Check(CanOrderMovement(id),"Selecione uma criatura sua durante seu turno, sem escolhas ou pilha pendentes.");
            Check(Valid(target)&&Position(id)!=target,"Escolha outro tile do tabuleiro.");
            var p=Find(id);movementOrders[id]=new MovementOrder(id,p.Owner,target);
            if(Phase==Stage.Main&&!movementWork.Contains(id))movementWork.Enqueue(id);
            Note(p.Card.Name+": movimento programado até "+target%11+","+target/11+". Pode cancelar antes da fase principal.");
        }
        void CancelMovement(int id)
        {
            if(id<0)
            {
                foreach(int key in movementOrders.Where(kv=>kv.Value.Owner==Controller).Select(kv=>kv.Key).ToArray())movementOrders.Remove(key);
            }
            else
            {
                Check(movementOrders.TryGetValue(id,out var order)&&order.Owner==Controller,"Essa rota não pertence a você.");movementOrders.Remove(id);
            }
            Note("Movimento programado cancelado.");
        }
        void QueueMovementOrders()
        {
            foreach(var order in OrdersFor(Active))if(!movementWork.Contains(order.Piece))movementWork.Enqueue(order.Piece);
        }
        void AdvanceMovementOrders()
        {
            foreach(int id in movementOrders.Keys.Where(id=>Find(id)==null).ToArray())movementOrders.Remove(id);
            while(!Over&&Phase==Stage.Main&&Priority==Active&&Choice==null&&Defense==null&&stack.Count==0&&movementWork.Count>0)
            {
                int id=movementWork.Peek();var p=Find(id);
                if(!movementOrders.TryGetValue(id,out var order)||p==null||p.Owner!=Active){movementWork.Dequeue();continue;}
                if(Position(id)==order.Destination){movementOrders.Remove(id);movementWork.Dequeue();continue;}
                if(p.Movement<=0){order.State=MovementOrderState.WaitingForMovement;movementWork.Dequeue();continue;}
                int next=order.Destination;
                if(!CanMove(id,next))
                {
                    var route=MovementRoute(id,order.Destination);
                    if(route.Count==0){order.State=MovementOrderState.WaitingForPath;movementWork.Dequeue();continue;}
                    next=route[0];
                    if(!CanMove(id,next))
                    {
                        order.State=cells[next].Terrain==null?MovementOrderState.WaitingForTerrain:MovementOrderState.WaitingForPath;
                        movementWork.Dequeue();continue;
                    }
                }
                order.State=MovementOrderState.Moving;
                Move(id,next);Drain();StateCheck();FinishCheck();AdvanceAutomaticResponses();
                // Triggers may interrupt the route; resume only after the player's choices resolve.
            }
        }
    }
}

