using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
 public sealed class ExilePermission
 {
  public int Id {get;internal set;} public int Owner {get;internal set;} public int CardOwner {get;internal set;}
  public Definition Card {get;internal set;} public bool AnyMana {get;internal set;}
 }
 public sealed partial class Match
 {
  readonly List<ExilePermission> exilePermissions=new List<ExilePermission>();
  readonly Dictionary<int,int> exhaustedAt=new Dictionary<int,int>();
  readonly HashSet<int> reflections=new HashSet<int>();
  readonly Dictionary<int,List<Piece>> dyingEnchantments=new Dictionary<int,List<Piece>>();
  int nextPermission;
  public IReadOnlyList<ExilePermission> ExiledFor(int owner)=>exilePermissions.Where(x=>x.Owner==owner).ToArray();
  bool SpellCard(Definition c)=>c.Playable&&(c.Kind==CardType.Spell||c.Kind==CardType.Instant);
  internal void ActionsSpent(Piece p,int before,int after)
  {
   if(before<=0||after!=0||p.Card.Rule!="end-messenger"||Find(p.Id)==null||exhaustedAt.TryGetValue(p.Id,out int turn)&&turn==Turn)return;
   exhaustedAt[p.Id]=Turn;work.Enqueue(()=>Trigger(p,"end-messenger"));
  }
  bool NewCanActivate(Piece p)=>p.Card.Rule=="abyss-hunger"?p.Actions>=1&&seats[p.Owner].mana.Sum()>0:
   p.Card.Rule=="moon-admirer"?p.Actions>=1&&seats[p.Owner].grave.Any(SpellCard):
   p.Card.Rule=="ancestral-monk"?p.Actions>=1&&All.Any(q=>q.Card.Kind==CardType.Creature&&!Immune(q,p.Owner)&&!reflections.Contains(q.Id)):
   p.Card.Rule=="infiltration"?p.Actions>=2&&All.Any(q=>q.Owner==p.Owner&&q.Card.Kind==CardType.Creature&&Enumerable.Range(0,121).Any(at=>InfiltrationTarget(q,at))):false;
  bool MonkResponse(Piece p)=>p!=null&&p.Card.Rule=="ancestral-monk"&&p.Owner==Priority&&CanAct(p)&&NewCanActivate(p)&&Choice==null&&Defense==null&&stack.Count>0;
  bool NewActivate(Piece p)
  {
   if(!new[]{"abyss-hunger","moon-admirer","ancestral-monk","infiltration"}.Contains(p.Card.Rule))return false;
   p.Actions-=p.Card.Rule=="infiltration"?2:1;
   int amount=0;if(p.Card.Rule=="abyss-hunger"){amount=seats[p.Owner].mana.Sum();Array.Clear(seats[p.Owner].mana,0,7);}
   Trigger(p,p.Card.Rule,repeatByAcademy:false);stack.Last().Amount=amount;return true;
  }
  int NewBonus(Piece p,int stat)
  {
   if(stat>1)return 0;int bonus=0;
   if(p.Card.Subtypes.Contains("Cavaleiro"))bonus+=All.Count(q=>q.Card.Rule=="radiant-knight"&&Allied(q.Owner,p.Owner))*All.Count(q=>q.Card.Subtypes.Contains("Cavaleiro")&&Allied(q.Owner,p.Owner)&&Position(q.Id)==Position(p.Id));
   if(p.Card.Rule=="curse-collector")bonus+=All.Count(q=>q.Card.Kind==CardType.Enchantment&&q.AttachedTo==p.Id);
   foreach(var q in All.Where(q=>q.AttachedTo==p.Id&&q.Card.Kind==CardType.Enchantment))bonus+=q.Card.Rule=="test-blessing"?1:q.Card.Rule=="test-curse"?-1:0;
   return bonus;
  }
  internal void GainLife(int owner,int amount)
  {
   if(amount<=0||seats[owner].Eliminated)return;seats[owner].Life+=amount;
   foreach(var p in All.Where(p=>p.Owner==owner&&p.Card.Rule=="moon-herald").ToArray())Trigger(p,"moon-herald");
  }
  void NewAfterDamage(Piece source,int amount)
  {if(source!=null&&amount>0&&source.Card.Rule=="burning-ancestor")Trigger(source,"burning-ancestor");}
  int ReflectDamage(Piece target,int amount,Piece source,int caster)
  {
   if(amount<=0||!reflections.Remove(target.Id))return amount;int reflected=amount/2;
   if(reflected>0)work.Enqueue(()=>{if(source!=null&&Find(source.Id)!=null)DamageTo(source,reflected,null,true,target.Owner);else if(caster>=0)DamageCapital(caster,reflected,caster:target.Owner);});
   return amount-reflected;
  }
  void NewEnter(Piece p)
  {
   if(p.Card.Kind!=CardType.Equipment)return;
   foreach(var knight in All.Where(q=>q.Owner==p.Owner&&q.Card.Rule=="thousand-beasts").ToArray())Trigger(knight,"thousand-beasts",p,Position(p.Id));
  }
  void NewBeforeDeath(Piece p,int killer)
  {
   var enchants=All.Where(q=>q.AttachedTo==p.Id&&q.Card.Kind==CardType.Enchantment).ToList();
   if(enchants.Count==0)return;
   dyingEnchantments[p.Id]=enchants;
   // Damage source, not merely its controller, must be the collector.
   if(lastDamageSource.TryGetValue(p.Id,out var source)&&source.Card.Rule=="curse-collector"&&source.Owner==killer&&source.Actions>=1)
    Trigger(source,"curse-steal",p,Position(p.Id));
  }
  readonly Dictionary<int,Piece> lastDamageSource=new Dictionary<int,Piece>();
  bool InfiltrationTarget(Piece p,int at)=>Valid(at)&&cells[at].Terrain!=null&&!Blocked(p,at)&&!cells[at].pieces.Any(q=>q.Card.Kind==CardType.Creature||q.Card.IsVehicle);
  bool NewResolve(Pending item)
  {
   int o=item.Owner;var src=item.Source;
   switch(item.Rule)
   {
    case "abyss-hunger":if(src!=null&&Find(src.Id)!=null)Buff(src,item.Amount,item.Amount);return true;
    case "end-messenger":
     PickHand(o,"Mensageiro: criatura grátis",c=>c.Playable&&c.Kind==CardType.Creature,c=>{
      var destinations=Enumerable.Range(0,121).Where(at=>cells[at].Owner==o&&cells[at].Terrain!=null&&!Blocked(new Piece(-1,o,c){Game=this},at)).ToArray();
      if(destinations.Length==0){seats[o].hand.Add(c);return;}
      Ask(o,"Terreno do seu reino",destinations.Select(at=>new ChoiceOption(at,"Tile "+at%11+","+at/11)),at=>SpawnNew(o,c,at));
     },true);return true;
    case "burning-ancestor":SelectGraveCast(o,true);return true;
    case "moon-admirer":SelectGraveCast(o,false);return true;
    case "ancestral-monk":Pick(o,"Proteger do próximo dano e refletir metade",p=>p.Card.Kind==CardType.Creature&&!Immune(p,o)&&!reflections.Contains(p.Id),p=>reflections.Add(p.Id));return true;
    case "moon-herald":
     int origin=src!=null&&Position(src.Id)>=0?Position(src.Id):item.SourceCell;
     var targets=All.Where(p=>(p.Card.Kind==CardType.Creature||p.Card.Kind==CardType.Construction)&&Distance(origin,Position(p.Id))<=3&&!Immune(p,o)).ToArray();
     Ask(o,"Arauto: 1 dano até 3 tiles",targets.Select(p=>new ChoiceOption(p.Id,p.Card.Name)).Concat(Enumerable.Range(0,seats.Length).Where(s=>!seats[s].Eliminated&&Distance(origin,Capital(s,seats.Length))<=3).Select(s=>new ChoiceOption(-10-s,"Capital "+seats[s].Name))),key=>{if(key<=-10){DamageCapital(-10-key,1,src,o);Preview(item,Capital(-10-key,seats.Length));}else{var p=Find(key);if(p!=null){Preview(item,Position(p.Id));DamageTo(p,1,src,true,o);}}});return true;
    case "infiltration":
     Pick(o,"Criatura sua para infiltrar",p=>p.Owner==o&&p.Card.Kind==CardType.Creature&&Enumerable.Range(0,121).Any(at=>InfiltrationTarget(p,at)),p=>
      Ask(o,"Terreno andável sem criaturas",Enumerable.Range(0,121).Where(at=>InfiltrationTarget(p,at)).Select(at=>new ChoiceOption(at,"Tile "+at%11+","+at/11)),at=>{
       if(Find(p.Id)==null||!InfiltrationTarget(p,at))return;if(GuardInvasion(p,at))return;
       int from=Position(p.Id);ReleasePassenger(p);cells[from].pieces.Remove(p);cells[at].pieces.Add(p);MarkMoved(p,from,at);Visual?.Invoke(new MatchEvent("move",from,at,p.Id,p.Card,p.Owner));Preview(item,at);
      }));return true;
    case "thousand-beasts":CreateBeast(item);return true;
    case "curse-steal":
     if(src==null||Find(src.Id)==null||src.Actions<1||item.Subject==null||!dyingEnchantments.TryGetValue(item.Subject.Id,out var stolen))return true;
     Ask(o,"Pagar 1 PA para roubar os encantamentos?",new[]{new ChoiceOption(1,"Roubar todos")},key=>{
      if(key!=1||Find(src.Id)==null||src.Actions<1)return;
      var valid=stolen.Where(p=>seats[p.OriginalOwner].grave.Contains(p.Card)).ToArray();if(valid.Length==0)return;src.Actions--;
      foreach(var old in valid){seats[old.OriginalOwner].grave.Remove(old.Card);var e=SpawnNew(o,old.Card,Position(src.Id),old.OriginalOwner);e.AttachedTo=src.Id;}
     },true);return true;
    case "test-counter":var countered=stack.LastOrDefault(x=>x.Rule==null&&x.Card!=null&&(x.Card.Kind==CardType.Spell||x.Card.Kind==CardType.Instant));if(countered!=null){stack.Remove(countered);FinishSpell(countered);StackResult(countered,true);}return true;
    case "test-mill":Ask(o,"Grimório alvo",Enumerable.Range(0,seats.Length).Where(s=>!seats[s].Eliminated).Select(s=>new ChoiceOption(s,seats[s].Name)),s=>MillCards(s,2));return true;
    case "test-blessing":case "test-curse":return true;
    default:return false;
   }
  }
  Piece SpawnNew(int owner,Definition card,int at,int original=-1)
  {
   var p=new Piece(nextPiece++,owner,card){Game=this,OriginalOwner=original<0?owner:original};cells[at].pieces.Add(p);Enter(p);Visual?.Invoke(new MatchEvent("summon",at,at,p.Id,card,owner));return p;
  }
  void CreateBeast(Pending item)
  {
   var equipment=item.Subject;if(equipment==null||Find(equipment.Id)==null)return;int o=item.Owner,at=Position(equipment.Id);
   Ask(o,"Ficha que receberá "+equipment.Card.Name,new[]{new ChoiceOption(0,"Urso 3/3 — missão"),new ChoiceOption(1,"Dinossauro 4/2 — missão"),new ChoiceOption(2,"Panda 2/4 — sem rota")},choice=>{
    if(Find(equipment.Id)==null)return;
    void Spawn(int enemy){
     var d=new Definition(new CardData{id="token-beast-"+choice,name=choice==0?"Urso da missão":choice==1?"Dinossauro da missão":"Urso Panda",kind="Creature",color=choice==1?5:0,attack=choice==0?3:choice==1?4:2,defense=choice==0?3:choice==1?2:4,actions=1,movement=2,art="beast",subtypes=new[]{choice==1?"Dinossauro":"Urso"}}, "generated");
     int where=Position(equipment.Id);if(where<0)return;var token=SpawnNew(o,d,where);token.Token=true;if(enemy>=0)missions[token.Id]=enemy;
     var old=Find(equipment.AttachedTo);if(old!=null&&Has(equipment,"equipment-pa"))old.Actions=Math.Max(0,old.Actions-1);
     equipment.AttachedTo=token.Id;if(Has(equipment,"equipment-pa"))token.Actions++;
    }
    if(choice==2)Spawn(-1);else Ask(o,"Capital da missão",Enumerable.Range(0,seats.Length).Where(s=>Enemies(o,s)).Select(s=>new ChoiceOption(s,seats[s].Name)),Spawn);
   });
  }
  void SelectGraveCast(int owner,bool free)
  {
   work.Enqueue(()=>{
    var cards=seats[owner].grave.Where(c=>SpellCard(c)&&(!free||(c.Rule!="destiny"&&ResponseTargets(c,owner)&&(!c.Effects.Any(e=>effects.Get(e.Operation).NeedsEnemy)||All.Any(p=>Enemies(owner,p.Owner)))))).Distinct().ToArray();
    Ask(owner,free?"Ancestral: conjurar gratuitamente":"Admirador: exilar e permitir conjuração",cards.Select((c,n)=>new ChoiceOption(n,c.Name)),n=>{
     if(n<0||n>=cards.Length||!seats[owner].grave.Remove(cards[n]))return;var card=cards[n];seats[owner].exile.Add(card);
     if(!free){exilePermissions.Add(new ExilePermission{Id=nextPermission++,Owner=owner,CardOwner=owner,Card=card});return;}
     QueueForeignCast(owner,owner,card,true,()=>seats[owner].exile.Remove(card));
    },true);
   });
  }
  void QueueForeignCast(int owner,int original,Definition card,bool exileAfter,Action commit)
  {
   void Cast(int at,int unit){commit();stack.Add(new Pending{Owner=owner,CardOwner=original,Card=card,Target=at,TargetUnit=unit,ExileAfter=exileAfter});passes=0;ReflectDeclaredTarget(stack.Last());AuthorPlayed(owner,card);Visual?.Invoke(new MatchEvent("cast",Capital(owner,seats.Length),Capital(owner,seats.Length),-1,card,owner));}
   if(card.Kind==CardType.Enchantment)Ask(owner,"Criatura para encantar",All.Where(p=>p.Card.Kind==CardType.Creature&&!Immune(p,owner)).Select(p=>new ChoiceOption(p.Id,p.Card.Name)),id=>Cast(Position(id),id));
   else if(card.Permanent)Ask(owner,"Terreno para a carta exilada",Enumerable.Range(0,121).Where(at=>cells[at].Terrain!=null&&cells[at].Owner==owner).Select(at=>new ChoiceOption(at,"Tile "+at%11+","+at/11)),at=>Cast(at,-1));
   else if(card.Effects.Any(e=>effects.Get(e.Operation).NeedsEnemy))Ask(owner,"Alvo da magia",All.Where(p=>Enemies(owner,p.Owner)).Select(p=>new ChoiceOption(p.Id,p.Card.Name)),id=>Cast(Position(id),id));
   else Cast(-1,-1);
  }
  public bool CanCastExiled(int id)
  {if(IsRemoteView)return NetCan("exile",id);
   var x=exilePermissions.FirstOrDefault(p=>p.Id==id&&p.Owner==Priority);
   if(x==null||Over||Choice!=null||Defense!=null||!x.Card.Playable||x.Card.Rule=="destiny")return false;
   if(x.Card.Kind==CardType.Instant&&!ResponseTargets(x.Card,Priority))return false;
   if(x.Card.Kind!=CardType.Instant&&(Phase!=Stage.Main||Priority!=Active||stack.Count>0))return false;
   if(x.Card.Kind==CardType.Instant&&Phase!=Stage.Main&&Phase!=Stage.End&&stack.Count==0)return false;
   if(!(x.AnyMana?seats[Priority].mana.Sum()>=x.Card.TotalCost:CanPay(Priority,x.Card)))return false;
   if(x.Card.Kind==CardType.Enchantment)return All.Any(p=>p.Card.Kind==CardType.Creature&&!Immune(p,Priority));
   if(x.Card.Permanent)return cells.Any(c=>c.Terrain!=null&&c.Owner==Priority);
   return !x.Card.Effects.Any(e=>effects.Get(e.Operation).NeedsEnemy)||All.Any(p=>Enemies(Priority,p.Owner));
  }
  void CastExiled(int id)
  {
   Check(CanCastExiled(id),"Carta exilada indisponível.");var x=exilePermissions.First(p=>p.Id==id);
   QueueForeignCast(x.Owner,x.CardOwner,x.Card,false,()=>{if(x.AnyMana)SpendGeneric(x.Owner,x.Card.TotalCost);else Pay(x.Owner,x.Card);seats[x.CardOwner].exile.Remove(x.Card);exilePermissions.Remove(x);});
  }
  internal void MillCards(int owner,int count)
  {
   for(int n=0;n<count&&seats[owner].main.Count>0;n++){
    var card=Pop(seats[owner].main);seats[owner].grave.Add(card);if(card.Kind==CardType.Terrain||!card.Playable)continue;
    foreach(var thief in All.Where(p=>p.Card.Rule=="crypt-thief").ToArray()){
     var captured=card;int who=owner;work.Enqueue(()=>{
      int o=thief.Owner;if(Find(thief.Id)==null||!seats[who].grave.Contains(captured)||TroopPA(o)<captured.TotalCost)return;
      Ask(o,"Ladrão: exilar "+captured.Name+" por "+captured.TotalCost+" PA?",new[]{new ChoiceOption(1,"Pagar com tropas")},key=>{
       if(key!=1||!seats[who].grave.Contains(captured))return;PayTroopPA(o,captured.TotalCost,()=>{if(!seats[who].grave.Remove(captured))return;seats[who].exile.Add(captured);exilePermissions.Add(new ExilePermission{Id=nextPermission++,Owner=o,CardOwner=who,Card=captured,AnyMana=true});});
      },true);
     });
    }
   }
  }
  int TroopPA(int owner)=>All.Where(p=>p.Owner==owner&&p.Card.Kind==CardType.Creature&&CanAct(p)).Sum(p=>p.Actions);
  void PayTroopPA(int owner,int remaining,Action done)
  {
   if(remaining<=0){done();return;}
   Ask(owner,"Escolha tropa: faltam "+remaining+" PA",All.Where(p=>p.Owner==owner&&p.Card.Kind==CardType.Creature&&CanAct(p)&&p.Actions>0).Select(p=>new ChoiceOption(p.Id,p.Card.Name+" · "+p.Actions+" PA")),id=>{var p=Find(id);if(p==null)return;int spent=Math.Min(remaining,p.Actions);p.Actions-=spent;PayTroopPA(owner,remaining-spent,done);});
  }
  void FinishSpell(Pending item)
  {
   int owner=item.CardOwner<0?item.Owner:item.CardOwner;
   if(item.ExileAfter)seats[owner].exile.Add(item.Card);else seats[owner].grave.Add(item.Card);
  }
 }
}
