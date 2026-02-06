using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class BookSelector : MonoBehaviour, IPointerClickHandler
{
	[Header("References")]
	public Image bookImage;
    [Tooltip("Add an 'Outline' component to your Book Image and drag it here.")]
	public Outline bookOutline;

    [Tooltip("This should be the parent/root RectTransform (for the zoom)")]
	public RectTransform bookRect;
    [Tooltip("This should be the child Image's RectTransform (for the animation)")]
    public RectTransform bookImageRect;
	
	[Header("Sprites")]
	public Sprite closedSprite;
	public Sprite openSprite;
	
    // --- UPDATED: Simplified Locking ---
    [Header("Locking")]
    [Tooltip("Is this book unlocked from the start? (Set for 'Math')")]
    public bool startUnlocked = false;
    [Tooltip("A unique ID for this book, e.g., 'Book_English'")]
    public string bookSaveID;
    [Tooltip("The color the book is tinted when locked.")]
    public Color lockedColor = Color.gray;
    [Header("Unlock Animation")]
    public float shakeDuration = 1.5f;
    public float maxShakeAmount = 8f; // Increased shake pixels
    public float popDuration = 0.3f;
    // --- END UPDATED ---

	[Header("Stylish Outline FX")]
	public float outlineHueSpeed = 0.5f;
    [Range(0f, 1f)]
    public float outlineSaturation = 1f;
    [Range(0f, 1f)]
    public float outlineValue = 1f;
    [Space]
    public float minOutlineDistance = 2f;
    public float maxOutlineDistance = 4f;
    public float outlineDistanceSpeed = 1.5f;
    
    [Header("Idle Animation (Book)")]
    public float idleFloatAmplitude = 10f;
    public float idleFloatSpeed = 0.6f;
    public float idleRotAmplitude = 1.5f;
    public float idleRotSpeed = 0.35f;
    public float idleScaleAmplitude = 0.01f;
    public float idleScaleSpeed = 0.5f;

    [Header("Selected Animation (Book)")]
    public float selectedFloatAmplitude = 2f;
    public float selectedFloatSpeed = 0.5f;
    public float selectedRotAmplitude = 1f;
    public float selectedRotSpeed = 0.2f;
    public float selectedScaleAmplitude = 0.02f;
    public float selectedScaleSpeed = 0.25f;

	[Header("Scene References (Optional)")]
    public string sceneToLoad = "YourSceneNameHere"; 
	public MainMenuController controller;
	
	// Internal Components
	private Button _confirmButton; 
	private bool _isSelected = false;
    private bool _isLocked = true; 
	private CanvasGroup _selfCanvasGroup;
	private Coroutine _fadeCo;
    
    private Vector2 _baseImagePos; 
    private Vector3 _baseImageRot;
    private Vector3 _baseImageScale; 
    private float _bookAnimPhase; 

	void Awake()
	{
		if (!bookRect) bookRect = GetComponent<RectTransform>();

		if (!bookImage) bookImage = GetComponentInChildren<Image>();
        if (bookImage)
        {
            if(bookImageRect == null) bookImageRect = bookImage.GetComponent<RectTransform>();
        }

        if(bookImageRect)
        {
            _baseImagePos = bookImageRect.anchoredPosition;
            _baseImageRot = bookImageRect.localEulerAngles;
            _baseImageScale = bookImageRect.localScale; 
            Debug.Log($"BookSelector: Base scale for {bookImageRect.name} saved as {_baseImageScale}", this);
        }
        else
        {
            Debug.LogError("BookSelector is missing its BookImageRect! Animations will fail.", this);
        }

		_confirmButton = GetComponent<Button>();
		_selfCanvasGroup = GetComponent<CanvasGroup>();
        if (bookImage) bookImage.preserveAspect = true;
        
        _bookAnimPhase = Random.value * Mathf.PI * 2f;

		if (bookImage && closedSprite) bookImage.sprite = closedSprite;
		
        // --- Check Lock State ---
        if (startUnlocked)
        {
            _isLocked = false;
            GameProgressManager.SaveBookUnlockState(bookSaveID, true); // Ensure it's saved
        }
        else
        {
            _isLocked = !GameProgressManager.IsBookUnlocked(bookSaveID);
        }

        // --- Removed lockCanvasGroup logic ---

		DeselectBook(true); 
		if (!controller) 
		{
            #if UNITY_2023_1_OR_NEWER
			controller = FindAnyObjectByType<MainMenuController>(FindObjectsInactive.Include);
            #else
			controller = FindObjectOfType<MainMenuController>();
            #endif
		}
		EnableSelectionConfirm(false); 

        UpdateVisuals(); // Set initial look
	}
	
    void OnEnable()
    {
        bool isNowUnlocked = GameProgressManager.IsBookUnlocked(bookSaveID);
        if (_isLocked && isNowUnlocked) // It *was* locked, but *now* it's unlocked
        {
            // Play the unlock animation!
            StartCoroutine(UnlockRoutine());
        }
        else
        {
            // Just update visuals normally
            UpdateVisuals();
        }
    }

    // --- UPDATED: Simplified Visuals ---
    void UpdateVisuals()
    {
        if (_isLocked)
        {
            bookImage.color = lockedColor; // Set to dim color
            if (bookOutline) bookOutline.enabled = false;
        }
        else
        {
            bookImage.color = Color.white; // Set to full color
            if (bookOutline) bookOutline.enabled = _isSelected;
        }
    }
    // --- END UPDATED ---
	
	void Update()
	{
        float t = Time.unscaledTime;

		if (_isSelected)
		{
            if (bookOutline && !_isLocked) // Only show outline if unlocked
            {
                // Gradient Animation
                float hue = (t * outlineHueSpeed) % 1f; 
                bookOutline.effectColor = Color.HSVToRGB(hue, outlineSaturation, outlineValue);

                // Throb Animation
                float distSin = (Mathf.Sin(t * outlineDistanceSpeed) * 0.5f) + 0.5f; 
                float distEase = Easing.CubicEaseOut(distSin); 
                float distance = Mathf.Lerp(minOutlineDistance, maxOutlineDistance, distEase);
                bookOutline.effectDistance = new Vector2(distance, -distance);

                bookOutline.enabled = true;
            }

            // Animate CHILD IMAGE
            if (bookImageRect)
            {
                float bookY = Mathf.Sin(t * selectedFloatSpeed + _bookAnimPhase) * selectedFloatAmplitude; 
                float bookRot = Mathf.Sin(t * selectedRotSpeed + _bookAnimPhase * 0.8f) * selectedRotAmplitude;
                float bookScale = 1f + (Mathf.Sin(t * selectedScaleSpeed + _bookAnimPhase * 0.6f) * selectedScaleAmplitude);
                
                bookImageRect.anchoredPosition = _baseImagePos + new Vector2(0, bookY);
                bookImageRect.localEulerAngles = _baseImageRot + new Vector3(0, 0, bookRot);
                bookImageRect.localScale = _baseImageScale * bookScale; 
            }
		}
        else
        {
            // --- IDLE STATE ANIMATION ---
            if (bookOutline) bookOutline.enabled = false;

            if (bookImageRect)
            {
                float bookY = Mathf.Sin(t * idleFloatSpeed + _bookAnimPhase) * idleFloatAmplitude;
                float bookRot = Mathf.Sin(t * idleRotSpeed + _bookAnimPhase * 0.8f) * idleRotAmplitude;
                float bookScale = 1f + (Mathf.Sin(t * idleScaleSpeed + _bookAnimPhase * 0.6f) * idleScaleAmplitude);

                bookImageRect.anchoredPosition = _baseImagePos + new Vector2(0, bookY);
                bookImageRect.localEulerAngles = _baseImageRot + new Vector3(0, 0, bookRot);
                bookImageRect.localScale = _baseImageScale * bookScale;
            }
        }
	}

	public void OnPointerClick(PointerEventData eventData)
	{
        if (_isLocked) return; // Don't do anything if locked

        if (controller != null && controller.IsTransitioning()) return;

		if (_selfCanvasGroup.alpha < 0.5f) return;

		if (!_isSelected)
		{
			SelectBook();
			if (controller && bookRect) 
			{
				controller.FocusOnBook(bookRect, this);
			}
		}
        else
        {
            if (controller)
            {
                controller.ResetZoom();
            }
        }
	}

	public void SelectBook()
	{
		if (_isSelected) return;
		_isSelected = true;
		if (bookImage && openSprite) bookImage.sprite = openSprite;

        UpdateVisuals(); // Update visuals on select

        if (bookImageRect)
        {
            bookImageRect.localScale = _baseImageScale;
            Debug.Log($"BookSelector: {bookImageRect.name} scale reset to {_baseImageScale} on SelectBook()", this);
        }
	}

	public void DeselectBook()
	{
		DeselectBook(false);
	}
	
	private void DeselectBook(bool instant)
	{
		_isSelected = false;
		if (bookImage && closedSprite) bookImage.sprite = closedSprite;
		
        UpdateVisuals(); // Update visuals on deselect

        if (bookImageRect)
        {
            bookImageRect.anchoredPosition = _baseImagePos;
            bookImageRect.localEulerAngles = _baseImageRot;
            bookImageRect.localScale = _baseImageScale; 
            Debug.Log($"BookSelector: {bookImageRect.name} scale reset to {_baseImageScale} on DeselectBook()", this);
        }
	}
	
	public void EnableSelectionConfirm(bool enable)
	{
		if (_confirmButton) _confirmButton.interactable = enable;
	}
	
	public void Fade(bool fadeIn, float duration)
	{
		if (_selfCanvasGroup == null) return;
		if (_fadeCo != null) StopCoroutine(_fadeCo);
		_fadeCo = StartCoroutine(AnimationHelper.FadeCanvasGroup(_selfCanvasGroup, fadeIn ? 0f : 1f, fadeIn ? 1f : 0f, duration));
	}

    // --- UPDATED: Simplified Unlock Animation ---
    IEnumerator UnlockRoutine()
    {
        _isLocked = false;

        if (bookOutline) bookOutline.enabled = true;

        // 1. Shake
        yield return StartCoroutine(AnimationHelper.ShakeTransform(bookImageRect, _baseImagePos, shakeDuration, maxShakeAmount, true));

        // 2. Update visuals
        bookImage.color = Color.white;
        UpdateVisuals();

        // 3. Pop animation
        float startScale = _baseImageScale.x;
        float popScale = startScale * 1.2f;
        yield return StartCoroutine(AnimationHelper.PopAnimation(bookImageRect, startScale, popScale, popDuration, true));
    }
}

