using UnityEngine;
using UnityEngine.Windows;

public class PlayerInputHandle
{
    private PlayerInputActions input;

    public PlayerInputHandle()
    {
        input = new PlayerInputActions();
    }

    public void EnableInput() => input.Player.Enable();
    public void DisableInput() => input.Player.Disable();
}
