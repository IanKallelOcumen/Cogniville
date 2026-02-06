using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIFirstAid : MonoBehaviour
{
    [Header("Assign your main UI Canvas (the one with your panels)")]
    public Canvas mainCanvas;

    [Header("Active panel that should receive clicks at start")]
    public GameObject activePanel; // e.g., PanelMain in MainMenu

    [Header("Options")]
    public bool ensureEventSystem = true;
    public bool ensureGraphicRaycaster = true;
    public bool logWhatIDid = true;

    void Awake()
    {
        if (ensureEventSystem) EnsureEventSystem();
        if (ensureGraphicRaycaster) EnsureRaycasterOn(mainCanvas);

        if (activePanel)
        {
            var cg = activePanel.GetComponent<CanvasGroup>(); if (!cg) cg = activePanel.AddComponent<CanvasGroup>();
            // Ensure alpha is visible and interactivity is on for the starting panel
            cg.alpha = Mathf.Max(0.001f, cg.alpha);
            cg.interactable = true;
            cg.blocksRaycasts = true;
            if (logWhatIDid) Debug.Log("[UIFirstAid] Active panel ready: " + activePanel.name);
        }
    }

    void EnsureEventSystem()
    {
        // --- FIX: Using modern API to find the EventSystem ---
        // FindAnyObjectByType is the modern, fast equivalent of FindObjectOfType
        // We include inactive objects because the EventSystem might be disabled temporarily.
        if (FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include) != null) return;

        var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
        // Uses the new Input System module if enabled
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        // Fallback to the classic Standalone Input Module
        es.AddComponent<StandaloneInputModule>();
#endif
        if (logWhatIDid) Debug.Log("[UIFirstAid] Created EventSystem with appropriate UI Input Module.");
    }

    void EnsureRaycasterOn(Canvas c)
    {
        if (!c) { if (logWhatIDid) Debug.LogWarning("[UIFirstAid] Main Canvas not assigned."); return; }
        if (!c.GetComponent<GraphicRaycaster>())
        {
            c.gameObject.AddComponent<GraphicRaycaster>();
            if (logWhatIDid) Debug.Log("[UIFirstAid] Added GraphicRaycaster to main Canvas: " + c.name);
        }
    }
}
