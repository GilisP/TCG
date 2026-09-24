using System;

using System.Linq;

using UnityEngine;

using UnityEngine.SceneManagement;

using TCG.Foundation;



namespace TCG.Table

{

    public sealed partial class TableView : MonoBehaviour

    {

        ContentCatalog catalog; EffectRegistry effects; Match match; TableWorld world;

        bool medieval=true; int firstColor; Definition inspected; Vector2 choiceScroll, detailScroll;

        bool menu=true, handoff, collection, help, confirmQuit, teams, attackGroup;

        int players=4, selectedCell=-1, selectedUnit=-1, selectedPile=0, catalogPage;

        string selectedCard, notice="Escolha uma mesa para começar.", fatal, filter="", expansion="";

        Vector2 handScroll, unitScroll, expansionScroll; int[] blockers; Pending lastDefense;

        GUIStyle title, menuTitle, heading, body, small, cardName, label; float scale;

        static readonly Color Ink=TableWorld.Hex("FFF8E4"), Muted=TableWorld.Hex("E1D2B8"), Panel=TableWorld.Hex("2A2420"), Gold=TableWorld.Hex("E1BC78"), Dark=TableWorld.Hex("171513");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]

        static void Boot() { if(SceneManager.GetActiveScene().name=="Mesa"&&FindFirstObjectByType<TableView>()==null) new GameObject("Fronteiras • mesa").AddComponent<TableView>(); }

        void Awake()

        {

            Application.runInBackground=true; Application.targetFrameRate=60;

            try { effects=new EffectRegistry(); catalog=ContentLoader.Load(effects); LoadLibrary(); world=new GameObject("Sala dos reinos").AddComponent<TableWorld>(); world.Setup();world.FoilAppearance=(owner,card)=>CardFoil(card,owner); world.SetRoom(PlayerPrefs.GetInt("TCG.Room",0));AudioListener.volume=PlayerPrefs.GetFloat("TCG.Volume",1);if(PlayerPrefs.HasKey("TCG.Fullscreen"))Screen.fullScreen=PlayerPrefs.GetInt("TCG.Fullscreen")==1; StartMatch(); menu=true; handoff=false; sessionAvailable=false; StartDiagnosticsIfRequested(); }

            catch(Exception e) { fatal=e.Message; Debug.LogException(e); }

        }

        void StartMatch()

        {

            SaveFinishedProgress();if(network!=null&&network.Running){networkPage=true;return;}
            exileOpen=false;if(useSavedDecks) {if(!StartSavedDeckMatch())return;} else match=commanderDemo?ContentLoader.CommanderTable(catalog,effects,players,teams,Environment.TickCount,demoCommanders):medieval?ContentLoader.MedievalTable(catalog,effects,players,teams,Environment.TickCount,firstColor):ContentLoader.TestTable(catalog,effects,players,teams,Environment.TickCount); match.AutomaticResponses=true; match.AutoAdvanceAfterTerrain=true; world.Bind(match); ApplyDeckCosmetics(); match.ResolveInitialEffects(); sessionAvailable=true; selectedCell=selectedUnit=-1; selectedCard=null;

            reactions=new ReactionChannel(); CloseReactionMenu(); ResetDrag(); handoff=true; menu=false; blockers=null; lastDefense=null; notice="Escolha a compra inicial. A pilha selecionada recebe o terreno comprado.";

        }

        void Styles()

        {

            if(body!=null) return;

            title=new GUIStyle(GUI.skin.label){fontSize=54,fontStyle=FontStyle.Bold,wordWrap=true};

            menuTitle=new GUIStyle(title){fontSize=40};

            heading=new GUIStyle(GUI.skin.label){fontSize=24,fontStyle=FontStyle.Bold,wordWrap=true};

            body=new GUIStyle(GUI.skin.label){fontSize=17,wordWrap=true}; small=new GUIStyle(body){fontSize=13};

            cardName=new GUIStyle(body){fontSize=15,fontStyle=FontStyle.Bold}; label=new GUIStyle(small){alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold};

            var serif=Font.CreateDynamicFontFromOSFont(new[]{"Georgia","Palatino Linotype","Times New Roman"},24);
            foreach(var style in new[]{title,menuTitle,heading,cardName})style.font=serif;
            foreach(var style in new[]{title,menuTitle,heading,body,small,cardName,label}) style.normal.textColor=Color.white;

        }

