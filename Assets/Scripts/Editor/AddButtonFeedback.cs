using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Adds ButtonFeedback component to all existing buttons in screens
/// </summary>
public class AddButtonFeedback : MonoBehaviour
{
    [MenuItem("Tools/Cogniville/Add Feedback to Buttons")]
    public static void AddFeedbackToAllButtons()
    {
        // Find all Button components in the scene
        Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        
        int addedCount = 0;
        foreach (Button btn in allButtons)
        {
            // Skip if already has ButtonFeedback
            if (btn.GetComponent<ButtonFeedback>() != null)
                continue;

            // Add ButtonFeedback component
            ButtonFeedback feedback = btn.gameObject.AddComponent<ButtonFeedback>();
            feedback.enableSquish = true;
            feedback.scaleOnPress = 1f;
            
            addedCount++;
            Debug.Log($"Added ButtonFeedback to button: {btn.gameObject.name}");
        }

        EditorUtility.DisplayDialog("Success!", $"Added ButtonFeedback to {addedCount} button(s)!", "OK");
    }
}
