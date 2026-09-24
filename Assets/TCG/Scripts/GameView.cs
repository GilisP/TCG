using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TCG
{
    public sealed partial class GameView : MonoBehaviour
    {
        Game game;
        int mode = 1, selection = -1, source = -1, counterId = -1;
        string notice = "Comece pela fase Início. Clique em Comprar 2 cartas.";
        bool error, help, confirmReset, handoff, deckEditor;
        CardDefinition zoomCard;
        Vector2 scroll, stackScroll;
        GUIStyle heading, body, small, tiny, center, button;
        readonly BoardArt art = new BoardArt();
        readonly Queue<VisualEvent> animations = new Queue<VisualEvent>();
        VisualEvent animation;
        float animationStart;
        bool Busy => animation != null || animations.Count > 0;
        readonly Color bg = new Color(.047f,.071f,.09f), panel = new Color(.083f,.118f,.145f),
            ink = new Color(.92f,.95f,.94f), muted = new Color(.64f,.73f,.76f),
            green = new Color(.42f,.84f,.62f), blue = new Color(.38f,.72f,.97f),
            red = new Color(1f,.47f,.40f), gold = new Color(.96f,.75f,.39f);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        { if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Mesa" && FindFirstObjectByType<GameView>() == null) new GameObject("TCG Game").AddComponent<GameView>(); }
        void Awake()
        {
            CustomCardStore.LoadResources(); LoadDecks(); NewGame(); mainMenu = true; hasStarted = false; Application.runInBackground = true;
        }
        void OnEnable()
        {
            // Unity can reload scripts while Play is active; rebuild non-serialized state.
            if (game == null) { CustomCardStore.LoadResources(); LoadDecks(); NewGame(); }
        }
        void NewGame()
        {
            if (game != null) game.Visual -= QueueVisual;
            game = new Game(42,decks[0],decks[1],selectedMode); mainMenu = false; playSetup = false; hasStarted = true; game.Visual += QueueVisual;
            gearSelection = passengerSelection = -1; animations.Clear(); animation = null; selection = source = counterId = -1; mode = 1;
            confirmReset = handoff = help = deckEditor = false; error = false; zoomCard = null;
            notice = "Partida iniciada. Clique em Comprar 2 cartas para começar.";
        }
        void QueueVisual(VisualEvent item) { animations.Enqueue(item); }
        void Update()
        {
            if (animation != null && Time.unscaledTime-animationStart >= .45f) animation = null;
            if (animation == null && animations.Count > 0) { animation = animations.Dequeue(); animationStart = Time.unscaledTime; }
        }
        void OnDestroy() { art.Dispose(); if (game != null) game.Visual -= QueueVisual; }
        Color OwnerColor(int owner) => owner == 0 ? gold : blue;
        string OwnerName(int owner) => owner == 0 ? "SOL" : "LUA";
        bool Execute(Action action)
        {
            int previous = game.Priority;
            try
            {
                action(); error = false;
                if (previous != game.Priority && game.Winner < 0 && game.Mode != MatchMode.Practice) { handoff = true; gearSelection = passengerSelection = -1; selection = source = counterId = -1; mode = 1; }
                return true;
            }
            catch (InvalidOperationException exception) { notice = exception.Message; error = true; return false; }
        }
        void Styles()
        {
            if (heading != null) return;
            heading = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, wordWrap = true };
            body = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
            small = new GUIStyle(body) { fontSize = 13 };
            tiny = new GUIStyle(body) { fontSize = 10,alignment = TextAnchor.MiddleCenter };
            center = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold };
            button = new GUIStyle(body) { alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold };
        }
        void Fill(Rect r,Color color) { var old = GUI.color; GUI.color = color; GUI.DrawTexture(r,Texture2D.whiteTexture); GUI.color = old; }
        void Text(float x,float y,float w,float h,string text,GUIStyle style,Color color)
        { var old = GUI.contentColor; GUI.contentColor = color; GUI.Label(new Rect(x,y,w,h),text,style); GUI.contentColor = old; }
        void Icon(Rect rect,CardDefinition card,Color color)
        { var old = GUI.color; GUI.color = color; GUI.DrawTexture(rect,art.Icon(card)); GUI.color = old; }
        bool Button(float x,float y,float w,float h,string text,Color color,bool enabled = true)
        {
            var r = new Rect(x,y,w,h); bool previous = GUI.enabled; GUI.enabled = previous && enabled;
            Fill(r,GUI.enabled ? color : new Color(.17f,.21f,.24f));
            if (GUI.enabled && r.Contains(Event.current.mousePosition)) Fill(r,new Color(1,1,1,.08f));
            Text(x,y,w,h,text,button,GUI.enabled ? ink : muted);
            bool clicked = GUI.Button(r,GUIContent.none,GUIStyle.none); GUI.enabled = previous; return clicked;
        }
        void Choose(int next)
        {
            mode = next; gearSelection = passengerSelection = -1; selection = source = counterId = -1; scroll = Vector2.zero; error = false;
            notice = next == 0 ? "Escolha um terreno e clique em COLOCAR." : next == 1 ? "Selecione uma carta. Leia o efeito e escolha o alvo destacado." :
                next == 2 ? "Clique numa criatura ou veículo seu para agir." : next == 4 ? "Clique num terreno para consultar construções e habilidades." : "Selecione o comandante para invocar ou use um comando se ele estiver no campo.";
        }
        CardDefinition Selected => mode == 3 ? game.Acting.Commander.Definition : mode == 1 && selection >= 0 && selection < game.Acting.Hand.Count ? game.Acting.Hand[selection].Definition : null;
        void OnGUI()
        {
            Styles(); var matrix = GUI.matrix;
            float scale = Mathf.Min(Screen.width/1440f,Screen.height/960f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width-1440*scale)/2,(Screen.height-960*scale)/2,0),Quaternion.identity,new Vector3(scale,scale,1));
            Fill(new Rect(0,0,1440,960),bg);
            if (deckEditor) { DrawDeckEditor(); GUI.matrix = matrix; return; }
            if (mainMenu) { DrawMainMenu(); GUI.matrix = matrix; return; }
            GUI.enabled = !help && !confirmReset && !handoff && zoomCard == null;
            Text(28,18,610,36,"FRONTEIRAS  /  CRÔNICAS DOS REINOS",heading,gold);
            Text(28,58,990,28,"Conquiste terrenos, invoque seu exército e ataque a capital. Truques podem responder às ações na pilha.",small,muted);
            if (Button(803,24,145,38,"Menu",panel)) { mainMenu = true; playSetup = false; }
            if (Button(958,24,136,38,"Arsenal",panel)) { deckEditor = true; selection = -1; }
            if (Button(1104,24,145,38,"Como jogar",panel)) help = true;
            if (Button(1259,24,153,38,"Nova partida",panel)) confirmReset = true;
            DrawPlayer(28,0); DrawPlayer(738,1);
            GUI.enabled = GUI.enabled && game.Winner < 0 && !Busy;
            DrawBoard(); if (!handoff) DrawControls();
            GUI.enabled = true;
            DrawAnimation();
            Fill(new Rect(28,884,1384,50),panel); Text(44,888,1350,44,Busy ? "Resolvendo ação..." : notice,body,error ? red : ink);
            Text(28,937,1380,20,"Dois jogadores locais • Regras experimentais 0.6 • Decks podem ser salvos; partidas ainda não",small,muted);
            if (help || confirmReset || handoff || game.Winner >= 0 || zoomCard != null) DrawModal();
            GUI.matrix = matrix;
        }
        void DrawPlayer(float x,int owner)
        {
            Fill(new Rect(x,98,674,80),panel); Fill(new Rect(x,98,5,80),OwnerColor(owner));
            Text(x+20,110,400,26,$"{OwnerName(owner)} · Jogador {owner+1}",body,OwnerColor(owner));
            Text(x+20,144,430,24,$"{(game.Active == owner ? "TURNO ATIVO" : "Fora do turno")}  ·  {(game.Priority == owner ? "TEM PRIORIDADE" : "aguardando")}",small,muted);
            Text(x+498,111,160,46,$"{game.Players[owner].Life} vida",heading,ink);
        }
        int TargetKind(int index)
        {
            if (game.Winner >= 0) return 0;
            if (mode == 0 && selection >= 0 && game.CanPlace(index)) return 1;
            if ((mode == 1 || mode == 3) && Selected != null && game.CanTarget(Selected,game.Priority,index)) return Selected.IsUnit ? 2 : 5;
            if (mode == 2 && source >= 0)
            { if (gearSelection >= 0) return game.CanChangeEquipment(source,gearSelection,index) ? 5 : 0; if (passengerSelection >= 0) return game.CanDisembark(source,passengerSelection,index) ? 3 : 0; if (game.CanEmbark(source,index)) return 6; if (game.CanAttack(source,index)) return 4; if (game.CanMove(source,index)) return 3; }
            return 0;
        }
        Rect TileRect(int index) => new Rect(46+index%11*55,267+index/11*55,52,52);
        void DrawBoard()
        {
            Text(28,194,660,30,"CAMPO DE BATALHA",body,ink);
            Text(28,226,670,25,"Dourado: Sol  ·  Azul: Lua  ·  Verde: alvo válido  ·  Vermelho: ataque",small,muted);
            for (int n = 0; n < 11; n++)
            { Text(46+n*55,245,55,22,((char)('A'+n)).ToString(),center,muted); Text(23,267+n*55,22,55,(n+1).ToString(),center,muted); }
            for (int i = 0; i < 121; i++)
            {
                var tile = game.Board[i]; int target = TargetKind(i); var r = TileRect(i);
                Color edge = source == i ? ink : target == 4 ? red : target == 3 ? blue : target > 0 ? green : new Color(.18f,.24f,.28f);
                Fill(r,edge); Fill(new Rect(r.x+2,r.y+2,48,48),tile.Terrain == null ? panel : Color.Lerp(panel,BoardArt.Palette[tile.Terrain.Element],.18f));
                if (tile.Owner >= 0) Fill(new Rect(r.x+2,r.y+2,48,3),OwnerColor(tile.Owner));
                if (tile.Terrain != null && tile.Terrain.Definition.ProductionColors.Length > 1) { var colors=tile.Terrain.Definition.ProductionColors; for(int c=0;c<colors.Length;c++) Fill(new Rect(r.x+2+c*48f/colors.Length,r.y+6,48f/colors.Length,3),BoardArt.Palette[colors[c]]); }
                if (tile.Terrain != null) Icon(new Rect(r.x+5,r.y+6,42,40),tile.Terrain.Definition,new Color(.6f,.75f,.7f,.24f));
                if (tile.Capital)
                {
                    Icon(new Rect(r.x+7,r.y+5,38,32),game.Players[tile.Owner].Commander.Definition,OwnerColor(tile.Owner));
                    Text(r.x+1,r.y+36,50,14,"CAPITAL",tiny,ink);
                }
                bool moving = animation != null && animation.Kind == "move" && animation.To == i;
                bool spawning = animation != null && animation.Kind == "summon" && animation.To == i;
                if (tile.Unit != null && !moving && !spawning)
                {
                    DrawToken(r,tile.Unit.Card.Definition,tile.Unit.Owner,tile.Unit.Life);
                    if (tile.Unit.Shield+tile.Unit.TemporaryShield > 0) { Fill(new Rect(r.x+2,r.y+5,20,15),blue); Text(r.x+2,r.y+5,20,15,"E"+(tile.Unit.Shield+tile.Unit.TemporaryShield),tiny,bg); }
                    if (tile.Unit.StunnedThroughTurn >= game.Turn) { Fill(new Rect(r.x+26,r.y+5,24,15),new Color(.7f,.9f,1)); Text(r.x+26,r.y+5,24,15,"GELO",tiny,bg); }
                }
                if (tile.Buildings.Count > 0) { Fill(new Rect(r.x+2,r.y+20,22,14),bronze); Text(r.x+2,r.y+20,22,14,"C"+tile.Buildings.Count,tiny,ink); }
                if (tile.Unit != null && tile.Unit.Equipment.Count > 0) Text(r.x+28,r.y+20,22,14,"+"+tile.Unit.Equipment.Count,tiny,gold);
                if (target > 0)
                {
                    Fill(new Rect(r.x+2,r.y+35,48,15),edge);
                    Text(r.x+2,r.y+35,48,15,target == 1 ? "COLOCAR" : target == 2 ? "INVOCAR" : target == 3 ? "MOVER" : target == 4 ? "ATACAR" : target == 6 ? "EMBARCAR" : "ALVO",tiny,bg);
                }
                if (GUI.Button(r,GUIContent.none,GUIStyle.none)) ClickTile(i);
            }
        }
        void DrawToken(Rect r,CardDefinition card,int owner,int life)
        {
            Fill(new Rect(r.x+9,r.y+7,34,34),Color.Lerp(bg,OwnerColor(owner),.19f));
            Icon(new Rect(r.x+10,r.y+6,33,34),card,OwnerColor(owner));
            Text(r.x+2,r.y+37,48,13,$"{(owner == 0 ? "S" : "L")}  {life}/{card.Life}",tiny,ink);
        }
        void DrawAnimation()
        {
            if (animation == null) return;
            float t = Mathf.Clamp01((Time.unscaledTime-animationStart)/.45f); var a = TileRect(animation.From); var b = TileRect(animation.To);
            if (animation.Kind == "move")
            {
                var r = new Rect(Vector2.Lerp(a.position,b.position,Mathf.SmoothStep(0,1,t)),a.size);
                DrawToken(r,animation.Card,animation.Owner,game.Board[animation.To].Unit?.Life ?? animation.Card.Life);
            }
            else if (animation.Kind == "attack")
            {
                var matrix = GUI.matrix; Vector2 delta = b.center-a.center;
                GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg,a.center);
                Fill(new Rect(a.center.x,a.center.y-2,delta.magnitude*t,4),gold); GUI.matrix = matrix;
                Fill(b,new Color(1,.25f,.16f,(1-t)*.35f));
            }
            else if (animation.Kind == "damage" || animation.Kind == "heal")
                Text(b.x-9,b.y-20-t*18,70,35,(animation.Kind == "heal" ? "+" : "−")+animation.Value,heading,animation.Kind == "heal" ? green : red);
            else if (animation.Kind == "summon")
            {
                float s = Mathf.Lerp(.2f,1,t); var r = new Rect(b.center-new Vector2(26,26)*s,new Vector2(52,52)*s);
                Icon(r,animation.Card,OwnerColor(animation.Owner));
            }
            else if (animation.Kind == "leave") Icon(new Rect(b.x-t*10,b.y-t*10,b.width+t*20,b.height+t*20),animation.Card,new Color(.6f,.6f,.95f,1-t));
            else if (animation.Kind == "shield" || animation.Kind == "stun") Fill(b,new Color(.4f,.75f,1,(1-t)*.6f));
            else Fill(b,new Color(.5f,1,.6f,(1-t)*.5f));
        }
        void DrawControls()
        {
            Fill(new Rect(710,194,702,678),panel);
            Text(730,204,662,34,$"Turno {game.Turn} / {game.PhaseName} / Prioridade: {OwnerName(game.Priority)}",heading,ink);
            string[] phases = { "Início", "Compra", "Terreno", "Principal", "Final" };
            for (int i = 0; i < 5; i++)
            { Fill(new Rect(730+i*133,246,125,25),(int)game.Phase == i ? new Color(.23f,.43f,.35f) : bg); Text(730+i*133,246,125,25,phases[i],center,ink); }
            string[] tabs = { "Terrenos", "Mão", "Criaturas", "Comandante", "Domínios" };
            for (int i = 0; i < 5; i++) if (Button(730+i*133,283,125,35,tabs[i],mode == i ? new Color(.22f,.38f,.40f) : bg)) Choose(i);
            Text(730,328,662,27,$"Mana: {game.Acting.Mana.Sum()}  |  "+string.Join("  ",game.Acting.Mana.Select((v,i) => ElementsShort(i)+":"+v)),small,gold);
            if (mode == 4) DrawDomainPanel(); else if (mode == 2) DrawUnitPanel(); else if (mode == 3) DrawCommander(); else DrawCards();
            DrawStack();
            bool canAdvance = game.Stack.Count == 0 && game.Priority == game.Active && !game.MustPlace;
            string[] next = { "Comprar 2 cartas", "Ir para Terrenos", "Ir para Principal", "Ir para Final", "Passar no Final" };
            if (Button(730,812,324,40,next[(int)game.Phase],new Color(.20f,.38f,.30f),canAdvance))
            {
                if (Execute(game.AdvancePhase)) { Choose(game.Phase == Phase.Terrain ? 0 : 1); notice = "Fase: "+game.PhaseName+". "+(game.Phase == Phase.Terrain ? "Coloque um terreno." : "Use a mão, criaturas ou comandante quando estiver na fase Principal."); }
            }
            if (Button(1066,812,326,40,$"Passar prioridade ({game.Passes}/2)",new Color(.19f,.29f,.43f),game.Phase == Phase.Main || game.Phase == Phase.End || game.Stack.Count > 0))
                if (Execute(game.PassPriority)) { selection = counterId = -1; notice = game.Log.Last(); }
        }
        string ElementsShort(int i) => new[] { "Sol", "Lua", "Água", "Fogo", "Ar", "Terra", "Inc" }[i];
        void DrawCards()
        {
            bool terrain = mode == 0; var cards = terrain ? game.Acting.Offer : game.Acting.Hand;
            Text(730,360,662,28,terrain ? "Escolha 1 terreno na fase Terreno." : "Selecione uma carta. Truques também podem responder ao adversário.",small,ink);
            scroll = GUI.BeginScrollView(new Rect(730,391,662,163),scroll,new Rect(0,0,638,Mathf.Ceil(cards.Count/2f)*82));
            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i].Definition; var r = new Rect(i%2*320,i/2*82,310,74);
                SmallCard(r,card,selection == i);
                if (GUI.Button(r,GUIContent.none,GUIStyle.none)) { selection = i; source = counterId = -1; error = false; notice = terrain ? "Clique em COLOCAR." : card.Text; }
            }
            GUI.EndScrollView();
            var selected = Selected;
            if (!terrain && selected != null)
            {
                string reason = game.CardBlockReason(selected,game.Priority);
                Text(730,561,662,47,reason == "" ? selected.CostLabel+" · "+selected.Text : reason,small,reason == "" ? ink : gold);
                if (Button(730,609,156,28,"Ampliar carta",panel)) zoomCard = selected;
                if (!selected.NeedsBoardTarget)
                {
                    if (Button(896,609,496,28,selected.NeedsStackTarget ? "Anular ação selecionada na pilha" : "Lançar carta",new Color(.20f,.32f,.28f),reason == "" && (!selected.NeedsStackTarget || counterId >= 0))) Cast(-1);
                }
                else Text(896,611,496,27,"Escolha uma casa ou criatura marcada ALVO / INVOCAR.",small,muted);
            }
            else Text(730,566,662,60,terrain ? "O terreno gera mana no próximo início de turno. Capitais não podem ser substituídas." : "Custos são pagos ao lançar. A carta só resolve depois de dois passes de prioridade.",small,muted);
        }
        void DrawUnitPanel()
        {
            DrawUnitActions(); return;
        }
        void DrawCommander()
        {
            var p = game.Acting; var card = p.Commander.Definition;
            Icon(new Rect(730,368,65,65),card,OwnerColor(game.Priority));
            Text(806,365,580,37,card.Name,heading,ink);
            Text(806,403,580,49,$"Custo atual: {game.CommanderCost(game.Priority)} mana · Popularidade: {p.Popularity}\n{(p.CommanderAvailable ? "Na zona de comando: clique em INVOCAR no campo." : p.Exile.Contains(p.Commander) ? "Exilado: indisponível nesta partida." : "No campo ou aguardando na pilha.")}",small,gold);
            Text(730,463,662,70,card.Text,small,ink);
            var unit = game.Board.Select(t => t.Unit).FirstOrDefault(u => u != null && u.Commander && u.Owner == game.Priority);
            bool available = game.OpenMain && unit != null && unit.Actions > 0 && !p.CommandUsed;
            if (Button(730,548,662,36,"Reunir: +1 popularidade e cura 2 do jogador",new Color(.19f,.31f,.26f),available)) Execute(() => game.ActivateCommand(game.Priority,0));
            if (Button(730,593,662,36,"Inspirar: −2 popularidade / +1 ataque aos aliados",new Color(.19f,.27f,.36f),available && p.Popularity >= 2)) Execute(() => game.ActivateCommand(game.Priority,1));
        }
        void DrawStack()
        {
            Text(730,643,662,28,$"PILHA ({game.Stack.Count}) · resolve do topo para baixo · dois passes resolvem 1 ação",small,blue);
            stackScroll = GUI.BeginScrollView(new Rect(730,674,662,128),stackScroll,new Rect(0,0,638,Math.Max(65,game.Stack.Count*48)));
            if (game.Stack.Count == 0) Text(8,8,610,80,"Pilha vazia. Dois passes na Principal vão ao Final; dois passes no Final encerram o turno.",small,muted);
            for (int n = 0; n < game.Stack.Count; n++)
            {
                var item = game.Stack[game.Stack.Count-1-n]; var r = new Rect(0,n*48,638,44);
                Fill(r,counterId == item.Id ? new Color(.36f,.23f,.24f) : bg);
                Text(10,r.y+4,616,38,$"{(n == 0 ? "TOPO" : "#"+item.Id)} · {OwnerName(item.Owner)} · {item.Description}",small,ink);
                if (GUI.Button(r,GUIContent.none,GUIStyle.none)) { counterId = item.Id; notice = "Ação selecionada: "+item.Description+". Use Negação para anular."; }
            }
            GUI.EndScrollView();
        }
        void Cast(int target)
        {
            if (Execute(() => game.PlayCard(game.Priority,selection,target,counterId)))
            { selection = source = counterId = -1; notice = "Carta na pilha. Passe prioridade ou responda com um truque."; }
        }
        void ClickTile(int index)
        {
            if (mode == 4) { source = index; return; }
            var unit = game.Board[index].Unit;
            if (mode == 2 && source >= 0)
            {
                if (gearSelection >= 0) { if (Execute(() => game.ChangeEquipment(source,gearSelection,index))) { gearSelection=-1; source=-1; } return; }
                if (passengerSelection >= 0) { if (Execute(() => game.Disembark(source,passengerSelection,index))) passengerSelection=-1; return; }
                if (game.CanEmbark(source,index)) { if (Execute(() => game.Embark(source,index))) source=index; return; }
            }
            if ((mode == 2 || (mode == 1 && selection < 0)) && unit != null && unit.Owner == game.Priority)
            { mode = 2; selection = -1; source = index; notice = unit.Card.Definition.Text; return; }
            if (mode == 0)
            { if (Execute(() => game.Place(selection,index))) { selection = -1; notice = "Terreno colocado. Clique em Ir para Principal."; } }
            else if (mode == 1) Cast(index);
            else if (mode == 3)
            { if (Execute(() => game.DeployCommander(game.Priority,index))) notice = "Comandante na pilha. Passe prioridade para permitir respostas."; }
            else if (source >= 0)
            {
                if (game.CanAttack(source,index)) { if (Execute(() => game.Attack(source,index))) notice = "Ataque na pilha. O adversário pode responder."; }
                else if (Execute(() => game.Move(source,index))) { source = index; notice = "Movimento concluído. Você pode continuar ou selecionar outra criatura."; }
            }
            else { notice = "Selecione uma carta ou uma criatura antes de escolher a casa."; error = true; }
        }
        void DrawModal()
        {
            Fill(new Rect(0,0,1440,960),bg); Fill(new Rect(300,200,840,550),panel);
            if (zoomCard != null)
            {
                CardFace(new Rect(505,80,430,680),zoomCard,true,true);
                if (Button(505,783,430,48,"Voltar à partida",panel)) zoomCard = null;
            }
            else if (confirmReset)
            {
                Text(335,255,770,70,"Iniciar outra partida com os decks equipados?",heading,ink);
                Text(335,354,770,110,"A partida atual será descartada. No Arsenal, salve e equipe as listas desejadas.\n\nSol: "+decks[0].name+"\nLua: "+decks[1].name,body,muted);
                if (Button(335,645,370,52,"Continuar esta partida",bg)) confirmReset = false;
                if (Button(720,645,385,52,"Iniciar nova partida",new Color(.38f,.22f,.18f))) NewGame();
            }
            else if (help)
            {
                Text(335,234,770,50,"Como jogar esta versão",heading,ink);
                Text(335,301,770,300,"1. Avance Início → Compra → Terreno e coloque um terreno.\n2. Na Principal, use Mão, Criaturas ou Comandante.\n3. Ao lançar uma carta ou atacar, a ação entra na PILHA.\n4. Quem tem prioridade pode usar um truque ou PASSAR.\n5. Dois passes seguidos resolvem apenas a ação do topo.\n6. Com a pilha vazia, avance ao Final; ambos passam para encerrar.\n\nDica: o comandante custa 3 mana inicialmente. Terrenos renovam mana no início. Movimento não usa pilha. Arqueiros têm alcance 2.\n\nArsenal: crie e salve listas, depois equipe uma para cada jogador. Nova partida usa as versões equipadas.",body,ink);
                if (Button(335,661,770,52,"Entendi",new Color(.19f,.38f,.30f))) help = false;
            }
            else if (game.Winner >= 0)
            {
                Text(335,285,770,60,$"Vitória do {OwnerName(game.Winner)}!",heading,OwnerColor(game.Winner));
                Text(335,381,770,80,game.Players[1-game.Winner].Life <= 0 ? "O adversário ficou sem vida." : "O adversário tentou comprar de um deck vazio.",body,ink);
                if (Button(335,645,770,52,"Jogar novamente",new Color(.19f,.38f,.30f))) NewGame();
            }
            else
            {
                Text(335,272,770,75,$"Prioridade do {OwnerName(game.Priority)}",heading,OwnerColor(game.Priority));
                Text(335,365,770,150,$"Passe o controle ao jogador {game.Priority+1}.\n\n{(game.Priority == game.Active ? "Você controla o turno atual." : "Você pode responder com um truque ou passar.")}\nA mão do próximo jogador aparecerá ao continuar.",body,ink);
                if (Button(335,645,770,52,"Estou pronto — ver minha mão",new Color(.19f,.38f,.30f))) { handoff = false; mode = game.Phase == Phase.Terrain ? 0 : 1; notice = "Você tem prioridade. Escolha uma ação válida ou passe."; }
            }
        }
    }
}
