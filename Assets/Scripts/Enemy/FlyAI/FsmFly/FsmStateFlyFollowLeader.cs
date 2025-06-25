using UnityEngine;
using UnityEngine.AI;

namespace Enemy.FSM.FsmStates
{
    public class FsmStateFlyFollowLeader : FsmState
    {
        private readonly Transform _transform;
        private readonly NavMeshAgent _agent;
        private readonly FlyAI _fly;
        private readonly float _moveSpeed;
        private readonly float _swarmDistance;
        private readonly float _bulletSpeed;
        private readonly float _fireRate;
        private readonly float _fireSpread;
        private float _angle;
        private float _checkTimer;
        private const float CheckInterval = 0.5f;
        private const float AvoidanceDistance = 1f;

        public FsmStateFlyFollowLeader(Fsm fsm, Transform transform, NavMeshAgent agent, FlyAI fly, float moveSpeed,
            float swarmDistance, float bulletSpeed, float fireRate, float fireSpread) : base(fsm)
        {
            _transform = transform;
            _agent = agent;
            _fly = fly;
            _moveSpeed = moveSpeed;
            _swarmDistance = swarmDistance;
            _bulletSpeed = bulletSpeed;
            _fireRate = fireRate;
            _fireSpread = fireSpread;
        }

        public override void Enter()
        {
            _agent.speed = _moveSpeed;
            _agent.isStopped = false;
            _angle = Random.Range(0f, 360f);
        }

        public override void Update()
        {
            if (!_agent.isOnNavMesh || _fly.Leader == null || !_fly.Leader.gameObject.activeInHierarchy)
            {
                _fly.Leader = null;
                Fsm.SetState<FsmStateFlyAIWander>();
                return;
            }

            if (_fly.PlayerPosition.HasValue)
            {
                _fly.ShootAtPlayer(_fly.PlayerPosition.Value);
            }


            MoveAroundLeader();
            AvoidOtherFollowers();
        }

        private void MoveAroundLeader()
        {
            Vector3 leaderPos = _fly.Leader.transform.position;
            _angle += Time.deltaTime * 50f; // Швидкість обертання по колу
            Vector3 offset = new Vector3(Mathf.Cos(_angle * Mathf.Deg2Rad), Mathf.Sin(_angle * Mathf.Deg2Rad), 0) *
                             _swarmDistance;
            Vector3 targetPos = leaderPos + offset;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, _swarmDistance, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
            }
        }

        private void AvoidOtherFollowers()
        {
            foreach (var follower in _fly.Leader.Followers)
            {
                if (follower == _fly || follower == null) continue;
                float distance = Vector2.Distance(_transform.position, follower.transform.position);
                if (distance < AvoidanceDistance)
                {
                    Vector2 away = (_transform.position - follower.transform.position).normalized;
                    Vector3 avoidPos = _transform.position + (Vector3)away * AvoidanceDistance;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(avoidPos, out hit, AvoidanceDistance, NavMesh.AllAreas))
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