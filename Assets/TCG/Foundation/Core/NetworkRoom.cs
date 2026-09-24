using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
namespace TCG.Foundation
{
 [Serializable] public sealed class RoomRequest { public string type,token,name,content; public int version=1,sequence; public Command command; public DeckData deck; public bool ready; }
 [Serializable] public sealed class RoomMemberView { public int seat; public string name,commander; public bool connected,ready; public float remaining; }
 [Serializable] public sealed class RoomReply { public string error="",token=""; public int seat=-1,ack,capacity; public bool teams,started,paused,closed; public RoomMemberView[] members; public MatchView view; }
 public sealed class NetworkRoom
 {
  sealed class Member { public string token,name; public ulong connection; public bool connected,ready; public double disconnected; public int sequence; public DeckData deck; }
  readonly List<Member> members=new List<Member>();readonly ContentCatalog catalog;readonly EffectRegistry effects;readonly string content;readonly Func<double> clock;
  int cachedRevision=-1;readonly Dictionary<int,MatchView> viewCache=new Dictionary<int,MatchView>();
  readonly List<NetVisual> visuals=new List<NetVisual>();public Match Game {get;private set;} public int Capacity {get;} public bool Teams {get;} public bool Paused=>Game!=null&&!Game.Over&&members.Any(x=>!x.connected&&clock()-x.disconnected<120);public bool Closed {get;private set;}
  public NetworkRoom(ContentCatalog c,EffectRegistry e,string hash,int capacity,bool teams,Func<double> time){if(capacity<2||capacity>4||teams&&capacity!=4)throw new ArgumentException("Escolha 2–4 jogadores; duplas exige 4.");catalog=c;effects=e;content=hash;Capacity=capacity;Teams=teams;clock=time;}
  static string Secret(){var b=new byte[32];using(var r=RandomNumberGenerator.Create())r.GetBytes(b);return Convert.ToBase64String(b);}
  public int SeatFor(ulong connection)=>members.FindIndex(m=>m.connected&&m.connection==connection);
  public RoomReply Receive(ulong connection,RoomRequest r)
  {
   try{
    if(Closed)throw new InvalidOperationException("O host encerrou a sala.");if(r==null||r.version!=1)throw new InvalidOperationException("Versão de rede incompatível.");
    if(r.type=="hello"){
     if(r.content!=content)throw new InvalidOperationException("Atualize o jogo: catálogo ou regras incompatíveis.");
     int existing=SeatFor(connection);if(existing>=0)return Reply(existing);
     Member m=null;
     if(!string.IsNullOrEmpty(r.token)){m=members.FirstOrDefault(x=>x.token==r.token);if(m==null||m.connected||clock()-m.disconnected>=120)throw new InvalidOperationException("Reconexão inválida ou expirada.");}
     else{int free=members.FindIndex(x=>!x.connected&&clock()-x.disconnected>=120);if(Game!=null||members.Count>=Capacity&&free<0)throw new InvalidOperationException("Sala cheia ou partida iniciada.");m=new Member{token=Secret(),name=CleanName(r.name)};if(free>=0)members[free]=m;else members.Add(m);}
     m.connection=connection;m.connected=true;return Reply(members.IndexOf(m));
    }
    int seat=SeatFor(connection);if(seat<0)throw new InvalidOperationException("Entre na sala primeiro.");var member=members[seat];
    if(r.type=="poll")return Reply(seat);
    if(r.sequence<=member.sequence)return Reply(seat);if(r.sequence!=member.sequence+1)throw new InvalidOperationException("Sequência inválida; reconecte.");member.sequence=r.sequence;
    switch(r.type){
     case "deck":if(Game!=null)throw new InvalidOperationException("Partida já iniciada.");ValidateDeck(r.deck);member.deck=r.deck.Copy();member.ready=false;break;
     case "ready":if(Game!=null||member.deck==null)throw new InvalidOperationException("Escolha um deck válido.");member.ready=r.ready;break;
     case "start":if(seat!=0||Game!=null||members.Count!=Capacity||members.Any(x=>!x.connected||!x.ready||x.deck==null))throw new InvalidOperationException("Só o host inicia, com todos prontos.");
      var random=new byte[4];using(var rng=RandomNumberGenerator.Create())rng.GetBytes(random);
      Game=new Match(catalog,effects,Capacity,Teams,BitConverter.ToInt32(random,0),members.Select(x=>x.deck.main.ToArray()).ToArray(),members.Select(x=>x.deck.terrains.ToArray()).ToArray(),members.Select(x=>x.deck.commander).ToArray()){AutomaticResponses=true,AutoAdvanceAfterTerrain=true};Game.Visual+=v=>visuals.Add(new NetVisual{kind=v.Kind,from=v.From,to=v.To,piece=v.Piece,owner=v.Owner,color=v.Color,amount=v.Amount,stackId=v.StackId,card=NetworkCards.Pack(v.Card)});Game.ResolveInitialEffects();break;
     case "action":if(Game==null||Paused)throw new InvalidOperationException("Aguardando partida ou reconexão.");var c=r.command;if(c==null||c.player!=seat||!Enum.IsDefined(typeof(ActionKind),c.kind)||c.units==null||c.blockers==null||c.units.Length>256||c.blockers.Length>256||(c.card?.Length??0)>120)throw new InvalidOperationException("Comando inválido para seu assento.");if(!Game.Try(c,out string error))throw new InvalidOperationException(error);break;
     case "leave":if(seat==0){Closed=true;break;}if(Game!=null&&!Game.Over)Game.Try(new Command{kind=ActionKind.Concede,player=seat,revision=Game.Revision},out _);member.connected=false;member.disconnected=clock()-121;break;
     default:throw new InvalidOperationException("Pedido desconhecido.");
    }
    return Reply(seat);
   }catch(InvalidOperationException e){return new RoomReply{error=e.Message,seat=SeatFor(connection),ack=SeatFor(connection)>=0?members[SeatFor(connection)].sequence:0};}
  }
  static string CleanName(string name)=>string.IsNullOrWhiteSpace(name)?"Jogador":new string(name.Where(c=>!char.IsControl(c)&&c!='<'&&c!='>').Take(24).ToArray());
  void ValidateDeck(DeckData d)
  {
   if(d==null||d.main==null||d.terrains==null||d.main.Count>150||d.terrains.Count>100||d.cardLooks==null||d.cardLooks.Count>300)throw new InvalidOperationException("Deck inválido ou grande demais.");
   var data=new CollectionData{owned=catalog.Cards.Select(c=>c.Id).ToList()};data.cosmetics=CosmeticCatalog.All.Select(c=>c.Id).ToList();data.variants=catalog.Cards.SelectMany(c=>CardStyles.All.Select(style=>CardStyles.Key(c.Id,style))).ToList();
   var errors=new CollectionLibrary(catalog,data).Validate(d,true);if(errors.Count>0)throw new InvalidOperationException(string.Join(" ",errors.Take(3)));
  }
  public void Disconnect(ulong connection){int seat=SeatFor(connection);if(seat<0)return;members[seat].connected=false;members[seat].disconnected=clock();if(seat==0)Closed=true;}
  public void Tick(){if(Game==null||Game.Over)return;for(int i=1;i<members.Count;i++)if(!members[i].connected&&clock()-members[i].disconnected>=120&&!Game.Seats[i].Eliminated)Game.Try(new Command{kind=ActionKind.Concede,player=i,revision=Game.Revision},out _);}
  public RoomReply Reply(int seat)
  {
   var r=new RoomReply{seat=seat,token=members[seat].token,ack=members[seat].sequence,capacity=Capacity,teams=Teams,started=Game!=null,paused=Paused,closed=Closed,members=members.Select((m,i)=>new RoomMemberView{seat=i,name=m.name,commander=m.deck?.commander,connected=m.connected,ready=m.ready,remaining=m.connected?0:(float)Math.Max(0,120-(clock()-m.disconnected))}).ToArray()};
   if(Game!=null){if(cachedRevision!=Game.Revision){viewCache.Clear();cachedRevision=Game.Revision;}if(!viewCache.TryGetValue(seat,out var view)){view=Game.ViewFor(seat);viewCache[seat]=view;}r.view=view;r.view.visuals=visuals.ToArray();r.view.looks=members.SelectMany((m,i)=>m.deck.cardLooks.Where(l=>Game.Board.Any(c=>c.Pieces.Any(p=>p.Owner==i&&p.Card.Id==l.cardId))||i==seat&&Game.HandFor(i).Any(c=>c.Id==l.cardId)||Game.Seats[i].Commander?.Id==l.cardId).Select(l=>new NetLook{owner=i,card=l.cardId,style=l.styleId,foil=l.foil})).ToArray();}return r;
  }
  public void ClearVisuals()=>visuals.Clear();
 }
}
