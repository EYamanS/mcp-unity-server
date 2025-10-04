// Tools/AnimationTools.cs
// Animation and Animator operations
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    public static class AnimationTools
    {
        public static object AddAnimator(JObject args)
        {
            var goName = args.Value<string>("gameObject");
            var obj = UnityMCPBridge.FindGameObjectOrThrow(goName);
            var animator = obj.GetComponent<Animator>() ?? Undo.AddComponent<Animator>(obj);

            if (args["controller"] is JToken controllerPath)
            {
                var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath.Value<string>());
                if (controller != null)
                {
                    Undo.RecordObject(animator, "Set Animator Controller");
                    animator.runtimeAnimatorController = controller;
                }
            }

            return new { success = true, message = $"Added/configured Animator on '{goName}'" };
        }
    }
}
#endif

