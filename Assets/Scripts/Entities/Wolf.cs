using UnityEngine;

namespace FlatEarth
{
    public class Wolf : Entity
    {
        private enum State
        {
            DEAD,
            LOOKING_FOR_FOOD,
            FLEEING,
            WANDERING,
            BREEDING,
        }
        
        private readonly EntityType type = EntityType.WOLF;
        private int _id;
        private Grid _grid;
        [SerializeField] private State _state;
        [SerializeField] private Node _currentNode;
        [SerializeField] private Node _oldNode;
        [SerializeField] private int _sensingRadius = 5;
        private readonly bool[] _urges;
        
        // Reproduction
        [SerializeField] private double _maxHealth = 100;
        [SerializeField] private float _health = 50;

        // Hunger

        [SerializeField] private float _hungerSpeed = 2;
        [SerializeField] private float _starveSpeed = 2;
        [SerializeField] private float _recoverSpeed;
        
        // Wandering
        private readonly int[] _wanderAngles = {-15, -10, 5, 0, 0, 5, 10, 15};
        [SerializeField] private float _maxHunger = 100;
        [SerializeField] private float _hungerLimit = 70;
        [SerializeField] private float _hunger = 0;
        [SerializeField] private Vector3 _targetPos;
        private float _walkSpeed = 0.02f;
 
        // urges
        private bool _isInDanger;
        private bool _isHungry;
        private bool _isHealthy;
        [SerializeField] private Vector3 _foodLocation;

        public void Init(Grid grid)
        {
            _id = gameObject.GetInstanceID();
            EventManager.StartListening("SheepEaten", Eaten);
            _grid = grid;
            _currentNode = _grid.GetNodeCenterFromWorldPos(transform.position);
            _currentNode.AddEntity(this);
            _oldNode = _currentNode;
        }

        public override EntityType GetEntityType()
        {
            return type;
        }
        public override int GetId()
        {
            return _id;
        }

        public override void Sense()
        {
            // Increment counters
            _hunger += Time.deltaTime * _hungerSpeed;
         
            // Is hungry?
            _isHungry = _hunger > _hungerLimit;
            
            // Wants to reproduce?
            _isHealthy = _health >= _maxHealth;
            
            // If starving lose health otherwise get stronger
            if (_hunger > _maxHunger)
            {
                _health -= Time.deltaTime * _starveSpeed;
            }
            else
            {
                _health += Time.deltaTime * _recoverSpeed;
            }

            // Starved to death
            if (_health < 0)
            {
                Die();
            }

            _currentNode = _grid.GetNodeCenterFromWorldPos(transform.position);
            
            if (_currentNode != null && _currentNode != _oldNode)
            {
                _currentNode.AddEntity(this);
                _oldNode.RemoveEntity(this);
                _oldNode = _currentNode;
            }

            //TODO find wolves
        }

        public override void Think()
        {
            // Default to wandering
            _state = State.WANDERING;

            // Priorities 
            // 1. Look for food
            if (_isHungry)
            {
                // TODO find where closest food is
                if (_foodLocation == Vector3.zero)
                {
                    _foodLocation = EntityManager.GetClosestSheepPos(_currentNode, _sensingRadius);
                }
                _state = State.LOOKING_FOR_FOOD;
                return;
            }
            // 2. Reproduce
            if (_isHealthy)
            {
                // TODO find where closest food is
                _state = State.BREEDING;
                return;
            }
            // 3. Wander
            FindWanderTarget();
        }
        
        public override void Act()
        {
            switch (_state) 
            {
                case State.DEAD:
                    break;
                case State.LOOKING_FOR_FOOD:
                    Eat();
                    break;
                case State.WANDERING:
                    Wander();
                    break;
                case State.FLEEING:
                    Flee();
                    break;
            }
        }

        private void Flee()
        {
        }

        private void FindWanderTarget()
        {
            int angle = 0;
            float maxTurningDelta = 1; // in degrees
            bool isOutsideGrid = _grid.IsOutsideGrid(transform.position + transform.forward*1);
                if (isOutsideGrid)
                { 
                    // We hit the end of the grid. Turn around
                    angle = 180;
                    maxTurningDelta = 180;
                }
                else
                {
                    // Randomly turn left or right or continue straight
                    angle = _wanderAngles[Random.Range(0, _wanderAngles.Length)];
                    maxTurningDelta = 1;
                }

                Quaternion rotateDir = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + angle, transform.rotation.eulerAngles.z);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateDir, maxTurningDelta);
                _targetPos = transform.position + transform.forward * 3;
        }

        private void Wander()
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, _walkSpeed);
        }

        private void Eat()
        {
            // Find grass
            bool hasFood = false;
            int id = 0;
            var entities = _grid.GetEntitiesOnNode(_currentNode);
            if (entities.Count > 0)
            {
                foreach (var e in entities)
                {
                    if (e.GetEntityType() == EntityType.SHEEP)
                    {
                        hasFood = true;
                        id = e.GetId();
                    }
                }
            }

            if (hasFood)
            {
                _foodLocation = Vector3.zero;
                _hunger = 0;
                EventManager.EventMessage message = new EventManager.EventMessage(_currentNode, id);
                EventManager.TriggerEvent("EntityDied", message);  
            }
            else
            {
                float maxTurningDelta = 15;
                Quaternion lookAt = Quaternion.LookRotation(_foodLocation - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAt, maxTurningDelta);
                transform.position = Vector3.MoveTowards(transform.position, _foodLocation, _walkSpeed);
            }
        }

        private void Eaten(EventManager.EventMessage message)
        {
            if (_id == message.id)
            {
                Die();
            }
        }

        private void Die()
        {
            _health = 0;
            _state = State.DEAD;
            EventManager.EventMessage message = new EventManager.EventMessage(_currentNode, _id);
            EventManager.TriggerEvent("EntityDied", message);
        }
    }
}