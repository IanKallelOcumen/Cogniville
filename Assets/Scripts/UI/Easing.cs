using UnityEngine;

/// <summary>
/// A static class with common easing functions.
/// We'll use this to make all animations consistent.
/// </summary>
public static class Easing
{
    // We will use CubicEaseOut for almost everything.
    // It starts fast and slows down at the end.
    public static float CubicEaseOut(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3);
    }
    
    // Starts slow, speeds up
    public static float CubicEaseIn(float t)
    {
        return t * t * t;
    }

    // Starts slow, speeds up, ends slow
    public static float CubicEaseInOut(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3) / 2f;
    }

    // No easing
    public static float Linear(float t)
    {
        return t;
    }
}
