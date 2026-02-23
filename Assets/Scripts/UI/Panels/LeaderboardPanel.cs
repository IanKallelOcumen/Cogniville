using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Displays global leaderboard on panelLeaderboard
/// Shows top students across all sessions (not filtered by teacher)
/// </summary>
public class LeaderboardPanel : MonoBehaviour
{
    public Transform leaderboardContent;  // Content inside ScrollView Viewport
    public TextMeshProUGUI titleText;
    public int maxDisplayCount = 20;

    private List<GameDataManager.LeaderboardEntry> _leaderboardData = new List<GameDataManager.LeaderboardEntry>();

    [Header("Empty state")]
    public string emptyMessage = "No scores yet. Play a round to appear here!";

    private void OnEnable()
    {
        if (!leaderboardContent || !titleText)
            AutoWireContent();
        RefreshLeaderboard();
    }

    /// <summary>
    /// Refresh from Firebase (if configured) then display the global leaderboard
    /// </summary>
    public void RefreshLeaderboard()
    {
        if (GameDataManager.Instance == null) return;

        GameDataManager.Instance.RefreshLeaderboardFromBackend(() =>
        {
            if (!leaderboardContent) return;
            _leaderboardData = GameDataManager.Instance.GetLeaderboard(maxDisplayCount);
            if (titleText)
            {
                var teacher = GameDataManager.Instance.GetSessionTeacher();
                titleText.text = $"Leaderboard — {teacher}";
            }

            if (leaderboardContent)
            {
                foreach (Transform child in leaderboardContent)
                    Destroy(child.gameObject);
            }

            if (_leaderboardData.Count == 0 && !string.IsNullOrEmpty(emptyMessage))
            {
                var emptyGo = new GameObject("EmptyMessage");
                emptyGo.transform.SetParent(leaderboardContent, false);
                var t = emptyGo.AddComponent<TextMeshProUGUI>();
                t.text = emptyMessage;
                t.fontSize = 28;
                t.alignment = TextAlignmentOptions.Center;
                t.color = new Color(1f, 1f, 1f, 0.8f);
                emptyGo.AddComponent<LayoutElement>().preferredHeight = 80;
            }
            else
            {
                int rank = 1;
                foreach (var entry in _leaderboardData)
                {
                    DisplayLeaderboardEntry(rank, entry);
                    rank++;
                }
            }

            Debug.Log($"[LeaderboardPanel] Displayed {_leaderboardData.Count} global leaderboard entries");
        });
    }

    /// <summary>
    /// Create a UI entry for a leaderboard player
    /// </summary>
    private void DisplayLeaderboardEntry(int rank, GameDataManager.LeaderboardEntry entry)
    {
        if (!leaderboardContent) return;

        GameObject entryGO = new GameObject($"LeaderboardEntry_{rank}");
        entryGO.transform.SetParent(leaderboardContent, false);

        TextMeshProUGUI entryText = entryGO.AddComponent<TextMeshProUGUI>();
        entryText.text = $"{rank}. {entry.playerName} - Score: {entry.score}";
        entryText.fontSize = 32;
        entryText.alignment = TextAlignmentOptions.Left;
        entryText.color = Color.white;

        LayoutElement layout = entryGO.AddComponent<LayoutElement>();
        layout.preferredHeight = 50;
    }

    /// <summary>
    /// Auto-wire by finding content in hierarchy
    /// Call this from editor or manually set in Inspector
    /// </summary>
    public void AutoWireContent()
    {
        if (!leaderboardContent)
        {
            Transform scrollView = transform.Find("LeaderboardScrollView");
            if (scrollView)
            {
                leaderboardContent = scrollView.Find("Viewport/Content");
            }
        }

        if (!titleText)
        {
            titleText = transform.Find("LeaderboardTitle")?.GetComponent<TextMeshProUGUI>();
        }

        if (leaderboardContent)
            Debug.Log("[LeaderboardPanel] Auto-wired content successfully");
        else
            Debug.LogWarning("[LeaderboardPanel] Could not auto-wire content. Make sure ScrollView exists.");
    }
}
