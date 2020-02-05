using UnityEngine;

namespace FlatEarth
{
    public abstract class Action
    {
        public float priority;
        public abstract bool CanDoAction(CurrentState state);
        public abstract void Act(Entity entity);
    }
}