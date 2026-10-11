using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
 public static class PreconAuthorSchema
 {
  public static readonly HashSet<string> Operations=new HashSet<string>{"pca-look-player","pca-mill-enemies","pca-swap-combat","pca-mark-tile","pca-wave","pca-block-tile","pca-exile-grave-mill","pca-document","pca-horror","pca-exumar","pca-symphony","pca-persuade","pca-realm-return","pca-mill-victim","pca-mill-killer","pca-top-keep-mill","pca-enemy-mill","pca-mill-damage","pca-bottom-recover","pca-secret-exile","pca-sacrifice-mill","pca-cross-pieces","pca-grave-other","pca-catacombs-return"};
  public static readonly HashSet<string> Conditions=new HashSet<string>{"pca-any-grave-creature","pca-own-grave-creature","pca-range-two"};
  public static readonly HashSet<string> Triggers=new HashSet<string>{"pca-near-terrain","pca-tile-attacked","pca-killed","pca-own-other-death"};
  public static readonly HashSet<string> Targets=new HashSet<string>{"pca-combat-own","pca-persuade","pca-allied"};
  public static readonly HashSet<string> Traits=new HashSet<string>{"pca-document","pca-factory","pca-jonas","pca-exumar","pca-tile-enchantment","pca-realm-dead","pass-units","pca-diagonal-only"};
 }
 public sealed partial class Match
 {
  readonly List<(int owner,int tile,int turn)> preconMarks=new List<(int,int,int)>();
  readonly Dictionary<int,(int owner,int turn)> preconTileBlocks=new Dictionary<int,(int,int)>();
  readonly Dictionary<int,(int stat,int amount)> preconDocuments=new Dictionary<int,(int,int)>();
  readonly Dictionary<int,int> preconFactoryCounters=new Dictionary<int,int>();
  readonly Dictionary<int,int> preconReturnedAtEnd=new Dictionary<int,int>();
  readonly Dictionary<int,int> preconMillDamage=new Dictionary<int,int>();
  readonly Dictionary<Definition,int> preconSecretExile=new Dictionary<Definition,int>();
  bool? PreconAuthorCondition(string condition,Piece p,int owner,int at,int target)
  {
   switch(condition){case "pca-any-grave-creature":return seats.Any(s=>s.grave.Any(c=>c.Kind==CardType.Creature));case "pca-own-grave-creature":return seats[owner].grave.Any(c=>c.Kind==CardType.Creature);case "pca-range-two":return p!=null;default:return null;}
  }
  bool? PreconAuthorTarget(CardAbility a,Pending item,Piece p,bool passive)
  {
   if(!PreconAuthorSchema.Targets.Contains(a.target))return null;
   if(p==null||p.Card.Kind!=CardType.Creature||p.Card.TotalCost>a.maxCost)return false;
   int at=AbilityOrigin(item),pos=Position(p.Id);
   bool type=a.target=="pca-combat-own"?p.Owner==item.Owner&&stack.Any(b=>b.Card==null&&(b.Attackers.Contains(p.Id)||b.Blockers.Contains(p.Id))):a.target=="pca-persuade"?Enemies(item.Owner,p.Owner)&&Distance(Capital(item.Owner,seats.Length),pos)<=3:Allied(item.Owner,p.Owner);
   if(!type||a.scope=="adjacent"&&!Adjacent(at,pos)||a.scope=="here"&&at!=pos||a.scope=="hereOrAdjacent"&&at!=pos&&!Adjacent(at,pos)||a.condition=="pca-range-two"&&Distance(at,pos)>2)return false;
   return (passive||!Immune(p,item.Owner))&&AbilityCondition(a.condition,p,item.Owner,pos,item.Target);
  }
  int PreconAuthorBonus(Piece p,int stat)
  {
   int value=0;
   if(stat==3&&p.Card.Traits.Contains("pca-jonas"))value+=seats.Sum(s=>s.grave.Where(c=>c.Kind==CardType.Creature).Select(c=>c.TotalCost).DefaultIfEmpty(0).Max())-p.Card.Actions;
   if(stat==0||stat==1){int at=Position(p.Id);if(p.Card.Kind==CardType.Creature&&Valid(at)){int copies=cells[at].pieces.Count(e=>e.Card.Traits.Contains("pca-tile-enchantment"));value-=copies*seats[p.Owner].grave.Count(c=>c.Kind==CardType.Creature);}
    foreach(var e in All.Where(e=>e.AttachedTo==p.Id&&e.Card.Traits.Contains("pca-document")))if(preconDocuments.TryGetValue(e.Id,out var d)&&d.stat==stat)value+=d.amount;
   }
   return value;
  }
  bool? PreconAuthorEnchantmentPlacement(Definition card,int owner,int tile,int unit)
  {if(!card.Traits.Contains("pca-tile-enchantment"))return null;return Valid(tile)&&cells[tile].Terrain!=null&&unit<0;}
  bool PreconAuthorMoveBlocked(Piece p,int tile)=>p!=null&&(preconTileBlocks.ContainsKey(tile)||preconTileBlocks.ContainsKey(Position(p.Id)));
  bool PreconAuthorCanCrossPieces(Piece p)=>p!=null&&p.Card.Traits.Contains("pass-units");
  bool PreconAuthorExileVisible(Definition card,int viewer)=>!preconSecretExile.TryGetValue(card,out int owner)||viewer==owner;
  void PreconAuthorRevealExile(Definition card)=>preconSecretExile.Remove(card);
  void PreconAuthorEntered(Piece p){if(p.Card.Traits.Contains("pca-factory"))preconFactoryCounters[p.Id]=0;}
  void PreconAuthorDied(Piece p,int at,int killer)
  {
   if(killer>=0&&killer<seats.Length)AbilityEvent(p,"pca-killed",cell:at,target:killer);
   if(p.Card.Kind==CardType.Creature)foreach(var listener in All.Where(q=>q!=p&&q.Owner==p.Owner).ToArray())AbilityEvent(listener,"pca-own-other-death",p,Position(listener.Id));
   if(p.Card.Kind==CardType.Creature)foreach(var source in All.Where(q=>q.Card.Traits.Contains("pca-factory")).ToArray())preconFactoryCounters[source.Id]=(preconFactoryCounters.TryGetValue(source.Id,out int count)?count:0)+1;
   preconDocuments.Remove(p.Id);preconFactoryCounters.Remove(p.Id);preconReturnedAtEnd.Remove(p.Id);preconMillDamage.Remove(p.Id);
  }
  void PreconAuthorTurn()
  {
   foreach(var pair in preconTileBlocks.Where(p=>p.Value.owner==Active&&Turn>p.Value.turn).ToArray())preconTileBlocks.Remove(pair.Key);
   // Remove the previous roll before sampling the current attribute; the bonus lasts until the next roll.
   foreach(var e in All.Where(q=>q.Owner==Active&&q.Card.Traits.Contains("pca-document")).ToArray()){preconDocuments.Remove(e.Id);var host=Find(e.AttachedTo);if(host!=null){int stat=authorRandom.Next(2);preconDocuments[e.Id]=(stat,Math.Max(0,stat==0?host.Attack:host.Defense));}}
  }
  void PreconAuthorEndTurn()
  {
   preconMarks.RemoveAll(m=>m.turn<=Turn);
   foreach(var pair in preconReturnedAtEnd.Where(p=>p.Value<=Turn).ToArray()){preconReturnedAtEnd.Remove(pair.Key);var piece=Find(pair.Key);if(piece!=null)CommitKill(piece);}
   foreach(var pair in preconMillDamage.Where(p=>p.Value<=Turn).ToArray())preconMillDamage.Remove(pair.Key);
  }
  void PreconAuthorMoved(Piece p,int from,int to){}
  void PreconAuthorTerrainPlaced(int owner,int tile)
  {
   foreach(var source in All.Where(p=>Adjacent(Position(p.Id),tile)).ToArray())AbilityEvent(source,"pca-near-terrain",cell:Position(source.Id),target:tile);
   foreach(var mark in preconMarks.Where(m=>m.owner==owner&&m.tile==tile&&m.turn==Turn).ToArray()){preconMarks.Remove(mark);DrawCard(owner);DrawCard(owner);}
  }
  void PreconAuthorAttacked(int owner,int tile)
  {
   if(!Valid(tile)||cells[tile].Owner<0||!Enemies(owner,cells[tile].Owner)||cells[tile].Terrain==null||!cells[tile].Terrain.Traits.Contains("pca-realm-dead")||cells[tile].pieces.Any(p=>p.Card.Kind==CardType.Creature))return;
   var card=cells[tile].Terrain;int defender=cells[tile].Owner;
   for(int i=0;i<card.Abilities.Count;i++){var a=card.Abilities[i];if(a.trigger!="pca-tile-attacked")continue;string key=AbilityKey(null,tile,i);if(a.once&&abilityUses.TryGetValue(key,out int turn)&&turn==Turn)continue;abilityUses[key]=Turn;QueueAbility(card,defender,null,tile,i,target:tile);}
  }
  void PreconAuthorCombatDamage(Piece source,Piece victim,int amount)
  {if(source!=null&&victim!=null&&victim.Card.Kind==CardType.Creature&&amount>0&&preconMillDamage.TryGetValue(source.Id,out int until)&&until==Turn)MillCards(victim.Owner,amount);}
  bool ResolvePreconAuthorOp(Pending item,CardAbility a,Piece target,AbilityOp op,int player,Action next)
  {
   if(!PreconAuthorSchema.Operations.Contains(op.kind))return false;int owner=item.Owner,origin=AbilityOrigin(item);
   switch(op.kind)
   {
    case "pca-look-player":if(player>=0&&player<seats.Length&&seats[player].main.Count>0)Ask(owner,"Topo de "+seats[player].Name+": "+seats[player].main.Last().Name,new[]{new ChoiceOption(0,"Continuar")},i=>next());else next();return true;
    case "pca-mill-enemies":foreach(int enemy in Enumerable.Range(0,seats.Length).Where(e=>Enemies(owner,e)&&!seats[e].Eliminated))MillCards(enemy,op.a);break;
    case "pca-enemy-mill":PreconLibraryEnemy(owner,"Oponente para triturar",enemy=>{MillCards(enemy,op.a);next();},next);return true;
    case "pca-swap-combat":PreconSwapCombat(owner,target,next);return true;
    case "pca-mark-tile":Ask(owner,"Marcar terreno a colocar neste turno",Enumerable.Range(0,121).Select(at=>new ChoiceOption(at,"Tile "+at%11+","+at/11)),at=>{preconMarks.Add((owner,at,Turn));next();});return true;
    case "pca-wave":Ask(owner,"Centro das ondas",Enumerable.Range(0,121).Where(at=>cells[at].Terrain!=null).Select(at=>new ChoiceOption(at,"Terreno "+at%11+","+at/11)),at=>{PreconWave(owner,at,item);next();});return true;
    case "pca-block-tile":var neighbors=Neighbors(origin).ToArray();if(neighbors.Length==0){next();return true;}Ask(owner,"Tile adjacente sem movimento até seu próximo turno",neighbors.Select(at=>new ChoiceOption(at,"Tile "+at%11+","+at/11)),at=>{preconTileBlocks[at]=(owner,Turn);next();});return true;
    case "pca-exile-grave-mill":var dead=seats.SelectMany((s,who)=>s.grave.Where(c=>c.Kind==CardType.Creature).Select(c=>(who,card:c))).ToArray();if(dead.Length==0){next();return true;}Ask(owner,"Criatura para exilar",dead.Select((x,i)=>new ChoiceOption(i,x.card.Name+" · "+seats[x.who].Name)),i=>{var x=dead[i];if(seats[x.who].grave.Remove(x.card)){seats[x.who].exile.Add(x.card);PreconLibraryGraveLeave(owner);MillCards(x.who,op.a);}next();});return true;
    case "pca-horror":int size=item.Source!=null&&preconFactoryCounters.TryGetValue(item.Source.Id,out int count)?count:0;var foes=Enumerable.Range(0,seats.Length).Where(e=>Enemies(owner,e)&&!seats[e].Eliminated).ToArray();if(foes.Length==0||item.Source==null||Find(item.Source.Id)==null){next();return true;}Ask(owner,"Capital inimiga do Horror",foes.Select(e=>new ChoiceOption(e,seats[e].Name)),enemy=>{if(Find(item.Source.Id)!=null){var horror=new Definition(new CardData{id="pca-horror-"+size,name="Horror",kind="Creature",attack=size,defense=size,actions=1,movement=2,range=1,subtypes=new[]{"Horror"},art="soldier"},"generated");SpawnMission(owner,Position(item.Source.Id),enemy,horror);preconFactoryCounters[item.Source.Id]=0;}next();});return true;
    case "pca-exumar":var ownDead=seats[owner].grave.Where(c=>c.Kind==CardType.Creature).ToArray();if(ownDead.Length==0){next();return true;}Ask(owner,"Exumar criatura: exilar e comprar",ownDead.Select((c,i)=>new ChoiceOption(i,c.Name)),i=>{if(seats[owner].grave.Remove(ownDead[i])){seats[owner].exile.Add(ownDead[i]);PreconLibraryGraveLeave(owner);DrawCard(owner);}next();});return true;
    case "pca-persuade":if(target!=null&&Find(target.Id)!=null){int original=target.OriginalOwner;target.OriginalOwner=original;target.Owner=owner;movementOrders.Remove(target.Id);PreconMoveToRealm(owner,target);}break;
    case "pca-realm-return":PreconRealmReturn(owner,origin,next);return true;
    case "pca-mill-victim":if(item.Subject!=null&&item.Subject.Card.Kind==CardType.Creature&&(op.b==0||Enemies(owner,item.Subject.Owner)))MillCards(item.Subject.Owner,op.a);break;
    case "pca-mill-killer":if(item.Target>=0&&item.Target<seats.Length)MillCards(item.Target,op.a);break;
    case "pca-top-keep-mill":var top=seats[owner].main.AsEnumerable().Reverse().Take(op.a).ToArray();if(top.Length==0){next();return true;}Ask(owner,"Carta para manter no topo; demais ao cemitério",top.Select((c,i)=>new ChoiceOption(i,c.Name)),i=>{foreach(var c in top.Where((c,n)=>n!=i))if(seats[owner].main.Remove(c))seats[CardOriginalOwner(c,owner)].grave.AddMilled(c);next();});return true;
    case "pca-mill-damage":if(target!=null&&Find(target.Id)!=null)preconMillDamage[target.Id]=Turn;break;
    case "pca-bottom-recover":PreconBottomRecover(owner,next);return true;
    case "pca-secret-exile":PreconSecretExile(owner,next);return true;
    case "pca-sacrifice-mill":MillCards(player,Math.Max(0,item.Amount));break;
    case "pca-grave-other":PreconGraveOther(item,op.a,next);return true;
    case "pca-catacombs-return":PreconCatacombsReturn(item,next);return true;
    // Passive operations are consumed by the dedicated stat/placement/movement hooks.
    case "pca-document":case "pca-symphony":case "pca-cross-pieces":break;
   }
   next();return true;
  }
  void PreconSwapCombat(int owner,Piece old,Action done)
  {
   if(old==null||Find(old.Id)==null||old.Owner!=owner||!stack.Any(b=>b.Card==null&&(b.Attackers.Contains(old.Id)||b.Blockers.Contains(old.Id)))){done();return;}
   var choices=All.Where(p=>p!=old&&p.Owner==owner&&p.Card.Kind==CardType.Creature&&!Immune(p,owner)).ToArray();if(choices.Length==0){done();return;}
   Ask(owner,"Outra criatura sua para substituir no combate",choices.Select(p=>new ChoiceOption(p.Id,p.Card.Name)),id=>{var replacement=Find(id);if(Find(old.Id)==null||replacement==null){done();return;}int from=Position(old.Id),to=Position(id);var oldGroup=PreconAttachedGroup(old).ToArray();var newGroup=PreconAttachedGroup(replacement).ToArray();foreach(var p in oldGroup){cells[Position(p.Id)].pieces.Remove(p);cells[to].pieces.Add(p);}foreach(var p in newGroup){cells[Position(p.Id)].pieces.Remove(p);cells[from].pieces.Add(p);}MarkMoved(old,from,to);MarkMoved(replacement,to,from);foreach(var battle in stack.Where(b=>b.Card==null)){battle.Attackers=battle.Attackers.Select(p=>p==old.Id?replacement.Id:p==replacement.Id?old.Id:p).ToArray();battle.Blockers=battle.Blockers.Select(p=>p==old.Id?replacement.Id:p==replacement.Id?old.Id:p).ToArray();}Visual?.Invoke(new MatchEvent("move",from,to,old.Id));Visual?.Invoke(new MatchEvent("move",to,from,replacement.Id));done();});
  }
  IEnumerable<Piece> PreconAttachedGroup(Piece root)
  {var group=new HashSet<Piece>{root};bool added;do{added=false;foreach(var p in All)if(group.Any(q=>p.AttachedTo==q.Id||p.CarrierId==q.Id)&&group.Add(p))added=true;}while(added);return group;}
  void PreconWave(int owner,int center,Pending item)
  {
   foreach(var p in cells[center].pieces.Where(p=>p.Card.Kind==CardType.Creature||p.Card.Kind==CardType.Construction).ToArray())if(!Immune(p,owner))DamageTo(p,3,item.Source,true,owner);
   int capital=cells[center].CapitalOwner;if(capital>=0)DamageCapital(capital,3,item.Source,owner);
   foreach(int tile in Neighbors(center)){int dx=tile%11-center%11,dy=tile/11-center/11;foreach(var p in cells[tile].pieces.Where(p=>!p.Card.Traits.Contains("pca-tile-enchantment")&&p.AttachedTo<0&&p.CarrierId<0).ToArray()){if(Find(p.Id)==null||Immune(p,owner)||AbilityMoveBlocked(p,owner))continue;int at=tile;for(int step=0;step<2;step++){int x=at%11+dx,y=at/11+dy;if(x<0||x>=11||y<0||y>=11)break;int next=x+y*11;if(cells[next].Terrain==null||Blocked(p,next)||PreconAuthorMoveBlocked(p,next))break;ApplyDisplacement(owner,p,next,null);if(Find(p.Id)==null)break;at=next;}}}
   Preview(item,center);
  }
  void PreconMoveToRealm(int owner,Piece p)
  {
   int start=Position(p.Id);var parent=new Dictionary<int,int>{{start,-1}};var pending=new Queue<int>();pending.Enqueue(start);int destination=-1;
   while(pending.Count>0){int at=pending.Dequeue();if(cells[at].Owner==owner&&cells[at].Terrain!=null&&!Blocked(p,at)&&!cells[at].pieces.Any(q=>q!=p&&q.Card.Kind==CardType.Creature&&Enemies(owner,q.Owner))){destination=at;break;}foreach(int next in Neighbors(at))if(!parent.ContainsKey(next)&&cells[next].Terrain!=null&&!Blocked(p,next)&&!PreconAuthorMoveBlocked(p,next)&&!cells[next].pieces.Any(q=>q!=p&&q.Card.Kind==CardType.Creature&&Enemies(owner,q.Owner))&&!Enemies(owner,cells[next].CapitalOwner)){parent[next]=at;pending.Enqueue(next);}}
   if(destination<0||destination==start)return;var route=new List<int>();for(int at=destination;at!=start;at=parent[at])route.Add(at);route.Reverse();foreach(int at in route){if(Find(p.Id)==null)break;ApplyDisplacement(owner,p,at,null);}
  }
  void PreconRealmReturn(int owner,int at,Action done)
  {
   if(!Valid(at)||cells[at].Terrain==null||cells[at].pieces.Any(p=>p.Card.Kind==CardType.Creature)){done();return;}var cards=seats[owner].grave.Where(c=>c.Kind==CardType.Creature).ToArray();if(cards.Length==0){done();return;}
   Ask(owner,"Criatura do cemitério para defender até o fim do turno",cards.Select((c,i)=>new ChoiceOption(i,c.Name)),i=>{if(i>=0&&seats[owner].grave.Remove(cards[i])){var p=new Piece(nextPiece++,owner,cards[i]){Game=this,OriginalOwner=CardOriginalOwner(cards[i],owner)};cells[at].pieces.Add(p);preconReturnedAtEnd[p.Id]=Turn;PreconLibraryGraveLeave(owner);Enter(p);Visual?.Invoke(new MatchEvent("summon",at,at,p.Id,p.Card,owner));}done();},true);
  }
  void PreconCatacombsReturn(Pending item,Action done)
  {
   int owner=item.Owner;var source=item.Source;int at=source!=null?Position(source.Id):-1;
   if(source==null||Find(source.Id)!=source||source.Owner!=owner||!Valid(at)||cells[at].Terrain==null||item.Subject==null||item.Subject==source||item.Subject.Owner!=owner||item.Subject.Card.Kind!=CardType.Creature){done();return;}
   var choices=seats[owner].grave.Where(c=>c.Kind==CardType.Creature&&c.TotalCost<=2&&c!=item.Subject.Card).ToArray();if(choices.Length==0){done();return;}
   Ask(owner,"Perder 2 de vida e reanimar no terreno do Senhor das Catacumbas?",choices.Select((c,i)=>new ChoiceOption(i,c.Name)),i=>{
    if(i<0){done();return;}at=Position(source.Id);var card=choices[i];
    if(Find(source.Id)!=source||source.Owner!=owner||!Valid(at)||cells[at].Terrain==null||!seats[owner].grave.Contains(card)){done();return;}
    LoseLife(owner,2);if(seats[owner].Eliminated||Find(source.Id)!=source||!seats[owner].grave.Remove(card)){done();return;}
    var resurrected=new Piece(nextPiece++,owner,card){Game=this,OriginalOwner=CardOriginalOwner(card,owner)};cells[at].pieces.Add(resurrected);PreconLibraryGraveLeave(owner);Enter(resurrected);Visual?.Invoke(new MatchEvent("summon",at,at,resurrected.Id,card,owner));done();
   },true);
  }
  void PreconGraveOther(Pending item,int left,Action done)
  {
   if(left<=0){done();return;}int owner=item.Owner;var choices=seats[owner].grave.Where(c=>c.Kind==CardType.Creature&&c!=item.Card).ToArray();if(choices.Length==0){done();return;}Ask(owner,"Outra criatura do cemitério para sua mão",choices.Select((c,i)=>new ChoiceOption(i,c.Name)),i=>{if(i<0){done();return;}if(seats[owner].grave.Remove(choices[i])){seats[owner].hand.Add(choices[i]);PreconLibraryGraveLeave(owner);}PreconGraveOther(item,left-1,done);},true);
  }
  void PreconBottomRecover(int owner,Action done)
  {
   var hand=seats[owner].hand.ToArray();if(hand.Length==0){done();return;}Ask(owner,"Carta da mão para o fundo?",hand.Select((c,i)=>new ChoiceOption(i,c.Name)),i=>{if(i<0){done();return;}if(!seats[owner].hand.Remove(hand[i])){done();return;}seats[owner].main.Insert(0,hand[i]);var grave=seats.SelectMany((s,who)=>s.grave.Select(c=>(who,card:c))).ToArray();if(grave.Length==0){done();return;}Ask(owner,"Carta de cemitério para sua mão",grave.Select((x,n)=>new ChoiceOption(n,x.card.Name+" · "+seats[x.who].Name)),n=>{var x=grave[n];if(seats[x.who].grave.Remove(x.card)){var card=new Definition(NetworkCards.Pack(x.card),x.card.Expansion);foreignOwners[card]=CardOriginalOwner(x.card,x.who);seats[owner].hand.Add(card);PreconLibraryGraveLeave(owner);}done();});},true);
  }
  void PreconSecretExile(int owner,Action done)
  {
   PreconLibraryEnemy(owner,"Grimório inimigo para olhar",enemy=>{var card=seats[enemy].main.LastOrDefault();if(card==null){done();return;}Ask(owner,"Topo: "+card.Name+". Exilar sem revelar?",new[]{new ChoiceOption(1,"Exilar e permitir conjurar com mana genérica")},i=>{if(i==1&&seats[enemy].main.LastOrDefault()==card){Pop(seats[enemy].main);int original=CardOriginalOwner(card,enemy);seats[original].exile.Add(card);preconSecretExile[card]=owner;exilePermissions.Add(new ExilePermission{Id=nextPermission++,Owner=owner,CardOwner=original,Card=card,AnyMana=true});}done();},true);},done);
  }
 }
}
