// Tools/LightingTools.cs
// Lighting creation and ambient light configuration
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    public static class LightingTools
    {
        public static object CreateLight(JObject args)
        {
            var name = args.Value<string>("name");
            var lightTypeStr = args.Value<string>("lightType");
            
            var obj = new GameObject(name);
            var light = Undo.AddComponent<Light>(obj);

            light.type = lightTypeStr switch
            {
                "Directional" => LightType.Directional,
                "Point" => LightType.Point,
                "Spot" => LightType.Spot,
                "Area" => LightType.Area,
                _ => throw new Exception($"Unknown light type: {lightTypeStr}")
            };

            if (args["color"] is JObject col)
                light.color = UnityMCPBridge.ParseColor(col, Color.white);
            
            if (args["intensity"] != null) light.intensity = args["intensity"].Value<float>();
            if (args["range"] != null) light.range = args["range"].Value<float>();

            Undo.RegisterCreatedObjectUndo(obj, "Create Light");
            return new { success = true, name = obj.name, message = $"Created {lightTypeStr} light '{name}'" };
        }

        public static object SetAmbientLight(JObject args)
        {
            // RenderSettings is a static class, no need for Undo.RecordObject
            if (args["color"] is JObject col)
                RenderSettings.ambientLight = UnityMCPBridge.ParseColor(col, new Color(0.5f, 0.5f, 0.5f));

            if (args["intensity"] != null) 
                RenderSettings.ambientIntensity = args["intensity"].Value<float>();

            return new { success = true, message = "Set ambient light" };
        }
    }
}
#endif

