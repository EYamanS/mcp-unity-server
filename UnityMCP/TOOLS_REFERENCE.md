# Unity MCP Tools Reference

Quick reference for all available game generation tools.

## 📦 GameObject Operations

| Tool | Description | Example |
|------|-------------|---------|
| `unity_create_gameobject` | Create empty GameObject | `{name: "Player", parent: "Characters"}` |
| `unity_delete_gameobject` | Delete GameObject | `{name: "OldObject"}` |
| `unity_clone_gameobject` | Duplicate GameObject | `{name: "Enemy", newName: "Enemy2"}` |
| `unity_set_gameobject_transform` | Set position/rotation/scale | `{name: "Player", position: {x:0, y:1, z:0}}` |
| `unity_set_gameobject_active` | Enable/disable | `{name: "UI", active: false}` |
| `unity_set_gameobject_parent` | Change parent | `{name: "Child", parentName: "Parent"}` |
| `unity_set_gameobject_tag` | Set tag | `{name: "Player", tag: "Player"}` |
| `unity_set_gameobject_layer` | Set layer | `{name: "Wall", layer: "Default"}` |
| `unity_create_primitive` | Create primitive | `{type: "Cube", name: "Box"}` |
| `unity_batch_create_gameobjects` | Create multiple | `{objects: [{name: "A"}, {name: "B"}]}` |

## 🔧 Component Operations

| Tool | Description | Example |
|------|-------------|---------|
| `unity_add_component` | Add component | `{gameObject: "Player", component: "Rigidbody"}` |
| `unity_remove_component` | Remove component | `{gameObject: "Player", component: "BoxCollider"}` |
| `unity_get_component_properties` | Get all properties | `{gameObject: "Player", component: "Transform"}` |
| `unity_set_component_property` | Set property value | `{gameObject: "Player", component: "Rigidbody", property: "mass", value: 2.0}` |

## 🎨 Material System

| Tool | Description | Example |
|------|-------------|---------|
| `unity_create_material` | Create material | `{name: "Red", path: "Materials/Red.mat", shader: "Standard"}` |
| `unity_list_materials` | List all materials | `{path: "Assets/Materials"}` |
| `unity_set_material_property` | Set material property | `{path: "Assets/Materials/Red.mat", property: "_Color", value: {r:1, g:0, b:0, a:1}}` |
| `unity_assign_material` | Assign to renderer | `{gameObject: "Cube", materialPath: "Assets/Materials/Red.mat"}` |

## 🎮 Prefab System

| Tool | Description | Example |
|------|-------------|---------|
| `unity_list_prefabs` | List prefabs | `{path: "Assets/Prefabs"}` |
| `unity_instantiate_prefab` | Spawn prefab | `{path: "Assets/Prefabs/Enemy.prefab", position: {x:5, y:0, z:0}}` |
| `unity_create_prefab` | Save as prefab | `{gameObject: "Player", path: "Prefabs/PlayerPrefab.prefab"}` |

## ⚡ Physics Configuration

| Tool | Description | Example |
|------|-------------|---------|
| `unity_configure_rigidbody` | Setup Rigidbody | `{gameObject: "Ball", mass: 1, useGravity: true, drag: 0.5}` |
| `unity_configure_collider` | Setup Collider | `{gameObject: "Box", colliderType: "BoxCollider", isTrigger: false, size: {x:1, y:1, z:1}}` |

## 💡 Lighting

| Tool | Description | Example |
|------|-------------|---------|
| `unity_create_light` | Create light | `{name: "Sun", lightType: "Directional", color: {r:1, g:1, b:1}, intensity: 1}` |
| `unity_set_ambient_light` | Set ambient | `{color: {r:0.3, g:0.3, b:0.4}, intensity: 1}` |

## 📷 Camera

| Tool | Description | Example |
|------|-------------|---------|
| `unity_create_camera` | Create camera | `{name: "MainCam", position: {x:0, y:5, z:-10}, fieldOfView: 60}` |
| `unity_configure_camera` | Configure camera | `{name: "MainCam", isOrthographic: true, orthographicSize: 10}` |

## 🖼️ UI Generation

| Tool | Description | Example |
|------|-------------|---------|
| `unity_create_canvas` | Create Canvas | `{name: "UICanvas", renderMode: "ScreenSpaceOverlay"}` |
| `unity_create_ui_element` | Create UI element | `{type: "Button", name: "PlayButton", parent: "Canvas", text: "Play"}` |
| `unity_set_rect_transform` | Configure RectTransform | `{name: "Panel", anchorMin: {x:0, y:0}, anchorMax: {x:1, y:1}}` |

