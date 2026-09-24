using System;
using UnityEngine;

namespace TCG
{
    public sealed partial class GameView
    {
        readonly Color parchment = new Color(.88f,.82f,.67f), parchmentInk = new Color(.19f,.15f,.10f), bronze = new Color(.53f,.39f,.20f);
        void Frame(Rect r,Color accent,bool chosen = false)
        {
            Fill(r,chosen ? accent : bronze); Fill(new Rect(r.x+2,r.y+2,r.width-4,r.height-4),bg);
            Fill(new Rect(r.x+5,r.y+5,r.width-10,r.height-10),Color.Lerp(panel,accent,.07f));
            for (int n = 0; n < 4; n++)
            {
                float x = n%2 == 0 ? r.x+5 : r.xMax-16, y = n < 2 ? r.y+5 : r.yMax-16;
                Fill(new Rect(x,y,11,2),accent); Fill(new Rect(x,y,2,11),accent);
            }
        }
        void CardFace(Rect r,CardDefinition card,bool chosen = false,bool detailed = false)
        {
            Color accent = BoardArt.Palette[card.Element]; Frame(r,accent,chosen);
            float pad = 9, titleHeight = detailed ? 48 : 33, imageY = r.y+pad+titleHeight;
            float pictureHeight = detailed ? 140 : 92;
            Text(r.x+pad,r.y+7,r.width-48,titleHeight,card.Name,detailed ? body : small,ink);
            Fill(new Rect(r.xMax-37,r.y+10,25,25),accent); Text(r.xMax-37,r.y+10,25,25,card.Kind == CardKind.Terrain ? "T" : card.TotalCost.ToString(),center,parchmentInk);
            var picture = new Rect(r.x+pad,imageY,r.width-pad*2,pictureHeight);
            GUI.DrawTexture(picture,art.Landscape(card),ScaleMode.ScaleAndCrop);
            float iconSize = pictureHeight*.7f; if(GeneratedArt.Get(card.Illustration)==null) Icon(new Rect(picture.center.x-iconSize/2,picture.center.y-iconSize/2,iconSize,iconSize),card,new Color(.94f,.87f,.66f,.94f));
            float textY = imageY+pictureHeight+4;
            Text(r.x+pad,textY,r.width-pad*2,22,card.TypeName+" · "+card.ColorLabel,small,accent);
            if(card.Kind == CardKind.Terrain) for(int e=0;e<card.ProductionColors.Length;e++) Fill(new Rect(r.x+pad+e*(r.width-pad*2)/card.ProductionColors.Length,textY+21,(r.width-pad*2)/card.ProductionColors.Length,2),BoardArt.Palette[card.ProductionColors[e]]);
            float rulesY = textY+24, rulesH = Math.Max(28,r.yMax-rulesY-28);
            Fill(new Rect(r.x+pad,rulesY,r.width-pad*2,rulesH),parchment);
            string description = card.Text+(detailed ? "\nCusto: "+card.CostLabel : "");
            string rules = !detailed && card.Text.Length > 50 ? card.Text.Substring(0,47)+"…" : description;
            Text(r.x+pad+6,rulesY+4,r.width-pad*2-12,rulesH-7,rules,small,parchmentInk);
            string stats = card.IsUnit ? $"ATQ {card.Attack}   VIDA {card.Life}   MOV {card.Movement}   ALC {card.Range}" : card.Kind == CardKind.Building ? "VIDA "+card.BuildingLife+" · HABITAÇÃO "+card.HousingUse : card.Kind == CardKind.Terrain ? "HABITAÇÃO "+card.Housing+" · MANA "+(1+card.ExtraMana) : card.Kind == CardKind.Trick ? "PODE RESPONDER NA PILHA" : "FASE PRINCIPAL";
            Text(r.x+pad,r.yMax-24,r.width-pad*2,20,stats,detailed ? small : tiny,accent);
        }
        void SmallCard(Rect r,CardDefinition card,bool chosen)
        {
            Frame(r,BoardArt.Palette[card.Element],chosen);
            GUI.DrawTexture(new Rect(r.x+6,r.y+6,53,r.height-12),art.Landscape(card),ScaleMode.ScaleAndCrop);
            if(GeneratedArt.Get(card.Illustration)==null) Icon(new Rect(r.x+12,r.y+13,40,40),card,BoardArt.Palette[card.Element]);
            Text(r.x+65,r.y+7,r.width-101,32,card.Name,small,ink);
            Text(r.x+65,r.y+42,r.width-75,23,card.TypeName+(card.IsUnit ? $" · {card.Attack}/{card.Life}" : ""),small,gold);
            Fill(new Rect(r.xMax-31,r.y+9,22,22),BoardArt.Palette[card.Element]); Text(r.xMax-31,r.y+9,22,22,card.Kind == CardKind.Terrain ? "T" : card.TotalCost.ToString(),center,parchmentInk);
        }
    }
}
