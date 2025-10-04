# Contributing to MCP Unity Server

Thank you for your interest in contributing! 🎮

## 🚀 Getting Started

### 1. Fork & Clone

```bash
git clone https://github.com/YOUR_USERNAME/mcp-unity-server.git
cd mcp-unity-server
npm install
```

### 2. Development Setup

```bash
# Build TypeScript
npm run build

# Run in development mode
npm run dev
```

### 3. Test Your Changes

1. Update your `.cursor/mcp.json` to point to local build
2. Restart Cursor
3. Test in Unity
4. Verify all tools work

## 🛠️ Adding New Tools

### Node.js Side (src/index.ts)

Add tool definition to the `tools` array:

```typescript
{
  name: "unity_my_new_tool",
  description: "What it does",
  inputSchema: {
    type: "object",
    properties: {
      param1: { type: "string", description: "Parameter description" }
    },
    required: ["param1"],
  },
},
```

### Unity Side

1. **Create new tool class** in `UnityMCP/Tools/MyNewTool.cs`:

```csharp
#if UNITY_EDITOR
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    public static class MyNewTool
    {
        public static object DoSomething(JObject args)
        {
            // Your implementation
            return new { success = true, message = "Done!" };
        }
    }
}
#endif
```

2. **Add routing** in `UnityMCP/UnityMCPBridge.Router.cs`:

```csharp
case "unity_my_new_tool": return MyNewTool.DoSomething(args);
```

3. **Document** in `UnityMCP/TOOLS_REFERENCE.md`

### Testing

- Test with multiple Unity versions if possible
- Verify Undo system works correctly
- Check for edge cases (null values, missing objects)
- Ensure error messages are helpful

## 📝 Code Style

- **Follow SOLID principles**
- **Each tool class = single responsibility**
- **Use Undo system** for all modifications
- **Meaningful variable names**
- **Add comments for complex logic**
- **Handle errors gracefully**

## 🐛 Reporting Bugs

Please include:
- Unity version
- Node.js version
- Steps to reproduce
- Expected vs actual behavior
- Error messages/stack traces

## 💡 Feature Requests

Open an issue describing:
- What you want to achieve
- Why it's useful
- Example use case
- Proposed API (if you have ideas)

## 📜 License

By contributing, you agree your contributions will be licensed under MIT License.

## 🙏 Thank You!

Every contribution helps make Unity development with AI better for everyone!

