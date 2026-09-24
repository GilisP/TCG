using UnityEngine;
using TCG.Foundation;
namespace TCG.Table
{
    public sealed partial class TableWorld
    {
        void AddFigureDetails(Definition card,Transform root)
        {
            if(card.Kind!=CardType.Creature)return;
            if(card.IsCommander){for(int side=-1;side<=1;side+=2){Block("Estandarte do comandante",new Vector3(side*.23f,.49f,.18f),new Vector3(.13f,.39f,.025f),Elements[card.Color]*.6f,root);Block("Haste dourada",new Vector3(side*.23f,.38f,.2f),new Vector3(.022f,.72f,.022f),Hex("EAC17B"),root);}Cone(new Vector3(0,.83f,0),.11f,.14f,Hex("FFE4A0"),root,6);}
            string shape=card.Art;Color tint=Elements[card.Color],gold=Hex("ECD092"),ink=Hex("14232C"),skin=Hex("D3A982");
            if(shape=="beast"||shape=="serpent"||shape=="winged")
            {
                Vector3 face=shape=="beast"?new Vector3(0,.36f,-.386f):shape=="serpent"?new Vector3(-.18f,.74f,-.201f):new Vector3(0,.59f,-.22f);
                for(int side=-1;side<=1;side+=2){Block("Olho brilhante",face+Vector3.right*side*.065f,new Vector3(.045f,.03f,.02f),gold,root);}
                if(shape=="beast")for(int n=0;n<3;n++)Cone(new Vector3(0,.43f,.08f+n*.08f),.055f,.16f,tint,root,4);
                return;
            }
            float k=shape=="giant"?1.35f:1;
            for(int side=-1;side<=1;side+=2)
            {
                Block("Olho",new Vector3(side*.037f,.548f*k,-.081f*k),new Vector3(.025f,.021f,.016f),ink,root);
                var arm=Block("Braço",new Vector3(side*.14f*k,.31f*k,0),new Vector3(.075f,.23f,.09f)*k,tint*.8f,root);arm.localRotation=Quaternion.Euler(-12,0,side*-9);
                Block("Luva",new Vector3(side*.16f*k,.21f*k,-.02f),new Vector3(.075f,.07f,.085f)*k,skin,root);
                if(shape!="mage"&&shape!="archer")Block("Ombreira",new Vector3(side*.14f*k,.445f*k,0),new Vector3(.14f,.08f,.17f)*k,Hex("CBD1CA"),root);
            }
            Block("Cinto",new Vector3(0,.215f*k,-.075f*k),new Vector3(.22f,.035f,.035f)*k,ink,root);
            Block("Brasão",new Vector3(0,.345f*k,-.082f*k),new Vector3(.055f,.085f,.018f)*k,gold,root);
            var cape=Block("Capa",new Vector3(0,.3f*k,.112f*k),new Vector3(.23f,.38f,.025f)*k,tint*.45f,root);cape.localRotation=Quaternion.Euler(-12,0,0);
            if(shape=="mage"){Cone(new Vector3(0,.19f,0),.19f,.32f,tint*.7f,root,7);Block("Barba",new Vector3(0,.46f,-.095f),new Vector3(.1f,.12f,.04f),Hex("E1D8C1"),root);}
            else if(shape=="archer") {Block("Aljava",new Vector3(.13f,.35f,.12f),new Vector3(.09f,.3f,.09f),Hex("79583C"),root);for(int n=0;n<3;n++)Block("Flecha reserva",new Vector3(.11f+n*.022f,.53f,.12f),new Vector3(.012f,.2f,.012f),gold,root);}
            else if(shape!="giant") {var plume=Block("Pluma",new Vector3(0,.735f,0),new Vector3(.055f,.17f,.19f),tint,root);plume.localRotation=Quaternion.Euler(-18,0,0);}
        }
    }
}
