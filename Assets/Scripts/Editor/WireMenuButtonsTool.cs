using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Tools → Cogniville → Double-check and wire menu buttons
/// Finds expected buttons (Login, Play, Principal, etc.) and adds persistent onClick listeners to MainMenuController methods.
/// Run with MainMenu (or your menu scene) open.
/// </summary>
public static class WireMenuButtonsTool
{
    static readonly (string buttonName, string methodName)[] ExpectedWiring = new[]
    {
        ("PlayButton", "OnPlay"),
        ("AboutButton", "OnAbout"),
        ("LeaderboardButton", "OnLeaderboard"),
        ("ExitButton", "OnExit"),
        ("LoginButton", "OnLogin"),
        ("Login", "OnLogin"),
        ("BtnLogin", "OnLogin"),
        ("LoginBtn", "OnLogin"),
        ("ContinueButton", "OnContinueFromName"),
        ("EnterButton", "OnContinueFromName"),
        ("SessionButton", "OnSession"),
        ("BookSelectButton", "OnGoToBookSelect"),
        ("ResultsButton", "OnResults"),
        ("HomeButton", "OnBack"),
        ("NextButton", "OnBack"),
        ("BackButton", "OnBack"),
        ("AboutBackButton", "OnBack"),
        ("LeaderboardBackButton", "OnBack"),
        ("LoginBackbutton", "OnLoginRoleBack"),
        ("PrincipalButton", "OnPrincipal"),
        ("TeacherButton", "OnTeacherChoice"),
        ("SubmitButton", "OnPrincipalCodeSubmit"),
        ("AddTeacherButton", null),
        ("EndSessionButton", null),
    };
    // Principal/Teacher/Submit on Panel Login are created at runtime; scene may not have them — that's OK.

    [MenuItem("Tools/Cogniville/Double-check and wire menu buttons")]
    public static void Run()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            EditorUtility.DisplayDialog("Cogniville", "Open the MainMenu (or menu) scene first.", "OK");
            return;
        }

        var controller = Object.FindFirstObjectByType<MainMenuController>();
        if (controller == null)
        {
            EditorUtility.DisplayDialog("Cogniville", "MainMenuController not found in the scene. Open the scene that has the main menu.", "OK");
            return;
        }

        var report = new List<string>();
        int wired = 0;
        int found = 0;
        int missing = 0;

        foreach (var (buttonName, methodName) in ExpectedWiring)
        {
            var buttonGo = FindInScene(buttonName);
            if (buttonGo == null && (buttonName.Contains("Login") || buttonName == "Login" || buttonName == "LoginButton"))
                buttonGo = FindInSceneContains("login");
            if (buttonGo == null)
            {
                report.Add("Missing: " + buttonName);
                missing++;
                continue;
            }
            found++;
            var button = buttonGo.GetComponent<Button>();
            if (button == null)
            {
                report.Add("Found but no Button: " + buttonName);
                continue;
            }
            if (string.IsNullOrEmpty(methodName))
            {
                report.Add("OK (panel script): " + buttonName);
                continue;
            }
            if (AddPersistentListenerIfNeeded(button, controller, methodName))
            {
                wired++;
                report.Add("Wired: " + buttonName + " -> " + methodName);
            }
            else
                report.Add("OK (already/listener): " + buttonName);
        }

        if (wired > 0) EditorSceneManager.MarkSceneDirty(scene);
        var msg = "Found " + found + " button(s), " + missing + " missing.\nWired " + wired + " to MainMenuController.\n\n" + string.Join("\n", report)
            + "\n\n(Principal/Teacher/Submit on login are created at runtime; missing in scene is OK.)";
        Debug.Log("[Cogniville] Wire buttons: " + msg.Replace("\n", " | "));
        EditorUtility.DisplayDialog("Cogniville – Wire buttons", msg, "OK");
    }

    static GameObject FindInScene(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            var t = root.transform.Find(name);
            if (t != null) return t.gameObject;
            var found = FindInChildren(root.transform, name);
            if (found != null) return found;
        }
        return null;
    }

    static GameObject FindInSceneContains(string namePart)
    {
        if (string.IsNullOrEmpty(namePart)) return null;
        var buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var b in buttons)
        {
            if (b != null && (b.gameObject.name ?? "").ToLowerInvariant().Contains(namePart.ToLowerInvariant()))
                return b.gameObject;
        }
        return null;
    }

    static GameObject FindInChildren(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.gameObject.name == name) return child.gameObject;
            var found = FindInChildren(child, name);
            if (found != null) return found;
        }
        return null;
    }

    static bool AddPersistentListenerIfNeeded(Button button, MainMenuController controller, string methodName)
    {
        var method = typeof(MainMenuController).GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, System.Type.EmptyTypes, null);
        if (method == null) return false;

        var so = new SerializedObject(button);
        var onClick = so.FindProperty("m_OnClick");
        if (onClick == null) return false;
        var calls = onClick.FindPropertyRelative("m_PersistentCalls.m_Calls");
        if (calls == null) return false;

        for (int i = 0; i < calls.arraySize; i++)
        {
            var call = calls.GetArrayElementAtIndex(i);
            var target = call.FindPropertyRelative("m_Target").objectReferenceValue;
            var methodStr = call.FindPropertyRelative("m_MethodName").stringValue;
            if (target == controller && methodStr == methodName) return false;
        }

        calls.arraySize++;
        var newCall = calls.GetArrayElementAtIndex(calls.arraySize - 1);
        newCall.FindPropertyRelative("m_Target").objectReferenceValue = controller;
        newCall.FindPropertyRelative("m_MethodName").stringValue = methodName;
        newCall.FindPropertyRelative("m_Mode").intValue = 1;
        newCall.FindPropertyRelative("m_Arguments").arraySize = 0;
        newCall.FindPropertyRelative("m_CallState").intValue = 2;
        so.ApplyModifiedPropertiesWithoutUndo();
        return true;
    }
}
