#!/usr/bin/env node
// mcp-server/src/index.ts
import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
} from "@modelcontextprotocol/sdk/types.js";
import WebSocket, { WebSocketServer } from "ws";

// --- WebSocket server (Node) <-> Unity (client) bridge ---

const WS_PORT = 8765;
let unitySocket: WebSocket | null = null;
let messageId = 0;
const pending = new Map<
  string,
  { resolve: (v: any) => void; reject: (e: any) => void }
>();

// Start WS server to accept Unity connection
const wss = new WebSocketServer({ host: "127.0.0.1", port: WS_PORT });
wss.on("listening", () => {
  console.error(`WS listening on ws://127.0.0.1:${WS_PORT}`);
});
wss.on("connection", (ws) => {
  console.error("Unity connected");
  unitySocket = ws;

  ws.on("message", (data: Buffer) => {
    try {
      const msg = JSON.parse(data.toString());
      const id = msg?.id != null ? String(msg.id) : null;
      if (!id) return;
      const waiter = pending.get(id);
      if (!waiter) return;

      pending.delete(id);
      if (msg.error) waiter.reject(new Error(msg.error.message ?? "Unknown error"));
      else waiter.resolve(msg.result);
    } catch (err) {
      console.error("Bad message from Unity:", err);
    }
  });

  ws.on("close", () => {
    console.error("Unity disconnected");
    if (unitySocket === ws) unitySocket = null;

    // Fail any in-flight requests
    for (const [, waiter] of pending) waiter.reject(new Error("Unity disconnected"));
    pending.clear();
  });

  ws.on("error", (e) => {
    console.error("WS error:", e);
  });
});

function callUnityTool(name: string, args: any): Promise<any> {
  if (!unitySocket || unitySocket.readyState !== WebSocket.OPEN) {
    throw new Error("Unity not connected");
  }
  const id = String(++messageId);
  const payload = {
    jsonrpc: "2.0",
    id,
    method: "tools/call",
    params: { name, arguments: args || {} },
  };

  return new Promise((resolve, reject) => {
    pending.set(id, { resolve, reject });
    try {
      unitySocket!.send(JSON.stringify(payload));
    } catch (err) {
      pending.delete(id);
      return reject(err instanceof Error ? err : new Error(String(err)));
    }

    const to = setTimeout(() => {
      if (pending.has(id)) {
        pending.delete(id);
        reject(new Error("Request timeout"));
      }
    }, 30_000);

    // Wrap resolve/reject to clear timeout
    const entry = pending.get(id);
    if (entry) {
      pending.set(id, {
        resolve: (v) => {
          clearTimeout(to);
          resolve(v);
        },
        reject: (e) => {
          clearTimeout(to);
          reject(e);
        },
      });
    }
  });
}

// --- MCP server (stdio) for Cursor ---

const server = new Server(
  { name: "unity-mcp-server", version: "0.1.0" },
  { capabilities: { tools: {} } }
);

