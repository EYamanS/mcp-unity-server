// UnityMCPBridge.Helpers.cs
// Shared utility methods and helpers
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    public static partial class UnityMCPBridge
    {
        // ===== Type Resolution =====

        internal static Type ResolveType(string componentType)
        {
            var t = Type.GetType(componentType);
            if (t != null) return t;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType(componentType);
                if (t != null) return t;
                foreach (var candidate in asm.GetTypes())
                    if (candidate.Name == componentType) return candidate;
            }
            return null;
        }

        // ===== GameObject Utilities =====

        internal static GameObject FindGameObjectOrThrow(string name)
        {
            var obj = GameObject.Find(name);
            if (obj == null) throw new Exception($"GameObject '{name}' not found");
            return obj;
        }

        internal static List<string> GetComponentNames(GameObject obj)
        {
            var comps = obj.GetComponents<Component>();
            var names = new List<string>();
            foreach (var c in comps) if (c != null) names.Add(c.GetType().Name);
            return names;
        }

        internal static object BuildHierarchyNode(GameObject obj)
        {
            var children = new List<object>();
            for (int i = 0; i < obj.transform.childCount; i++)
                children.Add(BuildHierarchyNode(obj.transform.GetChild(i).gameObject));

            return new
            {
                name = obj.name,
                active = obj.activeSelf,
                tag = obj.tag,
                layer = LayerMask.LayerToName(obj.layer),
                components = GetComponentNames(obj),
                children
            };
        }

        // ===== Vector Parsing =====

        internal static Vector3 ParseVector3(JObject obj, Vector3 defaultValue = default)
        {
            if (obj == null) return defaultValue;
            return new Vector3(
                obj["x"]?.Value<float>() ?? defaultValue.x,
                obj["y"]?.Value<float>() ?? defaultValue.y,
                obj["z"]?.Value<float>() ?? defaultValue.z
            );
        }

        internal static Vector2 ParseVector2(JObject obj, Vector2 defaultValue = default)
        {
            if (obj == null) return defaultValue;
            return new Vector2(
                obj["x"]?.Value<float>() ?? defaultValue.x,
                obj["y"]?.Value<float>() ?? defaultValue.y
            );
        }

        internal static Color ParseColor(JObject obj, Color defaultValue = default)
        {
            if (obj == null) return defaultValue;
            return new Color(
                obj["r"]?.Value<float>() ?? defaultValue.r,
                obj["g"]?.Value<float>() ?? defaultValue.g,
                obj["b"]?.Value<float>() ?? defaultValue.b,
                obj["a"]?.Value<float>() ?? defaultValue.a
            );
        }

        // ===== SerializedProperty Utilities =====

        internal static object GetSerializedValue(SerializedProperty p)
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.Integer: return p.intValue;
                case SerializedPropertyType.Boolean: return p.boolValue;
                case SerializedPropertyType.Float: return p.floatValue;
                case SerializedPropertyType.String: return p.stringValue;
                case SerializedPropertyType.Color: return p.colorValue;
                case SerializedPropertyType.ObjectReference: return p.objectReferenceValue ? p.objectReferenceValue.name : null;
                case SerializedPropertyType.Enum:
                    return p.enumNames != null && p.enumValueIndex >= 0 && p.enumValueIndex < p.enumNames.Length 
                        ? p.enumNames[p.enumValueIndex] 
                        : p.enumValueIndex;
                case SerializedPropertyType.Vector2: return new { p.vector2Value.x, p.vector2Value.y };
                case SerializedPropertyType.Vector3: return new { p.vector3Value.x, p.vector3Value.y, p.vector3Value.z };
                case SerializedPropertyType.Vector4: return new { p.vector4Value.x, p.vector4Value.y, p.vector4Value.z, p.vector4Value.w };
                case SerializedPropertyType.Quaternion: return new { p.quaternionValue.x, p.quaternionValue.y, p.quaternionValue.z, p.quaternionValue.w };
                case SerializedPropertyType.Rect: return new { p.rectValue.x, p.rectValue.y, p.rectValue.width, p.rectValue.height };
                case SerializedPropertyType.Bounds: return new { center = p.boundsValue.center, size = p.boundsValue.size };
                default: return p.displayName;
            }
        }

        internal static void SetSerializedPropertyValue(SerializedProperty p, JToken value)
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.Integer:
                    p.intValue = value.Value<int>();
                    break;
                case SerializedPropertyType.Boolean:
                    p.boolValue = value.Value<bool>();
                    break;
                case SerializedPropertyType.Float:
                    p.floatValue = value.Value<float>();
                    break;
                case SerializedPropertyType.String:
                    p.stringValue = value.Value<string>();
                    break;
                case SerializedPropertyType.Color:
                    if (value is JObject col)
                        p.colorValue = ParseColor(col, p.colorValue);
                    break;
                case SerializedPropertyType.Vector2:
                    if (value is JObject v2)
                        p.vector2Value = ParseVector2(v2, p.vector2Value);
                    break;
                case SerializedPropertyType.Vector3:
                    if (value is JObject v3)
                        p.vector3Value = ParseVector3(v3, p.vector3Value);
                    break;
                case SerializedPropertyType.Enum:
                    p.enumValueIndex = value.Value<int>();
                    break;
                default:
                    throw new Exception($"Unsupported property type: {p.propertyType}");
            }
        }

        // ===== Path Utilities =====

        internal static string NormalizePath(string path, string prefix = "Assets/", string extension = null)
        {
            if (!path.StartsWith(prefix)) path = prefix + path;
            if (extension != null && !path.EndsWith(extension)) path += extension;
            return path;
        }

        internal static void EnsureDirectoryExists(string path)
        {
            var dir = System.IO.Path.GetDirectoryName(path);
            if (!AssetDatabase.IsValidFolder(dir))
            {
                var folders = dir.Split('/');
                string currentPath = folders[0];
                for (int i = 1; i < folders.Length; i++)
                {
                    string nextPath = currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(nextPath))
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    currentPath = nextPath;
                }
            }
        }
    }
}
#endif

