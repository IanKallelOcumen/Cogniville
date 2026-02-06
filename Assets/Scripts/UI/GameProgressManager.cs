using UnityEngine;

/// <summary>
/// A simple, static class to manage game progress using PlayerPrefs.
/// This script can be placed anywhere in your project.
/// </summary>
public static class GameProgressManager
{
    // --- Define your save keys here ---
    public const string WORLD_ICE_UNLOCKED = "World_Ice_Unlocked";
    public const string WORLD_FIRE_UNLOCKED = "World_Fire_Unlocked";
    public const string WORLD_JUNGLE_UNLOCKED = "World_Jungle_Unlocked";
    // Add more keys for each world...

    /// <summary>
    /// Checks if a specific world is unlocked.
    /// </summary>
    public static bool IsBookUnlocked(string worldSaveID)
    {
        if (string.IsNullOrEmpty(worldSaveID)) return false;
        return PlayerPrefs.GetInt(worldSaveID, 0) == 1;
    }

    /// <summary>
    /// Saves the unlock state of a world.
    /// </summary>
    public static void SaveBookUnlockState(string worldSaveID, bool isUnlocked)
    {
        if (string.IsNullOrEmpty(worldSaveID)) return;
        PlayerPrefs.SetInt(worldSaveID, isUnlocked ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"Progress Saved: {worldSaveID} = {isUnlocked}");
    }

    /// <summary>
    /// Call this when the player finishes the Ice World.
    /// </summary>
    public static void CompleteIceWorld()
    {
        // Unlock the next world
        SaveBookUnlockState(WORLD_FIRE_UNLOCKED, true);
    }

    /// <summary>
    /// Call this when the player finishes the Fire World.
    /// </summary>
    public static void CompleteFireWorld()
    {
        // Unlock the next world
        SaveBookUnlockState(WORLD_JUNGLE_UNLOCKED, true);
    }
}