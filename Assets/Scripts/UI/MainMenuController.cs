using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelMain;
    public GameObject panelAbout;
    public GameObject panelLeaderboard;
    public GameObject panelBookSelect;
    public GameObject panelLogin;
    public GameObject panelName;
    public GameObject panelSession;
    public GameObject resultsScreen; 

    [Header("Book Zoom Focus")]
    public RectTransform bookPanelRoot; 
    public float focusScale = 1.5f;     
    public float focusDuration = 0.5f;  
    
    [Header("Book Focus UI")]
    public GameObject bookSelectHeaderUI; 
    public GameObject focusModeUI;        
    public Button focusPlayButton;
    public Button focusBackButton;
    // --- NEW ---
    [Tooltip("How long it takes the focus UI to fade in/out.")]
    public float focusUITransitionDuration = 0.2f;
    // --- END NEW ---

    [Header("UI (optional)")]
    public Toggle soundToggle;
    public Button exitButton;
    public AudioSource musicSource;

    [Header("Transitions")]
    public float transitionDuration = 0.25f;

    [Header("Auto-wire by name (optional)")]
    public bool autoWireByName = true;
    public bool debugLogs = true;
    public string namePanelMain = "PanelMain";
    public string namePanelAbout = "PanelAbout";
    public string namePanelLeaderboard = "PanelLeaderboard";
    public string namePanelBookSelect = "PanelBookSelect";
    public string namePanelLogin = "PanelLogin";
    public string namePanelName = "PanelName";
    public string namePanelSession = "PanelSession";
    public string nameResultsScreen = "ResultsScreen"; 
    public string nameBtnPlay = "PlayButton";
    public string nameBtnAbout = "AboutButton";
    public string nameBtnLeaderboard = "LeaderboardButton";
    public string nameBtnExit = "ExitButton";
    public string nameBtnAboutBack = "AboutBackButton";
    public string nameBtnLeaderboardBack = "LeaderboardBackButton";
    public string nameBtnBackGeneric = "BackButton";
    public string nameSoundToggle = "SoundToggle";
    public bool autoBindAnyBackButtons = true; 

    const string PrefKeySound = "Quested_Sound";
    GameObject _current;
    Coroutine _co;
    bool _isZoomed = false;
    BookSelector _selectedBook = null; 
    private BookSelector[] _allBooks; 
    private Vector2 _originalBookPanelPosition = Vector2.zero;
    private Vector3 _originalBookPanelScale = Vector3.one;
    
    private bool _isTransitioning = false;

    void Log(string m){ if (debugLogs) Debug.Log("[MainMenu] " + m); }
    void Err(string m){ Debug.LogError("[MainMenu] " + m); }

    public bool IsTransitioning()
    {
        return _isTransitioning;
    }

    void Awake()
    {
        // ... (Awake is unchanged) ...
		#if UNITY_IOS
        if (exitButton) exitButton.gameObject.SetActive(false);
		#endif
        if (autoWireByName) AutoWire();
        bool soundOn = PlayerPrefs.GetInt(PrefKeySound, 1) == 1;
        ApplySound(soundOn);
        if (soundToggle){ soundToggle.isOn = soundOn; soundToggle.onValueChanged.AddListener(OnSoundToggled); }
        Setup(panelMain, true);
        Setup(panelAbout, false);
        Setup(panelLeaderboard, false);
        Setup(panelBookSelect, false);
        Setup(panelLogin, false);
        Setup(panelName, false);
        Setup(panelSession, false);
        Setup(resultsScreen, false);
        _current = panelMain;
        if (!bookPanelRoot && panelBookSelect) bookPanelRoot = panelBookSelect.GetComponent<RectTransform>();
    }

    void Start()
    {
        SetState(panelMain, true, 1f);
        SetState(panelAbout, false, 0f);
        SetState(panelLeaderboard, false, 0f);
        SetState(panelBookSelect, false, 0f);
        SetState(panelLogin, false, 0f);
        SetState(panelName, false, 0f);
        SetState(panelSession, false, 0f);
        SetState(resultsScreen, false, 0f);
        _current = panelMain;
        
        if (bookPanelRoot)
        {
            _originalBookPanelPosition = bookPanelRoot.anchoredPosition;
            _originalBookPanelScale = bookPanelRoot.localScale;
            _allBooks = bookPanelRoot.GetComponentsInChildren<BookSelector>();
            Log($"Found {_allBooks.Length} books.");
        }

        if (focusModeUI)
        {
            focusModeUI.SetActive(false);
            var cg = focusModeUI.GetComponent<CanvasGroup>();
            if (!cg) cg = focusModeUI.AddComponent<CanvasGroup>(); 
            // --- UPDATED ---
            cg.alpha = 0f; // Start invisible
            // --- END UPDATED ---
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
        if (focusBackButton) focusBackButton.onClick.AddListener(OnBack);
        if (focusPlayButton) focusPlayButton.onClick.AddListener(OnFocusPlay);
    }
    
    public void OnPlay()
    {
        Log("OnPlay -> Fade to PanelName");
        if (!panelName) { Err("panelName is NULL."); return; }
        if (_current == panelName) return;
        StartFade(_current, panelName);
        
        // Focus input field when panel shows
        StartCoroutine(FocusInputFieldDelayed(panelName));
    }
    
    public void OnLogin()
    {
        Log("OnLogin clicked -> Fade to ResultsScreen (dummy)");
        if (!resultsScreen) { Err("resultsScreen is NULL"); return; }
        if (_current == resultsScreen) return;
        StartFade(_current, resultsScreen);
    }
    
    public void OnContinueFromName()
    {
        Log("OnContinueFromName -> Fade to PanelBookSelect");
        if (!panelBookSelect) { Err("panelBookSelect is NULL."); return; }
        if (_current == panelBookSelect) return;
        StartFade(_current, panelBookSelect); 
    }
    public void OnAbout()
    {
        Log("OnAbout clicked");
        if (!panelAbout){ Err("panelAbout is NULL"); return; }
        if (_current == panelAbout) return;
        StartFade(_current, panelAbout);
    }
    
    public void OnSession()
    {
        Log("OnSession clicked");
        if (!panelSession) { Err("panelSession is NULL"); return; }
        if (_current == panelSession) return;
        StartFade(_current, panelSession);
    }
    
    public void OnResults()
    {
        Log("OnResults clicked");
        if (!resultsScreen) { Err("resultsScreen is NULL"); return; }
        if (_current == resultsScreen) return;
        StartFade(_current, resultsScreen);
    }
    public void OnLeaderboard()
    {
        Log("OnLeaderboard clicked");
        if (!panelLeaderboard){ Err("panelLeaderboard is NULL"); return; }
        if (_current == panelLeaderboard) return;
        StartFade(_current, panelLeaderboard);
    }
    public void OnBack()
    {
        Log("OnBack clicked");
        if (_isZoomed && !_isTransitioning) 
        {
            ResetZoom();
            return;
        }

        // Context-aware back button
        if (_current == panelName) { StartFade(_current, panelMain); return; }
        if (_current == panelLogin) { StartFade(_current, panelMain); return; }
        if (_current == panelSession) { StartFade(_current, panelMain); return; }
        if (_current == resultsScreen) { StartFade(_current, panelMain); return; }
        if (_current == panelBookSelect) { StartFade(_current, panelMain); return; }
        if (_current == panelAbout) { StartFade(_current, panelMain); return; }
        if (_current == panelLeaderboard) { StartFade(_current, panelMain); return; }

        if (!panelMain){ Err("panelMain is NULL"); return; }
        if (_current == panelMain) { Log("Already on PanelMain"); return; }
        
        StartFade(_current, panelMain); 
    }
    public void OnExit()
    {
		#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
		#else
        Application.Quit();
		#endif
    }
    public void OnFocusPlay()
    {
        if (_selectedBook != null && !string.IsNullOrEmpty(_selectedBook.sceneToLoad))
        {
            Log("Loading scene: " + _selectedBook.sceneToLoad);
            SceneFader.FadeToScene(_selectedBook.sceneToLoad);
        }
        else { Err("No book selected or 'Scene To Load' is not set!"); }
    }


    // --- ZOOM/FOCUS METHODS ---
    
    public void FocusOnBook(RectTransform bookRect, BookSelector selector)
    {
        if (_isZoomed || _isTransitioning) return;

        if (!bookPanelRoot) { Err("Book Panel Root not assigned for zoom."); return; }

        _isZoomed = true;
        _selectedBook = selector;

        if (_allBooks != null)
        {
            foreach (var book in _allBooks)
            {
                if (book != _selectedBook)
                {
                    book.Fade(false, focusDuration * 0.5f); 
                }
            }
        }
        
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(FocusRoutine(bookRect));
    }

    IEnumerator FocusRoutine(RectTransform bookRect)
    {
        _isTransitioning = true; // We are busy!

        if (bookSelectHeaderUI) bookSelectHeaderUI.SetActive(false);

        Vector3 targetScale = Vector3.one * focusScale;
        Canvas rootCanvas = bookPanelRoot.GetComponentInParent<Canvas>();
        if (rootCanvas == null) { Err("Could not find Canvas parent."); yield break; }
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(bookPanelRoot, bookRect.position, rootCanvas.worldCamera, out Vector2 bookLocalPosition))
        {
             bookLocalPosition = bookPanelRoot.InverseTransformPoint(bookRect.position);
        }
        Vector2 targetPos = -bookLocalPosition * focusScale;
        Vector2 startPos = bookPanelRoot.anchoredPosition;
        Vector3 startScale = bookPanelRoot.localScale;
        
        float t = 0f;
        while (t < focusDuration)
        {
            float u = t / focusDuration;
            float s = Easing.CubicEaseOut(u); // Use consistent easing
            bookPanelRoot.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, s);
            bookPanelRoot.localScale = Vector3.LerpUnclamped(startScale, targetScale, s);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        bookPanelRoot.anchoredPosition = targetPos;
        bookPanelRoot.localScale = targetScale;
        
        // --- UPDATED: Fade in the UI ---
        if (focusModeUI)
        {
            focusModeUI.SetActive(true);
            var cg = focusModeUI.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                float fadeT = 0f;
                float duration = focusUITransitionDuration;
                while (fadeT < duration)
                {
                    fadeT += Time.unscaledDeltaTime;
                    float u = fadeT / duration;
                    cg.alpha = Easing.CubicEaseOut(u); // Lerp from 0 to 1
                    yield return null;
                }
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            else
            {
                Err("FocusModeUI is missing a CanvasGroup component!");
            }
        }
        // --- END UPDATED ---
        
        if (_selectedBook) _selectedBook.EnableSelectionConfirm(true); 

        _isTransitioning = false; // We are finished!
    }

    public void ResetZoom()
    {
        if (!_isZoomed || _isTransitioning) return;
        
        if (_allBooks != null)
        {
            foreach (var book in _allBooks)
            {
                if (book != _selectedBook)
                {
                    book.Fade(true, focusDuration * 0.8f); 
                }
            }
        }

        if (_selectedBook)
        {
            _selectedBook.EnableSelectionConfirm(false); 
            _selectedBook.DeselectBook(); 
        }

        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(ResetRoutine());
    }

    IEnumerator ResetRoutine()
    {
        _isTransitioning = true; // We are busy!
        
        // --- UPDATED: Fade out the UI FIRST ---
        if (focusModeUI)
        {
            var cg = focusModeUI.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                float fadeT = 0f;
                float duration = focusUITransitionDuration;
                float startAlpha = cg.alpha;
                
                cg.interactable = false; // Disable interaction immediately
                cg.blocksRaycasts = false;

                while (fadeT < duration)
                {
                    fadeT += Time.unscaledDeltaTime;
                    float u = fadeT / duration;
                    cg.alpha = Mathf.Lerp(startAlpha, 0f, Easing.CubicEaseOut(u)); // Lerp to 0
                    yield return null;
                }
                cg.alpha = 0f;
                cg.gameObject.SetActive(false);
            }
        }
        // --- END UPDATED ---

        Vector2 startPos = bookPanelRoot.anchoredPosition;
        Vector3 startScale = bookPanelRoot.localScale;
        
        float t = 0f;
        while (t < focusDuration)
        {
            float u = t / focusDuration;
            float s = Easing.CubicEaseOut(u); // Use consistent easing
            bookPanelRoot.anchoredPosition = Vector2.LerpUnclamped(startPos, _originalBookPanelPosition, s); 
            bookPanelRoot.localScale = Vector3.LerpUnclamped(startScale, _originalBookPanelScale, s);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        
        bookPanelRoot.anchoredPosition = _originalBookPanelPosition;
        bookPanelRoot.localScale = _originalBookPanelScale;

        if (bookSelectHeaderUI) bookSelectHeaderUI.SetActive(true);
        
        _isZoomed = false;
        _selectedBook = null;

        _isTransitioning = false; // We are finished!
    }

    void SetState(GameObject p, bool on, float a, bool interactableOverride = true)
    {
        if (!p) return;
        var cg = p.GetComponent<CanvasGroup>(); if (!cg) cg = p.AddComponent<CanvasGroup>();
        p.SetActive(on);
        cg.alpha = a;
        bool interact = on && a > 0.999f && interactableOverride; 
        cg.interactable = interact;
        cg.blocksRaycasts = interact;
    }

    void OnSoundToggled(bool isOn)
    {
        ApplySound(isOn);
        PlayerPrefs.SetInt(PrefKeySound, isOn ? 1 : 0);
        PlayerPrefs.Save();
        var skin = soundToggle ? soundToggle.GetComponent<SoundToggleSkin>() : null;
        if (skin) skin.SyncVisual();
    }

    void ApplySound(bool isOn)
    {
        AudioListener.volume = isOn ? 1f : 0f;
        if (musicSource && musicSource.clip)
        {
            if (isOn && !musicSource.isPlaying) musicSource.Play();
            if (!isOn && musicSource.isPlaying) musicSource.Pause();
        }
    }

    void Setup(GameObject p, bool on)
    {
        if (!p) return;
        var cg = p.GetComponent<CanvasGroup>(); if (!cg) cg = p.AddComponent<CanvasGroup>();
        cg.alpha = on ? 1f : 0f;
        cg.interactable = on;
        cg.blocksRaycasts = on;
        p.SetActive(on);
    }

    void StartFade(GameObject from, GameObject to)
    {
        if (!from || !to) return;
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Fade(from, to));
    }

    IEnumerator Fade(GameObject from, GameObject to)
    {
        _isTransitioning = true;

        var a = from.GetComponent<CanvasGroup>(); if (!a) a = from.AddComponent<CanvasGroup>();
        var b = to.GetComponent<CanvasGroup>();   if (!b) b = to.AddComponent<CanvasGroup>();

        to.SetActive(true);
        b.alpha = 0f; b.interactable = false; b.blocksRaycasts = false;

        float t = 0f, d = Mathf.Max(0.01f, transitionDuration);
        while (t < d)
        {
            float u = t / d;
            float s = Easing.CubicEaseOut(u);
            a.alpha = 1f - s;
            b.alpha = s;
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        a.alpha = 0f; a.interactable = false; a.blocksRaycasts = false; from.SetActive(false);
        b.alpha = 1f; b.interactable = true;  b.blocksRaycasts = true;
        _current = to; _co = null;

        _isTransitioning = false;
        Log("Fade complete -> " + _current.name);
    }
    
    void AutoWire()
    {
        if (!panelMain)        panelMain        = GameObject.Find(namePanelMain);
        if (!panelAbout)       panelAbout       = GameObject.Find(namePanelAbout);
        if (!panelLeaderboard) panelLeaderboard = GameObject.Find(namePanelLeaderboard);
        if (!panelBookSelect)  panelBookSelect  = GameObject.Find(namePanelBookSelect);
        if (!panelLogin)       panelLogin       = GameObject.Find(namePanelLogin);
        if (!panelName)        panelName        = GameObject.Find(namePanelName);
        if (!panelSession)     panelSession     = GameObject.Find(namePanelSession);
        if (!resultsScreen)    resultsScreen    = GameObject.Find(nameResultsScreen);

        TryBind(nameBtnPlay, OnPlay);
        TryBind(nameBtnAbout, OnAbout);
        TryBind(nameBtnLeaderboard, OnLeaderboard);
        TryBind(nameBtnExit, OnExit);
        TryBind(nameBtnAboutBack, OnBack);
        TryBind(nameBtnLeaderboardBack, OnBack);
        TryBind(nameBtnBackGeneric, OnBack);
        
        // Wire login button
        var loginBtn = GameObject.Find("LoginButton");
        if (loginBtn)
        {
            var btn = loginBtn.GetComponent<Button>();
            if (btn)
            {
                btn.onClick.RemoveListener(OnLogin);
                btn.onClick.AddListener(OnLogin);
                Log("Wired LoginButton -> OnLogin");
            }
        }

        TryBind(nameBtnPlay, OnPlay);
        TryBind(nameBtnAbout, OnAbout);
        TryBind(nameBtnLeaderboard, OnLeaderboard);
        TryBind(nameBtnExit, OnExit);
        TryBind(nameBtnAboutBack, OnBack);
        TryBind(nameBtnLeaderboardBack, OnBack);
        TryBind(nameBtnBackGeneric, OnBack);
        
        // Wire context-specific buttons
        TryBind("ContinueButton", OnContinueFromName);
        TryBind("EnterButton", OnContinueFromName); // For PanelName
        TryBind("SessionButton", OnSession);
        TryBind("ResultsButton", OnResults);

        if (autoBindAnyBackButtons)
        {
            int count = 0;
            var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (var btn in buttons)
            {
                var n = btn.gameObject.name.ToLower();
                if (n.Contains("back"))
                {
                    btn.onClick.RemoveListener(OnBack);
                    btn.onClick.AddListener(OnBack);
                    count++;
                }
            }
            Log("Auto-bound 'Back' buttons: " + count);
        }

        if (!soundToggle)
        {
            var t = GameObject.Find(nameSoundToggle);
            if (t) soundToggle = t.GetComponent<Toggle>();
        }
    }

    void TryBind(string goName, UnityEngine.Events.UnityAction action)
    {
        var go = GameObject.Find(goName);
        var btn = go ? go.GetComponent<Button>() : null;
        if (btn != null){ btn.onClick.RemoveListener(action); btn.onClick.AddListener(action); Log("Bound " + goName + " -> " + action.Method.Name); }
    }

    /// <summary>
    /// Focus first input field on a panel after a short delay
    /// </summary>
    IEnumerator FocusInputFieldDelayed(GameObject panel)
    {
        // Wait for panel to finish fading in
        yield return new WaitForSeconds(transitionDuration + 0.1f);
        
        if (panel != null)
        {
            var inputSetup = panel.GetComponent<InputFieldSetup>();
            if (inputSetup != null)
            {
                inputSetup.FocusFirstField();
                Log($"Focused input field on {panel.name}");
            }
        }
    }

}


