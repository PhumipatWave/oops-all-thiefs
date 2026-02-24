using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TestHealthUI : MonoBehaviour
{
    
    [Header("UI_Health")]
    [SerializeField] private float health = 100f;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Image healthBarImage;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TakeDamage(-10f);
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TakeDamage(10f);
        }
        
    }

    private void TakeDamage(float damage)
    {
        health += damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = health / maxHealth;
        }
        Debug.Log("Current Health: " + health);
    }
    
}



