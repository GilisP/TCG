using System;
using System.IO;
using System.Linq;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
 public sealed partial class TableView
 {
  bool shopBoosters;BoosterDefinition booster;BoosterReceipt openedBooster;float boosterOpenedAt;
  const float BoosterSealDuration=1.6f,BoosterCardDelay=.35f,BoosterFlipDuration=.6f;
  float BoosterOpeningDuration=>BoosterSealDuration+Mathf.Max(0,(openedBooster?.rewards.Count??1)-1)*BoosterCardDelay+BoosterFlipDuration;
  BoosterDefinition CurrentBooster(){if(booster==null){booster=JsonUtility.FromJson<BoosterDefinition>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"Economy","booster-medieval.json")));booster.Validate();}return booster;}
  bool PurchaseBooster(bool reward=false)
  {
   if(!collectionStore.CanWrite)return false;string before=JsonUtility.ToJson(library.Data);
   try{var random=new System.Random(Guid.NewGuid().GetHashCode());var receipt=reward?library.OpenRewardBooster(CurrentBooster(),random):library.OpenBooster(CurrentBooster(),random);collectionStore.Save(library);openedBooster=receipt;boosterOpenedAt=Time.unscaledTime;hubNotice="Booster adicionado à coleção.";return true;}
   catch(Exception e){library=new CollectionLibrary(catalog,JsonUtility.FromJson<CollectionData>(before));hubNotice="Abertura não concluída: "+e.Message;return false;}
  }
  void DrawBoosterShop()
  {
   var pack=CurrentBooster();var pool=pack.Pool(catalog);Fill(new Rect(45,357,525,493),Panel);
   DrawBoosterSeal(new Rect(153,382,290,385));Fill(new Rect(615,357,945,493),Panel);
   Text(650,389,870,48,pack.name,heading,Gold);Text(650,453,870,70,pack.cards+" cartas ou variantes para sua coleção. Cartas bloqueadas e pacotes de demonstração não entram neste booster.",body,Ink);
   int total=pack.standardWeight+pack.illuminatedWeight+pack.nocturneWeight;
   Text(650,546,850,85,"CHANCE POR ESPAÇO\nClássica "+(100f*pack.standardWeight/total).ToString("0.#")+"%   ·   Iluminura "+(100f*pack.illuminatedWeight/total).ToString("0.#")+"%   ·   Nocturna "+(100f*pack.nocturneWeight/total).ToString("0.#")+"%",body,Gold);
   Text(650,640,850,83,"Cada uma das "+pool.Length+" cartas tem chance igual, independentemente da raridade. Os sorteios são independentes. Variante repetida devolve "+pack.duplicateRefund+" moedas; uma arte nova também libera a carta-base.",small,Muted);
   if(Button(650,743,530,64,"COMPRAR E ABRIR · "+pack.price+" MOEDAS",library.Data.coins>=pack.price&&collectionStore.CanWrite,true))PurchaseBooster();
   if(Button(650,815,530,42,"ABRIR RECOMPENSA · "+library.Data.progress.boosters,library.Data.progress.boosters>0&&collectionStore.CanWrite))PurchaseBooster(true);
   if(Button(1200,743,320,64,"Última abertura",library.Data.lastBooster!=null)){openedBooster=library.Data.lastBooster;boosterOpenedAt=Time.unscaledTime-20;}
  }
  void DrawBoosterSeal(Rect r)
  {
   Fill(r,TableWorld.Hex("4E2226"));PaperGrain(r,.27f);OrnateBorder(new Rect(r.x+8,r.y+8,r.width-16,r.height-16),Gold);OrnateBorder(new Rect(r.x+20,r.y+20,r.width-40,r.height-40),TableWorld.Hex("997644"));
   Text(r.x+35,r.y+42,r.width-70,48,"CRÔNICAS",heading,Gold);Text(r.x+35,r.y+88,r.width-70,40,"DOS REINOS",cardName,Ink);Jewel(r.center,49,TableWorld.Hex("8B5937"));Text(r.center.x-25,r.center.y-25,50,50,"V",heading,Ink);
   Text(r.x+35,r.yMax-102,r.width-70,68,"UM NOVO CAPÍTULO\n5 cartas e variantes",label,Gold);
  }
  void DrawBoosterOpening()
  {
   if(openedBooster==null)return;GUI.enabled=true;float elapsed=Mathf.Max(0,Time.unscaledTime-boosterOpenedAt);
   Fill(new Rect(0,0,1600,1000),Dark);Text(85,55,1430,65,elapsed<BoosterSealDuration?"UM NOVO CAPÍTULO":"O SELO FOI ROMPIDO",menuTitle,Gold);Text(88,134,1400,50,"Estas cartas já estão salvas na sua coleção.",body,Ink);
   if(elapsed<BoosterSealDuration)DrawOpeningPack(elapsed);
   for(int n=0;n<openedBooster.rewards.Count;n++)
   {
    var reward=openedBooster.rewards[n];float w=Mathf.Min(260,1380f/openedBooster.rewards.Count-20),x=100+n*(w+20),y=255;
    float progress=Mathf.Clamp01((elapsed-BoosterSealDuration-n*BoosterCardDelay)/BoosterFlipDuration);
    if(elapsed<BoosterSealDuration+n*BoosterCardDelay)continue;
    float ease=1-Mathf.Pow(1-progress,3),center=Mathf.Lerp(800,x+w/2,ease),top=Mathf.Lerp(350,y,ease);
    float width=w*Mathf.Max(.02f,Mathf.Abs(Mathf.Cos(progress*Mathf.PI)));
    var rect=new Rect(center-width/2,top,width,w*1.5f);
    Solid(new Rect(rect.x+7,rect.y+12,rect.width,rect.height),new Color(0,0,0,.35f));
    if(progress<.5f)DrawOpeningBack(rect);
    else {var card=catalog.Cards.FirstOrDefault(c=>c.Id==reward.cardId);if(card!=null)RenderCardFace(rect,card,reward.styleId,true);else Text(rect.x,rect.y,rect.width,170,reward.cardId+"\nExpansão ausente",body,Ink);}
    if(progress<1)DrawBoosterSparks(new Vector2(center,top+w*.75f),progress,reward.styleId=="standard"?Gold:TableWorld.Hex("BBDFF5"));
    if(progress<1)continue;
    Text(x,y+w*1.5f+18,w,44,CardStyles.Name(reward.styleId),cardName,Gold);Text(x,y+w*1.5f+65,w,66,reward.duplicate?"REPETIDA\n+"+reward.refund+" moedas":"NOVA NA COLEÇÃO",small,reward.duplicate?Muted:Ink);
   }
   Text(95,800,1390,45,"Custo: "+openedBooster.price+" · retorno: "+openedBooster.refund+" · saldo: "+library.Data.coins+" moedas",body,Gold);
   if(elapsed<BoosterOpeningDuration){if(Button(95,872,1410,65,"PULAR ANIMAÇÃO",true,true))boosterOpenedAt=Time.unscaledTime-BoosterOpeningDuration;}
   else if(Button(95,872,1410,65,"GUARDAR NO ARSENAL",true,true))openedBooster=null;
  }
  void DrawOpeningBack(Rect rect)
  {
   var matrix=GUI.matrix;GUI.matrix=matrix*Matrix4x4.TRS(new Vector3(rect.x,rect.y,0),Quaternion.identity,new Vector3(rect.width/300,rect.height/450,1));
   DrawBoosterSeal(new Rect(0,0,300,450));GUI.matrix=matrix;
  }
  void DrawOpeningPack(float elapsed)
  {
   float charge=Mathf.Clamp01(elapsed/.85f),split=Mathf.Clamp01((elapsed-.85f)/.75f);
   var center=new Vector2(800,460);DrawBoosterSparks(center,charge*.45f,Gold);
   var matrix=GUI.matrix;GUIUtility.RotateAroundPivot(Mathf.Sin(elapsed*32)*charge*(1-split)*2,center);
   float scale=1+charge*.06f;var pack=new Rect(800-150*scale,225,300*scale,440*scale);
   if(split==0)DrawBoosterSeal(pack);
   else
   {
    foreach(int half in new[]{0,1})
    {
     var clip=new Rect(pack.x,pack.y+half*pack.height*.5f+(half==0?-1:1)*split*100,pack.width,pack.height*.5f);
     GUI.BeginGroup(clip);DrawBoosterSeal(new Rect(0,-half*pack.height*.5f,pack.width,pack.height));GUI.EndGroup();
    }
    Solid(new Rect(pack.x,pack.center.y-4-split*50,pack.width,8+split*100),new Color(1,.83f,.45f,(1-split)*.8f));
    DrawBoosterSparks(center,split,TableWorld.Hex("FFE7A5"));
   }
   GUI.matrix=matrix;
  }
  void DrawBoosterSparks(Vector2 center,float progress,Color tint)
  {
   var matrix=GUI.matrix;
   for(int i=0;i<18;i++)
   {
    float angle=i*Mathf.PI*2/18,dist=45+progress*(145+(i%3)*30);
    var point=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*dist;
    GUIUtility.RotateAroundPivot(i*20+45,point);
    Solid(new Rect(point.x-2,point.y-6,4,12),new Color(tint.r,tint.g,tint.b,(1-progress)*.8f));GUI.matrix=matrix;
   }
  }
 }
}
