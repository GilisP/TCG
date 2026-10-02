using System;
using System.Collections.Generic;
using System.Linq;

namespace TCG.Foundation
{
    [Serializable] public sealed class AbilityOp
    {
        public string kind="", value="",condition=""; public int a,b,c;
        public AbilityOp Copy()=>new AbilityOp{kind=kind,value=value,condition=condition,a=a,b=b,c=c};
    }
    [Serializable] public sealed class CardAbility
    {
        public string trigger="cast",target="owner",scope="any",condition="",gate="",subtype="",sacrifice="";
        public int mana,pa,manaColor=-1,colored,maxCost=999,count=1; public bool once,optional;
        public AbilityOp[] ops=Array.Empty<AbilityOp>();
        public CardAbility Copy()=>new CardAbility{trigger=trigger,target=target,scope=scope,condition=condition,gate=gate,subtype=subtype,sacrifice=sacrifice,mana=mana,pa=pa,manaColor=manaColor,colored=colored,maxCost=maxCost,count=count,once=once,optional=optional,ops=ops.Select(x=>x.Copy()).ToArray()};
    }
    public static class AbilitySchema
    {
        public static readonly HashSet<string> Triggers=new HashSet<string>{"cast","enter","death","combatDeath","attack","defend","afterAttack","hostAfterAttack","hurt","combatHit","capitalHit","armyCapitalHit","turn","instant","allyDeathHere","allyHurtHere","allyEnterHere","hostDeath","activate","aura"};
        public static readonly HashSet<string> Targets=new HashSet<string>{"self","host","subject","owner","own","other","enemy","creature","ownBuilding","enemyBuilding","enemyBody","ownVehicle","player","groupOwn","groupAny"};
        public static readonly HashSet<string> Scopes=new HashSet<string>{"any","here","adjacent","hereOrAdjacent"};
        public static readonly HashSet<string> Conditions=new HashSet<string>{"","adjAlly","sameOther","nearBuilding","still","capital","range2","noEnemyAdjacent","alone","wounded","moved","attacked","grave3","grave5","attackCapital","attackBuilding","attackWounded","targetHasAllyAdjacent","ownTurn","terrainCreature","terrainBuilding","terrainCapital","terrainTwo","terrainEntered","equipped"};
        public static readonly HashSet<string> Operations=new HashSet<string>{"buff","buffSource","shield","gainLife","loseLife","draw","scry","orderTop","selectTop","grave","damage","damageSelf","heal","move","groupMove","pull","sacrifice","sacrificeSource","loot","bottomHand","mill","millDamage","millSelf","mana","keyword","bounce","drawTerrain","noMove","noAttack","immune","immobile","marker","equipDiscount","pirateRecover"};
        public static bool Valid(CardAbility[] abilities)=>abilities!=null&&abilities.Length<=16&&abilities.All(a=>a!=null&&Triggers.Contains(a.trigger)&&Targets.Contains(a.target)&&Scopes.Contains(a.scope)&&Conditions.Contains(a.condition)&&Conditions.Contains(a.gate)&&(a.sacrifice==""||a.sacrifice=="own"||a.sacrifice=="other"||a.sacrifice=="self")&&a.mana>=0&&a.mana<=100&&a.pa>=0&&a.pa<=10&&a.manaColor>=-1&&a.manaColor<7&&a.colored>=0&&a.colored<=10&&a.maxCost>=0&&a.count>=1&&a.count<=121&&a.ops!=null&&a.ops.Length>0&&a.ops.Length<=12&&a.ops.All(o=>o!=null&&Operations.Contains(o.kind)&&Conditions.Contains(o.condition)&&Math.Abs(o.a)<=1000&&Math.Abs(o.b)<=1000&&Math.Abs(o.c)<=1000));
    }

