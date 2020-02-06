namespace FlatEarth
{
    public class SpreadAction : Action
    {
        public SpreadAction(int pPriority)
        {
            base.priority = pPriority;
        }

        public override bool CanDoAction(CurrentState state)
        {
            if(state.GetState("isHealthy") && state.GetState("isReadyToSpread"))
            {
                return true;
            }

            return false;
        }

        public override void Act(Entity entity)
        {
            entity.Breed();
        }
    }
}