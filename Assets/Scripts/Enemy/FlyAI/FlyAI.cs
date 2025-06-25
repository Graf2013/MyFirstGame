using System.Collections.Generic;
using Enemy.FSM;
using Enemy.FSM.FsmStates;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

namespace Enemy
{
    public class FlyAI : MonoBehaviour
    {
        [SerializeField] private float detectionRadius = 10f; // Радіус виявлення для одиночних ворогів
        [SerializeField] private float groupDetectionRadius = 15f; // Радіус виявлення для груп
        [SerializeField] private float fleeDistance = 15f; // Дистанція для втечі
        [SerializeField] private float swarmDistance = 3f; // Дистанція для руху підлеглих навколо лідера
        [SerializeField] private GameObject bulletPrefab; 
        [SerializeField] private float bulletSpeed = 10f; 
        [SerializeField] private float fireRate = 1f;
        [SerializeField] private float fireSpread = 10f; 
        [SerializeField] private float moveSpeed = 5f; 
        [SerializeField] private int maxFollowers = 9; 


        public FlyAI Leader { get; set; }
        public List<FlyAI> Followers { get; private set; } = new List<FlyAI>();
        public bool IsLeader => Leader == this;
        public Vector2? PlayerPosition { get; private set; }
        private Fsm _fsm;
        private NavMeshAgent _agent;
        private SpriteRenderer _spriteRenderer;
        private ObjectPool<GameObject> _bulletPool;
        private GameObject _cachedPlayer;
        private float _playerCheckTimer;
        private float _enemyCheckTimer;
        private float _fireTimer;
        private const float PlayerCheckInterval = 0.5f;
        private const float EnemyCheckInterval = 1f;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
            _agent.stoppingDistance = 0.5f; 
            
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

            _bulletPool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(bulletPrefab),
                actionOnGet: bullet => bullet.SetActive(true),
                actionOnRelease: bullet => bullet.SetActive(false),
                actionOnDestroy: Destroy,
                maxSize: 50
            );

            InitializeFsm();
        }

        private void Start()
        {
            if (!_agent.isOnNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
                {
                    transform.position = hit.position;
                    _agent.Warp(hit.position);
                }
                else
                {
                    gameObject.SetActive(false);
                    return;
                }
            }
            _agent.enabled = true; // Переконуємося, що агент активний
        }

        private void InitializeFsm()
        {
            _fsm = new Fsm();
            _fsm.AddState(new FsmStateFlyAIWander(_fsm, transform, _agent, this, moveSpeed, detectionRadius, groupDetectionRadius, maxFollowers));
            _fsm.AddState(new FsmStateFlyFlee(_fsm, transform, _agent, this, moveSpeed, fleeDistance, detectionRadius));
            _fsm.AddState(new FsmStateFlyFollowLeader(_fsm, transform, _agent, this, moveSpeed, swarmDistance, bulletSpeed, fireRate, fireSpread));
            _fsm.AddState(new FsmStateFlyAttack(_fsm, transform, _agent, this, moveSpeed, detectionRadius, bulletSpeed, fireRate, fireSpread));
            _fsm.SetState<FsmStateFlyAIWander>();
        }

        private void Update()
        {
            if (!Mathf.Approximately(transform.position.z, 0f))
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
            }
            
                _fsm.Update();
                UpdateColor();
                UpdatePlayerPosition();
                UpdateEnemyCheck();
        }

        private void UpdateColor()
        {
            _spriteRenderer.color = IsLeader ? Color.yellow : Color.blue;
        }

        private void UpdatePlayerPosition()
        {
            _playerCheckTimer += Time.deltaTime;
            if (_playerCheckTimer < PlayerCheckInterval) return;

            _playerCheckTimer = 0f;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, LayerMask.GetMask("Player"));
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    _cachedPlayer = collider.gameObject;
                    PlayerPosition = _cachedPlayer.transform.position;
                    return;
                }
            }

            _cachedPlayer = null;
            PlayerPosition = null;
        }

        private void UpdateEnemyCheck()
        {
            if (IsLeader || HasLeader()) return;

            _enemyCheckTimer += Time.deltaTime;
            if (_enemyCheckTimer < EnemyCheckInterval) return;

            _enemyCheckTimer = 0f;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, LayerMask.GetMask("Enemy"));
            FlyAI bestLeader = null;
            int maxFollowersCount = -1;

            foreach (var collider in colliders)
            {
                if (collider.gameObject == gameObject) continue;
                var enemy = collider.GetComponent<FlyAI>();
                if (enemy != null && enemy.IsLeader && enemy.Followers.Count < maxFollowers)
                {
                    if (enemy.Followers.Count > maxFollowersCount)
                    {
                        maxFollowersCount = enemy.Followers.Count;
                        bestLeader = enemy;
                    }
                }
            }

            if (bestLeader != null)
            {
                JoinGroup(bestLeader);
            }
            else
            {
                foreach (var collider in colliders)
                {
                    if (collider.gameObject == gameObject) continue;
                    var enemy = collider.GetComponent<FlyAI>();
                    if (enemy != null && !enemy.IsLeader && !enemy.HasLeader())
                    {
                        JoinGroup(this);
                        enemy.JoinGroup(this);
                        break;
                    }
                }
            }
        }

        public bool HasLeader()
        {
            return Leader != null && Leader.gameObject.activeInHierarchy;
        }

        public void JoinGroup(FlyAI newLeader)
        {
            if (newLeader == null || newLeader.Followers.Count >= maxFollowers) return;

            if (Leader != null && Leader != this)
            {
                Leader.Followers.Remove(this);
            }

            Leader = newLeader;
            if (newLeader != this)
            {
                newLeader.Followers.Add(this);
                _fsm.SetState<FsmStateFlyFollowLeader>();
            }
            else
            {
                _fsm.SetState<FsmStateFlyAIWander>();
            }
        }

        public void ShootAtPlayer(Vector2 target)
        {
            _fireTimer += Time.deltaTime;
            if (_fireTimer < 1f / fireRate) return;

            _fireTimer = 0f;
            GameObject bullet = _bulletPool.Get();
            bullet.transform.position = transform.position;
            Vector2 direction = (target - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + Random.Range(-fireSpread, fireSpread);
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = bullet.transform.right * bulletSpeed;
            }
            else
            {
                _bulletPool.Release(bullet);
            }
        }

        public bool IsNearOtherLeader(out Vector3 avoidPosition)
        {
            avoidPosition = Vector3.zero;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, groupDetectionRadius, LayerMask.GetMask("Enemy"));
            foreach (var collider in colliders)
            {
                if (collider.gameObject == gameObject) continue;
                var enemy = collider.GetComponent<FlyAI>();
                if (enemy != null && enemy.IsLeader && enemy != this)
                {
                    Vector2 direction = (transform.position - enemy.transform.position).normalized;
                    avoidPosition = transform.position + (Vector3)direction * groupDetectionRadius;
                    return true;
                }
            }
            return false;
        }

        private void OnDestroy()
        {
            if (IsLeader)
            {
                foreach (var follower in Followers)
                {
                    if (follower != null)
                    {
                        follower.Leader = null;
                        follower._fsm.SetState<FsmStateFlyFlee>();
                    }
                }
            }
            else if (Leader != null)
            {
                Leader.Followers.Remove(this);
            }

            _bulletPool?.Dispose();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            if (IsLeader)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, groupDetectionRadius);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, swarmDistance);
            }
        }
    }
}