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
    [Tooltip("Teacher first name. A random code is generated and shown when added.")]
    public TMP_InputField teacherNameInput;
    public Button addTeacherButton;
    public Button backButton;
    [Tooltip("Single text that lists all teachers with their codes (one per line)")]
    public TextMeshProUGUI teachersListText;
    [Tooltip("Optional: message when list is empty")]
    public string emptyListMessage = "No teachers added yet. Add a teacher first name above. You'll get a code to give them.";

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
        string firstName = GetTeacherNameFromInput();
        if (string.IsNullOrWhiteSpace(firstName))
        {
            if (teachersListText) teachersListText.text = "Enter teacher first name first.";
            return;
        }

        if (GameDataManager.Instance != null)
        {
            string code = GameDataManager.Instance.AddTeacher(firstName);
            if (code == null)
            {
                if (teachersListText) teachersListText.text = "A teacher with that name already exists.";
                return;
            }
            if (teacherNameInput) teacherNameInput.text = "";
            RefreshList();
            if (teachersListText)
                teachersListText.text += "\n\nGive this code to " + firstName.Trim() + ": " + code;
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

        var list = GameDataManager.Instance.GetTeachersWithCodes();
        if (list == null || list.Count == 0)
        {
            teachersListText.text = emptyListMessage;
            return;
        }

        var lines = new List<string> { "Teachers (give each their code to log in):" };
        foreach (var (name, code) in list)
            lines.Add(name + " — Code: " + code);
        teachersListText.text = string.Join("\n", lines);
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

    void AutoWire()
    {
        if (!teacherNameInput)
            teacherNameInput = transform.Find("TeacherNameInput")?.GetComponent<TMP_InputField>();
        if (!teacherNameInput)
            teacherNameInput = GetComponentInChildren<TMP_InputField>(true);

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
