using FSM.FsmStates;
using UnityEngine;

    public class FsmStateWalk : FsmState
    {
        protected readonly Rigidbody2D rigidbody;
        protected readonly float speed;

        public FsmStateWalk(Fsm fsm, Rigidbody2D rigidbody, float speed) : base(fsm)
        {
            this.rigidbody = rigidbody;
            this.speed = speed;
        }

        private Vector2 randomVector = Vector2.zero;
        private float timer = 0f;

        public override void Enter()
        {

        }


        public override void Update()
            {
                RandomMove(rigidbody, speed);

            }

        public override void Exit()
        {

        }


        private void RandomMove(Rigidbody2D rigidbody, float speed)
            {
                if (timer <= 2)
                {
                     timer += Time.deltaTime;
                     rigidbody.linearVelocity = randomVector * speed * Time.deltaTime;
                }
                else if(timer >= 2)
                {
                    randomVector = new Vector2(Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f));
                    Fsm.SetState<FsmStateWait>();
                    timer = 0;
                    rigidbody.linearVelocity = Vector2.zero;
                    
                }
                
            }
    }