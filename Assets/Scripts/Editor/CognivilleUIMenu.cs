using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor menu items to apply UI font/settings across the project and fix button scales.
/// Tools > Cogniville > ...
/// </summary>
public static class CognivilleUIMenu
{
    const string UiFontGuid = "042baed0a39830a4e9cfe1c6d37b133f"; // Comic Jungle SDF
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
        if (font == null) { EditorUtility.DisplayDialog("Cogniville", "UI font not found (Comic Jungle SDF).", "OK"); return; }
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

    /// <summary>
    /// Standard: same sprite (Play button idle), same pressed sprite, and same font on all buttons and text in the open scene.
    /// Uses the first Button that has an Image with a sprite as the reference (e.g. Play button).
    /// </summary>
    [MenuItem("Tools/Cogniville/Apply standard UI (Play look) – all buttons + fonts in scene")]
    public static void ApplyStandardPlayLookInScene()
    {
        UnityEngine.Object[] objs = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Button referenceButton = null;
        foreach (Button b in objs)
        {
            var img = b.GetComponent<Image>();
            if (img != null && img.sprite != null)
            {
                referenceButton = b;
                if (b.gameObject.name.IndexOf("Play", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    break;
            }
        }
        if (referenceButton == null)
        {
            EditorUtility.DisplayDialog("Cogniville", "No button with a sprite found in this scene. Open a scene that contains the Play button (e.g. WorldSelect or MainMenu), then run this again.", "OK");
            return;
        }
        Sprite idleSprite = referenceButton.GetComponent<Image>().sprite;
        Sprite pressedSprite = referenceButton.spriteState.pressedSprite;
        if (pressedSprite == null) pressedSprite = idleSprite;

        int buttonsUpdated = 0;
        int imagesUpdated = 0;
        foreach (Button b in objs)
        {
            var img = b.GetComponent<Image>();
            if (img != null)
            {
                Undo.RecordObject(img, "Apply standard button sprite");
                if (img.sprite != idleSprite) { img.sprite = idleSprite; imagesUpdated++; }
            }
            var selectable = b as UnityEngine.UI.Selectable;
            if (selectable != null && selectable.transition == Selectable.Transition.SpriteSwap)
            {
                var ss = selectable.spriteState;
                if (ss.pressedSprite != pressedSprite)
                {
                    Undo.RecordObject(selectable, "Apply standard pressed sprite");
                    ss.pressedSprite = pressedSprite;
                    selectable.spriteState = ss;
                    buttonsUpdated++;
                }
            }
            else if (selectable != null && selectable.transition != Selectable.Transition.SpriteSwap)
            {
                Undo.RecordObject(selectable, "Set Sprite Swap");
                selectable.transition = Selectable.Transition.SpriteSwap;
                var ss = selectable.spriteState;
                ss.pressedSprite = pressedSprite;
                selectable.spriteState = ss;
                buttonsUpdated++;
            }
        }

        var font = GetUIFont();
        int textsUpdated = 0;
        if (font != null)
        {
            foreach (var tmp in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Undo.RecordObject(tmp, "Apply UI font");
                if (tmp.font != font) { ApplyFont(tmp, font); textsUpdated++; }
            }
            foreach (var input in Object.FindObjectsByType<TMP_InputField>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (input.textComponent != null && input.textComponent.font != font) { ApplyFont(input.textComponent, font); input.textComponent.color = new Color(0.2f, 0.2f, 0.2f, 1f); textsUpdated++; }
                if (input.placeholder != null)
                {
                    var ph = input.placeholder as TMP_Text ?? input.placeholder.GetComponent<TMP_Text>();
                    if (ph != null && ph.font != font) { ph.font = font; ph.fontSize = UiFontSize; ph.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); textsUpdated++; }
                }
            }
        }

        if (EditorSceneManager.GetActiveScene().isDirty)
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Cogniville", $"Standard UI applied:\n• Button (idle) sprite: {imagesUpdated} Image(s)\n• Button (pressed) sprite: {buttonsUpdated} Button(s)\n• Font (Comic Jungle SDF): {textsUpdated} TMP/font(s)", "OK");
    }
}
