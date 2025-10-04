// Tools/TerrainTools.cs
// Terrain creation and configuration
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    public static class TerrainTools
    {
        public static object CreateTerrain(JObject args)
        {
            var name = args.Value<string>("name");
            var terrainData = new TerrainData();

            if (args["width"] != null && args["length"] != null)
            {
                terrainData.size = new Vector3(
                    args["width"].Value<float>(),
                    args?["height"]?.Value<float>() ?? 600,
                    args["length"].Value<float>()
                );
            }

            var obj = Terrain.CreateTerrainGameObject(terrainData);
            obj.name = name;

            Undo.RegisterCreatedObjectUndo(obj, "Create Terrain");
            return new { success = true, name = obj.name, message = $"Created Terrain '{name}'" };
        }
    }
}
#endif

