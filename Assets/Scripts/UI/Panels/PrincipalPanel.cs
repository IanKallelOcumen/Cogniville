using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Principal screen: add and manage teachers.
/// Panel Name is for students; this panel is for principals to add teachers who can then log in.
/// </summary>
public class PrincipalPanel : MonoBehaviour
{
    [Header("UI References (optional - auto-find by name)")]
    public TMP_InputField teacherNameInput;
    [Tooltip("Optional: password for the teacher. If set, teacher must enter this to log in.")]
    public TMP_InputField teacherPasswordInput;
    public Button addTeacherButton;
    public Button backButton;
    [Tooltip("Single text that lists all teachers (one per line)")]
    public TextMeshProUGUI teachersListText;
    [Tooltip("Optional: message when list is empty")]
    public string emptyListMessage = "No teachers added yet. Add a teacher name and password above.";

    private void OnEnable()
    {
        if (!teacherNameInput || !addTeacherButton || !teachersListText)
            AutoWire();
        if (addTeacherButton)
        {
            addTeacherButton.onClick.RemoveAllListeners();
            addTeacherButton.onClick.AddListener(OnAddTeacher);
        }
        if (backButton)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBack);
        }
        RefreshList();
    }

    public void OnAddTeacher()
    {
        string name = GetTeacherNameFromInput();
        if (string.IsNullOrWhiteSpace(name))
        {
            if (teachersListText) teachersListText.text = "Enter a teacher name first.";
            return;
        }
        string password = GetTeacherPasswordFromInput();

        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.AddTeacher(name, password);
            if (teacherNameInput) teacherNameInput.text = "";
            if (teacherPasswordInput) teacherPasswordInput.text = "";
            RefreshList();
        }
    }

    public void OnBack()
    {
#if UNITY_2023_1_OR_NEWER
        var controller = Object.FindFirstObjectByType<MainMenuController>();
#else
        var controller = Object.FindObjectOfType<MainMenuController>();
#endif
        if (controller) controller.OnBack();
    }

    void RefreshList()
    {
        if (!teachersListText) return;

        if (GameDataManager.Instance == null)
        {
            teachersListText.text = emptyListMessage;
            return;
        }

        var list = GameDataManager.Instance.GetTeachers();
        if (list == null || list.Count == 0)
        {
            teachersListText.text = emptyListMessage;
            return;
        }

        teachersListText.text = "Teachers:\n" + string.Join("\n", list);
    }

    string GetTeacherNameFromInput()
    {
        if (teacherNameInput != null)
            return (teacherNameInput.text ?? "").Trim();
        var all = GetComponentsInChildren<TMP_InputField>(true);
        if (all != null && all.Length > 0)
        {
            foreach (var t in all)
                if (t != null && (t.name ?? "").ToLowerInvariant().Contains("name"))
                    return (t.text ?? "").Trim();
            return (all[0].text ?? "").Trim();
        }
        return "";
    }

    string GetTeacherPasswordFromInput()
    {
        if (teacherPasswordInput != null)
            return (teacherPasswordInput.text ?? "").Trim();
        var all = GetComponentsInChildren<TMP_InputField>(true);
        if (all != null)
            foreach (var t in all)
                if (t != null && (t.name ?? "").ToLowerInvariant().Contains("password"))
                    return (t.text ?? "").Trim();
        return "";
    }

    void AutoWire()
    {
        if (!teacherNameInput)
            teacherNameInput = transform.Find("TeacherNameInput")?.GetComponent<TMP_InputField>();
        if (!teacherNameInput)
            teacherNameInput = GetComponentInChildren<TMP_InputField>(true);
        if (!teacherPasswordInput)
        {
            teacherPasswordInput = transform.Find("TeacherPasswordInput")?.GetComponent<TMP_InputField>();
            if (!teacherPasswordInput)
            {
                var all = GetComponentsInChildren<TMP_InputField>(true);
                if (all != null)
                    foreach (var t in all)
                        if (t != null && (t.name ?? "").ToLowerInvariant().Contains("password"))
                        { teacherPasswordInput = t; break; }
            }
        }

        if (!addTeacherButton)
        {
            var t = transform.Find("AddTeacherButton");
            if (t) addTeacherButton = t.GetComponent<Button>();
            if (!addTeacherButton)
            {
                var btns = GetComponentsInChildren<Button>(true);
                foreach (var b in btns)
                    if (b.name.ToLowerInvariant().Contains("add")) { addTeacherButton = b; break; }
            }
        }

        if (!backButton)
        {
            var t = transform.Find("BackButton");
            if (t) backButton = t.GetComponent<Button>();
            if (!backButton)
            {
                var btns = GetComponentsInChildren<Button>(true);
                foreach (var b in btns)
                    if (b.name.ToLowerInvariant().Contains("back")) { backButton = b; break; }
            }
        }

        if (!teachersListText)
        {
            teachersListText = transform.Find("TeachersListText")?.GetComponent<TextMeshProUGUI>();
            if (!teachersListText)
            {
                var all = GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var tmp in all)
                    if (tmp.name.ToLowerInvariant().Contains("list") || tmp.name.ToLowerInvariant().Contains("teachers"))
                    { teachersListText = tmp; break; }
            }
        }
    }
}
