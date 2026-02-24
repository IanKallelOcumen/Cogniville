using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Tools → Cogniville → Check for conflicting names in hierarchy
/// Finds duplicate GameObject names that can break Find() and assigned graphics.
/// </summary>
public static class CheckHierarchyDuplicateNames
{
    /// <summary>Names that code looks up by Find() - duplicates can bind the wrong object.</summary>
    static readonly string[] CodeLookupNames = new[]
    {
        "BackButton", "LoginButton", "LoginBackbutton", "ErrorText", "MessageText",
        "EmailInput", "PasswordInput", "SignupButton", "PrincipalButton",
        "TeacherNameInput", "AddTeacherButton", "TeachersListText",
        "StudentLastNameInput", "AddStudentButton", "SessionStudentsListText", "EndSessionButton",
        "PrincipalLogin", "LoginRoleChoice", "PrincipalCodeInput", "PrincipalLoginInfoText",
        "LeaderboardScrollView", "LeaderboardTitle",
        "ContinueButton", "EnterButton", "SessionButton", "ResultsButton", "HomeButton", "NextButton",
        "PlayButton", "AboutButton", "LeaderboardButton", "SoundToggle"
    };

    [MenuItem("Tools/Cogniville/Check for conflicting names in hierarchy")]
    public static void Run()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            EditorUtility.DisplayDialog("Cogniville", "Open a scene first.", "OK");
            return;
        }

        var nameToPaths = new Dictionary<string, List<string>>(System.StringComparer.Ordinal);
        foreach (var root in scene.GetRootGameObjects())
            CollectNames(root.transform, "", nameToPaths);

        var duplicates = new List<string>();
        foreach (var kv in nameToPaths)
            if (kv.Value.Count > 1) duplicates.Add(kv.Key);

        var codeUsedDuplicates = new List<string>();
        foreach (var name in CodeLookupNames)
        {
            if (nameToPaths.TryGetValue(name, out var paths) && paths.Count > 1)
                codeUsedDuplicates.Add(name);
        }

        if (duplicates.Count == 0)
        {
            Debug.Log("[Cogniville] No duplicate names in hierarchy. Assigned graphics lookups are safe.");
            EditorUtility.DisplayDialog("Cogniville", "No duplicate names found in hierarchy.\nAssigned graphics / Find() lookups are safe.", "OK");
            return;
        }

        var msg = "Duplicate names found (can break Find() and assigned graphics):\n\n";
        foreach (var name in duplicates)
        {
            bool isCodeUsed = codeUsedDuplicates.Contains(name);
            msg += (isCodeUsed ? "⚠ " : "") + name + " (" + nameToPaths[name].Count + "x)\n";
            foreach (var path in nameToPaths[name])
                msg += "  → " + path + "\n";
            msg += "\n";
        }
        if (codeUsedDuplicates.Count > 0)
            msg += "⚠ = used by code (rename one or ensure lookup is scoped to correct panel).\n";

        Debug.LogWarning("[Cogniville] " + msg.Replace("\n", " "));
        EditorUtility.DisplayDialog("Cogniville – Conflicting names", msg.TrimEnd() + "\n\nRename duplicates so each has a unique path, or ensure code finds by panel.", "OK");
    }

    static void CollectNames(Transform t, string parentPath, Dictionary<string, List<string>> nameToPaths)
    {
        string path = string.IsNullOrEmpty(parentPath) ? t.name : parentPath + "/" + t.name;
        string name = t.gameObject.name;
        if (!nameToPaths.ContainsKey(name)) nameToPaths[name] = new List<string>();
        nameToPaths[name].Add(path);
        for (int i = 0; i < t.childCount; i++)
            CollectNames(t.GetChild(i), path, nameToPaths);
    }
}
