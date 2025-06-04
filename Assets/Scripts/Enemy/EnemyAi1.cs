using System.Collections.Generic;
using Enemy.FSM;
using Enemy.FSM.FsmStates;
using GameMamager;
using UnityEngine;
using UnityEngine.Pool;

namespace Enemy
{
    public class EnemyAi1 : MonoBehaviour
    {
        public EnemyAi1 TargetEnemy { get; set; }
        public float searchRadius = 40f;
        public float swarmFormationDistance = 4f;
        public float playerDetectionRadius = 30f;
        public EnemyAi1 Leader { get; set; }
        public List<EnemyAi1> Followers { get; private set; } = new List<EnemyAi1>();
        public bool IsLeader => Leader == this;
        public bool IsInGroup => Leader != null && Leader.gameObject.activeInHierarchy;
        public GameObject bulletPrefab;
        public float bulletSpeed = 10f;
        public float fireRate = 1f;
        public float fireSpread = 10f;
        public Vector2? PlayerPosition => _cachedPlayerPosition;

        private Fsm _fsm;
        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private ObjectPool<GameObject> _bulletPool;
        private GameObject _cachedPlayer;
        private Vector2? _cachedPlayerPosition;
        private float _playerCheckTimer;
        private float _enemyCheckTimer;
        private readonly List<EnemyAi1> _cachedEnemies = new List<EnemyAi1>();
        private float _fireTimer;
        private const float PlayerCheckInterval = 0.5f;
        private const float EnemyCheckInterval = 1f;

        private readonly Dictionary<GameObject, Rigidbody2D> _bulletsRbs = new Dictionary<GameObject, Rigidbody2D>();

