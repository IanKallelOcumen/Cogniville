using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// DEPRECATED: Use ButtonFeedback component instead with enableSquish = true
/// Kept for backwards compatibility - field names are preserved.
/// </summary>
public class ButtonPressAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("Scale on press (1 = no change, 1.05 = 5% bigger)")]
    public float scaleOnPress = 1f;

    private Vector3 _originalScale;
    private ButtonFeedback _feedback;

    void Awake()
    {
        _originalScale = transform.localScale;

        // Try to find or create ButtonFeedback component
        _feedback = GetComponent<ButtonFeedback>();
        if (!_feedback)
        {
            _feedback = gameObject.AddComponent<ButtonFeedback>();
        }
        
        // Apply this component's settings to the feedback
        _feedback.enableSquish = true;
        _feedback.scaleOnPress = scaleOnPress;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Delegate to ButtonFeedback if it exists
        if (_feedback) _feedback.OnPointerDown(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Delegate to ButtonFeedback if it exists
        if (_feedback) _feedback.OnPointerUp(eventData);
    }
}
