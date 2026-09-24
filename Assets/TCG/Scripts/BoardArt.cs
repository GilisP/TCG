using System.Collections.Generic;
using UnityEngine;

namespace TCG
{
    // Original procedural heraldry: no downloaded art, no per-frame texture allocations.
    public sealed class BoardArt
    {
        readonly Dictionary<string,Texture2D> images = new Dictionary<string,Texture2D>();
        public static readonly Color[] Palette = { new Color(1,.76f,.33f),new Color(.66f,.61f,.95f),new Color(.32f,.77f,.96f),new Color(1,.43f,.27f),new Color(.71f,.92f,.89f),new Color(.45f,.75f,.41f),new Color(.76f,.77f,.70f) };
        public Texture2D Icon(CardDefinition card)
        {
            string key = card.Kind == CardKind.Terrain ? "land"+card.Element : card.Art ?? "0";
            if (images.TryGetValue(key,out var image)) return image;
            image = new Texture2D(64,64,TextureFormat.RGBA32,false) { filterMode = FilterMode.Bilinear,hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color[64*64];
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            {
                float u = (x-31.5f)/29f, v = (y-31.5f)/29f, radius = Mathf.Sqrt(u*u+v*v);
                bool shape;
                if (key.StartsWith("land"))
                {
                    int e = card.Element;
                    shape = e == 0 ? radius < .34f || (radius < .75f && radius > .5f && Mathf.Abs(Mathf.Sin(Mathf.Atan2(v,u)*6)) < .35f) :
                        e == 1 ? radius < .65f && Mathf.Sqrt((u-.3f)*(u-.3f)+(v-.13f)*(v-.13f)) > .54f :
                        e == 2 ? Mathf.Abs(u) < .55f && Mathf.Abs(v-Mathf.Sin(u*7)*.12f) < .12f || Mathf.Abs(u) < .55f && Mathf.Abs(v+.37f-Mathf.Sin(u*7)*.12f) < .10f :
                        e == 3 ? v > -.65f && v < .75f && Mathf.Abs(u+Mathf.Sin(v*5)*.11f) < (.75f-v)*.38f :
                        e == 4 ? Mathf.Abs(u) < .7f && (Mathf.Abs(v-u*.22f) < .07f || Mathf.Abs(v-u*.22f-.35f) < .07f || Mathf.Abs(v-u*.22f+.35f) < .07f) :
                        e == 5 ? v > -.5f && v < .65f && Mathf.Abs(u) < (.65f-v)*.65f : Mathf.Abs(u)+Mathf.Abs(v) < .73f;
                }
                else if (key == "heal") shape = Mathf.Abs(u) < .16f && Mathf.Abs(v) < .65f || Mathf.Abs(v) < .16f && Mathf.Abs(u) < .65f;
                else if (key == "fire" || key == "damage") shape = v > -.7f && v < .7f && Mathf.Abs(u+Mathf.Sin(v*6)*.15f) < (.75f-v)*.3f;
                else if (key == "shield") shape = Mathf.Abs(u) < .6f && v < .62f && v > -.72f+Mathf.Abs(u)*.65f && (Mathf.Abs(u) > .43f || v > .45f || v < -.52f+Mathf.Abs(u)*.65f);
                else if (key == "stun") shape = radius < .75f && (Mathf.Abs(u) < .06f || Mathf.Abs(v-u*.58f) < .06f || Mathf.Abs(v+u*.58f) < .06f);
                else if (key == "return") shape = radius > .43f && radius < .62f && (u > -.25f || v < 0) || u < -.24f && u > -.73f && v > .12f && v < .35f-Mathf.Abs(u+.47f);
                else if (key == "exile") shape = Mathf.Abs(Mathf.Abs(u)+Mathf.Abs(v)-.65f) < .09f || Mathf.Abs(u-v) < .055f && radius < .76f;
                else if (key == "destroy") shape = Mathf.Abs(u-v) < .10f && radius < .7f || Mathf.Abs(u+v) < .10f && radius < .7f;
                else if (key == "buff") shape = Mathf.Abs(u) < .13f && v < .5f && v > -.65f || v > .2f && v < .7f && Mathf.Abs(u) < .7f-v;
                else if (key == "gainmana") shape = Mathf.Abs(u)*Mathf.Abs(v) < .04f && radius < .75f;
                else if (key == "counter") shape = radius < .7f && radius > .52f || Mathf.Abs(v-u) < .1f && radius < .65f;
                else if (key == "draw") shape = Mathf.Abs(u) < .6f && Mathf.Abs(v) < .6f && (Mathf.Abs(u) > .5f || Mathf.Abs(v) > .5f || Mathf.Abs(u) < .05f || Mathf.Abs(v-.2f) < .04f);
                else if (key == "c") shape = v > -.45f && v < .35f && Mathf.Abs(u) < .65f && (v < 0 || Mathf.Abs(u) > .4f || Mathf.Abs(u) < .13f);
                else
                {
                    shape = (u*u+(v-.4f)*(v-.4f) < .075f) || (v < .17f && v > -.6f && Mathf.Abs(u) < .2f+(-v)*.25f);
                    if (key == "3") shape |= Mathf.Abs(radius-.68f) < .065f && u > .25f;
                    else if (key == "2") shape |= u > .28f && u < .68f && v < .14f && v > -.48f+Mathf.Abs(u-.48f);
                    else if (key == "4" || key == "5") shape |= Mathf.Abs(u-.6f) < .055f && Mathf.Abs(v) < .67f;
                    else shape |= Mathf.Abs(u+.61f) < .055f && v > -.3f && v < .68f;
                }
                pixels[y*64+x] = shape ? Color.white : Color.clear;
            }
            image.SetPixels(pixels); image.Apply(); images[key] = image; return image;
        }
        public Texture2D Landscape(CardDefinition card)
        {
            var illustrated=GeneratedArt.Get(card.Illustration); if(illustrated != null) return illustrated;
            string key = "landscape-"+card.Element+"-"+card.Art;
            if (images.TryGetValue(key,out var cached)) return cached;
            const int w = 256,h = 144;
            var image = new Texture2D(w,h,TextureFormat.RGBA32,false) { filterMode = FilterMode.Bilinear,hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color[w*h]; Color tint = Palette[card.Element];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
            {
                float u = x/(float)w,v = y/(float)h;
                Color color = Color.Lerp(new Color(.035f,.055f,.07f),tint*.62f,v);
                float orb = Vector2.Distance(new Vector2(u,v),new Vector2(.72f,.72f));
                if (orb < .15f) color = Color.Lerp(color,tint,Mathf.Clamp01((.15f-orb)*30));
                if (v < .37f+.11f*Mathf.Sin(u*12+card.Element)) color = Color.Lerp(tint,new Color(.035f,.06f,.07f),.69f);
                if (v < .19f+.10f*Mathf.Sin(u*17+2)) color = new Color(.035f,.055f,.065f);
                // Fortress silhouette and battlements.
                bool tower = (u > .10f && u < .15f && v < .62f) || (u > .28f && u < .34f && v < .54f);
                bool wall = u > .12f && u < .34f && v < .40f;
                if (tower || wall) color = new Color(.05f,.067f,.069f);
                if ((tower || wall) && ((x/4)%2 == 0) && v > .57f) color = Color.Lerp(tint,Color.black,.65f);
                float grain = ((x*13+y*17)%11)/850f; color += new Color(grain,grain,grain,0);
                pixels[y*w+x] = color;
            }
            image.SetPixels(pixels); image.Apply(); images[key] = image; return image;
        }
        public void Dispose() { foreach (var image in images.Values) Object.Destroy(image); images.Clear(); }
    }
}
