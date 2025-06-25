using Player.PlayerUI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Player
{
    public class CarController : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private Vector2 _moveDirection;
        public bool IsDrifting { get; private set; }
        private Vector2 _currentVelocity;
        private float _lastFrameVelocity;
        [SerializeField] private InputAction moveAction;

        [Header("Movement Setting")]
        [SerializeField] public float moveSpeed;
        [SerializeField] public float maxMoveSpeed;
        [SerializeField] public float minMoveSpeed;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float steeringFactor;
        
        [Header("Drift Setting")]
        [SerializeField] private float minDriftSpeed;
        [SerializeField] private float driftAngleThreshold;
        
        [Header("Damage Setting")]
        [SerializeField] private float baseDamage;
        [SerializeField] private float speedDamageMultiply;
        [SerializeField] private float minDamageSpeed;
        
        [Header("UI And Effect")]
        [SerializeField] private ParticleSystem smoke;
        [SerializeField] public float currentHealth;
        [SerializeField] public float maxHealth;
        [SerializeField] private HealthBarUI healthBarUI;
        [SerializeField] private EnergyBarUi energyBarUi;


        void Start()
        {
            moveAction = InputSystem.actions.FindAction("Move");
            _rb = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            _moveDirection = moveAction.ReadValue<Vector2>();
            Energy();
            if (!moveAction.enabled)
            {
                moveAction.Enable();
                Debug.Log($"MoveAction увімкнено примусово для {gameObject.name}");
            }
        }

        private void FixedUpdate()
        {
            _lastFrameVelocity = _rb.linearVelocity.magnitude;
            if (_moveDirection.magnitude > 0.1f)
            {
                //Прискорення героя в залежності від його теперішньої швидкості
                if (maxMoveSpeed > moveSpeed)
                {
                    if (moveSpeed < 5)
                    {
                        moveSpeed += Time.deltaTime * 6;

                    }
                    moveSpeed += Time.deltaTime * 3;
                }
                
                //Обчислюємо кут повороту від джойстика 
                float targetAngle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg - 90f;
                float currentAngle = transform.eulerAngles.z;
                
                //Плавно повертаємо героя на обчислений кут
                float angle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime);
                transform.rotation = Quaternion.Euler(0, 0, angle);

                //Обчислюємо напрямок і наближаємо героя до неї
                Vector2 targetVelocity = transform.up * (_moveDirection.magnitude * moveSpeed);
                _rb.linearVelocity = Vector2.SmoothDamp(_rb.linearVelocity, targetVelocity, ref _currentVelocity, steeringFactor);
                
                // обчислюємо кут повороту і якщо він більше за заданий driftAngleThreshold і більше заданої швидкості ми створюємо smoke
                float angleToVelocity = Vector2.Angle(_rb.linearVelocity, transform.up);
                IsDrifting = _rb.linearVelocity.magnitude > minDriftSpeed && angleToVelocity > driftAngleThreshold;
                if (IsDrifting)
                {
                    Instantiate(smoke, transform.position, Quaternion.identity);
                }
            }
            else
            {
                // зменшуємо швидкість якщо не рухаємося і відключаємо smoke
                _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, Vector2.zero, 0.1f);
                IsDrifting = false;

                if (minMoveSpeed < moveSpeed)
                {
                    if (moveSpeed > 10)
                    {
                        moveSpeed -= Time.deltaTime * 5;
                    }
                    moveSpeed -= Time.deltaTime * 2;
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.CompareTag("Obstacle"))
            {
                TakeDamageBySpeed(baseDamage, _lastFrameVelocity);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Obstacle"))
            {
                TakeDamageBySpeed(baseDamage, _lastFrameVelocity);
            }
        }
        
        //collisionVelocity швидкість героя в момент зіткнення
        private void TakeDamageBySpeed(float damage, float collisionVelocity)
        {
            //Якщо швидкість героя більша за мінімальну наносимо урон в залежності від швидкості героя і зменшуємо швидкість
            if (collisionVelocity > minDamageSpeed)
            {
                float impactFactor = 1.0f;

                float speedRatio = collisionVelocity / maxMoveSpeed;
                float damageBySpeed = damage * (1 + speedRatio * speedDamageMultiply) * impactFactor;

                TakeDamage(damageBySpeed);
                moveSpeed *= 0.2f;

            }
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Death();
            }
        }

        private void Energy()
        {
            energyBarUi.currentEnergy = moveSpeed;
        }

        private void Death()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}




