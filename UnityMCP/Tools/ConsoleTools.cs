// Tools/ConsoleTools.cs
// Console log operations
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityMCP
{
    public static class ConsoleTools
    {
        public static object GetLogs(int count)
        {
            var logs = UnityMCPBridge.GetConsoleLogs();
            var start = Math.Max(0, logs.Count - count);
            var result = logs.GetRange(start, Math.Min(count, logs.Count - start));
            return new { logs = result, count = result.Count };
        }

        public static object ClearConsole()
        {
            UnityMCPBridge.GetConsoleLogs().Clear();

            var editorAsm = typeof(Editor).Assembly;
            var logEntriesType = editorAsm.GetType("UnityEditor.LogEntries");
            var clearMethod = logEntriesType?.GetMethod(
                "Clear",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (clearMethod == null)
                throw new Exception("UnityEditor.LogEntries.Clear not found");

            clearMethod.Invoke(null, null);
            return new { success = true, message = "Console cleared" };
        }
    }
}
#endif

