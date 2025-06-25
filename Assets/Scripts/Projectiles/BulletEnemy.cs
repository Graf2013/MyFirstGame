using Player;
using UnityEngine;

namespace Projectiles
{
    public class EnemyBullet : MonoBehaviour
    {
        public float damage = 10f;
        public float lifetime = 5f;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Наносимо шкоду при влучанні в героя
            if (other.CompareTag("Player"))
            {
                var playerHealth = other.GetComponent<CarController>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
    }
}