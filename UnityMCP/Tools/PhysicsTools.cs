// Tools/PhysicsTools.cs
// Physics configuration (Rigidbody, Colliders, etc.)
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    public static class PhysicsTools
    {
        public static object ConfigureRigidbody(JObject args)
        {
            var goName = args.Value<string>("gameObject");
            var obj = UnityMCPBridge.FindGameObjectOrThrow(goName);
            var rb = obj.GetComponent<Rigidbody>();
            
            if (rb == null)
                rb = Undo.AddComponent<Rigidbody>(obj);
            else
                Undo.RecordObject(rb, "Configure Rigidbody");

            if (args["mass"] != null) rb.mass = args["mass"].Value<float>();
            if (args["drag"] != null) rb.drag = args["drag"].Value<float>();
            if (args["angularDrag"] != null) rb.angularDrag = args["angularDrag"].Value<float>();
            if (args["useGravity"] != null) rb.useGravity = args["useGravity"].Value<bool>();
            if (args["isKinematic"] != null) rb.isKinematic = args["isKinematic"].Value<bool>();

            return new { success = true, message = $"Configured Rigidbody on '{goName}'" };
        }

        public static object ConfigureCollider(JObject args)
        {
            var goName = args.Value<string>("gameObject");
            var colliderType = args.Value<string>("colliderType");
            var obj = UnityMCPBridge.FindGameObjectOrThrow(goName);

            var isTrigger = args?["isTrigger"]?.Value<bool>();
            var center = args["center"] as JObject;

            switch (colliderType)
            {
                case "BoxCollider":
                    return ConfigureBoxCollider(obj, isTrigger, center, args["size"] as JObject);
                    
                case "SphereCollider":
                    return ConfigureSphereCollider(obj, isTrigger, center, args["size"] as JObject);
                    
                case "CapsuleCollider":
                    return ConfigureCapsuleCollider(obj, isTrigger, center, args["size"] as JObject);
                    
                case "MeshCollider":
                    return ConfigureMeshCollider(obj, isTrigger);

                default:
                    throw new Exception($"Unknown collider type: {colliderType}");
            }
        }

        private static object ConfigureBoxCollider(GameObject obj, bool? isTrigger, JObject center, JObject size)
        {
            var box = obj.GetComponent<BoxCollider>() ?? Undo.AddComponent<BoxCollider>(obj);
            Undo.RecordObject(box, "Configure BoxCollider");
            
            if (isTrigger.HasValue) box.isTrigger = isTrigger.Value;
            if (center != null) box.center = UnityMCPBridge.ParseVector3(center, box.center);
            if (size != null) box.size = UnityMCPBridge.ParseVector3(size, box.size);

            return new { success = true, message = $"Configured BoxCollider on '{obj.name}'" };
        }

        private static object ConfigureSphereCollider(GameObject obj, bool? isTrigger, JObject center, JObject size)
        {
            var sphere = obj.GetComponent<SphereCollider>() ?? Undo.AddComponent<SphereCollider>(obj);
            Undo.RecordObject(sphere, "Configure SphereCollider");
            
            if (isTrigger.HasValue) sphere.isTrigger = isTrigger.Value;
            if (center != null) sphere.center = UnityMCPBridge.ParseVector3(center, sphere.center);
            if (size?["radius"] != null) sphere.radius = size["radius"].Value<float>();

            return new { success = true, message = $"Configured SphereCollider on '{obj.name}'" };
        }

        private static object ConfigureCapsuleCollider(GameObject obj, bool? isTrigger, JObject center, JObject size)
        {
            var capsule = obj.GetComponent<CapsuleCollider>() ?? Undo.AddComponent<CapsuleCollider>(obj);
            Undo.RecordObject(capsule, "Configure CapsuleCollider");
            
            if (isTrigger.HasValue) capsule.isTrigger = isTrigger.Value;
            if (center != null) capsule.center = UnityMCPBridge.ParseVector3(center, capsule.center);
            if (size?["radius"] != null) capsule.radius = size["radius"].Value<float>();
            if (size?["height"] != null) capsule.height = size["height"].Value<float>();

            return new { success = true, message = $"Configured CapsuleCollider on '{obj.name}'" };
        }

        private static object ConfigureMeshCollider(GameObject obj, bool? isTrigger)
        {
            var mesh = obj.GetComponent<MeshCollider>() ?? Undo.AddComponent<MeshCollider>(obj);
            Undo.RecordObject(mesh, "Configure MeshCollider");
            
            if (isTrigger.HasValue) mesh.isTrigger = isTrigger.Value;

            return new { success = true, message = $"Configured MeshCollider on '{obj.name}'" };
        }
    }
}
#endif

