using System.Collections.Generic;
using UnityEngine;
using TCG.Foundation;

namespace TCG.Table
{
 public sealed partial class TableWorld
 {
  // Procedural art only. No card rules, colliders or gameplay components live here.
  static readonly Dictionary<string,Mesh> commanderMeshes=new Dictionary<string,Mesh>();
  Transform CmdPivot(string name,Vector3 position,Transform parent){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;}
  Transform CmdPart(string name,Vector3 position,Vector3 size,Color color,Transform parent,string shape="round")
  {
   if(name=="Belt tome"||name=="Segmented breastplate"||name=="Vambrace"||name=="Buckle"||name=="Visor"||name=="Face mask"||name=="Ivory court lapel"||name=="Midnight court lapel"||name=="Travel satchel"||name=="Loot sack"||name=="Saddle"||name=="Martyr sash"||name=="Sea coat lapel")shape="plate";
   if(!commanderMeshes.TryGetValue(shape,out var mesh)){
    const int sides=32,rings=20;var v=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
    for(int y=0;y<=rings;y++){float f=y/(float)rings;float radius,height;
     if(shape=="round"){radius=Mathf.Sin(f*Mathf.PI)*.5f;height=-Mathf.Cos(f*Mathf.PI)*.5f;}
     else if(shape=="cone"){radius=(1-f)*.5f;height=f-.5f;}
     else if(shape=="robe"){radius=Mathf.Lerp(.5f,.23f,f)*(1+.025f*Mathf.Sin(f*12));height=f-.5f;}
     else {radius=.5f;height=f-.5f;if(y==0||y==rings)radius=0;}
     for(int x=0;x<=sides;x++){float a=x*Mathf.PI*2/sides;v.Add(new Vector3(Mathf.Sin(a)*radius,height,Mathf.Cos(a)*radius));uv.Add(new Vector2(x/(float)sides,f));}
    }
    for(int y=0;y<rings;y++)for(int x=0;x<sides;x++){int n=y*(sides+1)+x;triangles.Add(n);triangles.Add(n+1);triangles.Add(n+sides+1);triangles.Add(n+1);triangles.Add(n+sides+2);triangles.Add(n+sides+1);}
    var normals=new List<Vector3>();
    if(shape=="plate"){
     // Rounded box: flat faces and a continuous rounded bevel, with analytic normals.
     // A book/armor panel needs corners rather than an ellipsoid or cylinder.
     v.Clear();uv.Clear();triangles.Clear();const int grid=16;const float bevel=.1f;
     var axes=new[]{Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};
     foreach(var normal in axes){var axisU=Mathf.Abs(normal.y)>.5f?Vector3.right:Vector3.up;var axisV=Vector3.Cross(normal,axisU);int offset=v.Count;
      for(int y=0;y<=grid;y++)for(int x=0;x<=grid;x++){var p=normal*.5f+axisU*(x/(float)grid-.5f)+axisV*(y/(float)grid-.5f);var q=new Vector3(Mathf.Clamp(p.x,-.5f+bevel,.5f-bevel),Mathf.Clamp(p.y,-.5f+bevel,.5f-bevel),Mathf.Clamp(p.z,-.5f+bevel,.5f-bevel));var n=(p-q).normalized;v.Add(q+n*bevel);normals.Add(n);uv.Add(new Vector2(x/(float)grid,y/(float)grid));}
      for(int y=0;y<grid;y++)for(int x=0;x<grid;x++){int n=offset+y*(grid+1)+x;triangles.Add(n);triangles.Add(n+1);triangles.Add(n+grid+1);triangles.Add(n+1);triangles.Add(n+grid+2);triangles.Add(n+grid+1);}
     }
    }
    mesh=new Mesh{name="Commander smooth "+shape};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();
    if(shape=="round")mesh.SetNormals(v.ConvertAll(p=>p.normalized));
    else if(shape=="plate")mesh.SetNormals(normals);
    else {var safeNormals=mesh.normals;for(int i=0;i<safeNormals.Length;i++)if(safeNormals[i].sqrMagnitude<.0001f||float.IsNaN(safeNormals[i].sqrMagnitude)||float.IsInfinity(safeNormals[i].sqrMagnitude))safeNormals[i]=v[i].y>=0?Vector3.up:Vector3.down;mesh.SetNormals(safeNormals);}
    mesh.RecalculateBounds();commanderMeshes.Add(shape,mesh);
   }
   var t=CmdPivot(name,position,parent);t.localScale=size;t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=Material(color);return t;
  }
  Transform CmdLine(string name,Vector3 a,Vector3 b,float width,Color color,Transform parent)
  {var t=CmdPart(name,(a+b)*.5f,new Vector3(width,(b-a).magnitude+width,width),color,parent);t.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);return t;}
  void CmdCrown(Transform head,Color metal,int points=5)
  {CmdPart("Crown circlet",new Vector3(0,.065f,0),new Vector3(.22f,.035f,.2f),metal,head,"tube");for(int i=0;i<points;i++){float a=i*Mathf.PI*2/points;CmdPart("Crown point",new Vector3(Mathf.Sin(a)*.09f,.105f,Mathf.Cos(a)*.085f),new Vector3(.025f,.1f,.025f),metal,head,"cone");}}
  void CmdStaff(Transform hand,Color wood,Color gem,bool skull=false)
  {CmdLine("Staff",new Vector3(0,-.22f,0),new Vector3(0,.38f,0),.025f,wood,hand);CmdPart(skull?"Skull finial":"Crystal finial",new Vector3(0,.4f,0),new Vector3(.1f,.12f,.08f),gem,hand);if(skull)for(int s=-1;s<=1;s+=2)CmdPart("Skull socket",new Vector3(s*.024f,.41f,-.038f),new Vector3(.024f,.025f,.012f),Hex("152027"),hand);}
  void CmdBlade(Transform arm,Color metal,bool curved=false)
  {CmdLine("Hilt",new Vector3(0,-.18f,-.02f),new Vector3(0,-.18f,-.1f),.04f,Hex("583A2B"),arm);CmdLine("Guard",new Vector3(-.07f,-.18f,-.09f),new Vector3(.07f,-.18f,-.09f),.024f,metal,arm);CmdLine(curved?"Curved saber":"Blade",new Vector3(0,-.18f,-.1f),new Vector3(curved?.09f:0,.14f,-.17f),.04f,metal,arm);}
  void CmdBook(Transform arm,Color cover)
  {CmdPart("Book binding",new Vector3(0,-.14f,-.07f),new Vector3(.13f,.17f,.055f),cover,arm,"plate");CmdPart("Book pages",new Vector3(0,-.14f,-.099f),new Vector3(.11f,.14f,.014f),Hex("EEE3C1"),arm,"plate");for(int i=0;i<7;i++)CmdLine("Page line",new Vector3(-.046f,-.2f+i*.02f,-.108f),new Vector3(.046f,-.2f+i*.02f,-.108f),.0025f,Hex("BFAF8B"),arm);for(int i=0;i<2;i++)CmdPart("Book clasp",new Vector3(0,-.19f+i*.1f,-.11f),new Vector3(.1f,.007f,.007f),Hex("CBA65E"),arm,"plate");}
  void CmdBow(Transform arm,Color color)
  {var start=new Vector3(0,-.32f,-.08f);var end=new Vector3(0,.2f,-.08f);var prev=start;for(int i=1;i<=10;i++){float f=i/10f;var next=Vector3.Lerp(start,end,f)+new Vector3(-Mathf.Sin(f*Mathf.PI)*.1f,0,0);CmdLine("Bow limb",prev,next,.02f,color,arm);prev=next;}CmdLine("Bow string",start,end,.004f,Hex("E9DBB4"),arm);}
  Transform CommanderFigure(Definition card,Color owner,Transform parent)
  {
   var root=CmdPivot("Commander "+card.Id+" — "+card.Name,Vector3.zero,parent);
   var dark=Hex("20232B");var gold=Hex("DDBA69");var silver=Hex("BFCAD0");var leather=Hex("654332");var bone=Hex("E8DDC0");var skin=Hex("C99A79");var cloth=CmdCloth(card.Id,Elements[Mathf.Clamp(card.Color,0,Elements.Length-1)]*.65f);cloth.a=1;
   CmdPart("SeatEmblem",new Vector3(0,.025f,0),new Vector3(.43f,.05f,.43f),owner,root,"tube");
   if(card.Id=="MED-229"){CmdBear(root,leather,bone);return root;}
   if(card.Id=="MED-195"){CmdTree(root);return root;}
   if(card.Id=="MED-232"){CmdAbyss(root);return root;}
   bool shortBody=card.Id=="MED-194"||card.Id=="MED-199";bool robe=card.Art=="mage"||card.Id=="MED-226"||card.Id=="MED-237";
   var body=CmdPivot("Body",new Vector3(0,.35f,0),root);var head=CmdPivot("Head",new Vector3(0,.28f,0),body);
   CmdPart("Tailored torso",Vector3.zero,new Vector3(shortBody?.32f:.25f,.3f,.18f),cloth,body,"plate");
   CmdPart("Belt",new Vector3(0,-.11f,0),new Vector3(.26f,.04f,.19f),leather,body,"tube");CmdPart("Buckle",new Vector3(0,-.11f,-.102f),new Vector3(.045f,.035f,.015f),gold,body);
   if(card.Id=="MED-194")skin=Hex("7D9C55");if(card.Id=="MED-237")skin=Hex("D9C8D4");if(card.Id=="MED-234")skin=Hex("E48643");
   CmdPart("Face",new Vector3(0,0,-.018f),new Vector3(.17f,.21f,.16f),skin,head);CmdPart("Nose",new Vector3(0,-.01f,-.101f),new Vector3(.026f,.036f,.031f),skin,head);
   CmdPart("Mouth",new Vector3(0,-.049f,-.088f),new Vector3(.032f,.005f,.006f),Hex("8C5C51"),head);
   for(int s=-1;s<=1;s+=2){CmdPart("Ear",new Vector3(s*.086f,0,-.012f),new Vector3(.035f,.06f,.03f),skin,head);CmdPart("Eye",new Vector3(s*.035f,.027f,-.09f),new Vector3(.018f,.011f,.01f),dark,head);CmdLine("Eyebrow",new Vector3(s*.024f,.044f,-.089f),new Vector3(s*.051f,.043f,-.081f),.006f,dark,head);}
   CmdPart("Hair cap",new Vector3(0,.073f,.02f),new Vector3(.19f,.105f,.17f),dark,head);
   var la=CmdPivot("LeftArm",new Vector3(-.145f,.1f,0),body);var ra=CmdPivot("RightArm",new Vector3(.145f,.1f,0),body);
   foreach(var arm in new[]{la,ra}){CmdPart("Sleeve",new Vector3(0,-.075f,0),new Vector3(.09f,.18f,.105f),cloth,arm);CmdPart("Cuff",new Vector3(0,-.15f,0),new Vector3(.092f,.032f,.1f),gold,arm);CmdPart("Hand",new Vector3(0,-.19f,-.008f),new Vector3(.07f,.085f,.075f),skin,arm);}
   for(int s=-1;s<=1;s+=2){var leg=CmdPivot(s<0?"LeftLeg":"RightLeg",new Vector3(s*.063f,-.13f,0),body);CmdPart("Trouser",new Vector3(0,-.065f,0),new Vector3(.09f,.16f,.095f),dark,leg);CmdPart("Boot",new Vector3(0,-.15f,-.025f),new Vector3(.105f,.11f,.15f),leather,leg,"plate");CmdPart("Boot sole",new Vector3(0,-.198f,-.025f),new Vector3(.11f,.018f,.157f),dark,leg,"plate");}
   var cape=CmdPivot("Cape",new Vector3(0,.13f,.095f),body);CmdPart("Draped cape",new Vector3(0,-.16f,.035f),new Vector3(.29f,.39f,.065f),Color.Lerp(cloth,dark,.4f),cape,"robe");for(int i=-2;i<=2;i++)CmdLine("Cape fold",new Vector3(i*.035f,-.03f,.072f),new Vector3(i*.05f,-.34f,.077f),.012f,cloth,cape);
   if(robe)CmdPart("Robe skirt",new Vector3(0,-.16f,0),new Vector3(.3f,.3f,.24f),cloth,body,"robe");
   switch(card.Id){
    case "MED-193":CmdBook(la,Hex("315566"));CmdStaff(ra,leather,Hex("74D9E8"));for(int i=0;i<3;i++)CmdPart("Belt tome",new Vector3(-.11f+i*.09f,-.1f,.105f),new Vector3(.06f,.11f,.04f),leather,body,"tube");break;
    case "MED-194":for(int s=-1;s<=1;s+=2)CmdPart("Long goblin ear",new Vector3(s*.13f,.025f,0),new Vector3(.14f,.055f,.045f),skin,head);CmdLine("Mace handle",new Vector3(0,-.17f,0),new Vector3(0,.24f,-.03f),.035f,leather,ra);CmdPart("Meaty mace",new Vector3(0,.27f,-.03f),new Vector3(.17f,.21f,.14f),Hex("A34F45"),ra);break;
    case "MED-196":CmdCrown(head,silver,3);CmdLine("Long lance",new Vector3(0,-.3f,0),new Vector3(0,.46f,0),.02f,leather,ra);CmdPart("Lance tip",new Vector3(0,.5f,0),new Vector3(.055f,.15f,.04f),silver,ra,"cone");CmdPart("Round shield",new Vector3(-.03f,-.08f,-.05f),new Vector3(.19f,.24f,.055f),gold,la);break;
    case "MED-197":CmdStaff(ra,dark,bone,true);for(int i=0;i<4;i++)CmdPart("Ossuary beads",new Vector3((i-1.5f)*.04f,.09f,-.095f),Vector3.one*.037f,bone,body);CmdPart("Hood",new Vector3(0,.025f,.035f),new Vector3(.22f,.27f,.19f),dark,head);break;
    case "MED-198":CmdBow(la,dark);CmdPart("Veil",new Vector3(0,-.11f,.015f),new Vector3(.23f,.3f,.17f),dark,head);for(int s=-1;s<=1;s+=2)CmdLine("Spider pendant legs",new Vector3(0,.08f,-.11f),new Vector3(s*.055f,.02f,-.115f),.009f,silver,body);break;
    case "MED-199":CmdCrown(head,gold,7);CmdPart("Braided beard",new Vector3(0,-.13f,-.055f),new Vector3(.16f,.24f,.1f),Hex("918274"),head);CmdLine("Hammer haft",new Vector3(0,-.22f,0),new Vector3(0,.19f,0),.035f,leather,ra);CmdPart("Hammer head",new Vector3(0,.2f,0),new Vector3(.21f,.1f,.11f),silver,ra);break;
    case "MED-200":CmdBow(la,leather);CmdPart("Quiver",new Vector3(.1f,0,.13f),new Vector3(.075f,.34f,.085f),leather,body,"tube");for(int i=0;i<4;i++)CmdLine("Arrow",new Vector3(.065f+i*.025f,0,.13f),new Vector3(.065f+i*.025f,.26f,.13f),.008f,bone,body);break;
    case "MED-201":CmdPart("Fur hat",new Vector3(0,.1f,0),new Vector3(.25f,.11f,.22f),leather,head);CmdPart("Horsehair plume",new Vector3(0,.19f,.045f),new Vector3(.05f,.18f,.07f),dark,head);CmdBlade(ra,silver,true);for(int s=-1;s<=1;s+=2)CmdPart("Fur shoulder",new Vector3(s*.135f,.12f,0),Vector3.one*.14f,bone,body);break;
    case "MED-202":CmdCrown(head,silver);CmdBlade(ra,silver);CmdPart("Martyr sash",new Vector3(0,0,-.095f),new Vector3(.05f,.3f,.02f),Hex("862E40"),body,"tube");break;
    case "MED-225":CmdCrown(head,gold,6);for(int s=-1;s<=1;s+=2)CmdPart(s<0?"Moon pauldron":"Sun pauldron",new Vector3(s*.13f,.12f,0),Vector3.one*.14f,s<0?Hex("9985BE"):gold,body);CmdBlade(ra,gold);break;
    case "MED-226":CmdCrown(head,silver,3);CmdFemaleHair(head,Hex("775640"),true);CmdStaff(ra,dark,Hex("8DB88F"),true);break;
    case "MED-227":for(int s=-1;s<=1;s+=2)CmdPart("Goggle lens",new Vector3(s*.042f,.025f,-.077f),new Vector3(.07f,.055f,.033f),Hex("71C8C9"),head);CmdPart("Pressure tank",new Vector3(0,0,.15f),new Vector3(.18f,.32f,.14f),Hex("966B45"),body,"tube");for(int i=0;i<3;i++)CmdPart("Bomb flask",new Vector3((i-1)*.08f,-.09f,-.115f),Vector3.one*.07f,dark,body);break;
    case "MED-228":CmdPart("Wide fishing hat",new Vector3(0,.09f,0),new Vector3(.32f,.045f,.3f),bone,head);CmdPart("White beard",new Vector3(0,-.11f,-.04f),new Vector3(.14f,.18f,.09f),bone,head);CmdLine("Fishing rod",new Vector3(0,-.2f,0),new Vector3(-.09f,.4f,-.1f),.012f,leather,la);CmdLine("Fishing line",new Vector3(-.09f,.4f,-.1f),new Vector3(-.09f,-.15f,-.15f),.003f,silver,la);break;
    case "MED-230":for(int s=-1;s<=1;s+=2){CmdPart("Elemental pauldron",new Vector3(s*.14f,.13f,0),Vector3.one*.16f,s<0?Hex("7C9A63"):Hex("DE7040"),body);CmdPart("Horn",new Vector3(s*.09f,.12f,.02f),new Vector3(.04f,.18f,.05f),leather,head,"cone");}CmdPart("Core",new Vector3(0,.04f,-.095f),Vector3.one*.1f,Hex("F2AD61"),body);break;
    case "MED-231":CmdPart("Long chestnut hair",new Vector3(0,-.08f,.06f),new Vector3(.22f,.34f,.1f),leather,head);CmdPart("Travel satchel",new Vector3(-.12f,-.08f,.07f),new Vector3(.14f,.18f,.1f),Hex("598A8A"),body);CmdLine("Steering wrench",new Vector3(0,-.19f,0),new Vector3(0,.07f,-.04f),.025f,silver,ra);break;
    case "MED-233":CmdPart("Prophet hood",new Vector3(0,.02f,.04f),new Vector3(.23f,.27f,.19f),Hex("82745C"),head);CmdStaff(ra,leather,Hex("8CA572"));CmdBook(la,Hex("4E5141"));break;
    case "MED-234":for(int i=0;i<7;i++)CmdPart("Flame mane",new Vector3((i-3)*.029f,.11f+Mathf.Sin(i)*.03f,.035f),new Vector3(.045f,.2f,.07f),Hex("EBA15B"),head,"cone");CmdStaff(ra,Hex("702F25"),Hex("FFCC72"));break;
    case "MED-235":CmdPart("Shaven scalp",new Vector3(0,.062f,0),new Vector3(.174f,.13f,.163f),skin,head);for(int i=0;i<9;i++){float a=i*Mathf.PI/8;CmdPart("Prayer bead",new Vector3(Mathf.Cos(a)*.09f,.06f-Mathf.Sin(a)*.05f,-.09f),Vector3.one*.024f,leather,body);}la.localRotation=Quaternion.Euler(-25,0,-25);ra.localRotation=Quaternion.Euler(-25,0,25);break;
    case "MED-236":CmdPart("Astronomer hat",new Vector3(0,.18f,.03f),new Vector3(.23f,.32f,.21f),Hex("3C587E"),head,"cone");CmdBook(la,Hex("344D72"));CmdStaff(ra,silver,Hex("BCE7EF"));break;
    case "MED-237":CmdPart("High red collar",new Vector3(0,-.12f,.07f),new Vector3(.26f,.22f,.12f),Hex("8C344F"),head);CmdPart("Ruby mitre",new Vector3(0,.17f,0),new Vector3(.17f,.26f,.12f),Hex("AD4260"),head,"cone");CmdStaff(ra,silver,Hex("D63E57"));break;
    case "MED-238":CmdPart("Closed helmet",new Vector3(0,.025f,.018f),new Vector3(.21f,.23f,.19f),silver,head);CmdPart("Visor",new Vector3(0,.015f,-.085f),new Vector3(.14f,.02f,.018f),dark,head);CmdPart("Sun crest",new Vector3(0,.17f,0),new Vector3(.045f,.19f,.16f),gold,head);CmdBlade(ra,gold);CmdPart("Sun shield",new Vector3(-.02f,-.08f,-.04f),new Vector3(.2f,.26f,.05f),gold,la);break;
    case "MED-239":CmdBook(la,Hex("563445"));for(int i=0;i<5;i++)CmdPart("Curse talisman",new Vector3((i-2)*.05f,.1f-Mathf.Abs(i-2)*.02f,-.105f),new Vector3(.034f,.06f,.022f),i%2==0?bone:Hex("CF7848"),body);CmdStaff(ra,dark,Hex("A188BF"),true);break;
    case "MED-240":CmdBlade(ra,silver);for(int s=-1;s<=1;s+=2){CmdPart("Beast-head shoulder",new Vector3(s*.14f,.13f,0),new Vector3(.15f,.14f,.19f),bone,body);CmdPart("Beast snout",new Vector3(s*.14f,.115f,-.1f),new Vector3(.1f,.065f,.08f),leather,body);}CmdPart("Antler helm",new Vector3(0,.045f,.025f),new Vector3(.2f,.2f,.18f),silver,head);for(int s=-1;s<=1;s+=2)CmdLine("Antler",new Vector3(s*.07f,.11f,0),new Vector3(s*.16f,.25f,.025f),.022f,bone,head);break;
    case "MED-241":CmdPart("Infiltrator hood",new Vector3(0,.03f,.045f),new Vector3(.23f,.26f,.18f),Hex("344A62"),head);CmdPart("Face mask",new Vector3(0,-.045f,-.068f),new Vector3(.16f,.075f,.032f),dark,head);CmdBlade(la,silver);CmdBlade(ra,silver);break;
    case "MED-242":CmdPart("Grave robber cap",new Vector3(.02f,.095f,.03f),new Vector3(.22f,.09f,.2f),Hex("62546C"),head);CmdPart("Loot sack",new Vector3(.1f,-.05f,.12f),new Vector3(.19f,.22f,.16f),leather,body);CmdLine("Lantern handle",new Vector3(0,-.16f,0),new Vector3(0,-.26f,0),.012f,silver,la);CmdPart("Lantern",new Vector3(0,-.29f,0),new Vector3(.09f,.12f,.08f),Hex("EABD6C"),la,"tube");break;
    case "IDEIA-004":CmdPart("Pirate brim",new Vector3(0,.085f,0),new Vector3(.32f,.045f,.2f),dark,head);for(int s=-1;s<=1;s+=2)CmdPart("Tricorn fold",new Vector3(s*.1f,.13f,.01f),new Vector3(.13f,.1f,.17f),dark,head);CmdPart("Eyepatch",new Vector3(.035f,.023f,-.077f),new Vector3(.045f,.035f,.018f),dark,head);CmdBlade(ra,silver,true);CmdPart("Sea coat lapel",new Vector3(-.065f,.04f,-.095f),new Vector3(.075f,.23f,.02f),Hex("456A64"),body);break;
    default:CmdStaff(ra,leather,cloth);break;
   }
   CmdRefineProfile(card.Id,body,head,la,ra,cloth,skin,gold,silver,bone);
   CmdMiniatureFinish(card.Id,body,head,la,ra,cloth,skin);
   if(shortBody)body.localScale=new Vector3(1.08f,.85f,1.08f);
   if(card.Id=="MED-201"){
    body.name="RiderBody";head.name="RiderHead";la.name="RiderLeftArm";ra.name="RiderRightArm";cape.name="RiderCape";
    foreach(Transform child in body){if(child.name=="LeftLeg")child.name="RiderLeftLeg";else if(child.name=="RightLeg")child.name="RiderRightLeg";}
    body.localPosition=new Vector3(0,.57f,.035f);body.localScale=Vector3.one*.66f;CmdHorse(root);
   }
   return root;
  }
  Color CmdCloth(string id,Color fallback)
  {
   switch(id){
    case "MED-193":return Hex("33476F");case "MED-194":return Hex("9C4B42");case "MED-196":return Hex("DDD0AD");case "MED-197":return Hex("654478");case "MED-198":return Hex("3B2E4A");case "MED-199":return Hex("506681");case "MED-200":return Hex("46664A");case "MED-201":return Hex("437D80");case "MED-202":return Hex("CAC2A6");case "MED-225":return Hex("30394F");case "MED-226":return Hex("9987B1");case "MED-227":return Hex("963E43");case "MED-228":return Hex("AE8D50");case "MED-230":return Hex("566957");case "MED-231":return Hex("4B8A98");case "MED-233":return Hex("526747");case "MED-234":return Hex("493A38");case "MED-235":return Hex("D7CBAA");case "MED-236":return Hex("426498");case "MED-237":return Hex("722F45");case "MED-238":return Hex("C9A964");case "MED-239":return Hex("566447");case "MED-240":return Hex("4B7E9C");case "MED-241":return Hex("303F59");case "MED-242":return Hex("655270");case "IDEIA-004":return Hex("456E68");default:return fallback;
   }
  }
  void CmdRefineProfile(string id,Transform body,Transform head,Transform left,Transform right,Color cloth,Color skin,Color gold,Color silver,Color bone)
  {
   var dark=Hex("20232B");var wood=Hex("654332");
   if(id=="MED-193"||id=="IDEIA-004"){
    for(int i=-2;i<=2;i++)CmdPart("Beard lock",new Vector3(i*.024f,-.105f-Mathf.Abs(i)*-.012f,-.056f),new Vector3(.036f,.17f-Mathf.Abs(i)*.025f,.065f),id=="MED-193"?bone:wood,head);
   }
   if(id=="MED-194"){
    for(int s=-1;s<=1;s+=2)CmdPart("Goblin tusk",new Vector3(s*.045f,-.064f,-.076f),new Vector3(.018f,.05f,.019f),bone,head,"cone");
    for(int i=0;i<5;i++)CmdLine("Belt stitch",new Vector3(-.09f+i*.045f,-.123f,-.1f),new Vector3(-.085f+i*.045f,-.106f,-.102f),.005f,bone,body);
    CmdLine("Banner mast",new Vector3(.09f,-.08f,.13f),new Vector3(.09f,.37f,.13f),.012f,wood,body);CmdPart("Goblin banner",new Vector3(.15f,.26f,.13f),new Vector3(.15f,.17f,.018f),Hex("C45049"),body,"robe");
   }
   if(id=="MED-196"||id=="MED-199"||id=="MED-238"||id=="MED-240"||id=="MED-202"){
    Color plate=id=="MED-238"?gold:id=="MED-199"?Hex("647484"):id=="MED-196"?bone:silver;
    CmdPart("Segmented breastplate",new Vector3(0,.045f,-.081f),new Vector3(.225f,.17f,.04f),plate,body,"plate");
    for(int i=0;i<3;i++)CmdPart("Segmented breastplate",new Vector3(0,-.047f-i*.024f,-.091f),new Vector3(.208f+i*.006f,.028f,.035f),plate,body,"plate");
    for(int s=-1;s<=1;s+=2){CmdLine("Armor edging",new Vector3(s*.095f,.12f,-.105f),new Vector3(s*.095f,-.01f,-.105f),.006f,gold,body);CmdPart("Breastplate rivet",new Vector3(s*.095f,.102f,-.108f),Vector3.one*.011f,gold,body);}
    foreach(var arm in new[]{left,right}){CmdPart("Vambrace",new Vector3(0,-.1f,-.025f),new Vector3(.085f,.085f,.09f),plate,arm);for(int i=0;i<3;i++)CmdPart("Armor rivet",new Vector3(.031f,-.067f-i*.022f,-.068f),Vector3.one*.009f,gold,arm);}
    if(id=="MED-196")CmdPart("Helmet red crest",new Vector3(0,.17f,.03f),new Vector3(.045f,.16f,.17f),Hex("A84945"),head);
    if(id=="MED-199")CmdPart("Mountain shield",new Vector3(-.035f,-.07f,-.09f),new Vector3(.2f,.26f,.055f),Hex("647484"),left);
    if(id=="MED-202")for(int i=-1;i<=1;i++)CmdPart("Memorial medallion",new Vector3(i*.067f,-.05f,-.115f),new Vector3(.03f,.044f,.014f),gold,body);
   }
   if(id=="MED-197")for(int s=-1;s<=1;s+=2)CmdPart("Bone pauldron",new Vector3(s*.14f,.12f,0),new Vector3(.15f,.1f,.15f),bone,body);
   if(id=="MED-198")for(int s=-1;s<=1;s+=2)CmdPart("Pale hair lock",new Vector3(s*.075f,-.055f,-.025f),new Vector3(.027f,.2f,.025f),silver,head);
   if(id=="MED-200")CmdPart("Ranger hood",new Vector3(0,.026f,.075f),new Vector3(.23f,.26f,.19f),Hex("456C4D"),head);
   if(id=="MED-225"){
    CmdPart("Ivory court lapel",new Vector3(-.065f,.01f,-.09f),new Vector3(.095f,.26f,.038f),bone,body);CmdPart("Midnight court lapel",new Vector3(.065f,.01f,-.09f),new Vector3(.095f,.26f,.038f),Hex("253049"),body);
   }
   if(id=="MED-226"){CmdPart("Spectral orb",new Vector3(0,-.13f,-.06f),Vector3.one*.095f,Hex("A1CDA4"),left);for(int i=0;i<5;i++)CmdPart("Dress embroidery",new Vector3((i-2)*.045f,-.19f,-.13f),new Vector3(.018f,.1f,.009f),silver,body);}
   if(id=="MED-228"){CmdPart("Woven fishing basket",new Vector3(.125f,-.12f,.02f),new Vector3(.16f,.18f,.15f),Hex("A88C60"),body,"tube");for(int i=0;i<5;i++)CmdPart("Basket weave",new Vector3(.125f,-.185f+i*.033f,-.059f),new Vector3(.15f,.008f,.008f),bone,body,"tube");}
   if(id=="MED-231"){CmdPart("Compass pendant",new Vector3(0,.07f,-.104f),new Vector3(.049f,.049f,.018f),silver,body);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;CmdLine("Steering wheel spoke",new Vector3(0,.08f,-.045f),new Vector3(Mathf.Sin(a)*.06f,.08f+Mathf.Cos(a)*.06f,-.045f),.008f,silver,right);}}
   if(id=="MED-233"){CmdPart("Carved wood mask",new Vector3(0,0,-.075f),new Vector3(.145f,.175f,.03f),wood,head);for(int s=-1;s<=1;s+=2)CmdPart("Mask eye slit",new Vector3(s*.035f,.025f,-.093f),new Vector3(.038f,.012f,.009f),dark,head);}
   if(id=="MED-236"){CmdLine("Spyglass",new Vector3(0,-.12f,-.04f),new Vector3(0,-.12f,-.19f),.039f,silver,right);CmdPart("Celestial map case",new Vector3(.11f,-.08f,.09f),new Vector3(.05f,.24f,.07f),Hex("526C9D"),body,"tube");}
   if(id=="MED-239")for(int i=0;i<3;i++)CmdPart("Cursed bottle",new Vector3(-.1f+i*.1f,-.1f,.1f),new Vector3(.045f,.08f,.035f),i==1?Hex("8C9E6C"):Hex("9381B0"),body);
   if(id=="MED-241")for(int i=0;i<7;i++)CmdPart("Coiled climbing rope",new Vector3(.13f,-.07f+i*.007f,.07f),new Vector3(.075f,.009f,.11f),bone,body,"tube");
   if(id=="MED-242"){CmdLine("Shovel handle",new Vector3(0,-.2f,0),new Vector3(0,.13f,0),.019f,wood,right);CmdPart("Shovel blade",new Vector3(0,.19f,0),new Vector3(.09f,.13f,.02f),silver,right);for(int i=0;i<3;i++)CmdLine("Crypt key",new Vector3(.08f+i*.018f,-.1f,-.11f),new Vector3(.08f+i*.018f,-.16f,-.11f),.008f,gold,body);}
   if(id=="IDEIA-004"){CmdLine("Anchor shank",new Vector3(0,.1f,-.11f),new Vector3(0,.015f,-.11f),.009f,gold,body);for(int s=-1;s<=1;s+=2)CmdLine("Anchor fluke",new Vector3(0,.015f,-.11f),new Vector3(s*.035f,.04f,-.11f),.009f,gold,body);}
  }
  void CmdFemaleHair(Transform head,Color hair,bool braided)
  {
   var cap=head.Find("Hair cap");if(cap!=null){cap.GetComponent<Renderer>().sharedMaterial=Material(hair);cap.localPosition=new Vector3(0,.078f,.025f);cap.localScale=new Vector3(.195f,.105f,.18f);}
   CmdPart("Flowing back hair",new Vector3(0,-.095f,.071f),new Vector3(.205f,.34f,.12f),hair,head);
   for(int s=-1;s<=1;s+=2)for(int i=0;i<5;i++){
    float y=.025f-i*.047f;CmdPart(braided?"Braided chestnut lock":"Layered hair lock",new Vector3(s*(.088f+Mathf.Sin(i*1.2f)*.016f),y,.005f+Mathf.Sin(i)*.014f),new Vector3(.05f,.087f,.045f),Color.Lerp(hair,Hex("BA9469"),i*.035f),head);
    if(braided&&i%2==0)CmdPart("Braid silver tie",new Vector3(s*.094f,y-.025f,-.001f),new Vector3(.038f,.011f,.036f),Hex("BEC8C9"),head);
   }
   for(int i=0;i<4;i++){var hairLock=CmdPart("Swept fringe",new Vector3(-.062f+i*.034f,.074f,-.044f),new Vector3(.055f,.065f,.035f),hair,head);hairLock.localRotation=Quaternion.Euler(0,0,-20);}
  }
  void CmdMiniatureFinish(string id,Transform body,Transform head,Transform left,Transform right,Color cloth,Color skin)
  {
   bool female=id=="MED-226"||id=="MED-231"||id=="MED-198";
   foreach(Transform child in head){
    if(child.name=="Hood"||child.name=="Ranger hood"||child.name=="Infiltrator hood"||child.name=="Prophet hood"){var p=child.localPosition;p.z=Mathf.Max(p.z,.075f);child.localPosition=p;}
    if(female&&child.name=="Face"){child.localScale=new Vector3(id=="MED-198"?.155f:id=="MED-231"?.17f:.16f,id=="MED-231"?.205f:.21f,.15f);child.GetComponent<Renderer>().sharedMaterial=Material(id=="MED-198"?Hex("D2B6A8"):id=="MED-226"?Hex("D8B293"):Hex("BF916E"));}
    if(female&&child.name=="Nose")child.localScale=new Vector3(.019f,.029f,.026f);
    if(female&&child.name=="Mouth"){child.localScale=new Vector3(.037f,.008f,.008f);child.GetComponent<Renderer>().sharedMaterial=Material(Hex("A06864"));}
   }
   if(id=="MED-231")CmdFemaleHair(head,Hex("68402F"),false);
   if(id=="MED-198"){CmdFemaleHair(head,Hex("B5BDC8"),false);var veil=head.Find("Veil");if(veil!=null){veil.localPosition=new Vector3(0,-.09f,.11f);veil.localScale=new Vector3(.25f,.36f,.06f);}}
   if(female){var torso=body.Find("Tailored torso");if(torso!=null)torso.localScale=new Vector3(.21f,.3f,.17f);
    foreach(var arm in new[]{left,right}){var sleeve=arm.Find("Sleeve");if(sleeve!=null)sleeve.localScale=new Vector3(id=="MED-226"?.115f:.08f,.18f,.09f);}
    if(id=="MED-226"){var skirt=body.Find("Robe skirt");if(skirt!=null)skirt.localScale=new Vector3(.34f,.33f,.27f);for(int s=-1;s<=1;s+=2)CmdPart("Moss dress panel",new Vector3(s*.099f,-.15f,-.081f),new Vector3(.074f,.26f,.038f),Hex("667A54"),body,"plate");}
   }
   // Seams and lapels articulate cloth without changing the skeleton or silhouette scale.
   var thread=Color.Lerp(cloth,Hex("EEE3C1"),.35f);
   CmdLine("Tunic center seam",new Vector3(0,.125f,-.095f),new Vector3(0,-.095f,-.095f),.0035f,thread,body);
   for(int s=-1;s<=1;s+=2){CmdLine("Shoulder seam",new Vector3(s*.04f,.13f,-.085f),new Vector3(s*.108f,.125f,-.086f),.004f,thread,body);
    CmdLine("Coat panel seam",new Vector3(s*.086f,.105f,-.095f),new Vector3(s*.082f,-.085f,-.095f),.003f,thread,body);
   }
   for(int i=0;i<4;i++)CmdPart("Tunic button",new Vector3(.012f,.09f-i*.041f,-.1f),Vector3.one*.01f,Hex("A89267"),body);
   if(id=="MED-227"){
    CmdLine("Goggle strap",new Vector3(-.081f,.028f,-.025f),new Vector3(.081f,.028f,-.025f),.014f,Hex("71503D"),head);
    for(int s=-1;s<=1;s+=2){CmdPart("Goggle brass rim",new Vector3(s*.042f,.025f,-.086f),new Vector3(.079f,.063f,.031f),Hex("AE8B50"),head,"tube");CmdPart("Goggle inset lens",new Vector3(s*.042f,.025f,-.104f),new Vector3(.058f,.045f,.009f),Hex("71C8C9"),head);}
   }
   if(id=="MED-193"||id=="MED-228"||id=="MED-199"){
    for(int s=-1;s<=1;s+=2)CmdLine("Age brow crease",new Vector3(s*.025f,.057f,-.089f),new Vector3(s*.053f,.057f,-.077f),.002f,Hex("9B7B67"),head);
   }
  }
  void CmdHorse(Transform root)
  {
   var fur=Hex("795442");var mane=Hex("302B29");var gold=Hex("C39F5F");var body=CmdPivot("Body",new Vector3(0,.34f,.025f),root);
   CmdPart("Horse barrel",Vector3.zero,new Vector3(.26f,.27f,.48f),fur,body);CmdLine("Horse neck",new Vector3(0,.03f,-.18f),new Vector3(0,.19f,-.27f),.15f,fur,body);
   var head=CmdPivot("Head",new Vector3(0,.21f,-.29f),body);CmdPart("Horse head",new Vector3(0,-.025f,-.055f),new Vector3(.13f,.2f,.23f),fur,head);CmdPart("Horse muzzle",new Vector3(0,-.075f,-.15f),new Vector3(.125f,.09f,.1f),Hex("A28569"),head);
   for(int s=-1;s<=1;s+=2){CmdPart("Horse ear",new Vector3(s*.044f,.099f,.005f),new Vector3(.035f,.13f,.04f),fur,head);CmdPart("Horse eye",new Vector3(s*.06f,.025f,-.075f),Vector3.one*.018f,mane,head);for(int z=-1;z<=1;z+=2){var leg=CmdPivot((z<0?"Front":"Back")+(s<0?"LeftLeg":"RightLeg"),new Vector3(s*.078f,-.075f,z*.165f),body);CmdLine("Horse leg",Vector3.zero,new Vector3(0,-.2f,0),.052f,fur,leg);CmdPart("Hoof",new Vector3(0,-.235f,-.01f),new Vector3(.065f,.06f,.085f),mane,leg);}}
   for(int i=0;i<8;i++)CmdPart("Mane lock",new Vector3(0,.08f+i*.019f,-.14f-i*.01f),new Vector3(.06f,.08f,.055f),mane,body);
   var tail=CmdPivot("Tail",new Vector3(0,.05f,.25f),body);CmdLine("Horse tail",Vector3.zero,new Vector3(0,-.23f,.09f),.065f,mane,tail);
   CmdPart("Decorated saddle blanket",new Vector3(0,.14f,.02f),new Vector3(.3f,.09f,.27f),Hex("42777B"),body);CmdPart("Saddle",new Vector3(0,.175f,.025f),new Vector3(.19f,.055f,.23f),Hex("573C2D"),body);
   for(int s=-1;s<=1;s+=2){CmdLine("Bridle",new Vector3(s*.065f,.045f,-.08f),new Vector3(s*.06f,-.055f,-.15f),.013f,gold,head);CmdLine("Rein",new Vector3(s*.055f,.12f,-.15f),new Vector3(s*.09f,.27f,.02f),.009f,Hex("573C2D"),body);}
  }
  void CmdBear(Transform root,Color fur,Color claw)
  {
   var body=CmdPivot("Body",new Vector3(0,.28f,.02f),root);CmdPart("Bear barrel",Vector3.zero,new Vector3(.36f,.37f,.58f),fur,body);CmdPart("Shoulder hump",new Vector3(0,.13f,-.13f),new Vector3(.37f,.3f,.32f),fur,body);
   var head=CmdPivot("Head",new Vector3(0,.08f,-.29f),body);CmdPart("Bear head",Vector3.zero,new Vector3(.28f,.27f,.25f),fur,head);CmdPart("Muzzle",new Vector3(0,-.04f,-.12f),new Vector3(.18f,.12f,.14f),claw,head);CmdPart("Nose",new Vector3(0,-.01f,-.2f),new Vector3(.07f,.04f,.025f),Hex("25252A"),head);
   for(int s=-1;s<=1;s+=2){CmdPart("Round ear",new Vector3(s*.105f,.115f,.015f),Vector3.one*.08f,fur,head);CmdPart("Eye",new Vector3(s*.074f,.027f,-.112f),Vector3.one*.018f,Hex("22242A"),head);for(int z=-1;z<=1;z+=2){var leg=CmdPivot((z<0?"Front":"Back")+(s<0?"LeftLeg":"RightLeg"),new Vector3(s*.115f,-.08f,z*.18f),body);CmdPart("Fur leg",new Vector3(0,-.07f,0),new Vector3(.115f,.23f,.12f),fur,leg);CmdPart("Paw",new Vector3(0,-.165f,-.03f),new Vector3(.14f,.07f,.16f),fur,leg);for(int i=-1;i<=1;i++)CmdPart("Claw",new Vector3(i*.03f,-.17f,-.1f),new Vector3(.012f,.016f,.055f),claw,leg);}}
   var tail=CmdPivot("Tail",new Vector3(0,.015f,.29f),body);CmdPart("Short tail",Vector3.zero,Vector3.one*.085f,fur,tail);
   CmdPart("Blue guardian cloth",new Vector3(0,.17f,-.14f),new Vector3(.32f,.05f,.21f),Hex("58839F"),body);CmdPart("Guardian sun medallion",new Vector3(0,.08f,-.24f),new Vector3(.075f,.075f,.022f),Hex("DDBA69"),body);
  }
  void CmdTree(Transform root)
  {
   var bark=Hex("76604B");var moss=Hex("63824D");var body=CmdPivot("Body",new Vector3(0,.36f,0),root);CmdPart("Ancient trunk",Vector3.zero,new Vector3(.32f,.58f,.24f),bark,body,"robe");
   for(int s=-1;s<=1;s+=2){var leg=CmdPivot(s<0?"LeftLeg":"RightLeg",new Vector3(s*.1f,-.2f,0),body);CmdLine("Root leg",Vector3.zero,new Vector3(s*.04f,-.11f,-.06f),.105f,bark,leg);for(int i=0;i<3;i++)CmdLine("Ground root",new Vector3(0,-.11f,0),new Vector3(s*(.08f+i*.025f),-.12f,-.12f+i*.09f),.035f,bark,leg);var arm=CmdPivot(s<0?"LeftArm":"RightArm",new Vector3(s*.14f,.13f,0),body);CmdLine("Branch arm",Vector3.zero,new Vector3(s*.13f,-.16f,0),.07f,bark,arm);for(int i=0;i<3;i++)CmdLine("Twig finger",new Vector3(s*.13f,-.16f,0),new Vector3(s*(.15f+i*.015f),-.25f,.025f*i),.017f,bark,arm);}
   var head=CmdPivot("Head",new Vector3(0,.23f,0),body);CmdPart("Tree face",Vector3.zero,new Vector3(.24f,.23f,.22f),bark,head);for(int s=-1;s<=1;s+=2)CmdPart("Amber eye",new Vector3(s*.05f,.025f,-.105f),new Vector3(.028f,.018f,.012f),Hex("E5BE68"),head);
   for(int i=0;i<7;i++){float a=i*Mathf.PI*2/7;var end=new Vector3(Mathf.Sin(a)*.2f,.17f+Mathf.Cos(a)*.04f,Mathf.Cos(a)*.16f);CmdLine("Crown branch",new Vector3(0,.07f,0),end,.024f,bark,head);CmdPart("Leaf canopy",end,new Vector3(.19f,.13f,.16f),moss,head);}for(int i=0;i<6;i++)CmdPart("Moss knot",new Vector3(Mathf.Sin(i*2)*.14f,-.16f+i*.05f,-.1f),Vector3.one*.06f,moss,body);
  }
  void CmdAbyss(Transform root)
  {
   var flesh=Hex("364352");var body=CmdPivot("Body",new Vector3(0,.34f,0),root);CmdPart("Abyss body",Vector3.zero,new Vector3(.43f,.47f,.33f),flesh,body);var head=CmdPivot("Head",new Vector3(0,.16f,-.11f),body);CmdPart("Hungry maw",Vector3.zero,new Vector3(.28f,.29f,.18f),Hex("171621"),head);
   for(int i=0;i<12;i++){float a=i*Mathf.PI*2/12;var tooth=CmdPart("Maw tooth",new Vector3(Mathf.Sin(a)*.11f,Mathf.Cos(a)*.12f,-.09f),new Vector3(.025f,.065f,.027f),Hex("CBBFC1"),head,"cone");tooth.localRotation=Quaternion.Euler(0,0,-a*Mathf.Rad2Deg+180);}
   for(int i=0;i<6;i++){float a=i*Mathf.PI*2/6;string name=i==0?"LeftArm":i==1?"RightArm":i==2?"LeftLeg":i==3?"RightLeg":i==4?"Tail":"Tentacle";var pivot=CmdPivot(name,new Vector3(Mathf.Cos(a)*.15f,Mathf.Sin(a)*.1f,0),body);var prev=Vector3.zero;for(int n=1;n<=7;n++){var next=new Vector3(Mathf.Cos(a)*n*.025f,Mathf.Sin(a)*n*.02f-.12f*(n/7f),Mathf.Sin(n*.5f+i)*.06f);CmdLine("Smooth tentacle",prev,next,.05f*(1-n/9f),flesh,pivot);prev=next;}}
   for(int i=0;i<5;i++)CmdPart("Watchful eye",new Vector3((i-2)*.065f,.17f+Mathf.Abs(i-2)*.018f,-.145f),new Vector3(.043f,.025f,.018f),Hex("D995D7"),body);
  }
 }
}
