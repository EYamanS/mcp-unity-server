// Tools/UITools.cs
// UI Canvas and element creation
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using System;
using Newtonsoft.Json.Linq;

namespace UnityMCP
{
    public static class UITools
    {
        public static object CreateCanvas(JObject args)
        {
            var name = args?["name"]?.ToString() ?? "Canvas";
            var obj = new GameObject(name);
            var canvas = Undo.AddComponent<Canvas>(obj);
            var scaler = Undo.AddComponent<CanvasScaler>(obj);
            var raycaster = Undo.AddComponent<GraphicRaycaster>(obj);

            var renderModeStr = args?["renderMode"]?.ToString() ?? "ScreenSpaceOverlay";
            canvas.renderMode = renderModeStr switch
            {
                "ScreenSpaceOverlay" => RenderMode.ScreenSpaceOverlay,
                "ScreenSpaceCamera" => RenderMode.ScreenSpaceCamera,
                "WorldSpace" => RenderMode.WorldSpace,
                _ => RenderMode.ScreenSpaceOverlay
            };

            // Ensure EventSystem exists
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                Undo.AddComponent<EventSystem>(eventSystem);
                Undo.AddComponent<StandaloneInputModule>(eventSystem);
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
            }

            Undo.RegisterCreatedObjectUndo(obj, "Create Canvas");
            return new { success = true, name = obj.name, message = $"Created Canvas '{name}'" };
        }

        public static object CreateElement(JObject args)
        {
            var type = args.Value<string>("type");
            var name = args.Value<string>("name");
            var parentName = args?["parent"]?.ToString();
            var text = args?["text"]?.ToString();

            GameObject parent = null;
            if (!string.IsNullOrEmpty(parentName))
                parent = UnityMCPBridge.FindGameObjectOrThrow(parentName);

            GameObject uiObj = type switch
            {
                "Panel" => CreatePanel(name),
                "Image" => CreateImage(name),
                "Text" => CreateText(name, text),
                "Button" => CreateButton(name, text),
                "InputField" => CreateInputField(name),
                "Slider" => CreateSlider(name),
                _ => throw new Exception($"Unknown UI element type: {type}")
            };

            if (parent != null)
                uiObj.transform.SetParent(parent.transform, false);

            Undo.RegisterCreatedObjectUndo(uiObj, $"Create UI {type}");
            return new { success = true, name = uiObj.name, message = $"Created UI {type} '{name}'" };
        }

        private static GameObject CreatePanel(string name)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            var image = Undo.AddComponent<Image>(obj);
            image.color = new Color(1, 1, 1, 0.4f);
            return obj;
        }

        private static GameObject CreateImage(string name)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            Undo.AddComponent<Image>(obj);
            return obj;
        }

        private static GameObject CreateText(string name, string content)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            var textComp = Undo.AddComponent<Text>(obj);
            textComp.text = content ?? "New Text";
            textComp.color = Color.black;
            return obj;
        }

        private static GameObject CreateButton(string name, string content)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            var button = Undo.AddComponent<Button>(obj);
            var btnImage = Undo.AddComponent<Image>(obj);
            
            // Add text child
            var textObj = new GameObject("Text", typeof(RectTransform));
            var btnText = Undo.AddComponent<Text>(textObj);
            btnText.text = content ?? "Button";
            btnText.color = Color.black;
            btnText.alignment = TextAnchor.MiddleCenter;
            textObj.transform.SetParent(obj.transform);
            
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Undo.RegisterCreatedObjectUndo(textObj, "Create Button Text");
            return obj;
        }

        private static GameObject CreateInputField(string name)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            var inputField = Undo.AddComponent<InputField>(obj);
            Undo.AddComponent<Image>(obj);
            
            var inputText = new GameObject("Text", typeof(RectTransform));
            var inputTextComp = Undo.AddComponent<Text>(inputText);
            inputTextComp.supportRichText = false;
            inputText.transform.SetParent(obj.transform);
            inputField.textComponent = inputTextComp;
            
            Undo.RegisterCreatedObjectUndo(inputText, "Create InputField Text");
            return obj;
        }

        private static GameObject CreateSlider(string name)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            Undo.AddComponent<Slider>(obj);
            return obj;
        }

        public static object SetRectTransform(JObject args)
        {
            var name = args.Value<string>("name");
            var obj = UnityMCPBridge.FindGameObjectOrThrow(name);
            var rect = obj.GetComponent<RectTransform>() 
                ?? throw new Exception($"No RectTransform on '{name}'");

            Undo.RecordObject(rect, "Set RectTransform");

            if (args["anchorMin"] is JObject min)
                rect.anchorMin = UnityMCPBridge.ParseVector2(min, rect.anchorMin);
            
            if (args["anchorMax"] is JObject max)
                rect.anchorMax = UnityMCPBridge.ParseVector2(max, rect.anchorMax);
            
            if (args["pivot"] is JObject piv)
                rect.pivot = UnityMCPBridge.ParseVector2(piv, rect.pivot);
            
            if (args["sizeDelta"] is JObject size)
                rect.sizeDelta = UnityMCPBridge.ParseVector2(size, rect.sizeDelta);
            
            if (args["anchoredPosition"] is JObject pos)
                rect.anchoredPosition = UnityMCPBridge.ParseVector2(pos, rect.anchoredPosition);

            return new { success = true, message = $"Set RectTransform on '{name}'" };
        }
    }
}
#endif

