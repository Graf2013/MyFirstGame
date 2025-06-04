using Enemy.FSM;
using Enemy.FSM.FsmStates;
using UnityEngine;

namespace Enemy
{
    public class AIController : MonoBehaviour
    {
        private Fsm _fsm;
        private Rigidbody2D _rigidbody;
        [SerializeField] private float speed;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }
    
        private void Start()
        {
            _fsm = new Fsm();
        
            _fsm.AddState(new FsmStateWait(_fsm));
            _fsm.AddState(new FsmStateWalk(_fsm, _rigidbody, speed));
        
            _fsm.SetState<FsmStateWait>();
        }

        private void Update()
        {
            _fsm.Update();
        }
    }
}