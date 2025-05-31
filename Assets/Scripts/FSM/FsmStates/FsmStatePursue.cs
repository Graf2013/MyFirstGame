using UnityEngine;

public class FsmStatePursue : FsmState
{
    private readonly Transform transform;
    private readonly Rigidbody2D rb;
    private readonly EnemyAi1 enemy;
    private float _speed = 5f;
    private float _sineAmplitude = 2f;
    private float _sineFrequency = 1f;
    private float _time;

    public FsmStatePursue(Fsm fsm, Transform transform, Rigidbody2D rb, EnemyAi1 enemy) : base(fsm)
    {
        this.transform = transform;
        this.rb = rb;
        this.enemy = enemy;
    }

    public override void Enter()
    {
        _time = 0f;
    }

    public override void Update()
    {
        GameObject player = enemy.FindPlayer();
        if (player == null)
        {
            Fsm.SetState<FsmStateRandomWalking>();
            return;
        }

        Vector2 playerPos = player.transform.position;
        enemy.ShootAtPlayer(playerPos);

        _time += Time.deltaTime;
        Vector2 directionToPlayer = (playerPos - (Vector2)transform.position).normalized;
        Vector2 perpendicular = new Vector2(-directionToPlayer.y, directionToPlayer.x);
        Vector2 sineOffset = perpendicular * Mathf.Sin(_time * _sineFrequency) * _sineAmplitude;
        Vector2 desiredPos = (Vector2)transform.position + directionToPlayer * _speed * Time.deltaTime + sineOffset;

        rb.linearVelocity = (desiredPos - (Vector2)transform.position).normalized * _speed;
    }

    public override void Exit() { }
}