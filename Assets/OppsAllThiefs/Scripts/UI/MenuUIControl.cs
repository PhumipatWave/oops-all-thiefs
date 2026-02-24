using TMPro;
using UnityEngine;

public class MenuUIControl : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeField;
    public async void StartHost()
    {
        await HostHandler.Instance.GameManager.StartHostAsync();
    }

    public async void StartClient()
    {
        await ClientHandler.Instance.GameManager.StartClientAsync(joinCodeField.text);
    }
}
