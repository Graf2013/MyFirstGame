using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private GameObject bullet;

        [SerializeField] CarController carController;
        private Vector2 _attackDirection;
        [SerializeField] private InputAction attackAction; 
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float timeBetweenShots;
        private float _timer;

        private void Start()
        {
            attackAction = InputSystem.actions.FindAction("Attack");
        }
        void Update()
        {
            _attackDirection = attackAction.ReadValue<Vector2>();
            if (_attackDirection.magnitude > 0.1)
            {
                _timer += Time.deltaTime;
                if (_timer >= timeBetweenShots)
                {
                    Instantiate(bullet,transform.position,transform.rotation);
                    _timer = 0;
                }
            
            }
        
        }

        private void FixedUpdate()
        {
            // Повертаємо точку пострілу в залежності від джойстика
            float targetAngle = Mathf.Atan2(_attackDirection.y, _attackDirection.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = transform.eulerAngles.z;
            float angle = Mathf.MoveTowardsAngle(currentAngle, targetAngle,rotationSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
