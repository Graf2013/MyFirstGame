using UnityEngine;
using System.Collections.Generic;

public class FsmStateRandomWalking : FsmState
{
    private readonly Transform transform;
    private readonly Rigidbody2D rb;
    private readonly EnemyAi1 enemy;
    private float _radius = 5f;
    private float _speed = 5f;
    private Vector2 _targetPosition;
    private float _checkGroupTimer = 0f;
    private const float CheckGroupInterval = 1f;

    public FsmStateRandomWalking(Fsm fsm, Transform transform, Rigidbody2D rb, EnemyAi1 enemy) : base(fsm)
    {
        this.transform = transform;
        this.rb = rb;
        this.enemy = enemy;
    }

    public override void Enter()
    {
        CreateRandomTargetInCircle();
    }

    public override void Update()
    {
        MoveToTarget();
        CheckForGroup();
        CheckForPlayer();
    }

    public override void Exit() { }

    private void MoveToTarget()
    {
        Vector2 direction = (_targetPosition - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * _speed;

        if (Vector2.Distance(transform.position, _targetPosition) <= 0.1f)
            CreateRandomTargetInCircle();
    }

    private void CreateRandomTargetInCircle()
    {
        Vector2 randomDirection = Random.insideUnitCircle * _radius;
        _targetPosition = (Vector2)transform.position + randomDirection;
    }

    private void CheckForGroup()
    {
        _checkGroupTimer += Time.deltaTime;
        if (_checkGroupTimer < CheckGroupInterval)
            return;

        _checkGroupTimer = 0f;
        var nearbyEnemies = enemy.FindNearbyEnemies();
        if (nearbyEnemies.Count == 0)
            return;

        EnemyAi1 bestLeader = null;
        int maxFollowers = -1;

        foreach (var nearbyEnemy in nearbyEnemies)
        {
            if (nearbyEnemy.IsLeader && nearbyEnemy.Followers.Count < 9)
            {
                int followerCount = nearbyEnemy.Followers.Count;
                if (followerCount > maxFollowers)
                {
                    maxFollowers = followerCount;
                    bestLeader = nearbyEnemy;
                }
            }
        }

        if (bestLeader != null)
        {
            enemy.JoinGroup(bestLeader);
        }
        else
        {
            if (!enemy.IsInGroup)
            {
                enemy.JoinGroup(enemy);
                foreach (var nearbyEnemy in nearbyEnemies)
                {
                    if (!nearbyEnemy.IsInGroup && enemy.Followers.Count < 9)
                        nearbyEnemy.JoinGroup(enemy);
                }
            }
        }
    }

    private void CheckForPlayer()
    {
        GameObject player = enemy.FindPlayer();
        if (player != null && enemy.IsLeader)
        {
            Fsm.SetState<FsmStatePursue>();
        }
    }
}
