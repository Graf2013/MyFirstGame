using UnityEngine;
using UnityEngine.UI;

namespace Player.PlayerUI
{
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private Image healthBar;
        [SerializeField] private float maxHealth;
        [SerializeField] public float currentHealth;
        void Start()
        {
            currentHealth = maxHealth;
        }

        void Update()
        {
            healthBar.fillAmount = currentHealth / maxHealth;
        }
    }
}
