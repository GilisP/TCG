using UnityEngine;

namespace TCG.Table
{
    public sealed partial class TableWorld
    {
        // Presentation only: coordinates retain the seated player's hand and reaction anchors.
        void BuildStyledPlayer(int player,Transform place,Color skin)
        {
            var cloth=Seats[player]*.64f;cloth.a=1;
            var wood=Hex("453128");var trim=Hex("B69A64");var dark=Hex("29313B");
            var hair=Hex(new[]{"3D2D27","674534","C9A777","262931"}[player]);
            var chair=Group("Cadeira entalhada",place);
            CmdPart("Assento",new Vector3(0,-1.8f,8.8f),new Vector3(1.65f,.3f,1.6f),wood,chair,"plate");
            CmdPart("Almofada do assento",new Vector3(0,-1.61f,8.8f),new Vector3(1.4f,.18f,1.35f),cloth*.7f,chair,"plate");
            CmdPart("Encosto entalhado",new Vector3(0,-.5f,9.45f),new Vector3(1.7f,2.8f,.25f),wood,chair,"plate");
            CmdPart("Almofada do encosto",new Vector3(0,-.6f,9.27f),new Vector3(1.3f,2,.14f),cloth*.7f,chair,"plate");
            for(int side=-1;side<=1;side+=2)
            {
                for(int rear=0;rear<2;rear++)
                    CmdLine("Pé torneado",new Vector3(side*.68f,-3,8.2f+rear*1.1f),new Vector3(side*.68f,-1.75f,8.2f+rear*1.1f),.16f,wood,chair);
                CmdLine("Moldura dourada",new Vector3(side*.74f,-1.7f,9.25f),new Vector3(side*.74f,.82f,9.25f),.065f,trim,chair);
                CmdPart("Florão",new Vector3(side*.74f,.9f,9.35f),Vector3.one*.22f,trim,chair);
                CmdLine("Apoio do braço",new Vector3(side*.8f,-1.8f,8.25f),new Vector3(side*.8f,-.9f,8.25f),.12f,wood,chair);
                CmdPart("Braço da cadeira",new Vector3(side*.8f,-.85f,8.7f),new Vector3(.23f,.15f,1.2f),wood,chair,"plate");
            }
            for(int row=0;row<3;row++)for(int col=-1;col<=1;col++)
                CmdPart("Botão do estofado",new Vector3(col*.37f,-1.15f+row*.55f,9.175f),Vector3.one*.065f,trim,chair);
            CmdLine("Coroamento",new Vector3(-.78f,.84f,9.3f),new Vector3(.78f,.84f,9.3f),.09f,trim,chair);

            var body=Group("Jogador • modelo suave",place);
            CmdPart("Túnica ajustada",new Vector3(0,-.66f,8.8f),new Vector3(1.06f,1.77f,.65f),cloth,body,"plate");
            CmdPart("Saia da túnica",new Vector3(0,-1.45f,8.65f),new Vector3(1.12f,.45f,.9f),cloth,body,"robe");
            CmdPart("Cinto de couro",new Vector3(0,-1.2f,8.8f),new Vector3(1.1f,.14f,.68f),wood,body,"tube");
            CmdPart("Fivela",new Vector3(0,-1.2f,8.43f),new Vector3(.18f,.16f,.06f),trim,body,"plate");
            for(int side=-1;side<=1;side+=2)
            {
                CmdLine("Coxa sentada",new Vector3(side*.32f,-1.65f,8.75f),new Vector3(side*.34f,-1.8f,8.05f),.41f,dark,body);
                CmdLine("Perna da calça",new Vector3(side*.34f,-1.85f,8.08f),new Vector3(side*.34f,-2.6f,8.13f),.36f,dark,body);
                CmdPart("Bota de couro",new Vector3(side*.34f,-2.74f,7.98f),new Vector3(.44f,.32f,.74f),wood,body,"plate");
                CmdPart("Ombro",new Vector3(side*.52f,-.18f,8.75f),new Vector3(.43f,.48f,.51f),cloth,body);
                CmdLine("Manga",new Vector3(side*.56f,-.25f,8.72f),new Vector3(side*.65f,-.48f,8.28f),.35f,cloth,body);
                CmdLine("Braço sobre a mesa",new Vector3(side*.65f,-.43f,8.3f),new Vector3(side*.62f,-.29f,7.6f),.29f,cloth,body);
                CmdPart("Punho bordado",new Vector3(side*.62f,-.28f,7.63f),new Vector3(.3f,.24f,.18f),trim,body,"plate");
                CmdPart("Mão",new Vector3(side*.62f,-.28f,7.43f),new Vector3(.27f,.17f,.34f),skin,body);
                for(int finger=0;finger<4;finger++)
                    CmdLine("Dedo da mão",new Vector3(side*.62f+(finger-1.5f)*.052f,-.29f,7.36f),new Vector3(side*.62f+(finger-1.5f)*.052f,-.3f,7.24f),.045f,skin,body);
                CmdPart("Orelha",new Vector3(side*.305f,.72f,8.7f),new Vector3(.13f,.21f,.14f),skin,body);
                CmdPart("Olho",new Vector3(side*.13f,.77f,8.423f),new Vector3(.075f,.048f,.035f),Hex("E4DBCA"),body);
                CmdPart("Íris",new Vector3(side*.13f,.77f,8.402f),new Vector3(.032f,.036f,.018f),dark,body);
                CmdLine("Sobrancelha",new Vector3(side*.07f,.86f,8.432f),new Vector3(side*.2f,.85f,8.457f),.024f,hair,body);
            }
            CmdPart("Pescoço",new Vector3(0,.28f,8.73f),new Vector3(.32f,.38f,.34f),skin,body);
            CmdPart("Rosto",new Vector3(0,.7f,8.7f),new Vector3(.61f,.74f,.57f),skin,body);
            CmdPart("Nariz",new Vector3(0,.7f,8.391f),new Vector3(.095f,.14f,.12f),skin,body);
            CmdPart("Boca",new Vector3(0,.55f,8.437f),new Vector3(.13f,.018f,.028f),Hex("885B52"),body);
            CmdPart("Cabelo",new Vector3(0,1.01f,8.79f),new Vector3(.68f,.29f,.61f),hair,body);
            for(int strand=-2;strand<=2;strand++)
                CmdLine("Mecha",new Vector3(strand*.105f,1.06f,8.57f),new Vector3(strand*.11f,.92f,8.485f),.09f,hair,body);
            if(player==0||player==3)
            {
                CmdPart("Aba do chapéu",new Vector3(0,1.1f,8.77f),new Vector3(.92f,.075f,.84f),cloth,body,"tube");
                CmdPart("Chapéu de viajante",new Vector3(.05f,1.36f,8.8f),new Vector3(.64f,.6f,.57f),cloth,body,"cone");
                CmdPart("Faixa do chapéu",new Vector3(0,1.16f,8.78f),new Vector3(.59f,.095f,.54f),trim,body,"tube");
            }
            for(int button=0;button<4;button++)
                CmdPart("Botão da túnica",new Vector3(.1f,-.2f-button*.2f,8.465f),Vector3.one*.055f,trim,body);
            // This existing helper owns the private card fan and its interaction-compatible anchor.
            BuildHeldCards(player,place,skin);
        }

