using System.Linq;
namespace TCG.Foundation
{
    public sealed class CosmeticDefinition
    {
        public readonly string Id,Name,Color;public readonly int Price;
        public CosmeticDefinition(string id,string name,string color,int price){Id=id;Name=name;Color=color;Price=price;}
    }
    public static class CosmeticCatalog
    {
        public static readonly CosmeticDefinition[] All={
            new CosmeticDefinition("classic","Selo dos Reinos","25434A",0),
            new CosmeticDefinition("ember","Brasa Real","873F2C",150),
            new CosmeticDefinition("tide","Maré Arcana","245C87",150),
            new CosmeticDefinition("grove","Bosque Antigo","396A4E",150)};
        public static CosmeticDefinition Get(string id)=>All.FirstOrDefault(c=>c.Id==id)??All[0];
    }
}
