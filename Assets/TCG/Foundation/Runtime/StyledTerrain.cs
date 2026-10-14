using UnityEngine;
using TCG.Foundation;

namespace TCG.Table
{
 public sealed partial class TableWorld
 {
  // Decoration is confined to the rim: formation, hits and the playing surface stay unchanged.
  void StyledTerrainSurface(Transform root,Cell cell)
  {
   var land=Land[cell.Terrain.Color];
   CmdPart("Pedra",new Vector3(0,-.075f,0),new Vector3(.94f,.3f,.94f),Hex("526058"),root,"plate");
   CmdPart("Terreno",new Vector3(0,.085f,0),new Vector3(.92f,.035f,.92f),land,root,"plate");
   var stone=Color.Lerp(land,Hex("D9C9AA"),.38f);
   for(int side=-1;side<=1;side+=2)for(int i=0;i<4;i++)
   {
    float z=-.28f+i*.19f;
    CmdPart("Borda de pedra",new Vector3(side*.438f,.107f,z),new Vector3(.032f,.021f,.166f),stone,root,"plate");
   }
   for(int i=0;i<4;i++)CmdPart("Pedra da borda posterior",new Vector3(-.3f+i*.2f,.107f,.441f),new Vector3(.175f,.021f,.03f),stone,root,"plate");
   // Shallow paving at the rear evokes a miniature diorama without raising the piece base.
   for(int i=0;i<3;i++)CmdPart("Laje antiga",new Vector3(-.11f+i*.11f,.108f,.365f),new Vector3(.095f,.011f,.06f),Color.Lerp(land,stone,.5f),root,"plate");
  }
  void StyledTerrainCapital(Transform root,Color tint)
  {
   var fort=Group("Marco da capital",root);fort.localPosition=new Vector3(0,.1f,.34f);fort.localScale=Vector3.one*.4f;
   var stone=Hex("D0C6A7");var shade=Hex("B5AA91");var dark=Hex("403C35");
   CmdPart("Fortaleza",new Vector3(0,.24f,.15f),new Vector3(.46f,.28f,.34f),stone,fort,"plate");
   CmdPart("Cornija",new Vector3(0,.395f,.15f),new Vector3(.5f,.045f,.36f),shade,fort,"plate");
   for(int x=-1;x<=1;x+=2)
   {
    CmdPart("Torre",new Vector3(x*.24f,.34f,.2f),new Vector3(.16f,.5f,.18f),shade,fort,"tube");
    CmdPart("Telhado da torre",new Vector3(x*.24f,.66f,.2f),new Vector3(.23f,.2f,.23f),tint,fort,"cone");
    CmdPart("Janela da torre",new Vector3(x*.24f,.37f,.102f),new Vector3(.042f,.1f,.018f),dark,fort,"plate");
    CmdPart("Faixa da torre",new Vector3(x*.24f,.52f,.2f),new Vector3(.178f,.035f,.198f),stone,fort,"tube");
   }
   CmdPart("Arco do portão",new Vector3(0,.25f,-.027f),new Vector3(.15f,.245f,.025f),shade,fort,"plate");
   CmdPart("Portão",new Vector3(0,.23f,-.047f),new Vector3(.1f,.2f,.015f),dark,fort,"plate");
   for(int i=-1;i<=1;i++)CmdLine("Grade do portão",new Vector3(i*.025f,.145f,-.058f),new Vector3(i*.025f,.3f,-.058f),.009f,Hex("B69A65"),fort);
   for(int i=-2;i<=2;i++)CmdPart("Ameia",new Vector3(i*.092f,.439f,.16f),new Vector3(.055f,.064f,.3f),stone,fort,"plate");
   CmdLine("Mastro da capital",new Vector3(0,.42f,.22f),new Vector3(0,.72f,.22f),.013f,Hex("69513A"),fort);
   CmdPart("Estandarte da capital",new Vector3(.065f,.645f,.22f),new Vector3(.13f,.105f,.018f),tint,fort,"plate");
  }
  void StyledTerrainRuins(Transform root)
  {
   var stone=Hex("939087");var moss=Hex("68775C");
   for(int row=0;row<3;row++)for(int i=0;i<2;i++)
    if(row<2||i==0)CmdPart("Muro quebrado",new Vector3(-.345f+i*.105f+row%2*.02f,.13f+row*.064f,.34f),new Vector3(.1f,.058f,.115f),Color.Lerp(stone,Hex("77736D"),row*.15f),root,"plate");
   CmdPart("Base de coluna",new Vector3(.32f,.135f,.34f),new Vector3(.17f,.05f,.16f),stone,root,"plate");
   CmdPart("Coluna partida",new Vector3(.32f,.225f,.34f),new Vector3(.1f,.15f,.1f),stone,root,"tube");
   var fallen=CmdPart("Tambor caído",new Vector3(.22f,.145f,.37f),new Vector3(.08f,.12f,.08f),stone,root,"tube");fallen.localRotation=Quaternion.Euler(0,0,68);
   for(int i=0;i<3;i++)CmdPart("Musgo nas ruínas",new Vector3(-.39f+i*.07f,.114f,.37f),new Vector3(.08f,.016f,.055f),moss,root);
  }
  void StyledTerrainScenery(Transform root,int color)
  {
   var land=Land[color];var stone=Color.Lerp(land,Hex("D8C7AC"),.35f);var wood=Hex("71563E");
   if(color==5||color==6)
   {
    CmdLine("Tronco",new Vector3(-.34f,.11f,.34f),new Vector3(-.34f,.35f,.34f),.045f,wood,root);
    for(int i=0;i<3;i++)CmdPart("Copa suave",new Vector3(-.34f+(i-1)*.045f,.34f+i%2*.065f,.34f),new Vector3(.17f,.19f,.15f),i==1?Hex("658D68"):Hex("416F59"),root);
    CmdPart("Arbusto",new Vector3(.34f,.15f,.35f),new Vector3(.18f,.085f,.13f),Hex("78946A"),root);
   }
   else if(color==2)
   {
    CmdPart("Água da nascente",new Vector3(.33f,.11f,.33f),new Vector3(.2f,.012f,.17f),Hex("7CB9C1"),root);
    for(int i=0;i<3;i++)CmdPart("Pedra da nascente",new Vector3(.25f+i*.065f,.13f,.395f),new Vector3(.065f,.04f,.055f),stone,root);
    CmdLine("Junco",new Vector3(-.34f,.11f,.35f),new Vector3(-.32f,.27f,.35f),.012f,Hex("788668"),root);
    CmdLine("Junco",new Vector3(-.31f,.11f,.37f),new Vector3(-.3f,.23f,.37f),.012f,Hex("788668"),root);
   }
   else if(color==0)
   {
    CmdPart("Fardo de palha",new Vector3(.34f,.16f,.35f),new Vector3(.18f,.1f,.12f),Hex("D5B96E"),root,"tube");
    for(int i=0;i<2;i++)CmdPart("Cinta do fardo",new Vector3(.29f+i*.1f,.16f,.35f),new Vector3(.012f,.106f,.126f),wood,root,"tube");
    CmdLine("Poste rural",new Vector3(-.34f,.11f,.35f),new Vector3(-.34f,.27f,.35f),.025f,wood,root);
    CmdLine("Travessa rural",new Vector3(-.41f,.21f,.35f),new Vector3(-.27f,.21f,.35f),.021f,wood,root);
   }
   else
   {
    for(int side=-1;side<=1;side+=2)
    {
     var rock=CmdPart("Rocha arredondada",new Vector3(side*.34f,.17f,.35f),new Vector3(.16f,.14f,.14f),stone,root);rock.localRotation=Quaternion.Euler(12,side*20,side*15);
     CmdPart("Pedra menor",new Vector3(side*.25f,.13f,.38f),new Vector3(.065f,.046f,.06f),Color.Lerp(stone,land,.45f),root);
    }
    if(color==1)CmdPart("Cristal violeta",new Vector3(-.34f,.26f,.35f),new Vector3(.065f,.16f,.065f),Hex("B2A0CB"),root,"cone");
   }
  }
 }
}
