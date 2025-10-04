# Installation Guide

## Quick Install

### 1. Copy Files to Unity

Copy the entire `unity-bridge` directory to your Unity project:

**From:** `/Users/yamansivrikaya/wkspaces/mcp-unity-test/unity-bridge/`

**To:** `YourUnityProject/Assets/Editor/UnityMCP/`

### 2. Verify Structure

Your Unity project should now have:

```
Assets/
└── Editor/
    └── UnityMCP/
        ├── UnityMCPBridge.Core.cs
        ├── UnityMCPBridge.Router.cs
        ├── UnityMCPBridge.Helpers.cs
        ├── UnityMCPWindow.cs
        └── Tools/
            ├── AnimationTools.cs
            ├── AssetTools.cs
            ├── AudioTools.cs
            ├── CameraTools.cs
            ├── ComponentTools.cs
            ├── ConsoleTools.cs
            ├── GameObjectTools.cs
            ├── LightingTools.cs
            ├── MaterialTools.cs
            ├── ParticleTools.cs
            ├── PhysicsTools.cs
            ├── PlayModeTools.cs
            ├── PrefabTools.cs
            ├── ProjectTools.cs
            ├── SceneTools.cs
            ├── ScriptTools.cs
            ├── TerrainTools.cs
            └── UITools.cs
```

### 3. Install Required Dependencies

**Install Newtonsoft.Json (Required):**

**Option A: Package Manager (Recommended)**
1. Open Unity Package Manager: `Window > Package Manager`
2. Click the `+` button → `Add package by name`
3. Enter: `com.unity.nuget.newtonsoft-json`
4. Click `Add`

**Option B: Manual Installation**
1. Download from: https://github.com/JamesNK/Newtonsoft.Json/releases
2. Import the `.unitypackage` file into your project

### 4. Remove Old Bridge (if exists)

If you have an old `UnityMCPBridge.cs` (single file), delete it:
- Delete: `Assets/Editor/UnityMCP/UnityMCPBridge.cs`

### 5. Wait for Compilation

Unity will automatically detect and compile the new files. Check the Console for any errors.

### 6. Verify Connection

1. In Unity, go to: `Window > Unity MCP > Control Panel`
2. You should see:
   - Status: ● Connected (green)
   - Client Count: 1

### 7. Test from Cursor

In Cursor, try a simple command:
```
"Create a cube called TestCube in my Unity scene"
```

The AI should use the new tools to create it!

## Troubleshooting

### "Type 'ToolRouter' not found"
- Make sure ALL files were copied
- Check that `UnityMCPBridge.Router.cs` is in the correct location
- Try reimporting: Right-click folder → Reimport

### "WebSocket connection failed"
- Ensure Cursor is running and MCP server is active
- Check `.cursor/mcp.json` configuration
- Restart Cursor

### Compilation Errors

**"The type or namespace name 'Newtonsoft' could not be found"**
- Install Newtonsoft.Json package (see step 3 above)
- Use Package Manager: `com.unity.nuget.newtonsoft-json`

**Other compilation errors:**
- Check Unity version (needs 2020.3+)
- Verify all using statements are present
- Try reimporting: Right-click folder → Reimport

## What's New

### 🎮 Game Generation Tools

Now you can auto-generate games with:

- **30+ new tools** for complete game creation
- **Transform manipulation** (position, rotation, scale)  
- **Physics setup** (Rigidbody, Colliders)
- **Material system** (create, configure, assign)
- **Prefab workflow** (list, instantiate, create)
- **UI generation** (Canvas, Buttons, Text, etc.)
- **Lighting setup** (lights, ambient)
- **Script templates** (MonoBehaviour, NetworkBehaviour)
- **Batch operations** (create multiple GameObjects at once)

### 🏗️ Architecture Benefits

- **Modular**: Easy to find and modify specific functionality
- **Maintainable**: Each tool category in its own file
- **Extensible**: Add new tools without touching existing code
- **SOLID**: Follows Single Responsibility Principle
- **Clean**: No 1000+ line mega-files

## Next Steps

1. **Test the tools** - Try creating some game objects
2. **Explore capabilities** - Ask AI to "generate a simple platformer level"
3. **Customize** - Add your own tools in new files
4. **Share feedback** - Let us know how it works!

Enjoy building games with AI! 🚀

