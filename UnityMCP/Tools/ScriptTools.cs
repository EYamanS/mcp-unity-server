// Tools/ScriptTools.cs
// Script reading, writing, and generation
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    public static class ScriptTools
    {
        public static object ListScripts(string path = null)
        {
            string searchPath = string.IsNullOrEmpty(path) ? "Assets" : path;
            var guids = AssetDatabase.FindAssets("t:Script", new[] { searchPath });
            var scripts = new List<object>();
            
            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                scripts.Add(new 
                { 
                    path = assetPath, 
                    name = System.IO.Path.GetFileName(assetPath) 
                });
            }
            return new { scripts, count = scripts.Count };
        }

        public static object ReadScript(string path)
        {
            path = UnityMCPBridge.NormalizePath(path, "Assets/");
            var full = System.IO.Path.Combine(Application.dataPath.Replace("Assets", ""), path);
            if (!System.IO.File.Exists(full)) throw new Exception($"Script not found: {path}");
            
            string content = System.IO.File.ReadAllText(full);
            return new { path, content };
        }

        public static object WriteScript(string path, string content)
        {
            path = UnityMCPBridge.NormalizePath(path, "Assets/");
            var full = System.IO.Path.Combine(Application.dataPath.Replace("Assets", ""), path);
            var dir = System.IO.Path.GetDirectoryName(full);
            
            if (!System.IO.Directory.Exists(dir)) 
                System.IO.Directory.CreateDirectory(dir);
            
            System.IO.File.WriteAllText(full, content);
            AssetDatabase.Refresh();
            return new { success = true, path, message = "Script written successfully" };
        }

        public static object GenerateComponentScript(JObject args)
        {
            var name = args.Value<string>("name");
            var path = args.Value<string>("path");
            var template = args?["template"]?.ToString() ?? "monobehaviour";
            var ns = args?["namespace"]?.ToString();

            path = UnityMCPBridge.NormalizePath(path, "Assets/", ".cs");

            string content = template switch
            {
                "basic" => Templates.GenerateBasic(name, ns),
                "monobehaviour" => Templates.GenerateMonoBehaviour(name, ns),
                "networkbehaviour" => Templates.GenerateNetworkBehaviour(name, ns),
                _ => Templates.GenerateMonoBehaviour(name, ns)
            };

            var full = System.IO.Path.Combine(Application.dataPath.Replace("Assets", ""), path);
            var dir = System.IO.Path.GetDirectoryName(full);
            if (!System.IO.Directory.Exists(dir)) 
                System.IO.Directory.CreateDirectory(dir);
            
            System.IO.File.WriteAllText(full, content);
            AssetDatabase.Refresh();

            return new { success = true, path, message = $"Generated {template} script '{name}' at '{path}'" };
        }

        // ===== Script Templates =====
        
        private static class Templates
        {
            public static string GenerateBasic(string name, string ns)
            {
                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(ns)) sb.AppendLine($"namespace {ns}\n{{");
                
                sb.AppendLine($"    public class {name}");
                sb.AppendLine("    {");
                sb.AppendLine("        // Add your code here");
                sb.AppendLine("    }");
                
                if (!string.IsNullOrEmpty(ns)) sb.AppendLine("}");
                return sb.ToString();
            }

            public static string GenerateMonoBehaviour(string name, string ns)
            {
                var sb = new StringBuilder();
                sb.AppendLine("using UnityEngine;");
                sb.AppendLine();
                
                if (!string.IsNullOrEmpty(ns)) sb.AppendLine($"namespace {ns}\n{{");
                
                sb.AppendLine($"    public class {name} : MonoBehaviour");
                sb.AppendLine("    {");
                sb.AppendLine("        void Start()");
                sb.AppendLine("        {");
                sb.AppendLine("            // Initialization code");
                sb.AppendLine("        }");
                sb.AppendLine();
                sb.AppendLine("        void Update()");
                sb.AppendLine("        {");
                sb.AppendLine("            // Update code");
                sb.AppendLine("        }");
                sb.AppendLine("    }");
                
                if (!string.IsNullOrEmpty(ns)) sb.AppendLine("}");
                return sb.ToString();
            }

            public static string GenerateNetworkBehaviour(string name, string ns)
            {
                var sb = new StringBuilder();
                sb.AppendLine("using UnityEngine;");
                sb.AppendLine("using FishNet.Object;");
                sb.AppendLine();
                
                if (!string.IsNullOrEmpty(ns)) sb.AppendLine($"namespace {ns}\n{{");
                
                sb.AppendLine($"    public class {name} : NetworkBehaviour");
                sb.AppendLine("    {");
                sb.AppendLine("        public override void OnStartClient()");
                sb.AppendLine("        {");
                sb.AppendLine("            base.OnStartClient();");
                sb.AppendLine("            // Client initialization");
                sb.AppendLine("        }");
                sb.AppendLine();
                sb.AppendLine("        public override void OnStartServer()");
                sb.AppendLine("        {");
                sb.AppendLine("            base.OnStartServer();");
                sb.AppendLine("            // Server initialization");
                sb.AppendLine("        }");
                sb.AppendLine("    }");
                
                if (!string.IsNullOrEmpty(ns)) sb.AppendLine("}");
                return sb.ToString();
            }
        }
    }
}
#endif

