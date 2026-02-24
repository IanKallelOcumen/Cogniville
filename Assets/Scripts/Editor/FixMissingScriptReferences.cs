using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Fixes "Missing Script" references by updating the scene file to use the current
/// package script GUIDs (UGUI Mask/ScrollRect, URP Camera, 2D Light).
/// Run from menu: Tools → Cogniville → Fix Missing Script References
/// </summary>
public static class FixMissingScriptReferences
{
    // Old GUIDs that show as missing when package version doesn't match (no .meta in Assets)
    private static readonly Dictionary<string, Type> KnownMissingGuidToType = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
    {
        { "31a19414c41e5ae4aae2af33fee712f6", typeof(Mask) },
        { "1aa08ab6e0800fa44ae55d278d1423e3", typeof(ScrollRect) },
        { "a79441f348de89743a2939f4d699eac1", GetTypeByName("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime") },
        { "073797afb82c5a1438f328866b10b3f0", GetTypeByName("UnityEngine.Rendering.Universal.GlobalLight2D, Unity.RenderPipelines.Universal.Runtime") },
    };

    private static Type GetTypeByName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return null;
        var type = Type.GetType(fullName);
        if (type != null) return type;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = asm.GetType(fullName.Split(',')[0].Trim());
            if (type != null) return type;
        }
        return null;
    }

    [MenuItem("Tools/Cogniville/Fix Missing Script References")]
    public static void FixMissingScripts()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded || string.IsNullOrEmpty(scene.path))
        {
            Debug.LogWarning("Open a saved scene first, then run Fix Missing Script References.");
            return;
        }

        var scenePath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", scene.path));
        if (!File.Exists(scenePath))
        {
            Debug.LogWarning("Scene file not found: " + scene.path);
            return;
        }

        var text = File.ReadAllText(scenePath);
        int replaceCount = 0;

        foreach (var kv in KnownMissingGuidToType)
        {
            var oldGuid = kv.Key;
            var type = kv.Value;
            if (type == null) continue;
            var newGuid = GetCurrentScriptGuid(type);
            if (string.IsNullOrEmpty(newGuid) || string.Equals(oldGuid, newGuid, StringComparison.OrdinalIgnoreCase))
                continue;
            var oldPattern = "guid: " + oldGuid;
            var newPattern = "guid: " + newGuid;
            var count = 0;
            text = ReplaceFirstOccurrences(text, oldPattern, newPattern, 100, out count);
            if (count > 0)
            {
                replaceCount += count;
                Debug.Log($"Replaced {oldGuid} → {newGuid} ({type.Name}) x{count}");
            }
        }

        if (replaceCount > 0)
        {
            File.WriteAllText(scenePath, text);
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(scene.path);
            Debug.Log($"Fix Missing Scripts: updated {replaceCount} reference(s) in scene. Save the scene to keep changes.");
        }
        else
        {
            Debug.Log("Fix Missing Scripts: no known missing GUIDs found in scene, or current package GUIDs match. Try reimporting com.unity.ugui and com.unity.render-pipelines.universal.");
        }
    }

    private static string GetCurrentScriptGuid(Type type)
    {
        if (type == null) return null;
        try
        {
            var go = new GameObject("_temp_script_lookup");
            try
            {
                var comp = go.AddComponent(type);
                if (comp is MonoBehaviour mb)
                {
                    var script = MonoScript.FromMonoBehaviour(mb);
                    if (script != null)
                    {
                        var path = AssetDatabase.GetAssetPath(script);
                        if (!string.IsNullOrEmpty(path))
                            return AssetDatabase.AssetPathToGUID(path);
                    }
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"GetCurrentScriptGuid({type.Name}): {e.Message}");
        }
        return null;
    }

    private static string ReplaceFirstOccurrences(string text, string oldValue, string newValue, int maxReplace, out int count)
    {
        count = 0;
        int index;
        while (count < maxReplace && (index = text.IndexOf(oldValue, StringComparison.Ordinal)) >= 0)
        {
            text = text.Substring(0, index) + newValue + text.Substring(index + oldValue.Length);
            count++;
        }
        return text;
    }
}
