using System.Linq;
using UnityEngine;
using TCG.Foundation;
namespace TCG.Table
{
    public sealed partial class TableView
    {
        bool selectedCommander;
        void DrawCommanderZone()
        {
            if(handoff||menu)return;var seat=match.Seats[Viewer];if(seat.Commander==null)return;
            var c=seat.Commander;Fill(new Rect(995,170,177,204),Panel);Text(1003,177,160,43,c.Name,cardName,Gold);
            Text(1003,312,160,59,match.CommanderMechanicStatus(Viewer),small,Gold);
            string location=seat.CommanderReady?"Zona de comando":match.Board.Any(b=>b.Pieces.Any(p=>p.Owner==Viewer&&p.Card.Id==c.Id))?"Em campo":"Fora da zona";
            Text(1003,224,160,25,location+" · custo "+(c.TotalCost+seat.CommanderCasts*2),small,Muted);
            if(Button(1003,255,160,47,selectedCommander?"Escolha um terreno":"Conjurar",seat.CommanderReady&&Viewer==match.Active&&match.Phase==Stage.Main&&match.Stack.Count==0,true)){selectedCommander=true;selectedCard=null;selectedUnit=-1;ResetDrag();notice="Escolha um terreno seu para conjurar o comandante.";}
        }
    }
}

