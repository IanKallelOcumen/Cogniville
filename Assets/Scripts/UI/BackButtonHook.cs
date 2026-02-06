using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simplified back button handler.
/// Automatically connects to MainMenuController.OnBack().
/// </summary>
[RequireComponent(typeof(Button))]
public class BackButtonHook : MonoBehaviour
{
    private Button _btn;
    private MainMenuController _controller;

    void Awake()
    {
        _btn = GetComponent<Button>();
    }

    void Start()
    {
        // Find controller and hook up the button
        if (_controller == null)
        {
#if UNITY_2023_1_OR_NEWER
            _controller = Object.FindFirstObjectByType<MainMenuController>(FindObjectsInactive.Include);
#else
            var all = Resources.FindObjectsOfTypeAll<MainMenuController>();
            _controller = (all != null && all.Length > 0) ? all[0] : null;
#endif
        }

        if (_btn != null && _controller != null)
        {
            _btn.onClick.RemoveListener(_controller.OnBack);
            _btn.onClick.AddListener(_controller.OnBack);
        }
    }
}
