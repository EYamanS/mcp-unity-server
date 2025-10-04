// Tools/SceneTools.cs
// Scene management and hierarchy operations
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

namespace UnityMCP
{
    public static class SceneTools
    {
        public static object ListScenes()
        {
            var scenes = new List<object>();
            foreach (var s in EditorBuildSettings.scenes)
            {
                scenes.Add(new
                {
                    path = s.path,
                    enabled = s.enabled,
                    name = System.IO.Path.GetFileNameWithoutExtension(s.path)
                });
            }
            return new { scenes };
        }

        public static object GetActiveScene()
        {
            var scene = SceneManager.GetActiveScene();
            return new 
            { 
                name = scene.name, 
                path = scene.path, 
                isLoaded = scene.isLoaded, 
                rootCount = scene.rootCount 
            };
        }

        public static object GetSceneHierarchy()
        {
            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            var hierarchy = new List<object>();
            foreach (var obj in roots) 
                hierarchy.Add(UnityMCPBridge.BuildHierarchyNode(obj));
            return new { hierarchy };
        }

        public static object CreateScene(string name, string path)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
            EditorSceneManager.SaveScene(scene, path);
            return new { success = true, path, message = $"Created scene '{name}' at '{path}'" };
        }

        public static object SaveScene()
        {
            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(scene);
            return new { success = true, path = scene.path, message = $"Saved scene '{scene.name}'" };
        }

        public static object LoadScene(string path)
        {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            return new { success = true, path, message = $"Loaded scene '{path}'" };
        }
    }
}
#endif

