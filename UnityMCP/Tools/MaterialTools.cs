// Tools/MaterialTools.cs
// Material creation, modification, and assignment
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    public static class MaterialTools
    {
        public static object CreateMaterial(JObject args)
        {
            var name = args.Value<string>("name");
            var path = args.Value<string>("path");
            var shaderName = args?["shader"]?.ToString() ?? "Standard";

            path = UnityMCPBridge.NormalizePath(path, "Assets/", ".mat");

            var shader = Shader.Find(shaderName);
            if (shader == null) throw new Exception($"Shader '{shaderName}' not found");

            var mat = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(mat, path);
            AssetDatabase.SaveAssets();

            return new { success = true, path, message = $"Created material '{name}' at '{path}'" };
        }

        public static object ListMaterials(string path = null)
        {
            string searchPath = string.IsNullOrEmpty(path) ? "Assets" : path;
            var guids = AssetDatabase.FindAssets("t:Material", new[] { searchPath });
            var materials = new List<object>();
            
            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                materials.Add(new 
                { 
                    path = assetPath, 
                    name = System.IO.Path.GetFileNameWithoutExtension(assetPath) 
                });
            }
            return new { materials, count = materials.Count };
        }

        public static object SetProperty(JObject args)
        {
            var path = args.Value<string>("path");
            var propertyName = args.Value<string>("property");
            var value = args["value"];

            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) throw new Exception($"Material not found at '{path}'");

            if (!mat.HasProperty(propertyName))
                throw new Exception($"Property '{propertyName}' not found on material");

            Undo.RecordObject(mat, "Set Material Property");

            // Try color
            if (value is JObject colorObj && colorObj["r"] != null)
            {
                mat.SetColor(propertyName, UnityMCPBridge.ParseColor(colorObj));
            }
            // Try float
            else if (value.Type == JTokenType.Float || value.Type == JTokenType.Integer)
            {
                mat.SetFloat(propertyName, value.Value<float>());
            }
            // Try vector
            else if (value is JObject vecObj && vecObj["x"] != null)
            {
                if (vecObj["w"] != null)
                    mat.SetVector(propertyName, new Vector4(
                        vecObj["x"]?.Value<float>() ?? 0,
                        vecObj["y"]?.Value<float>() ?? 0,
                        vecObj["z"]?.Value<float>() ?? 0,
                        vecObj["w"]?.Value<float>() ?? 0
                    ));
                else
                    mat.SetVector(propertyName, UnityMCPBridge.ParseVector3(vecObj));
            }
            else
            {
                throw new Exception($"Unsupported value type for property '{propertyName}'");
            }

            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            return new { success = true, message = $"Set {propertyName} on material '{path}'" };
        }

        public static object AssignToRenderer(JObject args)
        {
            var goName = args.Value<string>("gameObject");
            var matPath = args.Value<string>("materialPath");
            var index = args?["index"]?.Value<int>() ?? 0;

            var obj = UnityMCPBridge.FindGameObjectOrThrow(goName);
            var renderer = obj.GetComponent<Renderer>();
            if (renderer == null) throw new Exception($"No Renderer component on '{goName}'");

            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) throw new Exception($"Material not found at '{matPath}'");

            Undo.RecordObject(renderer, "Assign Material");
            var mats = renderer.sharedMaterials;
            if (index >= mats.Length)
            {
                var newMats = new Material[index + 1];
                Array.Copy(mats, newMats, mats.Length);
                mats = newMats;
            }
            mats[index] = mat;
            renderer.sharedMaterials = mats;

            return new { success = true, message = $"Assigned material to '{goName}' at index {index}" };
        }
    }
}
#endif

