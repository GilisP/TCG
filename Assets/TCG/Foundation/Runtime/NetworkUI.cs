using System;
using System.Linq;
using System.Threading.Tasks;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
 public sealed partial class TableView
 {
  NetworkSession network;bool networkPage,networkInternet=true,networkPublic;string networkAddress="127.0.0.1",networkCode="",networkName="Mesa dos Reinos",networkNick="Jogador",networkNotice="";int networkDeck;Vector2 networkScroll;
  int Viewer=>network!=null&&network.State!=null?network.State.seat:match.Controller;
  bool NetworkTurn=>network==null||!network.Running||network.Connected&&!network.Busy&&network.State!=null&&!network.State.paused&&!network.State.closed&&match.Controller==Viewer;
  void EnsureNetwork(){if(network!=null)return;network=gameObject.AddComponent<NetworkSession>();network.Initialize(catalog,effects);network.Changed+=ReceiveRoom;}
  void ReceiveRoom(RoomReply state)
  {
   if(!state.started||state.view==null||state.view.seats==null)return;
   players=state.capacity;teams=state.teams;useSavedDecks=false;
   if(!match.IsRemoteView){match=Match.FromView(state.view,catalog,effects);world.Bind(match);reactions=new ReactionChannel();menu=libraryOpen=handoff=networkPage=false;sessionAvailable=true;selectedCell=selectedUnit=-1;selectedCard=null;}
   else if(state.view.revision!=match.Revision){match.ApplyView(state.view,catalog);selectedCard=null;selectedCommander=false;blockers=null;lastDefense=null;}
   notice=state.paused?network.Status:match.Controller==Viewer?"Sua prioridade · escolha uma ação":"Aguardando "+match.Seats[match.Controller].Name;handoff=false;
  }
  async void NetworkTask(Func<Task> run){networkNotice="";try{await run();}catch(Exception e){networkNotice=e.Message;}}
  bool SubmitNetwork(Command command){if(!NetworkTurn&&command.kind!=ActionKind.Concede){notice="Aguarde sua prioridade ou a reconexão.";return false;}command.player=Viewer;return network.Send(new RoomRequest{type="action",command=command});}
  DeckData NetworkDeck()
  {
   if(networkDeck>0&&networkDeck<=library.Data.decks.Count){var d=library.Data.decks[networkDeck-1].Copy();foreach(string id in d.main.Concat(d.terrains).Append(d.commander).Where(x=>!string.IsNullOrEmpty(x)).Distinct()){string style=library.SelectedStyle(id,d);bool foil=library.SelectedFoil(id,d);d.cardLooks.RemoveAll(l=>l.cardId==id);d.cardLooks.Add(new CardLook{cardId=id,styleId=style,foil=foil});}return d;}
   // Explicit test preset; it never grants cards or changes the saved collection.
   return new DeckData{name="Sol · teste online",experimental=true,main=Enumerable.Repeat("MED-085",48).ToList(),terrains=Enumerable.Repeat("test-land-0",50).ToList()};
  }
  void DrawNetworkPage()
  {
   EnsureNetwork();Fill(new Rect(0,0,1600,1000),Dark);Text(45,28,1280,54,"MESAS ENTRE JOGADORES",menuTitle,Gold);
   if(Button(1370,30,180,48,"Voltar")){networkPage=false;}
   Text(45,93,1450,42,"Quem cria a sala é o host. A partida fica no computador dele; mantenha o jogo aberto.",body,Muted);
   Fill(new Rect(40,150,720,730),Panel);Fill(new Rect(785,150,775,730),Panel);
   var state=network.State;
   if(network.Running&&state!=null){
    Text(65,175,650,42,"SALA · "+network.Code,heading,Gold);Text(65,224,650,30,state.teams?"Duplas · assentos 1+3 / 2+4":"Todos contra todos",body,Ink);
    foreach(var member in state.members??Array.Empty<RoomMemberView>()){float y=278+member.seat*73;Text(65,y,620,30,(member.seat+1)+" · "+member.name+(member.seat==state.seat?" (você)":""),body,Ink);Text(65,y+30,620,28,member.connected?(member.ready?"Pronto":"Escolhendo deck"):"Desconectado · "+Mathf.CeilToInt(member.remaining)+" s",small,Muted);}
    if(!state.started){Text(65,605,630,28,"Seu deck",body,Gold);if(Button(65,643,630,46,networkDeck==0?"Deck de teste · Sol":library.Data.decks[Math.Min(networkDeck-1,library.Data.decks.Count-1)].name))networkDeck=(networkDeck+1)%(library.Data.decks.Count+1);
     if(Button(65,707,300,48,"Enviar deck",!network.Busy))network.Send(new RoomRequest{type="deck",deck=NetworkDeck()});
     bool ready=state.members.Any(m=>m.seat==state.seat&&m.ready);if(Button(385,707,310,48,ready?"Não estou pronto":"Estou pronto",!network.Busy))network.Send(new RoomRequest{type="ready",ready=!ready});
     if(Button(65,773,630,55,"INICIAR PARTIDA",network.Hosting&&!network.Busy&&state.members.Length==state.capacity&&state.members.All(m=>m.ready&&m.connected),true))network.Send(new RoomRequest{type="start"});
    }else if(Button(65,650,630,55,"Voltar à mesa")){networkPage=menu=libraryOpen=false;}
    Text(815,180,715,60,network.Status,heading,Gold);Text(815,267,705,120,"Convidados têm 2 minutos para reconectar. Durante esse prazo, a partida aguarda. Se o host sair, a sala termina.",body,Ink);
    if(Button(815,440,690,55,"Reconectar",!network.Connected&&!network.Busy))NetworkTask(()=>network.Reconnect());
    if(Button(815,520,690,55,network.Hosting?"Encerrar sala":"Sair da sala"))NetworkTask(async()=>{if(network.Connected)network.Send(new RoomRequest{type="leave"});await network.Stop();StartMatch();menu=true;handoff=false;sessionAvailable=false;});
   }else{
    if(Button(65,175,300,48,"Internet",!network.Running,networkInternet))networkInternet=true;if(Button(385,175,310,48,"Rede local",!network.Running,!networkInternet))networkInternet=false;
    Text(65,242,630,28,"Seu nome",body,Gold);networkNick=GUI.TextField(new Rect(65,277,630,35),networkNick,24);
    Text(65,328,630,28,"Nome da sala",body,Gold);networkName=GUI.TextField(new Rect(65,363,630,35),networkName,50);
    for(int n=2;n<=4;n++)if(Button(65+(n-2)*215,425,200,45,n+" jogadores",!network.Running,players==n)){players=n;if(n!=4)teams=false;}
    if(Button(65,487,630,45,teams?"Duplas":"Todos contra todos",players==4&&!network.Running))teams=!teams;
    if(networkInternet)networkPublic=GUI.Toggle(new Rect(65,553,630,35),networkPublic,"Sala pública · aparecer na busca");else Text(65,553,630,35,"Rede local: convite pelo IP do host",body,Muted);
    if(Button(65,615,630,58,"CRIAR SALA",!network.Busy&&!network.Running,true))NetworkTask(()=>network.Host(networkInternet,networkPublic,networkName,players,teams,networkNick));
    Text(65,706,630,112,networkInternet?"Internet usa autenticação anônima, Relay e diretório de salas Unity. O projeto precisa estar vinculado aos serviços.":"Compartilhe seu IP local. Porta UDP 7777. Os jogadores precisam alcançar o computador do host.",body,Muted);
    Text(815,175,690,38,networkInternet?"ENTRAR COM CÓDIGO":"ENTRAR PELO IP DO HOST",heading,Gold);
    if(networkInternet)networkCode=GUI.TextField(new Rect(815,239,690,40),networkCode,16);else networkAddress=GUI.TextField(new Rect(815,239,690,40),networkAddress,100);
    if(Button(815,302,690,52,"Entrar",!network.Busy&&!network.Running))NetworkTask(()=>network.Join(networkInternet,networkInternet?networkCode:networkAddress,networkNick));
    if(Button(815,386,332,50,"Buscar salas públicas",networkInternet&&!network.Busy&&!network.Running))NetworkTask(()=>network.Search());
    if(Button(1163,386,342,50,"Entrar em uma sala",networkInternet&&!network.Busy&&!network.Running))NetworkTask(()=>network.QuickJoin(networkNick));
    networkScroll=GUI.BeginScrollView(new Rect(815,465,700,330),networkScroll,new Rect(0,0,670,Math.Max(320,network.Results.Length*66)));
    for(int n=0;n<network.Results.Length;n++){var result=network.Results[n];if(Button(0,n*66,660,56,result.Name+" · "+(result.MaxPlayers-result.AvailableSlots)+"/"+result.MaxPlayers,!network.Busy&&!network.Running))NetworkTask(()=>network.Join(true,result.Id,networkNick,true));}GUI.EndScrollView();
    if(network.Running&&Button(815,810,690,45,"Cancelar conexão"))NetworkTask(()=>network.Stop());
   }
   Text(45,907,1500,65,string.IsNullOrEmpty(networkNotice)?string.IsNullOrEmpty(network.LastError)?network.Status:network.LastError:networkNotice,body,Gold);
  }
  void DrawNetworkStatus(){if(network==null||!network.Running)return;if(Button(760,680,410,42,network.State?.paused==true?"Aguardando reconexão · abrir sala":"Sala online · "+network.Status)){networkPage=true;} }
 }
}
