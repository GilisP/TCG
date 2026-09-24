using System.Linq;
using TCG.Foundation;
using UnityEngine;

namespace TCG.Table
{
    public sealed class PieceHit:MonoBehaviour { public int Piece; }
    public sealed partial class TableView
    {
        bool pointerHeld,draggingPiece;
        int pressedCell=-1,pressedPiece=-1,dragPiece=-1,dragTile=-1;
        Vector2 pressPoint;
        static readonly Rect BoardArea=new Rect(0,160,1180,600);
        bool BoardHit(Vector2 point,out int cell,out int piece)
        {
            cell=piece=-1;
            var ray=world.View.ScreenPointToRay(new Vector3(point.x*scale,Screen.height-point.y*scale,0));
            if(!Physics.Raycast(ray,out var hit,70))return false;
            if(hit.collider.TryGetComponent<PieceHit>(out var model)){piece=model.Piece;cell=match.Position(piece);return cell>=0;}
            if(hit.collider.TryGetComponent<CellHit>(out var tile)){cell=tile.Index;return true;}
            return false;
        }
        void ResetDrag()
        {pointerHeld=draggingPiece=false;pressedCell=pressedPiece=dragPiece=dragTile=-1;world.PreviewMovementDrag(-1,-1);}
        void BoardInput()
        {
            var e=Event.current;
            if(HandleAvatarReaction(e))return;
            if(e.type==EventType.KeyDown&&e.keyCode==KeyCode.Escape){ResetDrag();e.Use();return;}
            if(pointerHeld&&e.type==EventType.MouseUp&&e.button==0)
            {
                bool wasDrag=draggingPiece;int id=dragPiece,origin=pressedCell,clickedPiece=pressedPiece;
                bool hit=BoardArea.Contains(e.mousePosition)&&BoardHit(e.mousePosition,out dragTile,out _);int destination=dragTile;
                ResetDrag();
                if(wasDrag)
                {
                    if(hit&&destination!=origin)DropMovement(id,destination);
                    else notice="Arraste cancelado; posição mantida.";
                }
                else if(hit&&destination==origin)ClickBoard(origin,clickedPiece);
                e.Use();return;
            }
            if(pointerHeld&&e.type==EventType.MouseDrag&&e.button==0)
            {
                if(dragPiece>=0&&Vector2.Distance(pressPoint,e.mousePosition)>8){draggingPiece=true;selectedUnit=dragPiece;selectedCell=pressedCell;}
                if(draggingPiece)
                {
                    dragTile=-1;if(BoardArea.Contains(e.mousePosition))BoardHit(e.mousePosition,out dragTile,out _);
                    world.PreviewMovementDrag(dragPiece,dragTile);e.Use();
                }
                return;
            }
            if(!BoardArea.Contains(e.mousePosition))return;
            if(e.type==EventType.ScrollWheel){world.Distance=Mathf.Clamp(world.Distance+e.delta.y*.4f,10,24);e.Use();}
            else if(e.type==EventType.MouseDrag&&e.button==1){ResetDrag();world.Yaw+=e.delta.x*.35f;world.Elevation=Mathf.Clamp(world.Elevation-e.delta.y*.25f,30,80);e.Use();}
            else if(e.type==EventType.MouseDown&&e.button==0)
            {
                var ray=world.View.ScreenPointToRay(new Vector3(e.mousePosition.x*scale,Screen.height-e.mousePosition.y*scale,0));
                if(Physics.Raycast(ray,out var hit,70)&&hit.collider.TryGetComponent<PileHit>(out var pile))
                {
                    if(pile.Player==match.Active&&match.Controller==match.Active&&(match.Phase==Stage.Draw||match.Phase==Stage.Terrain)){selectedPile=pile.Pile;selectedCard=null;}
                    e.Use();return;
                }
                if(BoardHit(e.mousePosition,out pressedCell,out pressedPiece))
                {
                    pointerHeld=true;pressPoint=e.mousePosition;
                    var candidate=pressedPiece>=0?match.Find(pressedPiece):match.Board[pressedCell].Pieces.FirstOrDefault(p=>p.Id==selectedUnit)??match.Board[pressedCell].Pieces.FirstOrDefault(p=>match.CanOrderMovement(p.Id));
                    dragPiece=!selectedCommander&&selectedCard==null&&candidate!=null&&match.CanOrderMovement(candidate.Id)?candidate.Id:-1;
                }
                e.Use();
            }
        }
        bool DropMovement(int id,int destination)
        {
            if(!Submit(ActionKind.PlanMove,destination,id))return false;
            selectedUnit=match.Find(id)!=null?id:-1;selectedCell=match.Position(id);
            notice=match.OrderFor(id)!=null?"Destino programado. A criatura avança na fase principal e espera se o caminho não estiver andável.":"Movimento concluído.";
            return true;
        }
        void ClickBoard(int index,int hitPiece)
        {
            if(selectedCommander){Submit(ActionKind.SummonCommander,index);return;}
            var target=match.Board[index];
            // A creature remains selectable before terrain placement, so its route can be cancelled.
            if(selectedCard==null&&hitPiece>=0&&match.Find(hitPiece)?.Owner==match.Controller)
            {selectedCell=index;selectedUnit=hitPiece;return;}
            if(match.Phase==Stage.Terrain&&match.CanPlace(index))Submit(ActionKind.Place,index);
            else if(selectedCard!=null)
            {
                var def=catalog.Get(selectedCard);var enemy=target.Pieces.FirstOrDefault(p=>p.Id==hitPiece&&match.Enemies(match.Controller,p.Owner))??target.Pieces.FirstOrDefault(p=>p.Id==selectedUnit&&match.Enemies(match.Controller,p.Owner))??target.Pieces.FirstOrDefault(p=>match.Enemies(match.Controller,p.Owner));
                if(def.Permanent)Submit(ActionKind.Play,index,card:selectedCard);
                else if(hitPiece>=0||target.Pieces.Count(p=>match.Enemies(match.Controller,p.Owner))<=1)Submit(ActionKind.Play,index,enemy?.Id??-1,selectedCard);
                else{selectedCell=index;notice="Selecione a criatura desejada na lista de alvos.";}
            }
            else if(selectedUnit>=0&&match.CanAttack(selectedUnit,index))
            {
                var ids=attackGroup?match.Board.SelectMany(c=>c.Pieces).Where(p=>match.CanAttack(p.Id,index)).Select(p=>p.Id).ToArray():new[]{selectedUnit};
                Submit(ActionKind.Attack,index,units:ids);
            }
            else if(selectedUnit>=0&&match.CanMove(selectedUnit,index))Submit(ActionKind.Move,index,selectedUnit);
            else{selectedCell=index;selectedUnit=target.Pieces.FirstOrDefault(p=>p.Owner==match.Controller)?.Id??-1;}
        }
        void DrawMovementOrders()
        {
            if(handoff)return;var orders=match.OrdersFor(match.Controller);if(orders.Count==0)return;
            var selected=match.OrderFor(selectedUnit);if(selected?.Owner!=match.Controller)selected=null;
            string status=selected==null?orders.Count+" rota(s) programada(s)":
                "Destino "+selected.Destination%11+","+selected.Destination/11+" · "+
                (selected.State==MovementOrderState.WaitingForTerrain?"aguardando terreno":selected.State==MovementOrderState.WaitingForPath?"aguardando caminho":selected.State==MovementOrderState.WaitingForMovement?"próximo turno":"programado");
            Text(1215,790,350,36,status,small,Gold);
            if(Button(1215,836,350,38,selected==null?"Cancelar todas as rotas":"Cancelar esta rota"))Submit(ActionKind.CancelMove,unit:selected?.Piece??-1);
        }
    }
}
