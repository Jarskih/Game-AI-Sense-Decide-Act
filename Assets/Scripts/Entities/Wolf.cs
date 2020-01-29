using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace FlatEarth
{
    public class Wolf : Entity
    {
        private GoapAgent _agent;
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
        [SerializeField] private int _sensingRadius = 10;
        private readonly bool[] _urges;
        
        // Reproduction
        [SerializeField] private double _maxHealth = 100;
        [SerializeField] private float _health = 50;

        // Hunger
        [SerializeField] private float _hungerSpeed = 2;
        [SerializeField] private float _starveSpeed = 2;
        [SerializeField] private float _recoverSpeed;
        [SerializeField] private Dictionary<Entity, float> _foodNear = new Dictionary<Entity, float>();
        
        // Wandering
        private readonly int[] _wanderAngles = {-15, -10, 5, 0, 0, 5, 10, 15};
        [SerializeField] private float _hungerLimit = 70;
        [SerializeField] private Vector3 _targetPos;
        private float _walkSpeed = 0.04f;
        private float _runSpeed = 0.06f;
        private float _slowTurnSpeed = 1;
        private float _fastTurnSpeed = 3;

        [SerializeField] private Vector3 _foodLocation;
        [SerializeField] private Entity _prey;

        public void Init(Grid grid)
        {

            _id = gameObject.GetInstanceID();
            _grid = grid;
            _currentNode = _grid.GetNodeCenterFromWorldPos(transform.position);
            _oldNode = _currentNode;
            
            // GOAP actions
          //  gameObject.AddComponent<WanderAction>();
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
            
            _foodNear = EntityManager.FindEntityAround(transform.position, _sensingRadius, EntityType.SHEEP);
            if (_foodNear == null)
            {
                Dictionary<Entity, float> _foodNear = EntityManager.FindEntityAround(transform.position, _sensingRadius, EntityType.SHEEP);
                var ordered = _foodNear.OrderByDescending(x => x.Value).ToList();
                _prey = ordered[0].Key;
            }
        }

        public override void Think()
        {
            return;
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

        public override Dictionary<Entity, float> FindFood()
        {
            return _foodNear;
        }

        private void Flee()
        {
        }

        private void FindWanderTarget()
        {
            int angle = 0;
            float maxTurningDelta = _slowTurnSpeed; // in degrees
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
                    maxTurningDelta = _fastTurnSpeed;
                }

                Quaternion rotateDir = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + angle, transform.rotation.eulerAngles.z);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateDir, maxTurningDelta);
                _targetPos = transform.position + transform.forward * 1; // target pos 1 units ahead
                Mathf.Clamp(_targetPos.x, 0, 25);
                Mathf.Clamp(_targetPos.z, 0, 25);
        }

        private void Wander()
        {
            _targetPos = GetWanderPos();
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, _walkSpeed);
        }

        private void Eat()
        {
            if (_prey == null)
            {
                Wander();
                return;
            }

            if (Vector3.Distance(transform.position, _prey.transform.position) < 0.5f)
            {
                _hunger = 0;
                EventManager.EventMessage message = new EventManager.EventMessage(_prey.GetId());
                EventManager.TriggerEvent("EntityDied", message);
            }
            else
            {
                float maxTurningDelta = 15;
                _foodLocation = _prey.transform.position;
                Quaternion lookAt = Quaternion.LookRotation(_foodLocation - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAt, maxTurningDelta);
                transform.position = Vector3.MoveTowards(transform.position, _foodLocation, _runSpeed);
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
        
        
        public override HashSet<KeyValuePair<string, object>> getWorldState()
        {
            HashSet<KeyValuePair<string,object>> worldData = new HashSet<KeyValuePair<string,object>> ();
            worldData.Add(new KeyValuePair<string, object>("isHungry", (_hunger > _hungerLimit) ));
          // worldData.Add(new KeyValuePair<string, object>("isScared", false));
          //  worldData.Add(new KeyValuePair<string, object>("isIdle", _hunger < _hungerLimit));
            return worldData;
        }
                
        public override HashSet<KeyValuePair<string,object>> createGoalState () {
            HashSet<KeyValuePair<string, object>> goal = new HashSet<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("isHungry", false),
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
    }
}