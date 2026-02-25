using UnityEngine;
using UnityEditor;

/// <summary>
/// Imports Firebase Unity SDK .unitypackage files from the FirebaseSDK folder (created when you unzipped the SDK).
/// Run once: Tools > Cogniville > Import Firebase SDK packages.
/// </summary>
public static class ImportFirebaseSdkPackages
{
    static readonly string[] PackagesToImport = new[]
    {
        "FirebaseAnalytics.unitypackage",
        "FirebaseAuth.unitypackage",
        "FirebaseFirestore.unitypackage"
    };

    [MenuItem("Tools/Cogniville/Import Firebase SDK packages")]
    public static void Import()
    {
        string projectRoot = Application.dataPath + "/..";
        string sdkFolder = System.IO.Path.Combine(projectRoot, "FirebaseSDK", "firebase_unity_sdk");
        if (!System.IO.Directory.Exists(sdkFolder))
        {
            EditorUtility.DisplayDialog("Firebase SDK", "FirebaseSDK folder not found.\n\nExpected: " + System.IO.Path.GetFullPath(sdkFolder) + "\n\nUnzip firebase_unity_sdk_13.8.0.zip into the project root (next to Assets).", "OK");
            return;
        }

        int imported = 0;
        foreach (var name in PackagesToImport)
        {
            string path = System.IO.Path.Combine(sdkFolder, name);
            if (System.IO.File.Exists(path))
            {
                AssetDatabase.ImportPackage(path, false);
                imported++;
            }
        }
        EditorUtility.DisplayDialog("Firebase SDK", imported > 0
            ? "Import started for " + imported + " package(s). Check the Editor for any import dialogs."
            : "No package files found in FirebaseSDK/firebase_unity_sdk.", "OK");
    }
}
