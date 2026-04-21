using TMPro;
using UnityEngine;
using Unity.Netcode;

public class TestItem : NetworkBehaviour
{
    public GameObject interactUI;
    private bool isPlayerNearby = false;

    void Update()
    {
        if (!IsOwner) return;

        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            PickUpServerRpc();
        }
    }

    [ServerRpc]
    void PickUpServerRpc()
    {
        Debug.Log("Picked up item!");
        DestroyItemClientRpc();
    }

    [ClientRpc]
    void DestroyItemClientRpc()
    {
        interactUI.SetActive(false);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<NetworkObject>().IsOwner) return;

        interactUI.SetActive(true);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Enter item!");
            isPlayerNearby = true;
            interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.GetComponent<NetworkObject>().IsOwner) return;

        interactUI.SetActive(false);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Out item!");
            isPlayerNearby = false;
            interactUI.SetActive(false);
        }
    }
}