## 📝 Script Generation

| Tool | Description | Example |
|------|-------------|---------|
| `unity_generate_component_script` | Generate script | `{name: "PlayerController", path: "Scripts/PlayerController.cs", template: "monobehaviour"}` |
| `unity_read_script` | Read script | `{path: "Scripts/GameManager.cs"}` |
| `unity_write_script` | Write script | `{path: "Scripts/Test.cs", content: "..."}` |

Templates: `basic`, `monobehaviour`, `networkbehaviour`

## 🗺️ Scene Management

| Tool | Description | Example |
|------|-------------|---------|
| `unity_create_scene` | Create new scene | `{name: "Level2", path: "Assets/Scenes/Level2.unity"}` |
| `unity_save_scene` | Save active scene | `{}` |
| `unity_load_scene` | Load scene | `{path: "Assets/Scenes/MainMenu.unity"}` |
| `unity_get_scene_hierarchy` | Get all GameObjects | `{}` |

## 🎵 Audio

| Tool | Description | Example |
|------|-------------|---------|
| `unity_add_audio_source` | Add AudioSource | `{gameObject: "Player", playOnAwake: true, loop: false, volume: 0.8}` |

## 🌋 Specialized Objects

| Tool | Description | Example |
|------|-------------|---------|
| `unity_create_particle_system` | Create particles | `{name: "Explosion", position: {x:0, y:1, z:0}}` |
| `unity_create_terrain` | Create terrain | `{name: "Terrain", width: 1000, length: 1000, height: 600}` |
| `unity_add_animator` | Add Animator | `{gameObject: "Character", controller: "Assets/Animators/Main.controller"}` |

## 📂 Asset Operations

| Tool | Description | Example |
|------|-------------|---------|
| `unity_create_folder` | Create folder | `{path: "Assets/MyFolder"}` |
| `unity_list_assets` | List by type | `{type: "Prefab", path: "Assets/Prefabs"}` |

Asset types: `Prefab`, `Material`, `AudioClip`, `Texture`, `Mesh`, `AnimatorController`, etc.

## 🎮 Console & Playmode

| Tool | Description | Example |
|------|-------------|---------|
| `unity_get_console_logs` | Get recent logs | `{count: 20}` |
| `unity_clear_console` | Clear console | `{}` |
| `unity_enter_playmode` | Start playing | `{}` |
| `unity_exit_playmode` | Stop playing | `{}` |
| `unity_is_playing` | Check if playing | `{}` |

## 💡 Pro Tips

### 1. Batch Creation for Efficiency
```json
unity_batch_create_gameobjects({
  objects: [
    {name: "Wall1", position: {x:-5, y:0, z:0}, components: ["BoxCollider"]},
    {name: "Wall2", position: {x:5, y:0, z:0}, components: ["BoxCollider"]},
    {name: "Floor", position: {x:0, y:-1, z:0}, components: ["BoxCollider"]}
  ]
})
```

### 2. Complete GameObject Setup
```json
1. unity_create_primitive({type: "Cube", name: "Player"})
2. unity_set_gameobject_transform({name: "Player", position: {x:0, y:1, z:0}})
3. unity_add_component({gameObject: "Player", component: "Rigidbody"})
4. unity_configure_rigidbody({gameObject: "Player", mass: 1, useGravity: true})
5. unity_set_gameobject_tag({name: "Player", tag: "Player"})
```

### 3. Material Workflow
```json
1. unity_create_material({name: "PlayerMat", path: "Materials/Player.mat"})
2. unity_set_material_property({path: "Assets/Materials/Player.mat", property: "_Color", value: {r:0, g:1, b:0, a:1}})
3. unity_assign_material({gameObject: "Player", materialPath: "Assets/Materials/Player.mat"})
```

### 4. UI Setup
```json
1. unity_create_canvas({name: "MainUI"})
2. unity_create_ui_element({type: "Panel", name: "MenuPanel", parent: "MainUI"})
3. unity_create_ui_element({type: "Button", name: "PlayBtn", parent: "MenuPanel", text: "Play Game"})
4. unity_set_rect_transform({name: "PlayBtn", anchorMin: {x:0.5, y:0.5}, anchorMax: {x:0.5, y:0.5}, sizeDelta: {x:200, y:50}})
```

## 🚀 Example: Complete Game Scene

Ask your AI:
> "Create a simple platformer level with a player (sphere with rigidbody), a ground plane, some obstacles (cubes), a main camera positioned behind the player, and directional lighting"

The AI will orchestrate all these tools to build it automatically!

