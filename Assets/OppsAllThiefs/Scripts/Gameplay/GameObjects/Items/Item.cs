using UnityEngine;
using TMPro; 

public class Item : MonoBehaviour
{
    public TextMeshProUGUI itemText; 
    private int count = 0;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            AddItem();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            SubtractItem();
        }
    }

    public void AddItem()
    {
        count++;
        UpdateUI();
    }

    public void SubtractItem()
    {
        if (count > 0) count--;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (itemText != null)
            itemText.text = $"Item: ${count}";
    }
}