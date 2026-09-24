using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
    public sealed partial class TableWorld
    {
        Transform TerrainPortrait(Definition card,Transform parent)
        {
            var root=Group(card.Name,parent);Color tint=Elements[card.Color];
            Block("Base de paisagem",Vector3.zero,new Vector3(.95f,.12f,.78f),Color.Lerp(Hex("455449"),tint,.35f),root);
            Block("Caminho",new Vector3(.05f,.075f,-.08f),new Vector3(.18f,.015f,.62f),Hex("B29B73"),root);
            if(card.Color==2)
            {
                Block("Rio",new Vector3(-.13f,.08f,0),new Vector3(.45f,.018f,.75f),Hex("439EBB"),root);
                for(int n=0;n<4;n++)Block("Reflexo da água",new Vector3(-.16f,.094f,-.28f+n*.18f),new Vector3(.28f,.008f,.016f),Hex("B5DDE0"),root);
                Block("Ponte",new Vector3(0,.12f,-.12f),new Vector3(.9f,.055f,.17f),Hex("A78660"),root);
            }
            for(int n=0;n<3;n++)
            {
                float x=-.32f+n*.31f,z=.2f+(n%2)*.06f;
                if(card.Color==5||card.Color==6)
                {Block("Tronco",new Vector3(x,.22f,z),new Vector3(.045f,.3f,.045f),Hex("71543E"),root);Cone(new Vector3(x,.43f,z),.19f,.42f,Hex("3F7551"),root,7);}
                else if(card.Color==3||card.Color==4)
                {Cone(new Vector3(x,.25f,z),.26f,.48f,Color.Lerp(Hex("626568"),tint,.35f),root,5);Cone(new Vector3(x,.46f,z),.095f,.15f,card.Color==3?Hex("FFB457"):Hex("E4DDD0"),root,5);}
                else
                {Block("Torre",new Vector3(x,.25f,z),new Vector3(.16f,.4f,.18f),card.Color==1?Hex("746981"):Hex("C6B994"),root);Cone(new Vector3(x,.51f,z),.14f,.17f,tint,root,4);Block("Janela",new Vector3(x,.3f,z-.096f),new Vector3(.045f,.09f,.012f),Hex("F4D581"),root);}
            }
            return root;
        }
    }
}
