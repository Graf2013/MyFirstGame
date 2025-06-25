using UnityEngine;

namespace Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f; // Максимальне здоров’я героя
        private float currentHealth;

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
            // Логіка смерті героя
            Debug.Log($"{gameObject.name} помер!");
            gameObject.SetActive(false); // Деактивація героя
            // Тут можна додати виклик Game Over, рестарт рівня тощо
        }

        // Додатковий метод для отримання поточного здоров’я (наприклад, для UI)
        public float GetCurrentHealth()
        {
            return currentHealth;
        }

        // Додатковий метод для отримання максимального здоров’я
        public float GetMaxHealth()
        {
            return maxHealth;
        }
    }
}