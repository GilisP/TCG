using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
 public static class PreconLibrarySchema
 {
  public static readonly HashSet<string> Operations=new HashSet<string>{"pcl-enemy-mill-draw","pcl-enemy-mill-exile","pcl-capital-top-mill","pcl-top-mill","pcl-exile-grave","pcl-grave","pcl-select-top","pcl-counter","pcl-bounce","pcl-top-hand-life","pcl-top-bottom","pcl-look","pcl-scry-other-spell","pcl-hand-spell-bottom-draw","pcl-grave-bottom-draw","pcl-enemy-mill","pcl-other-enemies-mill","pcl-capital-mill"};
  public static readonly HashSet<string> Traits=new HashSet<string>{"precon-random-mana"};
  public static readonly HashSet<string> Conditions=new HashSet<string>{"pcl-exile-permission"};
  public static readonly HashSet<string> Triggers=new HashSet<string>{"pcl-first-spell","pcl-first-instant","pcl-foreign-cast","pcl-grave-leave","pcl-equip-mage"};
  public static readonly HashSet<string> Targets=new HashSet<string>();
 }
 public sealed partial class Match
 {
  // Permissions expire after this turn, or after the caster's next completed turn.
  readonly Dictionary<int,(int turn,int owner,bool next)> preconPermissionExpiry=new Dictionary<int,(int,int,bool)>();
  readonly Dictionary<int,(int turn,int count,int sorceries)> preconSpells=new Dictionary<int,(int,int,int)>();
  bool? PreconLibraryCondition(string condition,Piece piece,int owner,int at,int target)
  {if(condition=="pcl-exile-permission")return exilePermissions.Any(x=>x.Owner==owner&&PreconLibraryPermissionValid(x.Id));return null;}
  bool? PreconLibraryHasTargets(CardAbility ability,Pending item)
  {
   if(ability.target!="owner")return null;
   bool handled=false;int owner=item.Owner;
   foreach(var op in ability.ops)
   {
    if(op.kind=="pcl-counter"){handled=true;if(!stack.Any(x=>x.Rule==null&&x.Card!=null&&(x.Card.Kind==CardType.Instant||x.Card.Kind==CardType.Spell)&&x.Card.TotalCost<=op.a))return false;}
    if(op.kind=="pcl-bounce"){handled=true;if(!All.Any(p=>p.Card.Kind==CardType.Creature&&p.Card.TotalCost<=op.a&&!Immune(p,owner)&&(op.value!="noncommander"||!p.CommandUnit)))return false;}
    // Optional recovery can still precede other effects (for example a token).
    if(op.kind=="pcl-grave"&&!ability.optional&&op.b==1){handled=true;var type=op.value.Split(':')[0];if(!seats[owner].grave.Any(c=>c.TotalCost<=op.a&&PreconLibraryType(c,type)))return false;}
    if(op.kind=="pcl-grave-bottom-draw"){handled=true;if(!seats[owner].grave.Any(c=>c.Kind==CardType.Instant))return false;}
    if(op.kind=="pcl-exile-grave"&&op.b==1){handled=true;if(!seats.SelectMany((s,who)=>s.grave.Where(c=>Enemies(owner,who)&&c.Kind!=CardType.Terrain&&c.TotalCost<=op.a)).Any())return false;}
   }
   return handled?(bool?)true:null;
  }
  void PreconLibraryPlayed(int owner,Definition card)
  {
   if(card.Kind!=CardType.Spell&&card.Kind!=CardType.Instant)return;
   if(!preconSpells.TryGetValue(owner,out var value)||value.turn!=Turn)value=(Turn,0,0);
   value.count++;if(card.Kind==CardType.Spell)value.sorceries++;preconSpells[owner]=value;
   if(card.Kind==CardType.Spell&&value.sorceries==1&&Active==owner)foreach(var p in All.Where(x=>x.Owner==owner).ToArray())AbilityEvent(p,"pcl-first-spell");
   if(card.Kind==CardType.Instant&&value.count-value.sorceries==1)foreach(var p in All.Where(x=>x.Owner==owner).ToArray())AbilityEvent(p,"pcl-first-instant");
  }
  void PreconLibraryForeignPlayed(int owner,int original,Definition card)
  {if(Enemies(owner,original))foreach(var p in All.Where(x=>x.Owner==owner).ToArray())AbilityEvent(p,"pcl-foreign-cast",target:original);}
  void PreconLibraryGraveLeave(int actor){foreach(var p in All.Where(x=>x.Owner==actor).ToArray())AbilityEvent(p,"pcl-grave-leave");}
  void PreconLibraryEquipped(Piece equipment,Piece host){if(equipment!=null&&host!=null&&host.Card.Subtypes.Contains("Mago"))AbilityEvent(equipment,"pcl-equip-mage",host);}
  bool PreconLibraryPermissionValid(int id)=>!preconPermissionExpiry.TryGetValue(id,out var expiry)||expiry.next||expiry.turn>=Turn;
  void PreconLibraryEndTurn(int owner)
  {
   foreach(var pair in preconPermissionExpiry.ToArray())if(!pair.Value.next||pair.Value.owner==owner&&Turn>pair.Value.turn){exilePermissions.RemoveAll(x=>x.Id==pair.Key);preconPermissionExpiry.Remove(pair.Key);}
  }
  bool PreconLibraryType(Definition c,string type)=>type=="any"||type=="magic"&&(c.Kind==CardType.Instant||c.Kind==CardType.Spell)||type=="spell"&&c.Kind==CardType.Spell||type=="instant"&&c.Kind==CardType.Instant||type=="creature"&&c.Kind==CardType.Creature||type=="equipment"&&c.Kind==CardType.Equipment;
  void PreconLibraryEnemy(int owner,string label,Action<int> action,Action done)
  {var enemies=Enumerable.Range(0,seats.Length).Where(x=>Enemies(owner,x)&&!seats[x].Eliminated).ToArray();if(enemies.Length==0){done();return;}Ask(owner,label,enemies.Select(x=>new ChoiceOption(x,seats[x].Name)),action);}
  bool ResolvePreconLibraryOp(Pending item,CardAbility ability,Piece target,AbilityOp op,int player,Action next)
  {
   int owner=item.Owner;
   switch(op.kind)
   {
    case "pcl-capital-mill":if(item.Target>=0&&item.Target<seats.Length)MillCards(item.Target,op.a);next();return true;
    case "pcl-enemy-mill":PreconLibraryEnemy(owner,"Oponente para triturar",who=>{MillCards(who,op.a);next();},next);return true;
    case "pcl-other-enemies-mill":foreach(int who in Enumerable.Range(0,seats.Length).Where(x=>Enemies(owner,x)&&x!=item.Target&&!seats[x].Eliminated))MillCards(who,op.a);next();return true;
    case "pcl-enemy-mill-draw":PreconLibraryEnemy(owner,"Oponente para triturar e comprar",who=>{MillCards(who,op.a);if(!seats[who].Eliminated)DrawCard(who);next();},next);return true;
    case "pcl-enemy-mill-exile":PreconLibraryEnemy(owner,"Oponente para triturar",who=>{MillCards(who,op.a);var cards=seats[who].grave.Where(c=>c.Kind==CardType.Creature).ToArray();if(cards.Length==0){next();return;}Ask(owner,"Exilar criatura do cemitério?",cards.Select((c,i)=>new ChoiceOption(i,c.Name)),i=>{if(i>=0&&seats[who].grave.Remove(cards[i])){seats[who].exile.Add(cards[i]);GainLife(owner,op.b);}next();},true);},next);return true;
    case "pcl-capital-top-mill":PreconTopMill(owner,item.Target,op.a,next);return true;
    case "pcl-top-mill":PreconTopMill(owner,player,op.a,next);return true;
    case "pcl-exile-grave":PreconExileGrave(owner,op,op.b,next);return true;
    case "pcl-grave":PreconGrave(owner,op,op.b,next);return true;
    case "pcl-select-top":PreconSelectTop(owner,op,next);return true;
    case "pcl-counter":PreconCounter(owner,op.a,op.b,next);return true;
    case "pcl-bounce":PreconBounce(owner,op,op.b,next);return true;
    case "pcl-top-hand-life":
     if(seats[owner].main.Count==0){next();return true;}var top=seats[owner].main.Last();Ask(owner,"Topo: "+top.Name,new[]{new ChoiceOption(1,"Colocar na mão e perder "+op.a+" de vida")},i=>{if(i==1&&seats[owner].main.LastOrDefault()==top){Pop(seats[owner].main);seats[owner].hand.Add(top);LoseLife(owner,op.a);}next();},true);return true;
    case "pcl-top-bottom":
     var cards=seats[owner].main.AsEnumerable().Reverse().Take(op.a).ToArray();if(cards.Length==0){next();return true;}Ask(owner,"Escolha a carta para o fundo",cards.Select((c,i)=>new ChoiceOption(i,c.Name)),i=>{seats[owner].main.Remove(cards[i]);seats[owner].main.Insert(0,cards[i]);next();});return true;
    case "pcl-look":if(seats[owner].main.Count==0){next();return true;}Ask(owner,"Topo: "+seats[owner].main.Last().Name,new[]{new ChoiceOption(0,"Continuar")},i=>next());return true;
    case "pcl-scry-other-spell":if(preconSpells.TryGetValue(owner,out var cast)&&cast.turn==Turn&&cast.count>=2)ScryAbility(owner,"bottom",next);else next();return true;
    case "pcl-hand-spell-bottom-draw":
     var spells=seats[owner].hand.Where(c=>c.Kind==CardType.Spell).ToArray();if(spells.Length==0){next();return true;}Ask(owner,"Feitiço para o fundo?",spells.Select((c,i)=>new ChoiceOption(i,c.Name)),i=>{if(i>=0&&seats[owner].hand.Remove(spells[i])){seats[owner].main.Insert(0,spells[i]);DrawCard(owner);}next();},true);return true;
    case "pcl-grave-bottom-draw":
     var instants=seats[owner].grave.Where(c=>c.Kind==CardType.Instant).ToArray();if(instants.Length==0){next();return true;}Ask(owner,"Truque para o fundo",instants.Select((c,i)=>new ChoiceOption(i,c.Name)),i=>{if(seats[owner].grave.Remove(instants[i])){seats[owner].main.Insert(0,instants[i]);PreconLibraryGraveLeave(owner);DrawCard(owner);}next();});return true;
    default:return false;
   }
  }
  void PreconTopMill(int owner,int victim,int count,Action done)
  {
   if(victim<0||victim>=seats.Length){done();return;}var cards=seats[victim].main.AsEnumerable().Reverse().Take(count).ToArray();if(cards.Length==0){done();return;}
   Ask(owner,"Escolha uma carta do topo para triturar",cards.Select((c,i)=>new ChoiceOption(i,c.Name)),i=>{var c=cards[i];if(seats[victim].main.Remove(c))seats[CardOriginalOwner(c,victim)].grave.AddMilled(c);PreconOrder(owner,victim,cards.Where((x,n)=>n!=i).ToList(),false,done);});
  }
  void PreconOrder(int chooser,int owner,List<Definition> cards,bool bottom,Action done)
  {
   var ordered=new List<Definition>();void Choose(){if(cards.Count==0){foreach(var c in ordered)seats[owner].main.Remove(c);if(bottom)seats[owner].main.InsertRange(0,ordered);else seats[owner].main.AddRange(ordered.AsEnumerable().Reverse());done();return;}Ask(chooser,bottom?"Fundo: mais profunda primeiro":"Topo: próxima compra",cards.Select((c,i)=>new ChoiceOption(i,c.Name)),i=>{ordered.Add(cards[i]);cards.RemoveAt(i);Choose();});}Choose();
  }
  void PreconSelectTop(int owner,AbilityOp op,Action done)
  {
   var cards=seats[owner].main.AsEnumerable().Reverse().Take(op.a).ToList();var legal=cards.Where(c=>PreconLibraryType(c,op.value)&&c.TotalCost>=op.b).ToArray();
   void Rest()=>PreconOrder(owner,owner,cards,true,done);
   if(legal.Length==0){Rest();return;}Ask(owner,"Revelar uma carta para a mão?",legal.Select((c,i)=>new ChoiceOption(i,c.Name)),i=>{if(i>=0){var c=legal[i];if(seats[owner].main.Remove(c)){seats[owner].hand.Add(c);cards.Remove(c);Note(seats[owner].Name+" revelou "+c.Name+".");}}Rest();},true);
  }
  void PreconGrave(int owner,AbilityOp op,int remaining,Action done)
  {
   if(remaining<=0){done();return;}var parts=op.value.Split(':');var cards=seats[owner].grave.Where(c=>c.TotalCost<=op.a&&PreconLibraryType(c,parts[0])).ToArray();if(cards.Length==0){done();return;}
   Ask(owner,"Escolha no cemitério",cards.Select((c,i)=>new ChoiceOption(i,c.Name)),i=>{if(i<0){done();return;}var c=cards[i];if(seats[owner].grave.Remove(c)){if(parts.Length<2||parts[1]=="hand")seats[owner].hand.Add(c);else if(parts[1]=="top")seats[owner].main.Add(c);else seats[owner].main.Insert(0,c);PreconLibraryGraveLeave(owner);}PreconGrave(owner,op,remaining-1,done);},op.b>1);
  }
  void PreconExileGrave(int owner,AbilityOp op,int remaining,Action done)
  {
   if(remaining<=0){done();return;}var choices=seats.SelectMany((s,who)=>s.grave.Where(c=>Enemies(owner,who)&&c.Kind!=CardType.Terrain&&c.TotalCost<=op.a).Select(c=>(who,card:c))).ToArray();if(choices.Length==0){done();return;}
   Ask(owner,"Exilar do cemitério inimigo",choices.Select((x,i)=>new ChoiceOption(i,x.card.Name+" · "+seats[x.who].Name)),i=>{if(i<0){done();return;}var x=choices[i];if(seats[x.who].grave.Remove(x.card)){seats[x.who].exile.Add(x.card);var permission=new ExilePermission{Id=nextPermission++,Owner=owner,CardOwner=x.who,Card=x.card,AnyMana=true};exilePermissions.Add(permission);preconPermissionExpiry[permission.Id]=(Turn,owner,op.c!=0);PreconLibraryGraveLeave(owner);}PreconExileGrave(owner,op,remaining-1,done);},op.b>1);
  }
  void PreconCounter(int owner,int maxCost,int count,Action done)
  {
   if(count<=0){done();return;}var choices=stack.Where(x=>x.Rule==null&&x.Card!=null&&(x.Card.Kind==CardType.Instant||x.Card.Kind==CardType.Spell)&&x.Card.TotalCost<=maxCost).ToArray();if(choices.Length==0){done();return;}
   Ask(owner,"Escolha a magia para anular",choices.Select((x,i)=>new ChoiceOption(i,x.Card.Name)),i=>{if(i<0){done();return;}var x=choices[i];if(stack.Remove(x)){FinishSpell(x,false);StackResult(x,true);}PreconCounter(owner,maxCost,count-1,done);},count>1);
  }
  void PreconBounce(int owner,AbilityOp op,int count,Action done)
  {
   if(count<=0){done();return;}var choices=All.Where(p=>p.Card.Kind==CardType.Creature&&p.Card.TotalCost<=op.a&&!Immune(p,owner)&&(op.value!="noncommander"||!p.CommandUnit)).ToArray();if(choices.Length==0){done();return;}
   Ask(owner,"Devolver criatura",choices.Select(p=>new ChoiceOption(p.Id,p.Card.Name)),id=>{if(id<0){done();return;}var p=Find(id);if(p==null){done();return;}int original=p.OriginalOwner;void Return(){ReturnHand(p);if(op.c==1)Ask(original,"Comprar uma carta?",new[]{new ChoiceOption(1,"Comprar")},i=>{if(i==1)DrawCard(original);PreconBounce(owner,op,count-1,done);},true);else PreconBounce(owner,op,count-1,done);}
    if(p.CommandUnit)Ask(original,"Destino do comandante",new[]{new ChoiceOption(0,"Mão"),new ChoiceOption(1,"Zona de comando")},i=>{ReturnHand(p);if(i==1){seats[original].hand.Remove(p.Card);seats[original].CommanderReady=true;}PreconBounce(owner,op,count-1,done);});else Return();},op.b>1);
  }
 }
}
