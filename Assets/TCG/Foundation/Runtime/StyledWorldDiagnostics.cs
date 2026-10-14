using System;
using System.IO;
using System.Linq;
using System.Collections;
using TCG.Foundation;
using UnityEngine;
namespace TCG.Table
{
 public sealed partial class TableView
 {
  IEnumerator VerifyStyledWorld()
  {
   var dir=Path.GetFullPath("Logs/StyledWorldVerification");Directory.CreateDirectory(dir);int checks=0;
   Application.LogCallback listener=(message,trace,type)=>{if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert){File.WriteAllText(Path.Combine(dir,"result.txt"),"FAIL: "+message);Application.Quit(1);}};
   Application.logMessageReceived+=listener;
   collectionStore=new CollectionStore(Path.Combine(dir,"profile-"+Guid.NewGuid().ToString("N")));library=collectionStore.Load(catalog);menu=true;
   yield return world.VerifyStyledWorldFixtures(catalog,effects,dir,Check);
   File.WriteAllText(Path.Combine(dir,"result.txt"),"PASS: "+checks+" checks; smooth terrain, avatar picking, 121 cells, 2/3/4 seats, shared piles and native renders; zero runtime errors");
   Application.logMessageReceived-=listener;Application.Quit(0);
   void Check(bool ok,string why){checks++;if(!ok)throw new Exception("STYLED WORLD: "+why);}
  }
 }
 public sealed partial class TableWorld
 {
  public IEnumerator VerifyStyledWorldFixtures(ContentCatalog catalog,EffectRegistry effects,string dir,Action<bool,string> check)
  {
   yield return null;
   check(GetComponentsInChildren<CellHit>().Length==121,"121 logical cells");
   foreach(var cell in GetComponentsInChildren<CellHit>())
   {
    check(cell.transform.position==Position(cell.Index),"cell position "+cell.Index);
    var collider=cell.GetComponent<BoxCollider>();check(collider.size==new Vector3(.96f,.12f,.96f),"cell picking size "+cell.Index);
   }
   foreach(int count in new[]{2,3,4})
   {
    var table=ContentLoader.TestTable(catalog,effects,count,false,140014);Bind(table);yield return null;
    for(int seat=0;seat<4;seat++)
    {
     check(places[seat].gameObject.activeSelf==(seat<count),"active avatar "+count+" "+seat);
     if(seat>=count)continue;
     var hit=places[seat].GetComponentInChildren<PlayerAvatarHit>();check(hit!=null&&hit.Player==seat,"avatar seat identity");
     var box=hit.GetComponent<BoxCollider>();var center=box.bounds.center;check(box.Raycast(new Ray(center+Vector3.up*5,Vector3.down),out var rayHit,10),"avatar still clickable");
     check(heldHands[seat]!=null&&heldHands[seat].GetChild(0).name=="Verso de carta 0","private card backs retained");
     check(Mathf.Approximately(CapitalYaw(seat,count),Mathf.Atan2(-Position(Match.Capital(seat,count)).x,-Position(Match.Capital(seat,count)).z)*Mathf.Rad2Deg),"camera capital orientation");
    }
    check(GetComponentsInChildren<PileHit>().Count(p=>p.Player<0)==4,"four common piles");
   }
   var cards=catalog.Cards.Where(c=>c.Kind==CardType.Terrain).ToArray();check(cards.Length>0,"terrain catalog");
   var sample=match.Board.First(c=>c.Terrain!=null);var setter=typeof(Cell).GetProperty("Terrain");var original=sample.Terrain;
   for(int i=0;i<cards.Length;i++)
   {
    setter.SetValue(sample,cards[i]);var model=BuildTerrain(60,sample);
    check(model.GetComponentsInChildren<Collider>().Length==0,"decorations do not intercept picking "+cards[i].Id);
    check(model.GetComponentsInChildren<MeshFilter>().Any(m=>m.sharedMesh.name.StartsWith("Commander smooth")),"smooth terrain "+cards[i].Id);
    Release(model);if(i%15==0)yield return null;
   }
   setter.SetValue(sample,original);
   // Native camera previews contain the actual runtime geometry, not a painted mockup.
   var view=ContentLoader.TestTable(catalog,effects,4,false,140014).ViewFor(0);
   for(int i=0;i<121;i++)if(view.cells[i].terrain==null){view.cells[i].terrain=cards[i%cards.Length].Id;view.cells[i].owner=i%4;}
   Bind(Match.FromView(view,catalog,effects));yield return null;yield return null;
   for(int room=0;room<2;room++){SetRoom(room);RenderStyledPreview(Path.Combine(dir,"mesa-"+room+".png"),new Vector3(15,17,-20),Vector3.zero);}
   SetRoom(0);var avatar=AvatarPosition(0);RenderStyledPreview(Path.Combine(dir,"jogador.png"),avatar+new Vector3(4,2,-4),avatar+Vector3.down*.4f);
  }
  void RenderStyledPreview(string path,Vector3 position,Vector3 target)
  {
   var camera=new GameObject("Preview camera").AddComponent<Camera>();camera.CopyFrom(View);camera.transform.position=position;camera.transform.LookAt(target);camera.fieldOfView=42;camera.tag="Untagged";
   var rt=new RenderTexture(1600,1100,24);var old=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
   var texture=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());RenderTexture.active=old;camera.targetTexture=null;rt.Release();Destroy(rt);Destroy(texture);Destroy(camera.gameObject);
  }
 }
}
