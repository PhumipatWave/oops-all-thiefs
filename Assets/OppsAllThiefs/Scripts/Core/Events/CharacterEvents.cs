using System;
using UnityEngine;

/// <summary>
/// This global event can't use in multiplayer
/// because it only called in local player
/// </summary>
public static class CharacterEvents
{
    // Declare events
    public static event Action<int> OnHealthChanged;
    public static event Action<Vector3> OnMoved;
    public static event Action<Vector3> OnSprinted;
    public static event Action OnJumped;
    public static event Action OnInteracted;
    public static event Action OnAttacked;

    // Use for Invoke the events
    public static void RaiseHealthChanged(int amount) => OnHealthChanged?.Invoke(amount);
    public static void RaiseMoved(Vector3 direction) => OnMoved?.Invoke(direction);
    public static void RaiseSprinted(Vector3 direction) => OnSprinted?.Invoke(direction);
    public static void RaiseJumped() => OnJumped?.Invoke();
    public static void RaiseInteracted() => OnInteracted?.Invoke();
    public static void RaiseAttacked() => OnAttacked?.Invoke();
}
