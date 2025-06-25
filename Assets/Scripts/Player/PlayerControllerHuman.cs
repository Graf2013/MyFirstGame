using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerControllerHuman : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float interactionRadius = 2f;
        
        private Rigidbody2D _rb;
        private Vector2 _moveDirection;
        private InputAction _moveAction;
        private VehicleManager _vehicleManager;
        private PlayerHealth _playerHealth;
        private InteractableVehicle _nearestVehicle;

        // Оптимізація пошуку транспорту
        private const float VEHICLE_CHECK_INTERVAL = 0.2f;
        private float _vehicleCheckTimer;

        private void Start()
        {
            InitializeComponents();
            InitializeInputs();
        }

        private void InitializeComponents()
        {
            _rb = GetComponent<Rigidbody2D>();
            _playerHealth = GetComponent<PlayerHealth>();
            _vehicleManager = FindFirstObjectByType<VehicleManager>();

            if (_rb == null)
                Debug.LogError($"Rigidbody2D не знайдено на {gameObject.name}!");
            if (_playerHealth == null)
                Debug.LogError($"PlayerHealth не знайдено на {gameObject.name}!");
            if (_vehicleManager == null)
                Debug.LogError($"VehicleManager не знайдено!");
        }

        private void InitializeInputs()
        {
            _moveAction = InputSystem.actions.FindAction("Move");
            if (_moveAction == null)
            {
                Debug.LogError($"Дія 'Move' не знайдена для {gameObject.name}!");
            }
            else
            {
                _moveAction.Enable();
            }
        }

        private void Update()
        {
            if (_moveAction != null)
            {
                _moveDirection = _moveAction.ReadValue<Vector2>();
            }

            // Оптимізуємо перевірку транспорту
            _vehicleCheckTimer += Time.deltaTime;
            if (_vehicleCheckTimer >= VEHICLE_CHECK_INTERVAL)
            {
                CheckForVehicles();
                _vehicleCheckTimer = 0f;
            }
        }

        private void FixedUpdate()
        {
            if (_rb != null)
            {
                _rb.linearVelocity = _moveDirection * moveSpeed;
            }
        }

        private void CheckForVehicles()
        {
            _nearestVehicle = null;
            float closestDistance = float.MaxValue;

            // Використовуємо OverlapCircle замість FindObjectsByType для оптимізації
            var vehicleColliders = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
            
            foreach (var collider in vehicleColliders)
            {
                var vehicle = collider.GetComponent<InteractableVehicle>();
                if (vehicle != null)
                {
                    float distance = Vector2.Distance(transform.position, vehicle.transform.position);
                    if (distance < closestDistance)
                    {
                        _nearestVehicle = vehicle;
                        closestDistance = distance;
                    }
                }
            }

            _vehicleManager?.SetInteractableVehicle(_nearestVehicle);
        }

        public void TakeDamage(float damage)
        {
            _playerHealth?.TakeDamage(damage);
        }

        public void DisableControls()
        {
            _moveAction?.Disable();
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }
            gameObject.SetActive(false);
        }

        public void EnableControls()
        {
            gameObject.SetActive(true);
            _moveAction?.Enable();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}