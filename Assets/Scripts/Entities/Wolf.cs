namespace FlatEarth
{
    public class Wolf : Entity
    {
        private readonly EntityType type = EntityType.WOLF;
        private int _health = 100;
        private int _id;
        private State _state;
        private Node _currentNode;

        private enum State
        {
            DEAD,
            EATING,
            FLEEING,
            WANDERING,
            BREEDING,
            GROWING,
            MATURE,
        }

        public void Init(Grid grid)
        {
            _id = gameObject.GetInstanceID();
        }

        public override EntityType GetEntityType()
        {
            return type;
        }
        public override int GetId()
        {
            return _id;
        }

        public override void Sense(float deltaTime)
        {
        }

        public override void Think(float deltaTime)
        {
        }

        public override void Act(float deltaTime)
        {
        }
        
        private void Die()
        {
            _health = 0;
            _state = State.DEAD;
        }
    }
}