// Tool definitions
const tools = [
  // Project Tools
  {
    name: "unity_get_project_info",
    description: "Get Unity project information including name, version, platform",
    inputSchema: { type: "object", properties: {}, required: [] },
  },
  {
    name: "unity_list_scenes",
    description: "List all scenes in the Unity project",
    inputSchema: { type: "object", properties: {}, required: [] },
  },
  {
    name: "unity_list_scripts",
    description: "List all C# scripts in the project",
    inputSchema: {
      type: "object",
      properties: {
        path: {
          type: "string",
          description: "Optional folder path to search within (default: Assets)",
        },
      },
    },
  },

  // Scene Tools
  {
    name: "unity_get_active_scene",
    description: "Get information about the currently active scene",
    inputSchema: { type: "object", properties: {}, required: [] },
  },
  {
    name: "unity_get_scene_hierarchy",
    description: "Get the complete GameObject hierarchy of the active scene",
    inputSchema: { type: "object", properties: {}, required: [] },
  },
  {
    name: "unity_find_gameobject",
    description: "Find a GameObject in the scene by name",
    inputSchema: {
      type: "object",
      properties: { name: { type: "string", description: "GameObject name" } },
      required: ["name"],
    },
  },

  // GameObject Tools
  {
    name: "unity_get_gameobject_info",
    description: "Get detailed information about a GameObject including transform and components",
    inputSchema: {
      type: "object",
      properties: { name: { type: "string", description: "GameObject name" } },
      required: ["name"],
    },
  },
  {
    name: "unity_create_gameobject",
    description: "Create a new GameObject in the scene",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "New GameObject name" },
        parent: { type: "string", description: "Optional parent GameObject name" },
      },
      required: ["name"],
    },
  },

  // Script Tools
  {
    name: "unity_read_script",
    description: "Read the contents of a C# script file",
    inputSchema: {
      type: "object",
      properties: {
        path: {
          type: "string",
          description: "Path to the script file (e.g., 'Scripts/PlayerController.cs')",
        },
      },
      required: ["path"],
    },
  },
  {
    name: "unity_write_script",
    description: "Write or modify a C# script file",
    inputSchema: {
      type: "object",
      properties: {
        path: { type: "string", description: "Path to the script file" },
        content: { type: "string", description: "Complete script content" },
      },
      required: ["path", "content"],
    },
  },

  // Component Tools
  {
    name: "unity_add_component",
    description: "Add a component to a GameObject",
    inputSchema: {
      type: "object",
      properties: {
        gameObject: { type: "string", description: "GameObject name" },
        component: {
          type: "string",
          description: "Component type (e.g., 'Rigidbody', 'BoxCollider', 'MyNamespace.MyComp')",
        },
      },
      required: ["gameObject", "component"],
    },
  },
  {
    name: "unity_get_component_properties",
    description: "Get all properties of a component on a GameObject",
    inputSchema: {
      type: "object",
      properties: {
        gameObject: { type: "string", description: "GameObject name" },
        component: { type: "string", description: "Component type name" },
      },
      required: ["gameObject", "component"],
    },
  },

  // Console Tools
  {
    name: "unity_get_console_logs",
    description: "Get recent console log messages",
    inputSchema: {
      type: "object",
      properties: { count: { type: "number", description: "How many (default 50)" } },
    },
  },
  {
    name: "unity_clear_console",
    description: "Clear the Unity console",
    inputSchema: { type: "object", properties: {}, required: [] },
  },

  // Play Mode Tools
  {
    name: "unity_enter_playmode",
    description: "Enter Unity play mode",
    inputSchema: { type: "object", properties: {}, required: [] },
  },
  {
    name: "unity_exit_playmode",
    description: "Exit Unity play mode",
    inputSchema: { type: "object", properties: {}, required: [] },
  },
  {
    name: "unity_is_playing",
    description: "Check if Unity is currently in play mode",
    inputSchema: { type: "object", properties: {}, required: [] },
  },

  // === PRIORITY 1: Core Game Generation Tools ===
  
  // Advanced GameObject Operations
  {
    name: "unity_delete_gameobject",
    description: "Delete a GameObject from the scene",
    inputSchema: {
      type: "object",
      properties: { name: { type: "string", description: "GameObject name" } },
      required: ["name"],
    },
  },
  {
    name: "unity_clone_gameobject",
    description: "Clone/duplicate a GameObject with all its components",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "GameObject to clone" },
        newName: { type: "string", description: "Name for the cloned object" },
      },
      required: ["name", "newName"],
    },
  },
  {
    name: "unity_set_gameobject_transform",
    description: "Set GameObject transform (position, rotation, scale)",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "GameObject name" },
        position: { type: "object", description: "Position {x, y, z}" },
        rotation: { type: "object", description: "Rotation in euler angles {x, y, z}" },
        scale: { type: "object", description: "Scale {x, y, z}" },
      },
      required: ["name"],
    },
  },
  {
    name: "unity_set_gameobject_active",
    description: "Enable or disable a GameObject",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "GameObject name" },
        active: { type: "boolean", description: "Active state" },
      },
      required: ["name", "active"],
    },
  },
  {
    name: "unity_set_gameobject_parent",
    description: "Set the parent of a GameObject",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "GameObject name" },
        parentName: { type: "string", description: "Parent GameObject name (null for root)" },
      },
      required: ["name"],
    },
  },

  // Component Modification
  {
    name: "unity_set_component_property",
    description: "Set a property value on a component",
    inputSchema: {
      type: "object",
      properties: {
        gameObject: { type: "string", description: "GameObject name" },
        component: { type: "string", description: "Component type" },
        property: { type: "string", description: "Property name" },
        value: { description: "Property value (any type)" },
      },
      required: ["gameObject", "component", "property", "value"],
    },
  },
  {
    name: "unity_remove_component",
    description: "Remove a component from a GameObject",
    inputSchema: {
      type: "object",
      properties: {
        gameObject: { type: "string", description: "GameObject name" },
        component: { type: "string", description: "Component type to remove" },
      },
      required: ["gameObject", "component"],
    },
  },

  // Prefab System
  {
    name: "unity_list_prefabs",
    description: "List all prefabs in the project",
    inputSchema: {
      type: "object",
      properties: {
        path: { type: "string", description: "Optional folder path to search" },
      },
    },
  },
  {
    name: "unity_instantiate_prefab",
    description: "Instantiate a prefab in the scene",
    inputSchema: {
      type: "object",
      properties: {
        path: { type: "string", description: "Prefab asset path" },
        name: { type: "string", description: "Name for the instance" },
        position: { type: "object", description: "Position {x, y, z}" },
        rotation: { type: "object", description: "Rotation {x, y, z}" },
      },
      required: ["path"],
    },
  },
  {
    name: "unity_create_prefab",
    description: "Create a prefab from a GameObject",
    inputSchema: {
      type: "object",
      properties: {
        gameObject: { type: "string", description: "GameObject name" },
        path: { type: "string", description: "Prefab save path (e.g., 'Assets/Prefabs/MyPrefab.prefab')" },
      },
      required: ["gameObject", "path"],
    },
  },

  // Material System
  {
    name: "unity_create_material",
    description: "Create a new material",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "Material name" },
        path: { type: "string", description: "Save path (e.g., 'Assets/Materials/MyMat.mat')" },
        shader: { type: "string", description: "Shader name (default: Standard)" },
      },
      required: ["name", "path"],
    },
  },
  {
    name: "unity_list_materials",
    description: "List all materials in the project",
    inputSchema: {
      type: "object",
      properties: {
        path: { type: "string", description: "Optional folder path" },
      },
    },
  },
  {
    name: "unity_set_material_property",
    description: "Set a material property (color, float, texture, etc.)",
    inputSchema: {
      type: "object",
      properties: {
        path: { type: "string", description: "Material asset path" },
        property: { type: "string", description: "Property name (e.g., '_Color', '_MainTex')" },
        value: { description: "Property value" },
      },
      required: ["path", "property", "value"],
    },
  },
  {
    name: "unity_assign_material",
    description: "Assign a material to a renderer component",
    inputSchema: {
      type: "object",
      properties: {
        gameObject: { type: "string", description: "GameObject name" },
        materialPath: { type: "string", description: "Material asset path" },
        index: { type: "number", description: "Material slot index (default: 0)" },
      },
      required: ["gameObject", "materialPath"],
    },
  },

  // Scene Management
  {
    name: "unity_create_scene",
    description: "Create a new scene",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "Scene name" },
        path: { type: "string", description: "Save path (e.g., 'Assets/Scenes/NewScene.unity')" },
      },
      required: ["name", "path"],
    },
  },
  {
    name: "unity_save_scene",
    description: "Save the active scene",
    inputSchema: { type: "object", properties: {}, required: [] },
  },
  {
    name: "unity_load_scene",
    description: "Load a scene by path",
    inputSchema: {
      type: "object",
      properties: {
        path: { type: "string", description: "Scene path" },
      },
      required: ["path"],
    },
  },

  // === PRIORITY 2: Physics & Rendering ===
  
  // Physics Configuration
  {
    name: "unity_configure_rigidbody",
    description: "Configure Rigidbody component properties",
    inputSchema: {
      type: "object",
      properties: {
        gameObject: { type: "string", description: "GameObject name" },
        mass: { type: "number", description: "Mass" },
        drag: { type: "number", description: "Drag" },
        angularDrag: { type: "number", description: "Angular drag" },
        useGravity: { type: "boolean", description: "Use gravity" },
        isKinematic: { type: "boolean", description: "Is kinematic" },
      },
      required: ["gameObject"],
    },
  },
  {
    name: "unity_configure_collider",
    description: "Configure a collider component",
    inputSchema: {
      type: "object",
      properties: {
        gameObject: { type: "string", description: "GameObject name" },
        colliderType: { type: "string", description: "BoxCollider, SphereCollider, CapsuleCollider, or MeshCollider" },
        isTrigger: { type: "boolean", description: "Is trigger" },
        center: { type: "object", description: "Center offset {x, y, z}" },
        size: { type: "object", description: "Size (for box) or radius (for sphere)" },
      },
      required: ["gameObject", "colliderType"],
    },
  },

  // Lighting
  {
    name: "unity_create_light",
    description: "Create a light GameObject",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "Light name" },
        lightType: { type: "string", description: "Directional, Point, Spot, or Area" },
        color: { type: "object", description: "Light color {r, g, b}" },
        intensity: { type: "number", description: "Light intensity" },
        range: { type: "number", description: "Light range (for Point/Spot)" },
      },
      required: ["name", "lightType"],
    },
  },
  {
    name: "unity_set_ambient_light",
    description: "Set scene ambient lighting",
    inputSchema: {
      type: "object",
      properties: {
        color: { type: "object", description: "Ambient color {r, g, b}" },
        intensity: { type: "number", description: "Ambient intensity" },
      },
      required: ["color"],
    },
  },

  // Camera
  {
    name: "unity_create_camera",
    description: "Create a camera GameObject",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "Camera name (default: Main Camera)" },
        position: { type: "object", description: "Position {x, y, z}" },
        fieldOfView: { type: "number", description: "Field of view" },
        clearFlags: { type: "string", description: "Skybox, SolidColor, Depth, or Nothing" },
      },
    },
  },
  {
    name: "unity_configure_camera",
    description: "Configure camera properties",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "Camera GameObject name" },
        fieldOfView: { type: "number", description: "Field of view" },
        nearClip: { type: "number", description: "Near clip plane" },
        farClip: { type: "number", description: "Far clip plane" },
        isOrthographic: { type: "boolean", description: "Orthographic mode" },
        orthographicSize: { type: "number", description: "Orthographic size" },
      },
      required: ["name"],
    },
  },

  // UI Generation
  {
    name: "unity_create_canvas",
    description: "Create a UI Canvas",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "Canvas name (default: Canvas)" },
        renderMode: { type: "string", description: "ScreenSpaceOverlay, ScreenSpaceCamera, or WorldSpace" },
      },
    },
  },
  {
    name: "unity_create_ui_element",
    description: "Create a UI element (Button, Text, Image, Panel)",
    inputSchema: {
      type: "object",
      properties: {
        type: { type: "string", description: "Button, Text, Image, Panel, InputField, Slider" },
        name: { type: "string", description: "Element name" },
        parent: { type: "string", description: "Parent GameObject (usually Canvas)" },
        text: { type: "string", description: "Text content (for Text/Button)" },
      },
      required: ["type", "name"],
    },
  },
  {
    name: "unity_set_rect_transform",
    description: "Configure RectTransform for UI elements",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "GameObject name" },
        anchorMin: { type: "object", description: "Anchor min {x, y}" },
        anchorMax: { type: "object", description: "Anchor max {x, y}" },
        pivot: { type: "object", description: "Pivot {x, y}" },
        sizeDelta: { type: "object", description: "Size delta {x, y}" },
        anchoredPosition: { type: "object", description: "Anchored position {x, y}" },
      },
      required: ["name"],
    },
  },

  // Asset Operations
  {
    name: "unity_create_folder",
    description: "Create a folder in the Assets directory",
    inputSchema: {
      type: "object",
      properties: {
        path: { type: "string", description: "Folder path (e.g., 'Assets/MyFolder')" },
      },
      required: ["path"],
    },
  },
  {
    name: "unity_list_assets",
    description: "List assets of a specific type",
    inputSchema: {
      type: "object",
      properties: {
        type: { type: "string", description: "Asset type (e.g., 'Prefab', 'Material', 'AudioClip')" },
        path: { type: "string", description: "Optional folder path" },
      },
      required: ["type"],
    },
  },
  {
    name: "unity_get_asset_path",
    description: "Get the asset path of a GameObject or component",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "GameObject name" },
        assetType: { 
          type: "string", 
          description: "Type of asset to find (e.g., 'Material', 'Prefab', 'Script', 'Texture')",
          enum: ["Material", "Prefab", "Script", "Texture", "AudioClip", "Animation", "AnimatorController", "Mesh", "Shader"]
        },
      },
      required: ["name", "assetType"],
    },
  },

  // === PRIORITY 2: Advanced Generation ===

  // Primitive Creation
  {
    name: "unity_create_primitive",
    description: "Create a primitive GameObject (Cube, Sphere, Capsule, Cylinder, Plane, Quad)",
    inputSchema: {
      type: "object",
      properties: {
        type: { type: "string", description: "Cube, Sphere, Capsule, Cylinder, Plane, or Quad" },
        name: { type: "string", description: "GameObject name" },
        position: { type: "object", description: "Position {x, y, z}" },
        scale: { type: "object", description: "Scale {x, y, z}" },
      },
      required: ["type", "name"],
    },
  },

  // Audio
  {
    name: "unity_add_audio_source",
    description: "Add and configure an AudioSource component",
    inputSchema: {
      type: "object",
      properties: {
        gameObject: { type: "string", description: "GameObject name" },
        clip: { type: "string", description: "AudioClip asset path" },
        playOnAwake: { type: "boolean", description: "Play on awake" },
        loop: { type: "boolean", description: "Loop audio" },
        volume: { type: "number", description: "Volume (0-1)" },
      },
      required: ["gameObject"],
    },
  },

  // Particle System
  {
    name: "unity_create_particle_system",
    description: "Create a particle system GameObject",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "Particle system name" },
        position: { type: "object", description: "Position {x, y, z}" },
      },
      required: ["name"],
    },
  },

  // Terrain
  {
    name: "unity_create_terrain",
    description: "Create a terrain GameObject",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "Terrain name" },
        width: { type: "number", description: "Terrain width" },
        length: { type: "number", description: "Terrain length" },
        height: { type: "number", description: "Terrain height" },
      },
      required: ["name"],
    },
  },

  // Tag & Layer
  {
    name: "unity_set_gameobject_tag",
    description: "Set GameObject tag",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "GameObject name" },
        tag: { type: "string", description: "Tag name" },
      },
      required: ["name", "tag"],
    },
  },
  {
    name: "unity_set_gameobject_layer",
    description: "Set GameObject layer",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "GameObject name" },
        layer: { type: "string", description: "Layer name" },
      },
      required: ["name", "layer"],
    },
  },

  // Animation
  {
    name: "unity_add_animator",
    description: "Add Animator component with controller",
    inputSchema: {
      type: "object",
      properties: {
        gameObject: { type: "string", description: "GameObject name" },
        controller: { type: "string", description: "Animator controller asset path" },
      },
      required: ["gameObject"],
    },
  },

  // Batch Operations
  {
    name: "unity_batch_create_gameobjects",
    description: "Create multiple GameObjects at once",
    inputSchema: {
      type: "object",
      properties: {
        objects: {
          type: "array",
          description: "Array of objects with {name, parent?, position?, components?}",
        },
      },
      required: ["objects"],
    },
  },

  // Script Generation
  {
    name: "unity_generate_component_script",
    description: "Generate a component script from a template",
    inputSchema: {
      type: "object",
      properties: {
        name: { type: "string", description: "Script name (e.g., 'PlayerController')" },
        path: { type: "string", description: "Save path" },
        template: { type: "string", description: "Template type (basic, monobehaviour, networkbehaviour)" },
        namespace: { type: "string", description: "Optional namespace" },
      },
      required: ["name", "path"],
    },
  },
];

server.setRequestHandler(ListToolsRequestSchema, async () => ({ tools }));

server.setRequestHandler(CallToolRequestSchema, async (request) => {
  const { name, arguments: args } = request.params;
  try {
    const result = await callUnityTool(name, args || {});
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  } catch (error) {
    return {
      content: [
        {
          type: "text",
          text: `Error: ${error instanceof Error ? error.message : String(error)}`,
        },
      ],
      isError: true,
    };
  }
});

// Start MCP (stdio)
async function main() {
  try {
    const transport = new StdioServerTransport();
    await server.connect(transport);
    console.error("MCP (stdio) up. Waiting for Unity on ws://127.0.0.1:8765 …");
  } catch (error) {
    console.error("Failed to start MCP server:", error);
    process.exit(1);
  }
}
main();

// Graceful shutdown
process.on("SIGINT", () => {
  console.error("Shutting down…");
  try { wss.close(); } catch {}
  process.exit(0);
});
