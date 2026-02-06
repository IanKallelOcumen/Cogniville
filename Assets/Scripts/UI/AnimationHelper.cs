using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shared animation utilities to eliminate duplicate coroutines across scripts.
/// Reduces code duplication and makes animations consistent everywhere.
/// </summary>
public static class AnimationHelper
{
    /// <summary>
    /// Generic fade for CanvasGroup from one alpha to another.
    /// </summary>
    public static IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float targetAlpha, float duration)
    {
        if (!cg) yield break;
        
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float u = t / duration;
            float eased = Easing.CubicEaseOut(u);
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, eased);
            yield return null;
        }
        cg.alpha = targetAlpha;
        
        bool isInteractable = targetAlpha > 0.5f;
        cg.interactable = isInteractable;
        cg.blocksRaycasts = isInteractable;
    }

    /// <summary>
    /// Scale a transform smoothly to a target scale.
    /// </summary>
    public static IEnumerator ScaleTransform(Transform target, Vector3 startScale, Vector3 endScale, float duration, bool useUnscaled = true)
    {
        if (!target) yield break;
        
        float t = 0f;
        while (t < duration)
        {
            t += useUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            float u = t / duration;
            float eased = Easing.CubicEaseOut(u);
            target.localScale = Vector3.Lerp(startScale, endScale, eased);
            yield return null;
        }
        target.localScale = endScale;
    }

    /// <summary>
    /// Move a RectTransform to a target position.
    /// </summary>
    public static IEnumerator MoveRectTransform(RectTransform rt, Vector2 startPos, Vector2 endPos, float duration, bool useUnscaled = true)
    {
        if (!rt) yield break;
        
        float t = 0f;
        while (t < duration)
        {
            t += useUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            float u = t / duration;
            float eased = Easing.CubicEaseOut(u);
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
            yield return null;
        }
        rt.anchoredPosition = endPos;
    }

    /// <summary>
    /// Shake a transform position with easing.
    /// </summary>
    public static IEnumerator ShakeTransform(Transform target, Vector3 basePos, float duration, float maxShakeAmount, bool useUnscaled = true)
    {
        if (!target) yield break;
        
        float t = 0f;
        while (t < duration)
        {
            t += useUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            float p = t / duration;
            float easeP = Easing.CubicEaseIn(p);
            float shakeAmount = easeP * maxShakeAmount;
            
            target.localPosition = basePos + (Vector3)Random.insideUnitCircle * shakeAmount;
            yield return null;
        }
        target.localPosition = basePos;
    }

    /// <summary>
    /// Pop animation (scale up then down).
    /// </summary>
    public static IEnumerator PopAnimation(Transform target, float startScale, float popScale, float duration, bool useUnscaled = true)
    {
        if (!target) yield break;
        
        float halfDuration = duration / 2f;
        float t = 0f;

        // Pop Out
        while (t < halfDuration)
        {
            t += useUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            float p = t / halfDuration;
            float eased = Easing.CubicEaseOut(p);
            target.localScale = Vector3.one * Mathf.Lerp(startScale, popScale, eased);
            yield return null;
        }

        // Pop Back
        t = 0f;
        while (t < halfDuration)
        {
            t += useUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            float p = t / halfDuration;
            float eased = Easing.CubicEaseOut(p);
            target.localScale = Vector3.one * Mathf.Lerp(popScale, startScale, eased);
            yield return null;
        }

        target.localScale = Vector3.one * startScale;
    }

    /// <summary>
    /// Bump animation with optional rotation.
    /// </summary>
    public static IEnumerator BumpAnimation(
        Transform target,
        float bumpHeight,
        float duration,
        float rotateAmount = 0f,
        bool useUnscaled = true)
    {
        if (!target) yield break;
        
        Vector3 startPos = target.localPosition;
        Vector3 startRot = target.localEulerAngles;
        float peakHeight = startPos.y + bumpHeight;
        float halfDuration = duration / 2f;

        // Bump Up
        float t = 0f;
        while (t < halfDuration)
        {
            t += useUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            float p = t / halfDuration;
            float eased = Easing.CubicEaseOut(p);
            
            Vector3 newPos = startPos;
            newPos.y = Mathf.Lerp(startPos.y, peakHeight, eased);
            target.localPosition = newPos;
            
            if (rotateAmount != 0f)
                target.localEulerAngles = startRot + new Vector3(0, 0, rotateAmount * (1f - p));
            
            yield return null;
        }

        // Bump Down
        t = 0f;
        while (t < halfDuration)
        {
            t += useUnscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            float p = t / halfDuration;
            float eased = Easing.CubicEaseOut(p);
            
            Vector3 newPos = startPos;
            newPos.y = Mathf.Lerp(peakHeight, startPos.y, eased);
            target.localPosition = newPos;
            
            yield return null;
        }

        target.localPosition = startPos;
        target.localEulerAngles = startRot;
    }
}
