using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Vector2 _moveDirection;
    public float AngleDraft {  get ; private set; }
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
    }

    private void FixedUpdate()
    {
        _lastFrameVelocity = _rb.linearVelocity.magnitude;
        if (_moveDirection.magnitude > 0.1f)
        {
            if (maxMoveSpeed > moveSpeed)
            {
                if (moveSpeed < 5)
                {
                    moveSpeed += Time.deltaTime * 6;

                }
                moveSpeed += Time.deltaTime * 3;
            }
            float targetAngle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = transform.eulerAngles.z;
            float angle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Euler(0, 0, angle);

            Vector2 targetVelocity = transform.up * (_moveDirection.magnitude * moveSpeed);
            _rb.linearVelocity = Vector2.SmoothDamp(_rb.linearVelocity, targetVelocity, ref _currentVelocity, steeringFactor);

            AngleDraft = angle;

            float angleToVelocity = Vector2.Angle(_rb.linearVelocity, transform.up);
            IsDrifting = _rb.linearVelocity.magnitude > minDriftSpeed && angleToVelocity > driftAngleThreshold;

            if (IsDrifting)
            {
                Instantiate(smoke, transform.position, Quaternion.identity);
            }
        }
        else
        {
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
    private void TakeDamageBySpeed(float damage, float collisionVelocity)
    {
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
        healthBarUI.currentHealth -= damage;
    }

    private void Energy()
    {
        energyBarUi.currentEnergy = moveSpeed;
    }
}




