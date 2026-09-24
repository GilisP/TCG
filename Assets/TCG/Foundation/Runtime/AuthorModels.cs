using UnityEngine;
using TCG.Foundation;
namespace TCG.Table
{
    public sealed partial class TableWorld
    {
        bool BuildAuthorObject(Definition card,Transform root)
        {
            var tint=Elements[card.Color];var stone=Hex("ACAA95");var wood=Hex("795942");var metal=Hex("C1CBC7");
            if(card.IsVehicle)
            {
                if(card.IsMeka)
                {
                    for(int side=-1;side<=1;side+=2){Block("Pé articulado",new Vector3(side*.17f,.13f,0),new Vector3(.17f,.18f,.31f),metal,root);Block("Perna",new Vector3(side*.17f,.35f,0),new Vector3(.11f,.32f,.12f),stone,root);}
                    Block("Cabine",new Vector3(0,.62f,0),new Vector3(.42f,.36f,.34f),metal,root);Block("Visor",new Vector3(0,.66f,-.19f),new Vector3(.29f,.13f,.025f),Elements[2],root);
                    for(int side=-1;side<=1;side+=2)Block("Braço",new Vector3(side*.31f,.57f,0),new Vector3(.14f,.35f,.15f),stone,root);
                }
                else
                {
                    Block("Plataforma de transporte",new Vector3(0,.24f,0),new Vector3(.59f,.12f,.66f),wood,root);
                    for(int side=-1;side<=1;side+=2){Block("Lateral",new Vector3(side*.28f,.39f,0),new Vector3(.065f,.25f,.65f),wood,root);for(int z=-1;z<=1;z+=2)Block("Roda",new Vector3(side*.35f,.16f,z*.22f),new Vector3(.09f,.28f,.28f),metal,root);}
                    Block("Lança de direção",new Vector3(0,.26f,-.48f),new Vector3(.055f,.05f,.36f),wood,root);
                }
                return true;
            }
            if(card.Rule=="cannon")
            {
                Block("Chassi",new Vector3(0,.19f,0),new Vector3(.4f,.18f,.4f),wood,root);
                var barrel=Block("Canhão de energia",new Vector3(0,.37f,-.05f),new Vector3(.2f,.19f,.53f),metal,root);barrel.localRotation=Quaternion.Euler(-18,0,0);
                Block("Boca luminosa",new Vector3(0,.46f,-.32f),new Vector3(.14f,.12f,.02f),tint,root);
                for(int x=-1;x<=1;x+=2)Block("Roda",new Vector3(x*.24f,.13f,0),new Vector3(.07f,.25f,.28f),Hex("293C41"),root);return true;
            }
            if(card.Rule=="mirror-copy")
            {
                Block("Moldura lunar",new Vector3(0,.4f,0),new Vector3(.42f,.6f,.085f),metal,root);
                Block("Espelho",new Vector3(0,.4f,-.05f),new Vector3(.33f,.49f,.02f),tint,root);
                Cone(new Vector3(0,.82f,0),.12f,.25f,tint,root,6);return true;
            }
            if(card.Rule=="catapult")
            {
                Block("Base",new Vector3(0,.14f,0),new Vector3(.5f,.15f,.47f),wood,root);
                for(int x=-1;x<=1;x+=2)Block("Apoio",new Vector3(x*.18f,.34f,0),new Vector3(.08f,.45f,.1f),wood,root);
                var arm=Block("Braço lançador",new Vector3(0,.5f,0),new Vector3(.065f,.065f,.7f),metal,root);arm.localRotation=Quaternion.Euler(40,0,0);
                Block("Cesto",new Vector3(0,.69f,.23f),new Vector3(.25f,.11f,.23f),wood,root);return true;
            }
            if(card.Kind==CardType.Terrain)
            {
                Block("Tile",new Vector3(0,.11f,0),new Vector3(.8f,.12f,.7f),tint*.6f,root);
                if(card.Rule=="portals"){for(int side=-1;side<=1;side+=2)Block("Pilar",new Vector3(side*.2f,.43f,0),new Vector3(.11f,.6f,.12f),stone,root);Block("Arco",new Vector3(0,.75f,0),new Vector3(.53f,.1f,.14f),stone,root);Block("Passagem",new Vector3(0,.41f,.03f),new Vector3(.28f,.48f,.035f),Hex("75A5C5"),root);}
                else {Cone(new Vector3(-.18f,.33f,.08f),.19f,.36f,tint,root,5);Block("Marco",new Vector3(.23f,.28f,-.07f),new Vector3(.13f,.24f,.14f),stone,root);}
                return true;
            }
            return false;
        }
    }
}

