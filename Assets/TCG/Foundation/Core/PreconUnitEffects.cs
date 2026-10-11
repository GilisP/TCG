using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
 public static class PreconUnitSchema
 {
  public static readonly HashSet<string> Traits=new HashSet<string>{"pcu-global","precon-random-mana"};
  public static readonly HashSet<string> Operations=new HashSet<string>{"pcu-token","pcu-declared-volley","pcu-free-move","pcu-heal-pool","pcu-heal-all","pcu-fight","pcu-shot","pcu-allow-attack","pcu-sacrifice-reward","pcu-draw-on-combat-death","pcu-passenger-bonus","pcu-heal-passengers","pcu-disembark","pcu-buff-passengers","pcu-passenger-temp","pcu-return-equipment"};
  public static readonly HashSet<string> Conditions=new HashSet<string>{"pcu-token","pcu-token-equipped","pcu-token-1-1","pcu-defense-greater","pcu-base-zero","pcu-base-at-most-2","pcu-other-goblin-here","pcu-token-capital","pcu-still-since-own","pcu-passengers-2","pcu-passengers-3","pcu-attack-stronger","pcu-sacrifice-goblin","pcu-sacrifice-here","pcu-any-equipped-token","pcu-base-one","pcu-radius-2","pcu-own-still","pcu-boarded"};
  public static readonly HashSet<string> Triggers=new HashSet<string>{"pcu-combat-enter","pcu-goblin-army","pcu-lone-entry","pcu-sacrifice-other","pcu-sacrifice-nontoken","pcu-archer-shot","pcu-unboard","pcu-host-defend","pcu-equipment-enter","pcu-host-capital-hit","pcu-equip-mage"};
  public static readonly HashSet<string> Targets=new HashSet<string>{"pcu-own-body","pcu-commander","pcu-carrier","pcu-allied-creature","pcu-own-other-than-subject"};
 }
 public sealed partial class Match
 {
  readonly Dictionary<Pending,List<(int shooter,int victim)>> preconVolleys=new Dictionary<Pending,List<(int,int)>>();
  bool PreconUnitDeclare(Pending item,Action commit)
  {
   var volley=item.Card?.Abilities.SelectMany(a=>a.ops).FirstOrDefault(op=>op.kind=="pcu-declared-volley");if(volley==null)return false;
   var pairs=new List<(int shooter,int victim)>();preconVolleys[item]=pairs;
   void Declare(int left)
   {
    var shooters=All.Where(p=>p.Owner==item.Owner&&p.Card.Subtypes.Contains("Arqueiro")&&!pairs.Any(x=>x.shooter==p.Id)&&All.Any(q=>q.Card.Kind==CardType.Creature&&Enemies(item.Owner,q.Owner)&&!Immune(q,item.Owner)&&Distance(Position(p.Id),Position(q.Id))<=p.Range)).ToArray();
    if(left<=0||shooters.Length==0){commit();return;}
    Ask(item.Owner,"Arqueiro para a salva (alvos declarados)",shooters.Select(p=>new ChoiceOption(p.Id,p.Card.Name)),id=>{var p=Find(id);if(p==null){commit();return;}var victims=All.Where(q=>q.Card.Kind==CardType.Creature&&Enemies(item.Owner,q.Owner)&&!Immune(q,item.Owner)&&Distance(Position(p.Id),Position(q.Id))<=p.Range).ToArray();if(victims.Length==0){commit();return;}Ask(item.Owner,"Alvo declarado do arqueiro",victims.Select(q=>new ChoiceOption(q.Id,q.Card.Name)),victim=>{pairs.Add((id,victim));Declare(left-1);});},true);
   }
   Declare(volley.b);return true;
  }
  readonly Dictionary<int,(int turn,int owner)> preconDeathDraw=new Dictionary<int,(int,int)>();
  readonly Dictionary<int,(int turn,int attack,int defense)> preconPassengerTemp=new Dictionary<int,(int,int,int)>();
  readonly Dictionary<int,int> preconAttackPermission=new Dictionary<int,int>();
  bool? PreconUnitCondition(string condition,Piece p,int owner,int at,int target)
  {
   switch(condition)
   {
    case "pcu-boarded":return p!=null&&p.CarrierId>=0&&Find(p.CarrierId)!=null;
    case "pcu-own-still":return Active==owner&&p!=null&&p.LastMovedTurn!=Turn;
    case "pcu-any-equipped-token":return All.Any(q=>q.Owner==owner&&q.Token&&All.Any(e=>e.AttachedTo==q.Id&&e.Card.Kind==CardType.Equipment));
    case "pcu-token":return p!=null&&p.Token;
    case "pcu-token-equipped":return p!=null&&p.Token&&All.Any(q=>q.AttachedTo==p.Id&&q.Card.Kind==CardType.Equipment);
    case "pcu-token-1-1":return p!=null&&p.Token&&(p.BaseAttackOverride??p.Card.Attack)==1&&(p.BaseDefenseOverride??p.Card.Defense)==1;
    case "pcu-defense-greater":return p!=null&&p.Defense>p.Attack;
    case "pcu-radius-2":return true;
    case "pcu-base-one":return p!=null&&(p.BaseAttackOverride??p.Card.Attack)==1;
    case "pcu-base-zero":return p!=null&&(p.BaseAttackOverride??p.Card.Attack)==0;
    case "pcu-base-at-most-2":return p!=null&&(p.BaseAttackOverride??p.Card.Attack)<=2;
    case "pcu-other-goblin-here":return p!=null&&All.Any(q=>q!=p&&q.Owner==owner&&q.Card.Subtypes.Contains("Goblin")&&Position(q.Id)==at);
    case "pcu-token-capital":return p!=null&&p.Token&&Valid(at)&&cells[at].CapitalOwner==owner;
    case "pcu-still-since-own":return p!=null&&p.LastMovedTurn<p.LastOwnTurn;
    case "pcu-passengers-2":return p!=null&&Passengers(p.Id).Count>=2;
    case "pcu-passengers-3":return p!=null&&Passengers(p.Id).Count>=3;
    case "pcu-attack-stronger":return p!=null&&Valid(target)&&cells[target].pieces.Any(q=>Enemies(owner,q.Owner)&&q.Card.Kind==CardType.Creature&&q.Attack>p.Attack);
    case "pcu-sacrifice-goblin":return All.Any(q=>q.Owner==owner&&q.Card.Kind==CardType.Creature&&q.Card.Subtypes.Contains("Goblin")&&(p==null||q!=p&&Position(q.Id)==at));
    case "pcu-sacrifice-here":return All.Any(q=>q.Owner==owner&&q.Card.Kind==CardType.Creature&&Position(q.Id)==at);
    default:return null;
   }
  }
  bool PreconShotAvailable(Piece shooter,int owner,int origin,AbilityOp op)
  {
   int at=shooter==null?origin:Position(shooter.Id);if(!Valid(at))return false;int range=op.b>0?op.b:shooter?.Range??1;
   return All.Any(q=>q.Card.Kind==CardType.Creature&&Enemies(owner,q.Owner)&&!Immune(q,owner)&&Distance(at,Position(q.Id))<=range)||op.c>0&&Enumerable.Range(0,seats.Length).Any(e=>Enemies(owner,e)&&Distance(at,Capital(e,seats.Length))<=range);
  }
  bool? PreconUnitHasTargets(CardAbility a,Pending item)
  {
   if(a.ops.Any(op=>op.kind=="pcu-shot"))return a.target=="owner"?a.ops.Where(op=>op.kind=="pcu-shot").All(op=>PreconShotAvailable(item.Source,item.Owner,AbilityOrigin(item),op)):All.Any(p=>AbilityTarget(a,item,p));
   if(a.ops.Any(op=>op.kind=="pcu-disembark"))return All.Any(p=>AbilityTarget(a,item,p));
   if(a.ops.Any(op=>op.kind=="pcu-return-equipment"))return All.Any(p=>p.Owner==item.Owner&&p.Card.Kind==CardType.Equipment);
   if(a.ops.Any(op=>op.kind=="pcu-declared-volley"))return All.Any(p=>p.Owner==item.Owner&&p.Card.Subtypes.Contains("Arqueiro")&&PreconShotAvailable(p,item.Owner,Position(p.Id),new AbilityOp()));
   return null;
  }
  bool? PreconUnitTarget(CardAbility a,Pending item,Piece p,bool passive)
  {
   if(!passive&&a.ops.Any(op=>op.kind=="pcu-shot"&&!PreconShotAvailable(p,item.Owner,AbilityOrigin(item),op)))return false;
   if(!passive&&a.ops.Any(op=>op.kind=="pcu-disembark")&&!Passengers(p.Id).Any(q=>q.Owner==item.Owner))return false;
   if(!PreconUnitSchema.Targets.Contains(a.target))return null;
   int at=AbilityOrigin(item),pos=Position(p.Id);
   bool type=a.target=="pcu-own-other-than-subject"?p.Owner==item.Owner&&p.Card.Kind==CardType.Creature&&p!=item.Subject:a.target=="pcu-allied-creature"?p.Card.Kind==CardType.Creature&&Allied(item.Owner,p.Owner)&&p!=item.Source:a.target=="pcu-own-body"?p.Owner==item.Owner&&(p.Card.Kind==CardType.Creature||p.Card.Kind==CardType.Construction):a.target=="pcu-commander"?p.Owner==item.Owner&&p.CommandUnit:item.Source!=null&&item.Source.CarrierId==p.Id;
   return type&&(a.condition!="pcu-radius-2"||Distance(at,pos)<=2)&&p.Card.TotalCost<=a.maxCost&&(a.subtype==""||p.Card.Subtypes.Contains(a.subtype))&&(a.scope!="here"||at==pos)&&(a.scope!="adjacent"||Adjacent(at,pos))&&(a.scope!="hereOrAdjacent"||at==pos||Adjacent(at,pos))&&AbilityCondition(a.condition,p,item.Owner,pos,item.Target)&&(passive||!Immune(p,item.Owner));
  }
  int PreconUnitBonus(Piece p,int stat)
  {
   int n=0;
   foreach(var a in p.Card.Abilities.Where(x=>x.trigger=="aura"))foreach(var op in a.ops.Where(x=>x.kind=="pcu-passenger-bonus"))
   {int count=Math.Max(0,Passengers(p.Id).Count-op.c);if(op.value=="attack"&&stat==0||op.value=="defense"&&stat==1)n+=count*op.a;if(op.value=="pair")n+=count*(stat==0?op.a:stat==1?op.b:0);}

   if(preconPassengerTemp.TryGetValue(p.Id,out var mod)&&mod.turn==Turn)n+=Passengers(p.Id).Count*(stat==0?mod.attack:stat==1?mod.defense:0);
   return n;
  }
  bool PreconUnitCanAttack(Piece p)=>p!=null&&preconAttackPermission.TryGetValue(p.Id,out int turn)&&turn==Turn&&(p.BaseAttackOverride??p.Card.Attack)==0;
  bool PreconUnitSacrificeAllowed(CardAbility a,Piece p,Piece source,int at)
  {return (a.gate!="pcu-sacrifice-goblin"||p.Card.Subtypes.Contains("Goblin"))&&(a.gate!="pcu-sacrifice-here"||Position(p.Id)==at);}
  void PreconUnitSacrificed(Piece dead,int at)
  {
   if(dead==null||dead.Card.Kind!=CardType.Creature)return;
   foreach(var p in All.Where(p=>p.Owner==dead.Owner&&p!=dead).ToArray())
   {AbilityEvent(p,"pcu-sacrifice-other",dead,Position(p.Id),at);if(!dead.Token)AbilityEvent(p,"pcu-sacrifice-nontoken",dead,Position(p.Id),at);}
  }
  void PreconUnitMoved(Piece p,int at)
  {if(p==null||!Valid(at)||cells[at].Owner!=p.Owner||All.Any(q=>q!=p&&q.Owner==p.Owner&&q.Card.Kind==CardType.Creature&&Position(q.Id)==at))return;foreach(var source in All.Where(q=>q.Owner==p.Owner).ToArray())AbilityEvent(source,"pcu-lone-entry",p,Position(source.Id));}
  void PreconUnitDisembarked(Piece p)=>AbilityEvent(p,"pcu-unboard");
  void PreconUnitEntered(Piece p)
  {if(p.Card.Kind==CardType.Creature)PreconUnitMoved(p,Position(p.Id));if(p.Card.Kind==CardType.Equipment)foreach(var source in All.Where(q=>q.Owner==p.Owner).ToArray())AbilityEvent(source,"pcu-equipment-enter",p,Position(source.Id));}
  void PreconUnitDied(Piece p)
  {if(p.CombatLethal&&preconDeathDraw.TryGetValue(p.Id,out var mod)&&mod.turn==Turn)DrawCard(mod.owner);preconDeathDraw.Remove(p.Id);preconPassengerTemp.Remove(p.Id);preconAttackPermission.Remove(p.Id);}
  readonly HashSet<Pending> preconCombatChosen=new HashSet<Pending>();
  bool PreconUnitCombatChoices(Pending battle,Action resume)
  {
   if(preconCombatChosen.Contains(battle))return false;preconCombatChosen.Add(battle);
   var entries=battle.Attackers.Concat(battle.Blockers).Distinct().Select(Find).Where(p=>p!=null).SelectMany(p=>p.Card.Abilities.Where(a=>a.trigger=="pcu-combat-enter").Select(a=>(source:p,ability:a))).ToArray();
   if(entries.Length==0)return false;
   void ChooseCombat(int index)
   {
    if(index>=entries.Length){resume();return;}var entry=entries[index];var p=entry.source;var a=entry.ability;
    var context=new Pending{Owner=p.Owner,Source=p,SourceCell=Position(p.Id),Target=battle.Target};var opts=All.Where(q=>AbilityTarget(a,context,q)).ToArray();
    if(opts.Length==0){ChooseCombat(index+1);return;}
    Ask(p.Owner,"Apoio ao entrar em combate",opts.Select(q=>new ChoiceOption(q.Id,q.Card.Name)),id=>{var q=Find(id);if(q!=null&&AbilityTarget(a,context,q))foreach(var op in a.ops.Where(op=>op.kind=="buff"))q.Modifiers.Add(new Modifier{Attack=op.a,Defense=op.b,Range=op.c,EndTurn=Turn});ChooseCombat(index+1);});
   }
   ChooseCombat(0);return true;
  }
  void PreconUnitCombatEntered(Piece p,bool defending)
  {
   if(!defending)return;
   foreach(var e in All.Where(q=>q.AttachedTo==p.Id).ToArray())foreach(var a in e.Card.Abilities.Where(a=>a.trigger=="pcu-host-defend"))foreach(var op in a.ops.Where(op=>op.kind=="buff"))p.Modifiers.Add(new Modifier{Attack=op.a,Defense=op.b,Range=op.c,CombatOnly=true,EndTurn=Turn});
  }
  void PreconUnitArmyFinished(int owner,Piece[] attackers)
  {if(attackers.Count(p=>p!=null&&p.Owner==owner&&p.Card.Subtypes.Contains("Goblin"))<2)return;foreach(var p in All.Where(p=>p.Owner==owner).ToArray())AbilityEvent(p,"pcu-goblin-army");}
  void PreconUnitShotDone(Piece shooter)
  {if(shooter!=null&&shooter.Card.Subtypes.Contains("Arqueiro"))foreach(var p in All.Where(q=>q.Owner==shooter.Owner).ToArray())AbilityEvent(p,"pcu-archer-shot",shooter,Position(p.Id));}
  void PreconFreeMove(int owner,Piece p,Action next)
  {
   if(p==null||Find(p.Id)==null){next();return;}
   var opts=Neighbors(Position(p.Id)).Where(at=>cells[at].Terrain!=null&&!Blocked(p,at)&&!cells[at].pieces.Any(q=>Enemies(owner,q.Owner))).ToArray();
   if(opts.Length==0){next();return;}
   Ask(owner,"Destino do movimento gratuito",opts.Select(at=>new ChoiceOption(at,"Terreno "+at%11+","+at/11)),at=>{if(at>=0&&Find(p.Id)!=null&&opts.Contains(at))ApplyDisplacement(owner,p,at,null);next();},true);
  }
  bool ResolvePreconUnitOp(Pending item,CardAbility a,Piece target,AbilityOp op,int player,Action next)
  {
   if(!PreconUnitSchema.Operations.Contains(op.kind))return false;
   int owner=item.Owner,origin=AbilityOrigin(item);bool alive=target!=null&&Find(target.Id)!=null;
   switch(op.kind)
   {
    case "pcu-declared-volley":
     if(preconVolleys.TryGetValue(item,out var volleyPairs)){preconVolleys.Remove(item);foreach(var pair in volleyPairs){var archer=Find(pair.shooter);var victim=Find(pair.victim);if(archer!=null&&victim!=null&&archer.Owner==owner&&Enemies(owner,victim.Owner)&&!Immune(victim,owner)&&Distance(Position(archer.Id),Position(victim.Id))<=archer.Range)DamageTo(victim,op.a,archer,true,owner);}}break;
    case "pcu-token":
     var parts=op.value.Split('|');if(parts.Length!=5)throw new InvalidOperationException("Perfil de ficha inválido");
     var data=new CardData{id="pcu-token-"+parts[0]+"-"+op.b+"-"+op.c+"-"+parts[1]+"-"+parts[2]+"-"+parts[3],name="Ficha "+parts[0],kind="Creature",attack=op.b,defense=op.c,actions=int.Parse(parts[1]),movement=int.Parse(parts[2]),range=int.Parse(parts[3]),subtypes=new[]{parts[0]},art="soldier"};
     var definition=new Definition(data,"generated");
     void SpawnAt(int at){if(!Valid(at)||cells[at].Terrain==null)return;var piece=new Piece(nextPiece++,owner,definition){Game=this,Token=true};cells[at].pieces.Add(piece);Enter(piece);Visual?.Invoke(new MatchEvent("summon",at,at,piece.Id,definition,owner));}
     void Tokens(int left){if(left<=0){next();return;}if(parts[4]=="choose"){var options=Enumerable.Range(0,121).Where(at=>cells[at].Terrain!=null&&cells[at].Owner==owner).ToArray();if(options.Length==0){next();return;}Ask(owner,"Terreno da ficha",options.Select(at=>new ChoiceOption(at,"Terreno "+at%11+","+at/11)),at=>{SpawnAt(at);Tokens(left-1);});}else{SpawnAt(parts[4]=="capital"?Capital(owner,seats.Length):parts[4]=="subject"&&item.Subject!=null?item.Target:origin);Tokens(left-1);}}
     Tokens(op.a);return true;
    case "pcu-free-move":PreconFreeMove(owner,target,next);return true;
    case "pcu-heal-all":foreach(var p in All.Where(q=>q.Owner==owner&&(q.Card.Kind==CardType.Creature||q.Card.Kind==CardType.Construction)))p.Damage=0;break;
    case "pcu-heal-pool":
     var healed=new HashSet<int>();void Heal(int amount,int left){var options=All.Where(p=>p.Owner==owner&&p.Damage>0&&(p.Card.Kind==CardType.Creature||p.Card.Kind==CardType.Construction)&&!healed.Contains(p.Id)).ToArray();if(amount<=0||left<=0||options.Length==0){next();return;}Ask(owner,"Distribuir cura",options.Select(p=>new ChoiceOption(p.Id,p.Card.Name)),id=>{if(id<0){next();return;}var p=Find(id);if(p==null){next();return;}int max=Math.Min(amount,p.Damage);Ask(owner,"Quantidade de cura",Enumerable.Range(1,max).Select(n=>new ChoiceOption(n,n.ToString())),n=>{p.Damage-=n;healed.Add(id);Heal(amount-n,left-1);});},true);}Heal(op.a,op.b);return true;
    case "pcu-fight":
     var used=new HashSet<int>();var fightPairs=new List<(int own,int enemy)>();
     void FinishFight(){var hits=fightPairs.Select(pair=>(p:Find(pair.own),q:Find(pair.enemy))).Where(pair=>pair.p!=null&&pair.q!=null&&Distance(Position(pair.p.Id),Position(pair.q.Id))<=1).Select(pair=>(p:pair.p,q:pair.q,pa:pair.p.Attack,qa:pair.q.Attack)).ToArray();var applied=new List<(Piece target,int amount,Piece source)>();foreach(var hit in hits){int before=hit.q.Damage;DamageTo(hit.q,hit.pa,hit.p,false,owner);applied.Add((hit.q,Math.Max(0,hit.q.Damage-before),hit.p));before=hit.p.Damage;DamageTo(hit.p,hit.qa,hit.q,false,hit.q.Owner);applied.Add((hit.p,Math.Max(0,hit.p.Damage-before),hit.q));}foreach(var hit in applied)AfterDamage(hit.target,hit.amount,hit.source);StateCheck();next();}
     void Fight(int left){var own=All.Where(p=>p.Owner==owner&&p.Card.Kind==CardType.Creature&&!used.Contains(p.Id)&&All.Any(q=>Enemies(owner,q.Owner)&&q.Card.Kind==CardType.Creature&&!used.Contains(q.Id)&&Distance(Position(p.Id),Position(q.Id))<=1&&!Immune(q,owner))).ToArray();if(left<=0||own.Length==0){FinishFight();return;}Ask(owner,"Criatura sua para lutar",own.Select(p=>new ChoiceOption(p.Id,p.Card.Name)),id=>{var p=Find(id);if(p==null){FinishFight();return;}var enemies=All.Where(q=>Enemies(owner,q.Owner)&&q.Card.Kind==CardType.Creature&&!used.Contains(q.Id)&&Distance(Position(p.Id),Position(q.Id))<=1&&!Immune(q,owner)).ToArray();if(enemies.Length==0){FinishFight();return;}Ask(owner,"Criatura inimiga para lutar",enemies.Select(q=>new ChoiceOption(q.Id,q.Card.Name)),eid=>{var q=Find(eid);if(q!=null&&Find(p.Id)!=null){used.Add(p.Id);used.Add(q.Id);fightPairs.Add((p.Id,q.Id));}Fight(left-1);});},true);}Fight(op.a);return true;
    case "pcu-shot":
     var shooter=target??item.Source;int at=shooter!=null?Position(shooter.Id):origin;int range=op.b>0?op.b:shooter?.Range??1;
     var shotTargets=All.Where(p=>p.Card.Kind==CardType.Creature&&Enemies(owner,p.Owner)&&Distance(at,Position(p.Id))<=range&&!Immune(p,owner)).ToArray();
     var optionsShot=shotTargets.Select(p=>new ChoiceOption(p.Id,p.Card.Name)).ToList();if(op.c>0)optionsShot.AddRange(Enumerable.Range(0,seats.Length).Where(e=>Enemies(owner,e)&&Distance(at,Capital(e,seats.Length))<=range).Select(e=>new ChoiceOption(-100-e,seats[e].Name+" (capital)")));
     if(optionsShot.Count==0){next();return true;}
     Ask(owner,"Alvo do tiro",optionsShot,id=>{if(shooter!=null&&Find(shooter.Id)==null){next();return;}int dealt=0;if(id<=-100){int victimOwner=-100-id;int before=seats[victimOwner].Life;LoseLife(victimOwner,op.a);dealt=Math.Max(0,before-seats[victimOwner].Life);}else{var victim=Find(id);if(victim!=null){int before=victim.Damage;DamageTo(victim,op.a,shooter,true,owner);dealt=Math.Max(0,victim.Damage-before);}}if(shooter!=null&&op.value=="noAttack")shooter.Modifiers.Add(new Modifier{NoAttack=true,EndTurn=Turn});if(a.trigger=="activate"&&dealt>0)PreconUnitShotDone(shooter);next();});return true;
    case "pcu-allow-attack":if(alive)preconAttackPermission[target.Id]=Turn;break;
    case "pcu-sacrifice-reward":
     var sacrificed=new HashSet<int>();void Sac(int left){var options=All.Where(p=>p.Owner==owner&&p.Card.Kind==CardType.Creature&&!sacrificed.Contains(p.Id)).ToArray();if(left<=0||options.Length==0){next();return;}Ask(owner,"Sacrificar para comprar e curar",options.Select(p=>new ChoiceOption(p.Id,p.Card.Name)),id=>{var p=Find(id);if(p==null){next();return;}sacrificed.Add(id);SacrificePiece(p);DrawCard(owner);GainLife(owner,op.b);Sac(left-1);},true);}Sac(op.a);return true;
    case "pcu-draw-on-combat-death":if(alive)preconDeathDraw[target.Id]=(Turn,owner);break;
    case "pcu-passenger-bonus":break;
    case "pcu-heal-passengers":if(alive)target.Damage=Math.Max(0,target.Damage-op.a*Passengers(target.Id).Count);break;
    case "pcu-disembark":if(alive){var ps=Passengers(target.Id).Where(p=>p.Owner==owner).ToArray();var selected=new HashSet<int>();void Unboard(int left){var opts=ps.Where(p=>Find(p.Id)!=null&&p.CarrierId==target.Id&&!selected.Contains(p.Id)).ToArray();if(left<=0||opts.Length==0){next();return;}Ask(owner,"Desembarcar gratuitamente",opts.Select(p=>new ChoiceOption(p.Id,p.Card.Name)),id=>{var p=Find(id);if(p==null){next();return;}selected.Add(id);p.CarrierId=-1;PreconUnitDisembarked(p);Unboard(left-1);},true);}Unboard(op.a);return true;}break;
    case "pcu-buff-passengers":if(alive)foreach(var p in Passengers(target.Id))Buff(p,op.a,op.b);break;
    case "pcu-passenger-temp":if(alive){if(preconPassengerTemp.TryGetValue(target.Id,out var old)&&old.turn==Turn)preconPassengerTemp[target.Id]=(Turn,old.attack+op.a,old.defense+op.b);else preconPassengerTemp[target.Id]=(Turn,op.a,op.b);}break;
    case "pcu-return-equipment":
     var chosen=new HashSet<int>();void Return(int left){var opts=All.Where(p=>p.Owner==owner&&p.Card.Kind==CardType.Equipment&&!chosen.Contains(p.Id)).ToArray();if(left<=0||opts.Length==0){next();return;}Ask(owner,"Equipamento para devolver",opts.Select(p=>new ChoiceOption(p.Id,p.Card.Name)),id=>{var p=Find(id);if(p==null){next();return;}chosen.Add(id);p.AttachedTo=-1;ReturnHand(p);Return(left-1);},true);}Return(op.a);return true;
   }
   next();return true;
  }
 }
}
