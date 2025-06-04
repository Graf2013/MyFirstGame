using UnityEngine;

namespace Enemy.FSM.FsmStates
{
    public class FsmStatePursue : FsmState
    {
        private readonly Transform _transform;
        private readonly Rigidbody2D _rb;
        private readonly EnemyAi1 _enemy;
        private readonly float _speed = 5f;
        private float _randomOffsetTimer;
        private Vector2 _randomOffset;
        private float _playerCheckTimer;
        private const float PlayerCheckInterval = 1f;
        private const float RandomOffsetInterval = 1.5f;
        private const float RandomOffsetRadius = 2f;

        public FsmStatePursue(Fsm fsm, Transform transform, Rigidbody2D rb, EnemyAi1 enemy) : base(fsm)
        {
            this._transform = transform;
            this._rb = rb;
            this._enemy = enemy;
        }

        public override void Enter()
        {
            _randomOffsetTimer = 0f;
            _playerCheckTimer = 0f;
            _randomOffset = Vector2.zero;
        }

        public override void Update()
        {
            _playerCheckTimer += Time.deltaTime;
            if (_playerCheckTimer >= PlayerCheckInterval)
            {
                _playerCheckTimer = 0f;
                if (_enemy.FindPlayer() == null)
                {
                    Fsm.SetState<FsmStateRandomWalking>();
                    return;
                }
            }

            if (_enemy.PlayerPosition.HasValue)
            {
                _enemy.ShootAtPlayer(_enemy.PlayerPosition.Value);
            }
        }

        public override void FixedUpdate()
        {
            if (!_enemy.PlayerPosition.HasValue) return;

            _randomOffsetTimer += Time.fixedDeltaTime;
            if (_randomOffsetTimer >= RandomOffsetInterval)
            {
                _randomOffset = Random.insideUnitCircle * RandomOffsetRadius;
                _randomOffsetTimer = Random.Range(-0.5f, 0f); // Зміщення кожні 1–2 секунди
            }

            Vector2 playerPos = _enemy.PlayerPosition.Value + _randomOffset;
            Vector2 currentPos = _transform.position;
            Vector2 directionToPlayer = (playerPos - currentPos).normalized;
            _rb.linearVelocity = directionToPlayer * _speed;
        }
    }
}