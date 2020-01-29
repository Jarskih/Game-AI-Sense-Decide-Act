using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using TMPro.EditorUtilities;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace FlatEarth
{
    public class Sheep : Entity
    {
        // GOAP
        private GoapAgent _agent;

        private enum State
        {
            DEAD,
            LOOKING_FOR_FOOD,
            FLEEING,
            WANDERING,
            BREEDING,
        }
        
        private readonly EntityType type = EntityType.SHEEP;
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
        [SerializeField] private float _starveSpeed = 5;
        [SerializeField] private float _recoverSpeed;
        
        // Wandering
        private readonly int[] _wanderAngles = {-15, -10, 5, 0, 0, 5, 10, 15};
        [SerializeField] private float _hungerLimit = 30;
        [SerializeField] private Vector3 _targetPos;
        private float _walkSpeed = 0.05f;
        private float _runSpeed = 0.1f;
 
        // urges
        private bool _isInDanger;
        [SerializeField] private Vector3 _foodLocation;

        [SerializeField] private Dictionary<Entity, float> _foodNear = new Dictionary<Entity, float>();
        [SerializeField] private Dictionary<Entity, float> _threatNear = new Dictionary<Entity, float>();

        public void Init(Grid grid)
        {

            _id = gameObject.GetInstanceID();
            EventManager.StartListening("SheepEaten", Eaten);
            _grid = grid;
            _currentNode = _grid.GetNodeCenterFromWorldPos(transform.position);
            _oldNode = _currentNode;

            // GOAP actions
             gameObject.AddComponent<WanderAction>();
            gameObject.AddComponent<EatFoodAction>();
            _agent = gameObject.AddComponent<GoapAgent>();
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
                return;
            }
            
            _currentNode = _grid.GetNodeCenterFromWorldPos(transform.position);
            
            if (_currentNode != null && _currentNode != _oldNode)
            {
                _currentNode.AddEntity(this);
                _oldNode.RemoveEntity(this);
                _oldNode = _currentNode;
            }
            
            // Sense nearby entities
            _threatNear = EntityManager.FindEntityAround(transform.position, _sensingRadius, EntityType.WOLF);
            _foodNear = EntityManager.FindEntityAround(transform.position, _sensingRadius, EntityType.GRASS);
        }

        public override void Think()
        {
            return;
            // Default to wandering
            _state = State.WANDERING;

            // Priorities 
            // 1. Run from danger
            if(_threatNear.Count > 0)
            {
                // TODO find flee direction
                _state = State.FLEEING;
                return;
            }
            // 2. Look for food
            if (_hunger > _hungerLimit)
            {
                
                // Sort the dictionary to get the highest priority item
                var ordered = _foodNear.OrderByDescending(x => x.Value).ToList();
                if (ordered.Count > 0)
                {
                    var food = ordered[0].Key;
                    _foodLocation = food.transform.position;
                }
                _state = State.LOOKING_FOR_FOOD;
                return;
            }
            // 3. Reproduce
            if (_health >= _maxHealth)
            {
                // TODO find where closest food is
                _state = State.BREEDING;
                return;
            }
            // 4. Wander
            FindWanderTarget();
        }
        
        public override void Act()
        {
            return;
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
        }

        public override Entity FindFood()
        {
            _foodNear = EntityManager.FindEntityAround(transform.position, _sensingRadius, EntityType.GRASS);
            if (_foodNear.Count > 0)
            {
                var ordered = _foodNear.OrderByDescending(x => x.Value).ToList();
                return ordered[0].Key;
            }
            return null;
        }
        
        private void Flee()
        {
          Quaternion lookAt = Quaternion.identity;
          float maxTurningDelta = 10;
          var direction = Vector3.zero;

          foreach (var wolf in _threatNear)
          {
              direction += transform.position - wolf.Key.transform.position;
          }
         
          _targetPos = transform.position + transform.forward * 1;
          
          if (_grid.IsOutsideGrid(_targetPos))
          { 
              // We hit the end of the grid. Turn around
              var angle = 180;
              lookAt = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + angle, transform.rotation.eulerAngles.z);
              maxTurningDelta = 180;
          }
          else
          {
              // Randomly turn left or right or continue straight
              lookAt = Quaternion.LookRotation(direction.normalized);
              maxTurningDelta = 1;
          }
          

          transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAt, maxTurningDelta);
          transform.position = Vector3.MoveTowards(transform.position, _targetPos, _runSpeed);
        }

        private void FindWanderTarget()
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
            _targetPos = transform.position + transform.forward * 1;
        }
        private void Wander()
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, _walkSpeed);
        }

        private void Eat()
        {
            // Find grass
            bool hasGrass = false;
            int id = 0;
            var entities = _grid.GetEntitiesOnNode(_currentNode);
            if (entities.Count > 0)
            {
                foreach (var e in entities)
                {
                    if (e.GetEntityType() == EntityType.GRASS)
                    {
                        hasGrass = true;
                        id = e.GetId();
                    }
                }
            }

            if (hasGrass)
            {
                _foodLocation = Vector3.zero;
                _hunger = 0;
                EventManager.EventMessage message = new EventManager.EventMessage(id);
                EventManager.TriggerEvent("RemoveDied", message);  
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
            EventManager.EventMessage message = new EventManager.EventMessage(_id);
            EventManager.TriggerEvent("EntityDied", message);
        }

        // GOAP

        public override HashSet<KeyValuePair<string, object>> createGoalState()
        {
            HashSet<KeyValuePair<string, object>> goal = new HashSet<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("isHungry", false),
                new KeyValuePair<string, object>("foundFood", false),
                //    new KeyValuePair<string, object>("breed", true),
                //    new KeyValuePair<string, object>("flee", true),
                //    new KeyValuePair<string, object>("idle", true)
            };

            return goal;
        }
        public override bool moveAgent(GoapAction nextAction)
        {
            if (nextAction.target == null)
            {
                Wander();
                return false;
            }

            if (Vector3.Distance(gameObject.transform.position,nextAction.target.transform.position) < 0.1f) {
                // we are at the target location, we are done
                nextAction.setInRange(true);
                return true;
            }

            float maxTurningDelta = 15;
            _foodLocation = nextAction.target.transform.position;
            Quaternion lookAt = Quaternion.LookRotation(_foodLocation - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAt, maxTurningDelta);
            transform.position = Vector3.MoveTowards(transform.position, _foodLocation, _runSpeed);
            return false;
        }

        public override Vector3 GetWanderPos()
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

        public override HashSet<KeyValuePair<string, object>> getWorldState()
        {
            HashSet<KeyValuePair<string, object>> worldData = new HashSet<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("isHungry", (_hunger > _hungerLimit)),
              //  new KeyValuePair<string, object>("isScared", (_threatNear.Count > 0)),
              //  new KeyValuePair<string, object>("isIdle", _hunger < _hungerLimit && _threatNear.Count == 0)
            };
            return worldData;
        }
    }
}
