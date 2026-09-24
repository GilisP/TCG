using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
    public sealed partial class Match
    {
        public string CommanderMechanicStatus(int owner)
        {
            if(owner<0||owner>=seats.Length)return "";var s=seats[owner];
            switch(s.Commander?.Rule)
            {
                case "valeria":return "Veículos: +1 vaga e palavras-chave dos passageiros";
                case "royal-devotion":return "Devoção Lua "+Devotion(owner,1)+" · Sol "+Devotion(owner,0);
                case "lissandra":return "Contadores de morte: "+s.DeathCounters;
                case "dual-receptacle":
                    bool present=All.Any(p=>p.Owner==owner&&p.Card.Rule=="dual-receptacle");
                    return present?(s.mana[5]==s.mana[3]?"Lucio + Dazmond ativos":s.mana[5]>s.mana[3]?"Lucio: tropas +1 PA":"Dazmond: dano não criatura +2"):"Passivas ao entrar em campo";
                case "old-fisher":return FisherDestination(owner)<0?"Crie um terreno para pescar":"Destino: tile "+(FisherDestination(owner)%11)+","+(FisherDestination(owner)/11);
                case "mother-bear":return "Protege seu reino antes do combate";
                case "suicidal-inventor":return "Mortes aliadas: explosão em raio 1";
                default:return "";
            }
        }
        Piece[] combatDeathObservers;
        internal int Devotion(int owner,int color)=>All.Where(p=>p.Owner==owner).Sum(p=>p.Card.ColoredCost[color]);
        internal int BasePower(Piece p)=>p.Card.Rule=="royal-devotion"?Devotion(p.Owner,1):p.Card.Attack;
        internal int BaseToughness(Piece p)=>p.Card.Rule=="royal-devotion"?Devotion(p.Owner,0):p.Card.Defense;
        bool Lucio(Piece p)=>p.Card.Rule=="dual-receptacle"&&seats[p.Owner].mana[5]>=seats[p.Owner].mana[3];
        bool Dazmond(Piece p)=>p.Card.Rule=="dual-receptacle"&&seats[p.Owner].mana[3]>=seats[p.Owner].mana[5];
        internal int ActionAura(Piece p)=>p.Card.Kind==CardType.Creature?All.Count(q=>q.Owner==p.Owner&&Lucio(q)):0;
        int NonCreatureDamageBonus(int amount,Piece source,int caster)
        {
            int owner=source?.Owner??caster;if(amount<=0||owner<0||source?.Card.Kind==CardType.Creature)return amount;
            return amount+2*All.Count(p=>p.Owner==owner&&Dazmond(p));
        }
        void DamageCapital(int target,int amount,Piece source=null,int caster=-1)
        {
            if(target<0||seats[target].Eliminated)return;
            LoseLife(target,NonCreatureDamageBonus(amount,source,caster));
        }
        void BicolorBeforeDeath(Piece dead,int at)
        {
            if(dead.Card.Kind!=CardType.Creature)return;
            var observers=combatDeathObservers??All.ToArray();
            foreach(var observer in observers.Where(p=>p.Owner==dead.Owner))
            {
                if(observer.Card.Rule=="lissandra"&&dead.CombatLethal)Trigger(observer,"lissandra-counter",dead,at);
                if(observer.Card.Rule=="suicidal-inventor")Trigger(observer,"inventor-death",dead,at);
            }
        }
        bool BicolorCanActivate(Piece p)
        {
            if(p.Card.Rule=="lissandra")return seats[p.Owner].grave.Any(c=>c.Kind==CardType.Creature&&c.Playable&&c.TotalCost<=seats[p.Owner].DeathCounters);
            if(p.Card.Rule=="old-fisher")return p.Actions>0&&seats[p.Owner].main.Count>0&&FisherDestination(p.Owner)>=0;
            return false;
        }
        int FisherDestination(int owner)
        {
            int at=seats[owner].LastCreatedTerrain;
            return Valid(at)&&cells[at].Terrain!=null?at:-1;
        }
        void QueueBicolorAbility(Piece p,int amount=0,Definition chosen=null)
        {
            Trigger(p,p.Card.Rule,repeatByAcademy:false);var item=stack.Last();item.Amount=amount;item.ChosenCard=chosen;
        }
        bool BicolorActivate(Piece p)
        {
            int owner=p.Owner;
            if(p.Card.Rule=="lissandra")
            {
                var choices=seats[owner].grave.Where(c=>c.Kind==CardType.Creature&&c.Playable&&c.TotalCost<=seats[owner].DeathCounters).Distinct().ToArray();
                Ask(owner,"Lissandra: copiar do seu cemitério",choices.Select((c,n)=>new ChoiceOption(n,c.Name+" · "+c.TotalCost+" pontos de morte")),n=>{
                    if(n<0||n>=choices.Length)return;var c=choices[n];if(!seats[owner].grave.Contains(c)||seats[owner].DeathCounters<c.TotalCost)return;
                    seats[owner].DeathCounters-=c.TotalCost;QueueBicolorAbility(p,c.TotalCost,c);
                },true);return true;
            }
            if(p.Card.Rule=="old-fisher")
            {
                Ask(owner,"Pescador: quantos PA usar?",Enumerable.Range(1,p.Actions).Select(n=>new ChoiceOption(n,n+" PA · olhar até "+n+" cartas")),n=>{
                    if(n<1||Find(p.Id)==null||p.Actions<n||FisherDestination(owner)<0)return;p.Actions-=n;QueueBicolorAbility(p,n);
                },true);return true;
            }
            return false;
        }
        bool ResolveBicolor(Pending item)
        {
            int owner=item.Owner;
            switch(item.Rule)
            {
                case "lissandra-counter":seats[owner].DeathCounters++;return true;
                case "lissandra":
                    if(item.ChosenCard!=null&&seats[owner].grave.Contains(item.ChosenCard))
                    {
                        int at=Capital(owner,seats.Length);
                        var copy=new Piece(nextPiece++,owner,item.ChosenCard){Game=this,Token=true,BaseAttackOverride=1,BaseDefenseOverride=1};
                        cells[at].pieces.Add(copy);Enter(copy);Visual?.Invoke(new MatchEvent("summon",at,at,copy.Id,copy.Card,owner));
                    }
                    return true;
                case "inventor-death":
                    if(item.Subject==null)return true;
                    int origin=item.SourceCell,power=item.Subject.DeathPower;
                    var victims=All.Where(p=>(p.Card.Kind==CardType.Creature||p.Card.Kind==CardType.Construction)&&Distance(origin,Position(p.Id))<=1).ToArray();
                    var dealt=new List<(Piece target,int amount)>(); foreach(var victim in victims){int before=victim.Damage;DamageTo(victim,power,item.Subject,false);dealt.Add((victim,victim.Damage-before));} foreach(var hit in dealt)AfterDamage(hit.target,hit.amount,item.Subject);
                    foreach(int seat in Enumerable.Range(0,seats.Length).Where(s=>Distance(origin,Capital(s,seats.Length))<=1).ToArray())DamageCapital(seat,power,item.Subject,owner);
                    var previous=combatDeathObservers;combatDeathObservers=All.ToArray();try{StateCheck();}finally{combatDeathObservers=previous;}Preview(item,origin);return true;
                case "old-fisher":ResolveFisher(item);return true;
                case "bear-guard":if(item.Source!=null&&Find(item.Source.Id)!=null)MoveGuard(item.Source,item.SourceCell);return true;
                default:return false;
            }
        }
        void ResolveFisher(Pending item)
        {
            int owner=item.Owner,at=FisherDestination(owner);if(at<0)return;
            var revealed=new List<Definition>();for(int n=0;n<item.Amount&&seats[owner].main.Count>0;n++)revealed.Add(Pop(seats[owner].main));
            var candidates=revealed.Where(c=>c.Kind==CardType.Creature&&c.Playable&&CardFilter.CardColors(c).Contains(2)).Distinct().ToArray();
            void Bottom(){seats[owner].main.InsertRange(0,revealed.AsEnumerable().Reverse());}
            string seen="Cartas vistas: "+string.Join(", ",revealed.Select(c=>c.Name)); if(candidates.Length==0){Ask(owner,seen,new[]{new ChoiceOption(0,"Nenhuma criatura de Água — colocar no fundo")},n=>Bottom());return;}
            Ask(owner,seen+". Escolha uma criatura de Água",candidates.Select((c,n)=>new ChoiceOption(n,c.Name)),n=>{
                if(n<0||n>=candidates.Length){Bottom();return;}var chosen=candidates[n];int destination=FisherDestination(owner);
                if(destination>=0){revealed.Remove(chosen);var p=new Piece(nextPiece++,owner,chosen){Game=this};cells[destination].pieces.Add(p);Enter(p);Visual?.Invoke(new MatchEvent("summon",destination,destination,p.Id,chosen,owner));}
                Bottom();
            });
        }
        int[] GuardPath(Piece bear,int target)
        {
            int start=Position(bear.Id);var parents=new Dictionary<int,int>{{start,-1}};var pending=new Queue<int>();pending.Enqueue(start);
            while(pending.Count>0&&!parents.ContainsKey(target))
            {
                int at=pending.Dequeue();
                foreach(int next in MovementNeighbors(bear,at).OrderBy(n=>n))
                {
                    if(parents.ContainsKey(next)||cells[next].Terrain==null||Blocked(bear,next))continue;
                    if(next!=target&&(Enemies(bear.Owner,cells[next].CapitalOwner)||cells[next].pieces.Any(p=>Enemies(bear.Owner,p.Owner)&&p.Card.Kind!=CardType.Equipment)))continue;
                    parents[next]=at;pending.Enqueue(next);
                }
            }
            if(!parents.ContainsKey(target))return Array.Empty<int>();
            var path=new List<int>();for(int n=target;n!=start;n=parents[n])path.Add(n);path.Reverse();return path.ToArray();
        }
        void MoveGuard(Piece bear,int target)
        {
            if(!Valid(target)||cells[target].Terrain==null||Position(bear.Id)==target)return;
            var path=GuardPath(bear,target);if(path.Length==0)path=new[]{target};
            foreach(int at in path)
            {
                int from=Position(bear.Id);if(from<0)break;cells[from].pieces.Remove(bear);cells[at].pieces.Add(bear);MarkMoved(bear,from,at);
                Visual?.Invoke(new MatchEvent("move",from,at,bear.Id,bear.Card,bear.Owner));
            }
        }
        bool GuardInvasion(Piece intruder,int target)
        {
            int owner=cells[target].Owner;if(owner<0||!Enemies(intruder.Owner,owner))return false;
            var guards=All.Where(p=>p.Card.Rule=="mother-bear"&&p.Owner==owner&&Position(p.Id)!=target).ToArray();
            if(guards.Length==0)return false;
            foreach(var bear in guards)MoveGuard(bear,target);
            Note("Ursa Mãe interrompeu a invasão. Ataque o terreno para avançar.");return true;
        }
        void GuardAttack(int target,int defender)
        {
            foreach(var bear in All.Where(p=>p.Card.Rule=="mother-bear"&&p.Owner==defender&&cells[target].Owner==p.Owner&&Position(p.Id)!=target).ToArray())
                Trigger(bear,"bear-guard",cell:target);
        }
    }
}



