using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using TCG.Foundation;
namespace TCG.Table
{
 public sealed class NetworkSession:MonoBehaviour
 {
  public string LastError {get;private set;}=""; public event Action<RoomReply> Changed;public string Status {get;private set;}="";public string Code=>session?.Code??"LAN";public bool Busy {get;private set;}public bool Hosting=>manager!=null&&manager.IsHost;public bool Connected=>manager!=null&&manager.IsConnectedClient;public bool Running=>manager!=null;public RoomReply State {get;private set;}
  NetworkManager manager;UnityTransport transport;ISession session;NetworkRoom room;ContentCatalog catalog;EffectRegistry effects;string fingerprint,token="",playerName="Jogador",address="127.0.0.1";ushort port=7777;int sequence;float nextBroadcast,lastRequest,reconnectUntil;bool reconnecting,cloud,stopping,awaitingHello,cloudLocked;readonly Dictionary<ulong,float> rate=new Dictionary<ulong,float>();
  public ISessionInfo[] Results {get;private set;}=Array.Empty<ISessionInfo>();
  public static string ContentHash(){using(var sha=SHA256.Create()){string sources=string.Join("\n",Directory.GetFiles(Path.Combine(Application.streamingAssetsPath,"Expansions"),"*.json").OrderBy(x=>x,StringComparer.Ordinal).Select(File.ReadAllText));return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes("tcg-network-4/20260924-shared-terrain\n"+sources))).Replace("-","");}}
  public void Initialize(ContentCatalog c,EffectRegistry e){catalog=c;effects=e;fingerprint=ContentHash();}
  void CreateManager()
  {
   var go=new GameObject("TCG · conexão");transport=go.AddComponent<UnityTransport>();transport.MaxPayloadSize=262144;manager=go.AddComponent<NetworkManager>();manager.NetworkConfig=new NetworkConfig{NetworkTransport=transport,EnableSceneManagement=false,ForceSamePrefabs=false,ConnectionApproval=false};
   manager.OnServerStarted+=Register;manager.OnClientStarted+=Register;manager.OnClientConnectedCallback+=ConnectedPeer;manager.OnClientDisconnectCallback+=DisconnectedPeer;
  }
  void Register(){manager.CustomMessagingManager.RegisterNamedMessageHandler("tcg-request",ReceiveRequest);manager.CustomMessagingManager.RegisterNamedMessageHandler("tcg-state",ReceiveState);}
  public async Task Host(bool internet,bool isPublic,string name,int count,bool teams,string nick,ushort listenPort=7777)
  {
   if(Running)throw new InvalidOperationException("Saia da sala atual primeiro.");Busy=true;stopping=false;cloud=internet;playerName=nick;port=listenPort;token="";sequence=0;
   try{if(internet)await Services();room=new NetworkRoom(catalog,effects,fingerprint,count,teams,()=>Time.realtimeSinceStartupAsDouble);CreateManager();if(internet){session=await MultiplayerService.Instance.CreateSessionAsync(new SessionOptions{Name=name,MaxPlayers=count,IsLocked=true,IsPrivate=!isPublic,SessionProperties=new Dictionary<string,SessionProperty>{{"tcg",new SessionProperty(fingerprint,VisibilityPropertyOptions.Public,PropertyIndex.String1)},{"mode",new SessionProperty(teams?"teams":"ffa")}}});await StartRelayHost(count);}else{transport.SetConnectionData("127.0.0.1",port,"0.0.0.0");if(!manager.StartHost())throw new InvalidOperationException("Não foi possível abrir a porta da sala.");}Status="Sala criada · "+Code;}catch{await Stop();throw;}finally{Busy=false;}
  }
  public async Task Join(bool internet,string destination,string nick,bool byId=false,ushort serverPort=7777)
  {
   if(Running)throw new InvalidOperationException("Saia da sala atual primeiro.");Busy=true;stopping=false;cloud=internet;playerName=nick;address=destination;port=serverPort;token="";sequence=0;
   try{if(internet)await Services();CreateManager();if(internet){session=byId?await MultiplayerService.Instance.JoinSessionByIdAsync(destination):await MultiplayerService.Instance.JoinSessionByCodeAsync(destination.Trim().ToUpperInvariant());await StartRelayClient();}else{transport.SetConnectionData(address,port);if(!manager.StartClient())throw new InvalidOperationException("Conexão não iniciada.");}Status="Conectando ao host…";}catch{await Stop();throw;}finally{Busy=false;}
  }
  async Task StartRelayHost(int count)
  {
   Status="Reservando conexão Relay…";var allocation=await RelayService.Instance.CreateAllocationAsync(count-1);var code=await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
   transport.SetRelayServerData(allocation.ToRelayServerData("dtls"));if(!manager.StartHost())throw new InvalidOperationException("Relay não iniciou o host.");
   var host=session.AsHost();host.SetProperty("tcg-relay",new SessionProperty(code,VisibilityPropertyOptions.Member));host.IsLocked=false;await host.SavePropertiesAsync();
  }
  async Task StartRelayClient()
  {
   if(!session.Properties.TryGetValue("tcg-relay",out var property))throw new InvalidOperationException("A sala ainda não publicou a conexão Relay. Tente novamente.");
   Status="Conectando pelo Relay…";var allocation=await RelayService.Instance.JoinAllocationAsync(property.Value);transport.SetRelayServerData(allocation.ToRelayServerData("dtls"));if(!manager.StartClient())throw new InvalidOperationException("Relay não iniciou o cliente.");
  }
  async Task Services(){if(string.IsNullOrEmpty(Application.cloudProjectId))throw new InvalidOperationException("Vincule este projeto em Unity > Project Settings > Services para habilitar salas pela internet.");Status="Inicializando serviços Unity…";if(UnityServices.State!=ServicesInitializationState.Initialized){var options=new InitializationOptions();var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-tcg-net-label");if(args.Contains("-tcg-net-cloud")&&at>=0&&at+1<args.Length)options.SetProfile("tcg-verify-"+args[at+1]);await UnityServices.InitializeAsync(options);}Status="Autenticando jogador na Unity…";if(!AuthenticationService.Instance.IsSignedIn)await AuthenticationService.Instance.SignInAnonymouslyAsync();}
  public async Task Search(){Busy=true;try{await Services();var result=await MultiplayerService.Instance.QuerySessionsAsync(new QuerySessionsOptions{Count=50,FilterOptions=new List<FilterOption>{new FilterOption(FilterField.StringIndex1,fingerprint,FilterOperation.Equal)}});Results=result.Sessions.Where(s=>!s.IsLocked&&s.AvailableSlots>0&&s.Properties!=null&&s.Properties.TryGetValue("tcg",out var value)&&value.Value==fingerprint).ToArray();Status=Results.Length+" sala(s) pública(s) compatível(is).";}finally{Busy=false;}}
  public async Task QuickJoin(string nick){await Search();foreach(var s in Results){try{await Join(true,s.Id,nick,true);return;}catch(Exception){if(Running)await Stop();}}throw new InvalidOperationException("Nenhuma sala disponível agora. Atualize a busca ou crie uma sala.");}
  void ConnectedPeer(ulong id){Register();if(id==manager.LocalClientId){reconnecting=false;SendRaw(new RoomRequest{type="hello",name=playerName,content=fingerprint,token=token});}else if(Hosting)nextBroadcast=0;}
  void DisconnectedPeer(ulong id){if(stopping)return;if(Hosting){room?.Disconnect(id);Broadcast();}else if(id==manager.LocalClientId){Status="Conexão perdida. Você tem 2 minutos para reconectar.";reconnectUntil=Time.unscaledTime+120;reconnecting=true;}}
  public bool Send(RoomRequest request){if(!Connected||Busy)return false;LastError="";request.sequence=++sequence;Busy=true;lastRequest=Time.unscaledTime;SendRaw(request);return true;}
  void SendRaw(RoomRequest request){if(request.type=="hello")awaitingHello=true;if(Hosting){Apply(room.Receive(manager.LocalClientId,request));Broadcast();}else SendPacket("tcg-request",NetworkManager.ServerClientId,JsonUtility.ToJson(request));}
  void ReceiveRequest(ulong sender,FastBufferReader reader)
  {
   if(!Hosting)return;try{if(rate.TryGetValue(sender,out float time)&&Time.unscaledTime-time<.04f)return;rate[sender]=Time.unscaledTime;var json=ReadPacket(reader,65536);var r=JsonUtility.FromJson<RoomRequest>(json);var reply=room.Receive(sender,r);SendPacket("tcg-state",sender,JsonUtility.ToJson(reply));Broadcast();}catch(Exception){manager.DisconnectClient(sender);}
  }
  void ReceiveState(ulong sender,FastBufferReader reader){if(sender!=NetworkManager.ServerClientId)return;try{Apply(JsonUtility.FromJson<RoomReply>(ReadPacket(reader,2097152)));}catch(Exception){Status="Resposta de rede inválida.";}}
  void Apply(RoomReply reply){if(reply==null)return;if(!string.IsNullOrEmpty(reply.error)){Status=LastError=reply.error;sequence=reply.ack;Busy=false;return;}State=reply;token=reply.token;sequence=awaitingHello?reply.ack:Math.Max(sequence,reply.ack);awaitingHello=false;Busy=reply.ack<sequence;Status=reply.closed?"O host encerrou a sala.":reply.paused?"Aguardando reconexão (até 2 minutos).":reply.started?"Partida conectada":"Sala · escolha seu deck e marque pronto";Changed?.Invoke(reply);}
  void Broadcast(){if(room==null||manager==null||!manager.IsListening)return;foreach(ulong id in manager.ConnectedClientsIds.ToArray()){int seat=room.SeatFor(id);if(seat<0)continue;var reply=room.Reply(seat);if(id==manager.LocalClientId)Apply(reply);else SendPacket("tcg-state",id,JsonUtility.ToJson(reply));}room.ClearVisuals();}
  static byte[] Compress(string text){using(var output=new MemoryStream()){using(var gzip=new GZipStream(output,System.IO.Compression.CompressionLevel.Fastest,true)){var bytes=Encoding.UTF8.GetBytes(text);gzip.Write(bytes,0,bytes.Length);}return output.ToArray();}}
  void SendPacket(string name,ulong id,string text){var bytes=Compress(text);if(bytes.Length>250000)throw new InvalidOperationException("Estado de rede grande demais.");using(var writer=new FastBufferWriter(bytes.Length+8,Allocator.Temp)){writer.WriteValueSafe(bytes.Length);writer.WriteBytesSafe(bytes);manager.CustomMessagingManager.SendNamedMessage(name,id,writer,NetworkDelivery.ReliableFragmentedSequenced);}}
  static string ReadPacket(FastBufferReader reader,int limit){if(reader.Length>262144)throw new InvalidOperationException("Pacote grande demais.");reader.ReadValueSafe(out int length);if(length<1||length>250000||length>reader.Length-reader.Position)throw new InvalidOperationException("Tamanho de pacote inválido.");var bytes=new byte[length];reader.ReadBytesSafe(ref bytes,length);using(var input=new MemoryStream(bytes))using(var gzip=new GZipStream(input,CompressionMode.Decompress))using(var output=new MemoryStream()){var b=new byte[8192];int n;while((n=gzip.Read(b,0,b.Length))>0){if(output.Length+n>limit)throw new InvalidOperationException("Pacote inválido.");output.Write(b,0,n);}return Encoding.UTF8.GetString(output.ToArray());}}
  public async Task Reconnect(){if(!reconnecting||Time.unscaledTime>=reconnectUntil)throw new InvalidOperationException("Prazo de reconexão expirado.");Busy=true;try{if(cloud&&session!=null){manager.Shutdown();await Task.Delay(500);await session.ReconnectAsync();await session.RefreshAsync();await StartRelayClient();}else{manager.Shutdown();await Task.Delay(500);transport.SetConnectionData(address,port);manager.StartClient();}}finally{Busy=false;}}
  void Update(){if(Hosting&&Time.unscaledTime>=nextBroadcast){nextBroadcast=Time.unscaledTime+.75f;room.Tick();if(cloud&&session!=null&&room.Game!=null&&!cloudLocked){cloudLocked=true;LockCloudRoom();}Broadcast();}if(Busy&&Connected&&Time.unscaledTime-lastRequest>15){Busy=false;Status="O host não confirmou a ação. Reconecte para atualizar.";}}
  async void LockCloudRoom(){try{var host=session.AsHost();host.IsLocked=true;await host.SavePropertiesAsync();}catch(Exception){Status="Partida iniciada; atualização do diretório indisponível.";}}
  public async Task Stop(){stopping=true;if(session!=null){try{if(session.IsHost)await session.AsHost().DeleteAsync();else await session.LeaveAsync();}catch(Exception){}session=null;}if(manager!=null){manager.Shutdown();Destroy(manager.gameObject);}manager=null;room=null;State=null;token="";sequence=0;Busy=false;reconnecting=false;cloudLocked=false;}
  public void SimulateConnectionLoss(){if(!Hosting&&manager!=null){manager.Shutdown();reconnecting=true;reconnectUntil=Time.unscaledTime+120;}}
  void OnDestroy(){if(manager!=null){stopping=true;manager.Shutdown();Destroy(manager.gameObject);}}
 }
}
