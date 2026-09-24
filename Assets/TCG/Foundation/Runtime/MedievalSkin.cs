using System;
using UnityEngine;
namespace TCG.Table
{
 public sealed partial class TableView
 {
  static Texture2D medievalGrain,diamond;
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
   foreach(float x in new[]{r.x+9,r.xMax-9})foreach(float y in new[]{r.y+9,r.yMax-9}){Solid(new Rect(x-6,y-1,12,2),c);Solid(new Rect(x-1,y-6,2,12),c);}
  }
  static void MedievalFill(Rect r,Color color)
  {
   Solid(r,color);
   if(r.width>90&&r.height>55&&(color==Panel||color==Dark))
   {PaperGrain(r,.045f);if(color==Panel){Solid(new Rect(r.x+1,r.y+1,r.width-2,3),TableWorld.Hex("84673F"));OrnateBorder(new Rect(r.x+4,r.y+4,r.width-8,r.height-8),TableWorld.Hex("665337"));}}
  }
 }
}
