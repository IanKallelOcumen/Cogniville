using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Displays current session info (teacher name, status), lets teacher add students (last name → code), and End Session button.
/// Attach to PanelSession. Refreshes when enabled.
/// </summary>
public class SessionPanel : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI teacherNameText;
    public TextMeshProUGUI sessionStatusText;
    public Button endSessionButton;
    [Header("Add students (optional - auto-find by name)")]
    public TMP_InputField studentLastNameInput;
    public Button addStudentButton;
    public TextMeshProUGUI sessionStudentsListText;
    [Header("Generated code display (shows code to give student after adding)")]
    public TextMeshProUGUI generatedCodeDisplayText;
    [Header("Navigation (same as student: Play / BookSelect)")]
    public Button playButton;
    public Button bookSelectButton;

    private void OnEnable()
    {
        if (!teacherNameText || !sessionStatusText || !endSessionButton)
            AutoWireFields();
        if (!studentLastNameInput || !addStudentButton || !sessionStudentsListText)
            AutoWireStudentFields();
        if (!generatedCodeDisplayText || !playButton || !bookSelectButton)
            AutoWireGeneratedCodeAndNav();
        RefreshSessionDisplay();
        RefreshSessionStudentsList();
        if (endSessionButton)
        {
            endSessionButton.onClick.RemoveListener(OnEndSession);
            endSessionButton.onClick.AddListener(OnEndSession);
        }
        if (addStudentButton)
        {
            addStudentButton.onClick.RemoveListener(OnAddSessionStudent);
            addStudentButton.onClick.AddListener(OnAddSessionStudent);
        }
        WireNavigationButtons();
    }

    void WireNavigationButtons()
    {
        var menu = Object.FindFirstObjectByType<MainMenuController>();
        if (!menu) return;
        if (playButton)
        {
            playButton.onClick.RemoveListener(menu.OnPlay);
            playButton.onClick.AddListener(menu.OnPlay);
        }
        if (bookSelectButton)
        {
            bookSelectButton.onClick.RemoveListener(menu.OnGoToBookSelect);
            bookSelectButton.onClick.AddListener(menu.OnGoToBookSelect);
        }
    }

    private void RefreshSessionDisplay()
    {
        if (GameDataManager.Instance == null) return;

        bool active = GameDataManager.Instance.IsSessionActive();
        string teacher = GameDataManager.Instance.GetSessionTeacher();
        if (teacherNameText) teacherNameText.text = string.IsNullOrEmpty(teacher) ? "No session" : teacher;
        if (sessionStatusText)
        {
            if (teacherNameText)
                sessionStatusText.text = active ? "Session active" : "No active session";
            else
                sessionStatusText.text = string.IsNullOrEmpty(teacher) ? "No session" : (active ? $"Teacher: {teacher} — Session active" : $"Teacher: {teacher} — Ended");
        }
        if (endSessionButton) endSessionButton.interactable = active;
        if (addStudentButton) addStudentButton.interactable = active;
        if (studentLastNameInput) studentLastNameInput.interactable = active;
        if (generatedCodeDisplayText && !active) generatedCodeDisplayText.text = "";
    }

    public void OnAddSessionStudent()
    {
        if (GameDataManager.Instance == null || !GameDataManager.Instance.IsSessionActive()) return;
        string lastName = GetStudentLastNameFromInput();
        if (string.IsNullOrWhiteSpace(lastName))
        {
            if (sessionStudentsListText) sessionStudentsListText.text = "Enter student last name first.";
            return;
        }
        string code = GameDataManager.Instance.AddSessionStudent(lastName);
        if (code == null)
        {
            if (sessionStudentsListText) sessionStudentsListText.text = "A student with that last name is already in this session.";
            return;
        }
        if (studentLastNameInput) studentLastNameInput.text = "";
        RefreshSessionStudentsList();
        if (sessionStudentsListText)
            sessionStudentsListText.text += "\n\nGive this code to " + lastName.Trim() + ": " + code;
        if (generatedCodeDisplayText)
            generatedCodeDisplayText.text = "Code for " + lastName.Trim() + ": " + code;
    }

    private void RefreshSessionStudentsList()
    {
        if (!sessionStudentsListText) return;
        if (GameDataManager.Instance == null || !GameDataManager.Instance.IsSessionActive())
        {
            sessionStudentsListText.text = "Start a session to add students.";
            return;
        }
        var list = GameDataManager.Instance.GetSessionStudentsWithCodes();
        if (list == null || list.Count == 0)
        {
            sessionStudentsListText.text = "No students added yet. Add a student last name above; you'll get a code to give them.";
            return;
        }
        var lines = new List<string> { "Students in this session (give each their code to join):" };
        foreach (var (lastName, code) in list)
            lines.Add(lastName + " — Code: " + code);
        sessionStudentsListText.text = string.Join("\n", lines);
    }

    private string GetStudentLastNameFromInput()
    {
        if (studentLastNameInput != null)
            return (studentLastNameInput.text ?? "").Trim();
        var all = GetComponentsInChildren<TMP_InputField>(true);
        if (all != null)
            foreach (var t in all)
                if (t != null && (t.name ?? "").ToLowerInvariant().Contains("last"))
                    return (t.text ?? "").Trim();
        return "";
    }

    private void AutoWireStudentFields()
    {
        if (!studentLastNameInput)
        {
            var t = transform.Find("StudentLastNameInput");
            if (t) studentLastNameInput = t.GetComponent<TMP_InputField>();
            if (!studentLastNameInput)
            {
                var all = GetComponentsInChildren<TMP_InputField>(true);
                foreach (var f in all)
                    if (f != null && (f.name ?? "").ToLowerInvariant().Contains("last"))
                    { studentLastNameInput = f; break; }
            }
        }
        if (!addStudentButton)
        {
            var t = transform.Find("AddStudentButton");
            if (t) addStudentButton = t.GetComponent<Button>();
            if (!addStudentButton)
            {
                var btns = GetComponentsInChildren<Button>(true);
                foreach (var b in btns)
                    if (b.name.ToLowerInvariant().Contains("add"))
                    { addStudentButton = b; break; }
            }
        }
        if (!sessionStudentsListText)
        {
            sessionStudentsListText = transform.Find("SessionStudentsListText")?.GetComponent<TextMeshProUGUI>();
            if (!sessionStudentsListText)
            {
                var all = GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var tmp in all)
                    if (tmp.name.ToLowerInvariant().Contains("student") && tmp != teacherNameText && tmp != sessionStatusText)
                    { sessionStudentsListText = tmp; break; }
            }
        }
    }

    private void AutoWireGeneratedCodeAndNav()
    {
        if (!generatedCodeDisplayText)
        {
            var t = transform.Find("GeneratedCodeDisplayText");
            if (t) generatedCodeDisplayText = t.GetComponent<TextMeshProUGUI>();
            if (!generatedCodeDisplayText)
            {
                var all = GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var tmp in all)
                    if (tmp != null && (tmp.name ?? "").ToLowerInvariant().Contains("generatedcode"))
                    { generatedCodeDisplayText = tmp; break; }
            }
        }
        if (!playButton)
        {
            var t = transform.Find("PlayButton");
            if (t) playButton = t.GetComponent<Button>();
            if (!playButton)
            {
                var btns = GetComponentsInChildren<Button>(true);
                foreach (var b in btns)
                    if (b != null && (b.name ?? "").ToLowerInvariant() == "playbutton")
                    { playButton = b; break; }
            }
        }
        if (!bookSelectButton)
        {
            var t = transform.Find("BookSelectButton");
            if (t) bookSelectButton = t.GetComponent<Button>();
            if (!bookSelectButton)
            {
                var btns = GetComponentsInChildren<Button>(true);
                foreach (var b in btns)
                    if (b != null && (b.name ?? "").ToLowerInvariant().Contains("bookselect"))
                    { bookSelectButton = b; break; }
            }
        }
    }

    public void OnEndSession()
    {
        if (GameDataManager.Instance == null) return;
        GameDataManager.Instance.EndSession();
        RefreshSessionDisplay();
    }

    /// <summary>
    /// Auto-find common UI elements under this panel
    /// </summary>
    public void AutoWireFields()
    {
        if (!teacherNameText || !sessionStatusText)
        {
            var allTmp = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTmp)
            {
                string n = tmp.name.ToLowerInvariant();
                if (!teacherNameText && n.Contains("teacher")) teacherNameText = tmp;
                else if (!sessionStatusText && (n.Contains("status") || n.Contains("session") || n.Contains("studentcount"))) sessionStatusText = tmp;
            }
        }
        if (!endSessionButton)
        {
            var t = transform.Find("EndSessionButton");
            if (t) endSessionButton = t.GetComponent<Button>();
            if (!endSessionButton)
            {
                var allBtns = GetComponentsInChildren<Button>(true);
                foreach (var btn in allBtns)
                    if (btn.name.ToLowerInvariant().Contains("end")) { endSessionButton = btn; break; }
            }
        }
    }
}
