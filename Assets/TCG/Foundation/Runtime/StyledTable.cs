using UnityEngine;
namespace TCG.Table
{
 public sealed partial class TableWorld
 {
  void BuildStyledTable()
  {
   CmdPart("Mesa • carvalho esculpido",new Vector3(0,-.75f,0),new Vector3(15.6f,.65f,15.6f),Hex("453329"),transform,"plate");
   CmdPart("Mesa • moldura de latão",new Vector3(0,-.4f,0),new Vector3(15.3f,.12f,15.3f),Hex("B79A61"),transform,"plate");
   CmdPart("Mesa • feltro verde",new Vector3(0,-.3f,0),new Vector3(15.05f,.18f,15.05f),Hex("203E40"),transform,"plate");
   for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
   {
    CmdPart("Pé torneado",new Vector3(x*6,-2,z*6),new Vector3(.58f,2.5f,.58f),Hex("493327"),transform,"tube");
    for(int n=0;n<3;n++)CmdPart("Anel do pé",new Vector3(x*6,-2.7f+n*.7f,z*6),new Vector3(.7f,.12f,.7f),Hex("805D3C"),transform,"tube");
   }
   for(int side=0;side<4;side++)
   {
    var rail=CmdPivot("Entalhes da mesa",Vector3.zero,transform);rail.localRotation=Quaternion.Euler(0,side*90,0);
    for(int n=-7;n<=7;n++)
    {
     CmdPart("Tacha de latão",new Vector3(n,-.385f,7.58f),new Vector3(.065f,.065f,.065f),Hex("D9BA78"),rail);
     if(n%2==0)CmdPart("Medalhão entalhado",new Vector3(n,-.72f,7.8f),new Vector3(.28f,.22f,.04f),Hex("A78350"),rail);
    }
   }
  }
 }
}
