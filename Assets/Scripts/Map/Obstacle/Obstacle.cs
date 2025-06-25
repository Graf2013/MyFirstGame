using GameMamager;
using UI;
using UnityEngine;
using VFX;

namespace Map.Obstacle
{
    public class Obstacle : MonoBehaviour
    {
        public WorldSpawner spawner;
        public WorldSpawner.Zone zone; 
        [SerializeField] private ParticleSystem breakEffectPrefab;
        [SerializeField] private GameObject breakEffect;
        [SerializeField] public float maxHealth;
        [SerializeField] public float currentHealth;
        [SerializeField] private float speedDamageMultiplier;
        [SerializeField] private float minDamage;
        [SerializeField] private float speedThresholdForMaxDamage;
        [SerializeField] private Rigidbody2D[] rbBreakEffect;
        private ExplosionCactusFragment _explosionCactusFragment;
        private Transform _obstacleHealthBar;

        private GameObject _lastColliderPlayer;

        [SerializeField] private GameObject floatingMessagePrefab;

        private void Start()
        {
            _explosionCactusFragment = Resources.Load<ExplosionCactusFragment>("Prefabs/CactusFragment");
            rbBreakEffect = GetComponentsInChildren<Rigidbody2D>();
            breakEffectPrefab = Resources.Load<ParticleSystem>("Prefabs/explosion");
            currentHealth = maxHealth;
            
            // Шукаємо полосу хп
            Transform healthBarParent = transform.Find("ObstacleHealthBar(Clone)");
            if (healthBarParent != null)
            {
                _obstacleHealthBar = healthBarParent.Find("FullHealth");
            }
        }

        private void Update()
        {
            // Обновлюємо полосу хп
            if (_obstacleHealthBar != null)
            {
                float percent = currentHealth / maxHealth;
                _obstacleHealthBar.localScale = new Vector3(percent, 1f, 1f);
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            // Наносимо урон в залежності від швидкості героя
            if (collision.gameObject.CompareTag("Player"))
            {
                _lastColliderPlayer = collision.gameObject;

                float playerSpeed = collision.relativeVelocity.magnitude;
                float damage = CalculateDamage(playerSpeed);
                TakeDamage(damage);
            }
        }

        private float CalculateDamage(float playerSpeed)
        {
            // розраховуємо урон 
            if (playerSpeed >= speedThresholdForMaxDamage)
                return maxHealth;
            
            if (playerSpeed <= 0.1f)
                return minDamage;
            
            return playerSpeed * speedDamageMultiplier;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            // Наносимо мінімальний урон якщо гравець стоїть в об'єкті
            if (collision.gameObject.CompareTag("Player"))
            {
                _lastColliderPlayer = collision.gameObject;
                TakeDamage(minDamage * Time.deltaTime);
            }
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            int roundDamage = Mathf.RoundToInt(damage);
            
            // Виводимо урон нанесений гравцем
            if (roundDamage > 1)
            {
                ShowFloatingMessage(roundDamage.ToString());
            }

            // Знищуємо об'єкт якщо здоровя упало
            if (currentHealth <= 0)
            {
                if (_lastColliderPlayer != null)
                {
                    DestroyObstacle(_lastColliderPlayer);
                }
                else
                {
                    Death();
                }
            }
        }

        private void ShowFloatingMessage(string message)
        {
            // Спавнимо нанесений текст кількості завданої шкоди
            Vector3 spawnPos = transform.position + Vector3.up * 1.2f;
            GameObject textObj = Instantiate(floatingMessagePrefab, spawnPos, Quaternion.identity);
            textObj.GetComponent<FloatingMassega>().SetMessage(message);
        }

        private void DestroyObstacle(GameObject player)
        {
            // Знищуємо об'єкт гравцем
            Vector2 direction = (transform.position - player.transform.position).normalized;
            SpawnDeathEffects(direction);
            NotifySpawnerAndDestroy();
        }

        private void Death()
        {
            // Знищуємо об'єкт 
            SpawnDeathEffects(Vector2.zero);
            NotifySpawnerAndDestroy();
        }

        private void SpawnDeathEffects(Vector2 direction)
        {
            // Спавнимо ефект смерті і розльоту частиць 
            Vector3 deathPosition = transform.position;
            ParticleSystem effect = Instantiate(breakEffectPrefab, deathPosition, Quaternion.identity);
            
            if (_explosionCactusFragment != null)
            {
                _explosionCactusFragment.Explosion();
            }

            if (direction != Vector2.zero)
            {
                var shape = effect.shape;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                shape.rotation = new Vector3(0, 0, angle);
                shape.arc = 90;
            }
            
            effect.Play();
            BreakEffect();
        }

        private void NotifySpawnerAndDestroy()
        {
            // Повідомляємо спавнер про знищення обєкта
            if (spawner != null && zone != null)
            {
                spawner.OnObjectDestroyed(gameObject, zone);
            }
            Destroy(gameObject);
        }

        private void BreakEffect()
        {
            // Задаємо рандомний напрямок частицям
            if (breakEffect != null)
            {
                Instantiate(breakEffect, transform.position, Quaternion.identity);
            }
            
            foreach (Rigidbody2D rb in rbBreakEffect)
            {
                if (rb != null)
                {
                    Vector2 rand = new Vector2(Random.Range(-3, 3), Random.Range(0, 5));
                    rb.AddForce(rand, ForceMode2D.Impulse);
                }
            }
        }
    }
}