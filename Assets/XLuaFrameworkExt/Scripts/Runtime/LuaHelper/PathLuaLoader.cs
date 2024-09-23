
namespace XLuaFrameworkExt
{
    using System.IO;
    using XLua;

    public class PathLuaLoader
    {
        private string luaPath;

        public PathLuaLoader(string luaPath)
        {
            this.luaPath = luaPath;
        }


        byte[] load(ref string filepath)
        {
            var origFilepath = filepath.Replace('.', '/');
            filepath = $"{luaPath}/{origFilepath}/init.lua";
            if (File.Exists(filepath))
            {
                return File.ReadAllBytes(filepath);
            }
            filepath = $"{luaPath}/{origFilepath}.lua";
            if (File.Exists(filepath))
            {
                return File.ReadAllBytes(filepath);
            }
            return null;
        }


        public static implicit operator LuaEnv.CustomLoader(PathLuaLoader loader)
        {
            return loader.load;
        }
    }
}