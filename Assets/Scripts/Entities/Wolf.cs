using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlatEarth
{
    public class Wolf : Entity
    {
        private Stats _stats;
        public Stats stats => _stats;
        
        // internal state
        private float _maxHunger = 100;
        [SerializeField] protected float _hunger = 0;
        [SerializeField] private float _health = 1;
        
        private readonly EntityType type = EntityType.WOLF;
        private int _id;
        private Grid _grid;

        [SerializeField] private Node _currentNode;
        [SerializeField] private Node _oldNode;
       
        // Hunger
        [SerializeField] private float _hungerSpeed = 2;
        [SerializeField] private float _starveSpeed = 2;
        [SerializeField] private float _recoverSpeed = 2;
        [SerializeField] private Dictionary<Entity, float> _foodNear = new Dictionary<Entity, float>();
        
        // Wandering
        private readonly int[] _wanderAngles = {-15, -10, 5, 0, 0, 5, 10, 15};
        [SerializeField] private Vector3 _targetPos;
        [SerializeField] private Vector3 _foodLocation;
        [SerializeField] private Entity _food;

        public void Init(Grid grid)
        {
            _id = gameObject.GetInstanceID();
            _grid = grid;
            _currentNode = _grid.GetNodeCenterFromWorldPos(transform.position);
            _oldNode = _currentNode;
            
            // Add actions this entity can perform and assign priorities for actions
            _availableActions.Add(new WanderAction(2));
            _availableActions.Add(new EatAction(1));
            
            // init stats
            _stats.hungerLimit = 70;
            _stats.walkSpeed = 0.05f;
            _stats.runSpeed = 0.1f;
            _stats.slowTurnSpeed = 1;
            _stats.fastTurnSpeed = 3;
            _stats.maxHealth = 100;
            _stats.sensingRadius = 5;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position,_stats.sensingRadius);
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
            _currentState.UpdateState("isHungry", _hunger > stats.hungerLimit);
            _currentState.UpdateState("isAfraid", false);
            _currentState.UpdateState("sawFood", _food != null);
            
            _currentNode = _grid.GetNodeCenterFromWorldPos(transform.position);
            
            if (_currentNode != null && _currentNode != _oldNode)
            {
                _currentNode.AddEntity(this);
                _oldNode.RemoveEntity(this);
                _oldNode = _currentNode;
            }
            
            _foodNear = EntityManager.FindEntityAround(transform.position, stats.sensingRadius, EntityType.SHEEP);
            if (_foodNear.Count > 0)
            {
                var ordered = _foodNear.OrderByDescending(x => x.Value).ToList();
                _food = ordered[0].Key;
            }
        }

        public override void Think()
        {
            // Get actions entity can do
           _currentAction = FindBestAction(_currentState);

           if (_currentAction == null)
           {
               Debug.LogError("No action available");
           }

          /*
            // Default to wandering
            _state = State.WANDERING;

            // Priorities 
            // 1. Look for food
            if (_hunger > _hungerLimit)
            {
                if (_prey == null)
                {
                    Dictionary<Entity, float> _foodNear = EntityManager.FindEntityAround(transform.position, _sensingRadius, EntityType.SHEEP);
                    var ordered = _foodNear.OrderByDescending(x => x.Value).ToList();
                    _prey = ordered[0].Key;
                }
                _state = State.LOOKING_FOR_FOOD;
                return;
            }
            // 2. Reproduce
            if (_health >= _maxHealth)
            {
                // TODO find where closest food is
                _state = State.BREEDING;
                return;
            }
            // 3. Wander
            FindWanderTarget();
            */
        }

        public override void Act()
        {
            // Increment counters
            _hunger += Time.deltaTime * _hungerSpeed;
            
            // If starving lose health otherwise get stronger
            if (_hunger > _maxHunger)
            {
                // _health -= Time.deltaTime * _starveSpeed;
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

            _currentAction.Act(this);

            /*
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
            
            Mathf.Clamp(transform.position.x, 0, 25);
            Mathf.Clamp(transform.position.z, 0, 25);
            
            */
        }

        public override Dictionary<Entity, float> FindFood()
        {
            return _foodNear;
        }

        public override void Flee()
        {
        }


        public override void Wander()
        {
            _targetPos = GetWanderPos();
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, stats.walkSpeed);
        }

        public override void Eat()
        {
            if (_food == null)
            {
                Wander();
                return;
            }

            if (Vector3.Distance(transform.position, _food.transform.position) < 0.5f)
            {
                _hunger = 0;
                EventManager.EventMessage message = new EventManager.EventMessage(_food.GetId());
                EventManager.TriggerEvent("EntityDied", message);
            }
            else
            {
                float maxTurningDelta = 15;
                _foodLocation = _food.transform.position;
                Quaternion lookAt = Quaternion.LookRotation(_foodLocation - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAt, maxTurningDelta);
                transform.position = Vector3.MoveTowards(transform.position, _foodLocation, stats.runSpeed);
            }
        }

        private void Die()
        {
            _health = 0;
            EventManager.EventMessage message = new EventManager.EventMessage(_id);
            EventManager.TriggerEvent("EntityDied", message);
        }

        private Vector3 GetWanderPos()
        {
            int angle = 0;
            float maxTurningDelta = 1; // in degrees
            var nextPos = transform.position + transform.forward * 1;
            if (_grid.IsOutsideGrid(nextPos))
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

            return transform.position + transform.forward * 1;
        }
    }
}