        void StyledRoomDetails(Transform root,int room)
        {
            var wood=Hex("463127");var gold=Hex("B69A64");
            for(int side=0;side<4;side++)
            {
                var wall=Group("Detalhes artesanais "+side,root);wall.localRotation=Quaternion.Euler(0,side*90,0);
                for(int x=-10;x<=10;x+=5)
                {
                    CmdPart("Base da coluna",new Vector3(x,-2.8f,12.5f),new Vector3(.85f,.25f,.9f),gold,wall,"plate");
                    if(room==0)
                    {
                        CmdPart("Medalhão do estandarte",new Vector3(x+1.5f,.3f,12.46f),new Vector3(.55f,.6f,.08f),gold,wall);
                        for(int tassel=-1;tassel<=1;tassel++)CmdLine("Franja",new Vector3(x+1.5f+tassel*.3f,-1.2f,12.5f),new Vector3(x+1.5f+tassel*.3f,-1.47f,12.5f),.045f,gold,wall);
                    }
                    else for(int bottle=0;bottle<4;bottle++)
                    {
                        var glass=Hex(bottle%2==0?"50684B":"9B7044");
                        CmdPart("Garrafa arredondada",new Vector3(x+1+bottle*.45f,-.56f,12),new Vector3(.22f,.43f,.22f),glass,wall);
                        CmdLine("Gargalo",new Vector3(x+1+bottle*.45f,-.37f,12),new Vector3(x+1+bottle*.45f,-.19f,12),.075f,glass,wall);
                        CmdPart("Rolha",new Vector3(x+1+bottle*.45f,-.17f,12),new Vector3(.085f,.045f,.085f),wood,wall,"tube");
                    }
                }
            }
            if(room==1)for(int log=0;log<3;log++)
                CmdLine("Lenha",new Vector3(-1f,-2.67f+log*.13f,11.1f+log*.1f),new Vector3(1f,-2.67f+log*.13f,11.3f-log*.1f),.19f,wood,root);
        }
    }
}
