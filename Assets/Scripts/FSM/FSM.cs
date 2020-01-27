using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlatEarth
{
public class FSM
{
        private Stack<FSMState> _states = new Stack<FSMState>();

        public delegate void FSMState(GameObject gameObject, FSM state);
        
        public void Update(GameObject gameObject) {
            if (_states.Peek() != null)
            {
                _states.Peek().Invoke(gameObject, this);
            }
       	}

        public void PushState(FSMState state)
        {
            _states.Push(state);
        }

        public void PopState()
        {
            _states.Pop();
        }
}
}