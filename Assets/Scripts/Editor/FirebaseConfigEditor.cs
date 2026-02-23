#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor menu to quickly select Firebase config and open setup docs.
/// </summary>
public static class FirebaseConfigEditor
{
    [MenuItem("Cogniville/Firebase/Select Firebase Config")]
    static void SelectFirebaseConfig()
    {
        var guids = AssetDatabase.FindAssets("FirebaseConfig t:FirebaseConfig");
        if (guids.Length == 0)
            guids = AssetDatabase.FindAssets("FirebaseConfig");
        UnityEngine.Object config = null;
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            if (path.EndsWith(".asset"))
            {
                config = AssetDatabase.LoadAssetAtPath(path, typeof(FirebaseConfig));
                if (config == null) config = AssetDatabase.LoadMainAssetAtPath(path);
                if (config != null) break;
            }
        }
        if (config != null)
        {
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }
        else
            Debug.LogWarning("[Cogniville] FirebaseConfig not found. Use Cogniville > Firebase > Open Firebase Console, then create Config via Create > Cogniville > Firebase Config and save to Assets/Resources/FirebaseConfig.");
    }

    [MenuItem("Cogniville/Firebase/Open Firebase Console")]
    static void OpenFirebaseConsole()
    {
        Application.OpenURL("https://console.firebase.google.com");
    }
}
#endif
