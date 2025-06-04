using UnityEngine;

namespace Enemy.FSM.FsmStates
{
    public class FsmStateFlee : FsmState
    {
        private readonly Transform _transform;
        private readonly Rigidbody2D _rb;
        private readonly EnemyAi1 _enemy;
        private readonly float _speed = 5f;
        private float _playerCheckTimer;
        private const float PlayerCheckInterval = 0.5f;

        public FsmStateFlee(Fsm fsm, Transform transform, Rigidbody2D rb, EnemyAi1 enemy) : base(fsm)
        {
            this._transform = transform;
            this._rb = rb;
            this._enemy = enemy;
        }

        public override void Enter()
        {
            _playerCheckTimer = 0f;
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
                }
            }
        }

        public override void FixedUpdate()
        {
            if (_enemy.PlayerPosition == null) return;

            Vector2 playerPos = _enemy.PlayerPosition.Value;
            Vector2 currentPos = _transform.position;
            Vector2 directionAway = (currentPos - playerPos).normalized;
            _rb.linearVelocity = directionAway * _speed;
        }
    }
}