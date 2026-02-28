using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor tool to wire UI buttons. Use UnityAction or lambda for AddListener to avoid CS1503.
/// </summary>
public static class WireAllUITool
{
    [MenuItem("Tools/Wire All UI")]
    public static void WireAllUI()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded) return;
        var buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int wired = 0;
        foreach (var btn in buttons)
        {
            if (btn == null) continue;
            var go = btn.gameObject;
            var name = go.name.ToLowerInvariant();
            if (name.Contains("back"))
            {
                if (btn.onClick.GetPersistentEventCount() == 0)
                {
                    UnityAction action = () => { if (Application.isPlaying) Debug.Log("Back"); };
                    btn.onClick.AddListener(action);
                    wired++;
                }
            }
        }
        if (wired > 0) EditorSceneManager.MarkSceneDirty(scene);
    }
}
