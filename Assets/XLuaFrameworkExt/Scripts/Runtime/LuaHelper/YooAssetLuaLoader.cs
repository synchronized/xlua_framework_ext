
namespace XLuaFrameworkExt
{
    using System.IO;
    using UnityEngine;
    using XLua;
    using YooAsset;

    public class YooAssetLuaLoader
    {
        public YooAssetLuaLoader() 
        { 

        }


        byte[] load(ref string filepath)
        {
            var origFilepath = filepath.Replace('.', '/');
            if (!origFilepath.EndsWith(".lua")) origFilepath+=".lua";
            filepath = $"{XLuaConfig.AssetGenLuaPath}/{origFilepath}";
            AssetHandle handle = YooAssets.LoadAssetSync<TextAsset>(filepath);
            TextAsset textAsset = handle.AssetObject as TextAsset;
            if (textAsset == null) return null;
            return textAsset.bytes; //二进制数据
        }


        public static implicit operator LuaEnv.CustomLoader(YooAssetLuaLoader loader)
        {
            return loader.load;
        }
    }
}