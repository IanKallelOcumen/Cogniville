using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Game Data Manager - Handles player info, game state, results
/// Bridges between screens and game logic
/// Singleton for easy access
/// </summary>
public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [System.Serializable]
    public class PlayerData
    {
        public string playerName;
        public string teacherName;
        public int totalScore;
        public int quesCorrect;
        public int questionsCount;
        public float timeTaken;
        public float accuracy;
    }

    [System.Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public int score;
        public int rank;
    }

    [System.Serializable]
    public class StudentStats
    {
        public string studentName;
        public int score;
        public int correctAnswers;
        public int totalQuestions;
        public float accuracy;
        public float timeTaken;
        public long timestamp;
    }

    private PlayerData _currentPlayer = new PlayerData();
    private List<LeaderboardEntry> _leaderboard = new List<LeaderboardEntry>();
    private Dictionary<string, StudentStats> _sessionStudents = new Dictionary<string, StudentStats>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadPlayerData();
        LoadLeaderboard();
    }

    // ==================== SESSION MANAGEMENT ====================
    private const string SESSION_ACTIVE_KEY = "SessionActive";
    private const string SESSION_TEACHER_KEY = "SessionTeacher";
    
    /// <summary>
    /// Check if a teacher has authorized a session for students to play
    /// </summary>
    public bool IsSessionActive()
    {
        return PlayerPrefs.GetInt(SESSION_ACTIVE_KEY, 0) == 1;
    }
    
    /// <summary>
    /// Teacher allows/starts a session for students
    /// </summary>
    public void StartSession(string teacherName)
    {
        PlayerPrefs.SetInt(SESSION_ACTIVE_KEY, 1);
        PlayerPrefs.SetString(SESSION_TEACHER_KEY, teacherName);
        PlayerPrefs.Save();
        Debug.Log($"[GameData] Session started by teacher: {teacherName}");
    }
    
    /// <summary>
    /// End the current session (students can no longer play)
    /// </summary>
    public void EndSession()
    {
        PlayerPrefs.SetInt(SESSION_ACTIVE_KEY, 0);
        PlayerPrefs.Save();
        ClearSessionStudents();
        Debug.Log("[GameData] Session ended");
    }
    
    /// <summary>
    /// Get the current session teacher name
    /// </summary>
    public string GetSessionTeacher()
    {
        return PlayerPrefs.GetString(SESSION_TEACHER_KEY, "Unknown");
    }
    // ==================== END SESSION MANAGEMENT ====================

    // ==================== SESSION ENROLLMENT & STUDENT STATS ====================
    /// <summary>
    /// Enroll a student in the current session (teacher's session)
    /// </summary>
    public void EnrollStudentInSession(string studentName)
    {
        if (!_sessionStudents.ContainsKey(studentName))
        {
            _sessionStudents.Add(studentName, new StudentStats { studentName = studentName });
            Debug.Log($"[GameData] Student enrolled: {studentName}");
        }
    }

    /// <summary>
    /// Check if a student is enrolled in current session
    /// </summary>
    public bool IsStudentEnrolled(string studentName)
    {
        return _sessionStudents.ContainsKey(studentName);
    }

    /// <summary>
    /// Record student performance/stats in the current session
    /// </summary>
    public void RecordStudentStats(string studentName, int score, int correctAnswers, int totalQuestions, float timeTaken)
    {
        if (_sessionStudents.ContainsKey(studentName))
        {
            var stats = _sessionStudents[studentName];
            stats.score = score;
            stats.correctAnswers = correctAnswers;
            stats.totalQuestions = totalQuestions;
            stats.accuracy = totalQuestions > 0 ? (correctAnswers / (float)totalQuestions) * 100f : 0f;
            stats.timeTaken = timeTaken;
            stats.timestamp = System.DateTime.Now.Ticks;
            Debug.Log($"[GameData] Recorded stats for {studentName}: Score={score}, Accuracy={stats.accuracy:F1}%");
        }
        else
        {
            EnrollStudentInSession(studentName);
            RecordStudentStats(studentName, score, correctAnswers, totalQuestions, timeTaken);
        }
    }

    /// <summary>
    /// Get all students enrolled in current session with their stats
    /// Sorted by score (highest first)
    /// </summary>
    public List<StudentStats> GetSessionStudentsLeaderboard()
    {
        List<StudentStats> sessionLeaderboard = new List<StudentStats>(_sessionStudents.Values);
        sessionLeaderboard.Sort((a, b) => b.score.CompareTo(a.score));
        return sessionLeaderboard;
    }

    /// <summary>
    /// Get a specific student's stats from current session
    /// </summary>
    public StudentStats GetStudentStats(string studentName)
    {
        if (_sessionStudents.ContainsKey(studentName))
            return _sessionStudents[studentName];
        return null;
    }

    /// <summary>
    /// Clear current session students when session ends
    /// </summary>
    public void ClearSessionStudents()
    {
        _sessionStudents.Clear();
        Debug.Log("[GameData] Session students cleared");
    }
    // ==================== END SESSION ENROLLMENT & STUDENT STATS ====================

    /// <summary>
    /// Load player info from PlayerPrefs
    /// </summary>
    private void LoadPlayerData()
    {
        _currentPlayer.playerName = PlayerPrefs.GetString("PlayerName", "Guest");
        _currentPlayer.teacherName = PlayerPrefs.GetString("TeacherName", "Unknown");
        _currentPlayer.totalScore = PlayerPrefs.GetInt("TotalScore", 0);
        Debug.Log($"[GameDataManager] Loaded player: {_currentPlayer.playerName}");
    }

    /// <summary>
    /// Load leaderboard from PlayerPrefs (empty on first run)
    /// </summary>
    private void LoadLeaderboard()
    {
        // Leaderboard starts empty; populated during gameplay
        _leaderboard.Clear();
        Debug.Log("[GameDataManager] Leaderboard initialized");
    }

    /// <summary>
    /// Save all player data
    /// </summary>
    private void SavePlayerData()
    {
        PlayerPrefs.SetString("PlayerName", _currentPlayer.playerName);
        PlayerPrefs.SetString("TeacherName", _currentPlayer.teacherName);
        PlayerPrefs.SetInt("TotalScore", _currentPlayer.totalScore);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Update player with game results
    /// </summary>
    public void SetGameResults(int score, int correct, int total, float timeTaken)
    {
        _currentPlayer.quesCorrect = correct;
        _currentPlayer.questionsCount = total;
        _currentPlayer.timeTaken = timeTaken;
        _currentPlayer.totalScore += score;
        _currentPlayer.accuracy = total > 0 ? (correct / (float)total) * 100f : 0f;

        SavePlayerData();
        AddToLeaderboard(score);
        
        // Also record in session stats (for teacher's session leaderboard)
        RecordStudentStats(_currentPlayer.playerName, score, correct, total, timeTaken);

        Debug.Log($"[GameDataManager] Game Results - Score: {score}, Accuracy: {_currentPlayer.accuracy}%");
    }

    /// <summary>
    /// Set player name (from EnterStudentNameScreen)
    /// </summary>
    public void SetPlayerName(string name)
    {
        _currentPlayer.playerName = name;
        PlayerPrefs.SetString("PlayerName", name);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Set teacher name (from teacher selection)
    /// </summary>
    public void SetTeacher(string teacherName)
    {
        _currentPlayer.teacherName = teacherName;
        PlayerPrefs.SetString("TeacherName", teacherName);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Get current player data
    /// </summary>
    public PlayerData GetPlayerData()
    {
        return _currentPlayer;
    }

    /// <summary>
    /// Add score to leaderboard
    /// </summary>
    private void AddToLeaderboard(int score)
    {
        var entry = new LeaderboardEntry 
        { 
            playerName = _currentPlayer.playerName, 
            score = score 
        };
        _leaderboard.Add(entry);
        SortLeaderboard();
    }

    /// <summary>
    /// Get top N leaderboard entries
    /// </summary>
    public List<LeaderboardEntry> GetLeaderboard(int topCount = 10)
    {
        return _leaderboard.GetRange(0, Mathf.Min(topCount, _leaderboard.Count));
    }

    private void SortLeaderboard()
    {
        _leaderboard.Sort((a, b) => b.score.CompareTo(a.score));
        for (int i = 0; i < _leaderboard.Count; i++)
        {
            _leaderboard[i].rank = i + 1;
        }
    }

    /// <summary>
    /// Clear game session (reset for new game)
    /// </summary>
    public void ClearSession()
    {
        _currentPlayer.quesCorrect = 0;
        _currentPlayer.questionsCount = 0;
        _currentPlayer.timeTaken = 0;
        _currentPlayer.accuracy = 0;
    }
}
