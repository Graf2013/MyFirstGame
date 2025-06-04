using UnityEngine;

namespace Enemy.FSM.FsmStates
{
    public class FsmStateSwarm : FsmState
    {
        private readonly Transform _transform;
        private readonly Rigidbody2D _rb;
        private readonly EnemyAi1 _enemy;
        private readonly float _speed = 5f;
        private readonly float _sineAmplitude = 1.1f; 
        private readonly float _sineFrequency = 0.2f; 
        private float _time;
        private float _playerCheckTimer;
        private Vector2 _currentVelocity;
        private const float PlayerCheckInterval = 0.5f;
        private const float SmoothingFactor = 0.1f; 

        public FsmStateSwarm(Fsm fsm, Transform transform, Rigidbody2D rb, EnemyAi1 enemy) : base(fsm)
        {
            this._transform = transform;
            this._rb = rb;
            this._enemy = enemy;
        }

        public override void Enter()
        {
            _time = Random.Range(0f, 2f * Mathf.PI);
            _playerCheckTimer = 0f;
            _currentVelocity = _rb.linearVelocity;
        }

        public override void Update()
        {
            if (_enemy.Leader == null || _enemy.Leader == _enemy || !_enemy.Leader.gameObject.activeInHierarchy)
            {
                _enemy.Leader = null;
                Fsm.SetState<FsmStateFlee>();
                return;
            }

            _playerCheckTimer += Time.deltaTime;
            if (_playerCheckTimer >= PlayerCheckInterval && _enemy.PlayerPosition.HasValue)
            {
                _playerCheckTimer = 0f;
                _enemy.ShootAtPlayer(_enemy.PlayerPosition.Value);
            }

            if (_enemy.Leader.Followers.Count >= 9 && !_enemy.IsLeader)
            {
                _enemy.JoinGroup(null);
                Fsm.SetState<FsmStateFlee>();
            }
        }

        public override void FixedUpdate()
        {
            if (_enemy.Leader == null || !_enemy.Leader.gameObject.activeInHierarchy) return;

            Vector2 leaderPos = _enemy.Leader.transform.position;
            Vector2 currentPos = _transform.position;
            Vector2 directionToLeader = (leaderPos - currentPos).normalized;
            Vector2 perpendicular = new Vector2(-directionToLeader.y, directionToLeader.x);
            _time += Time.fixedDeltaTime;
            Vector2 sineOffset = perpendicular * (Mathf.Sin(_time * _sineFrequency) * _sineAmplitude);

            float distanceToLeader = Vector2.Distance(currentPos, leaderPos);
            Vector2 desiredPos = leaderPos + sineOffset;
            Vector2 desiredVelocity;
            if (distanceToLeader > _enemy.swarmFormationDistance)
            {
                desiredVelocity = (desiredPos - currentPos).normalized * _speed;
            }
            else
            {
                desiredVelocity = sineOffset / Time.fixedDeltaTime;
            }
            
            _currentVelocity = Vector2.Lerp(_currentVelocity, desiredVelocity, SmoothingFactor);
            _rb.linearVelocity = _currentVelocity;
        }
    }
}