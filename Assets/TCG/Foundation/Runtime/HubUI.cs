using System;
using System.Linq;
using UnityEngine;
using TCG.Foundation;
namespace TCG.Table
{
    public sealed partial class TableView
    {
        enum HubPage { Play, Collection, Shop, Settings }
        HubPage hubPage;
        bool sessionAvailable, cosmeticsTab, exitRequested;
        string hubNotice="";
        readonly string[] matchBacks={"classic","classic","classic","classic"};
        void NavigateHub(HubPage page)
        {
            hubPage=page;menu=true;libraryOpen=page==HubPage.Collection;
            collection=help=confirmQuit=false;inspected=null;pendingPurchase=null;filtersOpen=false;ResetDrag();CloseReactionMenu();
        }
        void DrawHub()
        {
            Fill(new Rect(0,0,1600,1000),Dark);
            Text(36,20,620,39,"F R O N T E I R A S",heading,Gold);
            Text(36,59,580,24,"A MESA DOS REINOS",small,Muted);
            string[] tabs={"01  JOGAR","02  COLEÇÃO E DECKS","03  LOJA","04  MENU"};
            GUI.enabled=inspected==null&&!exitRequested&&pendingPurchase==null&&!filtersOpen&&openedBooster==null;
            for(int i=0;i<4;i++)if(Button(640+i*232,25,222,58,tabs[i],true,(int)hubPage==i))NavigateHub((HubPage)i);
            Fill(new Rect(35,105,1530,2),Gold*.45f);
            if(hubPage==HubPage.Play)DrawPlayPage();
            else if(hubPage==HubPage.Collection)
            {
                Text(35,127,730,38,"SEU ARSENAL",heading,Ink);
                if(Button(1110,125,215,40,"Cartas e decks",true,!cosmeticsTab))cosmeticsTab=false;
                if(Button(1340,125,225,40,"Cosméticos",true,cosmeticsTab))cosmeticsTab=true;
                if(cosmeticsTab)DrawCosmeticPage();
                else
                {
                    var matrix=GUI.matrix;
                    GUI.matrix=matrix*Matrix4x4.TRS(new Vector3(0,145,0),Quaternion.identity,new Vector3(1,.84f,1));
                    DrawLibrary();GUI.matrix=matrix;
                }
            }
            else if(hubPage==HubPage.Shop)DrawShopPage();
            else if(progressPage)DrawProgressPage();else DrawSettingsPage();
            GUI.enabled=true;
            if(openedBooster!=null)DrawBoosterOpening(); if(inspected!=null)DrawInspection(); if(pendingPurchase!=null)DrawPurchaseConfirmation(); if(filtersOpen)DrawFilterDialog();
            if(exitRequested)
            {
                Fill(new Rect(0,0,1600,1000),new Color(0,0,0,.85f));
                Fill(new Rect(410,310,780,330),Panel);
                Text(450,345,700,105,"Sair do jogo?",heading,Gold);
                Text(450,410,700,70,"Coleção e decks salvos serão mantidos. A partida atual não será salva.",body,Ink);
                if(Button(450,535,330,58,"Continuar",true,true))exitRequested=false;
                if(Button(805,535,330,58,"Sair"))Application.Quit();
            }
        }
        bool localDeckSetup;
        void PrepareLocalTable(){localDeckSetup=true;commanderDemo=true;useSavedDecks=true;hubNotice="Escolha um deck para cada jogador antes de iniciar.";}
        void DrawPlayPage()
        {
            Text(45,135,950,50,localDeckSetup?"ESCOLHA OS DECKS DA MESA":"ENCONTRE SUA PRÓXIMA MESA",menuTitle,Ink);
            Text(48,200,950,65,"Todo deck tem um comandante e seu próprio deck de terrenos.",body,Muted);
            if(localDeckSetup){if(useSavedDecks)DrawSavedDeckSetup();else DrawCommanderDemoSetup();}
            else{
                Fill(new Rect(45,300,990,550),Panel);
                Text(75,335,900,45,"Prepare sua próxima partida",heading,Gold);
                Text(75,410,900,120,"Crie uma mesa local e escolha um deck para cada jogador. Nas salas online, cada participante escolhe e confirma o próprio deck antes de ficar pronto.",body,Ink);
                if(Button(75,630,900,60,"Abrir coleção e montar um deck"))OpenLibrary();
            }
            Fill(new Rect(1080,140,480,785),Panel);
            Text(1110,168,420,45,"PREPARAR PARTIDA",heading,Gold);
            Text(1110,231,420,30,"Jogadores",body,Muted);
            for(int n=2;n<=4;n++)if(Button(1110+(n-2)*145,275,130,48,n.ToString(),true,players==n)){players=n;if(n!=4)teams=false;}
            if(Button(1110,348,420,48,teams?"Duplas · lados opostos":"Todos contra todos",players==4))teams=!teams;
            if(localDeckSetup&&Button(1110,428,420,50,useSavedDecks?"Salvos / precons / planejados":"Experimentar comandante"))useSavedDecks=!useSavedDecks;
            if(Button(1110,510,420,55,"SALAS ONLINE / REDE LOCAL"))networkPage=true;
            Text(1110,585,410,120,localDeckSetup?(useSavedDecks?"Escolha os decks à esquerda. Rascunhos inválidos não podem iniciar a partida.":"Escolha um comandante: precon correspondente, seguido do planejado."):"Criar mesa abre a escolha dos decks, antes de iniciar o jogo.",body,Muted);
            if(Button(1110,737,420,64,localDeckSetup?"INICIAR COM ESTES DECKS":"CRIAR MESA LOCAL",true,true)){if(localDeckSetup){commanderDemo=true;StartMatch();}else PrepareLocalTable();}
            if(Button(1110,823,420,51,"Voltar à partida",sessionAvailable&&!match.Over)){menu=libraryOpen=false;}
            Text(45,958,1490,27,hubNotice,small,Gold);
        }
        void DrawSettingsPage()
        {
            Text(45,140,1200,60,"DEIXE A MESA DO SEU JEITO",menuTitle,Ink);
            Fill(new Rect(45,240,730,610),Panel);Fill(new Rect(800,240,760,610),Panel);
            Text(75,269,650,45,"Apresentação",heading,Gold);
            if(Button(75,346,660,58,Screen.fullScreen?"Tela cheia: ligada":"Tela cheia: desligada")){Screen.fullScreen=!Screen.fullScreen;PlayerPrefs.SetInt("TCG.Fullscreen",Screen.fullScreen?1:0);PlayerPrefs.Save();}
            if(Button(75,424,660,58,"Ambiente: "+world.RoomName)){world.SetRoom(1-world.RoomIndex);PlayerPrefs.SetInt("TCG.Room",world.RoomIndex);PlayerPrefs.Save();}
            Text(75,525,650,42,"Volume geral",body,Ink);
            float volume=GUI.HorizontalSlider(new Rect(80,589,640,25),AudioListener.volume,0,1);if(!Mathf.Approximately(volume,AudioListener.volume)){AudioListener.volume=volume;PlayerPrefs.SetFloat("TCG.Volume",volume);PlayerPrefs.Save();}
            Text(75,663,650,90,"O ambiente muda a aparência da sala. As regras da partida permanecem as mesmas.",body,Muted);
            Text(830,269,690,45,"Sua sessão",heading,Gold);
            Text(830,346,675,105,"Coleção e decks ficam salvos neste computador. A mesa atual pode ser retomada enquanto o jogo estiver aberto.",body,Ink);
            if(Button(830,497,690,58,"Voltar à partida",sessionAvailable&&!match.Over)){menu=libraryOpen=false;}
            if(Button(830,575,690,58,"Gerenciar meus decks"))OpenLibrary();
            if(Button(830,650,690,58,"Estatísticas e missões"))progressPage=true;
            if(Button(830,724,690,58,"Sair do jogo"))exitRequested=true;
        }
        void DrawCosmeticPage()
        {
            Text(45,200,1490,55,"Escolha um deck para seu verso. Para trocar a arte de uma carta, abra sua ficha na biblioteca.",body,Muted);
            var items=CosmeticCatalog.All;
            for(int i=0;i<items.Length;i++)
            {
                var c=items[i];float x=45+i*380;Fill(new Rect(x,288,355,545),Panel);
                DrawBackPreview(new Rect(x+77,325,200,280),c);
                Text(x+20,640,315,35,c.Name,heading,Gold);
                Text(x+20,690,315,48,library.OwnsCosmetic(c.Id)?"Verso de cartas · possuído":"Disponível na loja · "+c.Price+" moedas",small,Muted);
                if(Button(x+20,756,315,52,!library.OwnsCosmetic(c.Id)?"Adquira na loja":draft==null?"Escolha um deck":draft.cardBack==c.Id?"Equipado":"Usar no deck",draft!=null&&library.OwnsCosmetic(c.Id),draft?.cardBack==c.Id))
                {draft.cardBack=c.Id;SaveDraft();}
            }
            if(Button(45,880,355,50,"Ver cosméticos na loja")){shopCosmetics=true;NavigateHub(HubPage.Shop);} Text(430,880,1100,65,draft==null?"Nenhum deck selecionado.":"Personalizando: "+draft.name,body,Gold);
        }
        void DrawBackPreview(Rect r,CosmeticDefinition cosmetic)
        {
            Fill(r,Gold);Fill(new Rect(r.x+5,r.y+5,r.width-10,r.height-10),TableWorld.Hex(cosmetic.Color));
            Fill(new Rect(r.x+16,r.y+16,r.width-32,2),Gold);Fill(new Rect(r.x+16,r.yMax-18,r.width-32,2),Gold);
            var matrix=GUI.matrix;GUIUtility.RotateAroundPivot(45,r.center);Fill(new Rect(r.center.x-32,r.center.y-32,64,64),Gold);Fill(new Rect(r.center.x-23,r.center.y-23,46,46),TableWorld.Hex(cosmetic.Color));GUI.matrix=matrix;
            Text(r.x+12,r.yMax-58,r.width-24,30,"F R O N T E I R A S",label,Gold);
        }
        void ApplyDeckCosmetics()
        {
            for(int p=0;p<4;p++)
            {
                var deck=useSavedDecks?library.Data.decks.FirstOrDefault(d=>d.id==seatDecks[p]):null;
                matchBacks[p]=deck?.cardBack??"classic";world.SetCardBack(p,matchBacks[p]);
            }
        }
    }
}



