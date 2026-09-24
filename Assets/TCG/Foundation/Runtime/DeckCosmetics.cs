using UnityEngine;
using TCG.Foundation;
namespace TCG.Table
{
    public sealed partial class TableWorld
    {
        public void SetCardBack(int player,string id)
        {
            if(player<0||player>=places.Length||places[player]==null)return;
            var color=Hex(CosmeticCatalog.Get(id).Color);
            foreach(var render in places[player].GetComponentsInChildren<Renderer>(true))
                if(render.name=="Verso")render.sharedMaterial=Material(color);
        }
    }
}
