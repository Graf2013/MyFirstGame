using Enemy;
using Map.Obstacle;
using UnityEngine;

namespace Projectiles
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private ParticleSystem explosionBullet;

        private void Start()
        {
            // Знищуємо кулю через 10 секунд
            Destroy(gameObject, 10f);
        }

        private void Update()
        {
            // Рухаємо кулю вперед відносно її повороту
            if (rb != null)
            {
                rb.linearVelocity = transform.up * 80;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Перевіряємо перешкоди
            var obstacle = collision.GetComponent<Map.Obstacle.Obstacle>();
            if (obstacle != null)
            {
                obstacle.TakeDamage(Random.Range(10, 25));
                CreateExplosion(collision.transform.position, collision.transform.rotation);
                Destroy(gameObject);
                return;
            }

            // Перевіряємо ворогів з EnemyHealthSystem
            var enemyHealth = collision.GetComponent<Enemy.EnemyHealthSystem>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(Random.Range(10, 25));
                CreateExplosion(collision.transform.position, collision.transform.rotation);
                Destroy(gameObject);
                return;
            }

            // Перевіряємо тестових ворогів
            var testEnemy = collision.GetComponent<Test.TestEnemy>();
            if (testEnemy != null)
            {
                testEnemy.TakeDamage(Random.Range(10, 25));
                CreateExplosion(collision.transform.position, collision.transform.rotation);
                Destroy(gameObject);
                return;
            }
        }

        private void CreateExplosion(Vector3 position, Quaternion rotation)
        {
            if (explosionBullet != null)
            {
                Instantiate(explosionBullet, position, rotation);
            }
        }
    }
}