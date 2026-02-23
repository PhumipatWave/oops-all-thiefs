using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;


public class TestMoneyUI : CharacterStatHandle
{
    public TextMeshProUGUI moneyText;
    private int money = 0;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            AddItem();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            SubtractItem();
        }
    }
    
    public void AddItem()
    {
        money += 100;
        UpdateUI();
    }

    public void SubtractItem()
    {
        if (money > 0) money -= 100;
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = $"Money: ${money}";
    }
}