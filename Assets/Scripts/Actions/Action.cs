namespace FlatEarth
{
    public abstract class Action
    {
        public int priority;
        public abstract bool CanDoAction(CurrentState state);
        public abstract void Act(Entity entity);
    }
}