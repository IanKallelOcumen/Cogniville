using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Legacy alias for ButtonFeedback.
/// Use ButtonFeedback component instead.
/// This exists only for backwards compatibility.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonBumpUI : ButtonFeedback
{
    // This class now inherits all functionality from ButtonFeedback
    // No additional code needed
}
