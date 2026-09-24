using UnityEngine;
using TCG.Foundation;
namespace TCG.Table
{
 public sealed partial class TableWorld
 {
  Transform Bead(string name,Vector3 pos,Vector3 scale,Color color,Transform parent)
  {
   var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localScale=scale;Destroy(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=Material(color);return go.transform;
  }
  void AddCraftDetails(Definition card,Transform root)
  {
   var gold=Hex("E8CA83");var steel=Hex("B7C1C6");var leather=Hex("5C3D2D");var dark=Hex("18232A");
   if(card.Kind==CardType.Construction){
    for(int side=-1;side<=1;side+=2){Block("Reforço de ferro",new Vector3(side*.19f,.24f,-.151f),new Vector3(.03f,.29f,.02f),steel,root);for(int n=0;n<3;n++)Bead("Rebite",new Vector3(side*.19f,.12f+n*.1f,-.17f),Vector3.one*.022f,gold,root);}return;
   }
   if(card.Kind!=CardType.Creature)return;string shape=card.Art;
   if(shape=="beast"){
    for(int side=-1;side<=1;side+=2){Bead("Orelha",new Vector3(side*.12f,.43f,-.25f),new Vector3(.09f,.11f,.07f),dark,root);for(int z=-1;z<=1;z+=2)for(int n=0;n<3;n++)Block("Garra",new Vector3(side*.13f+(n-1)*.018f,.025f,z*.15f-.042f),new Vector3(.012f,.025f,.045f),steel,root);}
    Bead("Nariz",new Vector3(0,.34f,-.391f),new Vector3(.09f,.055f,.04f),dark,root);return;
   }
   if(shape=="winged"){for(int side=-1;side<=1;side+=2)for(int n=0;n<4;n++){var feather=Block("Pena da asa",new Vector3(side*(.19f+n*.085f),.43f,-.07f+n*.035f),new Vector3(.05f,.028f,.3f-n*.025f),Color.Lerp(Elements[card.Color],steel,n*.16f),root);feather.localRotation=Quaternion.Euler(0,side*n*9,side*28);}return;}
   if(shape=="serpent"){for(int n=0;n<5;n++)Bead("Escama",new Vector3(.1f,.19f+n*.09f,-.11f),new Vector3(.12f,.035f,.025f),steel,root);return;}
   float k=shape=="giant"?1.35f:1;
   for(int side=-1;side<=1;side+=2){
    Bead("Articulação do ombro",new Vector3(side*.14f,.4f,0)*k,Vector3.one*.09f*k,steel,root);
    Block("Punho bordado",new Vector3(side*.16f,.248f,-.03f)*k,new Vector3(.085f,.025f,.09f)*k,gold,root);
    Block("Faixa da capa",new Vector3(side*.105f,.3f,.126f)*k,new Vector3(.016f,.32f,.012f)*k,gold,root);
   }
   Block("Fivela gravada",new Vector3(0,.215f,-.102f)*k,new Vector3(.06f,.045f,.018f)*k,gold,root);
   for(int x=-1;x<=1;x++)for(int y=0;y<3;y++)Bead("Malha de armadura",new Vector3(x*.045f,.27f+y*.04f,-.083f)*k,new Vector3(.024f,.029f,.011f)*k,steel,root);
   if(shape=="mage"){
    var book=Block("Tomo encadernado",new Vector3(.19f,.32f,-.06f),new Vector3(.12f,.15f,.045f),leather,root);book.localRotation=Quaternion.Euler(-12,0,-12);
    Block("Páginas do tomo",new Vector3(.19f,.32f,-.087f),new Vector3(.093f,.12f,.012f),Hex("E8DFC1"),root);
    Bead("Gema do cajado",new Vector3(-.2f,.79f,0),Vector3.one*.09f,Elements[card.Color],root);
   }
   if(card.Rule=="abyss-hunger"){for(int n=0;n<7;n++){float a=n*Mathf.PI*2/7;Cone(new Vector3(Mathf.Cos(a)*.22f,.73f,Mathf.Sin(a)*.2f),.055f,.29f,dark,root,5);}Bead("Olho do abismo",new Vector3(0,.72f,-.123f),new Vector3(.08f,.045f,.024f),Hex("B282E5"),root);}
   if(card.IsCommander){for(int n=-1;n<=1;n++)Bead("Selo do comandante",new Vector3(n*.12f,.09f,-.18f),new Vector3(.04f,.024f,.04f),gold,root);}
  }
 }
}
