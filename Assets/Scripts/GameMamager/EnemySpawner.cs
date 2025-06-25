using Enemy;
using UnityEngine;

namespace GameMamager
{
    public class EnemySpawner : MonoBehaviour
    {
        public WorldSpawner spawner;
        public WorldSpawner.Zone zone;

        public void Die()
        {
            spawner.OnEnemyDeath(gameObject, zone);
            gameObject.SetActive(false);
        }
        public void Respawn()
        {
            gameObject.SetActive(true);
        
            // Скидання здоров'я
            var health = GetComponent<EnemyHealthSystem>();
            if (health != null) health.Respawn();
        
            // Скидання стану AI
            var ai = GetComponent<EnemyAi2>();
            if (ai != null) ai.ResetState();
        }
    }
}