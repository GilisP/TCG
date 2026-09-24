using System.Collections.Generic;
using UnityEngine;
using TCG.Foundation;

namespace TCG.Table
{
    public sealed partial class TableWorld
    {
        public static readonly Color[] Elements={Hex("E9C56C"),Hex("AD8DC9"),Hex("66B6D8"),Hex("E38258"),Hex("B8DCCF"),Hex("82AB70"),Hex("B8B8A8")};
        readonly List<Material> effectMaterials=new List<Material>();
        readonly Dictionary<string,Texture2D> portraits=new Dictionary<string,Texture2D>();
        readonly List<(Transform root,float start,float duration,Vector3 origin,Vector3 target)> spells=new List<(Transform,float,float,Vector3,Vector3)>();
        Transform Figure(Definition card,Color owner,Transform parent)
        {
            if(card.Kind==CardType.Terrain)return TerrainPortrait(card,parent);
            var root=new GameObject(card.Name).transform; root.SetParent(parent,false);
            Color tint=Elements[card.Color],metal=Hex("B7C6BD"),stone=Hex("B9AC8A"),dark=Hex("283C43");
            Cone(new Vector3(0,.035f,0),.23f,.07f,owner,root,8);
            if(BuildAuthorObject(card,root)){AddCraftDetails(card,root);return root;}
            string shape=card.Art;
            if(card.Kind==CardType.Construction&&card.Name.Contains("Barricada")) {
                for(int n=-2;n<=2;n++){Block("Estaca",new Vector3(n*.11f,.27f,0),new Vector3(.075f,.45f,.09f),Hex("977048"),root);Cone(new Vector3(n*.11f,.55f,0),.06f,.17f,stone,root,4);}
                Block("Travessa",new Vector3(0,.3f,-.065f),new Vector3(.61f,.065f,.055f),Hex("674B35"),root);
            } else if(card.Kind==CardType.Equipment&&card.Name.Contains("Botas")) {
                for(int x=-1;x<=1;x+=2){Block("Cano da bota",new Vector3(x*.11f,.27f,.05f),new Vector3(.15f,.4f,.17f),Hex("795744"),root);Block("Pé",new Vector3(x*.11f,.1f,-.05f),new Vector3(.16f,.13f,.34f),Hex("533D31"),root);Block("Fivela",new Vector3(x*.11f,.32f,-.045f),new Vector3(.07f,.065f,.03f),metal,root);}
            } else if(card.Kind==CardType.Construction||shape=="castle") {
                Block("Muralha",new Vector3(0,.22f,0),new Vector3(.44f,.36f,.26f),stone,root);
                Block("Portão",new Vector3(0,.17f,-.14f),new Vector3(.14f,.27f,.025f),dark,root);
                for(int x=-1;x<=1;x+=2){Block("Torre",new Vector3(x*.24f,.32f,0),new Vector3(.16f,.6f,.3f),stone,root);Cone(new Vector3(x*.24f,.7f,0),.14f,.22f,tint,root,4);}
                for(int x=-1;x<=1;x++)Block("Ameia",new Vector3(x*.14f,.45f,-.04f),new Vector3(.08f,.1f,.25f),stone,root);
            } else if(card.Kind==CardType.Equipment||shape=="equipment") {
                Block("Pedestal",new Vector3(0,.12f,0),new Vector3(.28f,.17f,.25f),stone,root);
                Block("Lâmina",new Vector3(0,.48f,0),new Vector3(.065f,.46f,.045f),metal,root);
                Block("Guarda",new Vector3(0,.29f,0),new Vector3(.25f,.045f,.06f),tint,root);
                Cone(new Vector3(0,.76f,0),.065f,.16f,metal,root,4);
            } else if(card.Kind==CardType.Spell||card.Kind==CardType.Instant||shape=="sigil") {
                for(int n=0;n<6;n++){float a=n*Mathf.PI/3;Cone(new Vector3(Mathf.Cos(a)*.23f,.3f,Mathf.Sin(a)*.23f),.08f,.27f,tint,root,4);}
                Cone(new Vector3(0,.5f,0),.2f,.58f,tint,root,6);
                Cone(new Vector3(0,.86f,0),.1f,.16f,Hex("FFF1C6"),root,6);
            } else if(shape=="serpent") {
                for(int n=0;n<6;n++){float a=n*.9f;Block("Segmento aquático",new Vector3(Mathf.Sin(a)*.16f,.14f+n*.08f,Mathf.Cos(a)*.12f),new Vector3(.19f,.16f,.2f),tint,root);}
                Block("Cabeça marinha",new Vector3(-.18f,.7f,-.07f),new Vector3(.25f,.17f,.25f),tint,root);
                Cone(new Vector3(-.2f,.89f,-.07f),.12f,.2f,metal,root,3);
                for(int n=-1;n<=1;n+=2)Block("Presa",new Vector3(-.18f+n*.08f,.63f,-.22f),new Vector3(.04f,.14f,.04f),stone,root);
            } else if(shape=="beast") {
                Block("Corpo",new Vector3(0,.23f,0),new Vector3(.27f,.24f,.42f),tint*.65f,root);
                Block("Focinho",new Vector3(0,.32f,-.28f),new Vector3(.22f,.2f,.2f),tint,root);
                for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Block("Pata",new Vector3(x*.13f,.1f,z*.15f),new Vector3(.06f,.18f,.06f),dark,root);
                Cone(new Vector3(-.1f,.49f,-.24f),.075f,.16f,tint,root,3);Cone(new Vector3(.1f,.49f,-.24f),.075f,.16f,tint,root,3);
            } else if(shape=="winged") {
                Block("Corpo alado",new Vector3(0,.35f,0),new Vector3(.18f,.25f,.35f),tint,root);
                Cone(new Vector3(0,.61f,-.15f),.1f,.26f,tint,root,4);
                for(int x=-1;x<=1;x+=2){var wing=Block("Asa",new Vector3(x*.24f,.49f,.03f),new Vector3(.48f,.04f,.32f),tint*.8f,root);wing.localRotation=Quaternion.Euler(0,0,x*28);Block("Pata",new Vector3(x*.08f,.17f,0),new Vector3(.045f,.25f,.08f),metal,root);}
                Cone(new Vector3(0,.3f,.32f),.08f,.35f,tint,root,4);
            } else {
                float k=shape=="giant"?1.35f:1;
                Block("Tronco",new Vector3(0,.29f*k,0),new Vector3(.21f,.29f,.14f)*k,shape=="giant"?stone:tint*.7f,root);
                Block("Cabeça",new Vector3(0,.53f*k,0),new Vector3(.15f,.15f,.15f)*k,stone,root);
                for(int x=-1;x<=1;x+=2)Block("Bota",new Vector3(x*.065f,.1f,0),new Vector3(.07f,.16f,.11f),dark,root);
                if(shape=="mage") {
                    Cone(new Vector3(0,.7f,0),.16f,.25f,tint,root,5);
                    Block("Cajado",new Vector3(-.2f,.36f,0),new Vector3(.035f,.64f,.035f),stone,root);
                    Cone(new Vector3(-.2f,.76f,0),.1f,.2f,tint,root,5);
                } else if(shape=="archer") {
                    Cone(new Vector3(0,.65f,0),.15f,.18f,tint,root,5);
                    var bow=Block("Arco",new Vector3(-.2f,.36f,-.05f),new Vector3(.035f,.43f,.08f),stone,root);bow.localRotation=Quaternion.Euler(0,0,12);
                    Block("Flecha",new Vector3(-.13f,.38f,-.1f),new Vector3(.32f,.025f,.025f),metal,root);
                } else {
                    Block("Elmo",new Vector3(0,.64f*k,0),new Vector3(.19f,.07f,.19f)*k,metal,root);
                    Block("Espada",new Vector3(-.2f,.32f,0),new Vector3(.035f,.46f,.055f),metal,root);
                    Block("Escudo",new Vector3(.17f,.34f,-.09f),new Vector3(.08f,.28f,.2f),tint,root);
                    if(shape=="royal")for(int n=-1;n<=1;n++)Cone(new Vector3(n*.06f,.76f,0),.035f,.17f,Hex("E8C779"),root,4);
                }
            }
            AddFigureDetails(card,root);AddCraftDetails(card,root);
            return root;
        }
        public Texture2D Portrait(Definition card)
        {
            if(portraits.TryGetValue(card.Id,out var cached))return cached;
            var root=Figure(card,Elements[card.Color],null);root.position=new Vector3(1000,1000,1000);
            foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
            var go=new GameObject("Câmera de retrato");var cam=go.AddComponent<Camera>();
            cam.cullingMask=1<<30;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.Lerp(Hex("10282D"),Elements[card.Color],.18f);
            cam.orthographic=true;cam.orthographicSize=.6f;cam.transform.position=root.position+new Vector3(-1.2f,1.1f,-2);cam.transform.LookAt(root.position+Vector3.up*.38f);
            var rt=RenderTexture.GetTemporary(256,192,16);var prior=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
            var tex=new Texture2D(256,192,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,256,192),0,0);tex.Apply();tex.name=card.Id+" retrato procedural";
            RenderTexture.active=prior;cam.targetTexture=null;RenderTexture.ReleaseTemporary(rt);go.SetActive(false);root.gameObject.SetActive(false);Destroy(go);Release(root);portraits.Add(card.Id,tex);return tex;
        }
        void SpellVisual(MatchEvent e)
        {
            if(e.Card==null||(e.Kind!="cast"&&e.Kind!="trigger"&&e.Kind!="resolve"&&e.Kind!="impact"))return;
            bool announcement=e.Kind=="cast"||e.Kind=="trigger";Burst(announcement?e.From:e.To,e.Card,false,announcement);
        }
        void TickSpellVisuals()
        {
            for(int i=spells.Count-1;i>=0;i--){
                var s=spells[i];float t=(Time.unscaledTime-s.start)/s.duration;
                if(t>=1){foreach(var renderer in s.root.GetComponentsInChildren<Renderer>()){effectMaterials.Remove(renderer.sharedMaterial);Destroy(renderer.sharedMaterial);}Release(s.root);spells.RemoveAt(i);continue;}
                s.root.position=Vector3.Lerp(s.origin,s.target,t);s.root.rotation=Quaternion.Euler(0,t*150,0);s.root.localScale=Vector3.one*(.6f+Mathf.Sin(t*Mathf.PI)*.6f);
            }
        }
    }
}

