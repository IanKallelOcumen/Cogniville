using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class WorldSliderController : MonoBehaviour
{
    [Header("Scene References")]
    public Transform worldSliderParent;
    public Button nextButton;
    public Button prevButton;
    
    // --- NEW: BACK BUTTON ---
    [Header("Navigation")]
    [Tooltip("Drag your Back Button here")]
    public Button backButton;
    [Tooltip("Exact name of your Menu scene")]
    public string menuSceneName = "MainMenu"; 
    // ------------------------

    [Header("World Data")]
    public WorldItem[] worlds;

    [Header("Animation")]
    public float slideDuration = 0.4f;

    [Header("Juice / Feedback")]
    [Tooltip("Drag your 'spiral_0' object here if needed.")]
    public BackgroundWiggle backgroundWiggle; 
    public float bgKickAmount = 4f;
    
    // Auto-found references
    private UIFloat _titleFloat;
    private int _currentIndex = 0;
    private Coroutine _slideCoroutine;

    void Start()
    {
        // 1. Find Juice Scripts
        if (backgroundWiggle == null)
        {
#if UNITY_2023_1_OR_NEWER
            backgroundWiggle = Object.FindFirstObjectByType<BackgroundWiggle>();
#else
            backgroundWiggle = FindObjectOfType<BackgroundWiggle>();
#endif
        }

#if UNITY_2023_1_OR_NEWER
        _titleFloat = Object.FindFirstObjectByType<UIFloat>();
#else
        _titleFloat = FindObjectOfType<UIFloat>();
#endif

        // 2. Hook up buttons
        nextButton.onClick.AddListener(NextWorld);
        prevButton.onClick.AddListener(PreviousWorld);
        
        // --- Hook up Back Button ---
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBackToMenu);
        }

        // 3. Initialize World Items
        for (int i = 0; i < worlds.Length; i++)
        {
            int index = i; 
            worlds[i].playButton.onClick.AddListener(() => OnPlayPressed(index));
            worlds[i].Initialize(i == _currentIndex);
        }

        SnapToCurrent();
        UpdateNavButtons();
    }

    // --- NEW: Go Back Logic ---
    public void GoBackToMenu()
    {
        if (backgroundWiggle != null) backgroundWiggle.Kick(bgKickAmount);
        
        // Use SceneFader if it exists, otherwise standard load
        if (SceneFader.Instance != null) 
        {
            SceneFader.FadeToScene(menuSceneName);
        }
        else
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }

    public void NextWorld()
    {
        if (_currentIndex < worlds.Length - 1) ChangeWorld(_currentIndex + 1);
    }

    public void PreviousWorld()
    {
        if (_currentIndex > 0) ChangeWorld(_currentIndex - 1);
    }

    void ChangeWorld(int newIndex)
    {
        worlds[_currentIndex].Deselect();
        _currentIndex = newIndex;
        worlds[_currentIndex].Select();
        MoveToCurrent();
        TriggerFeedback();
    }

    void TriggerFeedback()
    {
        if (backgroundWiggle != null) backgroundWiggle.Kick(bgKickAmount);
        if (_titleFloat != null) _titleFloat.TriggerBump();
    }

    void MoveToCurrent()
    {
        Vector3 targetPos = worlds[_currentIndex].transform.localPosition * -1;
        targetPos.y = worldSliderParent.localPosition.y;
        targetPos.z = worldSliderParent.localPosition.z;

        if (_slideCoroutine != null) StopCoroutine(_slideCoroutine);
        _slideCoroutine = StartCoroutine(SlideRoutine(targetPos));
        UpdateNavButtons();
    }
    
    void SnapToCurrent()
    {
        Vector3 targetPos = worlds[_currentIndex].transform.localPosition * -1;
        targetPos.y = worldSliderParent.localPosition.y;
        targetPos.z = worldSliderParent.localPosition.z;
        worldSliderParent.localPosition = targetPos;
    }

    IEnumerator SlideRoutine(Vector3 targetPosition)
    {
        float t = 0f;
        Vector3 startPosition = worldSliderParent.localPosition;
        
        while(t < slideDuration)
        {
            float u = t / slideDuration;
            float eased = Easing.CubicEaseOut(u); 
            worldSliderParent.localPosition = Vector3.LerpUnclamped(startPosition, targetPosition, eased);
            t += Time.deltaTime;
            yield return null;
        }
        worldSliderParent.localPosition = targetPosition;
    }

    void UpdateNavButtons()
    {
        prevButton.interactable = (_currentIndex > 0);
        nextButton.interactable = (_currentIndex < worlds.Length - 1);
    }

    void OnPlayPressed(int index)
    {
        // This is where you load the game level
        string sceneToLoad = worlds[index].sceneToLoad;
        Debug.Log("Loading Level: " + sceneToLoad);
        // SceneFader.FadeToScene(sceneToLoad);
    }
}