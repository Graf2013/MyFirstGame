using UnityEngine;
using UnityEngine.AI;

namespace Enemy.FSM.FsmStates
{
    public class FsmStateFlyFlee : FsmState
    {
        private readonly Transform _transform;
        private readonly NavMeshAgent _agent;
        private readonly FlyAI _fly;
        private readonly float _moveSpeed;
        private readonly float _fleeDistance;
        private readonly float _detectionRadius;
        private float _checkTimer;
        private const float CheckInterval = 0.5f;

        public FsmStateFlyFlee(Fsm fsm, Transform transform, NavMeshAgent agent, FlyAI fly, float moveSpeed, float fleeDistance, float detectionRadius) : base(fsm)
        {
            _transform = transform;
            _agent = agent;
            _fly = fly;
            _moveSpeed = moveSpeed;
            _fleeDistance = fleeDistance;
            _detectionRadius = detectionRadius;
        }

        public override void Enter()
        {
            _agent.speed = _moveSpeed;
            _agent.isStopped = false;
            FleeFromPlayer();
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
                }
                else
                {
                    FleeFromPlayer();
                }
            }
        }

        private void FleeFromPlayer()
        {
            if (_fly.PlayerPosition.HasValue)
            {
                Vector2 direction = (_transform.position - (Vector3)_fly.PlayerPosition.Value).normalized;
                Vector3 targetPos = _transform.position + (Vector3)direction * _fleeDistance;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(targetPos, out hit, _fleeDistance, NavMesh.AllAreas))
                {
                    _agent.SetDestination(hit.position);
                }
            }
        }

        public override void Exit()
        {
            _agent.isStopped = true;
        }
    }
}