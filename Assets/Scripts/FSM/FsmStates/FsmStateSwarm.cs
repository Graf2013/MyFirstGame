using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class FsmStateSwarm : FsmState
{
    private readonly Transform transform;
    private readonly Rigidbody2D rb;
    private readonly EnemyAi1 enemy;
    private float _speed = 5f;
    private float _orbitAngle;
    private float _orbitSpeed = 2f;
    private Vector2 _randomOffset;
    private float _offsetTimer;
    private const float OffsetUpdateInterval = 2f;

    public FsmStateSwarm(Fsm fsm, Transform transform, Rigidbody2D rb, EnemyAi1 enemy) : base(fsm)
    {
        this.transform = transform;
        this.rb = rb;
        this.enemy = enemy;
    }

    public override void Enter()
    {
        _orbitAngle = Random.Range(0f, 360f);
        UpdateRandomOffset();
    }

    public override void Update()
    {
        if (enemy.Leader == null || enemy.Leader == enemy || !enemy.Leader.gameObject.activeInHierarchy)
        {
            enemy.Leader = null;
            Fsm.SetState<FsmStateRandomWalking>();
            return;
        }

        _offsetTimer += Time.deltaTime;
        if (_offsetTimer >= OffsetUpdateInterval)
        {
            UpdateRandomOffset();
            _offsetTimer = 0f;
        }

        GameObject player = enemy.FindPlayer();
        if (player != null)
        {
            enemy.ShootAtPlayer(player.transform.position);
        }

        Vector2 leaderPos = enemy.Leader.transform.position;
        float angleRad = _orbitAngle * Mathf.Deg2Rad;
        Vector2 basePos = leaderPos + new Vector2(
            Mathf.Cos(angleRad) * enemy.SwarmFormationDistance,
            Mathf.Sin(angleRad) * enemy.SwarmFormationDistance
        );

        Vector2 desiredPos = basePos + _randomOffset;
        Vector2 direction = (desiredPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * _speed;

        _orbitAngle += _orbitSpeed * Time.deltaTime;
        if (_orbitAngle >= 360f)
            _orbitAngle -= 360f;

        if (enemy.Leader.Followers.Count >= 9 && !enemy.IsLeader)
        {
            enemy.JoinGroup(null);
            Fsm.SetState<FsmStateRandomWalking>();
        }
    }

    private void UpdateRandomOffset()
    {
        float offsetMagnitude = enemy.SwarmFormationDistance * 0.3f;
        _randomOffset = Random.insideUnitCircle * offsetMagnitude;
    }

    public override void Exit() { }
}
