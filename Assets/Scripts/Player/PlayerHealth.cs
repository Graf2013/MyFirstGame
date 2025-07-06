using Player.PlayerInterface;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class PlayerHealth : MonoBehaviour, IUIProvider, IDamageable
    {
        [SerializeField] private float maxHealth = 100f; 
        private float currentHealth;
        
        public float GetCurrentHealth() => currentHealth;
        public float GetMaxHealth() => maxHealth;
        public float GetCurrentEnergy() => currentHealth;
        public float GetMaxEnergy() => maxHealth;

        private void Start()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
            Debug.Log($"{gameObject.name} отримав {damage} шкоди, здоров’я: {currentHealth}");
        }

        private void Die()
        {
            Debug.Log($"{gameObject.name} помер!");
            SceneManager.LoadScene("MainMenu");
            gameObject.SetActive(false);
        }
        
    }
}