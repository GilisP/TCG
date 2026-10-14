using System;
using UnityEngine;
namespace TCG.Table
{
 public sealed partial class TableView
 {
  static Texture2D medievalGrain,diamond;
  static void SurfaceGradient(Rect r,Color color,float lift=.12f)
  {
   Solid(r,color);int bands=16;for(int i=0;i<bands;i++){float t=i/(float)(bands-1);float alpha=lift*(1-t)*(1-t);Solid(new Rect(r.x,r.y+r.height*i/bands,r.width,r.height/bands+.5f),new Color(.72f,.82f,.82f,alpha));}
   Solid(new Rect(r.x,r.yMax-1,r.width,1),new Color(0,0,0,.35f));
  }
  static void HubBackdrop()
  {
   SurfaceGradient(new Rect(0,0,1600,1000),Dark,.07f);
   // Quiet manuscript geometry, confined to the canvas behind the existing controls.
   for(int i=0;i<7;i++){float inset=28+i*13;Stroke(new Rect(inset,118+inset*.15f,1600-inset*2,850-inset*.35f),new Color(Gold.r,Gold.g,Gold.b,.012f));}
   PaperGrain(new Rect(0,0,1600,1000),.025f);
   SurfaceGradient(new Rect(0,0,1600,105),TableWorld.Hex("101A1D"),.065f);
   Solid(new Rect(0,104,1600,1),new Color(Gold.r,Gold.g,Gold.b,.25f));
  }
  static void Solid(Rect r,Color c){var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
  static void PaperGrain(Rect r,float opacity)
  {
   if(medievalGrain==null){medievalGrain=new Texture2D(128,128,TextureFormat.RGBA32,false){wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear,hideFlags=HideFlags.HideAndDontSave};var random=new System.Random(781);var pixels=new Color[128*128];for(int i=0;i<pixels.Length;i++){float v=.35f+(float)random.NextDouble()*.4f;pixels[i]=new Color(v,v,v,1);}medievalGrain.SetPixels(pixels);medievalGrain.Apply();}
   var old=GUI.color;GUI.color=new Color(1,1,1,opacity);GUI.DrawTextureWithTexCoords(r,medievalGrain,new Rect(0,0,r.width/128,r.height/128));GUI.color=old;
  }
  static void Stroke(Rect r,Color c,float w=1){Solid(new Rect(r.x,r.y,r.width,w),c);Solid(new Rect(r.x,r.yMax-w,r.width,w),c);Solid(new Rect(r.x,r.y,w,r.height),c);Solid(new Rect(r.xMax-w,r.y,w,r.height),c);}
  static void Jewel(Vector2 center,float radius,Color color)
  {
   if(diamond==null){diamond=new Texture2D(64,64,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,hideFlags=HideFlags.HideAndDontSave};var pixels=new Color[4096];for(int y=0;y<64;y++)for(int x=0;x<64;x++)pixels[y*64+x]=new Color(1,1,1,Mathf.Clamp01((1-Mathf.Abs((x-31.5f)/31.5f)-Mathf.Abs((y-31.5f)/31.5f))*32));diamond.SetPixels(pixels);diamond.Apply();}
   var old=GUI.color;GUI.color=TableWorld.Hex("D2B271");GUI.DrawTexture(new Rect(center.x-radius,center.y-radius,radius*2,radius*2),diamond);GUI.color=color;GUI.DrawTexture(new Rect(center.x-radius*.8f,center.y-radius*.8f,radius*1.6f,radius*1.6f),diamond);GUI.color=old;
  }
  static void OrnateBorder(Rect r,Color c)
  {
   Stroke(r,c);Stroke(new Rect(r.x+4,r.y+4,r.width-8,r.height-8),new Color(c.r,c.g,c.b,.32f));
   foreach(float x in new[]{r.x+9,r.xMax-9})foreach(float y in new[]{r.y+9,r.yMax-9}){Jewel(new Vector2(x,y),3,c);}
  }
  static void MedievalFill(Rect r,Color color)
  {
   Solid(r,color);
   if(r.width>90&&r.height>55&&(color==Panel||color==Dark))
   {SurfaceGradient(r,color,color==Panel ? .09f : .035f);PaperGrain(r,.028f);if(color==Panel){Stroke(new Rect(r.x+1,r.y+1,r.width-2,r.height-2),TableWorld.Hex("45504C"));OrnateBorder(new Rect(r.x+5,r.y+5,r.width-10,r.height-10),TableWorld.Hex("746044"));Solid(new Rect(r.x+17,r.y+1,r.width-34,1),new Color(Gold.r,Gold.g,Gold.b,.6f));}}
  }
 }
}
