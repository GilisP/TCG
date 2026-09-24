using System;
using System.Collections.Generic;
using System.Linq;

namespace TCG.Foundation
{
    public enum Stage { Draw, Terrain, Main, End }
    public enum ActionKind { DrawMain, DrawTerrain, Place, NextPhase, Play, Move, Attack, Defend, Pass, Concede, Choose, Equip, Activate, PlanMove, CancelMove, SummonCommander, BoardVehicle, DisembarkVehicle, CastExiled }
    [Serializable] public sealed class Command
    {
        public int player, revision, target=-1, unit=-1, pile=-1;
        public string card;
        public ActionKind kind;
        public int[] units=Array.Empty<int>(), blockers=Array.Empty<int>();
    }
    public sealed class Piece
    {
        public int Id { get; } public int Owner { get; } public Definition Card { get; }
        public int Damage { get; internal set; } public int Movement { get; internal set; } int actionsRemaining; public int Actions { get=>Math.Max(0,actionsRemaining+(Game?.ActionAura(this)??0)); internal set {int before=Actions;actionsRemaining=value-(Game?.ActionAura(this)??0);Game?.ActionsSpent(this,before,Actions);} }
        internal int PermanentActions,DeathPower,BeforeLethalDamage; internal int? BaseAttackOverride,BaseDefenseOverride; internal bool CombatLethal; internal Match Game; internal bool CommandUnit; internal bool Token; internal int ExpireTurn=-1;
        internal int LastDamagedTurn=-1;
        int originalOwner=-1;public int OriginalOwner {get=>originalOwner<0?Owner:originalOwner;internal set=>originalOwner=value;} public int CarrierId {get;internal set;}=-1;
        public int AttachedTo { get; internal set; }=-1;
        public int LastMovedTurn { get; internal set; }=-100; public int PreviousOwnTurn { get; internal set; }=-1; internal int LastOwnTurn=-1;
        internal int OnceTurn=-1; internal readonly List<Modifier> Modifiers=new List<Modifier>();
        internal int NormalAttack=>Math.Max(0,(BaseAttackOverride??Game?.BasePower(this)??Card.Attack)+(Game?.Bonus(this,0)??0));
        internal int NormalDefense=>(BaseDefenseOverride??Game?.BaseToughness(this)??Card.Defense)+(Game?.Bonus(this,1)??0);
        internal int? NetAttack,NetDefense,NetRange;
        public int Attack => NetAttack ?? ( Game!=null&&Game.Inverted(this)?NormalDefense:NormalAttack);
        public int Defense => NetDefense ?? (Game!=null&&Game.Inverted(this)?NormalAttack:NormalDefense);
        public int Range => NetRange ?? Math.Max(1,Card.Range+(Game?.Bonus(this,2)??0));
        public int Health => Defense-Damage;
        internal Piece(int id,int owner,Definition card) { Id=id; Owner=owner; Card=card; Movement=card.Movement; Actions=card.Actions; }
    }
    public sealed class Cell
    {
        public int Owner { get; internal set; }=-1;
        public int CapitalOwner { get; internal set; }=-1;
        public int TerrainOwner { get; internal set; }=-1;
        public Definition Terrain { get; internal set; }
        internal readonly List<Piece> pieces=new List<Piece>();
        public IReadOnlyList<Piece> Pieces => pieces.AsReadOnly();
    }
    public sealed class TerrainCard
    {
        public Definition Card { get; } public int Owner { get; }
        public TerrainCard(Definition card,int owner) { Card=card; Owner=owner; }
    }
    public sealed class Seat
    {
        public string Name { get; } public int Team { get; } public int Life { get; internal set; }=50;
        public bool Eliminated { get; internal set; }
        internal readonly int[] mana=new int[7];
        internal readonly List<Definition> main=new List<Definition>(), terrainDeck=new List<Definition>(), hand=new List<Definition>(), grave=new List<Definition>(), terrainGrave=new List<Definition>(),exile=new List<Definition>();
        internal List<TerrainCard>[] piles;
        public IReadOnlyList<int> Mana => Array.AsReadOnly(mana);
        public Definition Commander {get;internal set;} public bool CommanderReady {get;internal set;} public int CommanderCasts {get;internal set;}
        public int DeathCounters {get;internal set;} public int LastCreatedTerrain {get;internal set;}=-1;
        internal int? NetHand,NetMain,NetTerrain; internal int[] NetPiles;
        public int HandCount=>NetHand??hand.Count; public int MainCount=>NetMain??main.Count; public int TerrainCount=>NetTerrain??terrainDeck.Count;
        public IReadOnlyList<Definition> Exile=>exile.AsReadOnly(); public IReadOnlyList<Definition> Graveyard=>grave.AsReadOnly(); public IReadOnlyList<Definition> TerrainGraveyard=>terrainGrave.AsReadOnly();
        internal Seat(string name,int team) { Name=name; Team=team; }
        public Definition Top(int pile)=>piles[pile].Count==0 ? null : piles[pile][piles[pile].Count-1].Card;
        public int TopOwner(int pile)=>piles[pile].Count==0?-1:piles[pile][piles[pile].Count-1].Owner;
        public int PileCount(int pile)=>NetPiles==null?piles[pile].Count:NetPiles[pile];
    }
    public sealed class Pending
    {
        public string VisualId { get; internal set; }=Guid.NewGuid().ToString("N");
        public int Owner { get; internal set; } public int Target { get; internal set; } public int TargetUnit { get; internal set; }
        public Definition Card { get; internal set; } public int[] Attackers { get; internal set; }=Array.Empty<int>();
        public int[] Blockers { get; internal set; }=Array.Empty<int>(); public int Defender { get; internal set; }=-1; public int Chooser { get; internal set; }
        public bool NeedsDefense { get; internal set; }
        internal bool ExileAfter; internal int CardOwner=-1; internal int Amount;internal Definition ChosenCard; internal bool CommandUnit; internal string Rule; internal Piece Source; internal int SourceCell=-1; internal Piece Subject; internal Action Continuation;
        public string Description => Card!=null ? Card.Name : "Ataque de "+Attackers.Length+" criatura(s)";
    }
    public sealed class MatchEvent
    {
        public readonly string Kind; public readonly int From,To,Piece; public readonly Definition Card; public readonly int Owner,Color,Amount; public readonly string StackId;
        public MatchEvent(string kind,int from,int to,int piece=-1,Definition card=null,int owner=-1,int color=-1,int amount=0,string stackId=null) { Kind=kind; From=from; To=to; Piece=piece; Card=card; Owner=owner; Color=color; Amount=amount; StackId=stackId; }
    }
    public interface IEffectHandler
    {
        string Key { get; }
        bool NeedsEnemy { get; }
        void Resolve(Match match,Pending item,int amount);
    }
    public sealed class DamageEffect : IEffectHandler
    {
        public string Key=>"damage"; public bool NeedsEnemy=>true;
        public void Resolve(Match m,Pending item,int amount) { var unit=m.Find(item.TargetUnit); if(unit!=null && m.Enemies(item.Owner,unit.Owner)) m.DamageTo(unit,amount,null,true,item.Owner); }
    }
    public sealed class DrawEffect : IEffectHandler
    {
        public string Key=>"draw"; public bool NeedsEnemy=>false;
        public void Resolve(Match m,Pending item,int amount) { for(int i=0;i<amount&&!m.Seats[item.Owner].Eliminated;i++) m.DrawCard(item.Owner); }
    }
    public sealed class ManaEffect : IEffectHandler
    {
        public string Key=>"mana"; public bool NeedsEnemy=>false;
        public void Resolve(Match m,Pending item,int amount) { m.AddMana(item.Owner,item.Card.Color,amount); }
    }
    public sealed class EffectRegistry
    {
        readonly Dictionary<string,IEffectHandler> handlers=new Dictionary<string,IEffectHandler>(StringComparer.Ordinal);
        public IEnumerable<string> Keys=>handlers.Keys;
        public EffectRegistry() { Register(new DamageEffect()); Register(new DrawEffect()); Register(new ManaEffect()); }
        public void Register(IEffectHandler handler) { if(handler==null) throw new ArgumentNullException(nameof(handler)); handlers.Add(handler.Key,handler); }
        public IEffectHandler Get(string key)=>handlers[key];
    }
    public sealed partial class Match
    {
        public const int Width=11;
        public static readonly int[] Capitals={55,5,65,115}; // Center of each edge, confirmed by the author on 2026-09-19.
        readonly Cell[] cells=Enumerable.Range(0,121).Select(_=>new Cell()).ToArray();
        readonly List<TerrainCard>[] terrainPiles=Enumerable.Range(0,4).Select(_=>new List<TerrainCard>()).ToArray();
        readonly Seat[] seats; readonly List<Pending> stack=new List<Pending>(); readonly List<string> log=new List<string>();
        readonly EffectRegistry effects; int nextPiece=1, passes; int[] turnOrder;
        public IReadOnlyList<Cell> Board=>Array.AsReadOnly(cells); public IReadOnlyList<Seat> Seats=>Array.AsReadOnly(seats);
        public IReadOnlyList<Pending> Stack=>stack.AsReadOnly(); public IReadOnlyList<string> Log=>log.AsReadOnly();
        public int Active { get; private set; } public int Priority { get; private set; } public int Revision { get; private set; }
        public int Turn { get; private set; }=1; public Stage Phase { get; private set; }
        public bool Placed { get; private set; } public bool Over { get; private set; } public int WinningTeam { get; private set; }=-1;
        public event Action<MatchEvent> Visual;
        public Pending Defense=>stack.Count>0&&stack[stack.Count-1].NeedsDefense?stack[stack.Count-1]:null;
        public int Controller=>remote?.controller ?? Choice?.Owner ?? Defense?.Chooser ?? Priority;
        public Match(ContentCatalog catalog,EffectRegistry registry,int count,bool teams,int seed,IReadOnlyList<string[]> main,IReadOnlyList<string[]> terrains,IReadOnlyList<string> commanders=null)
        {
            Check(count>=2&&count<=4,"A mesa suporta de 2 a 4 jogadores."); Check(!teams||count==4,"Duplas exige quatro jogadores.");
            Check(main!=null&&terrains!=null&&main.Count==count&&terrains.Count==count,"Decks ausentes.");
            effects=registry ?? throw new ArgumentNullException(nameof(registry));
            for(int p=0;p<count;p++) { var errors=catalog.ValidateDeck(main[p],terrains[p],false); Check(errors.Count==0,string.Join(" ",errors)); }
            seats=Enumerable.Range(0,count).Select(i=>new Seat(new[]{"Âmbar","Jade","Safira","Rubi"}[i],teams ? i%2 : i)).ToArray();
            if(commanders!=null){Check(commanders.Count==count,"Comandantes ausentes.");for(int p=0;p<count;p++)if(!string.IsNullOrEmpty(commanders[p])){var cmd=catalog.Get(catalog.Identity(commanders[p]));Check(cmd.IsCommander&&cmd.Playable,"Comandante inválido.");seats[p].Commander=cmd;seats[p].CommanderReady=true;}}
            SetupAuthorDefinitions(catalog,seed);
            var random=new Random(seed);
            for(int p=0;p<count;p++)
            {
                var seat=seats[p]; seat.piles=terrainPiles; seat.main.AddRange(main[p].Select(id=>catalog.Get(catalog.Identity(id)))); seat.terrainDeck.AddRange(terrains[p].Select(id=>catalog.Get(catalog.Identity(id))));
                Shuffle(seat.main,random); Shuffle(seat.terrainDeck,random);
                for(int n=0;n<5;n++) seat.hand.Add(Pop(seat.main)); // Explicit five-card test-room preset; full preparation remains a design question.
                int capital=Capital(p,count); cells[capital].CapitalOwner=p; SetTerrain(capital,p,Pop(seat.terrainDeck));
                foreach(int neighbor in Neighbors(capital).Take(3)) SetTerrain(neighbor,p,Ruins);

            }
            var contributors=Enumerable.Range(0,count).ToList(); Shuffle(contributors,random);
            var opening=new List<TerrainCard>();
            for(int n=0;n<8;n++){int owner=contributors[n%count];opening.Add(new TerrainCard(Pop(seats[owner].terrainDeck),owner));}
            Shuffle(opening,random);
            for(int n=0;n<8;n++)terrainPiles[n%4].Add(opening[n]);
            Active=random.Next(count);
            turnOrder=count==3 ? new[]{Active}.Concat(Enumerable.Range(0,count).Where(p=>p!=Active).OrderBy(p=>Math.Abs(Capital(p,count)%11-Capital(Active,count)%11)+Math.Abs(Capital(p,count)/11-Capital(Active,count)/11))).ToArray() : Enumerable.Range(0,count).ToArray();
            BeginTurn(); Note("Mesa de teste: cartas e decks provisórios; primeiro jogador sorteado.");
        }
        public void ResolveInitialEffects(){Drain();StateCheck();FinishCheck();AdvanceAutomaticResponses();}
        public static int Capital(int seat,int count)=>count==2 ? Capitals[seat*2] : Capitals[seat];
        public static IEnumerable<int> Neighbors(int i)
        { if(i%11>0) yield return i-1; if(i%11<10) yield return i+1; if(i>=11) yield return i-11; if(i<110) yield return i+11; }
        public static bool Adjacent(int a,int b)=>Valid(a)&&Valid(b)&&Math.Abs(a%11-b%11)+Math.Abs(a/11-b/11)==1;
        static bool Valid(int i)=>i>=0&&i<121;
        static void Check(bool condition,string message) { if(!condition) throw new InvalidOperationException(message); }
        static T Pop<T>(List<T> list) { var result=list[list.Count-1]; list.RemoveAt(list.Count-1); return result; }
        static void Shuffle<T>(List<T> list,Random r) { for(int i=list.Count-1;i>0;i--) { int j=r.Next(i+1); var a=list[i]; list[i]=list[j]; list[j]=a; } }
        public bool Enemies(int a,int b)=>a>=0&&b>=0&&a<seats.Length&&b<seats.Length&&seats[a].Team!=seats[b].Team&&!seats[b].Eliminated;
        public Piece Find(int id)=>cells.SelectMany(c=>c.pieces).FirstOrDefault(p=>p.Id==id);
        public int Position(int id)=>Array.FindIndex(cells,c=>c.pieces.Any(p=>p.Id==id));
        public IReadOnlyList<Definition> HandFor(int requester)=>requester>=0&&requester<seats.Length ? seats[requester].hand.AsReadOnly() : Array.Empty<Definition>();
        public bool NeedsTarget(Definition card)=>card.Permanent||card.Effects.Any(e=>effects.Get(e.Operation).NeedsEnemy);
        public bool CanPlace(int index)=>IsRemoteView?NetCan("place",index):Valid(index)&&Phase==Stage.Terrain&&!Placed&&stack.Count==0&&!Over&&
            (cells[index].Owner==Active || (cells[index].Terrain==null&&Neighbors(index).Any(n=>cells[n].Owner==Active&&cells[n].Terrain!=null)));
        public bool CanPlay(Definition card,int target,int unit=-1)
        {if(IsRemoteView)return card!=null&&NetCan("play",target,unit,card.Id);
            if(card==null||card.Rule=="destiny"||!card.Playable||Over||Choice!=null||Defense!=null||card.Kind==CardType.Terrain||!CanPay(Priority,card)) return false;
            if(card.Kind!=CardType.Instant && (Phase!=Stage.Main||Priority!=Active||stack.Count!=0)) return false;
            if(card.Kind==CardType.Instant && Phase!=Stage.Main&&Phase!=Stage.End&&stack.Count==0) return false;
            if(card.Rule=="test-counter"&&!ResponseTargets(card,Priority))return false;
            if(card.Kind==CardType.Enchantment){var host=Find(unit);return host!=null&&host.Card.Kind==CardType.Creature&&Position(unit)==target&&!Immune(host,Priority);} if(card.Permanent) return Valid(target)&&cells[target].Terrain!=null&&cells[target].Owner==Priority;
            if(card.Effects.Any(e=>effects.Get(e.Operation).NeedsEnemy)) { var piece=Find(unit); return piece!=null&&Enemies(Priority,piece.Owner)&&Position(unit)==target; }
            return true;
        }
        public bool Try(Command c,out string error)
        {
            error="";
            if(IsRemoteView){error="Envie a ação ao host.";return false;}
            try
            {
                Check(c!=null,"Ação ausente."); Check(!Over,"A partida terminou."); Check(c.revision==Revision,"Ação antiga; atualize a mesa.");
                Check(c.player>=0&&c.player<seats.Length&&!seats[c.player].Eliminated,"Jogador inválido.");
                foreach(var piece in cells.SelectMany(x=>x.pieces)) piece.Game=this;
                if(c.kind==ActionKind.Concede) { Eliminate(c.player); FinishCheck(); Drain(); AdvanceAutomaticResponses(); AdvanceMovementOrders(); AdvanceMissions(); Revision++; return true; }
                Check(c.player==Controller,"Aguarde sua prioridade.");
                if(Choice!=null) Check(c.kind==ActionKind.Choose,"Conclua a escolha pendente.");
                if(Defense!=null&&Choice==null) Check(c.kind==ActionKind.Defend,"O defensor deve distribuir os bloqueios.");
                switch(c.kind)
                {
                    case ActionKind.DrawMain: Buy(false,c.pile); break;
                    case ActionKind.DrawTerrain: Buy(true,c.pile); break;
                    case ActionKind.Place: Place(c.pile,c.target); break;
                    case ActionKind.NextPhase: NextPhase(); break;
                    case ActionKind.SummonCommander: SummonCommander(c.target); break;
                    case ActionKind.Play: Play(c.card,c.target,c.unit); break;
                    case ActionKind.Move: Move(c.unit,c.target); movementOrders.Remove(c.unit); break;
                    case ActionKind.PlanMove: PlanMovement(c.unit,c.target); break;
                    case ActionKind.CancelMove: CancelMovement(c.unit); break;
                    case ActionKind.Attack: Attack(c.units,c.target); break;
                    case ActionKind.Defend: Defend(c.blockers); break;
                    case ActionKind.Pass: Pass(); break;
                    case ActionKind.Choose: Choose(c.target); break;
                    case ActionKind.Equip: Equip(c.unit,c.target); break;
                    case ActionKind.BoardVehicle: BoardVehicle(c.unit,c.target); break;
                    case ActionKind.DisembarkVehicle: DisembarkVehicle(c.unit); break;
                    case ActionKind.Activate: Activate(c.unit); break; case ActionKind.CastExiled: CastExiled(c.target); break;
                    default: throw new InvalidOperationException("Ação não suportada.");
                }
                CountAction(c); Drain(); StateCheck(); FinishCheck(); AdvanceAutomaticResponses(); AdvanceMovementOrders(); AdvanceMissions(); Revision++; return true;
            }
            catch(InvalidOperationException e) { error=e.Message; return false; }
        }
        void MainOnly() { Check(Phase==Stage.Main&&Priority==Active&&stack.Count==0&&Choice==null,"Exige fase principal, sua prioridade e pilha vazia."); }
        void Buy(bool terrain,int pile)
        {
            Check(Phase==Stage.Draw&&Priority==Active&&stack.Count==0,"A compra é feita no início do turno."); Check(pile>=0&&pile<4,"Escolha a pilha que receberá o terreno.");
            if(Turn==1) { if(terrain) DrawTerrain(Active,pile); else DrawCard(Active); }
            else { DrawCard(Active); if(!seats[Active].Eliminated) DrawTerrain(Active,pile); }
            AuthorBuy();
            if(!Over&&!seats[Active].Eliminated) { Phase=Stage.Terrain; Note("Compra concluída; coloque um terreno."); }
        }
        internal void DrawCard(int owner)
        { if(seats[owner].main.Count==0) { Eliminate(owner); Note(seats[owner].Name+" tentou comprar do principal vazio."); } else seats[owner].hand.Add(Pop(seats[owner].main)); }
        void DrawTerrain(int owner,int pile) { if(seats[owner].terrainDeck.Count>0) terrainPiles[pile].Add(new TerrainCard(Pop(seats[owner].terrainDeck),owner)); }
        void SetTerrain(int index,int owner,TerrainCard card) { SetTerrain(index,owner,card.Card); cells[index].TerrainOwner=card.Owner; }
        void SetTerrain(int index,int owner,Definition card) { cells[index].Terrain=card; cells[index].Owner=owner; cells[index].TerrainOwner=owner; }
        void Place(int pile,int target)
        {
            Check(pile>=0&&pile<4&&seats[Active].Top(pile)!=null,"Escolha uma pilha com terreno."); Check(CanPlace(target),"Terreno exige território próprio ou vazio adjacente ao reino.");
            var cell=cells[target]; foreach(var piece in cell.pieces.ToArray()) Kill(piece);
            if(cell.Terrain!=null&&cell.Terrain.Rule!="ruins"&&cell.TerrainOwner>=0) seats[cell.TerrainOwner].terrainGrave.Add(cell.Terrain);
            SetTerrain(target,Active,Pop(seats[Active].piles[pile])); seats[Active].LastCreatedTerrain=target; if(seats[Active].piles[pile].Count==0) DrawTerrain(Active,pile);
            Placed=true; Note(seats[Active].Name+" colocou "+cell.Terrain.Name+"."); Visual?.Invoke(new MatchEvent("terrain",target,target));
            if(AutoAdvanceAfterTerrain) { Phase=Stage.Main; passes=0; QueueMovementOrders(); }
        }
        void NextPhase()
        {
            Check(Priority==Active&&stack.Count==0,"Resolva a pilha primeiro.");
            if(Phase==Stage.Terrain) { Check(Placed||!Enumerable.Range(0,4).Any(p=>seats[Active].Top(p)!=null)||!Enumerable.Range(0,121).Any(CanPlace),"A colocação de terreno é obrigatória."); Phase=Stage.Main; QueueMovementOrders(); }
            else if(Phase==Stage.Main) Phase=Stage.End;
            else if(Phase==Stage.End) { Pass(); return; }
            else throw new InvalidOperationException("Escolha a compra primeiro.");
            passes=0;
        }
        bool CanPay(int owner,Definition card)
        { var mana=seats[owner].mana; for(int i=0;i<7;i++) if(mana[i]<card.ColoredCost[i]) return false; return mana.Sum()-card.ColoredCost.Sum()>=card.Cost; }
        void Pay(int owner,Definition card)
        { var mana=seats[owner].mana; for(int i=0;i<7;i++) mana[i]-=card.ColoredCost[i]; int left=card.Cost; for(int i=6;i>=0&&left>0;i--) { int v=Math.Min(left,mana[i]); mana[i]-=v; left-=v; } }
        internal void AddMana(int owner,int color,int amount,int from=-1) { seats[owner].mana[color]+=amount; if(amount>0)Visual?.Invoke(new MatchEvent("mana",from>=0?from:Capital(owner,seats.Length),Capital(owner,seats.Length),owner:owner,color:color,amount:amount)); }
        void Play(string id,int target,int unit)
        {
            var card=seats[Priority].hand.FirstOrDefault(c=>c.Id==id); Check(card!=null,"Carta não está na sua mão."); Check(CanPlay(card,target,unit),"Carta indisponível: confira fase, mana e alvo.");
            Pay(Priority,card); seats[Priority].hand.Remove(card); stack.Add(new Pending{Owner=Priority,Card=card,Target=target,TargetUnit=unit,CommandUnit=card==seats[Priority].Commander}); passes=0; Note(seats[Priority].Name+" anunciou "+card.Name+".");
            ReflectDeclaredTarget(stack.Last()); AuthorPlayed(Priority,card);
            if(card.Kind==CardType.Spell||card.Kind==CardType.Instant) Visual?.Invoke(new MatchEvent("cast",Capital(Priority,seats.Length),Capital(Priority,seats.Length),-1,card,Priority));
        }
        public bool CanMove(int unit,int target)
        {if(IsRemoteView)return NetCan("move",unit,target); var p=Find(unit); return !Over&&Phase==Stage.Main&&Priority==Active&&stack.Count==0&&p!=null&&p.Owner==Active&&Mobile(p)&&p.Movement>0&&MovePath(p,target)&&cells[target].Terrain!=null&&!Blocked(p,target)&&!cells[target].pieces.Any(u=>(u.Card.Kind!=CardType.Equipment&&u.Card.Kind!=CardType.Enchantment)&&Enemies(Active,u.Owner))&&!Enemies(Active,cells[target].CapitalOwner); }
        void Move(int id,int target)
        {
            MainOnly(); Check(CanMove(id,target),"Movimento exige terreno adjacente sem inimigos e movimento disponível.");
            var p=Find(id); if(GuardInvasion(p,target))return; int from=Position(id); cells[from].pieces.Remove(p); cells[target].pieces.Add(p); p.Movement-=PortalLink(from,target)?1:Distance(from,target); MarkMoved(p,from,target);
            if(cells[target].Owner<0||Enemies(Active,cells[target].Owner)||seats[cells[target].Owner].Eliminated) cells[target].Owner=Active;
            Visual?.Invoke(new MatchEvent("move",from,target,id)); Note(p.Card.Name+" avançou.");
        }
        public bool CanAttack(int id,int target)
        {if(IsRemoteView)return NetCan("attack",id,target); var p=Find(id); return !Over&&Phase==Stage.Main&&Priority==Active&&stack.Count==0&&p!=null&&p.Owner==Active&&(p.Card.Kind==CardType.Creature||p.Card.IsVehicle)&&CanAct(p)&&p.Actions>0&&p.Movement>0&&InRange(p,target)&&cells[target].Terrain!=null&&(cells[target].pieces.Any(u=>(u.Card.Kind!=CardType.Equipment&&u.Card.Kind!=CardType.Enchantment)&&Enemies(Active,u.Owner))||Enemies(Active,cells[target].CapitalOwner)); }
        void Attack(int[] ids,int target)
        {
            MainOnly(); Check(ids!=null&&ids.Length>0&&ids.Distinct().Count()==ids.Length&&ids.All(i=>CanAttack(i,target)),"Selecione atacantes aptos e um terreno inimigo adjacente.");

            var owners=cells[target].pieces.Where(p=>(p.Card.Kind!=CardType.Equipment&&p.Card.Kind!=CardType.Enchantment)&&Enemies(Active,p.Owner)).Select(p=>p.Owner).Distinct().ToArray();
            Check(owners.Length<=1,"Combate com defensores de múltiplos donos aguarda definição; escolha outro alvo.");
            int defender=owners.Length==1 ? owners[0] : cells[target].CapitalOwner;
            foreach(int id in ids) { Find(id).Actions--; Find(id).Movement--; }
            foreach(int id in ids) OnAttack(Find(id),target);
            int chooser=cells[target].CapitalOwner==defender?defender:Active;
            stack.Add(new Pending{Owner=Active,Target=target,Attackers=(int[])ids.Clone(),Defender=defender,Chooser=chooser,NeedsDefense=true}); passes=0;
            GuardAttack(target,defender);
            Note(seats[Active].Name+" declarou ataque. "+seats[chooser].Name+" escolhe os confrontos.");
        }
        void Defend(int[] blockers)
        {
            var battle=Defense; Check(battle!=null&&blockers!=null&&blockers.Length==battle.Attackers.Length,"Atribua um bloqueio para cada atacante.");
            bool capital=cells[battle.Target].CapitalOwner==battle.Defender;
            for(int i=0;i<blockers.Length;i++) Check(blockers[i]==-1 ? capital : cells[battle.Target].pieces.Any(p=>p.Id==blockers[i]&&(p.Card.Kind!=CardType.Equipment&&p.Card.Kind!=CardType.Enchantment)&&p.Owner==battle.Defender),"Bloqueador inválido; apenas a capital aceita ataques sem bloqueio.");
            battle.Blockers=(int[])blockers.Clone(); battle.NeedsDefense=false; Priority=battle.Owner; passes=0;
        }
        void Pass()
        {
            Check(Defense==null,"Defina os bloqueios primeiro."); Check(stack.Count>0||Phase==Stage.Main||Phase==Stage.End,"Não há prioridade para passar nesta fase.");
            passes++;
            if(passes<seats.Count(s=>!s.Eliminated)) { Priority=Next(Priority); return; }
            passes=0;
            if(stack.Count>0) { var item=stack[stack.Count-1]; stack.RemoveAt(stack.Count-1); Resolve(item); completedVisuals.Add(item); if(!Over) Priority=Active; }
            else if(Phase==Stage.Main) { Phase=Stage.End; Priority=Active; }
            else { EndModifiers(); StateCheck(); if(stack.Count>0||Choice!=null||work.Count>0){Priority=Active;return;} Clean(Active); Active=Next(Active); Turn++; BeginTurn(); }
        }
        void Resolve(Pending item)
        {
            if(seats[item.Owner].Eliminated) return;
            if(item.Rule!=null) { ResolveRule(item); Drain(); FinishCheck(); return; }
            if(item.Card!=null)
            {
                if(item.Card.Kind==CardType.Enchantment){var host=Find(item.TargetUnit);if(host!=null&&!Immune(host,item.Owner)){var enchant=SpawnNew(item.Owner,item.Card,Position(host.Id),item.CardOwner);enchant.AttachedTo=host.Id;}else FinishSpell(item);} else if(item.Card.Permanent)
                {
                    if(cells[item.Target].Terrain!=null&&cells[item.Target].Owner==item.Owner) { var p=new Piece(nextPiece++,item.Owner,item.Card){Game=this,CommandUnit=item.CommandUnit,OriginalOwner=item.CardOwner<0?item.Owner:item.CardOwner}; cells[item.Target].pieces.Add(p); Enter(p); Visual?.Invoke(new MatchEvent("summon",item.Target,item.Target,p.Id)); }
                    else FinishSpell(item);
                }
                else { if(!string.IsNullOrEmpty(item.Card.Rule)) ResolveRule(new Pending{Owner=item.Owner,Card=item.Card,Rule=item.Card.Rule,Target=item.Target,TargetUnit=item.TargetUnit}); foreach(var e in item.Card.Effects) { if(seats[item.Owner].Eliminated) break; effects.Get(e.Operation).Resolve(this,item,e.Amount); } if(!seats[item.Owner].Eliminated) FinishSpell(item); }
                if(string.IsNullOrEmpty(item.Card.Rule)&&(item.Card.Kind==CardType.Spell||item.Card.Kind==CardType.Instant)) Visual?.Invoke(new MatchEvent("resolve",Capital(item.Owner,seats.Length),item.Target>=0?item.Target:Capital(item.Owner,seats.Length),-1,item.Card,item.Owner));
                Note(item.Card.Name+" resolveu.");
            }
            else ResolveCombat(item);
            FinishCheck();
        }
        void ResolveCombat(Pending item)
        {
            ArenaEnter(item); combatDeathObservers=All.ToArray();
            var hits=new List<(Piece target,int amount,Piece source)>(); var applied=new List<(Piece target,int amount,Piece source)>(); var killers=new Dictionary<Piece,Piece>(); var damage=new Dictionary<Piece,int>(); var budgets=new Dictionary<int,int>(); int capitalDamage=0;
            for(int i=0;i<item.Attackers.Length;i++)
            {
                var a=Find(item.Attackers[i]); if(a==null||!InRange(a,item.Target)) continue;
                Visual?.Invoke(new MatchEvent("attack",Position(a.Id),item.Target,a.Id,a.Card,a.Owner));
                int id=item.Blockers[i];
                if(id==-1) { if(Enemies(item.Owner,cells[item.Target].CapitalOwner)) {capitalDamage+=a.Attack;AuthorAfterDamage(a,a.Attack);MarkCommanderDamage(a,cells[item.Target].CapitalOwner,a.Attack);} continue; }
                var b=Find(id); if(b==null||Position(id)!=item.Target||!Enemies(a.Owner,b.Owner)) continue;
                killers[b]=a; killers[a]=b;
                int outgoing=a.Attack+(Has(a,"attack-wounded")&&b.LastDamagedTurn==Turn?1:0);
                hits.Add((b,outgoing,a)); damage[b]=(damage.TryGetValue(b,out int prior)?prior:0)+outgoing;
                if(!budgets.ContainsKey(id)) budgets[id]=b.Card.Kind==CardType.Construction||!CanAct(b)?0:b.Attack;
                int hit=Math.Min(budgets[id],Math.Max(0,a.Health)); budgets[id]-=hit; hits.Add((a,hit,b)); damage[a]=(damage.TryGetValue(a,out prior)?prior:0)+hit;
            }
            // Damage is assigned from the same snapshot, then deaths are checked together.
            foreach(var hit in hits) {int before=hit.target.Damage;DamageTo(hit.target,hit.amount,hit.source,false);applied.Add((hit.target,hit.target.Damage-before,hit.source));}
            foreach(var hit in applied)AfterDamage(hit.target,hit.amount,hit.source);
            var casualties=damage.Keys.Where(p=>p.Health<=0).ToArray(); foreach(var p in casualties){p.CombatLethal=true;p.DeathPower=p.Attack;}
            foreach(var p in casualties) { var killer=killers.TryGetValue(p,out var source)?source:null; p.CombatLethal=true; Kill(p,killer?.Owner??-1); if(killer!=null&&p.Card.Kind==CardType.Creature) KillReward(killer); }
            combatDeathObservers=null;
            foreach(int id in item.Attackers) { var a=Find(id); if(a!=null) AfterAttack(a); }
            foreach(var continuation in afterBattle.ToArray()) continuation(); afterBattle.Clear();
            int owner=cells[item.Target].CapitalOwner; if(owner>=0&&capitalDamage>0) { seats[owner].Life-=capitalDamage; if(seats[owner].Life<=0) Eliminate(owner); }
            foreach(var p in All)p.Modifiers.RemoveAll(m=>m.CombatOnly);
            Note("Combate resolvido; sobreviventes permanecem na origem.");
        }
        internal void Deal(Piece p,int amount) { DamageTo(p,amount,null,true); }
        void Kill(Piece p,int killer=-1) {if(OfferDeathResponse(p,killer))return;CommitKill(p,killer);}
        void CommitKill(Piece p,int killer=-1) { int pos=Position(p.Id); if(pos<0) return; if(!p.CombatLethal)p.DeathPower=p.Attack; NewBeforeDeath(p,killer); BicolorBeforeDeath(p,pos); VehicleDestroyed(p,killer); p.CarrierId=-1; Detach(p); cells[pos].pieces.Remove(p); if(!p.Token)seats[p.OriginalOwner].grave.Add(p.Card); CommanderDied(p); OnDeath(p,pos,killer); Visual?.Invoke(new MatchEvent("death",pos,pos,p.Id,p.Card,p.Owner)); }
        void Clean(int owner) { foreach(var p in cells.SelectMany(c=>c.pieces).Where(p=>p.Owner==owner)) p.Damage=0; }
        void BeginTurn()
        {
            Priority=Active; Phase=Stage.Draw; Placed=false; passes=0; Clean(Active); Array.Clear(seats[Active].mana,0,7);
            foreach(var c in cells)
            {
                if(c.Owner==Active&&c.Terrain!=null&&c.Terrain.Rule!="ruins") AddMana(Active,c.Terrain.Rule=="portals"?authorRandom.Next(6):c.Terrain.Color,1,Array.IndexOf(cells,c));
                foreach(var p in c.pieces.Where(p=>p.Owner==Active)) { p.Movement=p.Card.Movement; p.Actions=p.Card.Actions+p.PermanentActions+EquipmentBonus(p,3)+ActionAura(p); p.PreviousOwnTurn=p.LastOwnTurn; p.LastOwnTurn=Turn; }
            }
            StartModifiers(); TurnTriggers();
            Note("Turno "+Turn+" — "+seats[Active].Name+" renovou os recursos.");
        }
        int Next(int player) { int at=Array.IndexOf(turnOrder,player); for(int n=1;n<=seats.Length;n++) { int i=turnOrder[(at+n)%seats.Length]; if(!seats[i].Eliminated) return i; } return player; }
        void Eliminate(int owner)
        {
            if(seats[owner].Eliminated) return; seats[owner].Eliminated=true; if(Choice?.Owner==owner)Choice=null; seats[owner].hand.Clear(); seats[owner].main.Clear(); seats[owner].terrainDeck.Clear();
            seats[owner].grave.Clear(); seats[owner].terrainGrave.Clear(); Array.Clear(seats[owner].mana,0,7);
            foreach(var c in cells) c.pieces.RemoveAll(p=>p.Owner==owner); stack.RemoveAll(p=>p.Owner==owner||p.Defender==owner); passes=0;
            Note(seats[owner].Name+" foi eliminado; seus terrenos permanecem.");
        }
        void FinishCheck()
        {
            var alive=seats.Where(s=>!s.Eliminated).Select(s=>s.Team).Distinct().ToArray();
            if(alive.Length<=1) { Over=true; WinningTeam=alive.Length==1?alive[0]:-1; return; }
            if(seats[Active].Eliminated) { Active=Next(Active); Turn++; BeginTurn(); }
            if(seats[Priority].Eliminated) Priority=Next(Priority);
        }
        void Note(string text) { log.Add(text); if(log.Count>150) log.RemoveAt(0); }
    }
}




