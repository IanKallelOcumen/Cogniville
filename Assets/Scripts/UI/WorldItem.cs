using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement; // <--- REQUIRED FOR LEVEL LOADING

/// <summary>
/// Attach this to each World Prefab.
/// Features: Floating, Rainbow Outline, Highlight, Bump Effects, and Scene Loading.
/// </summary>
public class WorldItem : MonoBehaviour
{
    [Header("Data")]
    public string worldName = "Ice World";
    public string sceneToLoad = "IceLevel1";
    public string progress = "0/15";

    [Header("Locking")]
    public bool startUnlocked = false;
    public string worldSaveID;
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    
    [Header("Visual References")]
    public SpriteRenderer worldSpriteRenderer; 
    public SpriteRenderer outlineSprite;       
    public GameObject worldCanvas;
    
    public TextMeshProUGUI worldNameText;
    public TextMeshProUGUI progressText;
    public Button playButton;

    [Header("Selection Highlight")]
    public Color selectedColor = Color.white;        
    public Color unselectedColor = new Color(0.6f, 0.6f, 0.6f, 1f); 

    [Header("Rainbow Outline FX")]
    public float outlineHueSpeed = 0.5f;
    [Range(0f, 1f)] public float outlineSaturation = 1f;
    [Range(0f, 1f)] public float outlineValue = 1f;

    [Header("Floating FX")]
    public float floatAmplitude = 0.03f;
    public float floatSpeed = 0.8f;
    public float floatRotAmplitude = 0.5f;
    public float floatRotSpeed = 0.5f;

    [Header("Animations")]
    public Vector3 selectedScale = new Vector3(1.2f, 1.2f, 1.2f);
    public float scaleDuration = 0.2f;
    public float shakeDuration = 1.5f;
    public float maxShakeAmount = 0.2f; 
    public float popDuration = 0.3f;
    
    // Internal State
    private Vector3 _deselectedScale = Vector3.one;
    private bool _isLocked = true;
    private bool _isSelected = false;
    private Vector3 _basePos;
    private float _randomPhase;

    void Awake()
    {
        if (worldSpriteRenderer == null)
            worldSpriteRenderer = GetComponent<SpriteRenderer>();
        
        _basePos = transform.localPosition; 
        _randomPhase = Random.value * Mathf.PI * 2f; 

        // --- SCENE LOAD FIX: Setup Button ---
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(LoadWorldScene);
        }

        if (startUnlocked)
        {
            _isLocked = false;
            // Ensure this static method exists in your project, otherwise comment it out
            GameProgressManager.SaveBookUnlockState(worldSaveID, true);
        }
        else
        {
            // Ensure this static method exists in your project
            _isLocked = !GameProgressManager.IsBookUnlocked(worldSaveID);
        }
    }

    void OnEnable()
    {
        bool isNowUnlocked = GameProgressManager.IsBookUnlocked(worldSaveID);
        if (_isLocked && isNowUnlocked) 
        {
            StartCoroutine(UnlockRoutine());
        }
        else
        {
            UpdateVisuals();
        }
    }

    void Update()
    {
        // 1. Floating (Applied to local position relative to slider parent)
        float t = Time.time;
        float yOffset = Mathf.Sin(t * floatSpeed + _randomPhase) * floatAmplitude;
        float zRot = Mathf.Sin(t * floatRotSpeed + _randomPhase * 0.8f) * floatRotAmplitude;

        transform.localPosition = new Vector3(_basePos.x, _basePos.y + yOffset, _basePos.z);
        transform.localEulerAngles = new Vector3(0, 0, zRot);

        // 2. Rainbow Outline
        if (_isSelected && !_isLocked && outlineSprite != null)
        {
            float hue = (Time.unscaledTime * outlineHueSpeed) % 1f; 
            Color rainbow = Color.HSVToRGB(hue, outlineSaturation, outlineValue);
            outlineSprite.enabled = true;
            outlineSprite.color = rainbow;
        }
        else if (outlineSprite != null)
        {
            outlineSprite.enabled = false;
        }
    }

    public void Initialize(bool isSelected)
    {
        _isSelected = isSelected;
        UpdateVisuals();
        transform.localScale = isSelected ? selectedScale : _deselectedScale;
    }

    public void Select()
    {
        _isSelected = true;
        UpdateVisuals();
        StopAllCoroutines();
        StartCoroutine(AnimationHelper.ScaleTransform(transform, transform.localScale, selectedScale, scaleDuration, true));
    }

    public void Deselect()
    {
        _isSelected = false;
        UpdateVisuals();
        StopAllCoroutines();
        StartCoroutine(AnimationHelper.ScaleTransform(transform, transform.localScale, _deselectedScale, scaleDuration, true));
    }

    public void Bump()
    {
        if (_isSelected && !_isLocked)
        {
            StopAllCoroutines();
            StartCoroutine(AnimationHelper.BumpAnimation(transform, 0.5f, 0.3f, 0f, true));
        }
    }

    // --- SCENE LOADING FUNCTION ---
    public void LoadWorldScene()
    {
        if (_isLocked) return;

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"Loading scene: {sceneToLoad}");
            SceneFader.FadeToScene(sceneToLoad);
        }
        else
        {
            Debug.LogError($"Scene name is empty on WorldItem: {worldName}");
        }
    }

    public void UpdateVisuals()
    {
        if (worldNameText) worldNameText.text = worldName;

        if (_isLocked)
        {
            worldSpriteRenderer.color = lockedColor;
            if (outlineSprite) outlineSprite.enabled = false;
            if (worldCanvas) worldCanvas.SetActive(_isSelected);
            if (playButton) playButton.gameObject.SetActive(false);
            if (progressText) progressText.text = "LOCKED";
        }
        else
        {
            worldSpriteRenderer.color = _isSelected ? selectedColor : unselectedColor;
            if (worldCanvas) worldCanvas.SetActive(_isSelected);
            if (playButton) playButton.gameObject.SetActive(true);
            if (progressText) progressText.text = progress;
        }
    }

    // --- COROUTINES (Moved to AnimationHelper) ---

    IEnumerator UnlockRoutine()
    {
        _isLocked = false;
        Vector3 shakeBasePos = transform.localPosition;

        // 1. Shake Phase
        yield return StartCoroutine(AnimationHelper.ShakeTransform(transform, shakeBasePos, shakeDuration, maxShakeAmount, true));
        
        // 2. Update visuals
        worldSpriteRenderer.color = Color.Lerp(lockedColor, unselectedColor, 1f);
        UpdateVisuals();

        // 3. Pop animation
        float popScale = _deselectedScale.x * 1.3f;
        yield return StartCoroutine(AnimationHelper.PopAnimation(transform, _deselectedScale.x, popScale, popDuration, true));
        
        transform.localScale = selectedScale;
        Select();
    }
}
