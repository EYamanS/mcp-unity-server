# Unity MCP Bridge - Modular Architecture

A clean, SOLID-principles-based Unity MCP bridge with game generation capabilities.

## 📁 Architecture

```
unity-bridge/
├── UnityMCPBridge.Core.cs        # WebSocket connection & main loop
├── UnityMCPBridge.Router.cs      # Central tool routing
├── UnityMCPBridge.Helpers.cs     # Shared utilities
└── Tools/
    ├── ProjectTools.cs           # Project information
    ├── GameObjectTools.cs        # GameObject CRUD operations
    ├── ComponentTools.cs         # Component management
    ├── SceneTools.cs             # Scene operations
    ├── PrefabTools.cs            # Prefab operations
    ├── MaterialTools.cs          # Material creation & config
    ├── PhysicsTools.cs           # Rigidbody & Colliders
    ├── UITools.cs                # Canvas & UI elements
    ├── LightingTools.cs          # Lights & ambient
    ├── CameraTools.cs            # Camera operations
    ├── ScriptTools.cs            # Script generation
    ├── AssetTools.cs             # Asset database ops
    ├── AudioTools.cs             # Audio source config
    ├── ParticleTools.cs          # Particle systems
    ├── TerrainTools.cs           # Terrain creation
    ├── AnimationTools.cs         # Animator setup
    ├── ConsoleTools.cs           # Console operations
    └── PlayModeTools.cs          # Play mode control
```

## 🚀 Installation

### Step 1: Copy files to Unity project

Copy all files from this directory to your Unity project:

```
unity-bridge/ → YourUnityProject/Assets/Editor/UnityMCP/
```

The structure should look like:
```
YourUnityProject/
└── Assets/
    └── Editor/
        └── UnityMCP/
            ├── UnityMCPBridge.Core.cs
            ├── UnityMCPBridge.Router.cs
            ├── UnityMCPBridge.Helpers.cs
            ├── UnityMCPWindow.cs (keep your existing one)
            └── Tools/
                ├── ProjectTools.cs
                ├── GameObjectTools.cs
                ├── ... (all other tool files)
```

### Step 2: Remove old UnityMCPBridge.cs

Delete the old monolithic `UnityMCPBridge.cs` file if it exists.

### Step 3: Let Unity recompile

Unity will automatically detect and compile the new partial classes.

## ✨ Features

### Core Operations
- ✅ GameObject create/delete/clone/modify
- ✅ Component add/remove/configure
- ✅ Transform manipulation (position, rotation, scale)
- ✅ Hierarchy management (parenting)

### Asset Management
- ✅ Prefab instantiation & creation
- ✅ Material creation & property setting
- ✅ Asset listing by type
- ✅ Folder creation

### Scene Building
- ✅ Primitive creation (Cube, Sphere, etc.)
- ✅ Light creation (Directional, Point, Spot)
- ✅ Camera setup & configuration
- ✅ Physics configuration (Rigidbody, Colliders)

### UI Generation
- ✅ Canvas creation
- ✅ UI elements (Button, Text, Image, Panel, InputField, Slider)
- ✅ RectTransform configuration

### Advanced
- ✅ Script generation (basic, MonoBehaviour, NetworkBehaviour)
- ✅ Batch GameObject creation
- ✅ Terrain creation
- ✅ Particle system setup
- ✅ Audio source configuration
- ✅ Animator setup

## 🎯 Design Principles

This architecture follows SOLID principles:

- **Single Responsibility**: Each tool class handles one category
- **Open/Closed**: Easy to extend with new tool categories
- **Liskov Substitution**: Consistent interfaces across tools
- **Interface Segregation**: Tools are independent
- **Dependency Inversion**: Tools depend on abstractions (helpers)

## 📝 Example Usage

Once installed and connected, you can use these tools from Cursor:

```typescript
// Create a complete game scene
1. unity_create_primitive({type: "Cube", name: "Ground", scale: {x:10, y:0.1, z:10}})
2. unity_create_primitive({type: "Sphere", name: "Player", position: {x:0, y:1, z:0}})
3. unity_configure_rigidbody({gameObject: "Player", mass: 1, useGravity: true})
4. unity_configure_collider({gameObject: "Player", colliderType: "SphereCollider"})
5. unity_create_camera({name: "Main Camera", position: {x:0, y:3, z:-5}})
6. unity_create_light({name: "Sun", lightType: "Directional", intensity: 1})
7. unity_save_scene()
```

## 🔧 Extending

To add new tools:

1. Create a new file in `Tools/` (e.g., `CustomTools.cs`)
2. Add your tool implementation as a static class
3. Add routing in `UnityMCPBridge.Router.cs`
4. Add tool definition in Node.js `src/index.ts`

## 📚 Dependencies

- Newtonsoft.Json (Json.NET for Unity)
- Unity 2020.3+ (tested on Unity 6)
- .NET 4.x or .NET Standard 2.1

## 🐛 Troubleshooting

- **Tools not appearing**: Restart Cursor to reload MCP configuration
- **Connection failed**: Check Unity console for WebSocket errors
- **Timeout errors**: Large operations may need timeout adjustment in Core.cs
- **Type not found**: Use fully qualified names (e.g., "UnityEngine.Rigidbody")

## 📖 License

Same as your project license.

