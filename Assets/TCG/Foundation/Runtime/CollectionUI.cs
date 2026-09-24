using System;
using System.Linq;
using System.IO;
using UnityEngine;
using TCG.Foundation;
namespace TCG.Table
{
    public sealed partial class TableView
    {
        CollectionStore collectionStore;CollectionLibrary library;DeckData draft;
        bool libraryOpen;int libraryPage;string librarySearch="",libraryMessage="";
        Vector2 deckScroll,deckListScroll,validationScroll;readonly string[] seatDecks=new string[4];bool useSavedDecks;
        void LoadLibrary()
        {
            collectionStore=new CollectionStore(Path.Combine(Application.persistentDataPath,"Library"));library=collectionStore.Load(catalog);libraryMessage=collectionStore.Warning;
        }
        void OpenLibrary(){NavigateHub(HubPage.Collection);libraryPage=0;ResetDrag();}
        void PersistLibrary()
        {
            try{collectionStore.Save(library);libraryMessage="Salvo neste computador.";}
            catch(Exception e){libraryMessage=e.Message;library=collectionStore.Load(catalog);}
        }
        void CollectCard(string id){if(!collectionStore.CanWrite)return;library.Collect(id);PersistLibrary();}
        void AddToDraft(Definition c)
        {
            if(draft==null){libraryMessage="Crie ou abra um deck primeiro.";return;}
            if(!library.Owns(c.Id)){libraryMessage="Adicione a carta à coleção primeiro.";return;}
            if(c.IsCommander){draft.commander=c.Id;return;}
            var zone=c.Kind==CardType.Terrain?draft.terrains:draft.main;
            if(!draft.experimental&&zone.Count(x=>x==c.Id)>=c.MaxCopies){libraryMessage="Esta carta já está no deck.";return;}
            zone.Add(c.Id);SelectDeckSection(c.Kind==CardType.Terrain);
        }
        void NewDraft(bool test=false){SelectDeckSection(false);draft=new DeckData{experimental=test,name=test?"Meu deck de teste":"Meu deck"};libraryMessage="Rascunho novo. Salve para manter suas alterações.";}
        void SaveDraft(){try{library.SaveDeck(draft);PersistLibrary();}catch(Exception e){libraryMessage=e.Message;}}
        string CardName(string id){try{return catalog.Get(catalog.Identity(id)).Name;}catch{return "Carta ausente: "+id;}}
        void DrawLibrary()
        {
            GUI.enabled=GUI.enabled&&inspected==null&&!filtersOpen;
            
            
            Fill(new Rect(20,82,270,838),Panel);Text(35,96,240,35,"Meus decks",heading,Ink);
            if(Button(35,141,240,37,"Novo deck padrão"))NewDraft();
            if(Button(35,185,240,37,"Novo deck experimental"))NewDraft(true);
            Text(35,231,240,59,"Padrão: 100 + 50 + comandante. Experimental: mesa de testes com repetições.",small,Muted);
            deckListScroll=GUI.BeginScrollView(new Rect(28,302,250,425),deckListScroll,new Rect(0,0,230,Math.Max(415,library.Data.decks.Count*52)));
            for(int i=0;i<library.Data.decks.Count;i++){var d=library.Data.decks[i];if(Button(0,i*52,225,46,d.name,true,draft?.id==d.id)){draft=d.Copy();SelectDeckSection(false);libraryMessage="Deck aberto. Alterações só são persistidas ao salvar.";}}
            GUI.EndScrollView();
            Text(35,742,240,92,library.Data.owned.Count+" cartas na coleção\nAdquira novas cartas com moedas na Loja. Decks e coleção ficam salvos ao fechar o jogo.",small,Muted);
            Fill(new Rect(305,82,675,838),Panel);
            Text(324,96,180,30,"Biblioteca",heading,Ink);
            string search=GUI.TextField(new Rect(324,139,637,34),librarySearch);if(search!=librarySearch){librarySearch=search;libraryPage=0;}
            if(Button(324,183,235,36,"Filtros ("+libraryFilters.SelectedCount+")"))OpenFilters(false);
            bool owned=CheckBox(571,183,195,"Só minha coleção",libraryFilters.OwnedOnly);bool onlyCommanders=CheckBox(776,183,185,"Só comandantes",libraryFilters.CommandersOnly);if(onlyCommanders!=libraryFilters.CommandersOnly){libraryFilters.CommandersOnly=onlyCommanders;libraryPage=0;}
            if(owned!=libraryFilters.OwnedOnly){libraryFilters.OwnedOnly=owned;libraryPage=0;}
            var cards=FilteredLibrary();
            int pages=Math.Max(1,(cards.Length+5)/6);libraryPage=Mathf.Clamp(libraryPage,0,pages-1);
            for(int n=0;n<6&&libraryPage*6+n<cards.Length;n++)
            {
                var c=cards[libraryPage*6+n];float x=329+(n%3)*214,y=234+(n/3)*307;
                if(DrawCard(new Rect(x+24,y,145,260),c)){inspected=c;detailScroll=Vector2.zero;}
                if(Button(x,y+265,193,32,!c.Playable?"Em revisão":library.Owns(c.Id)?(c.IsCommander?"Usar comandante":"Adicionar ao deck"):"Ver na loja",c.Playable&&(library.Owns(c.Id)||collectionStore.CanWrite))){if(library.Owns(c.Id))AddToDraft(c);else{shopSearch=c.Name;shopBoosters=false;shopCosmetics=false;NavigateHub(HubPage.Shop);}}
            }
            if(Button(324,861,65,35,"‹",libraryPage>0))libraryPage--;Text(402,866,455,26,(libraryPage+1)+" / "+pages+" · "+cards.Length+" cartas",small,Muted);if(Button(896,861,65,35,"›",libraryPage<pages-1))libraryPage++;
            Fill(new Rect(995,82,585,838),Panel);
            if(draft==null)Text(1015,112,540,130,"Crie um deck ou abra um salvo. Você pode salvar um rascunho incompleto e terminar depois.",body,Ink);
            else
            {
                draft.name=GUI.TextField(new Rect(1015,99,350,36),draft.name,80);if(Button(1376,99,183,36,"Salvar rascunho",collectionStore.CanWrite,true))SaveDraft();
                Text(1015,145,540,30,(draft.experimental?"EXPERIMENTAL":"PADRÃO")+" · Principal "+draft.main.Count+" · Terrenos "+draft.terrains.Count,cardName,Gold);
                Text(1015,181,530,38,"Comandante: "+(string.IsNullOrEmpty(draft.commander)?"não definido":CardName(draft.commander)),small,Muted);
                if(!string.IsNullOrEmpty(draft.commander)&&Button(1482,182,77,31,"Limpar"))draft.commander="";
                if(Button(1015,232,260,40,"Principal · "+draft.main.Count,true,!editingTerrains))SelectDeckSection(false);
                if(Button(1288,232,270,40,"Terrenos · "+draft.terrains.Count,true,editingTerrains))SelectDeckSection(true);
                Text(1015,279,540,33,editingTerrains?"Deck de terrenos vinculado a: "+draft.name:"Cartas do deck principal",small,Muted);
                string searchDeck=GUI.TextField(new Rect(1015,321,313,32),deckSearch);
                if(searchDeck!=deckSearch){deckSearch=searchDeck;deckScroll=Vector2.zero;}
                if(Button(1340,321,220,32,"Filtros ("+deckFilters.SelectedCount+")"))OpenFilters(true);
                bool section=CheckBox(1015,363,540,"Biblioteca: só a aba em montagem",librarySectionOnly);
                if(section!=librarySectionOnly){librarySectionOnly=section;libraryPage=0;}
                var zone=editingTerrains?draft.terrains:draft.main;var entries=FilteredDeck().ToArray();
                deckScroll=GUI.BeginScrollView(new Rect(1010,407,555,208),deckScroll,new Rect(0,0,530,Math.Max(196,entries.Length*42)));
                for(int n=0;n<entries.Length;n++)
                {
                    string id=entries[n];int qty=zone.Count(x=>x==id);Text(5,n*42,405,37,qty+" × "+CardName(id),small,Ink);
                    if(Button(415,n*42,45,34,"+")){try{AddToDraft(catalog.Get(catalog.Identity(id)));}catch(InvalidOperationException e){libraryMessage=e.Message;}}
                    if(Button(472,n*42,45,34,"−"))zone.Remove(id);
                }
                if(entries.Length==0)Text(5,10,510,97,zone.Count==0?"Esta parte do deck está vazia. Adicione cartas pela biblioteca.":"Nenhuma carta corresponde aos filtros. O conteúdo do deck foi mantido.",small,Muted);
                GUI.EndScrollView();
                Text(1015,621,530,24,entries.Length+" cartas distintas visíveis · "+zone.Count+" cartas nesta parte do deck",small,Muted);
                var errors=library.Validate(draft,true);Text(1015,657,540,30,errors.Count==0?"Deck principal e terrenos prontos":"Revisão do conjunto",cardName,errors.Count==0?TableWorld.Seats[1]:Gold);
                validationScroll=GUI.BeginScrollView(new Rect(1010,703,555,192),validationScroll,new Rect(0,0,530,Math.Max(181,errors.Count*39)));
                for(int i=0;i<errors.Count;i++)Text(4,i*39,520,38,errors[i],small,Muted);GUI.EndScrollView();
            }
            Fill(new Rect(20,932,1560,52),Panel);Text(35,940,1520,42,libraryMessage,small,Gold);
            GUI.enabled=true;
        }
        bool StartSavedDeckMatch()
        {
            var selected=Enumerable.Range(0,players).Select(p=>library.Data.decks.FirstOrDefault(d=>d.id==seatDecks[p])).ToArray();
            var errors=selected.SelectMany((d,i)=>library.Validate(d,true).Select(e=>"Jogador "+(i+1)+": "+e)).ToArray();
            if(errors.Length>0){libraryMessage=string.Join("\n",errors.Take(5));OpenLibrary();return false;}
            match=new Match(catalog,effects,players,teams,Environment.TickCount,selected.Select(d=>d.main.ToArray()).ToArray(),selected.Select(d=>d.terrains.ToArray()).ToArray(),selected.Select(d=>d.commander).ToArray());return true;
        }
        void DrawSavedDeckSetup()
        {
            Fill(new Rect(45,280,990,645),Panel);Text(75,310,910,45,"DECKS DA MESA",heading,Gold);
            Text(75,368,910,57,"Cada jogador escolhe um deck salvo. Vocês compartilham a coleção deste computador.",body,Muted);
            for(int p=0;p<players;p++)
            {
                float x=75+(p%2)*465,y=462+(p/2)*153;
                var current=library.Data.decks.FirstOrDefault(d=>d.id==seatDecks[p]);
                Text(x,y,430,35,new[]{"Âmbar","Jade","Safira","Rubi"}[p],heading,TableWorld.Seats[p]);
                if(Button(x,y+48,435,60,current?.name??"Escolher deck",library.Data.decks.Count>0))
                {int index=library.Data.decks.FindIndex(d=>d.id==seatDecks[p]);seatDecks[p]=library.Data.decks[(index+1)%library.Data.decks.Count].id;}
            }
            if(Button(75,828,900,56,"Criar ou editar um deck"))OpenLibrary();
        }
    }
}