    public sealed partial class Match
    {
        readonly Dictionary<string,int> abilityUses=new Dictionary<string,int>();
        readonly Dictionary<int,int> enteredTerrain=new Dictionary<int,int>();
        readonly Dictionary<Definition,int> foreignOwners=new Dictionary<Definition,int>();
        string AbilityKey(Piece source,int cell,int index)=>source!=null?"p"+source.Id+":"+index:"t"+cell+":"+index;
        int AbilityOrigin(Pending item)=>item.Source!=null&&Position(item.Source.Id)>=0?Position(item.Source.Id):item.SourceCell;
        bool AbilityCondition(string condition,Piece p,int owner,int at,int target=-1)
        {
            switch(condition??"")
            {
                case "":return true;
                case "adjAlly":return p!=null&&Formation(p);
                case "sameOther":return p!=null&&All.Any(q=>q!=p&&q.Owner==owner&&q.Card.Kind==CardType.Creature&&Position(q.Id)==at);
                case "nearBuilding":return All.Any(q=>q.Owner==owner&&q.Card.Kind==CardType.Construction&&Adjacent(Position(q.Id),at));
                case "still":return p!=null&&p.LastMovedTurn!=Turn;
                case "capital":case "terrainCapital":return Valid(at)&&cells[at].CapitalOwner==owner;
                case "range2":return p!=null&&p.Card.Range+p.Modifiers.Sum(m=>m.Range)+AuthorBonus(p,2)+EquipmentBonus(p,2)+RangeSeed(p)>=2;
                case "noEnemyAdjacent":return !All.Any(q=>q.Card.Kind==CardType.Creature&&Enemies(owner,q.Owner)&&Adjacent(Position(q.Id),at));
                case "alone":return p!=null&&!All.Any(q=>q!=p&&q.Card.Kind==CardType.Creature&&Position(q.Id)==at);
                case "wounded":return p!=null&&p.Damage>0;
                case "moved":return p!=null&&p.LastMovedTurn==Turn;
                case "attacked":return p!=null&&attackedThisTurn.Contains(p.Id);
                case "grave3":return seats[owner].grave.Count(c=>c.Kind==CardType.Creature)>=3;
                case "grave5":return seats[owner].grave.Count(c=>c.Kind==CardType.Creature)>=5;
                case "attackCapital":return Valid(target)&&Enemies(owner,cells[target].CapitalOwner);
                case "attackBuilding":return Valid(target)&&cells[target].pieces.Any(q=>Enemies(owner,q.Owner)&&q.Card.Kind==CardType.Construction);
                case "attackWounded":return Valid(target)&&cells[target].pieces.Any(q=>Enemies(owner,q.Owner)&&q.Damage>0);
                case "targetHasAllyAdjacent":return Valid(target)&&All.Any(q=>q!=p&&q.Owner==owner&&q.Card.Kind==CardType.Creature&&Adjacent(Position(q.Id),target));
                case "ownTurn":return Active==owner;
                case "terrainCreature":return cells[at].pieces.Any(q=>q.Owner==owner&&q.Card.Kind==CardType.Creature);
                case "terrainBuilding":return cells[at].pieces.Any(q=>q.Owner==owner&&q.Card.Kind==CardType.Construction);
                case "terrainTwo":return cells[at].pieces.Count(q=>q.Owner==owner&&q.Card.Kind==CardType.Creature)>=2;
                case "terrainEntered":return enteredTerrain.TryGetValue(at*4+owner,out int turn)&&turn==Turn;
                case "equipped":return p!=null&&All.Any(q=>q.AttachedTo==p.Id&&q.Card.Kind==CardType.Equipment);
                default:return false;
            }
        }
        bool AbilityTarget(CardAbility a,Pending item,Piece p,bool passive=false)
        {
            int owner=item.Owner,at=AbilityOrigin(item),pos=Position(p.Id);
            bool type=a.target=="self"?p==item.Source:a.target=="host"?item.Source?.AttachedTo==p.Id:a.target=="subject"?p==item.Subject:
                a.target=="ownBuilding"?p.Owner==owner&&p.Card.Kind==CardType.Construction:
                a.target=="enemyBuilding"?Enemies(owner,p.Owner)&&p.Card.Kind==CardType.Construction:
                a.target=="enemyBody"?Enemies(owner,p.Owner)&&(p.Card.Kind==CardType.Creature||p.Card.Kind==CardType.Construction):
                a.target=="ownVehicle"?p.Owner==owner&&p.Card.IsVehicle:
                p.Card.Kind==CardType.Creature&&(a.target=="creature"||a.target=="groupAny"||a.target=="enemy"&&Enemies(owner,p.Owner)||(a.target=="own"||a.target=="groupOwn"||a.target=="other")&&p.Owner==owner&&(a.target!="other"||p!=item.Source));
            if(!type||p.Card.TotalCost>a.maxCost||!string.IsNullOrEmpty(a.subtype)&&!p.Card.Subtypes.Contains(a.subtype))return false;
            if(a.scope=="here"&&pos!=at||a.scope=="adjacent"&&!Adjacent(pos,at)||a.scope=="hereOrAdjacent"&&pos!=at&&!Adjacent(pos,at))return false;
            return AbilityCondition(a.condition,p,owner,pos,item.Target)&&(passive||!Immune(p,owner));
        }
        int RangeSeed(Piece p)
        {
            int n=0;foreach(var source in All)foreach(var a in source.Card.Abilities.Where(a=>a.trigger=="aura"&&a.condition!="range2"))if(AbilityTarget(a,new Pending{Owner=source.Owner,Source=source,SourceCell=Position(source.Id)},p,true))n+=a.ops.Where(op=>op.kind=="buff").Sum(op=>op.c);return n;
        }
        internal int AbilityBonus(Piece p,int stat)
        {
            int total=0;
            foreach(var source in All)
            foreach(var a in source.Card.Abilities.Where(x=>x.trigger=="aura"))
            {
                var item=new Pending{Owner=source.Owner,Source=source,SourceCell=Position(source.Id)};
                if(!AbilityTarget(a,item,p,true))continue;
                foreach(var op in a.ops.Where(x=>x.kind=="buff"))total+=stat==0?op.a:stat==1?op.b:stat==2?op.c:stat==3&&op.value.StartsWith("pa:")?int.Parse(op.value.Substring(3)):stat==4&&op.value.StartsWith("move:")?int.Parse(op.value.Substring(5)):0;
            }
            return total;
        }
        internal int MovementAdjustment(Piece p)=>AbilityBonus(p,4)+p.Modifiers.Sum(x=>x.Movement);
        IEnumerable<string> AbilityKeywords(Piece p)
        {
            foreach(var m in p.Modifiers)if(!string.IsNullOrEmpty(m.Keyword))yield return m.Keyword;
            foreach(var source in All.Where(q=>q.AttachedTo==p.Id||q==p))foreach(var a in source.Card.Abilities.Where(x=>x.trigger=="aura"))
                if(AbilityTarget(a,new Pending{Owner=source.Owner,Source=source,SourceCell=Position(source.Id)},p,true))foreach(var op in a.ops.Where(x=>x.kind=="keyword"))yield return op.value;
        }
        bool AbilityMoveBlocked(Piece p,int caster)
        {
            if(p.Modifiers.Any(m=>m.NoMove||m.ImmuneEnemy&&Enemies(caster,p.Owner)))return true;
            foreach(var source in All)foreach(var a in source.Card.Abilities.Where(x=>x.trigger=="aura"))
                if(AbilityTarget(a,new Pending{Owner=source.Owner,Source=source,SourceCell=Position(source.Id)},p,true)&&a.ops.Any(op=>op.kind=="immobile"&&(op.value=="any"||op.value=="own"&&caster==p.Owner||op.value=="enemy"&&Enemies(caster,p.Owner))))return true;
            return false;
        }
        void AbilityEvent(Piece source,string trigger,Piece subject=null,int cell=-1,int target=-1)
        {
            if(source==null)return;int at=cell>=0?cell:Position(source.Id);
            for(int i=0;i<source.Card.Abilities.Count;i++)
            {
                var a=source.Card.Abilities[i];if(a.trigger!=trigger)continue;
                if(!AbilityCondition(a.gate,source,source.Owner,at,target))continue;
                // Target conditions are checked against selected recipients, event conditions against the source.
                if((a.target=="self"||a.target=="owner"||a.target=="host")&&!AbilityCondition(a.condition,a.target=="host"?Find(source.AttachedTo):source,source.Owner,at,target))continue;
                string key=AbilityKey(source,at,i);if(a.once&&abilityUses.TryGetValue(key,out int turn)&&turn==Turn)continue;
                if(a.once)abilityUses[key]=Turn;
                QueueAbility(source.Card,source.Owner,source,at,i,subject,target);
            }
        }
        void QueueAbility(Definition card,int owner,Piece source,int at,int index,Piece subject=null,int target=-1,bool activated=false)
        {
            stack.Add(new Pending{Owner=owner,Card=card,Rule="abilities",Source=source,SourceCell=at,ScriptIndex=index,Subject=subject,Target=target});passes=0;
            if(!activated&&source!=null&&TerrainRule(at,"academy")&&source.Card.Subtypes.Contains("Mago"))stack.Add(new Pending{Owner=owner,Card=card,Rule="abilities",Source=source,SourceCell=at,ScriptIndex=index,Subject=subject,Target=target});
            Visual?.Invoke(new MatchEvent("trigger",Capital(owner,seats.Length),Capital(owner,seats.Length),source?.Id??-1,card,owner));
        }
        IEnumerable<Piece> SacrificeOptions(CardAbility a,int owner,Piece source)=>All.Where(p=>p.Owner==owner&&p.Card.Kind==CardType.Creature&&(a.sacrifice!="self"||p==source)&&(a.sacrifice!="other"||p!=source&&source!=null&&Position(p.Id)==Position(source.Id)));
        void PayAbilitySacrifice(CardAbility a,int owner,Piece source,Action<Piece> commit)
        {
            if(a.sacrifice==""){commit(null);return;}
            var options=SacrificeOptions(a,owner,source).ToArray();
            if(a.sacrifice=="self"){if(options.Length>0)commit(source);return;}
            Ask(owner,"Sacrifício como custo",options.Select(p=>new ChoiceOption(p.Id,p.Card.Name)),id=>{var p=Find(id);if(p!=null&&SacrificeOptions(a,owner,source).Contains(p))commit(p);},true);
        }
        bool AbilityPayable(CardAbility a,int owner,Piece p)=>(a.sacrifice==""||SacrificeOptions(a,owner,p).Any())&&seats[owner].mana.Sum()>=a.mana+a.colored&&(a.manaColor<0||seats[owner].mana[a.manaColor]>=a.colored)&&(a.pa==0||p!=null&&p.Actions>=a.pa);
        bool AbilityHasTargets(CardAbility a,Pending item)
        {
            if(a.target=="owner")return a.ops.All(op=>op.kind!="pirateRecover"||seats.Any(s=>s.grave.Milled.Any()));
            if(a.target=="player")return true;
            return All.Any(p=>AbilityTarget(a,item,p));
        }
        bool CanUseAbility(Piece p)
        {
            if(p==null)return false;
            return p.Card.Abilities.Select((a,i)=>(a,i)).Any(x=>x.a.trigger=="activate"&&AbilityPayable(x.a,p.Owner,p)&&(!x.a.once||!abilityUses.TryGetValue(AbilityKey(p,Position(p.Id),x.i),out int t)||t!=Turn)&&AbilityHasTargets(x.a,new Pending{Owner=p.Owner,Source=p,SourceCell=Position(p.Id)}));
        }
        void UseAbility(Piece p)
        {
            var list=p.Card.Abilities.Select((a,i)=>(a,i)).Where(x=>x.a.trigger=="activate"&&AbilityPayable(x.a,p.Owner,p)&&(!x.a.once||!abilityUses.TryGetValue(AbilityKey(p,Position(p.Id),x.i),out int t)||t!=Turn)&&AbilityHasTargets(x.a,new Pending{Owner=p.Owner,Source=p,SourceCell=Position(p.Id)})).ToArray();
            void Use(int index){var a=p.Card.Abilities[index];PayAbilitySacrifice(a,p.Owner,p,cost=>{if(!AbilityPayable(a,p.Owner,p))return;int at=Position(p.Id);if(a.manaColor>=0)seats[p.Owner].mana[a.manaColor]-=a.colored;SpendGeneric(p.Owner,a.mana);p.Actions-=a.pa;abilityUses[AbilityKey(p,at,index)]=Turn;QueueAbility(p.Card,p.Owner,p,at,index,activated:true);if(cost!=null)CommitKill(cost);});}
            if(list.Length==1)Use(list[0].i);else Ask(p.Owner,"Escolha a habilidade",list.Select(x=>new ChoiceOption(x.i,"Habilidade "+(x.i+1))),Use);
        }
        public bool CanActivateTerrain(int at)
        {
            if(IsRemoteView)return NetCan("terrainAbility",at);
            if(!Valid(at)||!CargoWindow||cells[at].Owner!=Active||cells[at].Terrain==null)return false;
            return cells[at].Terrain.Abilities.Select((a,i)=>(a,i)).Any(x=>x.a.trigger=="activate"&&AbilityPayable(x.a,Active,null)&&AbilityCondition(x.a.gate,null,Active,at)&&(!abilityUses.TryGetValue(AbilityKey(null,at,x.i),out int t)||t!=Turn)&&AbilityHasTargets(x.a,new Pending{Owner=Active,SourceCell=at}));
        }
        void ActivateTerrain(int at)
        {
            Check(CanActivateTerrain(at),"Habilidade do terreno indisponível.");var card=cells[at].Terrain;int index=card.Abilities.Select((a,i)=>(a,i)).First(x=>x.a.trigger=="activate").i;var a=card.Abilities[index];SpendGeneric(Active,a.mana);abilityUses[AbilityKey(null,at,index)]=Turn;QueueAbility(card,Active,null,at,index,activated:true);
        }
        bool AbilityEnchantmentTarget(Definition card,Piece host,int owner)
        {
            if(card.Kind!=CardType.Enchantment)return true;
            if(card.Id=="PD26-G-028")return host.Owner==owner&&host.Card.IsVehicle;
            if(card.Id=="PD26-L-028"||card.Id=="PD26-F-028")return host.Card.Kind==CardType.Creature;
            return !card.Id.StartsWith("PD26-")||host.Owner==owner&&host.Card.Kind==CardType.Creature;
        }
        bool AbilityResponse(Definition card,int owner)=>card.Abilities.Where(x=>x.trigger=="cast").All(a=>(a.sacrifice==""||SacrificeOptions(a,owner,null).Any())&&AbilityHasTargets(a,new Pending{Owner=owner,Card=card}));
        bool ResolveAbilities(Pending item)
        {
            if(item.Rule!="abilities")return false;
            var entries=item.ScriptIndex>=0?new[]{item.Card.Abilities[item.ScriptIndex]}:item.Card.Abilities.Where(x=>x.trigger=="cast").ToArray();
            foreach(var original in entries)
            {
                var a=original.Copy();
                work.Enqueue(()=>SelectAbilityTarget(item,a));
            }
            return true;
        }
        void SelectAbilityTarget(Pending item,CardAbility a)
        {
            if(a.optional&&a.target=="owner"){var next=a.Copy();next.optional=false;Ask(item.Owner,"Usar "+item.Card.Name+"?",new[]{new ChoiceOption(1,"Usar habilidade")},n=>{if(n==1)SelectAbilityTarget(item,next);},true);return;}
            if(a.target=="owner"){AbilityOps(item,a,null,0,item.Owner);return;}
            if(a.target=="player"){Ask(item.Owner,"Escolha o jogador",Enumerable.Range(0,seats.Length).Where(n=>!seats[n].Eliminated).Select(n=>new ChoiceOption(n,seats[n].Name)),n=>AbilityOps(item,a,null,0,n));return;}
            if(a.target=="self"||a.target=="host"||a.target=="subject")
            {var p=a.target=="self"?item.Source:a.target=="host"?Find(item.Source?.AttachedTo??-1):item.Subject;if(p!=null&&Find(p.Id)!=null&&AbilityTarget(a,item,p))AbilityOps(item,a,p,0,a.trigger=="capitalHit"?item.Target:item.Owner);return;}
            if(a.target.StartsWith("group"))
            {
                Ask(item.Owner,"Escolha o terreno do efeito",All.Where(p=>AbilityTarget(a,item,p)).Select(p=>Position(p.Id)).Distinct().Select(n=>new ChoiceOption(n,"Terreno "+n%11+","+n/11)),at=>{
                    if(a.ops.Any(op=>op.kind=="groupMove")){GroupMoveAbility(item,a,at);return;}
                    if(a.count>=121){foreach(var p in cells[at].pieces.Where(p=>AbilityTarget(a,item,p)).ToArray())AbilityOps(item,a,p,0,item.Owner);}
                    else Many(item.Owner,item.Card.Name,a.count,p=>Position(p.Id)==at&&AbilityTarget(a,item,p),p=>AbilityOps(item,a,p,0,item.Owner));
                });return;
            }
            if(a.count>1)Many(item.Owner,item.Card.Name,a.count,p=>AbilityTarget(a,item,p),p=>AbilityOps(item,a,p,0,item.Owner));
            else Pick(item.Owner,item.Card.Name,p=>AbilityTarget(a,item,p),p=>AbilityOps(item,a,p,0,item.Owner),a.optional);
        }
        void AbilityOps(Pending item,CardAbility a,Piece target,int index,int player)
        {
            if(index>=a.ops.Length||Over)return;var op=a.ops[index];int owner=item.Owner;
            void Next()=>work.Enqueue(()=>AbilityOps(item,a,target,index+1,player));
            bool alive=target!=null&&Find(target.Id)!=null;
            if(!AbilityCondition(op.condition,target??item.Source,owner,target!=null?Position(target.Id):AbilityOrigin(item),item.Target)){Next();return;}
            switch(op.kind)
            {
                case "buff":if(alive){int mv=op.value.StartsWith("move:")?int.Parse(op.value.Substring(5)):0;target.Modifiers.Add(new Modifier{Attack=op.a,Defense=op.b,Range=op.c,Movement=mv,EndTurn=op.value=="next"?-1:Turn,NextOwner=op.value=="next"?owner:-1,CombatOnly=op.value=="combat"});Preview(item,Position(target.Id));}break;
                case "buffSource":if(item.Source!=null&&Find(item.Source.Id)!=null)Buff(item.Source,op.a,op.b,op.c);break;
                case "marker":if(alive)target.Modifiers.Add(new Modifier{Attack=op.a,Defense=op.b});break;
                case "shield":if(alive)target.Modifiers.Add(new Modifier{Prevent=op.a,EndTurn=Turn,AccumulatePrevention=true});break;
                case "gainLife":GainLife(owner,op.a);break;
                case "loseLife":LoseLife(owner,op.a);break;
                case "draw":for(int n=0;n<op.a&&!seats[owner].Eliminated;n++)DrawCard(owner);break;
                case "damage":if(alive)DamageTo(target,op.a,item.Source,true,owner);break;
                case "damageSelf":if(item.Source!=null&&Find(item.Source.Id)!=null)DamageTo(item.Source,op.a,item.Source,true,owner);else if(alive)DamageTo(target,op.a,null,true,owner);break;
                case "heal":if(alive)target.Damage=Math.Max(0,target.Damage-op.a);break;
                case "keyword":if(alive)target.Modifiers.Add(new Modifier{Keyword=op.value,EndTurn=Turn});break;
                case "noMove":if(alive)target.Modifiers.Add(new Modifier{NoMove=true,EndTurn=Turn});break;
                case "noAttack":if(alive)target.Modifiers.Add(new Modifier{NoAttack=true,EndTurn=Turn});break;
                case "immune":if(alive)target.Modifiers.Add(new Modifier{ImmuneEnemy=true,EndTurn=Turn});break;
                case "bounce":if(alive)ReturnHand(target);break;
                case "move":if(alive)Displace(owner,target,op.a,true,op.value=="own"?(Func<int,bool>)(at=>cells[at].Owner==owner):null,Next,at=>{Preview(item,at);Next();});else Next();return;
                case "pull":if(alive){int at=AbilityOrigin(item);if(Valid(at)&&Adjacent(Position(target.Id),at)&&!Blocked(target,at))ApplyDisplacement(owner,target,at,n=>Preview(item,n));}break;
                case "sacrifice":if(alive)Kill(target);break;
                case "sacrificeSource":if(item.Source!=null&&Find(item.Source.Id)!=null)Kill(item.Source);break;
                case "mana":AddMana(owner,op.b,op.a);break;
                case "mill":MillCards(player,op.a);break;
                case "millDamage":MillCards(item.Target,item.Amount);break;
                case "millSelf":MillCards(owner,op.a);break;
                case "scry":ScryAbility(owner,op.value,Next);return;
                case "orderTop":OrderTopAbility(owner,op.a,Next);return;
                case "selectTop":SelectTopAbility(owner,op.a,Next);return;
                case "loot":LootAbility(owner,Next);return;
                case "bottomHand":BottomHandAbility(owner,Next);return;
                case "grave":GraveAbility(owner,op,Next);return;
                case "drawTerrain":Ask(owner,"Pilha para o terreno comprado",Enumerable.Range(0,4).Select(n=>new ChoiceOption(n,"Pilha "+(n+1))),n=>{DrawTerrain(owner,n);Next();});return;
                case "pirateRecover":RecoverMilled(owner,Next);return;
                default:throw new InvalidOperationException("Operação de habilidade inválida: "+op.kind);
            }
            Next();
        }
        void BottomHandAbility(int owner,Action done)
        {
            var hand=seats[owner].hand.ToArray();if(hand.Length==0){done();return;}
            Ask(owner,"Coloque uma carta da mão no fundo",hand.Select((c,n)=>new ChoiceOption(n,c.Name)),n=>{if(seats[owner].hand.Remove(hand[n]))seats[owner].main.Insert(0,hand[n]);done();});
        }
        void GroupMoveAbility(Pending item,CardAbility ability,int origin)
        {
            var selected=new List<Piece>();
            void Destination(){if(selected.Count==0)return;Ask(item.Owner,"Destino comum adjacente",Neighbors(origin).Where(at=>cells[at].Owner==item.Owner&&cells[at].Terrain!=null&&selected.All(p=>!Blocked(p,at))).Select(at=>new ChoiceOption(at,"Terreno "+at%11+","+at/11)),at=>{foreach(var p in selected.Where(p=>Find(p.Id)!=null&&Position(p.Id)==origin&&!Immune(p,item.Owner)).ToArray())ApplyDisplacement(item.Owner,p,at,n=>Preview(item,n));});}
            void Select(){if(selected.Count>=ability.count){Destination();return;}var choices=cells[origin].pieces.Where(p=>!selected.Contains(p)&&AbilityTarget(ability,item,p)).ToArray();if(choices.Length==0){Destination();return;}Ask(item.Owner,"Escolha até "+ability.count+" criaturas",choices.Select(p=>new ChoiceOption(p.Id,p.Card.Name)),id=>{if(id<0){Destination();return;}var p=Find(id);if(p!=null)selected.Add(p);Select();},true);}Select();
        }
        void ScryAbility(int owner,string mode,Action done)
        {
            if(seats[owner].main.Count==0){done();return;}var card=seats[owner].main.Last();Ask(owner,"Topo: "+card.Name,new[]{new ChoiceOption(0,"Manter no topo"),new ChoiceOption(1,mode=="mill"?"Colocar no cemitério":"Colocar no fundo")},n=>{if(n==1&&seats[owner].main.LastOrDefault()==card){if(mode=="mill")MillCards(owner,1);else{Pop(seats[owner].main);seats[owner].main.Insert(0,card);}}done();});
        }
        void OrderTopAbility(int owner,int count,Action done)
        {
            var original=seats[owner].main.AsEnumerable().Reverse().Take(count).ToList();var ordered=new List<Definition>();
            void ChooseNext(){if(original.Count==0){foreach(var card in ordered)seats[owner].main.Remove(card);seats[owner].main.AddRange(ordered.AsEnumerable().Reverse());done();return;}Ask(owner,"Ordenar topo: próxima carta a comprar",original.Select((c,n)=>new ChoiceOption(n,c.Name)),n=>{ordered.Add(original[n]);original.RemoveAt(n);ChooseNext();});}ChooseNext();
        }
        void SelectTopAbility(int owner,int count,Action done)
        {
            var cards=seats[owner].main.AsEnumerable().Reverse().Take(count).ToArray();if(cards.Length==0){done();return;}
            Ask(owner,"Escolha uma carta para a mão",cards.Select((c,n)=>new ChoiceOption(n,c.Name)),n=>{foreach(var c in cards)seats[owner].main.Remove(c);seats[owner].hand.Add(cards[n]);var remaining=cards.Where((c,i)=>i!=n).ToList();int bottomPosition=0;void Bottom(){if(remaining.Count==0){done();return;}Ask(owner,"Fundo: escolha a próxima carta (mais profunda primeiro)",remaining.Select((c,i)=>new ChoiceOption(i,c.Name)),i=>{seats[owner].main.Insert(bottomPosition++,remaining[i]);remaining.RemoveAt(i);Bottom();});}Bottom();});
        }
        void LootAbility(int owner,Action done)
        {
            var hand=seats[owner].hand.ToArray();if(hand.Length==0){done();return;}
            Ask(owner,"Colocar uma carta no fundo e comprar",hand.Select((c,n)=>new ChoiceOption(n,c.Name)),n=>{if(seats[owner].hand.Remove(hand[n])){seats[owner].main.Insert(0,hand[n]);DrawCard(owner);}done();});
        }
        void GraveAbility(int owner,AbilityOp op,Action done)
        {
            var parts=op.value.Split(':');string kind=parts[0],dest=parts.Length>1?parts[1]:"hand";
            var choices=seats[owner].grave.Where(c=>c.TotalCost<=op.a&&(kind=="any"||kind=="creature"&&c.Kind==CardType.Creature||kind=="spell"&&c.Kind==CardType.Spell||kind=="instant"&&c.Kind==CardType.Instant||kind=="equipment"&&c.Kind==CardType.Equipment)).Distinct().ToArray();
            if(choices.Length==0){done();return;}
            Ask(owner,"Escolha no seu cemitério",choices.Select((c,n)=>new ChoiceOption(n,c.Name)),n=>{var card=choices[n];if(seats[owner].grave.Remove(card)){if(dest=="hand")seats[owner].hand.Add(card);else if(dest=="top")seats[owner].main.Add(card);else seats[owner].main.Insert(0,card);}done();});
        }
        void RecoverMilled(int owner,Action done)
        {
            var choices=seats.SelectMany((s,n)=>s.grave.Milled.Select(x=>(owner:n,entry:x))).ToArray();
            if(choices.Length==0){done();return;}
            Ask(owner,"Carta que veio diretamente do grimório",choices.Select((x,n)=>new ChoiceOption(n,x.entry.Card.Name+" · "+seats[x.owner].Name)),n=>{var choice=choices[n];if(seats[choice.owner].grave.RemoveEntry(choice.entry.Id)){var card=new Definition(NetworkCards.Pack(choice.entry.Card),choice.entry.Card.Expansion);foreignOwners[card]=choice.owner;seats[owner].hand.Add(card);}done();});
        }
        int CardOriginalOwner(Definition card,int fallback)=>foreignOwners.TryGetValue(card,out int owner)?owner:fallback;
        void AbilityCapitalHit(Piece source,int victim,int amount)
        {
            if(source==null||amount<=0)return;AbilityEvent(source,"capitalHit",target:victim);
            foreach(var equipment in All.Where(p=>p.AttachedTo==source.Id).ToArray())AbilityEvent(equipment,"capitalHit",source,target:victim);
            if(source.Card.Kind==CardType.Creature)foreach(var pirate in All.Where(p=>p.Owner==source.Owner&&p.Card.Id=="IDEIA-004").ToArray())
                {QueueAbility(pirate.Card,pirate.Owner,pirate,Position(pirate.Id),1,source,victim);stack.Last().Amount=amount;}
        }
        void AbilitiesEntered(Piece p)
        {
            AbilityEvent(p,"enter");int at=Position(p.Id);if(p.Card.Kind==CardType.Creature){enteredTerrain[at*4+p.Owner]=Turn;foreach(var q in All.Where(q=>q.Owner==p.Owner&&q!=p&&Position(q.Id)==at).ToArray())AbilityEvent(q,"allyEnterHere",p);}
        }
        void AbilitiesDied(Piece p,int at)
        {
            AbilityEvent(p,"death",cell:at);if(p.CombatLethal)AbilityEvent(p,"combatDeath",cell:at);
            if(p.Card.Kind==CardType.Creature)foreach(var q in All.Where(q=>q.Owner==p.Owner&&Position(q.Id)==at).ToArray())AbilityEvent(q,"allyDeathHere",p);
        }
        void AbilitiesDamaged(Piece p,int amount,Piece source)
        {
            if(amount<=0)return;if(p.Health>0)AbilityEvent(p,"hurt",source);
            foreach(var q in All.Where(q=>q!=p&&q.Owner==p.Owner&&Position(q.Id)==Position(p.Id)).ToArray())AbilityEvent(q,"allyHurtHere",p);
        }
        void AbilitiesCombatHit(Piece source,Piece target,int amount)
        {
            if(amount<=0)return;AbilityEvent(source,"combatHit",target);foreach(var e in All.Where(e=>e.AttachedTo==source.Id).ToArray())AbilityEvent(e,"combatHit",target);
        }
        void ApplyDefenseAbilities(Pending battle)
        {
            foreach(var id in battle.Blockers.Where(x=>x>=0).Distinct()){
                var p=Find(id);if(p==null)continue;
                for(int i=0;i<p.Card.Abilities.Count;i++){var a=p.Card.Abilities[i];if(a.trigger!="defend")continue;var key=AbilityKey(p,Position(id),i);if(a.once&&abilityUses.TryGetValue(key,out int t)&&t==Turn)continue;abilityUses[key]=Turn;foreach(var op in a.ops)if(op.kind=="shield")Shield(p,op.a);}
            }
        }
        int AbilityEquipCost(Piece equipment,Piece host)
        {
            int cost=equipment.Card.EquipCost;
            foreach(var q in All.Where(q=>q.Owner==equipment.Owner&&Position(q.Id)==Position(host.Id)))
                if(q.Card.Abilities.Any(a=>a.ops.Any(o=>o.kind=="equipDiscount"))&&(!abilityUses.TryGetValue("equip"+q.Id,out int turn)||turn!=Turn))cost--;
            return Math.Max(0,cost);
        }
        void MarkAbilityEquip(Piece host){foreach(var q in All.Where(q=>q.Owner==host.Owner&&Position(q.Id)==Position(host.Id)&&q.Card.Abilities.Any(a=>a.ops.Any(o=>o.kind=="equipDiscount"))))abilityUses["equip"+q.Id]=Turn;}
    }
}
