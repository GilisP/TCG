using System;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace TCG.Editor
{
    public sealed class CardWorkshop : EditorWindow
    {
        CardDefinition draft;
        Vector2 scroll, listScroll;
        string search = "", status = "", selectedId;
        bool dirty, editAutomatic;
        int recipe;
        static readonly string[] kinds = {"Criatura","Feitiço","Truque","Terreno","Comandante","Ficha","Equipamento","Construção","Veículo"};
        static readonly string[] operations = {"Nenhum","Dano","Cura","Comprar cartas","Anular","Devolver à mão","Exilar","Destruir","Bônus de ataque","Escudo","Congelar","Gerar mana","Criar ficha","Descartar","Recuperar do cemitério","Ganhar popularidade","Ação e movimento extra","Enfraquecer","Limpar estados"};
        static readonly string[] targets = {"Seu jogador","Criatura inimiga","Criatura aliada","Qualquer criatura","Fonte / ocupante","Todas as aliadas","Casa sua vazia","Ação na pilha","Todos os inimigos","Jogador inimigo"};
        static readonly string[] durations = {"Instantâneo","Até o final do turno","Permanente","Até o final do próximo turno do controlador"};
        [MenuItem("TCG/Oficina de cartas e efeitos")]
        public static void Open() { var w=GetWindow<CardWorkshop>("Oficina de cartas"); w.minSize=new Vector2(980,800); w.Show(); }
        void OnEnable() { CustomCardStore.LoadResources(); if (draft == null) NewCard(); }
        bool LeaveDraft() => !dirty || EditorUtility.DisplayDialog("Rascunho não salvo","Descartar as alterações deste rascunho?","Descartar","Continuar editando");
        void NewCard()
        {
            draft=new CardDefinition {Id="custom-"+Guid.NewGuid().ToString("N"),Name="Nova criatura",Text="",Kind=CardKind.Creature,Attack=1,Life=2,Art="0",Effects=Array.Empty<EffectStep>()};
            selectedId=null; dirty=false; status="Crie uma carta, configure seus efeitos e salve no catálogo.";
        }
        void OnGUI()
        {
            if (draft == null) NewCard();
            EditorGUI.DrawRect(new Rect(0,0,position.width,48),new Color(.12f,.16f,.18f));
            var title=new GUIStyle(EditorStyles.boldLabel){fontSize=22,normal={textColor=new Color(.89f,.73f,.42f)}};
            GUI.Label(new Rect(20,10,700,32),"OFICINA  /  CARTAS DOS REINOS",title);
            GUILayout.Space(56);
            using(new EditorGUILayout.HorizontalScope())
            {
                using(new EditorGUILayout.VerticalScope(GUILayout.Width(230)))
                {
                    if (GUILayout.Button("+ Nova carta",GUILayout.Height(30)) && LeaveDraft()) NewCard();
                    search=EditorGUILayout.TextField("Buscar",search);
                    listScroll=EditorGUILayout.BeginScrollView(listScroll);
                    foreach(var card in Catalog.All.Where(c=>string.IsNullOrEmpty(search)||c.Name.IndexOf(search,StringComparison.OrdinalIgnoreCase)>=0).OrderBy(c=>c.Name).ToArray())
                        if(GUILayout.Button((card.Id.StartsWith("custom-") ? "✦ " : "")+card.Name,EditorStyles.miniButton,GUILayout.Height(28)) && LeaveDraft())
                        { draft=CardAuthoring.Copy(card); selectedId=card.Id; dirty=false; status=card.Id.StartsWith("custom-") ? "Editando carta personalizada." : "Modelo original: salvar criará uma nova carta personalizada."; }
                    EditorGUILayout.EndScrollView();
                }
                using(new EditorGUILayout.VerticalScope(GUILayout.MinWidth(420)))
                {
                    scroll=EditorGUILayout.BeginScrollView(scroll);
                    EditorGUI.BeginChangeCheck();
                    draft.Name=EditorGUILayout.TextField("Nome",draft.Name);
                    draft.Kind=(CardKind)EditorGUILayout.Popup("Tipo",(int)draft.Kind,kinds);
                    draft.Element=EditorGUILayout.Popup("Elemento visual / geração",draft.Element,Catalog.Elements);
                    var artOptions=new[]{"Sem ilustração"}.Concat(GeneratedArt.Keys).ToArray();
                    int illustration=Array.IndexOf(GeneratedArt.Keys,draft.Illustration)+1;
                    illustration=EditorGUILayout.Popup("Ilustração",illustration,artOptions);
                    draft.Illustration=illustration==0 ? null : GeneratedArt.Keys[illustration-1];
                    EditorGUILayout.LabelField("Descrição apresentada na carta");
                    draft.Text=EditorGUILayout.TextArea(draft.Text,GUILayout.Height(65));
                    EditorGUILayout.LabelField("Custo para lançar",EditorStyles.boldLabel);
                    draft.Cost=EditorGUILayout.IntSlider("Mana genérica",draft.Cost,0,20);
                    for(int e=0;e<6;e++) draft.ColoredCost[e]=EditorGUILayout.IntSlider(Catalog.Elements[e],draft.ColoredCost[e],0,20);
                    EditorGUILayout.HelpBox("Genérica aceita qualquer elemento. Os custos coloridos exigem exatamente o elemento indicado. Terrenos devem ter custo zero.",MessageType.Info);
                    if(draft.IsUnit)
                    {
                        draft.Attack=EditorGUILayout.IntSlider("Ataque",draft.Attack,0,30);
                        draft.Life=EditorGUILayout.IntSlider("Vida",draft.Life,1,50);
                        draft.Movement=EditorGUILayout.IntSlider("Movimento",draft.Movement,0,10);
                        draft.Range=EditorGUILayout.IntSlider("Alcance",draft.Range,1,10);
                        draft.Trait=(Trait)EditorGUILayout.Popup("Característica",(int)draft.Trait,new[]{"Nenhuma","Armadura 1","Drenar"});
                    }
                    if(draft.Kind==CardKind.Vehicle)
                    {
                        draft.Seats=EditorGUILayout.IntSlider("Assentos",draft.Seats,1,6);
                        draft.CrewRequired=EditorGUILayout.IntSlider("Tripulação mínima",draft.CrewRequired,1,draft.Seats);
                    }
                    if(draft.Kind==CardKind.Equipment)
                    {
                        draft.EquipmentAttack=EditorGUILayout.IntSlider("Bônus de ataque",draft.EquipmentAttack,0,10);
                        draft.EquipmentArmor=EditorGUILayout.IntSlider("Bônus de armadura",draft.EquipmentArmor,0,5);
                        draft.EquipmentMovement=EditorGUILayout.IntSlider("Bônus de movimento",draft.EquipmentMovement,0,5);
                    }
                    if(draft.Kind==CardKind.Terrain)
                    {
                        EditorGUILayout.LabelField("Cores que o terreno pode gerar (uma por turno)",EditorStyles.boldLabel);
                        var manaColors=draft.ProductionColors.ToList();
                        for(int e=0;e<7;e++)
                        {bool selected=EditorGUILayout.Toggle(Catalog.Elements[e],manaColors.Contains(e));if(selected&&!manaColors.Contains(e))manaColors.Add(e);else if(!selected)manaColors.Remove(e);}
                        draft.ManaColors=manaColors.ToArray();
                        draft.Housing=EditorGUILayout.IntSlider("Capacidade de habitação",draft.Housing,0,6);
                        draft.ExtraMana=EditorGUILayout.IntSlider("Mana extra no início",draft.ExtraMana,0,5);
                    }
                    if(draft.Kind==CardKind.Building)
                    {
                        draft.BuildingLife=EditorGUILayout.IntSlider("Vida da construção",draft.BuildingLife,1,50);
                        draft.HousingUse=EditorGUILayout.IntSlider("Habitação ocupada",draft.HousingUse,1,6);
                        draft.Autonomous=EditorGUILayout.Toggle("Funciona sem habitante",draft.Autonomous);
                    }
                    if(draft.Kind==CardKind.Terrain || draft.Kind==CardKind.Building)
                        draft.ActivationCost=EditorGUILayout.IntSlider("Ativação: mana genérica",draft.ActivationCost,0,20);
                    EditorGUILayout.Space(); EditorGUILayout.LabelField("Efeitos em ordem de resolução",EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox(draft.IsUnit ? "Efeitos ao entrar no campo." : draft.Kind==CardKind.Terrain || draft.Kind==CardKind.Building ? "Habilidade ativada uma vez por turno na aba Domínios." : draft.Kind==CardKind.Equipment ? "Equipamentos usam os bônus acima. Não adicione efeitos." : "Efeitos ao resolver a carta na pilha.",MessageType.None);
                    if(draft.Kind==CardKind.Terrain)
                    {
                        draft.AutomaticTrigger=(TerrainTrigger)EditorGUILayout.Popup("Gatilho automático",(int)draft.AutomaticTrigger,new[]{"Nenhum","Início do seu turno","Entrada de unidade sua"});
                        editAutomatic=EditorGUILayout.Toggle("Editar efeitos automáticos",editAutomatic);
                        EditorGUILayout.LabelField(editAutomatic ? "Lista AUTOMÁTICA (sem custo)" : "Lista ATIVADA (custo de ativação)",EditorStyles.boldLabel);
                    }
                    else editAutomatic=false;
                    DrawSteps();
                    if(EditorGUI.EndChangeCheck()) dirty=true;
                    EditorGUILayout.EndScrollView();
                }
                using(new EditorGUILayout.VerticalScope(GUILayout.Width(260)))
                {
                    DrawPreview();
                    var errors=CardAuthoring.Validate(draft);
                    if(errors.Count>0) EditorGUILayout.HelpBox(string.Join("\n",errors),MessageType.Warning);
                    using(new EditorGUI.DisabledScope(errors.Count>0))
                        if(GUILayout.Button("Salvar carta no catálogo",GUILayout.Height(38))) Save(false);
                    if(GUILayout.Button("Salvar como nova cópia",GUILayout.Height(30)) && errors.Count==0) Save(true);
                    EditorGUILayout.HelpBox(status,MessageType.Info);
                    EditorGUILayout.HelpBox("No jogo: Arsenal → adicione a carta → salve e equipe o deck → inicie uma nova partida. Partidas em andamento mantêm as definições antigas.",MessageType.None);
                }
            }
        }
        void DrawSteps()
        {
            var steps=(editAutomatic ? draft.AutomaticEffects : draft.Effects).ToList(); int remove=-1, up=-1;
            recipe=EditorGUILayout.Popup("Modelo pronto",recipe,EffectRecipes.Names);
            if(GUILayout.Button("+ Adicionar combinação pronta"))
            {
                var added=EffectRecipes.Create(recipe);
                if(steps.Count+added.Length<=8){steps.AddRange(added);dirty=true;}else status="A combinação ultrapassa o limite de 8 efeitos.";
            }
            for(int i=0;i<steps.Count;i++)
            {
                var s=steps[i];
                using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using(new EditorGUILayout.HorizontalScope())
                    { EditorGUILayout.LabelField("Efeito "+(i+1),EditorStyles.boldLabel); if(i>0 && GUILayout.Button("↑",GUILayout.Width(28))) up=i; if(GUILayout.Button("Remover",GUILayout.Width(75))) remove=i; }
                    s.Operation=(Effect)EditorGUILayout.Popup("Operação",(int)s.Operation,operations);
                    var allowed=CardAuthoring.AllowedTargets(s.Operation);
                    int at=Math.Max(0,Array.IndexOf(allowed,s.Target));
                    s.Target=allowed[EditorGUILayout.Popup("Alvo",at,allowed.Select(t=>targets[(int)t]).ToArray())];
                    var ds=CardAuthoring.AllowedDurations(s.Operation);
                    s.Duration=ds[EditorGUILayout.Popup("Duração",Math.Max(0,Array.IndexOf(ds,s.Duration)),ds.Select(d=>durations[(int)d]).ToArray())];
                    s.Value=EditorGUILayout.IntSlider("Quantidade",s.Value,0,30);
                    if(s.Operation==Effect.GainMana) s.Element=EditorGUILayout.Popup("Mana gerada",s.Element,Catalog.Elements);
                    if(s.Operation==Effect.SummonToken)
                    {
                        var tokens=Catalog.All.Where(c=>c.Kind==CardKind.Token).ToArray();
                        if(tokens.Length>0) s.TokenId=tokens[EditorGUILayout.Popup("Ficha",Math.Max(0,Array.FindIndex(tokens,c=>c.Id==s.TokenId)),tokens.Select(c=>c.Name).ToArray())].Id;
                    }
                }
            }
            if(remove>=0) {steps.RemoveAt(remove);dirty=true;}
            else if(up>=0) {var s=steps[up];steps[up]=steps[up-1];steps[up-1]=s;dirty=true;}
            if(steps.Count<8 && GUILayout.Button("+ Adicionar efeito")) {steps.Add(new EffectStep(Effect.Heal,EffectTarget.Owner,2));dirty=true;}
            if(editAutomatic)draft.AutomaticEffects=steps.ToArray();else draft.Effects=steps.ToArray();
        }
        void DrawPreview()
        {
            var r=GUILayoutUtility.GetRect(250,450);
            var gold=new Color(.68f,.50f,.24f); EditorGUI.DrawRect(r,gold);
            EditorGUI.DrawRect(new Rect(r.x+3,r.y+3,r.width-6,r.height-6),new Color(.16f,.19f,.19f));
            for(int i=0;i<4;i++) {float x=i%2==0?r.x+7:r.xMax-19,y=i<2?r.y+7:r.yMax-19;EditorGUI.DrawRect(new Rect(x,y,12,2),gold);EditorGUI.DrawRect(new Rect(x,y,2,12),gold);}
            var text=new GUIStyle(EditorStyles.wordWrappedLabel){normal={textColor=new Color(.94f,.87f,.71f)}};
            var name=new GUIStyle(text){fontSize=18,fontStyle=FontStyle.Bold};
            GUI.Label(new Rect(r.x+14,r.y+14,r.width-28,48),draft.Name,name);
            GUI.Label(new Rect(r.x+14,r.y+65,r.width-28,42),draft.TypeName+" · "+Catalog.Elements[draft.Element]+"\n"+draft.CostLabel,text);
            var art=GeneratedArt.Get(draft.Illustration);
            if(art!=null)GUI.DrawTexture(new Rect(r.x+12,r.y+109,r.width-24,119),art,ScaleMode.ScaleAndCrop);
            EditorGUI.DrawRect(new Rect(r.x+12,r.y+237,r.width-24,142),new Color(.87f,.81f,.65f));
            GUI.Label(new Rect(r.x+20,r.y+244,r.width-40,128),draft.Text,EditorStyles.wordWrappedLabel);
            GUI.Label(new Rect(r.x+14,r.y+389,r.width-28,56),draft.IsUnit ? $"Ataque {draft.Attack} · Vida {draft.Life}\nMovimento {draft.Movement} · Alcance {draft.Range}" : draft.Kind==CardKind.Equipment ? $"+{draft.EquipmentAttack} ataque · +{draft.EquipmentArmor} armadura\n+{draft.EquipmentMovement} movimento" : draft.Kind==CardKind.Terrain ? $"Habitação {draft.Housing} · Gera {1+draft.ExtraMana}\n{draft.Effects.Length} efeito(s)" : $"{draft.Effects.Length} efeito(s)",text);
        }
        void Save(bool asCopy)
        {
            try
            {
                var copy=CardAuthoring.Copy(draft);
                if(asCopy || !copy.Id.StartsWith("custom-")) copy.Id="custom-"+Guid.NewGuid().ToString("N");
                if(selectedId!=null && Catalog.Get(selectedId)?.Kind!=copy.Kind && copy.Id==selectedId)
                    throw new InvalidOperationException("Para mudar o tipo de uma carta salva, use Salvar como nova cópia. Assim os decks existentes continuam válidos.");
                bool imported=DeckPackages.UpdateInstalled(copy);
                if(!imported)CustomCardStore.Save(Path.Combine(Application.dataPath,"TCG/Resources/TCGCards"),copy);
                Catalog.RegisterCustom(copy); AssetDatabase.Refresh(); draft=CardAuthoring.Copy(copy); selectedId=copy.Id;dirty=false;
                status=imported ? "Carta importada atualizada na biblioteca local. Exporte um pacote para compartilhar." : "Salva em Assets/TCG/Resources/TCGCards. Disponível no Arsenal e incluída nas builds.";
            }
            catch(Exception e) {status="Não foi possível salvar: "+e.Message;}
        }
    }
}
