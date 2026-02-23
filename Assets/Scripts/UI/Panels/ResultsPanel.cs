using UnityEngine;
using TMPro;

/// <summary>
/// Displays the current player's game results (score, accuracy, time, correct/total).
/// Bind to the ResultsScreen GameObject; refreshes from GameDataManager when enabled.
/// </summary>
public class ResultsPanel : MonoBehaviour
{
    [Header("Stats Display (optional - auto-find by name)")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI timeTakenText;
    public TextMeshProUGUI correctTotalText;
    public TextMeshProUGUI playerNameText;
    [Tooltip("If set, all stats are shown in this one text (overrides individual refs for display)")]
    public TextMeshProUGUI combinedStatsText;

    private void OnEnable()
    {
        if (!scoreText || !accuracyText || !timeTakenText || !correctTotalText || !playerNameText)
            AutoWireFields();
        RefreshFromGameData();
    }

    /// <summary>
    /// Refresh displayed stats from GameDataManager
    /// </summary>
    public void RefreshFromGameData()
    {
        if (GameDataManager.Instance == null) return;

        var data = GameDataManager.Instance.GetPlayerData();
        if (combinedStatsText)
        {
            var name = string.IsNullOrEmpty(data.playerName) ? "Player" : data.playerName;
            bool hasRound = data.lastRoundTotal > 0;
            var acc = data.questionsCount > 0 ? $"{data.accuracy:F1}%" : "—";
            var time = data.timeTaken > 0 ? $"{data.timeTaken:F1}s" : "—";
            var ct = data.questionsCount > 0 ? $"{data.quesCorrect}/{data.questionsCount}" : "—";
            if (hasRound)
                combinedStatsText.text = $"{name}\nThis round: {data.lastRoundScore} pts  |  Accuracy: {acc}  |  Time: {time}  |  Correct: {ct}\nTotal score: {data.totalScore}";
            else
                combinedStatsText.text = $"{name}\nScore: {data.totalScore}  |  Accuracy: {acc}  |  Time: {time}  |  Correct: {ct}";
            // #region agent log
            DebugAgent.Log("ResultsPanel.cs:RefreshFromGameData", "Refreshed combinedStatsText", "{\"lastRoundTotal\":" + data.lastRoundTotal + ",\"lastRoundScore\":" + data.lastRoundScore + ",\"totalScore\":" + data.totalScore + ",\"textLen\":" + (combinedStatsText?.text?.Length ?? 0) + "}", "B");
            // #endregion
            return;
        }
        if (playerNameText) playerNameText.text = string.IsNullOrEmpty(data.playerName) ? "Player" : data.playerName;
        if (scoreText) scoreText.text = data.lastRoundTotal > 0 ? data.lastRoundScore.ToString() : data.totalScore.ToString();
        if (accuracyText) accuracyText.text = data.questionsCount > 0 ? $"{data.accuracy:F1}%" : "—";
        if (timeTakenText) timeTakenText.text = data.timeTaken > 0 ? $"{data.timeTaken:F1}s" : "—";
        if (correctTotalText) correctTotalText.text = data.questionsCount > 0 ? $"{data.quesCorrect} / {data.questionsCount}" : "—";
    }

    /// <summary>
    /// Programmatically set results (e.g. when returning from quiz before GameDataManager is updated)
    /// </summary>
    public void SetResults(int score, int correctAnswers, int totalQuestions, float timeTaken)
    {
        if (combinedStatsText)
        {
            var acc = totalQuestions > 0 ? $"{(correctAnswers / (float)totalQuestions) * 100f:F1}%" : "—";
            combinedStatsText.text = $"Score: {score}  |  Accuracy: {acc}  |  Time: {timeTaken:F1}s  |  Correct: {correctAnswers}/{totalQuestions}";
            return;
        }
        if (scoreText) scoreText.text = score.ToString();
        if (accuracyText) accuracyText.text = totalQuestions > 0 ? $"{(correctAnswers / (float)totalQuestions) * 100f:F1}%" : "—";
        if (timeTakenText) timeTakenText.text = $"{timeTaken:F1}s";
        if (correctTotalText) correctTotalText.text = $"{correctAnswers} / {totalQuestions}";
    }

    /// <summary>
    /// Auto-wire text fields by common names under this panel
    /// </summary>
    public void AutoWireFields()
    {
        var all = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in all)
        {
            string n = tmp.name.ToLowerInvariant();
            if (!combinedStatsText && (n.Contains("resultsstats") || n.Contains("combinedstats"))) combinedStatsText = tmp;
            else if (!scoreText && n.Contains("score")) scoreText = tmp;
            else if (!accuracyText && n.Contains("accuracy")) accuracyText = tmp;
            else if (!timeTakenText && n.Contains("time")) timeTakenText = tmp;
            else if (!correctTotalText && (n.Contains("correct") || n.Contains("total"))) correctTotalText = tmp;
            else if (!playerNameText && (n.Contains("player") || n.Contains("name"))) playerNameText = tmp;
        }
    }
}
