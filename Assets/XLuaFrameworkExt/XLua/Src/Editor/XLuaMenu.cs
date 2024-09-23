/*
Copyright (c) 2015-2021 topameng(topameng@qq.com)
https://github.com/topameng/tolua

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
//打开开关没有写入导出列表的纯虚类自动跳过
//#define JUMP_NODEFINED_ABSTRACT

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using XLua;
using XLua.Editor;

namespace XLua.Editor
{

    [InitializeOnLoad]
    public static class XLuaMenu
    {

        //private static bool beAutoGen = false;
        const string tip = "为防止误操作，已禁用该命令！";

        static void ClearAllLuaFiles() {
            string[] pathList = new string[]{
                //lua生成路径
                XLuaConfig.GenLuaPath,
            };

            foreach (string pathi in pathList) {
                EditorTools.ClearFolder(pathi);
                EditorTools.DeleteDirectory(pathi);
            }
        }


        public static void CopyLuaBytesFiles(string sourceDir, string destDir, bool appendext = true, string searchPattern = "*.lua")
        {
            Func<string, string> fnFileMap = null;
            if (appendext) fnFileMap = (savePath) => { return savePath + ".bytes"; };

            EditorTools.CopyDirectoryFileMap(sourceDir, destDir, searchPattern, fnFileMap);

            UnityEngine.Debug.Log("== copy " + sourceDir + " " + destDir);
        }

        [MenuItem("XLua/Copy Lua files", false, 51)]
        public static void CopyLuaFilesToLuaSource()
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("警告", "请等待编辑器完成编译再执行此功能", "确定");
                return;
            }

            ClearAllLuaFiles();

            string luaDestDir = XLuaConfig.GenLuaPath;
            EditorTools.ClearFolder(luaDestDir);
            foreach (var addLuaPath in XLuaConfig.GetLuaSearchPaths()) {
                CopyLuaBytesFiles(addLuaPath, luaDestDir);
            }
            UnityEngine.Debug.Log($"Copy lua files to {luaDestDir} over");

            //EmmyLuaTools.ExportUnityAPI();
            UnityEngine.Debug.Log($"Gen EmmyLua files to {luaDestDir} over");

            AssetDatabase.Refresh();
        }

        [MenuItem("XLua/Open Folder/Assets", false, 61)]
        public static void OpenFolder()
        {
            var targetDir = Application.dataPath;
            Application.OpenURL(targetDir);
        }

        [MenuItem("XLua/Open Folder/Streaming Assets", false, 61)]
        public static void OpenFolderStreamingAssets()
        {
            var targetDir = Application.streamingAssetsPath;
            Application.OpenURL(targetDir);
        }

        [MenuItem("XLua/Open Folder/Persistent", false, 61)]
        public static void OpenFolderPersistent()
        {
            var targetDir = Application.persistentDataPath;
            Application.OpenURL(targetDir);
        }

    }
}
