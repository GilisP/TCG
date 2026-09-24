using System;
using System.Linq;
using UnityEngine;
using TCG.Foundation;
namespace TCG.Table
{
    public sealed partial class TableView
    {
        int equipmentToAttach=-1;
        static string TypeLabel(Definition c)=>c.IsCommander?"COMANDANTE":c.Kind==CardType.Enchantment?"ENCANTAMENTO":c.Kind==CardType.Artifact?"ARTEFATO":c.Kind==CardType.Creature?"CRIATURA":c.Kind==CardType.Construction?"CONSTRUÇÃO":c.Kind==CardType.Equipment?"EQUIPAMENTO":c.Kind==CardType.Instant?"TRUQUE":c.Kind==CardType.Terrain?"TERRENO":"FEITIÇO";
        static string RarityLabel(Definition c)=>c.Rarity=="prototype"?"PROTÓTIPO":c.Rarity=="common"?"COMUM":c.Rarity=="uncommon"?"INCOMUM":c.Rarity=="rare"?"RARA":c.Rarity=="legendary"?"LENDÁRIA":c.Rarity=="mythic"?"MÍTICA":c.Rarity;
        void DrawInspection()
        {
            Veil();var c=inspected;if(inspectionIdentity!=c.Id){inspectionIdentity=c.Id;inspectionStyle=CardStyle(c);inspectionFoil=CardFoil(c);}
            Fill(new Rect(215,65,1170,865),Panel);RenderCardFace(new Rect(250,118,390,585),c,inspectionStyle,true);
            Text(685,108,655,80,c.Name,heading,Gold);Text(685,197,655,45,TypeLabel(c)+" · "+string.Join(" / ",c.Subtypes),body,Ink);
            Text(685,246,655,55,"Custo "+c.TotalCost+" · "+string.Join(" / ",c.ColoredCost.Select((n,i)=>n>0?n+" "+new[]{"Sol","Lua","Água","Fogo","Ar","Terra","Incolor"}[i]:"").Where(x=>x!=""))+(c.Cost>0?" + "+c.Cost+" genéricos":""),body,Muted);
            detailScroll=GUI.BeginScrollView(new Rect(685,314,655,252),detailScroll,new Rect(0,0,620,Math.Max(230,body.CalcHeight(new GUIContent(c.Text),610)+20)));Text(0,0,610,Math.Max(230,body.CalcHeight(new GUIContent(c.Text),610)+20),c.Text,body,Ink);GUI.EndScrollView();
            if(c.Rule=="ruins"){Text(685,585,650,110,"Terreno de cenário: pode ser ocupado e substituído. Não pertence à coleção nem aos boosters.",body,Muted);if(Button(250,845,1090,52,"Fechar ficha",true,true))inspected=null;return;}
            Text(685,585,650,32,"ARTES E MOLDURAS",cardName,Gold);
            for(int n=0;n<CardStyles.All.Length;n++){string style=CardStyles.All[n];if(Button(685+n*220,631,208,47,CardStyles.Name(style),true,inspectionStyle==style))inspectionStyle=style;}
            bool owned=library.OwnsVariant(c.Id,inspectionStyle);Text(685,699,650,57,owned?"Variante na coleção. A identidade e as regras da carta são preservadas.":"Prévia de variante. Encontre-a nos boosters para equipar.",small,Muted);
            if(Button(685,766,315,49,"Usar na coleção",owned&&collectionStore.CanWrite,true))ApplyCardStyle(false);
            bool inDeck=draft!=null&&(draft.commander==c.Id||draft.main.Contains(c.Id)||draft.terrains.Contains(c.Id));if(Button(1014,766,325,49,"Usar neste deck",owned&&inDeck&&collectionStore.CanWrite))ApplyCardStyle(true);
            inspectionFoil=CheckBox(250,717,385,"Foil · brilho ao entrar (teste)",inspectionFoil);Text(250,757,390,29,c.Id+" · "+RarityLabel(c)+(c.Playable?"":"\n"+c.UnavailableReason),small,Muted);
            if(!library.Owns(c.Id)&&Button(250,787,390,38,"Adicionar carta grátis (teste)",c.Playable&&collectionStore.CanWrite))CollectCard(c.Id);
            if(Button(250,845,1090,52,"Fechar ficha",true,true))inspected=null;
        }
        void DrawChoice()
        {
            var choice=match.Choice;if(choice==null)return;
            Veil();Text(250,95,1100,55,"ESCOLHA · "+match.Seats[choice.Owner].Name,heading,Gold);
            float promptHeight=Math.Max(80,body.CalcHeight(new GUIContent(choice.Prompt),1040)+20);
            choiceScroll=GUI.BeginScrollView(new Rect(250,165,1100,665),choiceScroll,new Rect(0,0,1060,Math.Max(645,promptHeight+choice.Options.Count*62)));
            Text(0,0,1040,promptHeight,choice.Prompt,body,Ink);
            foreach(var pair in choice.Options.Select((option,index)=>(option,index)).ToArray())
                if(Button(0,promptHeight+pair.index*62,1040,52,pair.option.Label)){Submit(ActionKind.Choose,pair.option.Key);choiceScroll=Vector2.zero;break;}
            GUI.EndScrollView();Text(250,860,1100,35,"A resolução aguarda sua escolha. Outros jogadores não podem agir agora.",small,Muted);
        }
        void BeginEquip(int id){equipmentToAttach=id;}
        void DrawEquipmentChoice()
        {
            if(equipmentToAttach<0)return;Veil();Text(280,180,1040,60,"Escolha quem recebe o equipamento",heading,Gold);
            var pieces=match.Board.SelectMany(c=>c.Pieces).Where(p=>match.CanEquip(equipmentToAttach,p.Id)).ToArray();
            choiceScroll=GUI.BeginScrollView(new Rect(280,280,1040,490),choiceScroll,new Rect(0,0,1000,Math.Max(460,pieces.Length*60)));
            for(int n=0;n<pieces.Length;n++)if(Button(0,n*60,980,50,pieces[n].Card.Name)){Submit(ActionKind.Equip,pieces[n].Id,equipmentToAttach);equipmentToAttach=-1;break;}
            GUI.EndScrollView();if(Button(280,820,1040,50,"Cancelar"))equipmentToAttach=-1;
        }
    }
}




