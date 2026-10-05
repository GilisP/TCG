using System.Linq;
using UnityEngine;
using TCG.Foundation;
namespace TCG.Table
{
    public sealed partial class TableView
    {
        bool selectedCommander, commanderHover;
        static readonly Rect CommanderPanel = new Rect(995,170,177,448);
        static readonly Rect CommanderPortrait = new Rect(1015,207,137,205.5f);
        bool CommanderHudContains(Vector2 point)=>!menu&&!handoff&&CommanderPanel.Contains(point);
        bool CommanderCanSelect()=>Viewer==match.Active&&Enumerable.Range(0,match.Board.Count).Any(match.CanSummonCommander);
        void SelectCommander()
        {
            if(selectedCommander){selectedCommander=false;ResetDrag();notice="Seleção do comandante cancelada.";return;}
            if(!CommanderCanSelect())return;
            selectedCommander=true;selectedCard=null;selectedUnit=-1;ResetDrag();
            notice="Escolha um terreno destacado do seu reino para conjurar o comandante. Esc cancela.";
        }
        void DrawCommanderZone(bool modal)
        {
            commanderHover=false;if(handoff||menu)return;
            var seat=match.Seats[Viewer];var card=seat.Commander;
            Fill(CommanderPanel,Panel);Text(1004,178,160,25,"COMANDANTE",cardName,Gold);
            if(card==null){Text(1008,230,151,145,"Sem comandante neste deck.\n\nEscolha um deck com comandante em Jogar ou use Experimentar comandantes.",small,Muted);return;}
            var point=Event.current.mousePosition;
            commanderHover=!modal&&CommanderPortrait.Contains(point);
            bool previous=GUI.enabled;GUI.enabled=true;
            RenderCardFace(CommanderPortrait,card,CardStyle(card),selectedCommander);
            GUI.enabled=!modal;
            var piece=match.Board.SelectMany(c=>c.Pieces).FirstOrDefault(p=>p.Owner==Viewer&&p.Card.Id==card.Id);
            bool pending=match.Stack.Any(p=>p.Owner==Viewer&&p.Card?.Id==card.Id);
            string location=seat.CommanderReady?"Zona de comando":piece!=null?"Em campo":pending?"Na pilha":"Fora da zona";
            Text(1004,421,161,24,location,cardName,Gold);
            Text(1004,448,161,43,"Custo: "+(card.TotalCost+seat.CommanderCasts*2)+" mana\nAdicional: +"+(seat.CommanderCasts*2)+" incolor",small,Muted);
            bool available=CommanderCanSelect();
            if(Button(1004,497,159,42,selectedCommander?"Cancelar seleção":piece!=null?"Selecionar em campo":"Conjurar",selectedCommander||available||piece!=null,true)){
                if(piece!=null){selectedCommander=false;selectedCard=null;selectedUnit=piece.Id;selectedCell=match.Position(piece.Id);ResetDrag();}
                else SelectCommander();
            }
            string hint=selectedCommander?"Clique em um terreno destacado.":seat.CommanderReady&&!available?(Viewer!=match.Active?"Aguarde seu turno.":match.Phase!=Stage.Main?"Disponível na fase principal.":match.Stack.Count>0?"Aguarde a pilha resolver.":"Confira mana e prioridade."):"Passe o mouse para ampliar.";
            Text(1004,544,161,36,hint,small,Muted);
            Text(1004,581,161,33,match.CommanderMechanicStatus(Viewer),small,Gold);
            if(!modal&&CommanderPortrait.Contains(point)&&Event.current.type==EventType.MouseDown){
                if(Event.current.button==1){inspected=card;detailScroll=Vector2.zero;}
                else if(Event.current.button==0)SelectCommander();
                Event.current.Use();
            }
            GUI.enabled=previous;
        }
        void DrawCommanderPreview(bool modal)
        {
            if(modal||!commanderHover||match.Seats[Viewer].Commander==null)return;
            RenderCardFace(new Rect(709,207,260,390),match.Seats[Viewer].Commander,CardStyle(match.Seats[Viewer].Commander),true);
        }
    }
}
