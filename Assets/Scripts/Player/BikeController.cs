using Player.PlayerInterface;
using Player.PlayerUI;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Player
{
    public class BikeController : MonoBehaviour, IVehicleController
    {
        private Rigidbody2D _rb;
        private Vector2 _moveDirection;
        public bool IsDrifting { get; private set; }
        private Vector2 _currentVelocity;
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
        



        public void Enable() => enabled = true;
        public void Disable() => enabled = false;
        
        void Start()
        {
            moveAction = InputSystem.actions.FindAction("Move");
            _rb = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            _moveDirection = moveAction.ReadValue<Vector2>();
            
            if (!moveAction.enabled)
            {
                moveAction.Enable();
            }
        }

        private void FixedUpdate()
        {
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

        


    }
    
}