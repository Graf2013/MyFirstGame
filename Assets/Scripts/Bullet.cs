using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    
    
    private void Update()
    {
        rb.linearVelocity = transform.up * 20;
        Destroy(gameObject, 10f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
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
