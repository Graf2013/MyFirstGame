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

        private readonly float _vehicleCheckInterval = 0.2f;
        private float _vehicleCheckTimer;

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _playerHealth = GetComponent<PlayerHealth>();
            _vehicleManager = FindFirstObjectByType<VehicleManager>();
            _moveAction = InputSystem.actions.FindAction("Move");
            _moveAction.Enable();
            
            var uiManager = FindFirstObjectByType<PlayerUI.PlayerUiManager>();
            if (uiManager != null && _playerHealth != null)
            {
                uiManager.SetUIProvider(_playerHealth);
            }
        }

        private void Update()
        {
            _moveDirection = _moveAction.ReadValue<Vector2>();


            // Оптимізуємо перевірку транспорту
            _vehicleCheckTimer += Time.deltaTime;
            if (_vehicleCheckTimer >= _vehicleCheckInterval)
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
            _rb.linearVelocity = Vector2.zero;

            gameObject.SetActive(false);
        }

        public void EnableControls()
        {
            gameObject.SetActive(true);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}