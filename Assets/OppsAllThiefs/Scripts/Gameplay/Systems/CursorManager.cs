using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private bool visibleCursor;
    [SerializeField] private CursorLockMode cursorMode;

    private void Start()
    {
        Cursor.visible = visibleCursor;
        Cursor.lockState = cursorMode;
    }
}