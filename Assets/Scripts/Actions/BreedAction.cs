namespace FlatEarth
{
    public class BreedAction : Action
    {
        public override bool CanDoAction(CurrentState state)
        {
            return state.GetState("isMature");
        }

        public override void Act(Entity entity)
        {
            entity.Breed();
        }
    }
}