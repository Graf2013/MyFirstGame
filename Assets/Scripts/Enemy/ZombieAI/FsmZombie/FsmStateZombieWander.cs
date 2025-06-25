using UnityEngine;

namespace Enemy.FSM.FsmStates
{
    public class FsmStateZombieWander : FsmState
    {
        private readonly EnemyAi2 _enemy;
        private readonly Rigidbody2D _rb;
        private readonly Transform _enemyTransform;
        private readonly float _wanderRadius;
        private readonly float _moveSpeed;
        private readonly float _attackRange;
        private readonly Collider2D _wanderZone;

        // Константи
        private const float MIN_DISTANCE_TO_POINT = 1f;
        private const float MIN_RANDOM_TIME = 0.8f;
        private const float MAX_RANDOM_TIME = 2.5f;
        private const int MAX_POINT_ATTEMPTS = 10;

        // Змінні стану
        private float _timer;
        private float _nextPointTime;
        private Vector2 _targetPoint;

        public FsmStateZombieWander(Fsm fsm, EnemyAi2 enemy) : base(fsm)
        {
            _enemy = enemy;
            _rb = enemy.Rb;
            _enemyTransform = enemy.EnemyTransform;
            _wanderRadius = enemy.WanderRadius;
            _moveSpeed = enemy.MoveSpeed;
            _attackRange = enemy.AttackRange;
            _wanderZone = enemy.WanderZone;
        }

        public override void Enter()
        {
            GenerateNewPoint();
        }

        public override void Update()
        {
            var target = _enemy.Target;
            if (target == null) return;

            // Перевіряємо дистанцію до цілі та чи ціль в зоні
            float distance = Vector2.Distance(target.position, _enemyTransform.position);
            bool targetInZone = _wanderZone.OverlapPoint(target.position);
            
            if (distance <= _attackRange && targetInZone)
            {
                Fsm.SetState<FsmStateZombieChase>();
            }
        }

        public override void FixedUpdate()
        {
            Vector2 direction = (_targetPoint - (Vector2)_rb.position).normalized;
            _rb.linearVelocity = direction * _moveSpeed;

            _timer += Time.fixedDeltaTime;
            
            // Генеруємо нову точку якщо дійшли до поточної або минув час
            float distanceToPoint = Vector2.Distance(_rb.position, _targetPoint);
            if (distanceToPoint <= MIN_DISTANCE_TO_POINT || _timer >= _nextPointTime)
            {
                GenerateNewPoint();
            }
        }

        private void GenerateNewPoint()
        {
            Vector2 validPoint = _rb.position;
            
            // Шукаємо валідну точку в зоні
            for (int i = 0; i < MAX_POINT_ATTEMPTS; i++)
            {
                Vector2 randomPoint = _rb.position + Random.insideUnitCircle * _wanderRadius;
                
                if (IsInsideZone(randomPoint))
                {
                    validPoint = randomPoint;
                    break;
                }
            }

            _targetPoint = validPoint;
            _timer = 0;
            _nextPointTime = Random.Range(MIN_RANDOM_TIME, MAX_RANDOM_TIME);
        }

        private bool IsInsideZone(Vector2 point)
        {
            return _wanderZone.OverlapPoint(point);
        }

        public override void Exit() { }
    }
}