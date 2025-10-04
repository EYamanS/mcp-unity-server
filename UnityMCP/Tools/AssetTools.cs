// Tools/AssetTools.cs
// Asset database operations (folders, listing, etc.)
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
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

        public static object GetAssetPath(string name, string assetType)
        {
            try
            {
                // First, try to find the GameObject by name
                var gameObject = GameObject.Find(name);
                if (gameObject == null)
                {
                    return new { success = false, message = $"GameObject '{name}' not found" };
                }

                // Map asset types to Unity types
                Type unityType = GetUnityTypeFromAssetType(assetType);
                if (unityType == null)
                {
                    return new { success = false, message = $"Unsupported asset type: {assetType}" };
                }

                // Search for the asset in the project
                var guids = AssetDatabase.FindAssets($"t:{unityType.Name}");
                var matchingAssets = new List<object>();
                
                foreach (var guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    string assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                    
                    // Check if this asset is related to the GameObject
                    if (IsAssetRelatedToGameObject(assetPath, gameObject, assetType))
                    {
                        matchingAssets.Add(new 
                        { 
                            path = assetPath, 
                            name = assetName,
                            type = assetType
                        });
                    }
                }

                if (matchingAssets.Count == 0)
                {
                    return new { 
                        success = false, 
                        message = $"No {assetType} assets found for GameObject '{name}'",
                        gameObject = name,
                        assetType = assetType
                    };
                }

                return new { 
                    success = true, 
                    assets = matchingAssets,
                    count = matchingAssets.Count,
                    gameObject = name,
                    assetType = assetType
                };
            }
            catch (Exception ex)
            {
                return new { success = false, message = $"Error: {ex.Message}" };
            }
        }

        private static Type GetUnityTypeFromAssetType(string assetType)
        {
            switch (assetType.ToLower())
            {
                case "material": return typeof(Material);
                case "prefab": return typeof(GameObject);
                case "script": return typeof(MonoScript);
                case "texture": return typeof(Texture2D);
                case "audioclip": return typeof(AudioClip);
                case "animation": return typeof(AnimationClip);
                case "animatorcontroller": return typeof(UnityEditor.Animations.AnimatorController);
                case "mesh": return typeof(Mesh);
                case "shader": return typeof(Shader);
                default: return null;
            }
        }

        private static bool IsAssetRelatedToGameObject(string assetPath, GameObject gameObject, string assetType)
        {
            try
            {
                var asset = AssetDatabase.LoadAssetAtPath(assetPath, GetUnityTypeFromAssetType(assetType));
                if (asset == null) return false;

                switch (assetType.ToLower())
                {
                    case "material":
                        // Check if this material is used by the GameObject's renderers
                        var renderers = gameObject.GetComponentsInChildren<Renderer>();
                        foreach (var renderer in renderers)
                        {
                            foreach (var material in renderer.sharedMaterials)
                            {
                                if (material == asset) return true;
                            }
                        }
                        break;
                    
                    case "prefab":
                        // Check if this is the prefab for the GameObject
                        var prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
                        if (prefab == asset) return true;
                        break;
                    
                    case "script":
                        // Check if this script is attached to the GameObject
                        var script = asset as MonoScript;
                        if (script != null)
                        {
                            var componentType = script.GetClass();
                            if (componentType != null && gameObject.GetComponent(componentType) != null)
                                return true;
                        }
                        break;
                    
                    case "texture":
                        // Check if this texture is used by materials on the GameObject
                        var texture = asset as Texture2D;
                        if (texture != null)
                        {
                            var renderers2 = gameObject.GetComponentsInChildren<Renderer>();
                            foreach (var renderer in renderers2)
                            {
                                foreach (var material in renderer.sharedMaterials)
                                {
                                    if (material != null && material.mainTexture == texture)
                                        return true;
                                }
                            }
                        }
                        break;
                    
                    case "audioclip":
                        // Check if this audio clip is used by AudioSource components
                        var audioClip = asset as AudioClip;
                        if (audioClip != null)
                        {
                            var audioSources = gameObject.GetComponentsInChildren<AudioSource>();
                            foreach (var audioSource in audioSources)
                            {
                                if (audioSource.clip == audioClip) return true;
                            }
                        }
                        break;
                    
                    case "animation":
                    case "animatorcontroller":
                        // Check if this animation/controller is used by Animator components
                        var animators = gameObject.GetComponentsInChildren<Animator>();
                        foreach (var animator in animators)
                        {
                            if (animator.runtimeAnimatorController != null)
                            {
                                var controllerPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
                                if (controllerPath == assetPath) return true;
                            }
                        }
                        break;
                    
                    case "mesh":
                        // Check if this mesh is used by MeshFilter components
                        var mesh = asset as Mesh;
                        if (mesh != null)
                        {
                            var meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
                            foreach (var meshFilter in meshFilters)
                            {
                                if (meshFilter.sharedMesh == mesh) return true;
                            }
                        }
                        break;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
#endif

