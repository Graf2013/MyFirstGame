using UnityEngine;
using UnityEngine.AI;

namespace Enemy.FSM.FsmStates
{
    public class FsmStateFlyAttack : FsmState
    {
        private readonly Transform _transform;
        private readonly NavMeshAgent _agent;
        private readonly FlyAI _fly;
        private readonly float _moveSpeed;
        private readonly float _detectionRadius;
        private readonly float _bulletSpeed;
        private readonly float _fireRate;
        private readonly float _fireSpread;
        private float _checkTimer;
        private const float CheckInterval = 0.5f;

        public FsmStateFlyAttack(Fsm fsm, Transform transform, NavMeshAgent agent, FlyAI fly, float moveSpeed, float detectionRadius, float bulletSpeed, float fireRate, float fireSpread) : base(fsm)
        {
            _transform = transform;
            _agent = agent;
            _fly = fly;
            _moveSpeed = moveSpeed;
            _detectionRadius = detectionRadius;
            _bulletSpeed = bulletSpeed;
            _fireRate = fireRate;
            _fireSpread = fireSpread;
        }

        public override void Enter()
        {
            _agent.speed = _moveSpeed;
            _agent.isStopped = false;
        }

        public override void Update()
        {
            if (!_agent.isOnNavMesh) return;

            _checkTimer += Time.deltaTime;
            if (_checkTimer >= CheckInterval)
            {
                _checkTimer = 0f;
                if (!_fly.PlayerPosition.HasValue)
                {
                    Fsm.SetState<FsmStateFlyAIWander>();
                    return;
                }
            }

            if (_fly.PlayerPosition.HasValue)
            {
                _fly.ShootAtPlayer(_fly.PlayerPosition.Value);
                if (_fly.IsLeader)
                {
                    Vector3 targetPos = _fly.PlayerPosition.Value;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(targetPos, out hit, _detectionRadius, NavMesh.AllAreas))
                    {
                        _agent.SetDestination(hit.position);
                    }
                }
            }
        }

        public override void Exit()
        {
            _agent.isStopped = true;
        }
    }
}