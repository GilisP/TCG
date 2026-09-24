using System;
using System.Linq;
using UnityEngine;
namespace TCG
{
    public sealed partial class GameView
    {
        bool mainMenu=true, playSetup, hasStarted;
        MatchMode selectedMode;
        string solSlot, luaSlot;
        Vector2 menuDeckScroll;
        static readonly string[] modeNames={"Duelo local","Partida rápida","Treino"};
        void OpenPlaySetup()
        {playSetup=true;solSlot=library.solId;luaSlot=library.luaId;}
        void StartSelectedMatch()
        {
            bool ok=Persist(next=>{
                if(!string.IsNullOrEmpty(solSlot))next.Equip(solSlot,0);
                if(!string.IsNullOrEmpty(luaSlot))next.Equip(luaSlot,1);
            },"Decks escolhidos para a partida.");
            if(ok)NewGame();
        }
        void DrawMainMenu()
        {
            Frame(new Rect(55,45,1330,855),bronze);
            var background=GeneratedArt.Get("realm-reference");
            if(background!=null) {GUI.DrawTexture(new Rect(61,51,1318,843),background,ScaleMode.ScaleAndCrop);Fill(new Rect(61,51,1318,843),new Color(.025f,.035f,.05f,.75f));}
            Text(90,83,1250,64,"FRONTEIRAS",new GUIStyle(heading){fontSize=42},gold);
            Text(92,151,1180,30,"CRÔNICAS DOS REINOS  ·  construa seu exército, defenda seu domínio",body,muted);
            if(!playSetup)
            {
                Frame(new Rect(96,225,555,559),bronze);
                Text(121,255,505,74,"Um reino começa com suas cartas.",heading,ink);
                if(Button(121,360,505,67,"JOGAR",new Color(.30f,.26f,.15f)))OpenPlaySetup();
                if(Button(121,449,505,67,"MONTAR DECK",new Color(.17f,.30f,.27f)))deckEditor=true;
                if(hasStarted && Button(121,548,505,49,"Continuar partida em andamento",panel))mainMenu=false;
                Text(121,632,505,109,"Seus decks ficam salvos em slots independentes. A partida atual pode ser retomada enquanto o jogo estiver aberto.",body,muted);
                Text(715,257,585,38,"SEUS SLOTS SALVOS",heading,gold);
                menuDeckScroll=GUI.BeginScrollView(new Rect(706,320,610,407),menuDeckScroll,new Rect(0,0,580,Math.Max(100,library.entries.Count*77)));
                for(int i=0;i<library.entries.Count;i++)
                {
                    var entry=library.entries[i]; Frame(new Rect(0,i*77,571,68),bronze);
                    Text(14,i*77+6,540,30,"Slot "+(i+1)+" · "+entry.deck.name,body,ink);
                    Text(14,i*77+38,540,23,Catalog.Validate(entry.deck).Count==0 ? "Pronto para jogar" : "Rascunho salvo — termine no Arsenal",small,muted);
                }
                GUI.EndScrollView();
                Text(715,754,580,67,"Três modos locais. Em Treino você controla os dois lados. Adversário por IA ainda não incluído.",small,muted);
            }
            else
            {
                Text(96,218,1240,40,"ESCOLHA O MODO",heading,gold);
                string[] descriptions={"Dois jogadores no mesmo computador. 50 de vida, mana dos territórios e troca de prioridade privada.","30 de vida e +1 mana incolor a cada início de turno. Mantém seus decks completos e as regras de combate.","Controle os dois lados, sem tela de troca de jogador. +10 mana de cada elemento por turno para testar cartas."};
                for(int i=0;i<3;i++)
                {
                    float x=96+i*418;Frame(new Rect(x,273,400,190),i==(int)selectedMode?gold:bronze,i==(int)selectedMode);
                    if(Button(x+15,290,370,43,modeNames[i],panel))selectedMode=(MatchMode)i;
                    Text(x+18,350,364,92,descriptions[i],body,ink);
                }
                DrawSlotPicker(96,499,0);DrawSlotPicker(741,499,1);
                Text(96,682,1230,63,hasStarted ? "Iniciar substitui a partida em andamento. Os decks salvos e seus rascunhos permanecem disponíveis." : "Escolha os slots de cada jogador. Rascunhos incompletos não aparecem nesta seleção.",body,muted);
                if(Button(96,774,300,57,"Voltar",panel))playSetup=false;
                if(Button(421,774,365,57,"Montar outro deck",panel))deckEditor=true;
                if(Button(811,774,530,57,"Iniciar · "+modeNames[(int)selectedMode],new Color(.23f,.36f,.24f)))StartSelectedMatch();
            }
            Text(92,854,1250,28,deckNotice,small,muted);
        }
        void DrawSlotPicker(float x,float y,int player)
        {
            Frame(new Rect(x,y,600,151),bronze);
            Text(x+16,y+12,567,30,player==0?"JOGADOR 1 · SOL":"JOGADOR 2 · LUA",body,player==0?gold:blue);
            var valid=library.entries.Where(e=>Catalog.Validate(e.deck).Count==0).ToArray();
            string selected=player==0?solSlot:luaSlot; var entry=valid.FirstOrDefault(e=>e.id==selected);
            string label=entry?.deck.name ?? (player==0?library.solDeck.name:library.luaDeck.name)+" (equipado)";
            if(Button(x+16,y+54,567,46,label+"  →",panel,valid.Length>0))
            {int index=Array.FindIndex(valid,e=>e.id==selected);string id=valid[(index+1)%valid.Length].id;if(player==0)solSlot=id;else luaSlot=id;}
            Text(x+16,y+108,567,27,"Clique para alternar · "+valid.Length+" slots completos disponíveis",small,muted);
        }
    }
}
