using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor menu items to apply UI font/settings across the project and fix button scales.
/// Tools > Cogniville > ...
/// </summary>
public static class CognivilleUIMenu
{
    const string UiFontGuid = "8f586378b4e144a9851e7b34d9b748ee";
    const int UiFontSize = 24;

    static TMP_FontAsset GetUIFont()
    {
        var path = AssetDatabase.GUIDToAssetPath(UiFontGuid);
        if (string.IsNullOrEmpty(path)) return null;
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
    }

    [MenuItem("Tools/Cogniville/Apply UI font to selected TextMeshPro")]
    public static void ApplyUIFontToSelection()
    {
        var font = GetUIFont();
        if (font == null) { EditorUtility.DisplayDialog("Cogniville", "UI font not found (LiberationSans SDF).", "OK"); return; }
        int n = 0;
        foreach (var go in Selection.gameObjects)
        {
            foreach (var t in go.GetComponentsInChildren<TMP_Text>(true))
            {
                ApplyFont(t, font);
                n++;
            }
        }
        if (n == 0) EditorUtility.DisplayDialog("Cogniville", "No TextMeshPro components in selection.", "OK");
        else { if (Selection.activeGameObject != null) EditorUtility.SetDirty(Selection.activeGameObject); Debug.Log($"[Cogniville] Applied UI font to {n} TMP component(s)."); }
    }

    [MenuItem("Tools/Cogniville/Apply UI font to selected TextMeshPro", true)]
    public static bool ApplyUIFontToSelectionValidate() => Selection.gameObjects.Length > 0;

    static void ApplyFont(TMP_Text tmp, TMP_FontAsset font)
    {
        if (tmp == null || font == null) return;
        Undo.RecordObject(tmp, "Apply UI font");
        tmp.font = font;
        tmp.fontSize = UiFontSize;
        tmp.fontSizeMin = 18;
        tmp.fontSizeMax = 72;
        tmp.enableAutoSizing = false;
    }

    [MenuItem("Tools/Cogniville/Apply UI font to all input fields in scene")]
    public static void ApplyUIFontToAllInputFields()
    {
        var font = GetUIFont();
        if (font == null) { EditorUtility.DisplayDialog("Cogniville", "UI font not found.", "OK"); return; }
        var inputs = Object.FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None);
        int n = 0;
        foreach (var input in inputs)
        {
            if (input.textComponent != null) { ApplyFont(input.textComponent, font); input.textComponent.color = Color.white; n++; }
            if (input.placeholder != null)
            {
                var ph = input.placeholder as TMP_Text;
                if (ph == null) ph = input.placeholder.GetComponent<TMP_Text>();
                if (ph != null) { ph.font = font; ph.fontSize = UiFontSize; ph.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); n++; }
            }
        }
        EditorUtility.DisplayDialog("Cogniville", $"Applied UI font to {n} input field text/placeholder(s) in scene.", "OK");
    }

    [MenuItem("Tools/Cogniville/Set all buttons hover scale to 1.1 (no shrink)")]
    public static void SetAllButtonsHoverScale()
    {
        var uifloats = Object.FindObjectsByType<UIFloat>(FindObjectsSortMode.None);
        int n = 0;
        foreach (var u in uifloats)
        {
            Undo.RecordObject(u, "Hover scale");
            u.hoverScaleAmount = 1.1f;
            n++;
        }
        EditorUtility.DisplayDialog("Cogniville", $"Set hoverScaleAmount = 1.1 on {n} UIFloat component(s).", "OK");
    }

    [MenuItem("Tools/Cogniville/Set all buttons press scale to 1 (no shrink)")]
    public static void SetAllButtonsPressScale()
    {
        int n = 0;
        foreach (var f in Object.FindObjectsByType<ButtonFeedback>(FindObjectsSortMode.None))
        {
            Undo.RecordObject(f, "Press scale");
            f.scaleOnPress = 1f;
            n++;
        }
        foreach (var a in Object.FindObjectsByType<ButtonPressAnimator>(FindObjectsSortMode.None))
        {
            Undo.RecordObject(a, "Press scale");
            a.scaleOnPress = 1f;
            n++;
        }
        EditorUtility.DisplayDialog("Cogniville", $"Set scaleOnPress = 1 on {n} button feedback component(s).", "OK");
    }
}
