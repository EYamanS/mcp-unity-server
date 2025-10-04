// Tools/GameObjectTools.cs
// GameObject creation, manipulation, and hierarchy operations
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    public static class GameObjectTools
    {
        public static object FindGameObject(string name)
        {
            var obj = GameObject.Find(name);
            if (obj == null) return new { found = false, message = $"GameObject '{name}' not found" };
            return new { found = true, info = UnityMCPBridge.BuildHierarchyNode(obj) };
        }

        public static object GetGameObjectInfo(string name)
        {
            var obj = UnityMCPBridge.FindGameObjectOrThrow(name);
            var t = obj.transform;
            return new
            {
                name = obj.name,
                active = obj.activeSelf,
                tag = obj.tag,
                layer = LayerMask.LayerToName(obj.layer),
                position = new { x = t.position.x, y = t.position.y, z = t.position.z },
                rotation = new { x = t.eulerAngles.x, y = t.eulerAngles.y, z = t.eulerAngles.z },
                scale = new { x = t.localScale.x, y = t.localScale.y, z = t.localScale.z },
                components = UnityMCPBridge.GetComponentNames(obj)
            };
        }

        public static object CreateGameObject(string name, string parentName = null)
        {
            var obj = new GameObject(name);
            if (!string.IsNullOrEmpty(parentName))
            {
                var parent = GameObject.Find(parentName);
                if (parent != null) obj.transform.SetParent(parent.transform);
            }
            Undo.RegisterCreatedObjectUndo(obj, "Create GameObject");
            return new { success = true, name = obj.name, message = $"Created GameObject '{name}'" };
        }

        public static object DeleteGameObject(string name)
        {
            var obj = UnityMCPBridge.FindGameObjectOrThrow(name);
            Undo.DestroyObjectImmediate(obj);
            return new { success = true, message = $"Deleted GameObject '{name}'" };
        }

        public static object CloneGameObject(string name, string newName)
        {
            var obj = UnityMCPBridge.FindGameObjectOrThrow(name);
            var clone = UnityEngine.Object.Instantiate(obj);
            clone.name = newName;
            Undo.RegisterCreatedObjectUndo(clone, "Clone GameObject");
            return new { success = true, name = clone.name, message = $"Cloned '{name}' to '{newName}'" };
        }

        public static object SetTransform(JObject args)
        {
            var name = args.Value<string>("name");
            var obj = UnityMCPBridge.FindGameObjectOrThrow(name);
            
            Undo.RecordObject(obj.transform, "Set Transform");

            if (args["position"] is JObject pos)
                obj.transform.position = UnityMCPBridge.ParseVector3(pos, obj.transform.position);

            if (args["rotation"] is JObject rot)
                obj.transform.eulerAngles = UnityMCPBridge.ParseVector3(rot, obj.transform.eulerAngles);

            if (args["scale"] is JObject scl)
                obj.transform.localScale = UnityMCPBridge.ParseVector3(scl, obj.transform.localScale);

            return new { success = true, message = $"Updated transform for '{name}'" };
        }

        public static object SetActive(string name, bool active)
        {
            var obj = UnityMCPBridge.FindGameObjectOrThrow(name);
            Undo.RecordObject(obj, "Set Active");
            obj.SetActive(active);
            return new { success = true, message = $"Set '{name}' active = {active}" };
        }

        public static object SetParent(string name, string parentName)
        {
            var obj = UnityMCPBridge.FindGameObjectOrThrow(name);
            Undo.RecordObject(obj.transform, "Set Parent");
            
            if (string.IsNullOrEmpty(parentName))
                obj.transform.SetParent(null);
            else
            {
                var parent = UnityMCPBridge.FindGameObjectOrThrow(parentName);
                obj.transform.SetParent(parent.transform);
            }
            return new { success = true, message = $"Set parent of '{name}' to '{parentName ?? "null"}'" };
        }

        public static object SetTag(string name, string tag)
        {
            var obj = UnityMCPBridge.FindGameObjectOrThrow(name);
            Undo.RecordObject(obj, "Set Tag");
            obj.tag = tag;
            return new { success = true, message = $"Set tag of '{name}' to '{tag}'" };
        }

        public static object SetLayer(string name, string layerName)
        {
            var obj = UnityMCPBridge.FindGameObjectOrThrow(name);
            var layer = LayerMask.NameToLayer(layerName);
            if (layer == -1) throw new Exception($"Layer '{layerName}' not found");
            
            Undo.RecordObject(obj, "Set Layer");
            obj.layer = layer;
            return new { success = true, message = $"Set layer of '{name}' to '{layerName}'" };
        }

        public static object BatchCreate(JObject args)
        {
            var objectsArray = args["objects"] as JArray;
            if (objectsArray == null) throw new Exception("'objects' array is required");

            var created = new List<string>();

            foreach (JObject objDef in objectsArray)
            {
                var name = objDef.Value<string>("name");
                var parentName = objDef?["parent"]?.ToString();
                var position = objDef["position"] as JObject;
                var components = objDef["components"] as JArray;

                var obj = new GameObject(name);

                if (!string.IsNullOrEmpty(parentName))
                {
                    var parent = GameObject.Find(parentName);
                    if (parent != null) obj.transform.SetParent(parent.transform);
                }

                if (position != null)
                    obj.transform.position = UnityMCPBridge.ParseVector3(position);

                if (components != null)
                {
                    foreach (var compToken in components)
                    {
                        var compType = compToken.Value<string>();
                        var type = UnityMCPBridge.ResolveType(compType);
                        if (type != null) Undo.AddComponent(obj, type);
                    }
                }

                Undo.RegisterCreatedObjectUndo(obj, "Batch Create GameObjects");
                created.Add(name);
            }

            return new { success = true, created, count = created.Count, message = $"Created {created.Count} GameObjects" };
        }

        public static object CreatePrimitive(JObject args)
        {
            var typeStr = args.Value<string>("type");
            var name = args.Value<string>("name");

            PrimitiveType primType = typeStr switch
            {
                "Cube" => PrimitiveType.Cube,
                "Sphere" => PrimitiveType.Sphere,
                "Capsule" => PrimitiveType.Capsule,
                "Cylinder" => PrimitiveType.Cylinder,
                "Plane" => PrimitiveType.Plane,
                "Quad" => PrimitiveType.Quad,
                _ => throw new Exception($"Unknown primitive type: {typeStr}")
            };

            var obj = GameObject.CreatePrimitive(primType);
            obj.name = name;

            if (args["position"] is JObject pos)
                obj.transform.position = UnityMCPBridge.ParseVector3(pos);

            if (args["scale"] is JObject scl)
                obj.transform.localScale = UnityMCPBridge.ParseVector3(scl, Vector3.one);

            Undo.RegisterCreatedObjectUndo(obj, "Create Primitive");
            return new { success = true, name = obj.name, message = $"Created {typeStr} primitive '{name}'" };
        }
    }
}
#endif

