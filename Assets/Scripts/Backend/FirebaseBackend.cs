using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Firebase backend using REST API (no SDK). Works with Spark (free) plan.
/// Uses Anonymous Auth for a token, then Firestore for leaderboard and sessions.
/// </summary>
public class FirebaseBackend : MonoBehaviour
{
    public static FirebaseBackend Instance { get; private set; }

    [SerializeField] FirebaseConfig config;
    string _idToken;
    bool _authDone;
    bool _authSuccess;

    const string AUTH_SIGNUP = "https://identitytoolkit.googleapis.com/v1/accounts:signUp";
    const string FIRESTORE_BASE = "https://firestore.googleapis.com/v1/projects";
    const string LEADERBOARD_COLLECTION = "leaderboard";
    const string SESSIONS_COLLECTION = "sessions";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (config == null)
            config = Resources.Load<FirebaseConfig>("FirebaseConfig");
        if (config != null && config.IsValid)
            StartCoroutine(AuthAnonymous());
        else
        {
            _authDone = true;
            if (config != null && config.useFirebase && string.IsNullOrWhiteSpace(config.apiKey))
                Debug.Log("[Firebase] Api Key / Project Id empty — using local-only. Add credentials to FirebaseConfig (Resources/FirebaseConfig) to enable cloud.");
        }
    }

    public bool IsReady => _authDone && _authSuccess && !string.IsNullOrEmpty(_idToken);
    public bool UseFirebase => config != null && config.IsValid;

    IEnumerator AuthAnonymous()
    {
        _authDone = false;
        _authSuccess = false;
        var body = "{\"returnSecureToken\":true}";
        var url = AUTH_SIGNUP + "?key=" + config.apiKey;
        using (var req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                var json = req.downloadHandler.text;
                _idToken = JsonUtilityHelper.GetString(json, "idToken");
                _authSuccess = !string.IsNullOrEmpty(_idToken);
                if (_authSuccess) Debug.Log("[Firebase] Anonymous auth OK");
                else Debug.LogWarning("[Firebase] No idToken in response");
            }
            else
                Debug.LogWarning("[Firebase] Auth failed: " + req.error);
            _authDone = true;
        }
    }

    string FirestoreUrl(string path) =>
        $"{FIRESTORE_BASE}/{config.projectId}/databases/(default)/documents/{path}";

    UnityWebRequest Request(string url, string method, string body = null)
    {
        var req = new UnityWebRequest(url, method);
        if (!string.IsNullOrEmpty(body))
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        if (!string.IsNullOrEmpty(_idToken))
            req.SetRequestHeader("Authorization", "Bearer " + _idToken);
        return req;
    }

    /// <summary>Add a leaderboard entry (playerName, score, teacherName).</summary>
    public void AddLeaderboardEntry(string playerName, int score, string teacherName, Action<bool> onDone = null)
    {
        if (!UseFirebase || !IsReady) { onDone?.Invoke(false); return; }
        StartCoroutine(AddLeaderboardEntryRoutine(playerName, score, teacherName, onDone));
    }

    IEnumerator AddLeaderboardEntryRoutine(string playerName, int score, string teacherName, Action<bool> onDone)
    {
        var docId = System.Guid.NewGuid().ToString("N");
        var body = "{\"fields\":{" +
            "\"playerName\":{\"stringValue\":\"" + JsonUtilityHelper.Escape(playerName ?? "") + "\"}," +
            "\"score\":{\"integerValue\":\"" + score + "\"}," +
            "\"teacherName\":{\"stringValue\":\"" + JsonUtilityHelper.Escape(teacherName ?? "") + "\"}," +
            "\"timestamp\":{\"timestampValue\":\"" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") + "\"}" +
            "}}";
        var url = FirestoreUrl(LEADERBOARD_COLLECTION) + "?documentId=" + docId;
        using (var req = Request(url, "POST", body))
        {
            yield return req.SendWebRequest();
            onDone?.Invoke(req.result == UnityWebRequest.Result.Success);
        }
    }

    /// <summary>Fetch top leaderboard entries from Firestore.</summary>
    public void GetLeaderboard(int topCount, Action<List<GameDataManager.LeaderboardEntry>> onDone)
    {
        if (!UseFirebase || !IsReady)
        {
            onDone?.Invoke(new List<GameDataManager.LeaderboardEntry>());
            return;
        }
        StartCoroutine(GetLeaderboardRoutine(topCount, onDone));
    }

    IEnumerator GetLeaderboardRoutine(int topCount, Action<List<GameDataManager.LeaderboardEntry>> onDone)
    {
        var runQueryUrl = FirestoreUrl(":runQuery");
        var queryBody = "{\"structuredQuery\":{" +
            "\"from\":[{\"collectionId\":\"" + LEADERBOARD_COLLECTION + "\"}]," +
            "\"orderBy\":[{\"field\":{\"fieldPath\":\"score\"},\"direction\":\"DESCENDING\"}]," +
            "\"limit\":" + Mathf.Clamp(topCount, 1, 50) + "}}";
        using (var req = Request(runQueryUrl, "POST", queryBody))
        {
            yield return req.SendWebRequest();
            var list = new List<GameDataManager.LeaderboardEntry>();
            if (req.result == UnityWebRequest.Result.Success)
                list = JsonUtilityHelper.ParseLeaderboardQuery(req.downloadHandler.text);
            onDone?.Invoke(list);
        }
    }

    /// <summary>Start or update a teacher session in Firestore.</summary>
    public void SetSessionActive(string teacherName, bool active, Action<bool> onDone = null)
    {
        if (!UseFirebase || !IsReady) { onDone?.Invoke(false); return; }
        StartCoroutine(SetSessionRoutine(teacherName, active, onDone));
    }

    IEnumerator SetSessionRoutine(string teacherName, bool active, Action<bool> onDone)
    {
        var docId = SanitizeDocId(teacherName);
        if (string.IsNullOrEmpty(docId)) docId = "default";
        var path = $"{SESSIONS_COLLECTION}/{docId}";
        var url = FirestoreUrl(path);
        var body = "{\"fields\":{" +
            "\"teacherName\":{\"stringValue\":\"" + JsonUtilityHelper.Escape(teacherName ?? "") + "\"}," +
            "\"isActive\":{\"booleanValue\":" + (active ? "true" : "false") + "}," +
            "\"updatedAt\":{\"timestampValue\":\"" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") + "\"}" +
            "}}";
        using (var req = Request(url, "PATCH", body))
        {
            yield return req.SendWebRequest();
            onDone?.Invoke(req.result == UnityWebRequest.Result.Success);
        }
    }

    /// <summary>Get whether a session is active (from Firestore).</summary>
    public void GetSessionActive(string teacherName, Action<bool> onDone)
    {
        if (!UseFirebase || !IsReady) { onDone?.Invoke(false); return; }
        StartCoroutine(GetSessionRoutine(teacherName, onDone));
    }

    IEnumerator GetSessionRoutine(string teacherName, Action<bool> onDone)
    {
        var docId = SanitizeDocId(teacherName);
        if (string.IsNullOrEmpty(docId)) docId = "default";
        var path = $"{SESSIONS_COLLECTION}/{docId}";
        var url = FirestoreUrl(path);
        using (var req = Request(url, "GET"))
        {
            yield return req.SendWebRequest();
            var active = false;
            if (req.result == UnityWebRequest.Result.Success)
                active = JsonUtilityHelper.GetFirestoreBool(req.downloadHandler.text, "isActive");
            onDone?.Invoke(active);
        }
    }

    static string SanitizeDocId(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        var sb = new System.Text.StringBuilder();
        foreach (var c in s.Trim())
            if (char.IsLetterOrDigit(c) || c == '_' || c == '-') sb.Append(c);
        return sb.ToString();
    }
}
