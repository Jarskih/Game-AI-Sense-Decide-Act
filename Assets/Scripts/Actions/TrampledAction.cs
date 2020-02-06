namespace FlatEarth
{
    public class TrampledAction : Action
    {
        public TrampledAction(int pPriority)
        {
            base.priority = pPriority;
        }
        
        public override bool CanDoAction(CurrentState state)
        {
            return state.GetState("isTrampled");
        }

        public override void Act(Entity entity)
        {
        }
    }
}