using System;
using System.Linq;
using UnityEngine;
using TCG.Foundation;

namespace TCG.Table
{
    public sealed partial class TableWorld
    {
        // Presentation profiles only: classification never grants a subtype or ability.
        bool TryTroopFigure(Definition card,Color owner,Transform parent,out Transform root)
        {
            root=null;
            if(card.Kind!=CardType.Creature||card.IsCommander||card.IsVehicle||
               card.Art=="beast"||card.Art=="serpent"||card.Art=="winged")return false;
            bool goblin=card.Subtypes.Any(s=>s.Equals("Goblin",StringComparison.OrdinalIgnoreCase));
            bool skeleton=card.Subtypes.Any(s=>s.Equals("Esqueleto",StringComparison.OrdinalIgnoreCase));
            bool mage=card.Art=="mage",archer=card.Art=="archer",giant=card.Art=="giant";
            int variant=0;foreach(char c in card.Id)variant=(variant*31+c)%997;
            var cloth=Color.Lerp(Elements[Mathf.Clamp(card.Color,0,6)],Hex("403832"),.36f+variant%3*.06f);
            var leather=Hex("684631");var iron=Hex("AEBBC0");var gold=Hex("D5B170");var dark=Hex("263139");
            var skin=goblin?Color.Lerp(Hex("6E9051"),Hex("9BAB66"),(variant%7)/7f):skeleton?Hex("E2D4AE"):Color.Lerp(Hex("B77E60"),Hex("E1BA93"),(variant%7)/7f);
            root=CmdPivot("Troop "+card.Id+" — "+card.Name,Vector3.zero,parent);
            CmdPart("OwnerPlate",new Vector3(0,.025f,0),new Vector3(.43f,.05f,.43f),owner,root,"tube");
            var body=CmdPivot("Body",new Vector3(0,goblin?.32f:.35f,0),root);
            var head=CmdPivot("Head",new Vector3(0,goblin?.22f:.26f,-.005f),body);
            CmdPart("Tailored tunic",Vector3.zero,new Vector3(goblin?.24f:.25f,.27f,.17f),cloth,body,"plate");
            CmdPart("Leather belt",new Vector3(0,-.09f,0),new Vector3(.26f,.035f,.185f),leather,body,"tube");
            CmdPart("Buckle",new Vector3(0,-.09f,-.1f),new Vector3(.047f,.032f,.018f),gold,body,"plate");
            CmdPart("Face",Vector3.zero,new Vector3(goblin?.2f:.155f,.19f,.15f),skin,head);
            CmdPart("Nose",new Vector3(0,-.009f,-.088f),new Vector3(goblin?.048f:.03f,.044f,goblin?.07f:.035f),skin,head);
            var jaw=CmdPivot("Jaw",new Vector3(0,-.05f,-.059f),head);
            CmdPart("Lower jaw",Vector3.zero,new Vector3(.08f,.036f,.068f),skin,jaw);
            CmdPart("Mouth line",new Vector3(0,.005f,-.036f),new Vector3(.054f,.008f,.006f),dark,jaw,"plate");
            for(int side=-1;side<=1;side+=2)
            {
                CmdPart("Ear",new Vector3(side*(goblin?.12f:.079f),.012f,0),new Vector3(goblin?.14f:.032f,.06f,.042f),skin,head);
                CmdPart("Eye",new Vector3(side*.038f,.028f,-.071f),new Vector3(.023f,.018f,.013f),Hex("EDE6CD"),head);
                CmdPart("Pupil",new Vector3(side*.038f,.028f,-.079f),new Vector3(.009f,.011f,.006f),dark,head);
                var lid=CmdPivot("Eyelid",new Vector3(side*.038f,.041f,-.079f),head);
                CmdPart("Lid surface",Vector3.zero,new Vector3(.026f,.007f,.009f),skin,lid);
                CmdLine("Brow",new Vector3(side*.024f,.055f,-.073f),new Vector3(side*.055f,.05f,-.068f),.007f,dark,head);
                if(goblin)CmdPart("Small tusk",new Vector3(side*.027f,-.039f,-.099f),new Vector3(.018f,.035f,.016f),Hex("E4D8AF"),head,"cone");
                var arm=CmdPivot(side<0?"LeftArm":"RightArm",new Vector3(side*.14f,.09f,0),body);
                CmdPart("Sleeve",new Vector3(0,-.075f,0),new Vector3(.085f,.17f,.09f),cloth,arm);
                CmdPart("Forearm bracer",new Vector3(0,-.132f,0),new Vector3(.091f,.065f,.098f),leather,arm,"plate");
                CmdPart("Hand",new Vector3(0,-.188f,-.006f),new Vector3(.064f,.077f,.07f),skin,arm);
                for(int finger=0;finger<3;finger++)CmdPart("Finger",new Vector3((finger-1)*.017f,-.215f,-.035f),new Vector3(.014f,.028f,.017f),skin,arm);
                var leg=CmdPivot(side<0?"LeftLeg":"RightLeg",new Vector3(side*.066f,-.112f,0),body);
                CmdPart("Trouser",new Vector3(0,-.061f,0),new Vector3(.078f,.145f,.086f),dark,leg);
                CmdPart("Knee pad",new Vector3(0,-.052f,-.045f),new Vector3(.077f,.063f,.024f),leather,leg,"plate");
                CmdPart("Boot",new Vector3(0,-.143f,-.02f),new Vector3(.1f,.104f,.14f),leather,leg,"plate");
                CmdPart("Boot sole",new Vector3(0,-.19f,-.02f),new Vector3(.105f,.018f,.147f),dark,leg,"plate");
                if(!mage)CmdPart("Shoulder plate",new Vector3(0,.004f,0),new Vector3(.12f,.06f,.12f),iron,arm,"plate");
            }
            var left=body.Find("LeftArm");var right=body.Find("RightArm");
            var cape=CmdPivot("Cape",new Vector3(0,.11f,.087f),body);
            CmdPart("Cloth mantle",new Vector3(0,-.14f,.026f),new Vector3(.25f,.34f,.055f),Color.Lerp(cloth,dark,.25f),cape,"robe");
            for(int fold=-2;fold<=2;fold++)CmdLine("Mantle seam",new Vector3(fold*.028f,-.02f,.062f),new Vector3(fold*.044f,-.28f,.062f),.008f,cloth,cape);
            var pouch=CmdPivot("Satchel",new Vector3(.12f,-.095f,.088f),body);
            CmdPart("Stitched pouch",Vector3.zero,new Vector3(.075f,.095f,.047f),leather,pouch,"plate");
            for(int stitch=-2;stitch<=2;stitch++)CmdPart("Pouch stitch",new Vector3(stitch*.012f,.025f,.026f),new Vector3(.006f,.003f,.003f),gold,pouch);
            if(mage)
            {
                CmdPart("Robe skirt",new Vector3(0,-.13f,0),new Vector3(.29f,.27f,.24f),cloth,body,"robe");
                CmdPart("Hood",new Vector3(0,.03f,.035f),new Vector3(.22f,.23f,.18f),cloth,head);
                CmdBook(left,leather);CmdStaff(right,leather,Elements[card.Color]);
            }
            else if(archer)
            {
                CmdBow(left,leather);
                CmdLine("Readied arrow",new Vector3(0,-.16f,-.09f),new Vector3(0,-.16f,-.34f),.009f,gold,right);
                CmdPart("Arrowhead",new Vector3(0,-.16f,-.35f),new Vector3(.026f,.025f,.044f),iron,right,"cone");
                CmdPart("Quiver",new Vector3(.13f,.045f,.113f),new Vector3(.09f,.21f,.075f),leather,body,"tube");
                for(int arrow=0;arrow<3;arrow++)CmdLine("Spare arrow",new Vector3(.1f+arrow*.025f,.045f,.11f),new Vector3(.1f+arrow*.025f,.23f,.11f),.009f,gold,body);
                CmdPart("Leather cap",new Vector3(0,.077f,.015f),new Vector3(.19f,.09f,.17f),cloth,head);
            }
            else if(goblin)
            {
                bool sapper=card.Name.IndexOf("bomb",StringComparison.OrdinalIgnoreCase)>=0||card.Name.IndexOf("explos",StringComparison.OrdinalIgnoreCase)>=0;
                if(GoblinPreconProps(card.Id,body,head,left,right,leather,iron,gold)){}
                else if(sapper){CmdPart("Powder flask",new Vector3(0,-.16f,-.075f),new Vector3(.11f,.14f,.11f),dark,right);CmdLine("Curled fuse",new Vector3(0,-.08f,-.075f),new Vector3(.025f,-.045f,-.075f),.014f,gold,right);}
                else{CmdLine("Club handle",new Vector3(0,-.25f,-.035f),new Vector3(0,.12f,-.035f),.034f,leather,right);CmdPart("Iron club head",new Vector3(0,.15f,-.035f),new Vector3(.11f,.13f,.1f),iron,right,"plate");}
                CmdPart("Scrap buckler",new Vector3(0,-.1f,-.08f),new Vector3(.16f,.19f,.04f),iron,left,"plate");
                CmdPart("Patch",new Vector3(-.063f,.019f,-.091f),new Vector3(.047f,.065f,.014f),leather,body,"plate");
            }
            else
            {
                CmdBlade(right,iron);
                CmdPart("Round shield",new Vector3(0,-.1f,-.072f),new Vector3(.21f,.25f,.047f),cloth,left);
                CmdPart("Shield boss",new Vector3(0,-.1f,-.106f),new Vector3(.058f,.058f,.034f),iron,left);
                CmdPart("Helmet",new Vector3(0,.065f,.022f),new Vector3(.19f,.11f,.18f),iron,head);
                CmdPart("Helmet rim",new Vector3(0,.035f,0),new Vector3(.205f,.017f,.19f),gold,head,"tube");
                if(card.Art=="royal")CmdCrown(head,gold,3);
            }
            if(skeleton)
            {
                for(int rib=0;rib<4;rib++)CmdPart("Ivory rib",new Vector3(0,-.065f+rib*.035f,-.094f),new Vector3(.18f,.015f,.016f),skin,body);
                CmdPart("Dark nasal socket",new Vector3(0,0,-.111f),new Vector3(.02f,.032f,.008f),dark,head);
            }
            if(giant){body.localScale=Vector3.one*1.22f;body.localPosition=new Vector3(0,.41f,0);}
            return true;
        }

