// Assets/Editor/UnityMCPWindow.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UnityMCP
{
    public class UnityMCPWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<string> activityLog = new List<string>();
        private int maxLogEntries = 200;

        [MenuItem("Window/Unity MCP/Control Panel")]
        public static void ShowWindow()
        {
            GetWindow<UnityMCPWindow>("Unity MCP");
        }

        private void OnEnable()
        {
            EditorApplication.update += Repaint;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }

        private void OnGUI()
        {
            GUILayout.Label("Unity MCP Bridge", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawServerStatus();
            EditorGUILayout.Space();

            DrawConnectionInfo();
            EditorGUILayout.Space();

            DrawActivityLog();
            EditorGUILayout.Space();

            DrawQuickActions();
        }

        private void DrawServerStatus()
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            Color statusColor = UnityMCPBridge.IsConnected ? Color.green : Color.red;
            GUI.color = statusColor;
            GUILayout.Label("●", GUILayout.Width(20));
            GUI.color = Color.white;

            GUILayout.Label(UnityMCPBridge.IsConnected ? "Connected to Node WS" : "Disconnected");

            GUILayout.FlexibleSpace();

            if (UnityMCPBridge.IsConnected)
            {
                if (GUILayout.Button("Disconnect", GUILayout.Width(100)))
                {
                    UnityMCPBridge.Disconnect();
                    AddLog("Disconnected by user");
                }
            }
            else
            {
                if (GUILayout.Button("Connect", GUILayout.Width(100)))
                {
                    UnityMCPBridge.Connect();
                    AddLog("Connect requested by user");
                }
            }

            GUILayout.EndHorizontal();
        }

        private void DrawConnectionInfo()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Connection Info", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("WebSocket URL:", UnityMCPBridge.Url);
            EditorGUILayout.LabelField("Connected:", UnityMCPBridge.IsConnected ? "Yes" : "No");
            EditorGUILayout.LabelField("Client Count:", UnityMCPBridge.ClientCount.ToString());

            if (GUILayout.Button("Copy WebSocket URL"))
            {
                GUIUtility.systemCopyBuffer = UnityMCPBridge.Url;
                AddLog("WebSocket URL copied to clipboard");
            }

            GUILayout.EndVertical();
        }

        private void DrawActivityLog()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Activity Log", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear", GUILayout.Width(60)))
            {
                activityLog.Clear();
            }
            GUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(220));

            foreach (var log in activityLog)
                EditorGUILayout.LabelField(log, EditorStyles.wordWrappedLabel);

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Dump Console Logs to Console"))
            {
                var logs = UnityMCPBridge.GetRecentLogs();
                Debug.Log("=== Unity MCP Recent Logs ===\n" + string.Join("\n", logs));
            }

            GUILayout.EndVertical();
        }

        private void DrawQuickActions()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Quick Actions", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Open Repo"))
            {
                Application.OpenURL("https://github.com/yourusername/unity-mcp");
            }

            if (GUILayout.Button("Enter Play Mode"))
            {
                EditorApplication.EnterPlaymode();
            }

            if (GUILayout.Button("Exit Play Mode"))
            {
                EditorApplication.ExitPlaymode();
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "Cursor/Claude should launch the MCP (Node) process.\n" +
                "This Unity Bridge connects to ws://127.0.0.1:8765.\n" +
                "Use tools from your AI client to interact with this project.",
                MessageType.Info
            );

            GUILayout.EndVertical();
        }

        private void AddLog(string message)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            activityLog.Add($"[{timestamp}] {message}");
            if (activityLog.Count > maxLogEntries) activityLog.RemoveAt(0);
            Repaint();
        }

        // Allow other classes to push UI logs
        public static void Log(string message)
        {
            var window = GetWindow<UnityMCPWindow>(false, "Unity MCP", false);
            if (window != null) window.AddLog(message);
        }
    }
}
#endif
