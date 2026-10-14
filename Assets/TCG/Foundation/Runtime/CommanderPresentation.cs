using TCG.Foundation;
using UnityEngine;
using UnityEngine.Rendering;
namespace TCG.Table
{
 public sealed partial class TableWorld
 {
  Transform CommanderPresentation(Definition card,Color owner,Transform parent)
  {
   var prefab=Resources.Load<GameObject>("CommanderModels/"+card.Id+"-v1");
   var root=prefab!=null?Instantiate(prefab,parent,false).transform:CommanderFigure(card,owner,parent);
   root.name=card.Name;
   foreach(var renderer in root.GetComponentsInChildren<Renderer>())if(renderer.name=="SeatEmblem")renderer.sharedMaterial=Material(owner);
   return root;
  }
  public void BeginCommanderPreview(){previousQuality=QualitySettings.renderPipeline;previousPipeline=GraphicsSettings.defaultRenderPipeline;QualitySettings.renderPipeline=null;GraphicsSettings.defaultRenderPipeline=null;}
  // Used by the asset baker and isolated art gallery; neither edits match state.
  public Transform CreateCommanderModel(Definition card,Color owner,Transform parent=null,bool usePrefab=true)
  {return usePrefab?CommanderPresentation(card,owner,parent):CommanderFigure(card,owner,parent);}
#if UNITY_EDITOR
  public void ReleasePreviewMaterials()
  {foreach(var material in materials.Values)if(material!=null)DestroyImmediate(material);materials.Clear();}
#endif
 }
}
