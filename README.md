# MCP Unity Server

> AI-powered Unity Editor integration via Model Context Protocol. Generate games, manipulate scenes, and automate Unity workflows directly from Cursor/Claude!

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Unity](https://img.shields.io/badge/Unity-2020.3+-black.svg)](https://unity.com/)
[![Node](https://img.shields.io/badge/Node.js-18+-green.svg)](https://nodejs.org/)

## 🎮 What is This?

An MCP (Model Context Protocol) server that connects AI assistants like Claude/Cursor to Unity Editor, enabling:

- 🤖 **AI-powered game generation** - "Create a platformer level with enemies"
- 🎨 **Scene manipulation** - Create, modify, delete GameObjects via natural language
- ⚙️ **Component configuration** - Setup physics, materials, lighting with AI
- 📦 **Asset management** - Generate prefabs, materials, scripts automatically
- 🖼️ **UI generation** - Create complete UI systems instantly
- 📝 **Script generation** - Auto-generate MonoBehaviour/NetworkBehaviour scripts

## 🚀 Quick Start

### 1. Install MCP Server

**Option A: Use with npx (No installation needed)**
```json
{
  "mcpServers": {
    "unity": {
      "command": "npx",
      "args": ["-y", "mcp-unity-server-yamansivrikaya"]
    }
  }
}
```

**Option B: From GitHub**
```json
{
  "mcpServers": {
    "unity": {
      "command": "npx",
      "args": ["-y", "github:EYamanS/mcp-unity-server"]
    }
  }
}
```

Add the above to your `.cursor/mcp.json` file.

### 2. Install Unity Bridge

Copy the Unity C# bridge to your Unity project:

```bash
# Clone or download this repo
git clone https://github.com/EYamanS/mcp-unity-server.git

# Copy Unity bridge to your project
cp -r mcp-unity-server/UnityMCP/* /path/to/YourUnityProject/Assets/Editor/UnityMCP/
```

### 3. Start Using

1. **Restart Cursor** to load MCP server
2. **Open Unity Editor** with your project
3. **Verify connection**: `Window > Unity MCP > Control Panel` (should show green ●)
4. **Start creating!** Ask AI to generate game objects

## ✨ Example Usage

Ask your AI assistant:

> *"Create a simple platformer level in Unity with a player sphere that has physics, a ground plane, some cube obstacles positioned randomly, a camera at (0, 5, -10) looking at the player, and directional lighting"*

The AI will automatically:
1. Create primitives for ground and obstacles
2. Setup player with Rigidbody and collider
3. Add and position camera
4. Create directional light
5. Position everything correctly
6. Save the scene

## 🎯 Features

### 30+ Game Generation Tools

<table>
<tr>
<td width="50%">

**GameObjects**
- Create, delete, clone
- Transform (position, rotation, scale)
- Parenting & hierarchy
- Batch creation
- Primitives (Cube, Sphere, etc.)

**Components**
- Add/remove components
- Get/set any property
- Component configuration

**Physics**
- Rigidbody setup
- Colliders (Box, Sphere, Capsule, Mesh)
- Physics materials

</td>
<td width="50%">

**Assets**
- Prefab workflow
- Material creation & assignment
- Script generation (3 templates)
- Asset listing & search
- Folder management

**Rendering**
- Lighting (Directional, Point, Spot)
- Camera setup & config
- Material properties
- Ambient light

**UI**
- Canvas creation
- UI elements (Button, Text, Image, Panel, InputField, Slider)
- RectTransform configuration

**Scene**
- Create, save, load scenes
- Hierarchy traversal

</td>
</tr>
</table>

## 📚 Available Tools

<details>
<summary><b>GameObject Operations (11 tools)</b></summary>

- `unity_create_gameobject` - Create empty GameObject
- `unity_delete_gameobject` - Delete GameObject
- `unity_clone_gameobject` - Duplicate with all components
- `unity_set_gameobject_transform` - Set position/rotation/scale
- `unity_set_gameobject_active` - Enable/disable
- `unity_set_gameobject_parent` - Change parent
- `unity_set_gameobject_tag` - Set tag
- `unity_set_gameobject_layer` - Set layer
- `unity_create_primitive` - Create Cube, Sphere, etc.
- `unity_batch_create_gameobjects` - Create multiple at once
- `unity_get_gameobject_info` - Get full details

</details>

<details>
<summary><b>Component Operations (4 tools)</b></summary>

- `unity_add_component` - Add component to GameObject
- `unity_remove_component` - Remove component
- `unity_get_component_properties` - Get all properties
- `unity_set_component_property` - Set any property value

</details>

<details>
<summary><b>Physics Tools (2 tools)</b></summary>

- `unity_configure_rigidbody` - Setup mass, drag, gravity
- `unity_configure_collider` - Setup Box/Sphere/Capsule/Mesh colliders

</details>

<details>
<summary><b>Material System (4 tools)</b></summary>

- `unity_create_material` - Create new material
- `unity_list_materials` - List all materials
- `unity_set_material_property` - Set color, texture, etc.
- `unity_assign_material` - Assign to renderer

</details>

<details>
<summary><b>Prefab Workflow (3 tools)</b></summary>

- `unity_list_prefabs` - List all prefabs
- `unity_instantiate_prefab` - Spawn in scene
- `unity_create_prefab` - Save GameObject as prefab

</details>

<details>
<summary><b>UI Generation (3 tools)</b></summary>

- `unity_create_canvas` - Create UI Canvas
- `unity_create_ui_element` - Create Button/Text/Image/Panel/InputField/Slider
- `unity_set_rect_transform` - Configure anchors, size, position

</details>

<details>
<summary><b>See UnityMCP/TOOLS_REFERENCE.md for complete API reference</b></summary>

Plus tools for: Lighting, Camera, Scene Management, Script Generation, Audio, Particles, Terrain, Animation, Console, and more!

</details>

## 🏗️ Architecture

**Clean, modular design following SOLID principles:**

```
┌─────────────┐
│   Cursor    │  (AI Assistant)
└──────┬──────┘
       │ stdio (MCP Protocol)
       ▼
┌─────────────────┐
│  Node.js MCP    │  (This Server)
│     Server      │
└──────┬──────────┘
       │ WebSocket (port 8765)
       ▼
┌─────────────────┐
│  Unity C# Bridge│  (18 Tool Classes)
└──────┬──────────┘
       │ Unity Editor API
       ▼
┌─────────────────┐
│  Unity Editor   │  (Your Game)
└─────────────────┘
```

**Unity Bridge Structure:**
- **Partial Classes**: Organized by responsibility
- **18 Specialized Tools**: Each category in its own file
- **Router Pattern**: Central dispatch to appropriate handler
- **Shared Utilities**: Common operations abstracted

## 📦 Installation

### Prerequisites

- **Node.js** 18+ ([Download](https://nodejs.org/))
- **Unity** 2020.3+ (tested on Unity 6)
- **Cursor** with MCP support
- **Newtonsoft.Json** in Unity (usually pre-installed)

### Setup

1. **Configure Cursor** - Add to `.cursor/mcp.json`:
```json
{
  "mcpServers": {
    "unity": {
      "command": "npx",
      "args": ["-y", "github:EYamanS/mcp-unity-server"]
    }
  }
}
```

2. **Install Unity Bridge**:
```bash
git clone https://github.com/EYamanS/mcp-unity-server.git
cp -r mcp-unity-server/UnityMCP/* /path/to/YourUnityProject/Assets/Editor/UnityMCP/
```

3. **Restart Cursor**

4. **Verify**: In Unity, check `Window > Unity MCP > Control Panel` shows **● Connected**

## 🎨 Example Projects

<details>
<summary><b>Generate a Simple Platformer</b></summary>

```
Create a platformer level with:
- Player sphere at (0, 2, 0) with rigidbody and sphere collider
- Ground plane at origin scaled to 20x1x20
- 5 cube obstacles positioned randomly
- Camera at (0, 5, -10) looking at origin
- Directional light from above
- Red material on player
- Blue material on ground
```

</details>

<details>
<summary><b>Generate UI System</b></summary>

```
Create a main menu UI with:
- Canvas in screen space overlay mode
- Background panel covering full screen
- Title text at top saying "My Game"
- Three buttons stacked vertically: Play, Settings, Quit
- Each button 200x50 pixels
```

</details>

<details>
<summary><b>Generate Physics Playground</b></summary>

```
Create a physics sandbox:
- Large cube as ground with box collider
- 10 spheres dropped from random heights with rigidbodies
- Each sphere with random colored material
- Camera positioned to see the action
```

</details>

## 🛠️ Development

```bash
# Clone repository
git clone https://github.com/EYamanS/mcp-unity-server.git
cd mcp-unity-server

# Install dependencies
npm install

# Build
npm run build

# Test locally
npm run dev
```

## 📖 Documentation

- [Tools Reference](./UnityMCP/TOOLS_REFERENCE.md) - Complete API documentation
- [Unity Bridge Architecture](./UnityMCP/README.md) - Design & patterns
- [Unity Installation Guide](./UnityMCP/INSTALLATION.md) - Detailed Unity setup

## 🤝 Contributing

Contributions welcome! To add new tools:

1. **Node.js side**: Add tool definition in `src/index.ts`
2. **Unity side**: Create tool class in `UnityMCP/Tools/YourTool.cs`
3. **Router**: Add routing in `UnityMCP/UnityMCPBridge.Router.cs`
4. **Document**: Update `UnityMCP/TOOLS_REFERENCE.md`

## 🐛 Troubleshooting

**MCP server not connecting:**
- Check Cursor MCP logs in output panel
- Verify Node.js 18+ is installed
- Restart Cursor completely

**Unity not connecting:**
- Check Unity Console for WebSocket errors
- Verify bridge files are in `Assets/Editor/UnityMCP/`
- Port 8765 must be free
- Check `Window > Unity MCP > Control Panel`

**Tools not working:**
- Ensure both Node and Unity bridges are up to date
- Rebuild Unity bridge if you made changes
- Check Unity Console for detailed error messages

## 📋 Requirements

- **Node.js**: 18.0.0 or higher
- **Unity**: 2020.3+ (tested on Unity 6)
- **Cursor**: Latest version with MCP support
- **Newtonsoft.Json**: For Unity (usually included)

## 📝 License

MIT License - see [LICENSE](./LICENSE) file

## 🙏 Credits

Built with:
- [Model Context Protocol](https://modelcontextprotocol.io/) by Anthropic
- [Unity Editor](https://unity.com/)
- [Cursor](https://cursor.sh/)

## 🌟 Star History

If this helps you build games faster, give it a star! ⭐

---

**Made with ❤️ for Unity developers who want AI superpowers**

---

## 🔗 Links

- [GitHub Repository](https://github.com/EYamanS/mcp-unity-server)
- [Report Issues](https://github.com/EYamanS/mcp-unity-server/issues)
- [MCP Documentation](https://modelcontextprotocol.io/)
