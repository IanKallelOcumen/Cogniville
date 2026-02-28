using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tools → Cogniville → Setup Student Join UI
/// Ensures Panel Name has two inputs (last name + code) and Panel Session has add-student UI
/// so students can join with last name + code and teachers can add students by last name.
/// </summary>
public static class SetupStudentJoinUI
{
    const string PanelNameObjectName = "PanelName";
    const string PanelSessionObjectName = "PanelSession";
    const string CodeInputName = "StudentCodeInput";
    const string StudentLastNameInputName = "StudentLastNameInput";
    const string AddStudentButtonName = "AddStudentButton";
    const string SessionStudentsListTextName = "SessionStudentsListText";
    const string GeneratedCodeDisplayTextName = "GeneratedCodeDisplayText";
    const string PlayButtonName = "PlayButton";
    const string BookSelectButtonName = "BookSelectButton";

    [MenuItem("Tools/Cogniville/Setup Student Join UI (Panel Name + Session)")]
    public static void Run()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded || string.IsNullOrEmpty(scene.path))
        {
            EditorUtility.DisplayDialog("Cogniville", "Open MainMenu (or your menu scene) first, then run this tool.", "OK");
            return;
        }

        var panelName = GameObject.Find(PanelNameObjectName);
        var panelSession = GameObject.Find(PanelSessionObjectName);
        if (!panelName)
        {
            EditorUtility.DisplayDialog("Cogniville", "Could not find '" + PanelNameObjectName + "' in the scene.", "OK");
            return;
        }
        if (!panelSession)
        {
            EditorUtility.DisplayDialog("Cogniville", "Could not find '" + PanelSessionObjectName + "' in the scene.", "OK");
            return;
        }

        int changes = 0;
        changes += SetupPanelName(panelName);
        changes += SetupPanelSession(panelSession);

        if (changes > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[Cogniville] Setup Student Join UI: " + changes + " change(s). Save the scene to keep them.");
        }
        EditorUtility.DisplayDialog("Cogniville", changes > 0
            ? "Student Join UI setup complete.\n• Panel Name: last name + code inputs\n• Panel Session: add-student UI, generated code display, Play & BookSelect buttons\nSave the scene to keep changes."
            : "Nothing to change. Panel Name and Panel Session already have the required UI.", "OK");
    }

    static int SetupPanelName(GameObject panelName)
    {
        int count = 0;
        var inputs = panelName.GetComponentsInChildren<TMP_InputField>(true);
        if (inputs == null || inputs.Length == 0) return 0;

        // Prefer existing "code" input as second
        TMP_InputField codeInput = null;
        foreach (var i in inputs)
            if (i != null && (i.name ?? "").ToLowerInvariant().Contains("code"))
            { codeInput = i; break; }

        if (codeInput != null)
        {
            SetPlaceholder(codeInput, "Code");
            return count;
        }

        if (inputs.Length >= 2)
            return 0;

        // Only one input: duplicate it as the code field
        var first = inputs[0];
        var duplicate = Object.Instantiate(first.gameObject, panelName.transform);
        duplicate.name = CodeInputName;
        Undo.RegisterCreatedObjectUndo(duplicate, "Add code input");
        var dupInput = duplicate.GetComponent<TMP_InputField>();
        if (dupInput) { dupInput.text = ""; SetPlaceholder(dupInput, "Code"); }
        count++;
        return count;
    }

    static void SetPlaceholder(TMP_InputField field, string text)
    {
        if (field?.placeholder == null) return;
        var t = field.placeholder as TMP_Text ?? field.placeholder.GetComponent<TMP_Text>();
        if (t != null) { Undo.RecordObject(t, "Placeholder"); t.text = text; }
    }

    static int SetupPanelSession(GameObject panelSession)
    {
        int count = 0;
        var sessionPanel = panelSession.GetComponent<SessionPanel>();
        if (sessionPanel == null)
        {
            sessionPanel = panelSession.AddComponent<SessionPanel>();
            Undo.RegisterCreatedObjectUndo(sessionPanel, "Add SessionPanel");
            count++;
        }

        // Ensure add-student UI exists and is wired
        bool needInput = sessionPanel.studentLastNameInput == null;
        bool needButton = sessionPanel.addStudentButton == null;
        bool needListText = sessionPanel.sessionStudentsListText == null;
        bool needGeneratedCode = sessionPanel.generatedCodeDisplayText == null;
        bool needPlayBtn = sessionPanel.playButton == null;
        bool needBookSelectBtn = sessionPanel.bookSelectButton == null;

        if (needInput)
        {
            var existing = panelSession.GetComponentInChildren<TMP_InputField>(true);
            if (existing != null)
            {
                var go = Object.Instantiate(existing.gameObject, panelSession.transform);
                go.name = StudentLastNameInputName;
                Undo.RegisterCreatedObjectUndo(go, "Add StudentLastNameInput");
                sessionPanel.studentLastNameInput = go.GetComponent<TMP_InputField>();
                if (sessionPanel.studentLastNameInput != null)
                { sessionPanel.studentLastNameInput.text = ""; SetPlaceholder(sessionPanel.studentLastNameInput, "Student last name"); }
                count++;
            }
            else
            {
                var findInScene = Object.FindFirstObjectByType<TMP_InputField>(FindObjectsInactive.Include);
                if (findInScene != null)
                {
                    var go = Object.Instantiate(findInScene.gameObject, panelSession.transform);
                    go.name = StudentLastNameInputName;
                    Undo.RegisterCreatedObjectUndo(go, "Add StudentLastNameInput");
                    sessionPanel.studentLastNameInput = go.GetComponent<TMP_InputField>();
                    if (sessionPanel.studentLastNameInput != null)
                    { sessionPanel.studentLastNameInput.text = ""; SetPlaceholder(sessionPanel.studentLastNameInput, "Student last name"); }
                    count++;
                }
            }
        }

        if (needButton)
        {
            var existing = panelSession.GetComponentInChildren<Button>(true);
            if (existing != null)
            {
                var go = Object.Instantiate(existing.gameObject, panelSession.transform);
                go.name = AddStudentButtonName;
                Undo.RegisterCreatedObjectUndo(go, "Add AddStudentButton");
                var btn = go.GetComponent<Button>();
                if (btn != null) sessionPanel.addStudentButton = btn;
                var label = go.GetComponentInChildren<TMP_Text>(true);
                if (label != null) { Undo.RecordObject(label, "Label"); label.text = "Add student"; }
                count++;
            }
            else
            {
                var findInScene = Object.FindFirstObjectByType<Button>(FindObjectsInactive.Include);
                if (findInScene != null)
                {
                    var go = Object.Instantiate(findInScene.gameObject, panelSession.transform);
                    go.name = AddStudentButtonName;
                    Undo.RegisterCreatedObjectUndo(go, "Add AddStudentButton");
                    var btn = go.GetComponent<Button>();
                    if (btn != null) sessionPanel.addStudentButton = btn;
                    var label = go.GetComponentInChildren<TMP_Text>(true);
                    if (label != null) { Undo.RecordObject(label, "Label"); label.text = "Add student"; }
                    count++;
                }
            }
        }

        if (needListText)
        {
            var existing = panelSession.GetComponentInChildren<TextMeshProUGUI>(true);
            if (existing != null)
            {
                var go = Object.Instantiate(existing.gameObject, panelSession.transform);
                go.name = SessionStudentsListTextName;
                Undo.RegisterCreatedObjectUndo(go, "Add SessionStudentsListText");
                var tmp = go.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    sessionPanel.sessionStudentsListText = tmp;
                    Undo.RecordObject(tmp, "Text"); tmp.text = "Students in this session (add above, give each their code):";
                }
                count++;
            }
            else
            {
                var findInScene = Object.FindFirstObjectByType<TextMeshProUGUI>(FindObjectsInactive.Include);
                if (findInScene != null)
                {
                    var go = Object.Instantiate(findInScene.gameObject, panelSession.transform);
                    go.name = SessionStudentsListTextName;
                    Undo.RegisterCreatedObjectUndo(go, "Add SessionStudentsListText");
                    var tmp = go.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        sessionPanel.sessionStudentsListText = tmp;
                        Undo.RecordObject(tmp, "Text"); tmp.text = "Students in this session (add above, give each their code):";
                    }
                    count++;
                }
            }
        }

        if (needGeneratedCode)
        {
            var existing = panelSession.GetComponentInChildren<TextMeshProUGUI>(true);
            if (existing != null)
            {
                var go = Object.Instantiate(existing.gameObject, panelSession.transform);
                go.name = GeneratedCodeDisplayTextName;
                Undo.RegisterCreatedObjectUndo(go, "Add GeneratedCodeDisplayText");
                var tmp = go.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    sessionPanel.generatedCodeDisplayText = tmp;
                    Undo.RecordObject(tmp, "Text"); tmp.text = "";
                }
                count++;
            }
        }

        if (needPlayBtn)
        {
            var existing = panelSession.GetComponentInChildren<Button>(true);
            if (existing != null)
            {
                var go = Object.Instantiate(existing.gameObject, panelSession.transform);
                go.name = PlayButtonName;
                Undo.RegisterCreatedObjectUndo(go, "Add PlayButton");
                var btn = go.GetComponent<Button>();
                if (btn != null) sessionPanel.playButton = btn;
                var label = go.GetComponentInChildren<TMP_Text>(true);
                if (label != null) { Undo.RecordObject(label, "Label"); label.text = "Play"; }
                count++;
            }
        }

        if (needBookSelectBtn)
        {
            var existing = panelSession.GetComponentInChildren<Button>(true);
            if (existing != null)
            {
                var go = Object.Instantiate(existing.gameObject, panelSession.transform);
                go.name = BookSelectButtonName;
                Undo.RegisterCreatedObjectUndo(go, "Add BookSelectButton");
                var btn = go.GetComponent<Button>();
                if (btn != null) sessionPanel.bookSelectButton = btn;
                var label = go.GetComponentInChildren<TMP_Text>(true);
                if (label != null) { Undo.RecordObject(label, "Label"); label.text = "Book Select"; }
                count++;
            }
        }

        if (count > 0)
            EditorUtility.SetDirty(sessionPanel);
        return count;
    }
}
