using System;
using System.IO;

/// <summary>Debug session logging for agent; writes NDJSON to workspace log file.</summary>
public static class DebugAgent
{
    const string LogPath = "debug-05851e.log";
    const string SessionId = "05851e";

    public static void Log(string location, string message, string dataJson, string hypothesisId)
    {
        try
        {
            var ts = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            var line = "{\"sessionId\":\"" + SessionId + "\",\"location\":\"" + Escape(location) + "\",\"message\":\"" + Escape(message) + "\",\"data\":" + (string.IsNullOrEmpty(dataJson) ? "{}" : dataJson) + ",\"timestamp\":" + ts + ",\"hypothesisId\":\"" + hypothesisId + "\"}\n";
            var path = Path.Combine(UnityEngine.Application.dataPath, LogPath);
            File.AppendAllText(path, line);
        }
        catch { }
    }

    static string Escape(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }
}
