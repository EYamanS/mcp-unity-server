// Tools/PrefabTools.cs
// Prefab listing, instantiation, and creation
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    public static class PrefabTools
    {
        public static object ListPrefabs(string path = null)
        {
            string searchPath = string.IsNullOrEmpty(path) ? "Assets" : path;
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { searchPath });
            var prefabs = new List<object>();
            
            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                prefabs.Add(new 
                { 
                    path = assetPath, 
                    name = System.IO.Path.GetFileNameWithoutExtension(assetPath) 
                });
            }
            return new { prefabs, count = prefabs.Count };
        }

        public static object InstantiatePrefab(JObject args)
        {
            var path = args.Value<string>("path");
            var name = args?["name"]?.ToString();
            
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) throw new Exception($"Prefab not found at '{path}'");

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (!string.IsNullOrEmpty(name)) instance.name = name;

            if (args["position"] is JObject pos)
                instance.transform.position = UnityMCPBridge.ParseVector3(pos);

            if (args["rotation"] is JObject rot)
                instance.transform.eulerAngles = UnityMCPBridge.ParseVector3(rot);

            Undo.RegisterCreatedObjectUndo(instance, "Instantiate Prefab");
            return new { success = true, name = instance.name, message = $"Instantiated prefab from '{path}'" };
        }

        public static object CreatePrefab(string goName, string path)
        {
            var obj = UnityMCPBridge.FindGameObjectOrThrow(goName);
            
            path = UnityMCPBridge.NormalizePath(path, "Assets/", ".prefab");
            UnityMCPBridge.EnsureDirectoryExists(path);

            var prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            return new { success = true, path, message = $"Created prefab at '{path}'" };
        }
    }
}
#endif

