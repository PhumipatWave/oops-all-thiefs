using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

[CreateAssetMenu(fileName = "InputReader", menuName = "ScriptableObjects/InputReader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    private PlayerInputActions inputActions;

    public event Action<int> OnHealthChanged;
    public event Action<Vector2> OnMoved;
    public event Action<Vector2> OnSprinted;
    public event Action OnJumped;
    public event Action OnInteracted;
    public event Action OnAttacked;

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.SetCallbacks(this);
        }
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log("Move Input");
        OnMoved?.Invoke(context.ReadValue<Vector2>());
    }
    public void OnSprint(InputAction.CallbackContext context)
    {
        Debug.Log("Sprint Input");
        OnSprinted?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("Jump Input");
        OnJumped?.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("Interact Input");
        OnInteracted?.Invoke();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        Debug.Log("Attack Input");
        OnAttacked?.Invoke();
    }
}
