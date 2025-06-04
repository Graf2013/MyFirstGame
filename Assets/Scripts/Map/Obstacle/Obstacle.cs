using UI;
using UnityEngine;
using VFX;

namespace Map.Obstacle
{
    class Obstacle : MonoBehaviour
    {
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
            Transform firstChild = gameObject.transform.Find("ObstacleHealthBar(Clone)");
            _obstacleHealthBar = firstChild.transform.Find("FullHealth");
        }

        private void Update()
        {
            float percent = currentHealth / maxHealth;
            _obstacleHealthBar.localScale = new Vector3(percent, 1f, 1f);
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                _lastColliderPlayer = collision.gameObject;

                float playerSpeed = collision.relativeVelocity.magnitude;
                float damage;

                if (playerSpeed >= speedThresholdForMaxDamage)
                {
                    damage = maxHealth;
                }
                else if (playerSpeed <= 0.1f)
                {
                    damage = minDamage;
                }
                else
                {
                    damage = playerSpeed * speedDamageMultiplier;
                }

                TakeDamage(damage);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
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
            if (roundDamage > 1)
            {
                ShowFloatingMessage(roundDamage.ToString());
            }


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
            Vector3 spawnPos = transform != null ? transform.position : transform.position + Vector3.up * 1.2f;

            GameObject textObj = Instantiate(floatingMessagePrefab, spawnPos, Quaternion.identity);
            textObj.GetComponent<FloatingMassega>().SetMessage(message);
        }

        private void DestroyObstacle(GameObject player)
        {
            Vector2 direction = (transform.position - player.transform.position).normalized;

            ParticleSystem effect = Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
            _explosionCactusFragment.Explosion();
            var shape = effect.shape;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            shape.rotation = new Vector3(0, 0, angle);
            shape.arc = 90;
            effect.Play();
            Destroy(gameObject);
            BreakEffect();
        }

        private void Death()
        {
            ParticleSystem effect = Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
            _explosionCactusFragment.Explosion();
            effect.Play();
            Destroy(gameObject);
            BreakEffect();
        }

        private void BreakEffect()
        {
            Instantiate(breakEffect, transform.position, Quaternion.identity);
            foreach (Rigidbody2D rb in rbBreakEffect)
            {
                Vector2 rand = new Vector2(Random.Range(-3, 4555555), Random.Range(0, 455555555555));
                rb.AddForce(rand, ForceMode2D.Impulse);
            }
        }
    }
}