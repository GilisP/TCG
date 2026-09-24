using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TCG
{
    public sealed partial class GameView
    {
        DeckList[] decks = new DeckList[2];
        DeckLibrary library;
        Dictionary<string,DeckList> working = new Dictionary<string,DeckList>();
        string importPath = "";
        string draftId, search = "", inspectedId = "core-spark", importText = "";
        string deckNotice = "Salve suas listas e equipe um deck para cada jogador.";
        int deckTab, elementFilter = -1, costFilter = -1, kindFilter, preset, deckDialog;
        bool onlyIncluded, onlyNew;
        Vector2 collectionScroll, includedScroll, libraryScroll, importScroll;
        DeckList Draft => working[draftId];
        void LoadDecks()
        {
            try { library = DeckLibraryStore.Load(DeckLibraryStore.Root,out string message); if (message != "") deckNotice = message; }
            catch (Exception e) { library = DeckLibrary.Create(); deckNotice = "Biblioteca não carregada. Arquivo preservado; usando listas padrão. "+e.Message; }
            decks[0] = library.solDeck.Copy(); decks[1] = library.luaDeck.Copy();
            working = library.entries.ToDictionary(e => e.id,e => e.deck.Copy());
            if (working.Count == 0) NewDraft(Expansion.Starter(0)); else draftId = working.Keys.First();
        }
        bool Dirty(string id)
        {
            var entry = library.entries.FirstOrDefault(e => e.id == id);
            return entry == null || JsonUtility.ToJson(entry.deck) != JsonUtility.ToJson(working[id]);
        }
        bool Persist(Action<DeckLibrary> change,string message)
        {
            try
            {
                var next = library.Copy(); change(next); DeckLibraryStore.Save(DeckLibraryStore.Root,next); library = next;
                decks[0] = next.solDeck.Copy(); decks[1] = next.luaDeck.Copy(); deckNotice = message; return true;
            }
            catch (Exception e) { deckNotice = "Não foi possível salvar. Rascunho preservado. "+e.Message; return false; }
        }
        void NewDraft(DeckList deck)
        { deck.name = (deck.name ?? "Novo deck").Substring(0,Math.Min(64,(deck.name ?? "Novo deck").Length)); draftId = Guid.NewGuid().ToString("N"); working[draftId] = deck; collectionScroll = Vector2.zero; deckNotice = "Novo rascunho. Clique em Salvar lista para guardar."; }
        bool IsIncluded(DeckList deck,CardDefinition card) => card.Kind == CardKind.Commander ? deck.commander == card.Id : card.Kind == CardKind.Terrain ? deck.terrains.Contains(card.Id) : deck.main.Contains(card.Id);
        void ToggleCard(DeckList deck,CardDefinition card)
        {
            if (card.Kind == CardKind.Commander) deck.commander = card.Id;
            else if (card.Kind != CardKind.Token)
            { var list = card.Kind == CardKind.Terrain ? deck.terrains : deck.main; if (list.Contains(card.Id)) list.Remove(card.Id); else list.Add(card.Id); }
        }
        void DrawDeckEditor()
        {
            GUI.enabled = deckDialog == 0;
            Text(24,18,970,42,"ARSENAL  /  CRÔNICAS DOS REINOS",heading,gold);
            Text(24,61,1190,26,"100 cartas + 50 terrenos + 1 comandante. Salve suas listas e equipe os decks para a próxima partida.",small,muted);
            DrawLibrary(); var draft = Draft;
            Text(278,96,78,30,"NOME",small,gold); draft.name = GUI.TextField(new Rect(351,94,726,34),draft.name ?? "",64);
            Frame(new Rect(278,141,799,37),bronze);
            Text(288,148,780,25,$"Principal {draft.main.Count}/100  ·  Terrenos {draft.terrains.Count}/50  ·  {(Catalog.Validate(draft).Count == 0 ? "Lista válida para jogar" : "Rascunho em construção")}",small,gold);
            string[] tabs = { "Principais", "Terrenos", "Comandantes" };
            for (int t = 0; t < 3; t++) if (Button(278+t*184,191,176,33,tabs[t],deckTab == t ? new Color(.31f,.25f,.15f) : panel)) { deckTab = t; collectionScroll = Vector2.zero; }
            onlyIncluded = GUI.Toggle(new Rect(845,199,230,24),onlyIncluded," Somente no deck");
            search = GUI.TextField(new Rect(278,239,363,32),search,60);
            if (Button(651,239,140,32,elementFilter < 0 ? "Todos elementos" : Catalog.Elements[elementFilter],panel)) { elementFilter = (elementFilter+2)%8-1; collectionScroll = Vector2.zero; }
            if (Button(801,239,121,32,costFilter < 0 ? "Todo custo" : costFilter == 5 ? "Custo 5+" : "Custo "+costFilter,panel)) { costFilter = (costFilter+2)%7-1; collectionScroll = Vector2.zero; }
            if (Button(932,239,145,32,new[] { "Todos os tipos", "Criaturas", "Feitiços", "Truques", "Equipamentos", "Construções", "Veículos" }[kindFilter],panel)) { kindFilter = (kindFilter+1)%7; collectionScroll = Vector2.zero; }
            onlyNew = GUI.Toggle(new Rect(278,283,245,26),onlyNew," Apenas a nova expansão");
            Text(537,283,540,27,"Buscar: nome, texto de efeito ou tipo de carta",small,muted);
            var collection = FilterCollection(draft);
            collectionScroll = GUI.BeginScrollView(new Rect(278,319,799,405),collectionScroll,new Rect(0,0,773,Mathf.Ceil(collection.Count/3f)*259));
            for (int i = 0; i < collection.Count; i++)
            {
                var card = collection[i]; var r = new Rect(i%3*258,i/3*259,244,216);
                CardFace(r,card,inspectedId == card.Id);
                if (GUI.Button(r,GUIContent.none,GUIStyle.none)) inspectedId = card.Id;
                bool included = IsIncluded(draft,card);
                if (Button(r.x,r.y+221,r.width,30,card.Kind == CardKind.Commander ? included ? "Comandante escolhido" : "Escolher comandante" : included ? "− Retirar do deck" : "+ Incluir no deck",included ? new Color(.31f,.20f,.15f) : new Color(.17f,.31f,.25f))) { ToggleCard(draft,card); inspectedId = card.Id; }
            }
            if (collection.Count == 0) Text(16,20,720,65,"Nenhuma carta com estes filtros. Limpe a busca ou troque elemento, tipo e custo.",body,muted);
            GUI.EndScrollView();
            DrawDeckStats(draft); DrawCardInspector(draft);
            var errors = Catalog.Validate(draft);
            Text(278,851,799,40,errors.Count == 0 ? "Deck pronto. Salve a lista e use Equipar no Sol/Lua." : string.Join(" ",errors.Take(2)),small,errors.Count == 0 ? green : gold);
            if (Button(24,902,230,39,mainMenu ? "Voltar ao menu" : "Voltar à partida",panel)) deckEditor = false;
            bool draftValid = Catalog.Validate(draft,false).Count == 0;
            if (Button(278,902,218,39,"Salvar lista",new Color(.22f,.35f,.24f),draftValid))
            { if (string.IsNullOrWhiteSpace(draft.name)) draft.name = "Deck sem nome"; Persist(next => next.Upsert(draftId,draft),"Lista salva. Equipar usa esta versão; listas incompletas permanecem como rascunho."); }
            if (Button(506,902,180,39,"Exportar pacote",panel,draftValid)) { try { string path=DeckPackages.ExportFile(draft); GUIUtility.systemCopyBuffer = DeckPackages.Export(draft); deckNotice = "Pacote com cartas salvo e copiado: "+path; } catch (Exception e) { deckNotice = e.Message; } }
            if (Button(696,902,180,39,"Importar JSON",panel)) { importText = ""; deckDialog = 1; }
            if (Button(886,902,191,39,"Escolher modo",panel)) { deckEditor = false; mainMenu = true; OpenPlaySetup(); }
            Text(1100,904,316,40,$"{library.entries.Count} listas salvas\n* indica alterações não salvas",small,muted);
            Text(24,946,1390,18,deckNotice,small,ink);
            GUI.enabled = true; if (deckDialog != 0) DrawDeckDialog();
        }
        List<CardDefinition> FilterCollection(DeckList draft) => Catalog.All
            .Where(c => deckTab == 0 ? c.MainDeckEligible : deckTab == 1 ? c.Kind == CardKind.Terrain : c.Kind == CardKind.Commander)
            .Where(c => string.IsNullOrWhiteSpace(search) || (c.Name+" "+c.Text+" "+c.TypeName).IndexOf(search,StringComparison.OrdinalIgnoreCase) >= 0)
            .Where(c => elementFilter < 0 || (c.Kind == CardKind.Terrain ? c.ProductionColors.Contains(elementFilter) : c.Element == elementFilter))
            .Where(c => costFilter < 0 || (costFilter == 5 ? c.TotalCost >= 5 : c.TotalCost == costFilter))
            .Where(c => deckTab != 0 || kindFilter == 0 || c.Kind == (kindFilter == 1 ? CardKind.Creature : kindFilter == 2 ? CardKind.Spell : kindFilter == 3 ? CardKind.Trick : kindFilter == 4 ? CardKind.Equipment : kindFilter == 5 ? CardKind.Building : CardKind.Vehicle))
            .Where(c => !onlyIncluded || IsIncluded(draft,c)).Where(c => !onlyNew || c.Id.StartsWith("confluence-") || c.Id.StartsWith("kingdom-") || c.Id.StartsWith("custom-"))
            .OrderBy(c => c.Id.StartsWith("custom-") ? 0 : c.Id.StartsWith("confluence-") ? 1 : c.Id.StartsWith("kingdom-") ? 2 : c.Id.StartsWith("core-") ? 3 : 4).ThenBy(c => c.TotalCost).ThenBy(c => c.Name).ToList();
        void DrawLibrary()
        {
            Frame(new Rect(24,94,230,793),bronze);
            Text(39,107,200,28,"SLOTS DE DECK",body,gold);
            var ids = working.Keys.ToArray();
            libraryScroll = GUI.BeginScrollView(new Rect(34,142,210,330),libraryScroll,new Rect(0,0,187,ids.Length*71));
            for (int i = 0; i < ids.Length; i++)
            {
                string id = ids[i]; var r = new Rect(0,i*71,184,65); Fill(r,id == draftId ? new Color(.28f,.23f,.15f) : panel);
                string label = working[id].name ?? "Sem nome"; if (label.Length > 42) label = label.Substring(0,39)+"…";
                Text(8,r.y+5,169,39,"Slot "+(i+1)+" · "+label+(Dirty(id) ? " *" : ""),small,ink);
                Text(8,r.y+43,169,19,$"{working[id].main.Count} cartas · {working[id].terrains.Count} terrenos",tiny,muted);
                if (GUI.Button(r,GUIContent.none,GUIStyle.none)) { draftId = id; includedScroll = Vector2.zero; }
            }
            GUI.EndScrollView();
            if (Button(38,486,202,34,"Novo deck vazio",panel)) NewDraft(new DeckList { name = "Novo deck",commander = "commander-sol" });
            if (Button(38,528,202,34,"Duplicar este deck",panel)) { var copy = Draft.Copy(); copy.name = "Cópia de "+copy.name; NewDraft(copy); }
            if (Button(38,570,202,34,"Criar deck sugerido",panel)) NewDraft(Expansion.Starter(preset));
            if (Button(38,611,202,30,new[] { "Modelo: equilibrado", "Modelo: agressivo", "Modelo: controle" }[preset],panel)) preset = (preset+1)%3;
            if (Button(38,655,202,31,"Excluir este deck",new Color(.31f,.18f,.16f))) deckDialog = 2;
            bool equip = !Dirty(draftId) && Catalog.Validate(Draft).Count == 0;
            if (Button(38,720,202,35,"Equipar no Sol",new Color(.37f,.29f,.14f),equip)) Persist(next => next.Equip(draftId,0),"Versão salva equipada no Sol. Inicie uma nova partida para usar.");
            if (Button(38,763,202,35,"Equipar na Lua",new Color(.22f,.25f,.39f),equip)) Persist(next => next.Equip(draftId,1),"Versão salva equipada na Lua. Inicie uma nova partida para usar.");
            string solName = library.solDeck.name ?? "Sem nome", luaName = library.luaDeck.name ?? "Sem nome";
            if (solName.Length > 24) solName = solName.Substring(0,21)+"…"; if (luaName.Length > 24) luaName = luaName.Substring(0,21)+"…";
            Text(38,810,202,65,"Sol: "+solName+"\nLua: "+luaName,small,muted);
        }
        void DrawDeckStats(DeckList deck)
        {
            Frame(new Rect(278,737,799,105),bronze);
            var main = deck.main.Select(Catalog.Get).Where(c => c != null).ToArray();
            Text(290,744,470,20,"CURVA DE MANA · cartas por custo",small,gold);
            var bins = Enumerable.Range(0,8).Select(n => main.Count(c => Math.Min(7,c.TotalCost) == n)).ToArray();
            int max = Math.Max(1,bins.Max());
            for (int n = 0; n < 8; n++)
            {
                float x = 294+n*58, height = bins[n]*30f/max;
                Fill(new Rect(x,808-height,39,height),gold); Text(x,788-height,39,19,bins[n].ToString(),tiny,ink);
                Text(x,811,39,20,n == 7 ? "7+" : n.ToString(),tiny,muted);
            }
            Text(785,760,276,70,$"Criaturas: {main.Count(c => c.Kind == CardKind.Creature)}\nFeitiços: {main.Count(c => c.Kind == CardKind.Spell)}   Truques: {main.Count(c => c.Kind == CardKind.Trick)}\nCusto médio: {(main.Length == 0 ? 0 : main.Average(c => c.TotalCost)):0.0}",small,ink);
        }
        void DrawCardInspector(DeckList deck)
        {
            Text(1100,96,316,26,"CARTA EM DESTAQUE",body,gold);
            var inspected = Catalog.Get(inspectedId) ?? Catalog.Get("core-spark"); CardFace(new Rect(1100,129,316,421),inspected,true,true);
            Frame(new Rect(1100,567,316,276),bronze);
            Text(1111,579,292,26,"LISTA · "+new[] { "principal", "terrenos", "comandante" }[deckTab],small,gold);
            var ids = deckTab == 0 ? deck.main : deckTab == 1 ? deck.terrains : new List<string> { deck.commander };
            includedScroll = GUI.BeginScrollView(new Rect(1111,612,292,218),includedScroll,new Rect(0,0,268,ids.Count*32));
            for (int i = 0; i < ids.Count; i++)
            {
                var card = Catalog.Get(ids[i]); var r = new Rect(0,i*32,266,29);
                Text(4,r.y+3,260,24,$"{card.TotalCost} · {card.Name}",small,ink);
                if (GUI.Button(r,GUIContent.none,GUIStyle.none)) inspectedId = card.Id;
            }
            GUI.EndScrollView();
        }
        void DrawDeckDialog()
        {
            Fill(new Rect(0,0,1440,960),bg); Frame(new Rect(314,185,812,610),gold);
            if (deckDialog == 1)
            {
                Text(345,212,750,42,"IMPORTAR DECK",heading,gold);
                Text(345,262,750,45,"Cole um pacote com cartas ou um JSON de deck antigo. Novas cartas são instaladas; conflitos recebem novos IDs.",body,ink);
                float height = Math.Max(290,importText.Split('\n').Length*17);
                importScroll = GUI.BeginScrollView(new Rect(345,321,750,292),importScroll,new Rect(0,0,726,height));
                importText = GUI.TextArea(new Rect(0,0,726,height),importText); GUI.EndScrollView();
                importPath=GUI.TextField(new Rect(345,620,570,32),importPath);
                if(Button(925,620,170,32,"Ler arquivo",panel)) { try { importText=System.IO.File.ReadAllText(importPath); } catch(Exception e) { deckNotice=e.Message; } }
                if (Button(345,666,240,37,"Colar da área de transferência",panel)) importText = GUIUtility.systemCopyBuffer;
                if (Button(601,666,239,37,"Importar como novo deck",new Color(.19f,.34f,.24f)))
                { try { NewDraft(DeckPackages.Import(importText)); deckDialog = 0; } catch (Exception e) { deckNotice = "Importação recusada: "+e.Message; } }
                if (Button(856,666,239,37,"Cancelar",panel)) deckDialog = 0;
                Text(345,722,750,64,deckNotice,small,ink);
            }
            else
            {
                Text(345,238,750,75,"Excluir “"+Draft.name+"”?",heading,gold);
                Text(345,359,750,110,"A lista salva e seu rascunho serão removidos. A partida atual e as cópias já equipadas nos jogadores serão preservadas.",body,ink);
                if (Button(345,661,361,48,"Cancelar",panel)) deckDialog = 0;
                if (Button(725,661,370,48,"Excluir deck",new Color(.38f,.20f,.16f)))
                {
                    string id = draftId;
                    if (!library.entries.Any(e => e.id == id) || Persist(next => next.Remove(id),"Deck excluído. Cópias equipadas preservadas."))
                    { working.Remove(id); deckDialog = 0; if (working.Count == 0) NewDraft(new DeckList { name = "Novo deck",commander = "commander-sol" }); else draftId = working.Keys.First(); }
                }
            }
        }
    }
}
