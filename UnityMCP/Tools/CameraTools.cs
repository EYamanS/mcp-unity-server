// Tools/CameraTools.cs
// Camera creation and configuration
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    public static class CameraTools
    {
        public static object CreateCamera(JObject args)
        {
            var name = args?["name"]?.ToString() ?? "Main Camera";
            var obj = new GameObject(name);
            var cam = Undo.AddComponent<Camera>(obj);

            cam.tag = "MainCamera";
            Undo.AddComponent<AudioListener>(obj);

            if (args?["position"] is JObject pos)
                obj.transform.position = UnityMCPBridge.ParseVector3(pos, new Vector3(0, 1, -10));

            if (args?["fieldOfView"] != null) 
                cam.fieldOfView = args["fieldOfView"].Value<float>();

            if (args?["clearFlags"] is JToken flags)
            {
                cam.clearFlags = flags.Value<string>() switch
                {
                    "Skybox" => CameraClearFlags.Skybox,
                    "SolidColor" => CameraClearFlags.SolidColor,
                    "Depth" => CameraClearFlags.Depth,
                    "Nothing" => CameraClearFlags.Nothing,
                    _ => CameraClearFlags.Skybox
                };
            }

            Undo.RegisterCreatedObjectUndo(obj, "Create Camera");
            return new { success = true, name = obj.name, message = $"Created camera '{name}'" };
        }

        public static object ConfigureCamera(JObject args)
        {
            var name = args.Value<string>("name");
            var obj = UnityMCPBridge.FindGameObjectOrThrow(name);
            var cam = obj.GetComponent<Camera>() 
                ?? throw new Exception($"No Camera component on '{name}'");

            Undo.RecordObject(cam, "Configure Camera");

            if (args["fieldOfView"] != null) cam.fieldOfView = args["fieldOfView"].Value<float>();
            if (args["nearClip"] != null) cam.nearClipPlane = args["nearClip"].Value<float>();
            if (args["farClip"] != null) cam.farClipPlane = args["farClip"].Value<float>();
            if (args["isOrthographic"] != null) cam.orthographic = args["isOrthographic"].Value<bool>();
            if (args["orthographicSize"] != null) cam.orthographicSize = args["orthographicSize"].Value<float>();

            return new { success = true, message = $"Configured Camera on '{name}'" };
        }
    }
}
#endif

