// Tools/PlayModeTools.cs
// Play mode control
#if UNITY_EDITOR
using UnityEditor;

namespace UnityMCP
{
    public static class PlayModeTools
    {
        public static object EnterPlayMode()
        {
            EditorApplication.EnterPlaymode();
            return new { success = true, message = "Entering play mode" };
        }

        public static object ExitPlayMode()
        {
            EditorApplication.ExitPlaymode();
            return new { success = true, message = "Exiting play mode" };
        }

        public static object IsPlaying()
        {
            return new { isPlaying = EditorApplication.isPlaying };
        }
    }
}
#endif

