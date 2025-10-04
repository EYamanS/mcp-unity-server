// Tools/AudioTools.cs
// Audio source configuration
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    public static class AudioTools
    {
        public static object AddAudioSource(JObject args)
        {
            var goName = args.Value<string>("gameObject");
            var obj = UnityMCPBridge.FindGameObjectOrThrow(goName);
            var audioSource = obj.GetComponent<AudioSource>() ?? Undo.AddComponent<AudioSource>(obj);

            Undo.RecordObject(audioSource, "Configure AudioSource");

            if (args["clip"] is JToken clipPath)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath.Value<string>());
                if (clip != null) audioSource.clip = clip;
            }

            if (args["playOnAwake"] != null) audioSource.playOnAwake = args["playOnAwake"].Value<bool>();
            if (args["loop"] != null) audioSource.loop = args["loop"].Value<bool>();
            if (args["volume"] != null) audioSource.volume = args["volume"].Value<float>();

            return new { success = true, message = $"Configured AudioSource on '{goName}'" };
        }
    }
}
#endif

