using UnityEngine;
using UnityEngine.AI;

namespace Enemy.FSM.FsmStates
{
    public class FsmStateFlyAIWander : FsmState
    {
        private readonly Transform _transform;
        private readonly NavMeshAgent _agent;
        private readonly FlyAI _fly;
        private readonly float _moveSpeed;
        private readonly float _detectionRadius;
        private readonly float _groupDetectionRadius;
        private readonly int _maxFollowers;
        private Vector3 _targetPosition;
        private float _checkTimer;
        private const float CheckInterval = 0.5f;

        public FsmStateFlyAIWander(Fsm fsm, Transform transform, NavMeshAgent agent, FlyAI fly, float moveSpeed, float detectionRadius, float groupDetectionRadius, int maxFollowers) : base(fsm)
        {
            _transform = transform;
            _agent = agent;
            _fly = fly;
            _moveSpeed = moveSpeed;
            _detectionRadius = detectionRadius;
            _groupDetectionRadius = groupDetectionRadius;
            _maxFollowers = maxFollowers;
        }

        public override void Enter()
        {
            _agent.speed = _moveSpeed;
            _agent.isStopped = false;
            _agent.stoppingDistance = 0.5f;
            SetRandomTarget();
        }

        public override void Update()
        {
            if (!_agent.isOnNavMesh || !_agent.enabled)
            {
                return;
            }

            if (!_agent.pathPending && (!_agent.hasPath || _agent.remainingDistance <= _agent.stoppingDistance))
            {
                SetRandomTarget();
            }

            _checkTimer += Time.deltaTime;
            if (_checkTimer >= CheckInterval)
            {
                _checkTimer = 0f;
                if (_fly.PlayerPosition.HasValue)
                {
                    Fsm.SetState<FsmStateFlyAttack>();
                }
                else if (_fly.IsLeader && _fly.IsNearOtherLeader(out _))
                {
                    SetRandomTarget();
                }
            }
        }

        private void SetRandomTarget()
        {
            int maxAttempts = 15; 
            float searchRadius = _fly.IsLeader ? _groupDetectionRadius : _detectionRadius;

            for (int i = 0; i < maxAttempts; i++)
            {
                // Генеруємо 2D-зміщення
                Vector2 randomOffset = Random.insideUnitCircle * searchRadius;
                Vector3 randomPoint = _transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, searchRadius * 2f, NavMesh.AllAreas))
                {
                    if (_fly.IsLeader)
                    {
                        // Перевіряємо, чи точка не в радіусі іншого лідера
                        if (!IsPointNearOtherLeader(hit.position))
                        {
                            _targetPosition = hit.position;
                            _agent.SetDestination(_targetPosition);

                            return;
                        }
                        else if (_fly.IsNearOtherLeader(out Vector3 avoidPos))
                        {
                            avoidPos.z = 0f;
                            if (NavMesh.SamplePosition(avoidPos, out hit, searchRadius * 2f, NavMesh.AllAreas))
                            {
                                _targetPosition = hit.position;
                                _agent.SetDestination(_targetPosition);

                                return;
                            }
                        }
                    }
                    else
                    {
                        _targetPosition = hit.position;
                        _agent.SetDestination(_targetPosition);

                        return;
                    }
                }
            }
            
        }

        private bool IsPointNearOtherLeader(Vector3 point)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(point, _groupDetectionRadius, LayerMask.GetMask("Enemy"));
            foreach (var collider in colliders)
            {
                if (collider.gameObject == _transform.gameObject) continue;
                var enemy = collider.GetComponent<FlyAI>();
                if (enemy != null && enemy.IsLeader && enemy != _fly)
                {
                    return true;
                }
            }
            return false;
        }

        public override void Exit()
        {
            _agent.isStopped = true;
        }
    }
}