// Tools/ComponentTools.cs
// Component add, remove, configure operations
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    public static class ComponentTools
    {
        public static object AddComponent(string goName, string componentType)
        {
            var obj = UnityMCPBridge.FindGameObjectOrThrow(goName);
            var type = UnityMCPBridge.ResolveType(componentType) 
                ?? throw new Exception($"Component type '{componentType}' not found");
            Undo.AddComponent(obj, type);
            return new { success = true, message = $"Added {type.FullName} to {goName}" };
        }

        public static object GetProperties(string goName, string componentType)
        {
            var obj = UnityMCPBridge.FindGameObjectOrThrow(goName);
            var type = UnityMCPBridge.ResolveType(componentType) 
                ?? throw new Exception($"Component type '{componentType}' not found");
            var component = obj.GetComponent(type) 
                ?? throw new Exception($"Component '{type.FullName}' not found on {goName}");

            var dict = new Dictionary<string, object>();
            var so = new SerializedObject(component);
            var it = so.GetIterator();
            
            if (it.NextVisible(true))
            {
                do
                {
                    if (it.propertyPath == "m_Script") continue;
                    dict[it.propertyPath] = UnityMCPBridge.GetSerializedValue(it);
                } while (it.NextVisible(false));
            }
            
            return new { component = type.FullName, properties = dict };
        }

        public static object SetProperty(JObject args)
        {
            var goName = args.Value<string>("gameObject");
            var componentType = args.Value<string>("component");
            var propertyName = args.Value<string>("property");
            var value = args["value"];

            var obj = UnityMCPBridge.FindGameObjectOrThrow(goName);
            var type = UnityMCPBridge.ResolveType(componentType) 
                ?? throw new Exception($"Component type '{componentType}' not found");
            var component = obj.GetComponent(type) 
                ?? throw new Exception($"Component '{componentType}' not found on '{goName}'");

            var so = new SerializedObject(component);
            var prop = so.FindProperty(propertyName) 
                ?? throw new Exception($"Property '{propertyName}' not found on {componentType}");

            Undo.RecordObject(component, "Set Component Property");
            UnityMCPBridge.SetSerializedPropertyValue(prop, value);
            so.ApplyModifiedProperties();

            return new { success = true, message = $"Set {componentType}.{propertyName} on '{goName}'" };
        }

        public static object RemoveComponent(string goName, string componentType)
        {
            var obj = UnityMCPBridge.FindGameObjectOrThrow(goName);
            var type = UnityMCPBridge.ResolveType(componentType) 
                ?? throw new Exception($"Component type '{componentType}' not found");
            var component = obj.GetComponent(type) 
                ?? throw new Exception($"Component '{componentType}' not found on '{goName}'");
            
            Undo.DestroyObjectImmediate(component);
            return new { success = true, message = $"Removed {componentType} from '{goName}'" };
        }
    }
}
#endif

