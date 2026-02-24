using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Game Data Manager - Handles player info, game state, results
/// Bridges between screens and game logic
/// Singleton for easy access
/// </summary>
public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    /// <summary>When true, MainMenu will auto-show Results panel on next load (e.g. after returning from a level).</summary>
    public static bool PendingShowResults { get; set; }

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
        /// <summary>Last completed round: score for that round only.</summary>
        public int lastRoundScore;
        public int lastRoundCorrect;
        public int lastRoundTotal;
        public float lastRoundTime;
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
    private List<(string lastName, string code)> _sessionStudentCodes = new List<(string, string)>();
    private const int SESSION_STUDENT_CODE_LENGTH = 6;
    private static readonly char[] SESSION_CODE_CHARS = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (FirebaseBackend.Instance == null && GetComponent<FirebaseBackend>() == null)
            gameObject.AddComponent<FirebaseBackend>();
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
        _sessionStudentCodes.Clear();
        if (FirebaseBackend.Instance != null && FirebaseBackend.Instance.IsReady)
            FirebaseBackend.Instance.SetSessionActive(teacherName, true);
        Debug.Log($"[GameData] Session started by teacher: {teacherName}");
    }
    
    /// <summary>
    /// End the current session (students can no longer play)
    /// </summary>
    public void EndSession()
    {
        var teacher = GetSessionTeacher();
        PlayerPrefs.SetInt(SESSION_ACTIVE_KEY, 0);
        PlayerPrefs.Save();
        if (FirebaseBackend.Instance != null && FirebaseBackend.Instance.IsReady)
            FirebaseBackend.Instance.SetSessionActive(teacher, false);
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
        _sessionStudentCodes.Clear();
        Debug.Log("[GameData] Session students cleared");
    }

    /// <summary>
    /// Teacher adds a student to the session by last name; returns the code to give to the student.
    /// </summary>
    public string AddSessionStudent(string studentLastName)
    {
        if (!IsSessionActive() || string.IsNullOrWhiteSpace(studentLastName)) return null;
        var lastName = studentLastName.Trim();
        if (_sessionStudentCodes.Exists(p => p.lastName.Equals(lastName, System.StringComparison.OrdinalIgnoreCase)))
            return null;
        var code = GenerateSessionStudentCode();
        _sessionStudentCodes.Add((lastName, code));
        Debug.Log($"[GameData] Session student added: {lastName} Code: {code}");
        return code;
    }

    private static string GenerateSessionStudentCode()
    {
        var s = new char[SESSION_STUDENT_CODE_LENGTH];
        for (int i = 0; i < SESSION_STUDENT_CODE_LENGTH; i++)
            s[i] = SESSION_CODE_CHARS[UnityEngine.Random.Range(0, SESSION_CODE_CHARS.Length)];
        return new string(s);
    }

    /// <summary>
    /// Get session students (last name + code) for teacher display.
    /// </summary>
    public List<(string lastName, string code)> GetSessionStudentsWithCodes()
    {
        return new List<(string, string)>(_sessionStudentCodes);
    }

    /// <summary>
    /// True if any students have been added to the current session (so we require last name + code to join).
    /// </summary>
    public bool HasSessionStudentsWithCodes()
    {
        return _sessionStudentCodes.Count > 0;
    }

    /// <summary>
    /// Check if student last name + code are allowed to join the current session.
    /// </summary>
    public bool IsAllowedSessionStudent(string studentLastName, string code)
    {
        if (string.IsNullOrWhiteSpace(studentLastName) || string.IsNullOrWhiteSpace(code)) return false;
        var lastName = studentLastName.Trim();
        var c = (code ?? "").Trim();
        foreach (var p in _sessionStudentCodes)
        {
            if (p.lastName.Equals(lastName, System.StringComparison.OrdinalIgnoreCase) && p.code == c)
                return true;
        }
        return false;
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
        _currentPlayer.lastRoundScore = score;
        _currentPlayer.lastRoundCorrect = correct;
        _currentPlayer.lastRoundTotal = total;
        _currentPlayer.lastRoundTime = timeTaken;
        _currentPlayer.quesCorrect = correct;
        _currentPlayer.questionsCount = total;
        _currentPlayer.timeTaken = timeTaken;
        _currentPlayer.totalScore += score;
        _currentPlayer.accuracy = total > 0 ? (correct / (float)total) * 100f : 0f;

        SavePlayerData();
        AddToLeaderboard(score);
        
        // Also record in session stats (for teacher's session leaderboard)
        RecordStudentStats(_currentPlayer.playerName, score, correct, total, timeTaken);

        PendingShowResults = true;
        Debug.Log($"[GameDataManager] Game Results - Score: {score}, Accuracy: {_currentPlayer.accuracy}%");
        // #region agent log
        DebugAgent.Log("GameDataManager.cs:SetGameResults", "SetGameResults called", "{\"score\":" + score + ",\"correct\":" + correct + ",\"total\":" + total + ",\"timeTaken\":" + timeTaken + ",\"pendingShowResults\":true}", "E");
        // #endregion
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
    /// Add score to leaderboard (local + Firebase if configured)
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
        if (FirebaseBackend.Instance != null && FirebaseBackend.Instance.IsReady)
            FirebaseBackend.Instance.AddLeaderboardEntry(_currentPlayer.playerName, score, _currentPlayer.teacherName);
    }

    /// <summary>
    /// Get top N leaderboard entries (from local cache)
    /// </summary>
    public List<LeaderboardEntry> GetLeaderboard(int topCount = 10)
    {
        if (_leaderboard == null || _leaderboard.Count == 0)
            return new List<LeaderboardEntry>();
        return _leaderboard.GetRange(0, Mathf.Min(topCount, _leaderboard.Count));
    }

    /// <summary>
    /// Refresh leaderboard from Firebase then invoke onDone. Call from LeaderboardPanel when opening.
    /// </summary>
    public void RefreshLeaderboardFromBackend(Action onDone)
    {
        if (FirebaseBackend.Instance == null || !FirebaseBackend.Instance.IsReady)
        {
            onDone?.Invoke();
            return;
        }
        FirebaseBackend.Instance.GetLeaderboard(50, list =>
        {
            if (list != null && list.Count > 0)
            {
                _leaderboard = list;
                SortLeaderboard();
            }
            onDone?.Invoke();
        });
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

    // ==================== PRINCIPAL ACCESS (bypass code only) ====================

    /// <summary>True if the given code is the principal bypass code.</summary>
    public bool IsValidPrincipalCode(string code)
    {
        return string.Equals((code ?? "").Trim(), "BYp4$s", System.StringComparison.Ordinal);
    }

    // ==================== PRINCIPAL: TEACHER LIST (firstName:randomCode) ====================
    private const string TEACHERS_PREF_KEY = "Cogniville_Teachers";
    private const char TEACHERS_SEPARATOR = '|';
    private const char TEACHER_PASS_SEP = ':';
    private const int TEACHER_CODE_LENGTH = 6;
    private static readonly char[] CODE_CHARS = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    /// <summary>
    /// Add a teacher by first name; generates a random code. Returns the code to give to the teacher.
    /// Principal-only (call after principal login).
    /// </summary>
    public string AddTeacher(string teacherFirstName)
    {
        return AddTeacher(teacherFirstName, null);
    }

    /// <summary>
    /// Add a teacher (second arg ignored; use single-arg overload). Kept for backward compatibility.
    /// </summary>
    public string AddTeacher(string teacherFirstName, string ignoredSecondArg)
    {
        if (string.IsNullOrWhiteSpace(teacherFirstName)) return null;
        var list = GetTeacherEntries();
        var name = teacherFirstName.Trim();
        if (list.Exists(e => GetTeacherNameFromEntry(e).Equals(name, System.StringComparison.OrdinalIgnoreCase)))
            return null;
        var code = GenerateTeacherCode();
        list.Add(name + TEACHER_PASS_SEP + code);
        SaveTeacherEntries(list);
        Debug.Log($"[GameDataManager] Teacher added: {name} Code: {code}");
        return code;
    }

    private static string GenerateTeacherCode()
    {
        var s = new char[TEACHER_CODE_LENGTH];
        for (int i = 0; i < TEACHER_CODE_LENGTH; i++)
            s[i] = CODE_CHARS[UnityEngine.Random.Range(0, CODE_CHARS.Length)];
        return new string(s);
    }

    /// <summary>
    /// Remove a teacher by name.
    /// </summary>
    public void RemoveTeacher(string teacherName)
    {
        if (string.IsNullOrWhiteSpace(teacherName)) return;
        var list = GetTeacherEntries();
        var name = teacherName.Trim();
        list.RemoveAll(e => GetTeacherNameFromEntry(e).Equals(name, System.StringComparison.OrdinalIgnoreCase));
        SaveTeacherEntries(list);
    }

    /// <summary>
    /// Get teacher names only (for display in principal list).
    /// </summary>
    public List<string> GetTeachers()
    {
        var entries = GetTeacherEntries();
        var names = new List<string>();
        foreach (var e in entries)
            names.Add(GetTeacherNameFromEntry(e));
        return names;
    }

    /// <summary>
    /// Get teacher entries as (displayName, code) for principal list display.
    /// </summary>
    public List<(string name, string code)> GetTeachersWithCodes()
    {
        var entries = GetTeacherEntries();
        var result = new List<(string, string)>();
        foreach (var e in entries)
            result.Add((GetTeacherNameFromEntry(e), GetTeacherPasswordFromEntry(e)));
        return result;
    }

    private List<string> GetTeacherEntries()
    {
        var raw = PlayerPrefs.GetString(TEACHERS_PREF_KEY, "");
        if (string.IsNullOrEmpty(raw)) return new List<string>();
        var list = new List<string>();
        foreach (var s in raw.Split(TEACHERS_SEPARATOR))
        {
            var t = s.Trim();
            if (!string.IsNullOrEmpty(t)) list.Add(t);
        }
        return list;
    }

    private static string GetTeacherNameFromEntry(string entry)
    {
        if (string.IsNullOrEmpty(entry)) return "";
        var i = entry.IndexOf(TEACHER_PASS_SEP);
        return i >= 0 ? entry.Substring(0, i).Trim() : entry.Trim();
    }

    private static string GetTeacherPasswordFromEntry(string entry)
    {
        if (string.IsNullOrEmpty(entry)) return "";
        var i = entry.IndexOf(TEACHER_PASS_SEP);
        return i >= 0 ? entry.Substring(i + 1).Trim() : "";
    }

    private void SaveTeacherEntries(List<string> list)
    {
        PlayerPrefs.SetString(TEACHERS_PREF_KEY, string.Join(TEACHERS_SEPARATOR.ToString(), list));
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Check if teacher first name + code are allowed. Code is the one generated when principal added the teacher.
    /// </summary>
    public bool IsAllowedTeacher(string teacherFirstName, string code = null)
    {
        var list = GetTeacherEntries();
        if (list.Count == 0) return false;
        var name = (teacherFirstName ?? "").Trim();
        var enteredCode = (code ?? "").Trim();
        foreach (var entry in list)
        {
            var entryName = GetTeacherNameFromEntry(entry);
            if (!entryName.Equals(name, System.StringComparison.OrdinalIgnoreCase)) continue;
            var storedCode = GetTeacherPasswordFromEntry(entry);
            return storedCode == enteredCode;
        }
        return false;
    }
}
