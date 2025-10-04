// Tools/AssetTools.cs
// Asset database operations (folders, listing, etc.)
#if UNITY_EDITOR
using UnityEditor;
using System;
using System.Collections.Generic;

namespace UnityMCP
{
    public static class AssetTools
    {
        public static object CreateFolder(string path)
        {
            if (!path.StartsWith("Assets/")) path = "Assets/" + path;

            var folders = path.Split('/');
            string currentPath = folders[0];
            
            for (int i = 1; i < folders.Length; i++)
            {
                string nextPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                currentPath = nextPath;
            }

            AssetDatabase.Refresh();
            return new { success = true, path, message = $"Created folder '{path}'" };
        }

        public static object ListAssets(string assetType, string path = null)
        {
            string searchPath = string.IsNullOrEmpty(path) ? "Assets" : path;
            var guids = AssetDatabase.FindAssets($"t:{assetType}", new[] { searchPath });
            var assets = new List<object>();
            
            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                assets.Add(new 
                { 
                    path = assetPath, 
                    name = System.IO.Path.GetFileNameWithoutExtension(assetPath) 
                });
            }
            
            return new { assets, count = assets.Count, type = assetType };
        }
    }
}
#endif