        bool GoblinPreconProps(string id,Transform body,Transform head,Transform left,Transform right,Color wood,Color iron,Color brass)
        {
            if(id=="ED261007-03-C001")
            {
                var previous=new Vector3(0,-.17f,-.04f);
                for(int i=1;i<=7;i++){var point=new Vector3(Mathf.Sin(i*.18f)*.11f,-.17f+i*.014f,-.04f-i*.031f);CmdLine("Crooked horn tube",previous,point,.026f+i*.003f,brass,right);previous=point;}
                var bell=CmdPart("Trumpet bell",previous,new Vector3(.15f,.14f,.15f),brass,right,"cone");bell.localRotation=Quaternion.Euler(90,0,0);
                CmdPart("Horn dark opening",previous+new Vector3(0,0,-.053f),new Vector3(.12f,.012f,.12f),Hex("4E3F24"),right,"tube").localRotation=Quaternion.Euler(90,0,0);
                return true;
            }
            if(id=="ED261007-03-C002")
            {
                for(int i=-1;i<=1;i++){var log=CmdPart("Carried firewood",new Vector3(i*.07f,-.03f,.17f),new Vector3(.063f,.39f,.063f),wood,body,"tube");log.localRotation=Quaternion.Euler(90,0,i*4);CmdPart("Cut log end",new Vector3(i*.07f,-.03f,-.026f),new Vector3(.064f,.012f,.064f),Hex("C5A271"),body,"tube").localRotation=Quaternion.Euler(90,0,0);}
                CmdLine("Carrying strap",new Vector3(-.12f,.09f,-.093f),new Vector3(.11f,-.1f,-.1f),.02f,wood,body);
                CmdLine("Wood axe handle",new Vector3(0,-.25f,0),new Vector3(0,.06f,0),.025f,wood,right);CmdPart("Axe head",new Vector3(.033f,.07f,0),new Vector3(.11f,.065f,.035f),iron,right,"plate");return true;
            }
            if(id=="ED261007-03-C003")
            {
                CmdPart("Cook apron",new Vector3(0,-.033f,-.105f),new Vector3(.19f,.23f,.02f),Hex("D8C7A0"),body,"plate");
                CmdPart("Cook cap",new Vector3(0,.098f,.015f),new Vector3(.23f,.13f,.2f),Hex("E4D5B8"),head);
                CmdLine("Pan handle",new Vector3(0,-.2f,-.02f),new Vector3(0,-.2f,-.16f),.022f,wood,right);
                CmdPart("Iron cooking pan",new Vector3(0,-.2f,-.23f),new Vector3(.19f,.04f,.19f),iron,right,"tube");
                CmdPart("Stew",new Vector3(0,-.179f,-.23f),new Vector3(.155f,.015f,.155f),Hex("94513C"),right,"tube");return true;
            }
            if(id=="ED261007-03-C004")
            {
                CmdPart("Oversized helmet",new Vector3(0,.057f,.026f),new Vector3(.3f,.17f,.255f),iron,head);
                CmdPart("Huge helmet rim",new Vector3(0,.031f,.005f),new Vector3(.31f,.023f,.26f),brass,head,"tube");
                CmdBlade(right,iron,true);return true;
            }
            if(id=="ED261007-03-C005")
            {
                CmdPart("Caravan meat sack",new Vector3(-.09f,-.04f,.13f),new Vector3(.15f,.19f,.12f),wood,body);
                CmdPart("Caravan chief scarf",new Vector3(0,.123f,-.02f),new Vector3(.22f,.035f,.18f),Hex("A8503D"),body,"tube");
                CmdLine("Cleaver handle",new Vector3(0,-.22f,0),new Vector3(0,-.12f,0),.028f,wood,right);
                CmdPart("Meat cleaver",new Vector3(.016f,-.025f,0),new Vector3(.115f,.15f,.032f),iron,right,"plate");return true;
            }
            return false;
        }
    }
}
