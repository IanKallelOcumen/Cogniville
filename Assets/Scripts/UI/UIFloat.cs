using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class UIFloat : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Idle Float")]
    public float amplitudeY = 10f;
    public float speedY = 0.6f;
    [Tooltip("Rotational amplitude (degrees)")]
    public float rotAmplitude = 1.5f;
    public float rotSpeed = 0.35f;
    [Tooltip("Scale factor amplitude (e.g., 0.01 means scale fluctuates by +/- 1%)")]
    public float scaleAmplitude = 0.01f;
    public float scaleSpeed = 0.5f;
    public bool useUnscaledTime = true;

    [Header("Bump on Press (Optional)")]
    public bool enableBump = false;
    public float bumpHeight = 12f;
    public float bumpOutTime = 0.10f;
    public float bumpBackTime = 0.22f;
    public float bumpRotate = 2.0f;

    [Header("Hover (Optional)")]
    public bool enableHover = true;
    [Tooltip("The scale to pop to on hover (e.g., 1.1)")]
    public float hoverScaleAmount = 1.1f;
    [Tooltip("The rotation to tilt to on hover (e.g., 5)")]
    public float hoverRotAmount = 5f;
    [Tooltip("How fast to animate the hover (10 = 0.1s)")]
    public float hoverAnimSpeed = 10f;

    RectTransform _rt;
    Vector2 _basePos;
    Vector3 _baseScale;
    float _phase; 
    float _extraYOffset;
    float _extraZRot;
    bool _isBumping;

    bool _isHovering = false;
    float _currentHoverScaleOffset = 1f;
    float _currentHoverRotOffset = 0f;
    
    // --- We need a flag to ensure base values are only set once ---
    private bool _hasSetBaseValues = false;
    private Vector2 _trueBasePos;
    private Vector3 _trueBaseScale;


    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _phase = Random.value * Mathf.PI * 2f; 
        
        // --- Store the *true* original values only once ---
        if (!_hasSetBaseValues)
        {
            _trueBasePos = _rt.anchoredPosition;
            _trueBaseScale = _rt.localScale;
            _hasSetBaseValues = true;
        }
    }

    // --- NEW: OnEnable() ---
    // This runs EVERY time the object is set active
    void OnEnable()
    {
        // --- FIX: Reset all values to their *true* base state ---
        // This stops the scale from corrupting on the second click.
        _isHovering = false;
        _isBumping = false;

        _basePos = _trueBasePos;
        _baseScale = _trueBaseScale;
        
        _currentHoverScaleOffset = 1f;
        _currentHoverRotOffset = 0f;
        _extraYOffset = 0f;
        _extraZRot = 0f;

        // Apply the reset values immediately
        _rt.anchoredPosition = _basePos;
        _rt.localScale = _baseScale;
        _rt.localEulerAngles = new Vector3(0, 0, 0);
    }
    // --- END NEW ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (enableHover && enabled)
            _isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (enableHover && enabled)
            _isHovering = false;
    }

    void Update()
    {
        if (!enabled) return;

        float t = useUnscaledTime ? Time.unscaledTime : Time.time;
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // Calculate float components (idle)
        float phaseT = (t * Mathf.PI * 2f);
        float y = Mathf.Sin((phaseT * speedY) + _phase) * amplitudeY;
        float z = Mathf.Sin((phaseT * rotSpeed) + _phase * 0.5f) * rotAmplitude;
        float scl = 1f + Mathf.Sin((phaseT * scaleSpeed) + _phase * 0.25f) * scaleAmplitude;

        // Calculate Hover
        float targetHoverScale = _isHovering ? hoverScaleAmount : 1f;
        float targetHoverRot = _isHovering ? hoverRotAmount : 0f;

        float lerpSpeed = dt * hoverAnimSpeed;
        _currentHoverScaleOffset = Mathf.Lerp(_currentHoverScaleOffset, targetHoverScale, lerpSpeed);
        _currentHoverRotOffset = Mathf.Lerp(_currentHoverRotOffset, targetHoverRot, lerpSpeed);

        // Apply all offsets together
        _rt.anchoredPosition = _basePos + new Vector2(0f, y + _extraYOffset);
        _rt.localEulerAngles = new Vector3(0f, 0f, z + _extraZRot + _currentHoverRotOffset);
        _rt.localScale = _baseScale * scl * _currentHoverScaleOffset;

        // Decay rotation if not currently bumping
        if (!_isBumping && Mathf.Abs(_extraZRot) > 0.001f)
            _extraZRot = Mathf.MoveTowards(_extraZRot, 0f, dt * 90f);
    }

    public void TriggerBump()
    {
        if (enableBump && gameObject.activeInHierarchy && enabled)
            StartCoroutine(AnimationHelper.BumpAnimation(transform, bumpHeight, bumpOutTime + bumpBackTime, bumpRotate, useUnscaledTime));
    }
}

