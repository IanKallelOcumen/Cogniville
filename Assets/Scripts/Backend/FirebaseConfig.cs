using UnityEngine;

/// <summary>
/// Firebase project config for Spark (free) plan.
/// Create a project at https://console.firebase.google.com, enable Auth (Anonymous) and Firestore.
/// </summary>
[CreateAssetMenu(fileName = "FirebaseConfig", menuName = "Cogniville/Firebase Config")]
public class FirebaseConfig : ScriptableObject
{
    [Header("Firebase Project (Spark plan)")]
    [Tooltip("From Firebase Console > Project Settings > General > Your apps > Web API Key")]
    public string apiKey = "";
    [Tooltip("From Firebase Console > Project Settings > General > Project ID")]
    public string projectId = "";

    [Header("Optional: leave empty to disable backend")]
    [Tooltip("If false or credentials empty, game uses local-only (PlayerPrefs)")]
    public bool useFirebase = true;

    public bool IsValid => useFirebase && !string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(projectId);
}
