using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helpers for Firebase Firestore REST API JSON (fields with stringValue, integerValue, etc.).
/// </summary>
public static class JsonUtilityHelper
{
    public static string GetString(string json, string key)
    {
        if (string.IsNullOrEmpty(json)) return null;
        var keyQuote = "\"" + key + "\"";
        var idx = json.IndexOf(keyQuote, StringComparison.Ordinal);
        if (idx < 0) return null;
        idx = json.IndexOf(":", idx);
        if (idx < 0) return null;
        idx = json.IndexOf("\"", idx);
        if (idx < 0) return null;
        var start = idx + 1;
        var end = start;
        while (end < json.Length && json[end] != '"')
        {
            if (json[end] == '\\') end++;
            end++;
        }
        if (end >= json.Length) return null;
        return json.Substring(start, end - start).Replace("\\\"", "\"");
    }

    public static bool GetBool(string json, string key)
    {
        if (string.IsNullOrEmpty(json)) return false;
        var keyQuote = "\"" + key + "\"";
        var idx = json.IndexOf(keyQuote, StringComparison.Ordinal);
        if (idx < 0) return false;
        idx = json.IndexOf(":", idx);
        if (idx < 0) return false;
        return json.IndexOf("true", idx, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>Get boolean from Firestore document fields (e.g. isActive.booleanValue)</summary>
    public static bool GetFirestoreBool(string json, string fieldName)
    {
        if (string.IsNullOrEmpty(json)) return false;
        var keyQuote = "\"" + fieldName + "\"";
        var idx = json.IndexOf(keyQuote, StringComparison.Ordinal);
        if (idx < 0) return false;
        var trueIdx = json.IndexOf("\"booleanValue\"", idx, StringComparison.Ordinal);
        if (trueIdx < 0) return false;
        return json.IndexOf(":true", trueIdx, StringComparison.Ordinal) >= 0 ||
               json.IndexOf(": true", trueIdx, StringComparison.Ordinal) >= 0;
    }

    public static string Escape(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }

    /// <summary>Parse runQuery response into LeaderboardEntry list.</summary>
    public static List<GameDataManager.LeaderboardEntry> ParseLeaderboardQuery(string runQueryJson)
    {
        var list = new List<GameDataManager.LeaderboardEntry>();
        if (string.IsNullOrEmpty(runQueryJson)) return list;
        var docIdx = 0;
        var rank = 1;
        while (true)
        {
            var docStart = runQueryJson.IndexOf("\"document\"", docIdx, StringComparison.Ordinal);
            if (docStart < 0) break;
            var fieldsStart = runQueryJson.IndexOf("\"fields\"", docStart, StringComparison.Ordinal);
            if (fieldsStart < 0) break;
            var openBrace = runQueryJson.IndexOf('{', fieldsStart + "\"fields\"".Length);
            if (openBrace < 0) break;
            var blockEnd = FindMatchingBrace(runQueryJson, openBrace);
            if (blockEnd < 0) break;
            var block = runQueryJson.Substring(openBrace, blockEnd - openBrace + 1);
            var playerName = GetFirestoreString(block, "playerName");
            var scoreStr = GetFirestoreString(block, "score");
            var score = 0;
            int.TryParse(scoreStr, out score);
            list.Add(new GameDataManager.LeaderboardEntry
            {
                playerName = playerName ?? "?",
                score = score,
                rank = rank++
            });
            docIdx = blockEnd + 1;
        }
        return list;
    }

    static string GetFirestoreString(string fieldsBlock, string fieldName)
    {
        var key = "\"" + fieldName + "\"";
        var idx = fieldsBlock.IndexOf(key, StringComparison.Ordinal);
        if (idx < 0) return null;
        var sv = "\"stringValue\"";
        var iv = "\"integerValue\"";
        var si = fieldsBlock.IndexOf(sv, idx, StringComparison.Ordinal);
        var ii = fieldsBlock.IndexOf(iv, idx, StringComparison.Ordinal);
        if (si >= 0 && (ii < 0 || si < ii))
        {
            var colon = fieldsBlock.IndexOf(":", si);
            var start = fieldsBlock.IndexOf("\"", colon) + 1;
            var end = start;
            while (end < fieldsBlock.Length && (fieldsBlock[end] != '"' || (end > 0 && fieldsBlock[end - 1] == '\\'))) end++;
            return end > start ? fieldsBlock.Substring(start, end - start).Replace("\\\"", "\"") : null;
        }
        if (ii >= 0)
        {
            var colon = fieldsBlock.IndexOf(":", ii);
            var start = fieldsBlock.IndexOf("\"", colon) + 1;
            var end = fieldsBlock.IndexOf("\"", start);
            return end > start ? fieldsBlock.Substring(start, end - start) : null;
        }
        return null;
    }

    static int FindMatchingBrace(string json, int openBraceIndex)
    {
        var depth = 1;
        var inString = false;
        var escape = false;
        var quote = '\0';
        for (var i = openBraceIndex + 1; i < json.Length; i++)
        {
            var c = json[i];
            if (escape) { escape = false; continue; }
            if (c == '\\' && inString) { escape = true; continue; }
            if ((c == '"' || c == '\'') && !inString) { inString = true; quote = c; continue; }
            if (inString && c == quote) { inString = false; continue; }
            if (inString) continue;
            if (c == '{') depth++;
            else if (c == '}')
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }
}
