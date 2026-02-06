using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Attach this to a UI Panel or Button to make it fade in and slide up when the scene starts.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class UIFadeIn : MonoBehaviour
{
    [Header("Animation Settings")]
    public float delay = 0.1f;          // Wait a tiny bit before starting
    public float duration = 0.6f;       // How long the fade takes
    public bool slideUp = true;         // Should it slide up?
    public float slideDistance = 50f;   // How far it slides

    private CanvasGroup _cg;
    private RectTransform _rt;
    private Vector2 _finalPos;

    void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        _rt = GetComponent<RectTransform>();

        // 1. Hide immediately
        _cg.alpha = 0f;

        // 2. Setup Slide Start Position
        if (slideUp && _rt != null)
        {
            _finalPos = _rt.anchoredPosition;
            _rt.anchoredPosition = new Vector2(_finalPos.x, _finalPos.y - slideDistance);
        }
    }

    IEnumerator Start()
    {
        if (delay > 0) yield return new WaitForSeconds(delay);

        float t = 0f;
        Vector2 startPos = (slideUp && _rt != null) ? _rt.anchoredPosition : Vector2.zero;

        while (t < duration)
        {
            t += Time.deltaTime;
            float u = t / duration;
            float eased = 1f - Mathf.Pow(1f - u, 3); // Cubic Ease Out

            // Fade Alpha
            _cg.alpha = Mathf.Lerp(0f, 1f, eased);

            // Slide Position
            if (slideUp && _rt != null)
            {
                _rt.anchoredPosition = Vector2.Lerp(startPos, _finalPos, eased);
            }

            yield return null;
        }

        // Snap to final values
        _cg.alpha = 1f;
        if (slideUp && _rt != null) _rt.anchoredPosition = _finalPos;
    }
}