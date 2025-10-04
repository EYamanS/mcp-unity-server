// UnityMCPBridge.Core.cs
// Main bridge class with WebSocket connection logic
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    [InitializeOnLoad]
    public static partial class UnityMCPBridge
    {
        private static ClientWebSocket _ws;
        private static CancellationTokenSource _cts;
        private static readonly Uri _uri = new Uri("ws://127.0.0.1:8765");
        private static readonly Queue<Action> _mainThreadQueue = new Queue<Action>();
        private static int _clientCount = 0;

        static UnityMCPBridge()
        {
            EditorApplication.update += ProcessMainThreadQueue;
            Application.logMessageReceived += LogCallback;
            Connect();
        }

        // ===== Public API =====
        
        public static bool IsConnected => _ws != null && _ws.State == WebSocketState.Open;
        public static int ClientCount => _clientCount;
        public static string Url => _uri.ToString();
        public static List<string> GetRecentLogs() => new List<string>(consoleLogs);

        // ===== Connection Management =====

        public static async void Connect()
        {
            if (_ws != null && _ws.State == WebSocketState.Open) return;

            _cts = new CancellationTokenSource();
            _ws = new ClientWebSocket();

            try
            {
                await _ws.ConnectAsync(_uri, _cts.Token);
                _clientCount = 1;
                UnityMCPWindow.Log("[Unity MCP] Connected to Node WS");
                _ = ReceiveLoop();
            }
            catch (Exception e)
            {
                _clientCount = 0;
                Debug.LogError($"[Unity MCP] WS connect failed: {e.Message}");
            }
        }

        public static async void Disconnect()
        {
            try
            {
                _cts?.Cancel();
                if (_ws != null)
                {
                    if (_ws.State == WebSocketState.Open)
                        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "User closed", CancellationToken.None);
                    _ws.Dispose();
                    _ws = null;
                }
            }
            catch { }
            finally
            {
                _clientCount = 0;
                UnityMCPWindow.Log("[Unity MCP] Disconnected from Node WS");
            }
        }

        private static async Task ReceiveLoop()
        {
            var buffer = new byte[64 * 1024];
            try
            {
                while (_ws.State == WebSocketState.Open)
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", _cts.Token);
                        break;
                    }

                    string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    string response = await ProcessMessage(json);

                    var bytes = Encoding.UTF8.GetBytes(response);
                    await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts.Token);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { Debug.LogError($"[Unity MCP] WS error: {e.Message}"); }
            finally
            {
                _clientCount = 0;
                UnityMCPWindow.Log("[Unity MCP] Disconnected from Node WS");
            }
        }

        // ===== JSON-RPC Protocol =====

        [Serializable]
        public class MCPRequest
        {
            public string jsonrpc;
            public string id;
            public string method;
            public Dictionary<string, object> @params;
        }

        private static async Task<string> ProcessMessage(string message)
        {
            try
            {
                var request = JsonConvert.DeserializeObject<MCPRequest>(message);
                var method = request?.method ?? "";

                switch (method)
                {
                    case "initialize":
                        return CreateResponse(request.id, GetServerInfo());
                    case "tools/list":
                        return CreateResponse(request.id, new { tools = GetToolsList() });
                    case "tools/call":
                    {
                        var p = request.@params ?? new Dictionary<string, object>();
                        var nameToken = p.TryGetValue("name", out var n) ? JToken.FromObject(n) : null;
                        var argsToken = p.TryGetValue("arguments", out var a) ? JToken.FromObject(a) : null;
                        var toolName = nameToken?.Value<string>() ?? throw new Exception("Missing 'name'");
                        var args = (argsToken as JObject) ?? new JObject();
                        var result = await ExecuteTool(toolName, args);
                        return CreateResponse(request.id, result);
                    }
                    default:
                        return CreateErrorResponse(request?.id, $"Unknown method: {request?.method}");
                }
            }
            catch (Exception e) { return CreateErrorResponse(null, e.Message); }
        }

        private static string CreateResponse(string id, object result) =>
            JsonConvert.SerializeObject(new { jsonrpc = "2.0", id, result });

        private static string CreateErrorResponse(string id, string message) =>
            JsonConvert.SerializeObject(new { jsonrpc = "2.0", id, error = new { code = -32000, message } });

        private static object GetServerInfo() => new
        {
            protocolVersion = "2024-11-05",
            capabilities = new { tools = new { }, experimental = new { } },
            serverInfo = new { name = "unity-mcp-bridge", version = "0.2.0" }
        };

        private static List<object> GetToolsList() => new List<object>
        {
            new { name = "unity_get_project_info" },
            new { name = "unity_list_scenes" },
            new { name = "unity_list_scripts" },
            new { name = "unity_get_active_scene" },
            new { name = "unity_get_scene_hierarchy" },
            new { name = "unity_find_gameobject" },
            new { name = "unity_get_gameobject_info" },
            new { name = "unity_create_gameobject" },
            new { name = "unity_delete_gameobject" },
            new { name = "unity_clone_gameobject" },
            new { name = "unity_set_gameobject_transform" },
            new { name = "unity_set_gameobject_active" },
            new { name = "unity_set_gameobject_parent" },
            new { name = "unity_set_gameobject_tag" },
            new { name = "unity_set_gameobject_layer" },
            new { name = "unity_batch_create_gameobjects" },
            new { name = "unity_read_script" },
            new { name = "unity_write_script" },
            new { name = "unity_generate_component_script" },
            new { name = "unity_add_component" },
            new { name = "unity_get_component_properties" },
            new { name = "unity_set_component_property" },
            new { name = "unity_remove_component" },
            new { name = "unity_list_prefabs" },
            new { name = "unity_instantiate_prefab" },
            new { name = "unity_create_prefab" },
            new { name = "unity_create_material" },
            new { name = "unity_list_materials" },
            new { name = "unity_set_material_property" },
            new { name = "unity_assign_material" },
            new { name = "unity_create_scene" },
            new { name = "unity_save_scene" },
            new { name = "unity_load_scene" },
            new { name = "unity_configure_rigidbody" },
            new { name = "unity_configure_collider" },
            new { name = "unity_create_light" },
            new { name = "unity_set_ambient_light" },
            new { name = "unity_create_camera" },
            new { name = "unity_configure_camera" },
            new { name = "unity_create_canvas" },
            new { name = "unity_create_ui_element" },
            new { name = "unity_set_rect_transform" },
            new { name = "unity_create_folder" },
            new { name = "unity_list_assets" },
            new { name = "unity_create_primitive" },
            new { name = "unity_add_audio_source" },
            new { name = "unity_create_particle_system" },
            new { name = "unity_create_terrain" },
            new { name = "unity_add_animator" },
            new { name = "unity_get_console_logs" },
            new { name = "unity_clear_console" },
            new { name = "unity_enter_playmode" },
            new { name = "unity_exit_playmode" },
            new { name = "unity_is_playing" },
        };

        // ===== Main Thread Execution =====

        private static Task<object> ExecuteTool(string toolName, JObject args)
        {
            var tcs = new TaskCompletionSource<object>();
            lock (_mainThreadQueue)
            {
                _mainThreadQueue.Enqueue(() =>
                {
                    try
                    {
                        var result = ToolRouter.Route(toolName, args);
                        tcs.SetResult(result);
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(new Exception($"[{toolName}] {e.GetType().Name}: {e.Message}\n{e.StackTrace}"));
                    }
                });
            }
            return tcs.Task;
        }

        private static void ProcessMainThreadQueue()
        {
            lock (_mainThreadQueue)
            {
                while (_mainThreadQueue.Count > 0)
                    _mainThreadQueue.Dequeue()?.Invoke();
            }
        }

        // ===== Console Logging =====

        private static List<string> consoleLogs = new List<string>();
        
        private static void LogCallback(string message, string stackTrace, LogType type)
        {
            consoleLogs.Add($"[{type}] {message}");
            if (consoleLogs.Count > 100) consoleLogs.RemoveAt(0);
        }

        internal static List<string> GetConsoleLogs() => consoleLogs;
    }
}
#endif

