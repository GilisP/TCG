using System;
using System.Collections.Generic;
using System.Linq;
namespace TCG.Foundation
{
    public static class AuthorRules
    {
        public static readonly HashSet<string> Programs=new HashSet<string>{"abyss-hunger","end-messenger","burning-ancestor","ancestral-monk","moon-admirer","moon-herald","radiant-knight","curse-collector","thousand-beasts","infiltration","crypt-thief","test-counter","test-mill","test-blessing","test-curse","valeria","royal-devotion","lissandra","suicidal-inventor","old-fisher","mother-bear","dual-receptacle","tomes","ossuary","widow","mountain-king","natural-aberration","martyrs","study","academy","wild-grave","mountain-crossing","reverse-garden","portals","market","arena","bridge","hungry-forest","goblin-warehouse","transmute","catapult","mausoleum","lighthouse","terramorph","teletransport","destiny","cannon","mirror-copy","mirror-shield","parasite","goblin-mission","spears","archer-master","khan"};
        public static readonly HashSet<string> Traits=new HashSet<string>{"author-tomes","author-ossuary","author-widow","author-mountain-king","author-natural","author-martyrs","author-spears","author-archer","author-khan","author-goblin","flying","goblin","mage"};
    }
    public sealed partial class Match
    {
        readonly Dictionary<int,int> walked=new Dictionary<int,int>();readonly HashSet<int> attackedThisTurn=new HashSet<int>();
        readonly Dictionary<int,int> missions=new Dictionary<int,int>();readonly Dictionary<int,List<Definition>> storedDead=new Dictionary<int,List<Definition>>();
        readonly Dictionary<int,int> replacements=new Dictionary<int,int>();readonly HashSet<int> bridged=new HashSet<int>();
        Random authorRandom; int authorEndProcessed=-1;
        bool TerrainRule(int at,string rule)=>Valid(at)&&cells[at].Terrain?.Rule==rule;
        internal bool Inverted(Piece p)=>p.Card.Kind==CardType.Creature&&TerrainRule(Position(p.Id),"reverse-garden");
        bool AuthorHas(Piece p,string rule)=>p!=null&&p.Card.Rule==rule;
        int AuthorBonus(Piece p,int stat)
        {
            int value=NewBonus(p,stat);
            if(stat==0&&All.Any(e=>e.AttachedTo==p.Id&&AuthorHas(e,"parasite")))value+=2*All.Count(e=>e.AttachedTo==p.Id&&AuthorHas(e,"parasite"));
            if(stat==2&&p.Card.Kind==CardType.Creature&&p.Card.Range+p.Modifiers.Sum(m=>m.Range)>=2&&!All.Any(q=>q.Card.Kind==CardType.Creature&&Enemies(p.Owner,q.Owner)&&Near(p,q)))value+=All.Count(q=>q.Owner==p.Owner&&AuthorHas(q,"archer-master"));
            return value;
        }
        void TerrainTrigger(int at,string rule,int owner=-1,Piece subject=null)
        {
            var cell=cells[at];int who=owner>=0?owner:cell.Owner;if(who<0||seats[who].Eliminated||cell.Terrain==null)return;
            stack.Add(new Pending{Owner=who,Card=cell.Terrain,Rule=rule,SourceCell=at,Subject=subject});passes=0;Visual?.Invoke(new MatchEvent("trigger",Capital(who,seats.Length),Capital(who,seats.Length),-1,cell.Terrain,who));
        }
        void AuthorTurn()
        {
            walked.Clear();attackedThisTurn.Clear();bridged.Clear();
            foreach(var p in All.ToArray())
            {
                if(AuthorHas(p,"tomes"))Trigger(p,"tomes");
                if(p.Owner==Active&&AuthorHas(p,"mountain-king")&&p.LastMovedTurn<p.PreviousOwnTurn)Trigger(p,"mountain-king");
            }
            for(int at=0;at<121;at++)if(TerrainRule(at,"market"))TerrainTrigger(at,"market");
        }
        void AuthorBuy()
        {
            int extra=cells.Count(c=>c.Terrain?.Rule=="study"&&c.Owner==Active);for(int n=0;n<extra&&!seats[Active].Eliminated;n++)DrawCard(Active);
        }
        void AuthorDeath(Piece p,int at)
        {
            missions.Remove(p.Id);replacements.Remove(p.Id);
            if(p.Card.Kind==CardType.Creature)
            {
                if(TerrainRule(at,"hungry-forest"))TerrainTrigger(at,"hungry-forest");
                for(int n=0;n<121;n++)if(TerrainRule(n,"wild-grave"))TerrainTrigger(n,"wild-grave");
                foreach(var q in All.Where(q=>q.Card.Kind==CardType.Creature).ToArray())
                {
                    if(AuthorHas(q,"widow")&&Enemies(q.Owner,p.Owner)&&p.Damage>0&&Distance(Position(q.Id),at)<=2)Trigger(q,"widow",p,at);
                    if(AuthorHas(q,"martyrs")&&q.Owner==p.Owner&&All.Any(r=>r.Owner==p.Owner&&r.Card.Kind==CardType.Creature&&Adjacent(Position(r.Id),at)))Trigger(q,"martyrs",p,at);
                }
                if(AuthorHas(p,"natural-aberration"))Trigger(p,"natural-aberration",p,at);
                var mausoleums=All.Where(q=>AuthorHas(q,"mausoleum")&&q.Owner==p.Owner&&Adjacent(Position(q.Id),at)).ToArray();
                if(mausoleums.Length>0)work.Enqueue(()=>{
                    if(!seats[p.Owner].grave.Contains(p.Card))return;
                    var valid=mausoleums.Where(q=>Find(q.Id)!=null).ToArray();
                    Action<int> store=id=>{if(Find(id)==null||!seats[p.Owner].grave.Remove(p.Card))return;if(!storedDead.TryGetValue(id,out var held)){held=new List<Definition>();storedDead[id]=held;}held.Add(p.Card);};
                    if(valid.Length==1)store(valid[0].Id);else if(valid.Length>1)Ask(p.Owner,"Mausoléu para guardar a criatura",valid.Select(q=>new ChoiceOption(q.Id,q.Card.Name)),store);
                });
            }
            if(storedDead.TryGetValue(p.Id,out var cards)){seats[p.Owner].grave.AddRange(cards);storedDead.Remove(p.Id);}
        }
        void AuthorPlayed(int owner,Definition card)
        {
            if(card.Kind!=CardType.Spell)return;
            foreach(var q in All.Where(q=>q.Owner==owner&&AuthorHas(q,"transmute")&&q.OnceTurn!=Turn).ToArray()) {Trigger(q,"transmute");}
        }
        void AuthorMoved(Piece p,int from,int to,bool byEffect=false)
        {
            walked[p.Id]=(walked.TryGetValue(p.Id,out int n)?n:0)+(PortalLink(from,to)?1:Distance(from,to));
            foreach(var warehouse in cells[to].pieces.Where(q=>AuthorHas(q,"goblin-warehouse")&&p.Card.Subtypes.Contains("Goblin")).ToArray())Trigger(warehouse,"goblin-warehouse",p);
            if(TerrainRule(to,"bridge")&&bridged.Add(p.Id))TerrainTrigger(to,"bridge",p.Owner,p);
        }
        void AuthorAttack(Piece p,int target)
        {
            if(AuthorHas(p,"natural-aberration")){int total=All.Where(q=>q.Card.Kind==CardType.Creature&&cells[Position(q.Id)].Owner==p.Owner).Sum(q=>q.Defense);Buff(p,defense:total);}
            if(!attackedThisTurn.Contains(p.Id)&&All.Any(q=>q.Owner==p.Owner&&AuthorHas(q,"khan")))p.Modifiers.Add(new Modifier{Attack=walked.TryGetValue(p.Id,out var n)?n:0,CombatOnly=true});
            attackedThisTurn.Add(p.Id);
        }
        void ArenaEnter(Pending battle)
        {
            var fighters=battle.Attackers.Select(Find).Concat(cells[battle.Target].pieces).Where(p=>p!=null&&p.Card.Kind==CardType.Creature).Distinct().ToArray();
            foreach(var p in fighters)if(TerrainRule(Position(p.Id),"arena"))p.Modifiers.Add(new Modifier{Attack=2,CombatOnly=true});
            int groups=battle.Attackers.Select(Find).Where(p=>p!=null).Select(p=>Position(p.Id)).Distinct().Count();
            foreach(var p in battle.Attackers.Select(Find).Where(p=>p!=null))if(All.Any(q=>AuthorHas(q,"spears")&&Allied(q.Owner,p.Owner))&&All.Any(q=>q!=p&&q.Owner==p.Owner&&q.Card.Kind==CardType.Creature&&Adjacent(Position(q.Id),battle.Target)))p.Modifiers.Add(new Modifier{Attack=2*groups});
        }
        void AuthorAfterDamage(Piece source,int amount)
        {
            NewAfterDamage(source,amount);if(source==null||amount<=0)return;
            foreach(var e in All.Where(e=>e.AttachedTo==source.Id&&AuthorHas(e,"parasite"))){e.OnceTurn=Turn;GainLife(source.Owner,1);}
        }
        void AuthorEnd(){ if(authorEndProcessed==Turn)return;authorEndProcessed=Turn;
            foreach(var e in All.Where(e=>AuthorHas(e,"parasite")&&e.OnceTurn!=Turn).ToArray()){var p=Find(e.AttachedTo);if(p!=null)DamageTo(p,3,null,true,e.Owner);}
            foreach(var p in All.Where(p=>p.ExpireTurn==Turn).ToArray()){int at=Position(p.Id);Detach(p);cells[at].pieces.Remove(p);missions.Remove(p.Id);Visual?.Invoke(new MatchEvent("return",at,at,p.Id,p.Card,p.Owner));}
        }
        bool AuthorCanActivate(Piece p)
        {
            if(p==null)return false;if(NewCanActivate(p))return true;if(BicolorCanActivate(p))return true;int at=Position(p.Id);
            switch(p.Card.Rule)
            {
                case "ossuary":return p.Actions>=1&&All.Any(q=>q!=p&&q.Owner==p.Owner&&q.Card.Kind==CardType.Creature);
                case "goblin-mission":return p.Actions>=3&&All.Any(q=>q.Card.Subtypes.Contains("Goblin"));
                case "catapult":return All.Any(q=>q.Card.Kind==CardType.Creature&&q.Owner==p.Owner&&(Position(q.Id)==at||Adjacent(Position(q.Id),at)));
                case "mausoleum":return seats[p.Owner].mana.Sum()>=3;
                case "mirror-copy":return seats[p.Owner].mana.Sum()>=2&&All.Any(q=>q.Owner==p.Owner&&q.Card.Kind==CardType.Creature);
                case "cannon":return cells[at].pieces.Any(q=>q.Owner==p.Owner&&q.Card.Kind==CardType.Creature&&q.Actions>0);
            }
            return false;
        }
        void AuthorActivate(Piece p)
        {if(NewActivate(p))return;if(BicolorActivate(p))return;if(p.Card.Rule=="ossuary")p.Actions--;if(p.Card.Rule=="goblin-mission")p.Actions-=3;if(p.Card.Rule=="mausoleum")SpendGeneric(p.Owner,3);if(p.Card.Rule=="mirror-copy")SpendGeneric(p.Owner,2);Trigger(p,p.Card.Rule,repeatByAcademy:false);}
        void PickHand(int owner,string prompt,Func<Definition,bool> filter,Action<Definition> apply,bool optional=true)
        {
            work.Enqueue(()=>{var list=seats[owner].hand.Where(filter).Distinct().ToArray();if(list.Length==0)return;var options=list.Select((c,n)=>new ChoiceOption(n,c.Name)).ToList();if(optional)options.Add(new ChoiceOption(-1,"Não usar"));Choice=new RuleChoice(owner,prompt,options,key=>{if(key>=0&&key<list.Length&&seats[owner].hand.Remove(list[key]))apply(list[key]);});});
        }
        void PickGraveSpell(int owner)
        {
            work.Enqueue(()=>{var list=seats[owner].grave.Where(c=>c.Kind==CardType.Spell||c.Kind==CardType.Instant).Distinct().ToArray();if(list.Length==0)return;Choice=new RuleChoice(owner,"Recuperar truque ou feitiço",list.Select((c,n)=>new ChoiceOption(n,c.Name)).Concat(new[]{new ChoiceOption(-1,"Não recuperar")}),key=>{if(key>=0&&key<list.Length&&seats[owner].grave.Remove(list[key]))seats[owner].hand.Add(list[key]);});});
        }
        void ReplaceTerrainFromPile(int owner,int at,int pile)
        {
            if(!Valid(at)||pile<0||pile>3||cells[at].Owner!=owner||seats[owner].Top(pile)==null)return;
            foreach(var p in cells[at].pieces.ToArray())Kill(p);if(cells[at].Terrain!=null&&cells[at].Terrain.Rule!="ruins")seats[cells[at].TerrainOwner].terrainGrave.Add(cells[at].Terrain);
            SetTerrain(at,owner,Pop(seats[owner].piles[pile]));seats[owner].LastCreatedTerrain=at;if(seats[owner].piles[pile].Count==0)DrawTerrain(Active,pile);Visual?.Invoke(new MatchEvent("terrain",at,at));
        }
        bool AuthorResolve(Pending item)
        {
            if(NewResolve(item)||ResolveBicolor(item))return true;
            int o=item.Owner;var src=item.Source;int at=src!=null&&Position(src.Id)>=0?Position(src.Id):item.SourceCell;
            switch(item.Rule)
            {
                case "tomes":PickGraveSpell(o);break;
                case "ossuary":Pick(o,"Destruir outra criatura sua",p=>p.Owner==o&&p!=src&&p.Card.Kind==CardType.Creature,p=>{Kill(p);DrawCard(o);if(src!=null&&Find(src.Id)!=null)src.Modifiers.Add(new Modifier{Attack=1,Defense=1});});break;
                case "widow":Grave(o,"Criatura para o topo do deck",999,c=>seats[o].main.Add(c),true);break;
                case "mountain-king":if(src!=null&&Find(src.Id)!=null)foreach(var p in All.Where(p=>p.Card.Kind==CardType.Creature&&Allied(o,p.Owner)&&(p==src||Adjacent(at,Position(p.Id)))))p.Modifiers.Add(new Modifier{Defense=1});break;
                case "martyrs":if(item.Subject!=null)Pick(o,"Herdar o ataque em marcadores",p=>p.Owner==o&&p.Card.Kind==CardType.Creature&&Adjacent(Position(p.Id),item.SourceCell),p=>p.Modifiers.Add(new Modifier{Attack=item.Subject.DeathPower}));break;
                case "natural-aberration":foreach(int n in Neighbors(at).Concat(new[]{at})){if(cells[n].Terrain!=null&&cells[n].Terrain.Rule!="ruins"&&cells[n].TerrainOwner>=0)seats[cells[n].TerrainOwner].terrainGrave.Add(cells[n].Terrain);cells[n].Terrain=basicForest;if(cells[n].Owner<0)cells[n].Owner=o;cells[n].TerrainOwner=cells[n].Owner;Visual?.Invoke(new MatchEvent("terrain",n,n));}break;
                case "market":PickHand(o,"Mercado: carta para o fundo e comprar",c=>true,c=>{seats[o].main.Insert(0,c);DrawCard(o);});break;
                case "transmute":PickHand(o,"Transmutação: outro Feitiço para o fundo",c=>c.Kind==CardType.Spell,c=>{if(src!=null)src.OnceTurn=Turn;seats[o].main.Insert(0,c);DrawCard(o);});break;
                case "hungry-forest":GainLife(o,1);break;
                case "bridge":if(item.Subject!=null&&Find(item.Subject.Id)!=null){Displace(o,item.Subject,1,false);work.Enqueue(()=>bridged.Remove(item.Subject.Id));}break;
                case "goblin-warehouse":if(item.Subject!=null&&Find(item.Subject.Id)!=null)Ask(o,"Armazém: bônus permanente",new[]{new ChoiceOption(0,"+1 ataque"),new ChoiceOption(1,"+1 defesa"),new ChoiceOption(2,"+1 PA")},key=>{var p=item.Subject;if(Find(p.Id)==null)return;if(key==2){p.PermanentActions++;p.Actions++;}else p.Modifiers.Add(new Modifier{Attack=key==0?1:0,Defense=key==1?1:0});});break;
                case "terramorph":Ask(o,"Terreno do seu reino",Enumerable.Range(0,121).Where(n=>cells[n].Owner==o&&cells[n].Terrain!=null).Select(n=>new ChoiceOption(n,"Tile "+n%11+","+n/11)),n=>Ask(o,"Escolha o topo da pilha",Enumerable.Range(0,4).Where(k=>seats[o].Top(k)!=null).Select(k=>new ChoiceOption(k,seats[o].Top(k).Name)),k=>ReplaceTerrainFromPile(o,n,k)));break;
                case "teletransport":Ask(o,"Primeiro terreno",Enumerable.Range(0,121).Where(n=>cells[n].Terrain!=null&&cells[n].CapitalOwner<0).Select(n=>new ChoiceOption(n,"Tile "+n%11+","+n/11)),a=>Ask(o,"Segundo terreno",Enumerable.Range(0,121).Where(n=>n!=a&&cells[n].Terrain!=null&&cells[n].CapitalOwner<0).Select(n=>new ChoiceOption(n,"Tile "+n%11+","+n/11)),b=>{var temp=cells[a];cells[a]=cells[b];cells[b]=temp;foreach(var seat in seats){if(seat.LastCreatedTerrain==a)seat.LastCreatedTerrain=b;else if(seat.LastCreatedTerrain==b)seat.LastCreatedTerrain=a;}foreach(var p in cells[a].pieces.ToArray())Visual?.Invoke(new MatchEvent("move",b,a,p.Id));foreach(var p in cells[b].pieces.ToArray())Visual?.Invoke(new MatchEvent("move",a,b,p.Id));Visual?.Invoke(new MatchEvent("terrain",a,b));}));break;
                case "mirror-copy":Pick(o,"Criatura sua para copiar",p=>p.Owner==o&&p.Card.Kind==CardType.Creature,p=>{int pos=Position(p.Id);var copy=new Piece(nextPiece++,o,p.Card){Game=this,Token=true,ExpireTurn=Turn};cells[pos].pieces.Add(copy);Enter(copy);Visual?.Invoke(new MatchEvent("summon",pos,pos,copy.Id));});break;
                case "mausoleum":if(src!=null&&Find(src.Id)!=null){var saved=storedDead.TryGetValue(src.Id,out var list)?list.ToArray():Array.Empty<Definition>();Kill(src);if(saved.Length>0)Ask(o,"Recuperar uma das criaturas armazenadas",saved.Select((c,n)=>new ChoiceOption(n,c.Name)),n=>{if(n>=0&&n<saved.Length&&seats[o].grave.Remove(saved[n]))seats[o].hand.Add(saved[n]);});}break;
                case "catapult":Pick(o,"Sacrificar criatura no tile ou adjacente",p=>p.Owner==o&&p.Card.Kind==CardType.Creature&&(Position(p.Id)==at||Adjacent(Position(p.Id),at)),p=>{int power=p.Attack;Kill(p);ChooseArtillery(o,at,4,power);});break;
                case "cannon":Pick(o,"Criatura que fornecerá PA",p=>p.Owner==o&&p.Card.Kind==CardType.Creature&&Position(p.Id)==at&&p.Actions>0,p=>Ask(o,"Quantos PA consumir?",Enumerable.Range(1,p.Actions).Select(n=>new ChoiceOption(n,n+" PA → "+(n*2)+" dano")),n=>{if(Find(p.Id)==null||p.Actions<n)return;p.Actions-=n;ChooseArtillery(o,at,3,n*2);}));break;
                case "goblin-mission":Ask(o,"Tile com goblins",Enumerable.Range(0,121).Where(n=>cells[n].pieces.Any(p=>p.Card.Subtypes.Contains("Goblin"))).Select(n=>new ChoiceOption(n,"Tile "+n%11+","+n/11)),n=>Ask(o,"Capital inimiga da missão",Enumerable.Range(0,seats.Length).Where(e=>Enemies(o,e)).Select(e=>new ChoiceOption(e,seats[e].Name)),enemy=>{if(src==null||Position(src.Id)<0)return;int amount=cells[n].pieces.Count(p=>p.Card.Subtypes.Contains("Goblin"));for(int k=0;k<amount;k++)SpawnMission(o,Position(src.Id),enemy,goblinToken);}));break;
                case "wild-grave":var enemies=Enumerable.Range(0,seats.Length).Where(e=>Enemies(o,e)).ToArray();if(enemies.Length>0)SpawnMission(o,item.SourceCell,enemies[authorRandom.Next(enemies.Length)],skeletonToken);break;
                case "destiny":ResolveDeathResponse(item);break;
                default:return false;
            }
            return true;
        }
        void ChooseArtillery(int owner,int origin,int range,int damage)
        {
            var units=All.Where(p=>p.Card.Kind==CardType.Creature||p.Card.Kind==CardType.Construction).Where(p=>Distance(origin,Position(p.Id))<=range).ToArray();
            var caps=Enumerable.Range(0,seats.Length).Where(p=>!seats[p].Eliminated&&Distance(origin,Capital(p,seats.Length))<=range).ToArray();
            Ask(owner,"Alvo do disparo · "+damage+" dano",units.Select(p=>new ChoiceOption(p.Id,p.Card.Name)).Concat(caps.Select(p=>new ChoiceOption(-10-p,"Capital de "+seats[p].Name))),key=>{if(key<=-10){int seat=-10-key;DamageCapital(seat,damage,caster:owner);Preview(new Pending{Owner=owner,Card=seats[owner].Commander??basicForest},Capital(seat,seats.Length));}else {var target=Find(key);if(target!=null&&Distance(origin,Position(key))<=range)DamageTo(target,damage,null,true,owner);}});
        }
        Definition basicForest,goblinToken,skeletonToken;
        void SetupAuthorDefinitions(ContentCatalog catalog,int seed)
        {
            authorRandom=new Random(seed^419);
            basicForest=new Definition(new CardData{id="generated-basic-forest",name="Floresta básica",kind="Terrain",color=5,art="terrain",movement=0,actions=0},"generated");
            goblinToken=new Definition(new CardData{id="token-goblin-mission",name="Goblin da missão",kind="Creature",color=3,attack=1,defense=1,actions=1,traits=new[]{"goblin"},subtypes=new[]{"Goblin"},art="soldier"},"generated");
            skeletonToken=new Definition(new CardData{id="token-skeleton-mission",name="Esqueleto da missão",kind="Creature",subtypes=new[]{"Esqueleto"},color=1,attack=1,defense=1,actions=1,art="soldier"},"generated");
        }

        void SpawnMission(int owner,int at,int enemy,Definition card){if(!Valid(at)||cells[at].Terrain==null)return;var p=new Piece(nextPiece++,owner,card){Game=this,Token=true};cells[at].pieces.Add(p);missions[p.Id]=enemy;Visual?.Invoke(new MatchEvent("summon",at,at,p.Id,card,owner));}
    }
}

