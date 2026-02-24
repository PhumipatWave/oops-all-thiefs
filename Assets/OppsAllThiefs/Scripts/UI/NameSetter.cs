using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameSetter : MonoBehaviour
{
    [Header("Name Components")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button connectButton;

    [SerializeField] private int minNameLength = 1;
    [SerializeField] private int maxNameLength = 12;

    private void Start()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        nameInputField.text = PlayerPrefs.GetString(UserConstKey.GetPlayerNameKey(), string.Empty);
        HandleNameChanged();
    }

    public void HandleNameChanged()
    {
        connectButton.interactable = 
            nameInputField.text.Length >= minNameLength && 
            nameInputField.text.Length <= maxNameLength;
    }

    public void Connect()
    {
        PlayerPrefs.SetString(UserConstKey.GetPlayerNameKey(), nameInputField.text);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
