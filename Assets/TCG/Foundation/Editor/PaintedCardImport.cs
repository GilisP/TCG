using UnityEditor;
namespace TCG.Foundation.Editor
{
 public sealed class PaintedCardImport:AssetPostprocessor
 {
  void OnPreprocessTexture()
  {
   if(!assetPath.StartsWith("Assets/TCG/Resources/CardArt/PreconD01/"))return;
   var texture=(TextureImporter)assetImporter;texture.textureType=TextureImporterType.Default;texture.mipmapEnabled=false;texture.maxTextureSize=1024;texture.textureCompression=TextureImporterCompression.CompressedHQ;texture.sRGBTexture=true;texture.wrapMode=UnityEngine.TextureWrapMode.Clamp;texture.filterMode=UnityEngine.FilterMode.Bilinear;texture.alphaSource=TextureImporterAlphaSource.None;
  }
 }
}