        private void Awake()
        {
            if (bulletPrefab == null)
            {
                Debug.LogError("BulletPrefab is not assigned! Please assign it in the Inspector.", this);
                enabled = false;
                return;
            }

            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _fsm = new Fsm();

            _bulletPool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject bullet = Instantiate(bulletPrefab);
                    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        _bulletsRbs.Add(bullet, rb);
                    }
                    else
                    {
                        Debug.LogError("Bullet Prefab could not be created", bullet);
                    }
                    return bullet;
                },
                actionOnGet: bullet =>
                {
                    bullet.SetActive(true);
                    bullet.transform.position = transform.position;
                },
                actionOnRelease: bullet =>
                {
                    bullet.SetActive(false);
                    if (_bulletsRbs.ContainsKey(bullet))
                    {
                        _bulletsRbs[bullet].linearVelocity = Vector2.zero;
                    }
                },
                actionOnDestroy: bullet =>
                {
                    _bulletsRbs.Remove(bullet);
                    Destroy(bullet);
                },
                maxSize: 50
            );

            _fsm.AddState(new FsmStateRandomWalking(_fsm, transform, _rb, this));
            _fsm.AddState(new FsmStateSwarm(_fsm, transform, _rb, this));
            _fsm.AddState(new FsmStatePursue(_fsm, transform, _rb, this));
            _fsm.AddState(new FsmStateFlee(_fsm, transform, _rb, this));
            _fsm.SetState<FsmStateRandomWalking>();
        }

        private void Update()
        {
            if (Leader != null && !Leader.gameObject.activeInHierarchy)
            {
                Leader = null;
                _fsm.SetState<FsmStateFlee>();
            }

            _fsm.Update();
            UpdateColor();
            UpdatePlayerPosition();
        }

        private void FixedUpdate()
        {
            _fsm.FixedUpdate();
        }

        private void UpdateColor()
        {
            _spriteRenderer.color = IsLeader ? Color.yellow : Color.white;
        }

        public List<EnemyAi1> FindNearbyEnemies()
        {
            if (Time.time - _enemyCheckTimer >= EnemyCheckInterval)
            {
                _enemyCheckTimer = Time.time;
                _cachedEnemies.Clear();
                Collider2D[] colliders =
                    Physics2D.OverlapCircleAll(transform.position, searchRadius, LayerMask.GetMask("EnemyAi1"));
                foreach (var collider in colliders)
                {
                    if (collider.gameObject == gameObject) continue;
                    EnemyAi1 enemy = collider.GetComponent<EnemyAi1>();
                    if (enemy != null && enemy.gameObject.activeInHierarchy)
                    {
                        _cachedEnemies.Add(enemy);
                    }
                }

                if (_cachedEnemies.Count == 0)
                {
                    EnemyAi1[] allEnemies = FindObjectsByType<EnemyAi1>(FindObjectsSortMode.None);
                    foreach (var enemy in allEnemies)
                    {
                        if (enemy == this || !enemy.gameObject.activeInHierarchy) continue;
                        if (Vector2.Distance(transform.position, enemy.transform.position) <= searchRadius)
                        {
                            _cachedEnemies.Add(enemy);
                        }
                    }
                }
            }
            return _cachedEnemies;
        }

        public GameObject FindPlayer()
        {
            if (Time.time - _playerCheckTimer >= PlayerCheckInterval)
            {
                _playerCheckTimer = Time.time;
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, playerDetectionRadius,
                    LayerMask.GetMask("Player"));
                foreach (var collider in colliders)
                {
                    if (collider.CompareTag("Player"))
                    {
                        _cachedPlayer = collider.gameObject;
                        _cachedPlayerPosition = _cachedPlayer.transform.position;
                        return _cachedPlayer;
                    }
                }

                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null && Vector2.Distance(transform.position, player.transform.position) <=
                    playerDetectionRadius)
                {
                    _cachedPlayer = player;
                    _cachedPlayerPosition = player.transform.position;
                    return _cachedPlayer;
                }

                _cachedPlayer = null;
                _cachedPlayerPosition = null;
            }
            return _cachedPlayer;
        }

        public void UpdatePlayerPosition()
        {
            if (_cachedPlayer != null && _cachedPlayer.activeInHierarchy)
            {
                _cachedPlayerPosition = _cachedPlayer.transform.position;
            }
            else if (_cachedPlayer != null && !_cachedPlayer.activeInHierarchy)
            {
                _cachedPlayer = null;
                _cachedPlayerPosition = null;
            }
        }

        public void JoinGroup(EnemyAi1 newLeader)
        {
            if (Leader != null && Leader != this && Leader.gameObject.activeInHierarchy)
            {
                Leader.Followers.Remove(this);
                GameManager.Instance.UnregisterLeader(this);
            }

            Leader = newLeader;
            if (newLeader != null && newLeader != this && newLeader.gameObject.activeInHierarchy)
            {
                newLeader.Followers.Add(this);
                GameManager.Instance.RegisterLeader(newLeader);
                _fsm.SetState<FsmStateSwarm>();
            }
            else if (newLeader == this)
            {
                GameManager.Instance.RegisterLeader(this);
                _fsm.SetState<FsmStatePursue>();
            }
            else
            {
                _fsm.SetState<FsmStateFlee>();
            }
        }

        public void ShootAtPlayer(Vector2 playerPos)
        {
            _fireTimer += Time.deltaTime;
            if (_fireTimer < 1f / fireRate)
            {
                return;
            }

            _fireTimer = 0f;
            Vector2 direction = (playerPos - (Vector2)transform.position).normalized;
            float randomAngle = Random.Range(-fireSpread, fireSpread);
            Quaternion rotation =
                Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + randomAngle);

            GameObject bullet = _bulletPool.Get();
            if (bullet == null || !_bulletsRbs.ContainsKey(bullet))
            {
                Debug.LogWarning("Bullet could not be created or Rigidbody2D not found.", bullet);
                if (bullet != null) _bulletPool.Release(bullet);
                return;
            }

            bullet.transform.rotation = rotation;
            Rigidbody2D bulletRb = _bulletsRbs[bullet];
            bulletRb.linearVelocity = rotation * Vector2.right * bulletSpeed;
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
                        follower._fsm.SetState<FsmStateFlee>();
                    }
                }
                Followers.Clear();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.UnregisterLeader(this);
                }
            }
            else if (Leader != null && Leader.gameObject.activeInHierarchy)
            {
                Leader.Followers.Remove(this);
            }

            _bulletPool?.Dispose();
            _bulletsRbs.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, searchRadius);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);

            if (IsLeader)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, swarmFormationDistance);
            }
        }
    }
}