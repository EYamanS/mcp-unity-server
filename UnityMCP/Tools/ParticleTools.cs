// Tools/ParticleTools.cs
// Particle system creation
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    public static class ParticleTools
    {
        public static object CreateParticleSystem(JObject args)
        {
            var name = args.Value<string>("name");
            var obj = new GameObject(name);
            Undo.AddComponent<ParticleSystem>(obj);

            if (args["position"] is JObject pos)
                obj.transform.position = UnityMCPBridge.ParseVector3(pos);

            Undo.RegisterCreatedObjectUndo(obj, "Create Particle System");
            return new { success = true, name = obj.name, message = $"Created Particle System '{name}'" };
        }
    }
}
#endif