        static void Fill(Rect rect,Color color) { MedievalFill(rect,color); }

        void Text(float x,float y,float w,float h,string text,GUIStyle style,Color color) { var old=GUI.contentColor; GUI.contentColor=color; GUI.Label(new Rect(x,y,w,h),text,style); GUI.contentColor=old; }

        bool Button(float x,float y,float w,float h,string text,bool enabled=true,bool accent=false)

        {

            var rect=new Rect(x,y,w,h); bool old=GUI.enabled; GUI.enabled=old&&enabled;

            Fill(rect,GUI.enabled?(accent?Gold:TableWorld.Hex("4B3327")):TableWorld.Hex("302A24"));

            Stroke(new Rect(x+1,y+1,w-2,h-2),accent?TableWorld.Hex("F3DBAB"):TableWorld.Hex("8C6C43"));PaperGrain(new Rect(x+3,y+3,w-6,h-6),.08f);
            if(GUI.enabled&&rect.Contains(Event.current.mousePosition)) Fill(rect,new Color(1,1,1,.09f));

            bool active=GUI.enabled; GUI.enabled=true; Text(x+6,y,w-12,h,text,label,active&&accent?Dark:active?Ink:Muted); GUI.enabled=active;

            bool clicked=GUI.Button(rect,GUIContent.none,GUIStyle.none); GUI.enabled=old; return clicked;

        }

        bool Submit(ActionKind kind,int target=-1,int unit=-1,string card=null,int[] units=null,int[] defense=null)

        {

            int previous=match.Controller;

            var command=new Command{kind=kind,player=previous,revision=match.Revision,target=target,unit=unit,card=card,pile=selectedPile,units=units??Array.Empty<int>(),blockers=defense??Array.Empty<int>()};

            if(network!=null&&network.Running)return SubmitNetwork(command);
            if(!match.Try(command,out var error)) { notice=error; return false; }

            SaveFinishedProgress(); selectedCommander=false; selectedCard=null; selectedUnit=-1; notice=match.Log.LastOrDefault()??"Ação concluída.";

            if(match.Controller!=previous&&!match.Over) { handoff=true; selectedCell=-1; }

            return true;

        }

        void OnGUI()

