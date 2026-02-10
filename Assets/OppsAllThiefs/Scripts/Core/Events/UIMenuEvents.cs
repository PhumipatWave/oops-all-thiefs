using System;
using UnityEngine;

public static class UIMenuEvents
{
    // Declare events
    public static event Action OnGameInitialized;
    public static event Action OnClickPlayed;
    public static event Action OnClickExited;

    // Use for Invoke the events
    public static void RaiseGameInitialized() => OnGameInitialized?.Invoke();
    public static void RaiseClickPlayed() => OnClickPlayed?.Invoke();
    public static void RaiseClickExited() => OnClickExited?.Invoke();
}
