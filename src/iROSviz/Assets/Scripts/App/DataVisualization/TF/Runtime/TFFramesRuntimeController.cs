using UnityEngine;

public static class TFRuntimeController
{
    private static bool isPaused = false;

    public static void Pause() =>
        isPaused = true;

    public static void Play() =>
        isPaused = false;

    public static bool IsPaused() =>
        isPaused;
}
