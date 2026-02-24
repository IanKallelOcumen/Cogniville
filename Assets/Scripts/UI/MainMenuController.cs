using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

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
    public GameObject panelPrincipal;
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
    public string namePanelPrincipal = "PanelPrincipal";
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
    private float _lastLoginErrorTime = -999f;
    private string _loginSubView = "role";
    private GameObject _loginRoleChoiceRoot;
    private GameObject _loginPrincipalLoginRoot;

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
        Setup(panelPrincipal, false);
        Setup(resultsScreen, false);
        _current = panelMain;
        if (!bookPanelRoot && panelBookSelect) bookPanelRoot = panelBookSelect.GetComponent<RectTransform>();
        HideMainPrincipalButton();
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
        SetState(panelPrincipal, false, 0f);
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

        EnsureLoginPrincipalLoginUI();
        NormalizeAllInputFieldSizesAndCaps();
        ApplyGlobalButtonFeedbackToAll();
        NormalizePrincipalPanelUI();

        // If we just returned from a level, show results panel
        // #region agent log
        DebugAgent.Log("MainMenuController.cs:Start", "PendingShowResults check", "{\"pendingShowResults\":" + (GameDataManager.PendingShowResults ? "true" : "false") + ",\"resultsScreenNotNull\":" + (resultsScreen != null ? "true" : "false") + ",\"instanceNotNull\":" + (GameDataManager.Instance != null ? "true" : "false") + "}", "A");
        // #endregion
        if (GameDataManager.PendingShowResults && resultsScreen != null && GameDataManager.Instance != null)
        {
            GameDataManager.PendingShowResults = false;
            SetState(panelMain, false, 0f);
            SetState(resultsScreen, true, 1f);
            _current = resultsScreen;
            if (resultsScreen != null)
                resultsScreen.SendMessage("RefreshFromGameData", SendMessageOptions.DontRequireReceiver);
            Log("Showing Results (returned from level).");
            // #region agent log
            DebugAgent.Log("MainMenuController.cs:Start", "Showing results panel", "{}", "A");
            // #endregion
        }
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
        // From main menu: open Panel Login and show "Principal or Teacher?"
        if (_current != panelLogin)
        {
            Log("OnLogin from main -> open Panel Login (Principal or Teacher?)");
            if (!panelLogin) { Err("panelLogin is NULL."); return; }
            StartFade(_current, panelLogin);
            return;
        }
        // From role choice: show teacher form so they can enter name/password
        if (_loginSubView != "teacher")
        {
            if (_loginSubView == "role")
                ShowLoginTeacherView();
            return;
        }
        // Teacher login: first name + code (code from principal when they added you)
        string teacherFirstName = GetNameFromPanel(panelLogin);
        string code = GetPasswordFromLoginPanel(panelLogin);
        if (string.IsNullOrWhiteSpace(teacherFirstName))
        {
            if (Time.time - _lastLoginErrorTime > 0.5f)
            {
                _lastLoginErrorTime = Time.time;
                Err("Please enter your first name.");
                ShowPanelError(panelLogin, "Please enter your first name.");
            }
            return;
        }
        ClearPanelError(panelLogin);
        teacherFirstName = teacherFirstName.Trim();
        if (GameDataManager.Instance != null)
        {
            if (!GameDataManager.Instance.IsAllowedTeacher(teacherFirstName, code))
            {
                ShowPanelError(panelLogin, "Not found or wrong code. Ask principal to add you and give you your code.");
                return;
            }
            GameDataManager.Instance.StartSession(teacherFirstName);
            GameDataManager.Instance.SetTeacher(teacherFirstName);
        }
        Log("OnLogin -> Session started, Fade to PanelSession");
        if (!panelSession) { Err("panelSession is NULL"); return; }
        if (_current == panelSession) return;
        StartFade(_current, panelSession);
    }
    
    public void OnContinueFromName()
    {
        string lastName = GetNameFromPanel(panelName);
        string code = GetCodeFromPanel(panelName);
        if (GameDataManager.Instance != null && GameDataManager.Instance.IsSessionActive() && GameDataManager.Instance.HasSessionStudentsWithCodes())
        {
            if (string.IsNullOrWhiteSpace(lastName))
            {
                ShowPanelError(panelName, "Please enter your last name.");
                return;
            }
            if (string.IsNullOrWhiteSpace(code))
            {
                ShowPanelError(panelName, "Please enter the code your teacher gave you.");
                return;
            }
            ClearPanelError(panelName);
            if (!GameDataManager.Instance.IsAllowedSessionStudent(lastName, code))
            {
                ShowPanelError(panelName, "Last name and code don't match. Ask your teacher for your code.");
                return;
            }
            GameDataManager.Instance.EnrollStudentInSession(lastName.Trim());
            if (GameDataManager.Instance != null)
                GameDataManager.Instance.SetPlayerName(lastName.Trim());
        }
        else
        {
            if (string.IsNullOrWhiteSpace(lastName))
            {
                Err("Please enter your name.");
                ShowPanelError(panelName, "Please enter your name.");
                return;
            }
            ClearPanelError(panelName);
            if (GameDataManager.Instance != null)
                GameDataManager.Instance.SetPlayerName(lastName.Trim());
        }
        Log("OnContinueFromName -> Saved name, Fade to PanelBookSelect");
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

    /// <summary>Principal menu: add/manage teachers. From role choice, show principal login (code) first.</summary>
    public void OnPrincipal()
    {
        Log("OnPrincipal clicked");
        if (!panelPrincipal) { Err("panelPrincipal is NULL"); return; }
        if (_current == panelPrincipal) return;
        if (_current == panelLogin && _loginSubView == "role")
        {
            ShowLoginPrincipalLoginView();
            return;
        }
        StartFade(_current, panelPrincipal);
    }

    /// <summary>Submit code to access principal panel (bypass code only).</summary>
    public void OnPrincipalCodeSubmit()
    {
        string code = GetPrincipalCodeFromInput();
        if (string.IsNullOrWhiteSpace(code))
        {
            HidePrincipalLoginInfoText();
            ShowPanelError(panelLogin, "Enter code.");
            return;
        }
        HidePrincipalLoginInfoText();
        ClearPanelError(panelLogin);
        if (GameDataManager.Instance == null) return;
        if (GameDataManager.Instance.IsValidPrincipalCode(code))
        {
            ShowLoginPrincipalView();
            return;
        }
        HidePrincipalLoginInfoText();
        ShowPanelError(panelLogin, "Wrong code.");
    }

    void HidePrincipalLoginInfoText()
    {
        var infoT = _loginPrincipalLoginRoot?.transform.Find("PrincipalLoginInfoText")?.GetComponent<TMPro.TMP_Text>();
        if (infoT != null) { infoT.text = ""; infoT.gameObject.SetActive(false); }
    }

    /// <summary>Ensure Panel Login has an ErrorText so validation messages can be shown.</summary>
    void EnsureLoginErrorText()
    {
        if (panelLogin == null) return;
        var existing = panelLogin.transform.Find("ErrorText");
        if (existing != null) return;
        var go = new GameObject("ErrorText");
        go.transform.SetParent(panelLogin.transform, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.15f);
        rect.anchorMax = new Vector2(0.5f, 0.15f);
        rect.sizeDelta = new Vector2(420, 60);
        rect.anchoredPosition = Vector2.zero;
        var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.fontSize = 28;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.3f, 0.3f, 1f);
        tmp.text = "";
        go.SetActive(false);
    }

    /// <summary>Make teacher form inputs consistent: same font size and style (no bold/italic).</summary>
    void NormalizeLoginInputStyles()
    {
        if (panelLogin == null) return;
        var inputs = panelLogin.GetComponentsInChildren<TMP_InputField>(true);
        const int fontSize = 32;
        foreach (var input in inputs)
        {
            if (input == null) continue;
            if (_loginPrincipalLoginRoot != null && input.transform.IsChildOf(_loginPrincipalLoginRoot.transform))
                continue;
            if (input.textComponent != null)
            {
                input.textComponent.fontSize = fontSize;
                input.textComponent.fontStyle = TMPro.FontStyles.Normal;
                input.textComponent.color = Color.white;
            }
            if (input.placeholder != null)
            {
                var ph = input.placeholder as TMPro.TMP_Text ?? input.placeholder.GetComponent<TMPro.TMP_Text>();
                if (ph != null) { ph.fontSize = fontSize; ph.fontStyle = TMPro.FontStyles.Normal; ph.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); }
            }
        }
    }

    /// <summary>
    /// Set all TMP_InputField in the menu to the same size as the student (PanelName) input and enable autocapitalization.
    /// </summary>
    void NormalizeAllInputFieldSizesAndCaps()
    {
        Vector2? referenceSize = null;
        if (panelName != null)
        {
            var studentInput = panelName.GetComponentInChildren<TMPro.TMP_InputField>(true);
            if (studentInput != null)
            {
                var rt = studentInput.GetComponent<RectTransform>();
                if (rt != null) referenceSize = rt.sizeDelta;
            }
        }
        if (!referenceSize.HasValue) return;

        const int inputFontSize = 32;
        var allInputs = Object.FindObjectsByType<TMPro.TMP_InputField>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var input in allInputs)
        {
            if (input == null) continue;
            var rt = input.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = referenceSize.Value;
            input.contentType = TMP_InputField.ContentType.Name;
            if (input.textComponent != null) { input.textComponent.fontSize = inputFontSize; input.textComponent.color = Color.white; input.textComponent.fontStyle = TMPro.FontStyles.Normal; }
            if (input.placeholder != null)
            {
                var ph = input.placeholder as TMPro.TMP_Text ?? input.placeholder.GetComponent<TMPro.TMP_Text>();
                if (ph != null) { ph.fontSize = inputFontSize; ph.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); ph.fontStyle = TMPro.FontStyles.Normal; }
            }
        }
    }

    public void OnTeacherChoice()
    {
        Log("Teacher choice clicked");
        ShowLoginTeacherView();
    }

    public void OnLoginRoleBack()
    {
        if (_loginSubView == "principal_login" || _loginSubView == "principal" || _loginSubView == "teacher")
        {
            ShowLoginRoleChoice();
            return;
        }
        StartFade(panelLogin, panelMain);
    }

    void HideMainPrincipalButton()
    {
        var principalBtn = panelMain != null ? FindInChildren(panelMain.transform, "PrincipalButton") : null;
        if (principalBtn == null) principalBtn = FindGameObjectByNameIncludingInactive("PrincipalButton");
        if (principalBtn != null && (panelMain == null || principalBtn.transform.IsChildOf(panelMain.transform)))
        {
            principalBtn.SetActive(false);
            Log("Hidden main menu Principal button.");
        }
    }

    void EnsureLoginRoleChoice()
    {
        if (panelLogin == null || _loginRoleChoiceRoot != null) return;
        var existing = panelLogin.transform.Find("LoginRoleChoice");
        if (existing != null) { _loginRoleChoiceRoot = existing.gameObject; return; }
        _loginRoleChoiceRoot = CreateLoginRoleChoiceUI();
        if (_loginRoleChoiceRoot != null)
            _loginRoleChoiceRoot.transform.SetParent(panelLogin.transform, false);
    }

    GameObject CreateLoginRoleChoiceUI()
    {
        var root = new GameObject("LoginRoleChoice");
        var rect = root.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var canvas = panelLogin.GetComponentInParent<Canvas>();
        if (canvas != null) root.AddComponent<CanvasRenderer>();
        var textGo = new GameObject("Label");
        textGo.transform.SetParent(root.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.7f);
        textRect.anchorMax = new Vector2(0.5f, 0.7f);
        textRect.sizeDelta = new Vector2(480, 60);
        textRect.anchoredPosition = Vector2.zero;
        var tmp = textGo.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = "Principal or Teacher?";
        tmp.fontSize = 36;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        foreach (var name in new[] { "Principal", "Teacher", "Back" })
        {
            var btnGo = new GameObject(name + "Button");
            btnGo.transform.SetParent(root.transform, false);
            var btnRect = btnGo.AddComponent<RectTransform>();
            float y = name == "Principal" ? 0.1f : (name == "Teacher" ? -0.1f : -0.35f);
            btnRect.anchorMin = new Vector2(0.5f, 0.5f + y);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f + y);
            btnRect.sizeDelta = new Vector2(260, 56);
            btnRect.anchoredPosition = Vector2.zero;
            var img = btnGo.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0.2f, 0.5f, 0.9f);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            var labelGo = new GameObject("Text");
            labelGo.transform.SetParent(btnGo.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var labelTmp = labelGo.AddComponent<TMPro.TextMeshProUGUI>();
            labelTmp.text = name;
            labelTmp.fontSize = 30;
            labelTmp.alignment = TMPro.TextAlignmentOptions.Center;
            if (name == "Principal") btn.onClick.AddListener(OnPrincipal);
            else if (name == "Teacher") btn.onClick.AddListener(OnTeacherChoice);
            else btn.onClick.AddListener(OnLoginRoleBack);
            AddButtonFeedbackComponents(btnGo);
        }
        return root;
    }

    void ShowLoginRoleChoice()
    {
        _loginSubView = "role";
        _current = panelLogin;
        EnsureLoginRoleChoice();
        if (_loginRoleChoiceRoot != null) _loginRoleChoiceRoot.SetActive(true);
        if (_loginPrincipalLoginRoot != null) _loginPrincipalLoginRoot.SetActive(false);
        SetLoginTeacherFormVisible(false);
        if (panelPrincipal != null) panelPrincipal.SetActive(false);
    }

    /// <summary>Set Panel Login to show only "Principal or Teacher?" before the panel becomes visible (avoids teacher form flash).</summary>
    void PreparePanelLoginForShow()
    {
        if (panelLogin == null) return;
        EnsureLoginRoleChoice();
        if (_loginRoleChoiceRoot != null) _loginRoleChoiceRoot.SetActive(true);
        if (_loginPrincipalLoginRoot != null) _loginPrincipalLoginRoot.SetActive(false);
        SetLoginTeacherFormVisible(false);
        if (panelPrincipal != null) panelPrincipal.SetActive(false);
        ClearPanelError(panelLogin);
    }

    /// <summary>If BackgroundWiggle is under the panel we're leaving, reparent it to the canvas so the bg doesn't stop when that panel is deactivated.</summary>
    void KeepBackgroundVisibleWhenLeaving(GameObject panelWeAreLeaving)
    {
        var bg = Object.FindFirstObjectByType<BackgroundWiggle>(FindObjectsInactive.Include);
        if (bg == null || !bg.transform.IsChildOf(panelWeAreLeaving.transform)) return;
        var canvas = panelLogin != null ? panelLogin.GetComponentInParent<Canvas>() : null;
        if (canvas == null) canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas != null)
        {
            bg.transform.SetParent(canvas.transform, true);
            bg.gameObject.SetActive(true);
        }
    }

    void EnsureLoginPrincipalLoginUI()
    {
        if (panelLogin == null || _loginPrincipalLoginRoot != null) return;
        var existing = panelLogin.transform.Find("PrincipalLogin");
        if (existing != null) { _loginPrincipalLoginRoot = existing.gameObject; return; }
        _loginPrincipalLoginRoot = CreateLoginPrincipalLoginUI();
        if (_loginPrincipalLoginRoot != null)
        {
            _loginPrincipalLoginRoot.transform.SetParent(panelLogin.transform, false);
            _loginPrincipalLoginRoot.SetActive(false);
        }
    }

    GameObject CreateLoginPrincipalLoginUI()
    {
        var root = new GameObject("PrincipalLogin");
        var rect = root.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        root.AddComponent<CanvasRenderer>();
        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(root.transform, false);
        var labelRect = labelGo.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.55f);
        labelRect.anchorMax = new Vector2(0.5f, 0.55f);
        labelRect.sizeDelta = new Vector2(380, 48);
        var labelTmp = labelGo.AddComponent<TMPro.TextMeshProUGUI>();
        labelTmp.text = "Code:";
        labelTmp.fontSize = 32;
        labelTmp.alignment = TMPro.TextAlignmentOptions.Center;
        var inputGo = new GameObject("PrincipalCodeInput");
        inputGo.transform.SetParent(root.transform, false);
        var inputRect = inputGo.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, 0.42f);
        inputRect.anchorMax = new Vector2(0.5f, 0.42f);
        inputRect.sizeDelta = new Vector2(340, 52);
        var inputImg = inputGo.AddComponent<UnityEngine.UI.Image>();
        inputImg.color = new Color(1f, 1f, 1f, 0.95f);
        var inputField = inputGo.AddComponent<TMP_InputField>();
        inputField.contentType = TMP_InputField.ContentType.Alphanumeric;
        var placeholderGo = new GameObject("Placeholder");
        placeholderGo.transform.SetParent(inputGo.transform, false);
        var phRect = placeholderGo.AddComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.offsetMin = new Vector2(10, 6);
        phRect.offsetMax = new Vector2(-10, -6);
        var phTmp = placeholderGo.AddComponent<TMPro.TextMeshProUGUI>();
        phTmp.text = "Enter code";
        phTmp.fontSize = 28;
        phTmp.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        inputField.placeholder = phTmp;
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(inputGo.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12, 8);
        textRect.offsetMax = new Vector2(-12, -8);
        var textTmp = textGo.AddComponent<TMPro.TextMeshProUGUI>();
        textTmp.fontSize = 28;
        inputField.textComponent = textTmp;
        var infoGo = new GameObject("PrincipalLoginInfoText");
        infoGo.transform.SetParent(root.transform, false);
        var infoRect = infoGo.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.5f, 0.35f);
        infoRect.anchorMax = new Vector2(0.5f, 0.35f);
        infoRect.sizeDelta = new Vector2(400, 56);
        var infoTmp = infoGo.AddComponent<TMPro.TextMeshProUGUI>();
        infoTmp.fontSize = 26;
        infoTmp.alignment = TMPro.TextAlignmentOptions.Center;
        infoTmp.color = new Color(0.95f, 0.95f, 0.95f, 0.95f);
        infoTmp.text = "";
        infoGo.SetActive(false);
        foreach (var name in new[] { "Submit", "Back" })
        {
            var btnGo = new GameObject(name + "Button");
            btnGo.transform.SetParent(root.transform, false);
            float y = name == "Submit" ? 0.28f : 0.12f;
            var btnRect = btnGo.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, y);
            btnRect.anchorMax = new Vector2(0.5f, y);
            btnRect.sizeDelta = new Vector2(240, 56);
            var img = btnGo.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0.2f, 0.5f, 0.9f);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            var blGo = new GameObject("Text");
            blGo.transform.SetParent(btnGo.transform, false);
            var blRect = blGo.AddComponent<RectTransform>();
            blRect.anchorMin = Vector2.zero;
            blRect.anchorMax = Vector2.one;
            blRect.offsetMin = Vector2.zero;
            blRect.offsetMax = Vector2.zero;
            var blTmp = blGo.AddComponent<TMPro.TextMeshProUGUI>();
            blTmp.text = name;
            blTmp.fontSize = 30;
            blTmp.alignment = TMPro.TextAlignmentOptions.Center;
            if (name == "Submit") btn.onClick.AddListener(OnPrincipalCodeSubmit);
            else btn.onClick.AddListener(OnLoginRoleBack);
            AddButtonFeedbackComponents(btnGo);
        }
        return root;
    }

    /// <summary>Add UIFloat (hover 1.1) and ButtonFeedback (press scale 1) so this button matches the rest of the menu.</summary>
    void AddButtonFeedbackComponents(GameObject buttonGo)
    {
        if (buttonGo == null) return;
        if (buttonGo.GetComponent<UIFloat>() == null)
        {
            var uf = buttonGo.AddComponent<UIFloat>();
            uf.enableHover = true;
            uf.hoverScaleAmount = 1.1f;
        }
        if (buttonGo.GetComponent<ButtonFeedback>() == null)
        {
            var bf = buttonGo.AddComponent<ButtonFeedback>();
            bf.scaleOnPress = 1f;
        }
    }

    /// <summary>Apply same look and feedback to every button in the scene (sprite from Play/reference, hover 1.1, press scale 1).</summary>
    void ApplyGlobalButtonFeedbackToAll()
    {
        var allButtons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        UnityEngine.UI.Image referenceImage = null;
        foreach (var b in allButtons)
        {
            if (b == null) continue;
            var img = b.GetComponent<UnityEngine.UI.Image>();
            if (img != null && img.sprite != null && b.name != null && b.name.IndexOf("Play", System.StringComparison.OrdinalIgnoreCase) >= 0)
            { referenceImage = img; break; }
        }
        if (referenceImage == null)
        {
            foreach (var b in allButtons)
            {
                if (b == null) continue;
                var img = b.GetComponent<UnityEngine.UI.Image>();
                if (img != null && img.sprite != null) { referenceImage = img; break; }
            }
        }
        foreach (var b in allButtons)
        {
            if (b == null) continue;
            var go = b.gameObject;
            if (go.GetComponent<UIFloat>() == null)
            {
                var uf = go.AddComponent<UIFloat>();
                uf.enableHover = true;
                uf.hoverScaleAmount = 1.1f;
            }
            if (go.GetComponent<ButtonFeedback>() == null)
            {
                var bf = go.AddComponent<ButtonFeedback>();
                bf.scaleOnPress = 1f;
            }
            if (referenceImage != null)
            {
                var img = b.GetComponent<UnityEngine.UI.Image>();
                if (img != null && img != referenceImage && img.sprite != referenceImage.sprite)
                    img.sprite = referenceImage.sprite;
                var refBtn = referenceImage.GetComponent<Button>();
                if (refBtn != null && refBtn != b && b.transition == Selectable.Transition.SpriteSwap)
                {
                    var ss = b.spriteState;
                    if (refBtn.spriteState.pressedSprite != null && ss.pressedSprite != refBtn.spriteState.pressedSprite)
                    {
                        ss.pressedSprite = refBtn.spriteState.pressedSprite;
                        b.spriteState = ss;
                    }
                }
            }
        }
    }

    /// <summary>Make Principal panel match the rest of the menu: same button size (260x56), same font sizes.</summary>
    void NormalizePrincipalPanelUI()
    {
        if (panelPrincipal == null) return;
        const float btnW = 260f;
        const float btnH = 56f;
        const int buttonLabelSize = 30;
        const int titleLabelSize = 32;
        const int listTextSize = 28;
        var buttonSize = new Vector2(btnW, btnH);

        foreach (var b in panelPrincipal.GetComponentsInChildren<Button>(true))
        {
            if (b == null) continue;
            var rt = b.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = buttonSize;
            var label = b.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (label != null) { label.fontSize = buttonLabelSize; label.fontStyle = TMPro.FontStyles.Normal; }
        }

        foreach (var tmp in panelPrincipal.GetComponentsInChildren<TMPro.TMP_Text>(true))
        {
            if (tmp == null) continue;
            string n = (tmp.name ?? "").ToLowerInvariant();
            if (n.Contains("list") || n.Contains("teacher"))
                tmp.fontSize = listTextSize;
            else
                tmp.fontSize = titleLabelSize;
            tmp.fontStyle = TMPro.FontStyles.Normal;
        }

        foreach (var input in panelPrincipal.GetComponentsInChildren<TMP_InputField>(true))
        {
            if (input == null) continue;
            var rt = input.GetComponent<RectTransform>();
            if (rt != null && panelName != null)
            {
                var refInput = panelName.GetComponentInChildren<TMP_InputField>(true);
                if (refInput != null)
                {
                    var refRt = refInput.GetComponent<RectTransform>();
                    if (refRt != null) rt.sizeDelta = refRt.sizeDelta;
                }
            }
            const int inputFontSize = 32;
            if (input.textComponent != null) { input.textComponent.fontSize = inputFontSize; input.textComponent.fontStyle = TMPro.FontStyles.Normal; }
            if (input.placeholder != null)
            {
                var ph = input.placeholder as TMPro.TMP_Text ?? input.placeholder.GetComponent<TMPro.TMP_Text>();
                if (ph != null) { ph.fontSize = inputFontSize; ph.fontStyle = TMPro.FontStyles.Normal; }
            }
        }
    }

    string GetPrincipalCodeFromInput()
    {
        if (panelLogin == null || _loginPrincipalLoginRoot == null) return "";
        var input = _loginPrincipalLoginRoot.transform.Find("PrincipalCodeInput")?.GetComponent<TMP_InputField>();
        return input != null ? (input.text ?? "").Trim() : "";
    }

    void ShowLoginPrincipalLoginView()
    {
        _loginSubView = "principal_login";
        _current = panelLogin;
        EnsureLoginPrincipalLoginUI();
        EnsureLoginErrorText();
        if (_loginRoleChoiceRoot != null) _loginRoleChoiceRoot.SetActive(false);
        if (_loginPrincipalLoginRoot != null)
        {
            _loginPrincipalLoginRoot.SetActive(true);
            var input = _loginPrincipalLoginRoot.transform.Find("PrincipalCodeInput")?.GetComponent<TMP_InputField>();
            if (input != null) input.text = "";
        }
        SetLoginTeacherFormVisible(false);
        if (panelPrincipal != null) panelPrincipal.SetActive(false);
        ClearPanelError(panelLogin);
        var infoT = _loginPrincipalLoginRoot?.transform.Find("PrincipalLoginInfoText")?.GetComponent<TMPro.TMP_Text>();
        if (infoT != null) { infoT.text = ""; infoT.gameObject.SetActive(false); }
    }

    void ShowLoginPrincipalView()
    {
        _loginSubView = "principal";
        if (_loginRoleChoiceRoot != null) _loginRoleChoiceRoot.SetActive(false);
        if (_loginPrincipalLoginRoot != null) _loginPrincipalLoginRoot.SetActive(false);
        SetLoginTeacherFormVisible(false);
        if (panelPrincipal != null)
        {
            panelPrincipal.SetActive(true);
            var cg = panelPrincipal.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; }
            var back = panelPrincipal.transform.Find("BackButton")?.GetComponent<Button>();
            if (back != null) { back.onClick.RemoveAllListeners(); back.onClick.AddListener(OnLoginRoleBack); }
        }
        _current = panelPrincipal;
    }

    void ShowLoginTeacherView()
    {
        _loginSubView = "teacher";
        if (_loginRoleChoiceRoot != null) _loginRoleChoiceRoot.SetActive(false);
        SetLoginTeacherFormVisible(true);
        if (panelPrincipal != null) panelPrincipal.SetActive(false);
    }

    void SetLoginTeacherFormVisible(bool visible)
    {
        if (panelLogin == null) return;
        if (visible) EnsureLoginErrorText();
        var back = panelLogin.transform.Find("LoginBackbutton");
        if (back != null) back.gameObject.SetActive(visible);
        foreach (var name in new[] { "EmailInput", "PasswordInput", "LoginButton" })
        {
            var t = panelLogin.transform.Find(name);
            if (t != null) t.gameObject.SetActive(visible);
        }
        var signup = panelLogin.transform.Find("SignupButton");
        if (signup != null) signup.gameObject.SetActive(false);
        if (visible) NormalizeLoginInputStyles();
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
        // #region agent log
        DebugAgent.Log("MainMenuController.cs:OnBack", "OnBack clicked", "{\"currentName\":\"" + (_current != null ? _current.name : "null") + "\",\"isResultsScreen\":" + (_current == resultsScreen ? "true" : "false") + "}", "C");
        // #endregion
        Log("OnBack clicked");
        if (_isZoomed && !_isTransitioning) 
        {
            ResetZoom();
            return;
        }

        // Context-aware back button
        if (_current == panelName) { StartFade(_current, panelMain); return; }
        if (_current == panelLogin) { OnLoginRoleBack(); return; }
        if (_current == panelPrincipal) { OnLoginRoleBack(); return; }
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
        if (!bookPanelRoot) { _isTransitioning = false; yield break; }
        
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

        // Principal panel is inside PanelLogin; ensure parent is active so it's visible
        if (to == panelPrincipal && panelLogin != null && !panelLogin.activeSelf)
            panelLogin.SetActive(true);

        // When opening Panel Login: set "Principal or Teacher?" before panel is visible so teacher form doesn't flash
        if (to == panelLogin)
            PreparePanelLoginForShow();

        // Keep background visible when switching to Panel Login (bg may be under panelMain; reparent so it doesn't stop)
        if (to == panelLogin && from != null)
            KeepBackgroundVisibleWhenLeaving(from);

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
        // When leaving Principal (which lives inside PanelLogin), hide the whole login panel
        if (from == panelPrincipal && panelLogin != null)
            panelLogin.SetActive(false);
        b.alpha = 1f; b.interactable = true;  b.blocksRaycasts = true;
        _current = to; _co = null;

        if (to == panelPrincipal)
        {
            NormalizePrincipalPanelUI();
            var back = to.transform.Find("BackButton")?.GetComponent<Button>();
            if (back != null) { back.onClick.RemoveAllListeners(); back.onClick.AddListener(OnLoginRoleBack); Log("Bound Principal BackButton -> Back to role choice"); }
        }
        if (to == panelLogin)
        {
            ShowLoginRoleChoice();
            NormalizeLoginInputStyles();
        }
        // Keep background wiggling visible when on any panel (ensure BackgroundWiggle stays enabled)
        var bg = Object.FindFirstObjectByType<BackgroundWiggle>();
        if (bg != null && !bg.gameObject.activeSelf) bg.gameObject.SetActive(true);

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
        if (!panelPrincipal)   panelPrincipal   = GameObject.Find(namePanelPrincipal);
        if (!resultsScreen)    resultsScreen    = GameObject.Find(nameResultsScreen);

        TryBind(nameBtnPlay, OnPlay);
        TryBind(nameBtnAbout, OnAbout);
        TryBind(nameBtnLeaderboard, OnLeaderboard);
        TryBind(nameBtnExit, OnExit);
        TryBind(nameBtnAboutBack, OnBack);
        TryBind(nameBtnLeaderboardBack, OnBack);
        TryBind(nameBtnBackGeneric, OnBack);
        
        // Wire login button (prefer under PanelLogin to avoid binding wrong button if duplicate names exist)
        GameObject loginBtn = null;
        if (panelLogin != null)
        {
            var t = panelLogin.transform.Find("LoginButton");
            if (t != null) loginBtn = t.gameObject;
        }
        if (loginBtn == null) loginBtn = GameObject.Find("LoginButton") ?? FindGameObjectByNameIncludingInactive("LoginButton");
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
        
        // Wire context-specific buttons
        TryBind("ContinueButton", OnContinueFromName);
        TryBind("EnterButton", OnContinueFromName); // For PanelName
        TryBind("SessionButton", OnSession);
        TryBind("ResultsButton", OnResults);
        TryBind("HomeButton", OnBack);
        TryBind("NextButton", OnBack);

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
        if (!go) go = FindGameObjectByNameIncludingInactive(goName);
        var btn = go ? go.GetComponent<Button>() : null;
        // #region agent log
        if (goName == "HomeButton" || goName == "NextButton" || goName.Contains("Back"))
            DebugAgent.Log("MainMenuController.cs:TryBind", "TryBind " + goName, "{\"goFound\":" + (go != null ? "true" : "false") + ",\"btnFound\":" + (btn != null ? "true" : "false") + ",\"action\":\"" + (action?.Method?.Name ?? "") + "\"}", "C");
        // #endregion
        if (btn != null){ btn.onClick.RemoveListener(action); btn.onClick.AddListener(action); Log("Bound " + goName + " -> " + action.Method.Name); }
    }

    GameObject FindGameObjectByNameIncludingInactive(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            var t = root.transform.Find(name);
            if (t != null) return t.gameObject;
            var found = FindInChildren(root.transform, name);
            if (found != null) return found;
        }
        return null;
    }

    static GameObject FindInChildren(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.gameObject.name == name) return child.gameObject;
            var found = FindInChildren(child, name);
            if (found != null) return found;
        }
        return null;
    }

    /// <summary>
    /// Show validation error on panel (find TextMeshProUGUI named ErrorText or MessageText)
    /// </summary>
    void ShowPanelError(GameObject panel, string message)
    {
        if (!panel) return;
        var tmp = panel.transform.Find("ErrorText")?.GetComponent<TMP_Text>();
        if (!tmp) tmp = panel.transform.Find("MessageText")?.GetComponent<TMP_Text>();
        if (tmp) { tmp.text = message; tmp.gameObject.SetActive(true); }
    }

    void ClearPanelError(GameObject panel)
    {
        if (!panel) return;
        var tmp = panel.transform.Find("ErrorText")?.GetComponent<TMP_Text>();
        if (!tmp) tmp = panel.transform.Find("MessageText")?.GetComponent<TMP_Text>();
        if (tmp) tmp.text = string.Empty;
    }

    /// <summary>
    /// Get first input field text from a panel (PanelName or PanelLogin).
    /// For PanelLogin, uses only the login form (EmailInput); ignores Principal panel so teacher login works.
    /// </summary>
    string GetNameFromPanel(GameObject panel)
    {
        if (!panel) return string.Empty;
        var setup = panel.GetComponent<InputFieldSetup>();
        if (setup != null && setup.inputFields != null && setup.inputFields.Length > 0)
        {
            var input = setup.inputFields[0];
            if (input != null) return (input.text ?? string.Empty).Trim();
        }
        var allInputs = panel.GetComponentsInChildren<TMPro.TMP_InputField>(true);
        if (allInputs == null || allInputs.Length == 0) return string.Empty;

        bool isLoginPanel = (panel == panelLogin);
        Transform principalRoot = null;
        if (isLoginPanel)
        {
            if (panelPrincipal != null) principalRoot = panelPrincipal.transform;
            else principalRoot = panel.transform.Find("PanelPrincipal"); // resolve if inactive at Awake
        }

        // Login panel: prefer "email" (login form); skip anything under PanelPrincipal
        foreach (var input in allInputs)
        {
            if (input == null) continue;
            if (principalRoot != null && input.transform.IsChildOf(principalRoot)) continue;
            string name = (input.name ?? "").ToLowerInvariant();
            if (name.Contains("email")) return (input.text ?? string.Empty).Trim();
        }
        foreach (var input in allInputs)
        {
            if (input == null) continue;
            if (principalRoot != null && input.transform.IsChildOf(principalRoot)) continue;
            string name = (input.name ?? "").ToLowerInvariant();
            if (name.Contains("teacher") || name.Contains("name")) return (input.text ?? string.Empty).Trim();
        }
        foreach (var input in allInputs)
        {
            if (input == null) continue;
            if (principalRoot != null && input.transform.IsChildOf(principalRoot)) continue;
            return (input.text ?? string.Empty).Trim();
        }
        return string.Empty;
    }

    /// <summary>
    /// Get "code" field from a panel (e.g. PanelName: student code). Prefers input whose name contains "code", else second input.
    /// </summary>
    string GetCodeFromPanel(GameObject panel)
    {
        if (!panel) return string.Empty;
        var allInputs = panel.GetComponentsInChildren<TMPro.TMP_InputField>(true);
        if (allInputs == null || allInputs.Length == 0) return string.Empty;
        foreach (var input in allInputs)
        {
            if (input == null) continue;
            if ((input.name ?? "").ToLowerInvariant().Contains("code"))
                return (input.text ?? "").Trim();
        }
        if (allInputs.Length >= 2 && allInputs[1] != null)
            return (allInputs[1].text ?? "").Trim();
        return string.Empty;
    }

    /// <summary>
    /// Get password from login panel (login form only; ignores Principal panel inputs).
    /// </summary>
    string GetPasswordFromLoginPanel(GameObject panel)
    {
        if (!panel) return string.Empty;
        var allInputs = panel.GetComponentsInChildren<TMPro.TMP_InputField>(true);
        if (allInputs == null || allInputs.Length == 0) return string.Empty;
        Transform principalRoot = null;
        if (panel == panelLogin)
        {
            if (panelPrincipal != null) principalRoot = panelPrincipal.transform;
            else principalRoot = panel.transform.Find("PanelPrincipal");
        }
        foreach (var input in allInputs)
        {
            if (input == null) continue;
            if (principalRoot != null && input.transform.IsChildOf(principalRoot)) continue;
            if ((input.name ?? "").ToLowerInvariant().Contains("password"))
                return input.text ?? string.Empty;
        }
        int loginFormIndex = 0;
        for (int i = 0; i < allInputs.Length; i++)
        {
            if (allInputs[i] == null) continue;
            if (principalRoot != null && allInputs[i].transform.IsChildOf(principalRoot)) continue;
            if (loginFormIndex == 1) return (allInputs[i].text ?? string.Empty).Trim();
            loginFormIndex++;
        }
        return string.Empty;
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
