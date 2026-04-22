using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIControl : NetworkBehaviour
{
    [Header("Player Component")]
    [SerializeField] private Character player;

    private GameObject gameHUD;

    [Header("UI Component")]
    [SerializeField] private GameObject healthBarObject;
    [SerializeField] private Image healthBarImage;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            healthBarObject.SetActive(false);
        }

        if (IsClient)
        {
            player.CurrentHealth.OnValueChanged += HandleHealthChanged;
            HandleHealthChanged(0, player.CurrentHealth.Value);

            Debug.Log($"UI Current health{player.CurrentHealth.OnValueChanged}");
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient) return;
        player.CurrentHealth.OnValueChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int oldHealth, int newHealth)
    {
        healthBarImage.fillAmount = (float)newHealth / player.MaxHealth;
    }
}