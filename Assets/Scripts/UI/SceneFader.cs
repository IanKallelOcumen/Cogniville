using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    // ... (Instance getter and static functions are unchanged) ...
    private static SceneFader _instance;
    public static SceneFader Instance {
        get {
            if (_instance == null) {
                var go = new GameObject("SceneFader");
                _instance = go.AddComponent<SceneFader>();
                DontDestroyOnLoad(go);
                _instance.BuildOverlay();
            }
            return _instance;
        }
    }

    public static void FadeToScene(string sceneName, float fadeOut = 0.35f, float fadeIn = 0.25f, float hold = 0.03f)
    {
        Instance.StartFade(sceneName, fadeOut, fadeIn, hold);
    }

    // ... (BuildOverlay and StartFade are unchanged) ...
    Canvas _canvas;
    CanvasGroup _cg;
    Image _img;
    bool _busy;

    void BuildOverlay()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 32767;
        gameObject.AddComponent<GraphicRaycaster>();
        _img = gameObject.AddComponent<Image>();
        _img.color = Color.black;
        _img.raycastTarget = false;
        _cg = gameObject.AddComponent<CanvasGroup>();
        _cg.alpha = 0f;
        var rt = transform as RectTransform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    public void StartFade(string sceneName, float fadeOut, float fadeIn, float hold)
    {
        if (!_busy) StartCoroutine(FadeRoutine(sceneName, fadeOut, fadeIn, hold));
    }

    // ... (FadeRoutine is unchanged, it already calls the Fade coroutine) ...
    IEnumerator FadeRoutine(string sceneName, float fadeOut, float fadeIn, float hold)
    {
        _busy = true;
        if (_img) _img.raycastTarget = true;
        yield return Fade(0f, 1f, Mathf.Max(0.01f, fadeOut));
        if (hold > 0f) yield return new WaitForSecondsRealtime(hold);
        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false; 
        while (op.progress < 0.9f)
        {
            yield return null;
        }
        yield return Fade(1f, 0f, Mathf.Max(0.01f, fadeIn));
        op.allowSceneActivation = true; 
        while (!op.isDone) 
        {
            yield return null;
        }
        if (_img) _img.raycastTarget = false;
        _busy = false;
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            float u = t / duration; 
            
            // --- UPDATED ---
            // Was: float s = 1f - Mathf.Pow(1f - u, 3);
            float s = Easing.CubicEaseOut(u); // Use consistent easing
            // --- END UPDATED ---

            _cg.alpha = Mathf.Lerp(from, to, s);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        _cg.alpha = to;
    }
}
