using System.Linq;
using UnityEngine;
namespace TCG
{
    public sealed partial class GameView
    {
        Vector2 domainScroll;
        void DrawDomainPanel()
        {
            if (source < 0) { Text(730,370,662,90,"Escolha uma casa do campo para ver o terreno, as construções e suas habilidades.",body,ink); return; }
            var tile = game.Board[source];
            domainScroll = GUI.BeginScrollView(new Rect(730,362,662,270),domainScroll,new Rect(0,0,638,225+tile.Buildings.Count*140));
            Text(10,0,610,35,tile.Terrain?.Name ?? (tile.Capital ? "Capital" : "Território sem terreno"),heading,gold);
            Text(10,37,610,25,$"Habitação: {tile.HousingUsed}/{tile.Terrain?.Definition.Housing ?? 0} · "+(tile.Unit == null ? "Desabitado" : "Ocupante: "+tile.Unit.Card.Name),small,ink);
            if (tile.Terrain != null)
            {
                Text(10,65,610,52,tile.Terrain.Definition.Text,small,ink);
                if (Button(10,124,165,30,"Ver terreno",panel)) zoomCard = tile.Terrain.Definition;
                var colors=tile.Terrain.Definition.ProductionColors;
                if(Button(10,161,610,31,"Próxima geração: "+Catalog.Elements[tile.ProducedElement]+" · clique para alternar",panel,game.OpenMain && tile.Owner==game.Active && colors.Length>1))
                    Execute(()=>game.ChooseTerrainMana(source,colors[(System.Array.IndexOf(colors,tile.ProducedElement)+1)%colors.Length]));
                string reason = game.AbilityBlockReason(source);
                if (Button(185,124,435,30,reason == "" ? "Ativar terreno · "+tile.Terrain.Definition.ActivationCost+" mana" : reason,bg,reason == "")) Execute(() => game.ActivatePermanent(source));
            }
            for (int i=0;i<tile.Buildings.Count;i++)
            {
                var b=tile.Buildings[i]; float y=218+i*140;
                Frame(new Rect(4,y,628,134),bronze);
                Text(14,y+6,604,23,b.Card.Name+" · "+b.Card.Definition.HousingUse+" habitação · Vida "+b.Life+"/"+b.Card.Definition.BuildingLife,body,gold);
                Text(14,y+31,604,25,b.Card.Definition.Autonomous ? "Autônoma" : "Requer uma criatura sua neste terreno",small,muted);
                if (Button(14,y+58,150,28,"Ver carta",panel)) zoomCard=b.Card.Definition;
                string reason=game.AbilityBlockReason(source,i); int chosen=i;
                if (Button(174,y+58,442,28,reason == "" ? "Ativar · "+b.Card.Definition.ActivationCost+" mana" : reason,bg,reason == "")) Execute(()=>game.ActivatePermanent(source,chosen));
                if (Button(14,y+96,604,28,"Reparar 2 de vida · 1 mana · exige habitante",panel,game.CanRepair(source,chosen))) Execute(()=>game.Repair(source,chosen));
            }
            GUI.EndScrollView();
        }
    }
}
