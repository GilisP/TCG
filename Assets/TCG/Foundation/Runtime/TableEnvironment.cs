using System.Collections.Generic;
using UnityEngine;

namespace TCG.Table
{
    public sealed class PileHit:MonoBehaviour { public int Player, Pile; }

    public sealed partial class TableWorld
    {
        readonly Transform[] rooms=new Transform[2],places=new Transform[4];
        readonly List<DeckDisplay> decks=new List<DeckDisplay>();
        sealed class DeckDisplay
        {
            public int Player,Kind; public Transform Root,Stack; public TextMesh Label;
        }
        public int RoomIndex {get;private set;}
        public string RoomName=>RoomIndex==0?"Salão dos Reinos":"Taverna do Carvalho";
        public void SetRoom(int index)
        {
            RoomIndex=Mathf.Clamp(index,0,1);
            for(int i=0;i<rooms.Length;i++)if(rooms[i]!=null)rooms[i].gameObject.SetActive(i==RoomIndex);
        }
        Transform Group(string name,Transform parent)
        {var t=new GameObject(name).transform;t.SetParent(parent,false);return t;}
        void BuildRooms()
        {
            for(int r=0;r<2;r++)
            {
                var root=rooms[r]=Group(r==0?"Ambiente • Salão dos Reinos":"Ambiente • Taverna do Carvalho",transform);
                Color stone=Hex(r==0?"53636A":"514137"),wood=Hex("342A24"),gold=Hex("AD8850");
                Block("Piso",new Vector3(0,-3.2f,0),new Vector3(29,.2f,29),Hex(r==0?"303D43":"514032"),root);
                for(int n=-14;n<=14;n++)
                    Block("Junta do piso",new Vector3(n,-3.085f,0),new Vector3(.025f,.012f,28),wood,root);
                for(int side=0;side<4;side++)
                {
                    var wall=Group("Ala "+side,root);wall.localRotation=Quaternion.Euler(0,side*90,0);
                    Block("Parede baixa",new Vector3(0,-1.9f,13),new Vector3(26,2.4f,.5f),stone,wall);
                    Block("Rodapé",new Vector3(0,-2.8f,12.65f),new Vector3(26,.3f,.25f),gold,wall);
                    for(int x=-10;x<=10;x+=5)
                    {
                        Block(r==0?"Coluna de pedra":"Viga de carvalho",new Vector3(x,-.5f,12.5f),new Vector3(.6f,5,.7f),r==0?stone*1.3f:wood,wall);
                        Block("Capitel",new Vector3(x,2,12.5f),new Vector3(.9f,.25f,1),gold,wall);
                        if(r==0)
                        {
                            Block("Estandarte",new Vector3(x+1.5f,.2f,12.55f),new Vector3(1.1f,2.9f,.05f),Seats[side]*.65f,wall);
                            Block("Símbolo dourado",new Vector3(x+1.5f,.25f,12.49f),new Vector3(.22f,1.5f,.04f),gold,wall);
                        }
                        else
                        {
                            Block("Prateleira",new Vector3(x+1.8f,-.9f,12),new Vector3(2.4f,.18f,.75f),wood,wall);
                            for(int b=0;b<4;b++)Block("Garrafa",new Vector3(x+1+b*.45f,-.55f,12),new Vector3(.18f,.55f,.2f),Hex(b%2==0?"50684B":"9B7044"),wall);
                        }
                    }
                }
                if(r==1)
                {
                    Block("Lareira",new Vector3(0,-1.3f,11.9f),new Vector3(3.5f,3.6f,1.2f),stone,root);
                    Block("Interior da lareira",new Vector3(0,-1.8f,11.25f),new Vector3(2.5f,2,.08f),Hex("191714"),root);
                    for(int n=0;n<5;n++)Cone(new Vector3(-.8f+n*.4f,-2.1f,11),.28f,1.1f,Hex(n%2==0?"EFA448":"CB633A"),root,5);
                    var fire=Group("Luz da lareira",root);fire.localPosition=new Vector3(0,-1,10.8f);var l=fire.gameObject.AddComponent<Light>();l.type=LightType.Point;l.range=8;l.intensity=2;l.color=Hex("FFB865");
                }
            }
            SetRoom(0);
        }
        void BuildPlaces()
        {
            for(int p=0;p<4;p++)
            {
                var g=places[p]=Group("Jogador "+(p+1),transform);g.localRotation=Quaternion.Euler(0,270-p*90,0);
                var avatar=Group("Avatar • interação",g);avatar.localPosition=new Vector3(0,-.1f,8.75f);
                var avatarHit=avatar.gameObject.AddComponent<BoxCollider>();avatarHit.center=new Vector3(0,.4f,0);avatarHit.size=new Vector3(1.55f,2.8f,1.05f);avatar.gameObject.AddComponent<PlayerAvatarHit>().Player=p;
                var tint=Seats[p];var wood=Hex("3F3029");var skin=Hex(new[]{"C89470","B77F5E","D9AB85","8C624A"}[p]);
                Block("Assento",new Vector3(0,-1.8f,8.8f),new Vector3(1.65f,.3f,1.6f),wood,g);
                Block("Encosto entalhado",new Vector3(0,-.5f,9.45f),new Vector3(1.7f,2.8f,.25f),wood,g);
                Block("Almofada",new Vector3(0,-.6f,9.27f),new Vector3(1.3f,2,.12f),tint*.5f,g);
                for(int x=-1;x<=1;x+=2)
                {
                    Block("Perna da cadeira",new Vector3(x*.68f,-2.5f,9.3f),new Vector3(.15f,1.3f,.15f),wood,g);
                    Block("Calça",new Vector3(x*.34f,-2,8.4f),new Vector3(.4f,1.2f,.45f),Hex("29333A"),g);
                    Block("Bota",new Vector3(x*.34f,-2.7f,8.2f),new Vector3(.45f,.3f,.75f),wood,g);
                    Block("Manga",new Vector3(x*.64f,-.65f,8.6f),new Vector3(.35f,.95f,.42f),tint*.65f,g);
                    Block("Braço sobre a mesa",new Vector3(x*.65f,-.33f,7.95f),new Vector3(.32f,.28f,1.1f),tint*.8f,g);
                    Block("Mão",new Vector3(x*.65f,-.3f,7.42f),new Vector3(.28f,.17f,.34f),skin,g);
                }
                Block("Túnica",new Vector3(0,-.7f,8.8f),new Vector3(1.05f,1.8f,.65f),tint*.65f,g);
                Block("Cinto",new Vector3(0,-1.23f,8.43f),new Vector3(1.1f,.15f,.12f),wood,g);
                Block("Fivela",new Vector3(0,-1.23f,8.34f),new Vector3(.2f,.18f,.07f),Hex("C6A669"),g);
                Block("Pescoço",new Vector3(0,.28f,8.73f),new Vector3(.32f,.35f,.35f),skin,g);
                Block("Rosto",new Vector3(0,.7f,8.7f),new Vector3(.62f,.72f,.58f),skin,g);
                Block("Cabelo",new Vector3(0,1.07f,8.77f),new Vector3(.68f,.22f,.65f),Hex(p%2==0?"3C302C":"C0A17A"),g);
                for(int x=-1;x<=1;x+=2)Block("Olho",new Vector3(x*.14f,.76f,8.399f),new Vector3(.065f,.065f,.025f),Hex("252628"),g);
                if(p==0||p==3)Cone(new Vector3(0,1.38f,8.77f),.48f,.55f,tint*.8f,g,6);
                BuildHeldCards(p,g,skin);
                for(int k=0;k<6;k++)
                {
                    var d=new DeckDisplay{Player=p,Kind=k,Root=Group(k<4?"Pilha de terreno "+(k+1):k==4?"Deck principal":"Deck de terrenos",g)};
                    d.Root.localPosition=new Vector3((k-2.5f)*1.05f,-.16f,6.55f);
                    Block("Bandeja",Vector3.zero,new Vector3(.9f,.04f,1.12f),tint*.48f,d.Root);
                    d.Stack=Block("Cartas",new Vector3(0,.15f,0),new Vector3(.68f,.25f,.88f),Hex("D6C9A9"),d.Root);
                    Block("Verso",new Vector3(0,.505f,0),new Vector3(.64f,.02f,.84f),tint*.55f,d.Root);
                    var label=Group("Contagem pública",d.Root);label.localPosition=new Vector3(0,.54f,0);label.localRotation=Quaternion.Euler(90,180,0);
                    d.Label=label.gameObject.AddComponent<TextMesh>();d.Label.fontSize=40;d.Label.characterSize=.07f;d.Label.anchor=TextAnchor.MiddleCenter;d.Label.color=Color.white;
                    if(k<4){var hit=d.Root.gameObject.AddComponent<BoxCollider>();hit.center=new Vector3(0,.25f,0);hit.size=new Vector3(.9f,.6f,1.12f);var tag=d.Root.gameObject.AddComponent<PileHit>();tag.Player=p;tag.Pile=k;}
                    decks.Add(d);
                }
            }
        }
        int selectedPlayer=-1,selectedPile=-1;
        public void SelectPile(int player,int pile)
        {
            if(selectedPlayer==player&&selectedPile==pile)return;
            selectedPlayer=player;selectedPile=pile;
            foreach(var d in decks)d.Root.Find("Bandeja").GetComponent<Renderer>().sharedMaterial=Material(d.Player==player&&d.Kind==pile?Hex("F6D993"):Seats[d.Player]*.48f);
        }
        void SyncPlaces()
        {
            for(int p=0;p<4;p++)
            {
                places[p].gameObject.SetActive(p<match.Seats.Count);
                if(p<match.Seats.Count)places[p].localRotation=Quaternion.Euler(0,270-(match.Seats.Count==2?p*2:p)*90,0);
            }
            foreach(var d in decks)
            {
                if(d.Player>=match.Seats.Count)continue;
                var s=match.Seats[d.Player];int count=d.Kind<4?s.PileCount(d.Kind):d.Kind==4?s.MainCount:s.TerrainCount;
                float h=count==0?.01f:Mathf.Min(.48f,.04f+count*.015f);d.Stack.localScale=new Vector3(.68f,h,.88f);d.Stack.localPosition=new Vector3(0,h/2+.02f,0);
                d.Root.Find("Verso").localPosition=new Vector3(0,h+.035f,0);d.Label.transform.localPosition=new Vector3(0,h+.06f,0);
                d.Label.text=(d.Kind<4?"P"+(d.Kind+1):d.Kind==4?"D":"T")+"\n"+count;
                d.Root.Find("Verso").gameObject.SetActive(count>0);
            }
        }
    }
}
