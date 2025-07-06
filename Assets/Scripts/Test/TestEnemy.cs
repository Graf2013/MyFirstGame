using UnityEngine;

namespace Test
{
    public class TestEnemy : MonoBehaviour
    {
        [Header("Enemy Settings")]
        public float health = 100f;
        public GameObject deathEffect;
    
        public void TakeDamage(float damage)
        {
            health -= damage;
        
            if (health <= 0)
            {
                Die();
            }
        }
    
        void Die()
        {
            if (deathEffect != null)
            {
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            }
        
            Destroy(gameObject);
        }
    }
}