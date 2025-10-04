// UnityMCPBridge.Router.cs
// Central routing for all tool calls
#if UNITY_EDITOR
using Newtonsoft.Json.Linq;
using System;

namespace UnityMCP
{
    public static class ToolRouter
    {
        public static object Route(string toolName, JObject args)
        {
            // Route to appropriate tool category
            switch (toolName)
            {
                // Project & Info
                case "unity_get_project_info": return ProjectTools.GetProjectInfo();
                case "unity_list_scenes": return SceneTools.ListScenes();
                case "unity_list_scripts": return ScriptTools.ListScripts(args?["path"]?.ToString());

                // Scene Operations
                case "unity_get_active_scene": return SceneTools.GetActiveScene();
                case "unity_get_scene_hierarchy": return SceneTools.GetSceneHierarchy();
                case "unity_create_scene": return SceneTools.CreateScene(args.Value<string>("name"), args.Value<string>("path"));
                case "unity_save_scene": return SceneTools.SaveScene();
                case "unity_load_scene": return SceneTools.LoadScene(args.Value<string>("path"));

                // GameObject Operations
                case "unity_find_gameobject": return GameObjectTools.FindGameObject(args.Value<string>("name"));
                case "unity_get_gameobject_info": return GameObjectTools.GetGameObjectInfo(args.Value<string>("name"));
                case "unity_create_gameobject": return GameObjectTools.CreateGameObject(args.Value<string>("name"), args?["parent"]?.ToString());
                case "unity_delete_gameobject": return GameObjectTools.DeleteGameObject(args.Value<string>("name"));
                case "unity_clone_gameobject": return GameObjectTools.CloneGameObject(args.Value<string>("name"), args.Value<string>("newName"));
                case "unity_set_gameobject_transform": return GameObjectTools.SetTransform(args);
                case "unity_set_gameobject_active": return GameObjectTools.SetActive(args.Value<string>("name"), args.Value<bool>("active"));
                case "unity_set_gameobject_parent": return GameObjectTools.SetParent(args.Value<string>("name"), args?["parentName"]?.ToString());
                case "unity_set_gameobject_tag": return GameObjectTools.SetTag(args.Value<string>("name"), args.Value<string>("tag"));
                case "unity_set_gameobject_layer": return GameObjectTools.SetLayer(args.Value<string>("name"), args.Value<string>("layer"));
                case "unity_batch_create_gameobjects": return GameObjectTools.BatchCreate(args);
                case "unity_create_primitive": return GameObjectTools.CreatePrimitive(args);

                // Component Operations
                case "unity_add_component": return ComponentTools.AddComponent(args.Value<string>("gameObject"), args.Value<string>("component"));
                case "unity_get_component_properties": return ComponentTools.GetProperties(args.Value<string>("gameObject"), args.Value<string>("component"));
                case "unity_set_component_property": return ComponentTools.SetProperty(args);
                case "unity_remove_component": return ComponentTools.RemoveComponent(args.Value<string>("gameObject"), args.Value<string>("component"));

                // Prefab Operations
                case "unity_list_prefabs": return PrefabTools.ListPrefabs(args?["path"]?.ToString());
                case "unity_instantiate_prefab": return PrefabTools.InstantiatePrefab(args);
                case "unity_create_prefab": return PrefabTools.CreatePrefab(args.Value<string>("gameObject"), args.Value<string>("path"));

                // Material Operations
                case "unity_create_material": return MaterialTools.CreateMaterial(args);
                case "unity_list_materials": return MaterialTools.ListMaterials(args?["path"]?.ToString());
                case "unity_set_material_property": return MaterialTools.SetProperty(args);
                case "unity_assign_material": return MaterialTools.AssignToRenderer(args);

                // Physics Operations
                case "unity_configure_rigidbody": return PhysicsTools.ConfigureRigidbody(args);
                case "unity_configure_collider": return PhysicsTools.ConfigureCollider(args);

                // Lighting Operations
                case "unity_create_light": return LightingTools.CreateLight(args);
                case "unity_set_ambient_light": return LightingTools.SetAmbientLight(args);

                // Camera Operations
                case "unity_create_camera": return CameraTools.CreateCamera(args);
                case "unity_configure_camera": return CameraTools.ConfigureCamera(args);

                // UI Operations
                case "unity_create_canvas": return UITools.CreateCanvas(args);
                case "unity_create_ui_element": return UITools.CreateElement(args);
                case "unity_set_rect_transform": return UITools.SetRectTransform(args);

                // Asset Operations
                case "unity_create_folder": return AssetTools.CreateFolder(args.Value<string>("path"));
                case "unity_list_assets": return AssetTools.ListAssets(args.Value<string>("type"), args?["path"]?.ToString());
                case "unity_get_asset_path": return AssetTools.GetAssetPath(args.Value<string>("name"), args.Value<string>("assetType"));

                // Specialized GameObject Creation
                case "unity_add_audio_source": return AudioTools.AddAudioSource(args);
                case "unity_create_particle_system": return ParticleTools.CreateParticleSystem(args);
                case "unity_create_terrain": return TerrainTools.CreateTerrain(args);
                case "unity_add_animator": return AnimationTools.AddAnimator(args);

                // Script Operations
                case "unity_read_script": return ScriptTools.ReadScript(args.Value<string>("path"));
                case "unity_write_script": return ScriptTools.WriteScript(args.Value<string>("path"), args.Value<string>("content"));
                case "unity_generate_component_script": return ScriptTools.GenerateComponentScript(args);

                // Console & Playmode
                case "unity_get_console_logs": return ConsoleTools.GetLogs(args?["count"]?.Value<int>() ?? 50);
                case "unity_clear_console": return ConsoleTools.ClearConsole();
                case "unity_enter_playmode": return PlayModeTools.EnterPlayMode();
                case "unity_exit_playmode": return PlayModeTools.ExitPlayMode();
                case "unity_is_playing": return PlayModeTools.IsPlaying();

                default:
                    throw new Exception($"Unknown tool: {toolName}");
            }
        }
    }
}
#endif

