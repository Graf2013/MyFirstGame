using GameMamager;
using UI;
using UnityEngine;

namespace Enemy
{
    public class EnemyHealthSystem : MonoBehaviour
    {
        private EnemySpawner _spawner;
        private Vector3 _originalPosition;
        private GameObject _healthBarPrefab;
        private Transform _obstacleHealthBar;
        private Vector3 _offset = new Vector3(0f, 1.2f, 0f);
        private Vector3 _scale = new Vector3(4f, 3f, 1f);
        [SerializeField] public float maxHealth;
        [SerializeField] public float currentHealth;
        private GameObject _lastColliderPlayer;
        [SerializeField] private float minDamage;
        [SerializeField] private float speedDamageMultiplier;
        [SerializeField] private float speedThresholdForMaxDamage;
        [SerializeField] private GameObject floatingMessagePrefab;

        private void Awake()
        {
            _spawner = gameObject.GetComponent<EnemySpawner>();
            _healthBarPrefab = Resources.Load<GameObject>("Prefabs/ObstacleHealthBar");
            _healthBarPrefab.transform.localScale = _scale;
            Instantiate(_healthBarPrefab, transform.position + _offset, Quaternion.identity, this.transform);
            Transform firstChild = gameObject.transform.Find("ObstacleHealthBar(Clone)");
            _obstacleHealthBar = firstChild.transform.Find("FullHealth");
            currentHealth = maxHealth;
        }

        private void Update()
        {
            float percent = currentHealth / maxHealth;
            _obstacleHealthBar.localScale = new Vector3(percent, 1f, 1f);

            if (currentHealth <= 0)
            {
                _spawner.Die();
            }
        }

        public void TakeDamage(float damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            int roundDamage = Mathf.RoundToInt(damage);
            if (roundDamage > 1)
            {
                ShowFloatingMessage(roundDamage.ToString());  
            }
        }
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Vehicle"))
            {
                _lastColliderPlayer = collision.gameObject;
                TakeDamage(minDamage * Time.deltaTime);
            }
        }
    
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Vehicle"))
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
    
        private void ShowFloatingMessage(string message)
        {
            Vector3 spawnPos = transform != null ? transform.position : transform.position + Vector3.up * 1.2f;
        
            GameObject textObj = Instantiate(floatingMessagePrefab, spawnPos, Quaternion.identity);
            textObj.GetComponent<FloatingMassega>().SetMessage(message);
        }
        
        public void Respawn()
        {
            gameObject.SetActive(true);
            transform.position = _originalPosition;
            currentHealth = maxHealth;
        
            // Скидання стану FSM
            var ai = GetComponent<EnemyAi2>();
            if (ai != null) ai.ResetState();
        }
    }
}