using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAi1 : MonoBehaviour
{
    public EnemyAi1 TargetEnemy { get; set; }
    public float SearchRadius = 5f;
    public float SwarmFormationDistance = 4f;
    public float PlayerDetectionRadius = 15f; // Збільшено для надійності
    public EnemyAi1 Leader { get; set; }
    public List<EnemyAi1> Followers { get; private set; } = new List<EnemyAi1>();
    public bool IsLeader => Leader == this;
    public bool IsInGroup => Leader != null && Leader.gameObject.activeInHierarchy;
    public GameObject BulletPrefab;
    public float BulletSpeed = 10f;
    public float FireRate = 1f;
    public float FireSpread = 10f;

    private Fsm _fsm;
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private float _fireTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _fsm = new Fsm();

        _fsm.AddState(new FsmStateRandomWalking(_fsm, transform, _rb, this));
        _fsm.AddState(new FsmStateSwarm(_fsm, transform, _rb, this));
        _fsm.AddState(new FsmStatePursue(_fsm, transform, _rb, this));

        _fsm.SetState<FsmStateRandomWalking>();
    }

    private void Update()
    {
        if (Leader != null && !Leader.gameObject.activeInHierarchy)
        {
            Leader = null; // Очистити лідера, якщо він знищений
            _fsm.SetState<FsmStateRandomWalking>();
        }
        _fsm.Update();
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = IsLeader ? Color.yellow : Color.white;
        }
    }

    public List<EnemyAi1> FindNearbyEnemies()
    {
        List<EnemyAi1> nearbyEnemies = new List<EnemyAi1>();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, SearchRadius);

        foreach (var collider in colliders)
        {
            if (collider.gameObject == gameObject)
                continue;

            EnemyAi1 enemy = collider.GetComponent<EnemyAi1>();
            if (enemy != null && enemy.gameObject.activeInHierarchy)
                nearbyEnemies.Add(enemy);
        }
        return nearbyEnemies;
    }

    public GameObject FindPlayer()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, PlayerDetectionRadius);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return collider.gameObject;
            }
        }
        return null;
    }

    public void JoinGroup(EnemyAi1 newLeader)
    {
        if (Leader != null && Leader != this && Leader.gameObject.activeInHierarchy)
            Leader.Followers.Remove(this);

        Leader = newLeader;
        if (newLeader != null && newLeader != this && newLeader.gameObject.activeInHierarchy)
            newLeader.Followers.Add(this);

        _fsm.SetState<FsmStateSwarm>();
    }

    public void ShootAtPlayer(Vector2 playerPos)
    {
        if (BulletPrefab == null)
        {
            return;
        }

        _fireTimer += Time.deltaTime;
        if (_fireTimer < 1f / FireRate)
            return;

        _fireTimer = 0f;
        Vector2 direction = (playerPos - (Vector2)transform.position).normalized;
        float randomAngle = Random.Range(-FireSpread, FireSpread);
        Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + randomAngle);

        GameObject bullet = GameObject.Instantiate(BulletPrefab, transform.position, rotation);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = rotation * Vector2.right * BulletSpeed;
        }

    }

    private void OnDestroy()
    {
        if (IsLeader)
        {
            foreach (var follower in Followers)
            {
                if (follower != null)
                    follower.Leader = null;
            }
            Followers.Clear();
        }
        else if (Leader != null && Leader.gameObject.activeInHierarchy)
        {
            Leader.Followers.Remove(this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, SearchRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, PlayerDetectionRadius);

        if (IsLeader)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, SwarmFormationDistance);
        }
    }
}
