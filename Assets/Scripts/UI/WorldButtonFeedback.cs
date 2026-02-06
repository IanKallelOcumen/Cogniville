using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Unified button feedback system (formerly WorldButtonFeedback + ButtonBumpUI).
/// Backwards compatible with existing scene assignments.
/// </summary>
public class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Original Field Names (for backwards compatibility)")]
    [Tooltip("The UIFloat element to bump (ButtonBumpUI: 'target')")]
    public UIFloat target;
    
    [Tooltip("Background to kick (formerly 'bg')")]
    public BackgroundWiggle bg;
    
    [Tooltip("World item (WorldButtonFeedback assignment)")]
    public WorldItem worldItem;

    [Header("Settings")]
    [Tooltip("How hard to kick the background")]
    public float kickAmount = 6f;
    
    [Tooltip("Button scale on press (1.05 = 5% bigger, 1.0 = no change)")]
    public float scaleOnPress = 1.05f;
    
    public bool enableSquish = false;

    private Vector3 _originalScale;
    private Button _button;

    void Awake()
    {
        _button = GetComponent<Button>();
        if (_button) _originalScale = _button.transform.localScale;

        // Auto-find UIFloat if not assigned
        if (target == null)
        {
            target = GetComponentInParent<UIFloat>();
            if (target == null)
            {
#if UNITY_2023_1_OR_NEWER
                target = Object.FindFirstObjectByType<UIFloat>();
#else
                target = FindObjectOfType<UIFloat>();
#endif
            }
        }

        // Auto-find BackgroundWiggle if not assigned
        if (bg == null)
        {
#if UNITY_2023_1_OR_NEWER
            bg = Object.FindFirstObjectByType<BackgroundWiggle>();
#else
            bg = FindObjectOfType<BackgroundWiggle>();
#endif
        }

        // Auto-find WorldItem if not assigned
        if (worldItem == null)
            worldItem = GetComponentInParent<WorldItem>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Apply squish
        if (enableSquish && _button)
            _button.transform.localScale = _originalScale * scaleOnPress;

        // Trigger bump
        if (target != null) 
            target.TriggerBump();

        // Trigger background kick
        if (bg != null)
            bg.Kick(kickAmount);

        // Trigger world item bump
        if (worldItem != null)
            worldItem.Bump();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (enableSquish && _button)
            _button.transform.localScale = _originalScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (enableSquish && _button)
            _button.transform.localScale = _originalScale;
    }
}
