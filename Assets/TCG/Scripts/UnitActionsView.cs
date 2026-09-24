using System;
using System.Linq;
using UnityEngine;
namespace TCG
{
    public sealed partial class GameView
    {
        int gearSelection=-1, passengerSelection=-1;
        Vector2 unitActionsScroll;
        void DrawUnitActions()
        {
            if(source<0||game.Board[source].Unit==null)
            {Text(730,370,660,80,"Selecione uma criatura ou veículo seu. Equipamentos podem ser transferidos; criaturas adjacentes podem embarcar.",body,ink);return;}
            var unit=game.Board[source].Unit;
            unitActionsScroll=GUI.BeginScrollView(new Rect(730,361,662,277),unitActionsScroll,new Rect(0,0,638,220+unit.Equipment.Count*73+(unit.IsVehicle?70+unit.Passengers.Count*65:0)));
            Text(8,0,620,36,unit.Card.Name,heading,gold);
            Text(8,41,620,49,$"Vida {unit.Life}/{unit.Card.Definition.Life} · Ataque {unit.Attack} · Armadura {unit.Armor}\nMovimento {unit.Movement} · Ações {unit.Actions} · Alcance {unit.Range}",body,ink);
            Text(8,98,620,30,"Escudo "+(unit.Shield+unit.TemporaryShield)+(unit.StunnedThroughTurn>=game.Turn?" · CONGELADO":""),small,blue);
            if(Button(8,132,220,30,"Ver carta",panel))zoomCard=unit.Card.Definition;
            if(Button(241,132,382,30,gearSelection>=0||passengerSelection>=0?"Cancelar escolha de alvo":"Desmarcar",panel))
            {if(gearSelection>=0||passengerSelection>=0){gearSelection=passengerSelection=-1;}else source=-1;}
            float y=176;
            if(unit.IsVehicle)
            {
                Text(8,y,620,48,$"Assentos {unit.Passengers.Count}/{unit.Card.Definition.Seats} · Tripulação mínima {unit.Card.Definition.CrewRequired}\n"+(unit.Crewed(game.Turn)?"Pronto para operar":"Falta tripulação ativa para mover e atacar"),small,ink);y+=61;
                for(int i=0;i<unit.Passengers.Count;i++)
                {
                    int chosen=i;Text(8,y,610,25,unit.Passengers[i].Card.Name+" · Vida "+unit.Passengers[i].Life,small,gold);
                    if(Button(8,y+26,615,29,"Desembarcar → escolha casa adjacente vazia",panel,game.OpenMain))
                    {passengerSelection=chosen;gearSelection=-1;notice="Escolha uma casa adjacente sua ou neutra marcada MOVER.";}y+=65;
                }
            }
            else if(unit.Equipment.Count==0)Text(8,y,620,45,"Sem equipamentos. Para embarcar, clique num veículo aliado adjacente marcado EMBARCAR.",small,muted);
            for(int i=0;i<unit.Equipment.Count;i++)
            {
                int chosen=i;Text(8,y,618,28,unit.Equipment[i].Name,body,gold);
                if(Button(8,y+30,300,31,"Desequipar · 1 mana",panel,game.CanChangeEquipment(source,i)))
                    if(Execute(()=>game.ChangeEquipment(source,chosen))){gearSelection=-1;source=-1;}
                if(Button(320,y+30,303,31,"Transferir · 1 mana",panel,game.CanChangeEquipment(source,i)))
                {gearSelection=chosen;passengerSelection=-1;notice="Selecione uma criatura sua adjacente com espaço para equipamento.";}y+=73;
            }
            GUI.EndScrollView();
        }
    }
}
