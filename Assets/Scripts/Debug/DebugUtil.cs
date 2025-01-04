using UnityEngine;

public static class DebugUtil
{
    // Категорії логування
    public static bool EnableNetwork = true;
    public static bool EnableGameplay = true;
    public static bool EnableResources = true;
    public static bool EnableCombat = true;
    public static bool EnableAI = true;

    // Рівні логування
    private static bool logInfo = true;
    private static bool logWarning = true;
    private static bool logError = true;

    // Методи для логування з категоріями
    public static void LogNetwork(string message, LogType type = LogType.Log)
    {
        if (!EnableNetwork) return;
        Log($"[Network] {message}", type);
    }

    public static void LogGameplay(string message, LogType type = LogType.Log)
    {
        if (!EnableGameplay) return;
        Log($"[Gameplay] {message}", type);
    }

    public static void LogResources(string message, LogType type = LogType.Log)
    {
        if (!EnableResources) return;
        Log($"[Resources] {message}", type);
    }

    public static void LogCombat(string message, LogType type = LogType.Log)
    {
        if (!EnableCombat) return;
        Log($"[Combat] {message}", type);
    }

    public static void LogAI(string message, LogType type = LogType.Log)
    {
        if (!EnableAI) return;
        Log($"[AI] {message}", type);
    }

    // Базовий метод логування
    private static void Log(string message, LogType type)
    {
        switch (type)
        {
            case LogType.Log when logInfo:
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, message);
                break;
            case LogType.Warning when logWarning:
                Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, null, message);
                break;
            case LogType.Error when logError:
                Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, null, message);
                break;
        }
    }

    // Методи для керування логуванням
    public static void EnableAll()
    {
        EnableNetwork = true;
        EnableGameplay = true;
        EnableResources = true;
        EnableCombat = true;
        EnableAI = true;
        logInfo = true;
        logWarning = true;
        logError = true;
    }

    public static void DisableAll()
    {
        EnableNetwork = false;
        EnableGameplay = false;
        EnableResources = false;
        EnableCombat = false;
        EnableAI = false;
        logInfo = false;
        logWarning = false;
        logError = true; // Залишаємо помилки увімкненими
    }

    // Форматування часу для логів
    public static string FormatTime(float time)
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        int milliseconds = (int)((time * 1000) % 1000);
        return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
    }

    // Форматування мережевої сторони
    public static string FormatNetworkSide(bool isServer)
    {
        return isServer ? "Server" : "Client";
    }
}