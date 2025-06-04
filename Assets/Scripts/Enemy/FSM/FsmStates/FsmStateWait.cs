using UnityEngine;

namespace Enemy.FSM.FsmStates
{
    public class FsmStateWait : FsmState
    {
        private float _time;

        public FsmStateWait(Fsm fsm) : base(fsm)
        {
        }

        public override void Enter()
        {
        }

        public override void Update()
        {
            _time += Time.deltaTime;
            if (_time > 2)
            {
                _time = 0;
                Fsm.SetState<FsmStateWalk>();
            }
        }

        public override void Exit()
        {
        }
    }
}