        {

            Styles(); scale=Mathf.Min(Screen.width/1600f,Screen.height/1000f); var old=GUI.matrix;

            GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));

            if(fatal!=null) { Fill(new Rect(0,0,1600,1000),Dark); Text(80,100,1400,100,"Não foi possível carregar a mesa",heading,Gold); Text(80,220,1400,550,fatal,body,Ink); GUI.matrix=old; return; }

            world.View.rect=new Rect(0,(Screen.height-766*scale)/Screen.height,1190*scale/Screen.width,656*scale/Screen.height);

            if(networkPage){DrawNetworkPage();GUI.matrix=old;return;}
            if(libraryOpen){ResetDrag();world.ShowMovement(-1,false);DrawHub();GUI.matrix=old;return;}
            bool movementInput=NetworkTurn&&reactionMenu<0&&!menu&&!handoff&&!collection&&!help&&!confirmQuit&&!match.Over&&inspected==null&&match.Choice==null&&equipmentToAttach<0;
            if(!movementInput)ResetDrag();
            world.ShowMovement(selectedUnit,movementInput&&!selectedCommander&&selectedCard==null&&!draggingPiece&&match.OrderFor(selectedUnit)==null);
            world.ShowMovementOrder(selectedUnit,movementInput&&!selectedCommander&&selectedCard==null);
            if(menu) { DrawMenu(); GUI.matrix=old; return; }

            bool modal=exileOpen||reactionMenu>=0||handoff||collection||help||confirmQuit||match.Over||inspected!=null||match.Choice!=null||equipmentToAttach>=0;

            GUI.enabled=!modal;

            DrawHeader(); GUI.enabled=!modal&&NetworkTurn; DrawSidebar(); DrawMovementOrders(); DrawHand(); DrawBoardLabels(); DrawCommanderZone();DrawExiledCards();

            Fill(new Rect(0,965,1600,35),Dark); Text(24,972,1550,24,notice,small,Gold);

            if(!modal&&NetworkTurn) { BoardInput(); ProcessDragVerification(); ProcessPresentationVerification(); }

            GUI.enabled=true;

            DrawNetworkStatus();DrawSocialReactions();
            if(handoff) DrawHandoff(); else if(inspected!=null) DrawInspection(); else if(equipmentToAttach>=0) DrawEquipmentChoice(); else if(match.Choice!=null&&match.Choice.Owner==Viewer) DrawChoice(); else if(collection) DrawCollection(); else if(help) DrawHelp(); else if(confirmQuit) DrawQuit(); else if(match.Over) DrawResult();

            GUI.matrix=old;

        }

        void DrawMenu() { DrawHub(); }
        void DrawHeader()

        {

            Fill(new Rect(0,0,1600,110),Dark);

            Text(25,14,440,30,"FRONTEIRAS  /  MESA DOS REINOS",cardName,Gold);

            for(int p=0;p<match.Seats.Count;p++)

            {

                var s=match.Seats[p]; float x=25+p*278;

                Fill(new Rect(x,49,264,48),Panel); Fill(new Rect(x,49,4,48),TableWorld.Seats[p]);

                Text(x+14,52,175,23,s.Name+(teams?" · dupla "+(s.Team+1):""),cardName,TableWorld.Seats[p]);

                Text(x+14,76,195,19,s.Eliminated?"ELIMINADO":p==match.Controller?"PRIORIDADE":p==match.Active?"TURNO ATIVO":"AGUARDANDO",small,Muted);

                Text(x+209,56,50,34,s.Life.ToString(),heading,Ink);

            }

            if(Button(1185,25,118,54,"Coleção")) OpenLibrary();

            if(Button(1312,25,118,54,"Como jogar")) help=true;

            if(Button(1439,25,135,54,"Menu")) NavigateHub(HubPage.Settings);

        }

        void DrawSidebar()

        {

            Fill(new Rect(1190,110,410,855),Panel);

            Text(1215,131,355,32,"Turno "+match.Turn+"  /  "+match.Seats[match.Active].Name,heading,Ink);

            string[] phases={"COMPRA","TERRENO","PRINCIPAL","FINAL"};

            Text(1215,175,345,28,phases[(int)match.Phase]+"  ·  "+match.Seats[match.Controller].Name,cardName,Gold);

            if(match.Choice!=null) { Text(1215,240,350,120,match.Choice.Prompt,body,Gold); return; }

            if(match.Defense!=null) { DrawDefense(); return; }

            var seat=match.Seats[match.Controller];

            Text(1215,215,345,26,"MANA  "+seat.Mana.Sum()+"    •    Mão "+seat.HandCount,body,Ink);

            for(int i=0;i<7;i++) { var c=TableWorld.Hex(new[]{"D5C084","B69AD0","7AAFCF","CB7963","A0AFC4","7DBB94","DBD8C5"}[i]); Fill(new Rect(1218+i*49,252,37,28),c*.35f); Text(1218+i*49,252,37,28,seat.Mana[i].ToString(),label,c); }

            if(match.Phase==Stage.Draw&&match.Stack.Count==0)

            {

                Text(1215,306,350,52,match.Turn==1?"Primeiro turno: escolha uma das compras.":"Compre uma carta principal e um terreno.",body,Ink);

                if(Button(1215,370,170,42,match.Turn==1?"Carta principal":"Comprar as duas",true,true)) Submit(ActionKind.DrawMain);

                if(match.Turn==1&&Button(1395,370,175,42,"Terreno")) Submit(ActionKind.DrawTerrain);

            }

            else

            {

                bool can=match.Stack.Count==0&&match.Priority==match.Active;

                if(Button(1215,306,170,42,match.Phase==Stage.End?"Passar / encerrar":"Próxima fase",can,true)) Submit(ActionKind.NextPhase);

                if(Button(1395,306,175,42,"Passar prioridade",match.Phase==Stage.Main||match.Phase==Stage.End||match.Stack.Count>0)) Submit(ActionKind.Pass);

            }

            if((match.Phase==Stage.Draw||match.Phase==Stage.Terrain)&&match.Stack.Count==0)

            {

                Text(1215,435,350,44,"QUATRO PILHAS  ·  selecione uma",cardName,Gold);

                for(int p=0;p<4;p++)

                {

                    float y=478+p*64; var top=match.Seats[match.Active].Top(p);

                    if(Button(1215,y,350,54,(p+1)+"   "+(top?.Name??"Vazia")+"   ·   "+match.Seats[match.Active].PileCount(p),true,p==selectedPile)) { selectedPile=p; selectedCard=null; }

                }

                Text(1215,754,350,80,"A compra de terreno entra na pilha escolhida. Ao esvaziar uma pilha, a reposição é imediata.",small,Muted);

            }

            else if(match.Stack.Count>0)

            {

                Text(1215,375,345,28,"PILHA  ·  "+match.Stack.Count+" ação(ões)",cardName,Gold);

                for(int i=0;i<Math.Min(6,match.Stack.Count);i++) Text(1215,418+i*47,350,43,(i==0?"TOPO   ":"↓   ")+match.Stack[match.Stack.Count-1-i].Description,body,Ink);

                Text(1215,730,350,63,"Passe para resolver o topo ou use uma resposta disponível.",small,Muted);int responseIndex=0;foreach(var monk in match.Board.SelectMany(c=>c.Pieces).Where(p=>match.CanActivate(p.Id)).Take(2)){if(Button(1215,800+responseIndex*43,350,38,"Responder · "+monk.Card.Name))Submit(ActionKind.Activate,unit:monk.Id);responseIndex++;}

            }

            else DrawSelectedCell();

            Text(1215,895,345,55,match.Log.LastOrDefault()??"",small,Muted);

        }

        void DrawSelectedCell()

        {

            if(selectedCell<0) { Text(1215,395,350,140,"Explore a mesa\n\nSelecione uma carta na mão ou um terreno para ver suas criaturas. Arraste a mesa para girar; use a roda para aproximar.",body,Muted); return; }

            var cell=match.Board[selectedCell];

            Text(1215,379,350,32,cell.Terrain?.Name??"Vazio",heading,Ink);

            Text(1215,421,350,24,"CASA "+selectedCell%11+" × "+selectedCell/11+"   ·   "+cell.Pieces.Count+" peça(s)",small,Gold);

            unitScroll=GUI.BeginScrollView(new Rect(1215,459,355,250),unitScroll,new Rect(0,0,328,Math.Max(240,cell.Pieces.Count*62)));

            for(int n=0;n<cell.Pieces.Count;n++)

            {

                var p=cell.Pieces[n]; string text=p.Card.Name+"  "+p.Attack+"/"+p.Health+"\nPA "+p.Actions+"  ·  Movimento "+p.Movement;

                if(p.CarrierId>=0)text=p.Card.Name+" · embarcada\nPA "+p.Actions+" · "+match.Find(p.CarrierId)?.Card.Name;
                else if(p.Card.IsVehicle)text=p.Card.Name+" · "+p.Attack+"/"+p.Health+"\nPassageiros "+match.Passengers(p.Id).Count+"/"+match.VehicleCapacity(p.Id);
                if(Button(0,n*62,327,56,text,true,selectedUnit==p.Id)) { selectedUnit=p.Id; }

            }

            GUI.EndScrollView();

            if(selectedUnit>=0)

            {

                var unit=match.Find(selectedUnit); if(unit==null) { selectedUnit=-1; return; }

                if(unit.Card.Kind==CardType.Equipment) {

                    Text(1215,722,345,40,"Equipar: "+unit.Card.EquipCost+" mana genérica",small,Gold);

                    if(Button(1215,765,350,40,"Escolher criatura para equipar")) BeginEquip(unit.Id);

                    return;

                }

                if(match.CanActivate(unit.Id)&&Button(1215,685,350,38,"Ativar habilidade")) Submit(ActionKind.Activate,unit:unit.Id);

                if(Button(1215,729,350,40,attackGroup?"Atacar com grupo apto da casa":"Atacar apenas com a selecionada")) attackGroup=!attackGroup;

                bool cargoAction=false;
                if(match.CanDisembark(unit.Id)){cargoAction=true;if(Button(1215,777,350,40,"Desembarcar · 1 PA"))Submit(ActionKind.DisembarkVehicle,unit:unit.Id);}
                else if(match.Board.SelectMany(c=>c.Pieces).Any(v=>match.CanBoard(unit.Id,v.Id))){cargoAction=true;if(Button(1215,777,350,40,"Embarcar · 1 PA"))Submit(ActionKind.BoardVehicle,unit:unit.Id);}
                if(unit.Card.IsVehicle){cargoAction=true;var words=match.EffectiveKeywords(unit.Id);Text(1215,784,350,92,"Tripulação: "+match.Passengers(unit.Id).Count+" / "+unit.Card.VehicleCrew+" exigido\nPalavras-chave: "+(words.Count==0?"nenhuma":string.Join(", ",words.Select(KeywordLabel))),small,Gold);}
                if(!cargoAction&&match.OrdersFor(match.Controller).Count==0) Text(1215,784,350,85,"Arraste a criatura até o destino. Rotas longas continuam nos próximos turnos; buracos e bloqueios fazem a peça esperar.",small,Muted);

            }

        }

        void DrawDefense()

        {

            var battle=match.Defense;

            var defenders=match.Board[battle.Target].Pieces.Where(p=>p.Owner==battle.Defender&&p.Card.Kind!=CardType.Equipment).ToArray();

            bool capital=match.Board[battle.Target].CapitalOwner==battle.Defender;

            var options=(capital?new[]{-1}:Array.Empty<int>()).Concat(defenders.Select(p=>p.Id)).ToArray();

            if(lastDefense!=battle) { lastDefense=battle; blockers=Enumerable.Repeat(options.FirstOrDefault(),battle.Attackers.Length).ToArray(); unitScroll=Vector2.zero; }

            Text(1215,229,350,105,"DISTRIBUA OS CONFRONTOS\nCada botão alterna o bloqueador. A mesma criatura pode enfrentar vários atacantes.",body,Gold);

            unitScroll=GUI.BeginScrollView(new Rect(1215,355,355,380),unitScroll,new Rect(0,0,328,Math.Max(360,blockers.Length*85)));

            for(int i=0;i<blockers.Length;i++)

            {

                var a=match.Find(battle.Attackers[i]); string defending=blockers[i]<0?"Capital recebe dano":match.Find(blockers[i])?.Card.Name??"Bloqueador ausente";

                Text(0,i*85,320,29,a?.Card.Name??"Atacante ausente",cardName,Ink);

                if(Button(0,i*85+31,327,44,defending)) blockers[i]=options[(Array.IndexOf(options,blockers[i])+1)%options.Length];

            }

            GUI.EndScrollView();

            if(Button(1215,769,350,48,"Confirmar bloqueios",true,true)) Submit(ActionKind.Defend,defense:blockers);

            Text(1215,840,350,85,"Depois dos bloqueios, todos podem responder na pilha antes do dano.",small,Muted);

        }

        bool DrawCard(Rect rect,Definition card,bool selected=false,bool interactive=true)

        {

            RenderCardFace(rect,card,CardStyle(card),selected);
            if(!interactive)return false;
            if(rect.Contains(Event.current.mousePosition)&&Event.current.type==EventType.MouseDown&&Event.current.button==1){inspected=card;detailScroll=Vector2.zero;Event.current.Use();return false;}

            return GUI.Button(rect,GUIContent.none,GUIStyle.none);

        }

        void DrawBoardLabels()

        {

            Text(27,122,750,27,"SALA DOS REINOS   /   TERRENOS EM DISPUTA",cardName,Gold);

            if(Button(760,118,178,35,world.RoomName)) world.SetRoom(1-world.RoomIndex);
            if(Button(947,118,64,35,"↶")) world.Yaw-=45;

            if(Button(1018,118,64,35,"↷")) world.Yaw+=45;

            if(Button(1089,118,76,35,"Vista")) { world.Elevation=world.Elevation>65?57:78; }

            world.SelectPile(match.Active,selectedPile);
            world.Highlight(i=>!handoff&&(match.Phase==Stage.Terrain?match.CanPlace(i):selectedCommander?match.CanSummonCommander(i):selectedCard!=null?match.CanPlay(catalog.Get(selectedCard),i,match.Board[i].Pieces.FirstOrDefault(p=>match.Enemies(match.Controller,p.Owner))?.Id??-1):selectedUnit>=0&&(match.CanMove(selectedUnit,i)||match.CanAttack(selectedUnit,i))));

            for(int i=0;i<121;i++)

            {

                if(match.Board[i].Pieces.Count<=1) continue;

                var point=world.View.WorldToScreenPoint(TableWorld.Position(i)+Vector3.up*.8f); if(point.z<=0) continue;

                Text(point.x/scale-20,(Screen.height-point.y)/scale,40,23,match.Board[i].Pieces.Count.ToString(),label,Gold);

            }

        }

        void Veil() { Fill(new Rect(0,0,1600,1000),new Color(.025f,.065f,.085f,.94f)); }

        void DrawHandoff()

        {

            Veil(); var color=TableWorld.Seats[match.Controller];

            Text(430,275,740,48,"A MESA AGUARDA",heading,Gold);

            Text(430,350,740,80,match.Seats[match.Controller].Name,title,color);

            Text(430,460,740,100,match.Defense!=null?"Sua vez de escolher os bloqueios. Passe a tela para este jogador.":"Passe a tela para este jogador antes de revelar a mão.",body,Ink);

            if(Button(430,595,740,62,"Estou pronto · revelar minha mão",true,true)) handoff=false;

        }

        void DrawCollection()

        {

            Veil(); Text(55,38,1100,60,"BIBLIOTECA DE EXPANSÕES",title,Ink);

            if(Button(1370,45,175,45,"Voltar à mesa")) collection=false;

            Text(60,120,1480,42,catalog.Expansions.Count+" pacotes · "+catalog.Cards.Count+" definições · conteúdo gratuito de teste",body,Gold);

            filter=GUI.TextField(new Rect(60,178,450,35),filter); Text(525,182,320,30,"Buscar por nome ou ID",small,Muted);

            if(Button(60,236,150,36,"Todas",true,expansion=="")) { expansion=""; catalogPage=0; }

            expansionScroll=GUI.BeginScrollView(new Rect(225,230,1320,61),expansionScroll,new Rect(0,0,Math.Max(1295,catalog.Expansions.Count*213),37));

            int n=0; foreach(var item in catalog.Expansions) { if(Button(n*213,0,203,36,item.Value,true,expansion==item.Key)) { expansion=item.Key; catalogPage=0; } n++; }

            GUI.EndScrollView();

            var cards=catalog.Cards.Where(c=>(expansion==""||c.Expansion==expansion)&&(c.Name.IndexOf(filter,StringComparison.OrdinalIgnoreCase)>=0||c.Id.IndexOf(filter,StringComparison.OrdinalIgnoreCase)>=0)).OrderBy(c=>c.Expansion).ThenBy(c=>c.Name).ToArray();

            int pages=Math.Max(1,(cards.Length+11)/12); catalogPage=Mathf.Clamp(catalogPage,0,pages-1);

            for(int i=0;i<12&&catalogPage*12+i<cards.Length;i++) { var c=cards[catalogPage*12+i]; if(DrawCard(new Rect(60+i%6*248,305+i/6*300,232,270),c)) {inspected=c;detailScroll=Vector2.zero;} Text(65+i%6*248,578+i/6*300,225,22,c.Id,small,Muted); }

            if(Button(60,921,170,42,"Anterior",catalogPage>0)) catalogPage--;

            Text(255,930,400,28,"Página "+(catalogPage+1)+" / "+pages+"   ·   "+cards.Length+" cartas",small,Ink);

            if(Button(650,921,170,42,"Próxima",catalogPage+1<pages)) catalogPage++;

        }

        void DrawHelp()

        {

            Veil(); Text(220,115,1100,65,"DA PRIMEIRA CARTA AO COMBATE",title,Ink);

            Text(230,235,1100,540,"1. COMPRE — no primeiro turno do primeiro jogador, escolha um deck. Depois, compre de ambos. Escolha a pilha que receberá o terreno.\n\n2. CONSTRUA — selecione uma das quatro pilhas e clique na borda dourada. Substituir um terreno destrói seus ocupantes.\n\n3. INVOQUE — na fase principal, selecione uma carta da mão e um terreno seu. Todos passam prioridade para resolver a pilha.\n\n4. AVANCE — selecione a miniatura na lista do terreno e clique numa casa adjacente. O vazio não pode ser atravessado. Use terrenos para construir caminhos.\n\n5. ATAQUE — o defensor distribui os bloqueios. Depois das respostas, o dano é simultâneo e acumula até o início/fim do próprio turno. A capital compartilha a vida do jogador.\n\nCâmera: botão direito + arrastar, roda para zoom, botões para girar e vista alta. Mãos ficam ocultas nas trocas de jogador.",body,Ink);

            Text(230,795,1100,65,"Esta base cobre criaturas, terrenos e efeitos básicos. Cartas medievais, equipamentos e construções estão em teste. General, Comandante, missões e rede continuam pendentes.",small,Gold);

            if(Button(230,881,540,54,"Voltar ao jogo",true,true)) help=false;

            if(Button(790,881,540,54,"Desistir com confirmação")) { help=false; confirmQuit=true; }

        }

        void DrawQuit()

        {

            Veil(); Text(410,320,780,90,"Desistir elimina "+match.Seats[Viewer].Name+" desta partida.",heading,Ink);

            if(Button(410,520,365,55,"Continuar jogando",true,true)) confirmQuit=false;

            if(Button(795,520,365,55,"Confirmar desistência")) { confirmQuit=false; Submit(ActionKind.Concede); }

        }

        void DrawResult()

        {

            Veil(); string result=match.WinningTeam<0?"EMPATE":teams?"DUPLA "+(match.WinningTeam+1)+" VENCEU":match.Seats.First(s=>s.Team==match.WinningTeam).Name.ToUpperInvariant()+" VENCEU";

            Text(340,340,960,150,result,title,Gold);

            if(Button(490,560,620,65,"Preparar outra mesa",true,true)) NavigateHub(HubPage.Play);

        }

        void OnDestroy() { DisposeReactionIcons(); if(world!=null) Destroy(world.gameObject); }

    }

}






