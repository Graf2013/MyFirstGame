using UnityEngine;

namespace Enemy.FSM.FsmStates
{
    public class FsmStateWalk : FsmState
    {
        private readonly Rigidbody2D _rigidbody;
        private readonly float _speed;

        public FsmStateWalk(Fsm fsm, Rigidbody2D rigidbody, float speed) : base(fsm)
        {
            this._rigidbody = rigidbody;
            this._speed = speed;
        }

        private Vector2 _randomVector = Vector2.zero;
        private float _timer;

        public override void Enter()
        {
        }


        public override void Update()
        {
            RandomMove(_rigidbody, _speed);
        }

        public override void Exit()
        {
        }


        private void RandomMove(Rigidbody2D rigidbody, float speed)
        {
            if (_timer <= 2)
            {
                _timer += Time.deltaTime;
                rigidbody.linearVelocity = _randomVector * (speed * Time.deltaTime);
            }
            else if (_timer >= 2)
            {
                _randomVector = new Vector2(Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f));
                Fsm.SetState<FsmStateWait>();
                _timer = 0;
                rigidbody.linearVelocity = Vector2.zero;
            }
        }
    }
}