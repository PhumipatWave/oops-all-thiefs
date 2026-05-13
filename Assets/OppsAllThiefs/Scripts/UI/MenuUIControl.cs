using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUIControl : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeField;
    [SerializeField] private Toggle privateToggle;

    public async void StartHost()
    {
        await HostHandler.Instance.GameManager.StartHostAsync(privateToggle.isOn);
    }

    public async void StartClient()
    {
        await ClientHandler.Instance.GameManager.StartClientAsync(joinCodeField.text);
    }
}
