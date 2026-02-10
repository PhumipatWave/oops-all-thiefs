using System;
using UnityEngine;

public static class CharacterEvents
{
    // Declare events
    public static event Action<int> OnHealthChanged;
    public static event Action<Vector2> OnMoved;
    public static event Action OnAttacked;

    // Use for Invoke the events
    public static void RaiseHealthChanged(int amount) => OnHealthChanged?.Invoke(amount);
    public static void RaiseMoved(Vector2 direction) => OnMoved?.Invoke(direction);
    public static void RaiseAttacked() => OnAttacked?.Invoke();
}
