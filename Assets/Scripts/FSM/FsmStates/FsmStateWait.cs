using UnityEngine;

namespace FSM.FsmStates
{
    public class FsmStateWait : FsmState
    {
        private float time = 0;
        public FsmStateWait(Fsm fsm) : base(fsm) { }

        public override void Enter()
        {

        }

        public override void Update()
        {

            time += Time.deltaTime;
            if (time > 2)
            {
                time = 0;
                Fsm.SetState<FsmStateWalk>();
            }
        }

        public override void Exit()
        {

        }
    }
}