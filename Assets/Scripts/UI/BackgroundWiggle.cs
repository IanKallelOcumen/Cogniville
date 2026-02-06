using UnityEngine;

/// <summary>
/// A Universal Background script. 
/// Can handle simple Wiggling (Menu) OR Continuous Spinning (Spiral),
/// and reacts to "Kick" events from buttons.
/// Works on both UI (Canvas) and World Sprites.
/// </summary>
public class BackgroundWiggle : MonoBehaviour
{
    [Header("Mode")]
    [Tooltip("If true, it spins forever (Like your Spiral). If false, it just wiggles (Like your Menu).")]
    public bool enableContinuousRotation = false;
    [Tooltip("How fast it spins (Degrees per second). Only used if above is checked.")]
    public float rotationSpeed = 10f;

    [Header("Wiggle Settings (Idle)")]
    // Set these to 0 if you want the Spiral to ONLY spin and not wiggle
    public Vector2 posAmplitude = new Vector2(5f, 5f);
    public float rotAmplitude = 1.0f; 
    public float scaleAmplitude = 0.01f;
    
    [Header("Speeds")]
    public float motionSpeed = 0.1f;

    [Header("Kick Reactions")]
    public float kickDuration = 0.3f;
    public float kickPower = 10f; // How far it moves when kicked

    // Internal state
    private RectTransform _rt;
    private Transform _t;
    private Vector3 _basePos;
    private Vector3 _baseScale;
    private float _currentRotationZ;
    private float _kickTimer;
    
    // Random seeds for noise
    private float _seedX, _seedY, _seedR;

    void Awake()
    {
        // Detect if this is UI (RectTransform) or World Object (Transform)
        _rt = GetComponent<RectTransform>();
        _t = transform;

        // Remember where we started
        if (_rt != null) _basePos = _rt.anchoredPosition;
        else             _basePos = _t.localPosition;

        _baseScale = _t.localScale;
        _currentRotationZ = _t.localEulerAngles.z;

        // Randomize noise
        _seedX = Random.value * 100f;
        _seedY = Random.value * 100f;
        _seedR = Random.value * 100f;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        float time = Time.time;

        // 1. Continuous Rotation (For the Spiral)
        if (enableContinuousRotation)
        {
            _currentRotationZ -= rotationSpeed * dt; // Subtract to rotate clockwise, Add for counter-clockwise
            _currentRotationZ %= 360f;
        }

        // 2. Calculate Wiggle (Idle Motion)
        float wiggleX = (Mathf.PerlinNoise(_seedX + time * motionSpeed, 0) - 0.5f) * posAmplitude.x;
        float wiggleY = (Mathf.PerlinNoise(_seedY + time * motionSpeed, 0) - 0.5f) * posAmplitude.y;
        float wiggleR = (Mathf.PerlinNoise(_seedR + time * motionSpeed, 0) - 0.5f) * rotAmplitude;
        
        // 3. Calculate Kick (Juice)
        float kickOffset = 0f;
        if (_kickTimer > 0)
        {
            _kickTimer -= dt;
            float p = _kickTimer / kickDuration; // 1.0 to 0.0
            float curve = p * p; // Quadratic ease out
            kickOffset = curve * kickPower;
        }

        // 4. Apply to Object
        // Apply Position (Base + Wiggle + Kick)
        Vector3 finalPos = _basePos;
        finalPos.x += wiggleX;
        finalPos.y += wiggleY + kickOffset; // Kick usually bumps Y up

        if (_rt != null) _rt.anchoredPosition = finalPos;
        else             _t.localPosition = finalPos;

        // Apply Rotation (Continuous + Wiggle)
        _t.localEulerAngles = new Vector3(0, 0, _currentRotationZ + wiggleR);

        // Apply Scale (Breathing + Kick)
        float kickScale = (_kickTimer > 0) ? 0.05f * (_kickTimer / kickDuration) : 0f;
        _t.localScale = _baseScale + (Vector3.one * (Mathf.Sin(time) * scaleAmplitude + kickScale));
    }

    // Call this from your buttons!
    public void Kick(float power)
    {
        _kickTimer = kickDuration;
        // Optional: override default power with specific event power
        // kickPower = power; 
    }
}