using GameMamager;
using UnityEngine;

namespace Enemy.FSM.FsmStates
{
    public class FsmStateRandomWalking : FsmState
    {
        private readonly Transform _transform;
        private readonly Rigidbody2D _rb;
        private readonly EnemyAi1 _enemy;
        private readonly float _radius = 5f;
        private readonly float _speed = 5f;
        private Vector2 _targetPosition;
        private float _checkGroupTimer;
        private float _checkPlayerTimer;
        private const float CheckGroupInterval = 1f;
        private const float CheckPlayerInterval = 0.5f;

        public FsmStateRandomWalking(Fsm fsm, Transform transform, Rigidbody2D rb, EnemyAi1 enemy) : base(fsm)
        {
            this._transform = transform;
            this._rb = rb;
            this._enemy = enemy;
        }

        public override void Enter()
        {
            CreateRandomTargetInCircle();
            _checkGroupTimer = 0f;
            _checkPlayerTimer = 0f;
        }

        public override void Update()
        {
            _checkGroupTimer += Time.deltaTime;
            if (_checkGroupTimer >= CheckGroupInterval)
            {
                CheckForGroup();
                _checkGroupTimer = 0f;
            }

            _checkPlayerTimer += Time.deltaTime;
            if (_checkPlayerTimer >= CheckPlayerInterval)
            {
                _checkPlayerTimer = 0f;
                if (!_enemy.IsInGroup && _enemy.FindPlayer() != null)
                {
                    Fsm.SetState<FsmStateFlee>();
                }
                else if (_enemy.IsLeader && _enemy.FindPlayer() != null)
                {
                    Fsm.SetState<FsmStatePursue>();
                }
            }
        }

        public override void FixedUpdate()
        {
            MoveToTarget();
        }

        private void MoveToTarget()
        {
            Vector2 currentPos = _transform.position;
            Vector2 direction = (_targetPosition - currentPos).normalized;
            _rb.linearVelocity = direction * _speed;

            if (Vector2.Distance(currentPos, _targetPosition) <= 0.1f)
                CreateRandomTargetInCircle();
        }

        private void CreateRandomTargetInCircle()
        {
            _targetPosition = (Vector2)_transform.position + Random.insideUnitCircle * _radius;
        }

        private void CheckForGroup()
        {
            var nearbyEnemies = _enemy.FindNearbyEnemies();
            if (nearbyEnemies.Count == 0) return;

            EnemyAi1 bestLeader = GameManager.Instance.FindBestLeader(_transform.position, _enemy.searchRadius);
            if (bestLeader != null)
            {
                _enemy.JoinGroup(bestLeader);
            }
            else if (!_enemy.IsInGroup)
            {
                _enemy.JoinGroup(_enemy);
                foreach (var nearbyEnemy in nearbyEnemies)
                {
                    if (!nearbyEnemy.IsInGroup && _enemy.Followers.Count < 9)
                    {
                        nearbyEnemy.JoinGroup(_enemy);
                    }
                }
            }
        }
    }
}