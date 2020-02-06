namespace FlatEarth
{
    public class IdleAction : Action
    {
        public IdleAction(int pPriority)
        {
            base.priority = pPriority;
        }

        public override bool CanDoAction(CurrentState state)
        {
            return true;
        }

        public override void Act(Entity entity)
        {
        }
    }
}