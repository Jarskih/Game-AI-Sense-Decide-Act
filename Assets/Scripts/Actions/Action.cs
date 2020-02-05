using UnityEngine;

namespace FlatEarth
{
    public abstract class Action
    {
        public float priority;
        public abstract bool CanDoAction(CurrentState state);
        public abstract void Act(Entity entity);

        public void UpdatePriority(float urge)
        {
            priority *= urge/100;
            Mathf.Clamp(priority, 0, 1);
        }
    }
}