using Enemy;
using Map.Obstacle;
using UnityEngine;

namespace Projectiles
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
    
    
        private void Update()
        {
            //Рухаємо кулю вперед відносно її повороту і знищуємо після 10 секунд
            rb.linearVelocity = transform.up * 20;
            Destroy(gameObject, 10f);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // якщо куля влучає в об'єкт який має компонент  Obstacle , EnemyHealthSystem викликаємо в цього об'єкта метод TakeDamage 
            var damageable = collision.GetComponent<Obstacle>();
            if (damageable != null)
            {
                damageable.TakeDamage(Random.Range(1, 5));
                Destroy(gameObject);
            }
            var damageableEnemy = collision.GetComponent<EnemyHealthSystem>();
            if (damageableEnemy != null)
            {
                damageableEnemy.TakeDamage(Random.Range(1, 5));
            }
        
        }


    
    
    }
}
