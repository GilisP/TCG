using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
    public sealed partial class Match
    {
        void AdvanceMissions()
        {
            foreach(int id in missions.Keys.ToArray())
            {
                var p=Find(id);if(p==null){missions.Remove(id);continue;}if(p.Owner!=Active||seats[missions[id]].Eliminated)continue;
                int guard=0;
                while(!Over&&Phase==Stage.Main&&Priority==Active&&stack.Count==0&&Choice==null&&p.Movement>0&&guard++<30)
                {
                    int target=Capital(missions[id],seats.Length);int step=MissionStep(p,target,false);if(step<0)step=MissionStep(p,target,true);if(step<0)break;
                    if(cells[step].Terrain==null)break;
                    if(CanAttack(id,step)){Attack(new[]{id},step);break;}
                    if(!CanMove(id,step))break;Move(id,step);Drain();StateCheck();FinishCheck();AdvanceAutomaticResponses();if(Find(id)==null)break;
                }
            }
        }
        int MissionStep(Piece p,int target,bool holes)
        {
            int start=Position(p.Id);if(start==target)return -1;var parents=new Dictionary<int,int>{{start,-1}};var queue=new Queue<int>();queue.Enqueue(start);
            while(queue.Count>0&&!parents.ContainsKey(target)){int at=queue.Dequeue();foreach(int next in MovementNeighbors(p,at))if(!parents.ContainsKey(next)&&!Blocked(p,next)&&(holes||cells[next].Terrain!=null)){parents[next]=at;queue.Enqueue(next);}}
            if(!parents.ContainsKey(target))return -1;int step=target;while(parents[step]!=start&&parents[step]>=0)step=parents[step];return step;
        }
        void ReflectDeclaredTarget(Pending item)
        {
            var target=Find(item.TargetUnit);if(item.Card?.Kind!=CardType.Instant||target==null||!Enemies(item.Owner,target.Owner))return;
            var shield=All.FirstOrDefault(e=>e.AttachedTo==target.Id&&AuthorHas(e,"mirror-shield")&&e.OnceTurn!=Turn);if(shield==null)return;shield.OnceTurn=Turn;
            var other=All.Where(p=>p!=target&&p.Card.Kind==CardType.Creature&&Enemies(item.Owner,p.Owner)).ToArray();if(other.Length==0)return;
            Ask(target.Owner,"Escudo Espelhado: redirecionar Truque",other.Select(p=>new ChoiceOption(p.Id,p.Card.Name)),id=>{var p=Find(id);if(p!=null){item.TargetUnit=id;item.Target=Position(id);}});
        }        void ApplyDisplacement(int owner,Piece p,int destination,Action<int> impact)
        {
            if(Find(p.Id)==null||cells[destination].Terrain==null||Blocked(p,destination))return;
            int from=Position(p.Id);cells[from].pieces.Remove(p);cells[destination].pieces.Add(p);MarkMoved(p,from,destination);impact?.Invoke(destination);Visual?.Invoke(new MatchEvent("move",from,destination,p.Id));
        }
        void RedirectDisplacement(int owner,Piece p,int destination,HashSet<int> reached,Action<int> impact)
        {
            var beacons=All.Where(q=>AuthorHas(q,"lighthouse")&&q.Owner==p.Owner&&reached.Contains(Position(q.Id))).Select(q=>Position(q.Id)).Where(n=>n!=destination).Distinct().ToArray();
            if(beacons.Length==0){ApplyDisplacement(owner,p,destination,impact);return;}
            Ask(p.Owner,"Farol: redirecionar o movimento?",new[]{new ChoiceOption(destination,"Destino original")}.Concat(beacons.Select(n=>new ChoiceOption(n,"Farol em "+n%11+","+n/11))),n=>ApplyDisplacement(owner,p,n,impact));
        }
        void ApplyChosenTarget(int caster,Piece original,Func<Piece,bool> valid,Action<Piece> apply)
        {
            if(resolvingVisual?.Kind==CardType.Instant&&Enemies(caster,original.Owner))
            {
                var shield=All.FirstOrDefault(e=>e.AttachedTo==original.Id&&AuthorHas(e,"mirror-shield")&&e.OnceTurn!=Turn);
                var alternatives=All.Where(p=>p!=original&&p.Card.Kind==CardType.Creature&&valid(p)).ToArray();
                if(shield!=null){shield.OnceTurn=Turn;if(alternatives.Length>0){Ask(original.Owner,"Escudo Espelhado: novo alvo válido",alternatives.Select(p=>new ChoiceOption(p.Id,p.Card.Name)),id=>{var p=Find(id);if(p!=null&&valid(p))apply(p);});return;}}
            }
            apply(original);
        }
    }
}
