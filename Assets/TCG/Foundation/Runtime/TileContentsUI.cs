using System;
using TCG.Foundation;
using UnityEngine;

namespace TCG.Table
{
    public sealed partial class TableView
    {
        bool tileContentsOpen;
        Vector2 tileContentsScroll;

        void SelectTileForInspection(int index)
        {
            if(match==null||index<0||index>=match.Board.Count)return;
            ResetDrag(); selectedCell=index; tileContentsOpen=true;
            tileContentsScroll=Vector2.zero;
            notice="Consulta do terreno. Ficha abre a carta; Selecionar prepara a peça para ações.";
        }

        string TileSeatName(int owner)=>owner>=0&&owner<match.Seats.Count?match.Seats[owner].Name:"sem dono";

        void InspectTileCard(Definition card)
        {
            inspected=card; detailScroll=Vector2.zero;
        }

        void DrawTileContentsWindow()
        {
            if(!tileContentsOpen)return;
            Veil();Fill(new Rect(380,125,840,750),Panel);
            Text(410,150,620,42,"CARTAS NO TERRENO",heading,Gold);
            if(Button(1065,150,125,36,"Fechar")){tileContentsOpen=false;return;}
            if(Event.current.type==EventType.KeyDown&&Event.current.keyCode==KeyCode.Escape){tileContentsOpen=false;Event.current.Use();return;}
            DrawTileContents();
        }

        void DrawTileContents()
        {
            if(selectedCell<0||selectedCell>=match.Board.Count)return;
            var cell=match.Board[selectedCell];
            Text(410,205,780,32,cell.Terrain?.Name??"Terreno vazio",heading,Ink);
            Text(410,246,780,28,"CASA "+selectedCell%11+" × "+selectedCell/11+" · "+cell.Pieces.Count+" ocupante(s) · Reino: "+TileSeatName(cell.Owner),small,Gold);
            Text(410,280,780,32,"Ficha abre a carta. Selecionar prepara a peça para ações na mesa.",small,Muted);
            int rows=cell.Pieces.Count+(cell.Terrain!=null?1:0);
            tileContentsScroll=GUI.BeginScrollView(new Rect(410,325,780,510),tileContentsScroll,new Rect(0,0,750,Math.Max(490,rows*170)));
            int row=0;
            if(cell.Terrain!=null)
            {
                DrawTileCardRow(row++,cell.Terrain,"Terreno · dono: "+TileSeatName(cell.TerrainOwner),-1);
            }
            foreach(var piece in cell.Pieces)
            {
                string state="Dono: "+TileSeatName(piece.OriginalOwner)+" · Controlador: "+TileSeatName(piece.Owner)+"\n"+piece.Attack+"/"+piece.Health+" · PA "+piece.Actions+" · Movimento "+piece.Movement;
                if(piece.CarrierId>=0)state+="\nEmbarcada em "+(match.Find(piece.CarrierId)?.Card.Name??"veículo");
                else if(piece.AttachedTo>=0)state+="\nAnexada a "+(match.Find(piece.AttachedTo)?.Card.Name??"peça");
                DrawTileCardRow(row++,piece.Card,state,piece.Id);
            }
            if(rows==0)Text(5,12,310,80,"Esta casa não contém cartas.",body,Muted);
            GUI.EndScrollView();
        }

        void DrawTileCardRow(int row,Definition card,string state,int piece)
        {
            float y=row*170;
            RenderCardFace(new Rect(0,y+3,82,123),card,CardStyle(card),piece>=0&&selectedUnit==piece);
            if(GUI.Button(new Rect(0,y+3,82,123),GUIContent.none,GUIStyle.none))InspectTileCard(card);
            Text(100,y,620,35,card.Name,heading,Ink);
            Text(100,y+38,620,84,state,body,Muted);
            if(Button(100,y+127,150,32,"Ficha"))InspectTileCard(card);
            if(piece>=0&&Button(265,y+127,180,32,"Selecionar",NetworkTurn,selectedUnit==piece))
            {selectedUnit=piece;tileContentsOpen=false;}
        }
    }
}
