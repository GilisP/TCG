using TCG.Foundation;
using UnityEngine;

namespace TCG.Table
{
    // Small code-drawn UI icons, independent of operating-system emoji fonts.
    static class ReactionIconArt
    {
        public static Texture2D Create(ReactionKind kind)
        {
            const int size=128;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){name="Reação "+kind,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color[size*size];Color gold=new Color(1,.76f,.25f),ink=new Color(.2f,.13f,.12f),light=new Color(1,.9f,.55f);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                Color color=Color.clear;
                void Disc(float cx,float cy,float radius,Color tint){float a=Mathf.Clamp01(radius-Vector2.Distance(new Vector2(x,y),new Vector2(cx,cy))+.5f);if(a>0)color=Color.Lerp(color,tint,a);}
                void Box(float left,float bottom,float right,float top,Color tint){if(x>=left&&x<=right&&y>=bottom&&y<=top)color=tint;}
                void Stroke(float ax,float ay,float bx,float by,float width,Color tint){var p=new Vector2(x-ax,y-ay);var d=new Vector2(bx-ax,by-ay);float t=Mathf.Clamp01(Vector2.Dot(p,d)/d.sqrMagnitude);float a=Mathf.Clamp01(width-Vector2.Distance(p,d*t)+.5f);if(a>0)color=Color.Lerp(color,tint,a);}
                if(kind==ReactionKind.ThumbsUp)
                {
                    Disc(70,60,32,ink);Box(40,30,87,78,ink);Stroke(54,60,60,103,13,ink);Disc(61,102,12,ink);
                    Disc(70,60,27,gold);Box(43,34,85,77,gold);Stroke(55,60,61,102,8,gold);Disc(61,103,7,light);
                    Box(20,27,43,73,new Color(.25f,.65f,.85f));Box(24,31,39,69,new Color(.4f,.8f,1));
                    Stroke(78,48,95,48,1.7f,ink);Stroke(80,60,98,60,1.7f,ink);Stroke(79,71,94,71,1.7f,ink);
                }
                else
                {
                    Disc(64,62,46,ink);Disc(64,63,42,kind==ReactionKind.Angry?new Color(1,.43f,.28f):gold);
                    Disc(46,81,11,light);
                    if(kind==ReactionKind.Celebrate)
                    {
                        Stroke(15,103,20,115,3,new Color(.35f,.9f,.9f));Stroke(106,102,114,115,3,new Color(.95f,.3f,.7f));Disc(15,68,4,new Color(.65f,.5f,1));Disc(110,49,4,new Color(.4f,1,.6f));
                        if(y>94&&y<125&&Mathf.Abs(x-64)<(125-y)*.62f)color=new Color(.6f,.4f,.95f);
                        Stroke(49,95,79,95,3,ink);Disc(64,124,3,light);
                    }
                    if(kind==ReactionKind.Happy||kind==ReactionKind.Celebrate)
                    {
                        Stroke(39,73,47,79,2.7f,ink);Stroke(47,79,55,73,2.7f,ink);Stroke(73,73,81,79,2.7f,ink);Stroke(81,79,89,73,2.7f,ink);
                        if(y<55&&y>31&&Vector2.Distance(new Vector2(x,y),new Vector2(64,56))<25)color=ink;
                        if(y<54&&y>47&&x>43&&x<85)color=Color.white;
                        Disc(64,33,9,new Color(.96f,.4f,.4f));
                    }
                    else
                    {
                        Disc(48,73,5,ink);Disc(80,73,5,ink);
                        if(kind==ReactionKind.Angry){Stroke(37,86,56,78,3,ink);Stroke(72,78,91,86,3,ink);}
                        else{Stroke(38,84,53,89,2.5f,ink);Stroke(75,89,90,84,2.5f,ink);Disc(85,58,5,new Color(.35f,.8f,1));}
                        for(int i=0;i<16;i++){float a=i*Mathf.PI/16,b=(i+1)*Mathf.PI/16;Stroke(64+20*Mathf.Cos(a),34+12*Mathf.Sin(a),64+20*Mathf.Cos(b),34+12*Mathf.Sin(b),2.8f,ink);}
                    }
                }
                pixels[y*size+x]=color;
            }
            texture.SetPixels(pixels);texture.Apply(false,true);return texture;
        }
    }
}
