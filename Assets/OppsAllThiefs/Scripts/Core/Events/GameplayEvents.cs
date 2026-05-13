using System;
using UnityEngine;

public static class GameplayEvents
{
    // Declare events
    public static event Action OnGameStarted;
    public static event Action OnGameFinished;

    // Use for Invoke the events
    public static void RaiseGameStarted() => OnGameStarted?.Invoke();
    public static void RaiseGameFinished() => OnGameFinished?.Invoke();
}
