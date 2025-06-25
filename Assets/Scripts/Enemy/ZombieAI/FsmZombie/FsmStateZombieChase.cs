using Player;
using UnityEngine;

namespace Enemy.FSM.FsmStates
{
    public class FsmStateZombieChase : FsmState
    {
        private readonly EnemyAi2 _enemy;
        private readonly Rigidbody2D _rb;
        private readonly Transform _enemyTransform;
        private readonly float _moveSpeed;
        private readonly float _attackRange;
        private readonly Collider2D _wanderZone;

        // Константи
        private const float APPROACH_DISTANCE = 2.5f;
        private const float ATTACK_COOLDOWN = 1f;
        private const float AVOIDANCE_RADIUS = 2.4f;
        private const float DETECTION_RADIUS = 1f;
        private const float LERP_SPEED = 20f;
        private const float MIN_MOVEMENT_THRESHOLD = 0.1f;
        private const float STUCK_TIME_THRESHOLD = 2f;
        private const float STUCK_DISTANCE_THRESHOLD = 0.1f;

        // Змінні стану
        private float _attackTimer;
        private float _stuckTimer;
        private Vector2 _lastPosition;
        private Vector2 _lastValidDirection;
        
        // Кешовані компоненти
        private LayerMask _enemyLayer;
        private VehicleManager _vehicleManager;

        public FsmStateZombieChase(Fsm fsm, EnemyAi2 enemy) : base(fsm)
        {
            _enemy = enemy;
            _rb = enemy.Rb;
            _enemyTransform = enemy.EnemyTransform;
            _moveSpeed = enemy.MoveSpeed;
            _attackRange = enemy.AttackRange;
            _wanderZone = enemy.WanderZone;
            
            _enemyLayer = LayerMask.GetMask("EnemyMelee");
            _lastValidDirection = Vector2.right;
            
            // Кешуємо VehicleManager
            _vehicleManager = Object.FindFirstObjectByType<VehicleManager>();
        }

        public override void Enter()
        {
            _attackTimer = 0;
            _stuckTimer = 0;
            _lastPosition = _enemyTransform.position;
        }

        public override void Update() { }

        public override void Exit() { }

        public override void FixedUpdate()
        {
            var target = _enemy.Target;
            if (target == null)
            {
                Fsm.SetState<FsmStateZombieWander>();
                return;
            }

            float distanceToTarget = Vector2.Distance(target.position, _enemyTransform.position);

            // Перевіряємо чи ціль в зоні дії та в радіусі атаки
            if (distanceToTarget >= _attackRange || !_wanderZone.OverlapPoint(target.position))
            {
                Fsm.SetState<FsmStateZombieWander>();
                return;
            }

            CheckIfStuck();

            if (distanceToTarget <= APPROACH_DISTANCE)
            {
                HandleCloseRange();
            }
            else
            {
                MoveToTarget(target);
            }
        }

        private void CheckIfStuck()
        {
            float movementDistance = Vector2.Distance(_enemyTransform.position, _lastPosition);
            if (movementDistance < STUCK_DISTANCE_THRESHOLD)
            {
                _stuckTimer += Time.fixedDeltaTime;
            }
            else
            {
                _stuckTimer = 0;
                _lastPosition = _enemyTransform.position;
            }
        }

        private void HandleCloseRange()
        {
            // Зупиняємо рух
            _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * LERP_SPEED);

            // Атакуємо з кулдауном
            _attackTimer += Time.fixedDeltaTime;
            if (_attackTimer >= ATTACK_COOLDOWN)
            {
                PerformAttack();
                _attackTimer = 0;
            }
        }

        private void PerformAttack()
        {
            const float damage = 13f;
            
            // Визначаємо що атакувати
            if (_vehicleManager?.isInVehicle == true && _vehicleManager.currentVehicle != null)
            {
                var carController = _vehicleManager.currentVehicle.GetComponent<CarController>();
                carController?.TakeDamage(damage);
            }
            else if (_enemy.Player != null)
            {
                _enemy.Player.TakeDamage(damage);
            }
        }

        private void MoveToTarget(Transform target)
        {
            Vector2 directionToTarget = (target.position - _enemyTransform.position).normalized;
            Vector2 movement = CalculateMovement(directionToTarget);

            if (movement.magnitude < MIN_MOVEMENT_THRESHOLD)
            {
                _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * LERP_SPEED);
                return;
            }

            Vector2 targetVelocity = movement * _moveSpeed;
            _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * LERP_SPEED);
        }

        private Vector2 CalculateMovement(Vector2 baseDirection)
        {
            Vector2 separationForce = GetSeparationForce();
            
            // Якщо сила розділення дуже велика, використовуємо тільки її
            if (separationForce.magnitude > 1.5f)
            {
                return separationForce.normalized;
            }

            Vector2 combinedDirection = baseDirection + (separationForce * 0.7f);
            Vector2 obstacleAvoidance = GetObstacleAvoidance(combinedDirection.normalized);
            Vector2 finalDirection = (combinedDirection + obstacleAvoidance).normalized;

            if (finalDirection.magnitude > 0.1f)
            {
                _lastValidDirection = finalDirection;
            }

            return finalDirection;
        }

        private Vector2 GetObstacleAvoidance(Vector2 direction)
        {
            if (!IsPathBlocked(direction))
                return Vector2.zero;

            // Якщо застрягли надовго, рухаємося випадково
            if (_stuckTimer > STUCK_TIME_THRESHOLD)
            {
                _stuckTimer = 0;
                return Random.insideUnitCircle.normalized * 0.8f;
            }

            // Пробуємо рухатися вліво або вправо
            Vector2 leftDirection = Vector2.Perpendicular(direction);
            Vector2 rightDirection = -Vector2.Perpendicular(direction);

            if (!IsPathBlocked(leftDirection))
                return leftDirection * 0.6f;
            if (!IsPathBlocked(rightDirection))
                return rightDirection * 0.6f;

            return Vector2.zero;
        }

        private bool IsPathBlocked(Vector2 direction)
        {
            var hit = Physics2D.CircleCast(
                _enemyTransform.position,
                DETECTION_RADIUS * 0.8f,
                direction,
                AVOIDANCE_RADIUS * 0.8f,
                _enemyLayer
            );

            return hit.collider != null && hit.collider.transform != _enemyTransform;
        }

        private Vector2 GetSeparationForce()
        {
            Vector2 separation = Vector2.zero;
            int nearbyCount = 0;

            var nearbyEnemies = Physics2D.OverlapCircleAll(_enemyTransform.position, AVOIDANCE_RADIUS, _enemyLayer);

            foreach (var enemy in nearbyEnemies)
            {
                if (enemy.transform == _enemyTransform) continue;

                Vector2 awayDirection = (_enemyTransform.position - enemy.transform.position);
                float distance = awayDirection.magnitude;

                if (distance > 0 && distance < AVOIDANCE_RADIUS)
                {
                    float force = (AVOIDANCE_RADIUS - distance) / AVOIDANCE_RADIUS;
                    separation += awayDirection.normalized * force;
                    nearbyCount++;
                }
            }

            // Якщо багато ворогів поруч, збільшуємо силу розділення
            if (nearbyCount > 2)
            {
                separation *= 1.5f;
            }

            return separation;
        }
    }
}