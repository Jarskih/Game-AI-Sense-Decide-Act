using System.Diagnostics;

namespace FlatEarth
{
    public class GrowAction : Action
    {
        public GrowAction(int pPriority)
        {
            base.priority = pPriority;
        }
        
        public override bool CanDoAction(CurrentState state)
        {
            return true;
        }

        public override void Act(Entity entity)
        {
            entity.Grow();
        }
    }
}