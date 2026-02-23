using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays current session info (teacher name, status) and End Session button.
/// Attach to PanelSession. Refreshes when enabled.
/// </summary>
public class SessionPanel : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI teacherNameText;
    public TextMeshProUGUI sessionStatusText;
    public Button endSessionButton;

    private void OnEnable()
    {
        if (!teacherNameText || !sessionStatusText || !endSessionButton)
            AutoWireFields();
        RefreshSessionDisplay();
        if (endSessionButton)
        {
            endSessionButton.onClick.RemoveListener(OnEndSession);
            endSessionButton.onClick.AddListener(OnEndSession);
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
