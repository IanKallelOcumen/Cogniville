using UnityEngine;
using TMPro;

/// <summary>
/// Auto-setup for input fields to ensure they're editable
/// Attach to panels with input fields (PanelLogin, PanelName)
/// </summary>
public class InputFieldSetup : MonoBehaviour
{
    [Header("Auto-detect Input Fields")]
    public bool autoDetect = true;
    
    [Header("Manual Assignment (optional)")]
    public TMP_InputField[] inputFields;

    void Awake()
    {
        if (autoDetect)
        {
            // Get all input fields in this panel
            inputFields = GetComponentsInChildren<TMP_InputField>(true);
        }

        if (inputFields == null || inputFields.Length == 0)
        {
            Debug.LogWarning($"[InputFieldSetup] No input fields found on {gameObject.name}");
            return;
        }

        // Configure each input field
        foreach (var input in inputFields)
        {
            if (input == null) continue;

            // Ensure input is interactable
            input.interactable = true;
            
            // Match UI: large, readable font size and colors as rest of menu
            if (input.textComponent != null)
            {
                input.textComponent.raycastTarget = true;
                input.textComponent.color = Color.white;
                input.textComponent.fontSize = 32;
            }
            
            if (input.placeholder != null)
            {
                var placeholderText = input.placeholder as TMP_Text;
                if (placeholderText == null) placeholderText = input.placeholder.GetComponent<TMP_Text>();
                if (placeholderText != null)
                {
                    placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    placeholderText.fontSize = 32;
                }
            }

            // Enable rich text support
            input.richText = false;
            
            // Set character limit if not set
            if (input.characterLimit == 0)
            {
                input.characterLimit = 100; // Default limit
            }

            Debug.Log($"[InputFieldSetup] Configured input field: {input.name} on {gameObject.name}");
        }
    }

    /// <summary>
    /// Get text from specific input field by name
    /// </summary>
    public string GetInputText(string fieldName)
    {
        foreach (var input in inputFields)
        {
            if (input != null && input.name.Contains(fieldName))
            {
                return input.text;
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// Clear all input fields
    /// </summary>
    public void ClearAll()
    {
        foreach (var input in inputFields)
        {
            if (input != null) input.text = string.Empty;
        }
    }

    /// <summary>
    /// Focus on first input field
    /// </summary>
    public void FocusFirstField()
    {
        if (inputFields != null && inputFields.Length > 0 && inputFields[0] != null)
        {
            inputFields[0].Select();
            inputFields[0].ActivateInputField();
        }
    }
}
