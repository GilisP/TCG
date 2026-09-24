using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using TCG.Foundation;

namespace TCG.Table
{
    public sealed class CellHit : MonoBehaviour { public int Index; }
    public sealed partial class TableWorld : MonoBehaviour
    {
        public static readonly Color[] Seats={Hex("EAC17B"),Hex("72B59D"),Hex("78ACD2"),Hex("D98785")};
        static readonly Color[] Land={Hex("C9B577"),Hex("88769A"),Hex("759BB8"),Hex("B96852"),Hex("ADB8BD"),Hex("52776B"),Hex("829080")};
        readonly Dictionary<Color,Material> materials=new Dictionary<Color,Material>();
        readonly Dictionary<int,Transform> pieces=new Dictionary<int,Transform>();
        readonly Dictionary<int,Queue<Vector3>> walkPoints=new Dictionary<int,Queue<Vector3>>();
        readonly Dictionary<int,Vector3> destinations=new Dictionary<int,Vector3>();
        readonly List<(Transform root,float until)> dying=new List<(Transform,float)>();
        readonly Transform[] terrain=new Transform[121],edges=new Transform[121];
        readonly string[] signatures=new string[121];
        readonly Dictionary<int,float> pulse=new Dictionary<int,float>();
        readonly Dictionary<int,Vector3> lunges=new Dictionary<int,Vector3>();
        public Camera View { get; private set; }
        public float Yaw=0,Distance=23,Elevation=57;
        Match match; int seenRevision=-1;
        RenderPipelineAsset previousQuality,previousPipeline;
        public static Color Hex(string value) { ColorUtility.TryParseHtmlString("#"+value,out var c); return c; }
        public static Vector3 Position(int i)=>new Vector3(i%11-5,0,i/11-5);
        public static float CapitalYaw(int seat,int count){var home=Position(Match.Capital(seat,count));return Mathf.Atan2(-home.x,-home.z)*Mathf.Rad2Deg;}
        public void Setup()
        {
            // The legacy project selects a 2D renderer in its quality profile. This scene uses
            // built-in 3D materials; restore the original pipeline when leaving the scene.
            previousQuality=QualitySettings.renderPipeline; previousPipeline=GraphicsSettings.defaultRenderPipeline;
            QualitySettings.renderPipeline=null; GraphicsSettings.defaultRenderPipeline=null;
            View=new GameObject("Mesa • câmera").AddComponent<Camera>(); View.transform.SetParent(transform);
            View.clearFlags=CameraClearFlags.SolidColor; View.backgroundColor=Hex("111F24"); View.fieldOfView=48; View.nearClipPlane=.1f; View.farClipPlane=70;
            View.cullingMask=~(1<<30); View.tag="MainCamera"; View.gameObject.AddComponent<AudioListener>();
            RenderSettings.ambientLight=Hex("85918C"); RenderSettings.fog=true; RenderSettings.fogColor=Hex("111F24"); RenderSettings.fogMode=FogMode.Linear; RenderSettings.fogStartDistance=24; RenderSettings.fogEndDistance=48;
            var light=new GameObject("Luz de fim de tarde").AddComponent<Light>(); light.transform.SetParent(transform); light.type=LightType.Directional; light.intensity=1.3f; light.color=Hex("FFE1B0"); light.transform.rotation=Quaternion.Euler(55,-35,0); light.shadows=LightShadows.Soft;
            QualitySettings.shadows=ShadowQuality.All; QualitySettings.shadowDistance=35;
            BuildRooms();
            Block("Mesa • madeira",new Vector3(0,-.75f,0),new Vector3(15.6f,.65f,15.6f),Hex("433B33"),transform);
            Block("Mesa • contorno de bronze",new Vector3(0,-.4f,0),new Vector3(15.3f,.12f,15.3f),Hex("9C8255"),transform);
            Block("Mesa • veludo",new Vector3(0,-.3f,0),new Vector3(15.05f,.18f,15.05f),Hex("1B3336"),transform);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Block("Pé da mesa",new Vector3(x*6,-2,z*6),new Vector3(.6f,2.5f,.6f),Hex("39372F"),transform);
            for(int n=0;n<=11;n++)
            {
                Block("Grade do tapete",new Vector3(n-5.5f,-.195f,0),new Vector3(.014f,.01f,11),Hex("294244"),transform);
                Block("Grade do tapete",new Vector3(0,-.195f,n-5.5f),new Vector3(11,.01f,.014f),Hex("294244"),transform);
            }
            BuildPlaces();
            for(int n=0;n<121;n++)
            {
                var hit=new GameObject("Casa "+(n%11)+","+(n/11)); hit.transform.SetParent(transform); hit.transform.position=Position(n);
                var collider=hit.AddComponent<BoxCollider>(); collider.size=new Vector3(.96f,.12f,.96f); hit.AddComponent<CellHit>().Index=n;
                var edge=new GameObject("Alvo "+n).transform; edge.SetParent(transform); edge.position=Position(n)+Vector3.up*.045f;
                Block("Borda",new Vector3(0,0,.47f),new Vector3(.96f,.018f,.018f),Hex("F6D993"),edge);
                Block("Borda",new Vector3(0,0,-.47f),new Vector3(.96f,.018f,.018f),Hex("F6D993"),edge);
                Block("Borda",new Vector3(.47f,0,0),new Vector3(.018f,.018f,.96f),Hex("F6D993"),edge);
                Block("Borda",new Vector3(-.47f,0,0),new Vector3(.018f,.018f,.96f),Hex("F6D993"),edge);
                edges[n]=edge; edge.gameObject.SetActive(false);
            }
        }
        public void Bind(Match next)
        {
            ClearBursts();ShowMovement(-1,false); PreviewMovementDrag(-1,-1); ShowMovementOrder(-1,false); lightRevision=-1; if(match!=null) match.Visual-=Animate; match=next; match.Visual+=Animate;
            foreach(var t in pieces.Values) Release(t); pieces.Clear(); destinations.Clear(); walkPoints.Clear();
            foreach(var d in dying) Release(d.root); dying.Clear(); pulse.Clear(); lunges.Clear();
            foreach(var t in terrain) if(t!=null) Release(t); Array.Clear(signatures,0,121); seenRevision=-1;
        }
        public void Highlight(Func<int,bool> valid) { for(int i=0;i<121;i++) edges[i].gameObject.SetActive(valid(i)); }
        void Animate(MatchEvent e) { if(e.Kind=="move"&&e.Piece>=0) { if(!walkPoints.TryGetValue(e.Piece,out var path)){path=new Queue<Vector3>();walkPoints.Add(e.Piece,path);}path.Enqueue(Position(e.To)+Vector3.up*.16f); } SpellVisual(e); FoilEntry(e); if(e.Piece>=0) { pulse[e.Piece]=Time.unscaledTime; lunges[e.Piece]=e.Kind=="attack"?(Position(e.To)-Position(e.From))*.55f:Vector3.zero; } }
        void LateUpdate()
        {
            if(View==null) return;
            View.transform.position=Quaternion.Euler(Elevation,Yaw,0)*new Vector3(0,0,-Distance); View.transform.LookAt(Vector3.zero);
            TickSpellVisuals(); TickMovementLights(); TickHeldCards();
            if(match==null) return;
            if(seenRevision!=match.Revision) { Sync(); seenRevision=match.Revision; }
            foreach(var pair in pieces)
            {
                var target=destinations[pair.Key]; if(walkPoints.TryGetValue(pair.Key,out var path)&&path.Count>0){target=path.Peek();if(Vector3.Distance(pair.Value.position,target)<.03f)path.Dequeue();} float age=pulse.TryGetValue(pair.Key,out float start)?Time.unscaledTime-start:10;
                if(age<.6f&&(!walkPoints.TryGetValue(pair.Key,out var moving)||moving.Count==0)) { float wave=Mathf.Sin(age/.6f*Mathf.PI); target.y+=wave*.3f; if(lunges.TryGetValue(pair.Key,out var lunge)) target+=lunge*wave; }
                pair.Value.position=Vector3.Lerp(pair.Value.position,target,1-Mathf.Exp(-12*Time.unscaledDeltaTime));
            }
            for(int i=dying.Count-1;i>=0;i--) { var d=dying[i]; d.root.localScale*=Mathf.Exp(-6*Time.unscaledDeltaTime); if(Time.unscaledTime>d.until) { Release(d.root); dying.RemoveAt(i); } }
        }
        void Sync()
        {
            SyncPlaces();
            var alive=new HashSet<int>();
            for(int i=0;i<121;i++)
            {
                var cell=match.Board[i]; string signature=(cell.Terrain?.Id ?? "")+":"+cell.Owner+":"+cell.CapitalOwner;
                if(signatures[i]!=signature)
                {
                    if(terrain[i]!=null) Release(terrain[i]); signatures[i]=signature;
                    if(cell.Terrain!=null) terrain[i]=BuildTerrain(i,cell);
                }
                for(int n=0;n<cell.Pieces.Count;n++)
                {
                    var p=cell.Pieces[n]; alive.Add(p.Id);
                    // Display up to nine models per cell; UI lists every occupant without a rule cap.
                    bool visible=n<PieceFormation.VisibleLimit;
                    if(!pieces.TryGetValue(p.Id,out var model)) { model=BuildPiece(p); pieces.Add(p.Id,model); model.position=Position(i)+Vector3.up*.8f; }
                    model.gameObject.SetActive(visible);
                    
                    destinations[p.Id]=Position(i)+PieceFormation.Offset(n,cell.Pieces.Count)+(cell.CapitalOwner>=0?Vector3.back*.12f:Vector3.zero);
                    model.localScale=Vector3.one*PieceFormation.Scale(cell.Pieces.Count);                    if(p.CarrierId>=0)
                    {
                        int carrier=cell.Pieces.ToList().FindIndex(q=>q.Id==p.CarrierId);
                        if(carrier>=0){var passengers=match.Passengers(p.CarrierId);int seat=passengers.ToList().FindIndex(q=>q.Id==p.Id);float formationScale=PieceFormation.Scale(cell.Pieces.Count);destinations[p.Id]=Position(i)+PieceFormation.Offset(carrier,cell.Pieces.Count)+new Vector3((seat%2-.5f)*.23f,.48f,(seat/2)*.18f)*formationScale+(cell.CapitalOwner>=0?Vector3.back*.12f:Vector3.zero);model.localScale=Vector3.one*formationScale*.43f;}
                    }
                }
            }
            foreach(int id in pieces.Keys.Where(id=>!alive.Contains(id)).ToArray()) { dying.Add((pieces[id],Time.unscaledTime+.55f)); pieces.Remove(id); destinations.Remove(id); walkPoints.Remove(id); pulse.Remove(id); }
        }
        Transform BuildTerrain(int index,Cell cell)
        {
            var root=new GameObject(cell.Terrain.Name).transform; root.SetParent(transform); root.position=Position(index);
            Block("Pedra",new Vector3(0,-.075f,0),new Vector3(.94f,.3f,.94f),Hex("526058"),root);
            Block("Terreno",new Vector3(0,.085f,0),new Vector3(.92f,.035f,.92f),Land[cell.Terrain.Color],root);
            Block("Influência",new Vector3(0,.11f,-.435f),new Vector3(.75f,.026f,.035f),Seats[Math.Max(0,cell.Owner)],root);
            if(cell.CapitalOwner>=0)
            {
                var fort=Group("Marco da capital",root);fort.localPosition=new Vector3(0,.1f,.34f);fort.localScale=Vector3.one*.4f;
                var tint=Seats[cell.CapitalOwner]; Block("Fortaleza",new Vector3(0,.24f,.15f),new Vector3(.46f,.28f,.34f),Hex("D0C6A7"),fort);
                for(int x=-1;x<=1;x+=2) { Block("Torre",new Vector3(x*.24f,.34f,.2f),new Vector3(.16f,.5f,.18f),Hex("C2B99C"),fort); Cone(new Vector3(x*.24f,.66f,.2f),.15f,.2f,tint,fort,4); }
                Block("Portão",new Vector3(0,.23f,-.03f),new Vector3(.1f,.2f,.015f),Hex("353D37"),fort);
            }
            else if(cell.Terrain.Rule=="ruins")
            {
                Block("Muro quebrado",new Vector3(-.29f,.2f,.29f),new Vector3(.22f,.25f,.13f),Hex("77736D"),root);
                Block("Coluna partida",new Vector3(.31f,.18f,.3f),new Vector3(.12f,.18f,.12f),Hex("939087"),root);
            }
            else
            {
                Cone(new Vector3(.32f,.16f,.3f),.14f,.12f,Land[cell.Terrain.Color]*.75f,root,5);
                if(cell.Terrain.Color==5||cell.Terrain.Color==6) { Block("Tronco",new Vector3(-.32f,.18f,.29f),new Vector3(.05f,.2f,.05f),Hex("685447"),root); Cone(new Vector3(-.32f,.44f,.29f),.14f,.35f,Hex("294F46"),root,5); }
                else Cone(new Vector3(-.32f,.2f,.31f),.09f,.25f,Land[cell.Terrain.Color]*1.2f,root,5);
            }
            return root;
        }
        Transform BuildPiece(Piece p)
        {
            var root=Figure(p.Card,Seats[p.Owner],transform);root.gameObject.AddComponent<PieceHit>().Piece=p.Id;
            var hit=root.gameObject.AddComponent<BoxCollider>();hit.center=new Vector3(0,.4f,0);hit.size=new Vector3(.68f,1.05f,.68f); if(p.Card.Kind==CardType.Creature)root.gameObject.AddComponent<FigureMotion>().Initialize(p.Card.Art,p.Id);return root;
        }
        Transform Block(string title,Vector3 pos,Vector3 scale,Color color,Transform parent)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name=title; obj.transform.SetParent(parent,false); obj.transform.localPosition=pos; obj.transform.localScale=scale;
            Destroy(obj.GetComponent<Collider>()); obj.GetComponent<Renderer>().sharedMaterial=Material(color); return obj.transform;
        }
        void Cone(Vector3 pos,float radius,float height,Color color,Transform parent,int sides)
        {
            var obj=new GameObject("Forma low-poly"); obj.transform.SetParent(parent,false); obj.transform.localPosition=pos;
            var vertices=new List<Vector3>(); var triangles=new List<int>();
            for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides; int at=vertices.Count;
                vertices.Add(new Vector3(Mathf.Cos(a)*radius,-height/2,Mathf.Sin(a)*radius)); vertices.Add(new Vector3(0,height/2,0)); vertices.Add(new Vector3(Mathf.Cos(b)*radius,-height/2,Mathf.Sin(b)*radius)); triangles.Add(at); triangles.Add(at+1); triangles.Add(at+2);
            }
            var mesh=new Mesh{name="Geometria low-poly"}; mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals();
            obj.AddComponent<MeshFilter>().sharedMesh=mesh; obj.AddComponent<MeshRenderer>().sharedMaterial=Material(color);
        }
        Material Material(Color color)
        {
            if(!materials.TryGetValue(color,out var material))
            {
                var template=Resources.Load<Material>("TableSurface"); material=template!=null?new Material(template):new Material(Shader.Find("Standard"));
                material.color=color; material.SetFloat("_Glossiness",.12f); materials.Add(color,material);
            }
            return material;
        }
        static void Release(Transform root) { if(root==null) return; foreach(var filter in root.GetComponentsInChildren<MeshFilter>()) if(filter.sharedMesh!=null&&filter.sharedMesh.name=="Geometria low-poly") Destroy(filter.sharedMesh); Destroy(root.gameObject); }
        void OnDestroy()
        {
            if(match!=null) match.Visual-=Animate;
            foreach(var portrait in portraits.Values) Destroy(portrait);
            foreach(var material in effectMaterials) Destroy(material);
            foreach(var m in materials.Values) Destroy(m);
            foreach(var filter in GetComponentsInChildren<MeshFilter>()) if(filter.sharedMesh!=null&&filter.sharedMesh.name=="Geometria low-poly") Destroy(filter.sharedMesh);
#if UNITY_EDITOR
            // A standalone build contains only Mesa and starts with its own rendering profile.
            // Restoring a stripped editor-only URP asset while the player quits is invalid.
            QualitySettings.renderPipeline=previousQuality; GraphicsSettings.defaultRenderPipeline=previousPipeline;
#endif
        }
    }
}

