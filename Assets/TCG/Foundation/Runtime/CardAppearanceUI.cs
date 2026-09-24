using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
 [Serializable] public sealed class CardArtEntry {public string cardId,styleId,resourcePath;}
 [Serializable] public sealed class CardArtManifest {public CardArtEntry[] entries=Array.Empty<CardArtEntry>();}
 public sealed partial class TableView
 {
  CardArtManifest artManifest;readonly Dictionary<string,Texture2D> alternateArt=new Dictionary<string,Texture2D>();
  bool inspectionFoil;string inspectionIdentity,inspectionStyle="standard";
  string CardStyle(Definition card)
  {
   if(network!=null&&network.State?.started==true&&!menu)return network.State.view?.looks?.LastOrDefault(x=>x.owner==Viewer&&x.card==card.Id)?.style??"standard";
   if(library==null||card.Rule=="ruins")return "standard";DeckData d=menu?draft:useSavedDecks&&match!=null?library.Data.decks.FirstOrDefault(x=>x.id==seatDecks[match.Controller]):null;
   return library.SelectedStyle(card.Id,d);
  }
  Texture2D CardIllustration(Definition card,string style)
  {
   if(artManifest==null){string file=Path.Combine(Application.streamingAssetsPath,"CardArt","manifest.json");artManifest=File.Exists(file)?JsonUtility.FromJson<CardArtManifest>(File.ReadAllText(file)):new CardArtManifest();}
   string key=CardStyles.Key(card.Id,style);if(!alternateArt.TryGetValue(key,out var texture))
   {var entry=artManifest.entries.FirstOrDefault(x=>x.cardId==card.Id&&x.styleId==style);texture=entry==null?null:Resources.Load<Texture2D>(entry.resourcePath);alternateArt[key]=texture;}
   return texture!=null?texture:world.Portrait(card);
  }
  bool ApplyCardStyle(bool toDeck)
  {
   string before=JsonUtility.ToJson(library.Data);var oldDraft=draft?.Copy();
   try{library.SelectStyle(inspected.Id,inspectionStyle,toDeck?draft:null);library.SelectFoil(inspected.Id,inspectionFoil,toDeck?draft:null);if(toDeck)library.SaveDeck(draft);collectionStore.Save(library);libraryMessage=hubNotice="Aparência equipada: "+CardStyles.Name(inspectionStyle);return true;}
   catch(Exception e){library=new CollectionLibrary(catalog,JsonUtility.FromJson<CollectionData>(before));draft=oldDraft;libraryMessage=hubNotice="Não foi possível equipar: "+e.Message;return false;}
  }
  bool CardFoil(Definition card,int owner=-1)
  {
   if(card.Rule=="ruins")return false;
   if(network!=null&&network.State?.started==true&&!menu)return network.State.view?.looks?.LastOrDefault(x=>x.owner==(owner>=0?owner:Viewer)&&x.card==card.Id)?.foil??card.Foil;
   if(library==null)return card.Foil;DeckData d=menu?draft:useSavedDecks&&match!=null?library.Data.decks.FirstOrDefault(x=>x.id==seatDecks[owner>=0?owner:match.Controller]):null;
   try{return library.SelectedFoil(card.Id,d);}catch(InvalidOperationException){return card.Foil;}
  }
  void RenderCardFace(Rect rect,Definition card,string style,bool selected=false)
  {
   var matrix=GUI.matrix;GUI.matrix=matrix*Matrix4x4.TRS(new Vector3(rect.x,rect.y,0),Quaternion.identity,new Vector3(rect.width/260f,rect.height/390f,1));
   bool night=style=="nocturne",gilded=style=="illuminated";
   Color edge=selected?new Color(1,.88f,.5f):night?TableWorld.Hex("B9AED6"):gilded?TableWorld.Hex("E5C374"):TableWorld.Hex("AC864A");
   Color dark=night?TableWorld.Hex("221B33"):TableWorld.Hex("32251D"),paper=night?TableWorld.Hex("352C45"):gilded?TableWorld.Hex("F4E5BF"):TableWorld.Hex("D9C7A0"),ink=night?TableWorld.Hex("EEE5F5"):TableWorld.Hex("33271D");
   Solid(new Rect(3,5,257,385),new Color(0,0,0,.7f));Solid(new Rect(0,0,260,390),edge);Solid(new Rect(4,4,252,382),dark);OrnateBorder(new Rect(7,7,246,376),edge);
   Solid(new Rect(13,13,234,46),paper);PaperGrain(new Rect(13,13,234,46),.12f);CardText(new Rect(20,16,190,41),card.Name,17,ink,true);
   Jewel(new Vector2(226,35),17,TableWorld.Elements[card.Color]);CardText(new Rect(211,20,30,30),card.TotalCost.ToString(),18,TableWorld.Elements[card.Color].grayscale>.45f?Color.black:Color.white,true,TextAnchor.MiddleCenter);
   Solid(new Rect(12,63,236,176),edge);DrawIllustration(new Rect(15,66,230,170),CardIllustration(card,style));
   var colors=CardFilter.CardColors(card);for(int n=0;n<colors.Length;n++)Solid(new Rect(15+n*230f/colors.Length,231,230f/colors.Length,5),TableWorld.Elements[colors[n]]);
   Solid(new Rect(13,242,234,24),dark);CardText(new Rect(19,244,222,20),TypeLabel(card)+(card.Subtypes.Count>0?" · "+string.Join(" / ",card.Subtypes):""),11,edge,true);
   Solid(new Rect(13,269,234,82),paper);PaperGrain(new Rect(13,269,234,82),.16f);
   string rules=card.Text;var textStyle=new GUIStyle(GUI.skin.label){font=body.font,fontSize=12,wordWrap=true,normal={textColor=ink}};while(textStyle.fontSize>9&&textStyle.CalcHeight(new GUIContent(rules),219)>75)textStyle.fontSize--;
   if(textStyle.CalcHeight(new GUIContent(rules),219)>75)rules=rules.Length>205?rules.Substring(0,202)+"…":rules;GUI.Label(new Rect(20,272,219,76),rules,textStyle);
   string stats=card.Kind==CardType.Creature||card.IsVehicle?card.PrintedStats+"   PA "+card.Actions+"   MOV "+card.Movement+"   ALC "+card.Range:card.Kind==CardType.Construction?"DEFESA "+card.Defense:card.Kind==CardType.Equipment?"EQUIPAR "+card.EquipCost:card.Kind==CardType.Terrain?"T E R R E N O":"";
   CardText(new Rect(16,353,228,20),stats,12,edge,true,TextAnchor.MiddleCenter);CardText(new Rect(17,369,226,10),card.Id+"  ·  "+RarityLabel(card)+"  ·  "+CardStyles.Name(style),8,edge,false,TextAnchor.MiddleCenter);
   if((inspected==card?inspectionFoil:CardFoil(card))){float sweep=Mathf.Repeat(Time.unscaledTime*.22f,1);for(int n=0;n<5;n++)Solid(new Rect(10,64+sweep*173+n,240,1),new Color(.6f+n*.08f,1,1,.16f));CardText(new Rect(189,350,52,14),"FOIL",10,Color.cyan,true);}GUI.matrix=matrix;
  }
  void DrawIllustration(Rect r,Texture2D texture)
  {
   float sourceAspect=(float)texture.width/texture.height,targetAspect=r.width/r.height;
   if(sourceAspect<targetAspect){float height=sourceAspect/targetAspect;GUI.DrawTextureWithTexCoords(r,texture,new Rect(0,1-height,1,height));}
   else{float width=targetAspect/sourceAspect;GUI.DrawTextureWithTexCoords(r,texture,new Rect((1-width)/2,0,width,1));}
  }
  void CardText(Rect r,string text,int size,Color color,bool bold=false,TextAnchor anchor=TextAnchor.UpperLeft)
  {var s=new GUIStyle(GUI.skin.label){padding=new RectOffset(0,0,0,0),font=bold?heading.font:body.font,fontSize=size,fontStyle=bold?FontStyle.Bold:FontStyle.Normal,wordWrap=true,alignment=anchor};s.normal.textColor=color;GUI.Label(r,text,s);}
 }
}

