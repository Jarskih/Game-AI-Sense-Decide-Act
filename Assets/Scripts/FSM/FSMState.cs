using UnityEngine;

namespace FlatEarth
{
    public interface FSMState
    {
        void Update(GameObject gameObject, FSM state);
    }
}
