using Enemy.FSM;
using Enemy.FSM.FsmStates;
using GameMamager;
using Player;
using UnityEngine;

namespace Enemy
{
    public class EnemyAi2 : MonoBehaviour
    {
        [SerializeField] private float wanderRadius = 5f;
        [SerializeField] private float attackRange = 1f;
        [SerializeField] private float attackStopDistance = 0.5f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private Collider2D wanderZone;
        public WorldSpawner.Zone spawnZone;

        private Fsm _fsm;
        private Rigidbody2D _rb;
        private Transform _target;
        private PlayerControllerHuman _player;
        private VehicleManager _vehicleManager;
        
        // Кешовані значення
        private static readonly int PlayerTag = Animator.StringToHash("Player");
        private const float TARGET_UPDATE_INTERVAL = 0.1f;
        private float _targetUpdateTimer;

        // Properties
        public Rigidbody2D Rb => _rb;
        public Transform EnemyTransform => transform;
        public Transform Target => _target;
        public PlayerControllerHuman Player => _player;
        public float WanderRadius => wanderRadius;
        public float MoveSpeed => moveSpeed;
        public float AttackRange => attackRange;
        public float AttackStopDistance => attackStopDistance;
        public float AttackCooldown => attackCooldown;
        public Collider2D WanderZone => wanderZone;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _fsm = new Fsm();
        }

        private void Start()
        {
            InitializeReferences();
            InitializeStates();
        }

        private void InitializeReferences()
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
            {
                _player = playerGO.GetComponent<PlayerControllerHuman>();
                _target = _player.transform;
            }

            _vehicleManager = FindFirstObjectByType<VehicleManager>();
            
            if (spawnZone?.zoneArea != null)
            {
                wanderZone = spawnZone.zoneArea;
            }

            if (_player == null)
            {
                Debug.LogWarning($"Player not found for {gameObject.name}");
                _target = transform; // Fallback
            }
        }

        private void InitializeStates()
        {
            _fsm.AddState(new FsmStateZombieWander(_fsm, this));
            _fsm.AddState(new FsmStateZombieChase(_fsm, this));
            _fsm.SetState<FsmStateZombieWander>();
        }

        private void Update()
        {
            // Оптимізуємо оновлення цілі
            _targetUpdateTimer += Time.deltaTime;
            if (_targetUpdateTimer >= TARGET_UPDATE_INTERVAL)
            {
                UpdateTarget();
                _targetUpdateTimer = 0f;
            }

            _fsm.Update();
        }

        private void UpdateTarget()
        {
            if (_player == null)
            {
                var playerGO = GameObject.FindWithTag("Player");
                if (playerGO != null)
                {
                    _player = playerGO.GetComponent<PlayerControllerHuman>();
                }
                return;
            }

            Transform newTarget = GetCurrentTarget();
            
            if (newTarget != null && newTarget != _target)
            {
                _target = newTarget;
                // Форсуємо перехід в chase стан тільки якщо ворог в зоні дії
                float distanceToTarget = Vector2.Distance(_target.position, transform.position);
                if (distanceToTarget <= attackRange && wanderZone.OverlapPoint(_target.position))
                {
                    _fsm.SetState<FsmStateZombieChase>();
                }
            }
        }

        private Transform GetCurrentTarget()
        {
            if (_vehicleManager?.isInVehicle == true && _vehicleManager.currentVehicle != null)
            {
                return _vehicleManager.currentVehicle.transform;
            }
            return _player?.transform;
        }

        private void FixedUpdate()
        {
            _fsm.FixedUpdate();
        }

        public void ResetState()
        {
            _fsm?.SetState<FsmStateZombieWander>();
            
            if (spawnZone != null)
            {
                transform.position = GetRandomPointInSpawnZone();
            }
        }

        public Vector2 GetRandomPointInSpawnZone()
        {
            if (spawnZone?.zoneArea == null)
                return transform.position;

            var bounds = spawnZone.zoneArea.bounds;
            const int maxAttempts = 30;

            for (int i = 0; i < maxAttempts; i++)
            {
                var randomPoint = new Vector2(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y)
                );

                if (spawnZone.zoneArea.OverlapPoint(randomPoint))
                    return randomPoint;
            }

            return bounds.center;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, wanderRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, attackStopDistance);
        }
    }
}