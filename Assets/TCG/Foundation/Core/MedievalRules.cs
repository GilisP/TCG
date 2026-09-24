using System;
using System.Collections.Generic;
using System.Linq;

namespace TCG.Foundation
{
    public sealed class ChoiceOption
    {
        public int Key { get; } public string Label { get; }
        public ChoiceOption(int key,string label) { Key=key; Label=label; }
    }
    public sealed class RuleChoice
    {
        public int Owner { get; } public string Prompt { get; } public IReadOnlyList<ChoiceOption> Options { get; }
        internal readonly Action<int> Apply;
        internal RuleChoice(int owner,string prompt,IEnumerable<ChoiceOption> options,Action<int> apply)
        { Owner=owner; Prompt=prompt; Options=Array.AsReadOnly(options.ToArray()); Apply=apply; }
    }
    internal sealed class Modifier
    {
        internal int Attack,Defense,Range,Prevent,EndTurn=-1,NextOwner=-1,DelayedDamage;
        internal bool Formation,CombatOnly;
    }
    public static class MedievalRules
    {
        public static readonly HashSet<string> Programs=new HashSet<string>{
            "", "equipment-death","phoenix-return","rage","rage-one", "guard","march-pair","formation-prevent","rally","sun-turn","bastion-move","king-turn",
            "grave-bottom","blood-price","grave-cycle","recover-one","death-burst","recover-small","recover-two","recover-any","altar","recover-two-hand","immortal",
            "hold","repair","fortify","attack-buff","damage-two","damage-building","explosion","fire-march","phoenix",
            "move-ally","range-buff","move-any-two","swap","move-self","move-subject","wind-all","wind-extra",
            "enter-move-ally","move-adjacent","reduce-attack","enter-push","prevent-three","bounce-small","bounce-draw","push-attacker","tide-response","retreat","leviathan-enter","leviathan-hit"
        };
        public static readonly HashSet<string> Traits=new HashSet<string>{
            "formation-defense","aura-defense-1","equipment-attack-1-formation","aura-army","turn-sun","aura-defense-2","hurt-bastion","aura-king","turn-king",
            "death-bottom","attack-wounded","enter-recover-one","equipment-attack-2-death","death-recover-small","death-recover-first","activate-altar","enter-recover-two","death-immortal",
            "block-all","near-building","enter-repair","immune-enemy","block-enemy","still-defense","aura-defense-radius","aura-immune","immune-all","hold-defense",
            "hurt-rage","equipment-attack-2-defense-minus","kill-move","enter-fire","kill-rage","death-phoenix",
            "pass-units","equipment-pa","after-move","ally-after-move","move-extra",
            "enter-move-ally","enter-push","hurt-push","enemy-after-push","enter-leviathan","hit-push"
        };
        public static bool Valid(string rule,string[] traits)=>(Programs.Contains(rule??"")||AuthorRules.Programs.Contains(rule??""))&&(traits==null||traits.All(t=>Traits.Contains(t)||AuthorRules.Traits.Contains(t)));
    }
    public sealed partial class Match
    {
        public RuleChoice Choice { get; private set; }
        readonly Queue<Action> work=new Queue<Action>();
        readonly List<Pending> completedVisuals=new List<Pending>();
        void StackResult(Pending item,bool canceled) { if(item.Card!=null)Visual?.Invoke(new MatchEvent(canceled?"stack-canceled":"stack-resolved",Capital(item.Owner,seats.Length),item.Target,card:item.Card,owner:item.Owner,stackId:item.VisualId)); }
        readonly List<(int owner,int turn,Definition card)> phoenix=new List<(int,int,Definition)>();
        readonly List<(int owner,int turn,int cell)> enemyDeaths=new List<(int,int,int)>();
        readonly HashSet<int> impactTiles=new HashSet<int>();
        int effectBudget; bool checking; Definition resolvingVisual; int resolvingOwner; bool hadImpact;
        IEnumerable<Piece> All=>cells.SelectMany(c=>c.pieces);
        public bool Allied(int a,int b)=>a>=0&&b>=0&&seats[a].Team==seats[b].Team&&!seats[b].Eliminated;
        static bool Has(Piece p,string trait)=>p!=null&&(p.Card.Traits.Contains(trait)||p.Card.Keywords.Contains(trait)||p.Card.IsVehicle&&(p.Game?.EffectiveKeywords(p.Id).Contains(trait)??false));
        static int Distance(int a,int b)=>Math.Abs(a%11-b%11)+Math.Abs(a/11-b/11);
        bool Near(Piece a,Piece b,int radius=1)=>a!=b&&Position(a.Id)>=0&&Position(b.Id)>=0&&(radius==1?Distance(Position(a.Id),Position(b.Id))==1:Distance(Position(a.Id),Position(b.Id))<=radius);
        bool Formation(Piece p)=>All.Any(q=>q.Card.Kind==CardType.Creature&&Allied(q.Owner,p.Owner)&&Near(p,q));
        bool Powered(Piece p)=>p.Card.Kind!=CardType.Construction||All.Any(q=>q.Card.Kind==CardType.Creature&&Allied(q.Owner,p.Owner)&&Position(q.Id)==Position(p.Id));
        public int EquipmentBonus(Piece p,int stat)
        {
            int result=0;
            foreach(var e in All.Where(e=>e.AttachedTo==p.Id))
            {
                if(stat==0) {
                    if(Has(e,"equipment-attack-1-formation")) result+=Formation(p)?2:1;
                    if(Has(e,"equipment-attack-2-death")||Has(e,"equipment-attack-2-defense-minus")) result+=2;
                }
                if(stat==1&&Has(e,"equipment-attack-2-defense-minus")) result--;
                if(stat==3&&Has(e,"equipment-pa")) result++;
            }
            return result;
        }
        internal int Bonus(Piece p,int stat)
        {
            int n=AuthorBonus(p,stat)+EquipmentBonus(p,stat)+p.Modifiers.Sum(m=>stat==0?m.Attack:stat==1?m.Defense:stat==2?m.Range:0);
            if(stat==1&&Has(p,"formation-defense")&&Formation(p)) n++;
            if(stat==0&&Has(p,"near-building")&&All.Any(q=>q.Card.Kind==CardType.Construction&&Allied(p.Owner,q.Owner)&&Near(p,q))) n++;
            if(stat==1&&Has(p,"hold-defense")&&p.LastMovedTurn!=Turn) n+=3;
            if(stat==1&&Has(p,"still-defense")&&p.LastMovedTurn<p.PreviousOwnTurn) n+=2;
            if(p.Card.Kind!=CardType.Creature) return n;
            foreach(var q in All.Where(q=>q!=p&&Allied(q.Owner,p.Owner)&&Powered(q)))
            {
                if(Near(p,q)) {
                    if(stat==1&&Has(q,"aura-defense-1")) n++;
                    if(stat==1&&Has(q,"aura-defense-2")) n+=2;
                    if((stat==0||stat==1)&&Has(q,"aura-army")) n++;
                }
                if(Near(p,q,2)) {
                    if((stat==0||stat==1)&&Has(q,"aura-king")) n++;
                    if(stat==1&&Has(q,"aura-defense-radius")) n++;
                }
            }
            return n;
        }
        internal void StateCheck()
        {
            if(checking) return; checking=true;
            try { int guard=0; while(All.Any(p=>(p.Card.Kind!=CardType.Equipment&&p.Card.Kind!=CardType.Enchantment)&&p.Health<=0&&!pendingDeaths.Contains(p.Id))&&guard++<200) foreach(var p in All.Where(p=>(p.Card.Kind!=CardType.Equipment&&p.Card.Kind!=CardType.Enchantment)&&p.Health<=0&&!pendingDeaths.Contains(p.Id)).ToArray()) Kill(p); }
            finally { checking=false; }
        }
        void Drain()
        {
            effectBudget=0;
            while(Choice==null&&work.Count>0&&!Over) { Check(++effectBudget<=1000,"Sequência de efeitos excedeu o limite de segurança."); work.Dequeue()(); StateCheck(); }
            if(Choice==null&&work.Count==0){foreach(var item in completedVisuals)StackResult(item,false);completedVisuals.Clear();}
            if(Choice==null&&work.Count==0&&resolvingVisual!=null){if(!hadImpact)Visual?.Invoke(new MatchEvent("resolve",Capital(resolvingOwner,seats.Length),Capital(resolvingOwner,seats.Length),-1,resolvingVisual,resolvingOwner));resolvingVisual=null;}
        }
        void Ask(int owner,string prompt,IEnumerable<ChoiceOption> options,Action<int> apply,bool optional=false)
        {
            work.Enqueue(()=>{
                if(seats[owner].Eliminated) return;
                var list=options.ToList(); if(list.Count==0) return;
                if(optional) list.Add(new ChoiceOption(-1,"Não usar / concluir seleção"));
                Choice=new RuleChoice(owner,prompt,list,apply);
            });
        }
        void Choose(int key)
        {
            Check(Choice!=null&&Choice.Options.Any(o=>o.Key==key),"Escolha inválida.");
            var choice=Choice; Choice=null; choice.Apply(key);
        }
        void Pick(int owner,string title,Func<Piece,bool> filter,Action<Piece> apply,bool optional=false)
        {
            work.Enqueue(()=>{
                var candidates=All.Where(p=>(p.Card.Kind!=CardType.Equipment&&p.Card.Kind!=CardType.Enchantment)&&filter(p)).ToArray();
                if(candidates.Length==0) return;
                var opts=candidates.Select(p=>new ChoiceOption(p.Id,p.Card.Name+" — "+p.Attack+"/"+p.Health+" · "+Position(p.Id)%11+","+Position(p.Id)/11)).ToList();
                if(optional) opts.Add(new ChoiceOption(-1,"Não usar"));
                Choice=new RuleChoice(owner,title,opts,key=>{ var p=Find(key); if(p!=null&&filter(p)) ApplyChosenTarget(owner,p,filter,apply); });
            });
        }
        void Many(int owner,string title,int count,Func<Piece,bool> filter,Action<Piece> apply,HashSet<int> selected=null)
        {
            if(count<=0) return; selected=selected??new HashSet<int>();
            Pick(owner,title+" ("+count+" restante(s))",p=>!selected.Contains(p.Id)&&filter(p),p=>{selected.Add(p.Id);apply(p);Many(owner,title,count-1,filter,apply,selected);},true);
        }
        void Grave(int owner,string title,int maxCost,Action<Definition> apply,bool optional=false,Definition exclude=null,bool any=false)
        {
            work.Enqueue(()=>{
                var candidates=seats[owner].grave.Where(c=>(any||c.Kind==CardType.Creature)&&c.TotalCost<=maxCost&&c!=exclude).Distinct().ToArray();
                if(candidates.Length==0) return;
                var opts=candidates.Select((c,i)=>new ChoiceOption(i,c.Name+" · custo "+c.TotalCost)).ToList(); if(optional) opts.Add(new ChoiceOption(-1,"Não usar"));
                Choice=new RuleChoice(owner,title,opts,i=>{ if(i>=0&&i<candidates.Length&&seats[owner].grave.Remove(candidates[i])) apply(candidates[i]); });
            });
        }
        void Buff(Piece p,int attack=0,int defense=0,int range=0,bool next=false,int nextOwner=-1)
        { p.Modifiers.Add(new Modifier{Attack=attack,Defense=defense,Range=range,EndTurn=next?-1:Turn,NextOwner=next?(nextOwner>=0?nextOwner:p.Owner):-1});if(resolvingVisual!=null)Preview(new Pending{Owner=resolvingOwner,Card=resolvingVisual},Position(p.Id)); }
        void Preview(Pending item,int target)
        { hadImpact=true; if(!impactTiles.Add(target))return; Visual?.Invoke(new MatchEvent("impact",Capital(item.Owner,seats.Length),target,-1,item.Card,item.Owner)); }
        void Shield(Piece p,int amount,bool formation=false)=>p.Modifiers.Add(new Modifier{Prevent=amount,EndTurn=Turn,Formation=formation});
        void EndModifiers()
        {
            AuthorEnd();
            foreach(var p in All.ToArray()) foreach(var m in p.Modifiers.Where(m=>m.EndTurn==Turn).ToArray()) {p.Modifiers.Remove(m);if(m.DelayedDamage>0) DamageTo(p,m.DelayedDamage,null,true);}
        }
        void StartModifiers()
        { foreach(var p in All.ToArray()) p.Modifiers.RemoveAll(m=>m.NextOwner==Active); StateCheck(); }
        bool MovePath(Piece p,int target)
        {
            int origin=Position(p.Id); if(!Valid(target))return false;
            if(PortalLink(origin,target))return true;
            if(Adjacent(origin,target))return true;
            if(!Has(p,"pass-units")||Distance(origin,target)>p.Movement)return false;
            var visited=new HashSet<int>{origin};var frontier=new List<int>{origin};
            for(int k=0;k<p.Movement;k++){var next=new List<int>();foreach(int at in frontier)foreach(int n in Neighbors(at))if(cells[n].Terrain!=null&&!Blocked(p,n)&&visited.Add(n)){if(n==target)return true;next.Add(n);}frontier=next;}
            return false;
        }
        void KillReward(Piece p){if(Find(p.Id)==null)return;if(Has(p,"kill-move"))Trigger(p,"move-self");if(Has(p,"kill-rage"))Trigger(p,"rage-one");}
        internal void DamageTo(Piece p,int amount,Piece source,bool check=true,int caster=-1)
        {
            if(amount<=0||p==null||Find(p.Id)==null) return;
            int remaining=ReflectDamage(p,NonCreatureDamageBonus(amount,source,caster),source,caster);if(remaining>0&&source!=null)lastDamageSource[p.Id]=source;else if(remaining>0)lastDamageSource.Remove(p.Id);
            foreach(var m in p.Modifiers.Where(m=>m.Prevent>0&&(!m.Formation||Formation(p)))) {int used=Math.Min(m.Prevent,remaining);remaining-=used;m.Prevent=0;}
            if(p.Health>0&&remaining>=p.Health)p.BeforeLethalDamage=p.Damage;
            p.Damage+=remaining;if(remaining>0)p.LastDamagedTurn=Turn;
            if(check) { AfterDamage(p,remaining,source); if(p.Health<=0) { Kill(p,source?.Owner??caster); if(source!=null&&p.Card.Kind==CardType.Creature)KillReward(source); } }
        }
        void AfterDamage(Piece p,int amount,Piece source)
        {
            if(amount<=0) return; AuthorAfterDamage(source,amount);
            if(p.Health>0&&Has(p,"hurt-rage")) Trigger(p,"rage");
            if(p.Health>0&&source!=null&&Has(p,"hurt-push")) Trigger(p,"push-attacker",source);
            if(source!=null&&Has(source,"hit-push")&&source.OnceTurn!=Turn) {source.OnceTurn=Turn;Trigger(source,"leviathan-hit",p);}
            foreach(var q in All.Where(q=>Has(q,"hurt-bastion")&&Powered(q)&&Allied(q.Owner,p.Owner)&&Near(q,p)).ToArray()) Trigger(q,"bastion-move",p);
        }
        void Trigger(Piece p,string rule,Piece subject=null,int cell=-1,bool repeatByAcademy=true)
        {
            if(seats[p.Owner].Eliminated) return;
            stack.Add(new Pending{Owner=p.Owner,Card=p.Card,Rule=rule,Source=p,SourceCell=cell>=0?cell:Position(p.Id),Subject=subject});
            if(repeatByAcademy&&TerrainRule(Position(p.Id),"academy")&&p.Card.Subtypes.Contains("Mago"))stack.Add(new Pending{Owner=p.Owner,Card=p.Card,Rule=rule,Source=p,SourceCell=cell>=0?cell:Position(p.Id),Subject=subject});
            passes=0; Visual?.Invoke(new MatchEvent("trigger",Capital(p.Owner,seats.Length),Capital(p.Owner,seats.Length),p.Id,p.Card,p.Owner));
        }
        void Enter(Piece p)
        {
            NewEnter(p);foreach(var pair in new[]{("enter-recover-one","recover-one"),("enter-recover-two","recover-two-hand"),("enter-repair","repair"),("enter-fire","damage-two"),("enter-move-ally","enter-move-ally"),("enter-push","enter-push"),("enter-leviathan","leviathan-enter")})
                if(Has(p,pair.Item1)) Trigger(p,pair.Item2);
        }
        void TurnTriggers()
        {
            AuthorTurn();
            foreach(var p in All.Where(p=>p.Owner==Active).ToArray()) { if(Has(p,"turn-sun")) Trigger(p,"sun-turn");if(Has(p,"turn-king")) Trigger(p,"king-turn"); }
            foreach(var f in phoenix.Where(f=>f.owner==Active&&f.turn<Turn).ToArray())
            {
                phoenix.Remove(f);
                if(seats[f.owner].grave.Contains(f.card)){
                    stack.Add(new Pending{Owner=f.owner,Card=f.card,Rule="phoenix-return"});passes=0;
                    Visual?.Invoke(new MatchEvent("trigger",Capital(f.owner,seats.Length),Capital(f.owner,seats.Length),-1,f.card,f.owner));
                }
            }
        }
        void OnDeath(Piece p,int cell,int killer)
        {
            AuthorDeath(p,cell);
            if(p.Card.Kind==CardType.Creature&&killer>=0&&Enemies(p.Owner,killer)) enemyDeaths.Add((p.Owner,Turn,cell));
            if(Has(p,"death-bottom")) Trigger(p,"grave-bottom",null,cell);
            if(Has(p,"death-immortal")) Trigger(p,"immortal",null,cell);
            if(Has(p,"death-phoenix")) { Trigger(p,"phoenix",null,cell);phoenix.Add((p.Owner,Turn,p.Card)); }
            foreach(var q in All.Where(q=>p.Card.Kind==CardType.Creature&&q.Owner==p.Owner&&q.Card.Kind==CardType.Creature).ToArray()) {
                if(Has(q,"death-recover-small")&&q.OnceTurn!=Turn){q.OnceTurn=Turn;Trigger(q,"recover-small",p);}
                if(Has(q,"death-recover-first")&&q.Owner==Active&&q.OnceTurn!=Turn){q.OnceTurn=Turn;Trigger(q,"recover-any",p);}
            }
        }
        void Detach(Piece p)
        {
            foreach(var e in All.Where(e=>e.AttachedTo==p.Id).ToArray()) {
                if(Has(e,"equipment-attack-2-death")) Trigger(e,"equipment-death",p);
                e.AttachedTo=-1;if(e.Card.Kind==CardType.Enchantment)CommitKill(e);
            }
        }
        void LoseLife(int owner,int amount) {seats[owner].Life-=amount;if(seats[owner].Life<=0)Eliminate(owner);}
        public bool CanEquip(int equipment,int host)
        {if(IsRemoteView)return NetCan("equip",equipment,host);
            var e=Find(equipment);var p=Find(host);
            return Choice==null&&Phase==Stage.Main&&Active==Priority&&stack.Count==0&&e!=null&&e.Card.Kind==CardType.Equipment&&e.Owner==Active&&p!=null&&p.Card.Kind==CardType.Creature&&p.Owner==Active&&e.AttachedTo!=host&&seats[Active].mana.Sum()>=e.Card.EquipCost;
        }
        void Equip(int equipment,int host)
        {
            MainOnly();Check(CanEquip(equipment,host),"Selecione equipamento seu e criatura sua; pague equipar.");
            var e=Find(equipment);var old=Find(e.AttachedTo); if(old!=null&&Has(e,"equipment-pa"))old.Actions=Math.Max(0,old.Actions-1);
            int left=e.Card.EquipCost;for(int i=6;i>=0&&left>0;i--){int v=Math.Min(left,seats[Active].mana[i]);seats[Active].mana[i]-=v;left-=v;}
            e.AttachedTo=host;var p=Find(host);if(Has(e,"equipment-pa"))p.Actions++;
            cells[Position(e.Id)].pieces.Remove(e);cells[Position(host)].pieces.Add(e);
            Note(e.Card.Name+" equipado em "+p.Card.Name+".");StateCheck();
        }
        public bool CanActivate(int id) {if(IsRemoteView)return NetCan("activate",id);var p=Find(id);if(MonkResponse(p))return true;return p!=null&&CanAct(p)&&p.Owner==Active&&(Has(p,"activate-altar")&&Powered(p)&&p.OnceTurn!=Turn||AuthorCanActivate(p))&&stack.Count==0&&Priority==Active&&Phase==Stage.Main;}
        void Activate(int id){if(MonkResponse(Find(id))){NewActivate(Find(id));return;}MainOnly();Check(CanActivate(id),"Habilidade indisponível.");var p=Find(id);if(AuthorCanActivate(p)){AuthorActivate(p);return;}p.OnceTurn=Turn;Trigger(p,"altar",repeatByAcademy:false);}
        bool Immune(Piece p,int caster)=>Has(p,"immune-all")||(Enemies(caster,p.Owner)&&(Has(p,"immune-enemy")||All.Any(q=>Has(q,"aura-immune")&&Powered(q)&&Allied(q.Owner,p.Owner)&&Near(q,p))));
        bool Blocked(Piece p,int cell)=>(TerrainRule(cell,"mountain-crossing")&&!Has(p,"flying"))||cells[cell].pieces.Any(q=>q!=p&&Powered(q)&&(Has(q,"block-all")||Has(q,"block-enemy")&&Enemies(p.Owner,q.Owner)));
        bool InRange(Piece p,int target)
        {
            int origin=Position(p.Id); if(origin<0||!Valid(target))return false;
            if(p.Range==1)return Adjacent(origin,target);
            if(origin%11!=target%11&&origin/11!=target/11)return false;
            if(Distance(origin,target)>p.Range||origin==target)return false;
            int step=origin%11==target%11?(target>origin?11:-11):(target>origin?1:-1);
            for(int i=origin+step;i!=target;i+=step)if(cells[i].Terrain==null)return false;
            return true;
        }
        int AttackValue(Piece p,int target)
        {return p.Attack+(Has(p,"attack-wounded")&&cells[target].pieces.Any(q=>Enemies(p.Owner,q.Owner)&&q.LastDamagedTurn==Turn)?1:0);}
        void OnAttack(Piece p,int target)
        {
            AuthorAttack(p,target);
            foreach(var q in All.Where(q=>Has(q,"enemy-after-push")&&Enemies(q.Owner,p.Owner)&&Near(q,p,2)&&q.OnceTurn!=Turn).ToArray()){q.OnceTurn=Turn;afterBattle.Add(()=>Trigger(q,"tide-response",p));}
        }
        readonly List<Action> afterBattle=new List<Action>();
        void AfterAttack(Piece p)
        {
            if(Has(p,"after-move"))Trigger(p,"move-self");
            foreach(var q in All.Where(q=>Has(q,"ally-after-move")&&Allied(q.Owner,p.Owner)&&Near(q,p,2)&&q.OnceTurn!=Turn).ToArray()){q.OnceTurn=Turn;Trigger(q,"move-subject",p);}
            foreach(var a in afterBattle.ToArray())a();afterBattle.Clear();
        }
        void MarkMoved(Piece p,int from,int to)
        {
            if(p.CarrierId>=0&&Position(p.CarrierId)!=to)p.CarrierId=-1;CarryPassengers(p,from,to);p.LastMovedTurn=Turn;AuthorMoved(p,from,to);
            foreach(var e in All.Where(e=>e.AttachedTo==p.Id).ToArray()){cells[Position(e.Id)].pieces.Remove(e);cells[to].pieces.Add(e);}
            foreach(var q in All.Where(q=>Has(q,"move-extra")&&Allied(q.Owner,p.Owner)&&Distance(Position(q.Id),to)<=3&&q.OnceTurn!=Turn).ToArray()){q.OnceTurn=Turn;Trigger(q,"wind-extra",p);}
        }
        void Displace(int owner,Piece p,int steps,bool optional=true,Func<int,bool> constraint=null,Action declined=null,Action<int> impact=null)
        {
            work.Enqueue(()=>{
                if(Find(p.Id)==null||p.Card.Kind!=CardType.Creature||Immune(p,owner)){declined?.Invoke();return;}
                int start=Position(p.Id);var reached=new HashSet<int>{start};var frontier=new List<int>{start};
                for(int k=0;k<steps;k++){var next=new List<int>();foreach(int pos in frontier)foreach(int n in Neighbors(pos))if(cells[n].Terrain!=null&&!Blocked(p,n)&&(constraint==null||constraint(n))&&reached.Add(n))next.Add(n);frontier=next;}
                reached.Remove(start);if(reached.Count==0){declined?.Invoke();return;}
                var opts=reached.OrderBy(i=>i).Select(i=>new ChoiceOption(i,"Mover "+p.Card.Name+" para "+i%11+","+i/11)).ToList();if(optional)opts.Add(new ChoiceOption(-1,"Não mover"));
                Choice=new RuleChoice(owner,"Destino · "+p.Card.Name,opts,dest=>{if(dest<0||Find(p.Id)==null){declined?.Invoke();return;}RedirectDisplacement(owner,p,dest,reached,impact);});
            });
        }
        void ReturnHand(Piece p){int pos=Position(p.Id);if(pos<0)return;ReleasePassenger(p);foreach(var e in All.Where(e=>e.AttachedTo==p.Id).ToArray()){e.AttachedTo=-1;if(e.Card.Kind==CardType.Enchantment)CommitKill(e);}cells[pos].pieces.Remove(p);seats[p.OriginalOwner].hand.Add(p.Card);Visual?.Invoke(new MatchEvent("return",pos,pos,p.Id,p.Card,p.Owner));}
        void ResolveRule(Pending i)
        {
            resolvingVisual=i.Card;resolvingOwner=i.Owner;hadImpact=false;impactTiles.Clear();
            int o=i.Owner;Piece src=i.Source;int origin=src!=null&&Position(src.Id)>=0?Position(src.Id):i.SourceCell;
            Func<Piece,bool> ally=p=>p.Card.Kind==CardType.Creature&&Allied(o,p.Owner);
            Func<Piece,bool> own=p=>p.Card.Kind==CardType.Creature&&p.Owner==o;
            Func<Piece,bool> enemy=p=>p.Card.Kind==CardType.Creature&&Enemies(o,p.Owner);
            Action<Piece> impact=p=>Preview(i,Position(p.Id));
            void MoveEffect(Piece p,int steps,bool optional=true,Func<int,bool> constraint=null,Action declined=null)=>Displace(o,p,steps,optional,constraint,declined,dest=>Preview(i,dest));
            if(AuthorResolve(i))return;
            switch(i.Rule) {
                case "equipment-death":DrawCard(o);if(!seats[o].Eliminated)LoseLife(o,1);break;
                case "phoenix-return":
                    if(seats[o].grave.Contains(i.Card)&&seats[o].mana[3]>=3)
                        Ask(o,"Fênix: pagar FFF para devolver à mão?",new[]{new ChoiceOption(1,"Pagar FFF")},key=>{if(key==1&&seats[o].mana[3]>=3&&seats[o].grave.Remove(i.Card)){seats[o].mana[3]-=3;seats[o].hand.Add(i.Card);}},true);
                    break;
                case "rage":case "rage-one":if(src!=null&&Find(src.Id)!=null)Buff(src,i.Rule=="rage"?2:1);break;
                case "guard":Pick(o,"Proteger criatura aliada",ally,p=>{Buff(p,0,Formation(p)?3:2);impact(p);});break;
                case "hold":Pick(o,"Manter posição",ally,p=>{Buff(p,0,p.LastMovedTurn==Turn?2:3);impact(p);});break;
                case "attack-buff":Pick(o,"Aumentar ataque",ally,p=>{Buff(p,2);impact(p);});break;
                case "range-buff":Pick(o,"Aumentar alcance",ally,p=>{Buff(p,0,0,1);impact(p);});break;
                case "reduce-attack":Pick(o,"Reduzir ataque inimigo",enemy,p=>{Buff(p,-2);impact(p);});break;
                case "prevent-three":Pick(o,"Prevenir próximos 3 de dano",ally,p=>{Shield(p,3);impact(p);});break;
                case "formation-prevent":Pick(o,"Reduzir próximo dano em formação",ally,p=>{Shield(p,2,true);impact(p);});break;
                case "blood-price":Pick(o,"Preço de sangue",own,p=>{Buff(p,2);p.Modifiers.Add(new Modifier{EndTurn=Turn,DelayedDamage=2});impact(p);});break;
                case "damage-two":
                    if(src!=null){foreach(var p in All.Where(p=>enemy(p)&&Adjacent(origin,Position(p.Id))).ToArray()){DamageTo(p,2,src);impact(p);}}
                    else Pick(o,"Causar 2 de dano",p=>p.Card.Kind==CardType.Creature,p=>{impact(p);DamageTo(p,2,null,true,o);});break;
                case "damage-building":Pick(o,"Causar 4 a uma construção",p=>p.Card.Kind==CardType.Construction,p=>{impact(p);DamageTo(p,4,null,true,o);});break;
                case "explosion":Pick(o,"Centro da explosão",p=>p.Card.Kind==CardType.Creature,p=>{int at=Position(p.Id);var near=All.Where(q=>q.Card.Kind==CardType.Creature&&Adjacent(at,Position(q.Id))).ToArray();impact(p);DamageTo(p,3,null,true,o);foreach(var q in near)DamageTo(q,1,null,true,o);});break;
                case "grave-bottom":Grave(o,"Colocar carta no fundo",999,c=>seats[o].main.Insert(0,c),false,null,true);break;
                case "grave-cycle":Grave(o,"Criatura no fundo; comprar",999,c=>{seats[o].main.Insert(0,c);DrawCard(o);});break;
                case "recover-one":Grave(o,"Recuperar criatura de custo 1",1,c=>seats[o].hand.Add(c));break;
                case "recover-small":Ask(o,"Pagar 2 de vida para recuperar?",new[]{new ChoiceOption(1,"Pagar 2")},k=>{if(k<0&&src!=null)src.OnceTurn=-1;if(k==1){LoseLife(o,2);Grave(o,"Criatura diferente de custo até 2",2,c=>seats[o].hand.Add(c),false,i.Subject?.Card);}},true);break;
                case "recover-any":Grave(o,"Recuperar criatura diferente",999,c=>seats[o].hand.Add(c),false,i.Subject?.Card);break;
                case "recover-two":Grave(o,"Primeira criatura: mão",999,c=>{seats[o].hand.Add(c);Grave(o,"Segunda criatura: fundo",999,d=>seats[o].main.Insert(0,d),true);},true);break;
                case "recover-two-hand":Grave(o,"Primeira criatura: mão",999,c=>{seats[o].hand.Add(c);Grave(o,"Segunda criatura: mão",999,d=>seats[o].hand.Add(d),true);},true,src?.Card);break;
                case "immortal":Ask(o,"Pagar 5 de vida para devolver o Rei?",new[]{new ChoiceOption(1,"Pagar 5")},k=>{if(k==1&&seats[o].grave.Remove(i.Card)){LoseLife(o,5);if(!seats[o].Eliminated)seats[o].hand.Add(i.Card);}},true);break;
                case "altar":Pick(o,"Destruir sua criatura adjacente",p=>own(p)&&Adjacent(origin,Position(p.Id)),p=>{Kill(p);DrawCard(o);if(!seats[o].Eliminated)DrawCard(o);LoseLife(o,2);},true);break;
                case "death-burst":
                    var death=enemyDeaths.LastOrDefault(d=>d.owner==o&&d.turn==Turn);
                    if(enemyDeaths.Any(d=>d.owner==o&&d.turn==Turn))Pick(o,"Retaliar na vizinhança da morte",p=>enemy(p)&&Adjacent(death.cell,Position(p.Id)),p=>{impact(p);DamageTo(p,2,null,true,o);});break;
                case "repair":Pick(o,"Reparar construção adjacente",p=>p.Card.Kind==CardType.Construction&&Allied(o,p.Owner)&&Adjacent(origin,Position(p.Id)),p=>{p.Damage=Math.Max(0,p.Damage-3);impact(p);});break;
                case "fortify":Pick(o,"Construção a reforçar",p=>p.Card.Kind==CardType.Construction&&Allied(o,p.Owner),p=>{Buff(p,0,2,0,true,o);impact(p);int at=Position(p.Id);Many(o,"Reforçar criatura adjacente",2,q=>ally(q)&&Adjacent(at,Position(q.Id)),q=>Buff(q,0,2,0,true,o));});break;
                case "phoenix":foreach(var p in All.Where(p=>p.Card.Kind==CardType.Creature&&Adjacent(origin,Position(p.Id))).ToArray()){impact(p);DamageTo(p,3,src);}break;
                case "bounce-small":Pick(o,"Devolver criatura de custo até 2",p=>p.Card.Kind==CardType.Creature&&p.Card.TotalCost<=2,p=>{impact(p);ReturnHand(p);});break;
                case "bounce-draw":Pick(o,"Devolver criatura; controlador compra",p=>p.Card.Kind==CardType.Creature,p=>{int owner=p.Owner;impact(p);ReturnHand(p);DrawCard(owner);});break;
                case "move-ally":Pick(o,"Mover criatura aliada",ally,p=>MoveEffect(p,1));break;
                case "enter-move-ally":Pick(o,"Mover aliada adjacente",p=>ally(p)&&Adjacent(origin,Position(p.Id)),p=>MoveEffect(p,1),true);break;
                case "enter-push":Pick(o,"Mover criatura adjacente",p=>p.Card.Kind==CardType.Creature&&Adjacent(origin,Position(p.Id)),p=>MoveEffect(p,2));break;
                case "move-adjacent":Pick(o,"Referência para adjacência (sua criatura)",own,p=>{int at=Position(p.Id);Pick(o,"Criatura adjacente a mover",q=>q.Card.Kind==CardType.Creature&&Adjacent(at,Position(q.Id)),q=>MoveEffect(q,1));});break;
                case "move-any-two":Pick(o,"Mover criatura até 2 tiles",p=>p.Card.Kind==CardType.Creature,p=>MoveEffect(p,2));break;
                case "move-self":if(src!=null&&Find(src.Id)!=null)MoveEffect(src,1);break;
                case "move-subject":case "wind-extra":case "push-attacker":case "tide-response":if(i.Subject!=null&&Find(i.Subject.Id)!=null)MoveEffect(i.Subject,1,true,null,()=>{if(src!=null)src.OnceTurn=-1;});break;
                case "leviathan-hit":if(i.Subject!=null&&Find(i.Subject.Id)!=null)MoveEffect(i.Subject,2,true,null,()=>{if(src!=null)src.OnceTurn=-1;});break;
                case "bastion-move":Pick(o,"Mover aliada junto ao Bastião",p=>ally(p)&&p!=i.Subject&&Adjacent(origin,Position(p.Id)),p=>MoveEffect(p,1),true);break;
                case "sun-turn":Many(o,"Apoiar tropa adjacente",2,p=>ally(p)&&Adjacent(origin,Position(p.Id)),p=>{Buff(p,1);MoveEffect(p,1);});break;
                case "king-turn":if(All.Count(p=>ally(p)&&p!=src&&Distance(origin,Position(p.Id))<=2)>=3)Many(o,"Mover tropa do Rei",3,p=>ally(p)&&p!=src&&Distance(origin,Position(p.Id))<=2,p=>MoveEffect(p,1));break;
                case "march-pair":Pick(o,"Primeira tropa (até duas)",ally,p=>{int at=Position(p.Id);Pick(o,"Segunda tropa adjacente (opcional)",q=>ally(q)&&Adjacent(at,Position(q.Id)),q=>MoveEffect(q,1),true);MoveEffect(p,1);},true);break;
                case "rally":Pick(o,"Tropa de referência",own,p=>{int at=Position(p.Id);Many(o,"Reagrupar",3,q=>ally(q)&&q!=p,q=>{int before=Distance(Position(q.Id),at);MoveEffect(q,1,true,n=>Distance(n,at)<before);});});break;
                case "fire-march":Many(o,"Marcha das chamas",3,ally,p=>{Buff(p,2);var enemies=All.Where(enemy).Select(q=>Position(q.Id)).ToArray();int start=Position(p.Id);MoveEffect(p,1,true,n=>enemies.Any(t=>Distance(n,t)<Distance(start,t)));});break;
                case "swap":Pick(o,"Primeira criatura da troca",ally,p=>Pick(o,"Segunda criatura até 3 tiles",q=>ally(q)&&q!=p&&Distance(Position(p.Id),Position(q.Id))<=3,q=>{
                    if(Immune(p,o)||Immune(q,o))return;int a=Position(p.Id),b=Position(q.Id);cells[a].pieces.Remove(p);cells[b].pieces.Remove(q);cells[a].pieces.Add(q);cells[b].pieces.Add(p);MarkMoved(p,a,b);MarkMoved(q,b,a);Preview(i,a);Preview(i,b);
                }));break;
                case "wind-all":Ask(o,"Centro do vendaval",Enumerable.Range(0,121).Where(n=>cells[n].Terrain!=null).Select(n=>new ChoiceOption(n,"Tile "+n%11+","+n/11)),at=>{foreach(var p in All.Where(p=>p.Card.Kind==CardType.Creature&&Adjacent(at,Position(p.Id))).ToArray()){int d=Distance(Position(p.Id),at);MoveEffect(p,2,true,n=>Distance(n,at)>d);}});break;
                case "retreat":Many(o,"Maré de retirada",3,p=>p.Card.Kind==CardType.Creature,p=>{int start=Position(p.Id);var capitals=Enumerable.Range(0,seats.Length).Where(e=>Enemies(p.Owner,e)).Select(e=>Capital(e,seats.Length)).ToArray();MoveEffect(p,2,true,n=>capitals.All(c=>Distance(n,c)>=Distance(start,c)));});break;
                case "leviathan-enter":foreach(var p in All.Where(p=>p.Card.Kind==CardType.Creature&&p!=src&&Adjacent(origin,Position(p.Id))).ToArray())MoveEffect(p,2);break;
                default:throw new InvalidOperationException("Programa não implementado: "+i.Rule);
            }
        }
    }
}



