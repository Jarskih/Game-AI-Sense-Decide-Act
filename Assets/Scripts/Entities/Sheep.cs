using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;

namespace FlatEarth
{
    public sealed class Sheep : Entity
    {
        // Components
        private Stats _stats;
        public Stats stats => _stats;
        
        private Hearing _hearing;
        private Eyesight _eyeSight;
        
        // internal state
        private float _maxHunger = 100;
        [SerializeField] private float _health = 1;
        [SerializeField] private float _hunger = 0;
 
        private readonly EntityType type = EntityType.SHEEP;
        private int _id;
        private Grid _grid;
        
        [SerializeField] private Node _currentNode;
        [SerializeField] private Node _oldNode;
        
        // Hunger
        [SerializeField] private float _hungerSpeed = 2;
        [SerializeField] private float _starveSpeed = 2;
        [SerializeField] private float _recoverSpeed = 2;
        
        // Wandering
        private readonly int[] _wanderAngles = {-15, -10, 5, 0, 0, 5, 10, 15};
        [SerializeField] private float _hungerLimit = 30;
        [SerializeField] private Vector3 _targetPos;

        [SerializeField] private Dictionary<Entity, float> _foodNear = new Dictionary<Entity, float>();
        [SerializeField] private Dictionary<Entity, float> _threatNear = new Dictionary<Entity, float>();
        [SerializeField] private Entity _foodInSight;
        [SerializeField] private Entity _foodInMemory;
        [SerializeField] private static float _memoryTime = 5f;
        private WaitForSeconds _wait = new WaitForSeconds(_memoryTime);
        
        [SerializeField] private bool _isEating;


        public void Init(Grid grid)
        { 
            EventManager.StartListening("SheepEaten", Eaten);

            _id = gameObject.GetInstanceID();
            _grid = grid;
            _currentNode = _grid.GetNodeCenterFromWorldPos(transform.position);
            _oldNode = _currentNode;
            
            // Senses
            _eyeSight = gameObject.AddComponent<Eyesight>();
            _hearing = gameObject.AddComponent<Hearing>();

            // Add actions this entity can perform and assign priorities for actions
            _availableActions.Add(new WanderAction(1));
            _availableActions.Add(new EatAction(2));
            _availableActions.Add(new FleeAction(3));
            
            // Init stats
            _stats.hungerLimit = 30;
            _stats.walkSpeed = 0.04f;
            _stats.runSpeed = 0.06f;
            _stats.slowTurnSpeed = 1;
            _stats.fastTurnSpeed = 180;
            _stats.maxHealth = 100;
            _stats.hearingDistance = 3;
            _stats.visionAngle = 90;
            _stats.visionDistance = 3;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position,_stats.hearingDistance);
            
            Gizmos.color = Color.blue;
            if (_foodInSight != null)
            {
                Gizmos.DrawCube(_foodInSight.transform.position, Vector3.one);
            }
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
            _currentState.UpdateState("isHungry", _hunger > _hungerLimit);
            _currentState.UpdateState("isAfraid", _threatNear.Count > 0);
            _currentState.UpdateState("sawFood", _foodInSight != null);
            _currentState.UpdateState("isEating", _isEating);
           
            _currentNode = _grid.GetNodeCenterFromWorldPos(transform.position);
            
            if (_currentNode != null && _currentNode != _oldNode)
            {
                _currentNode.AddEntity(this);
                _oldNode.RemoveEntity(this);
                _oldNode = _currentNode;
            }
            
            // Listen for wolves
            _threatNear = _hearing.DetectEntities(EntityType.WOLF, stats.hearingDistance);
            
            // Try to see food
            _foodNear = _eyeSight.DetectEntities(EntityType.GRASS, stats.visionDistance, stats.visionAngle);
            
            // Focus on closest food      
            if (_foodNear.Count > 0)
            {
                _foodInSight = _foodNear.OrderByDescending(x => x.Value).ToList().Last().Key;
            }
            
            // Remember one food spot just in case we don't see any food
            if (_foodInMemory == null)
            {
                _foodInMemory = _foodInSight;
                StartCoroutine(ForgetFood());
            }
        }

        IEnumerator ForgetFood()
        {
            yield return _wait;
            _foodInSight = null;
        }

        public override void Think()
        {
            // Get actions entity can do
            _currentAction = FindBestAction(_currentState);

            if (_currentAction == null)
            {
                Debug.LogError("No action available");
                Debug.LogError("isHungry: " + _currentState.GetState("isHungry").ToString());
                Debug.LogError("isAfraid: " + _currentState.GetState("isAfraid").ToString());
                Debug.LogError("sawFood: " + _currentState.GetState("sawFood").ToString());
                Debug.LogError("isEating: " + _currentState.GetState("isEating").ToString());
            }
        }

        public override void Act()
        {   
            // Increment counters
            _hunger += Time.deltaTime * _hungerSpeed;
            
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

            _currentAction?.Act(this);
        }

        public override Dictionary<Entity, float> FindFood()
        {
            return _foodNear;
        }
       
        public override void Flee()
        {
           _isEating = false;
            
          Quaternion lookAt = Quaternion.identity;
          float maxTurningDelta = 45;
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
          transform.position = Vector3.MoveTowards(transform.position, _targetPos, stats.runSpeed);
        }


        public override void Wander()
        {
            _isEating = false;
            _targetPos = GetWanderPos();
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, stats.walkSpeed);
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
                maxTurningDelta = stats.fastTurnSpeed;
            }
            else
            {
                // Randomly turn left or right or continue straight
                angle = _wanderAngles[Random.Range(0, _wanderAngles.Length)];
                maxTurningDelta = stats.slowTurnSpeed;
            }

            Quaternion rotateDir = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + angle, transform.rotation.eulerAngles.z);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateDir, maxTurningDelta);

            return transform.position + transform.forward * 1;
        }

        /// <summary>
        /// Move to food location if entity has seen food or remember a good spot. Eat if on same tile as food, otherwise move to food location.
        /// </summary>
        public override void Eat()
        {
            if (_isEating)
            {
                return;
            }
            
            Entity targetFood = null;
            
            if (_foodInSight == null && _foodInMemory == null)
            {
                Wander();
                return;
            }

            if (_foodInSight != null)
            {
                targetFood = _foodInSight;
            }
            else
            {
                targetFood = _foodInMemory;
            }

            Entity foodOnTile = null;

            var entities = _grid.GetEntitiesOnNode(_currentNode);
            foreach (var e in entities)
            {
                if (e.GetEntityType() == EntityType.GRASS)
                {
                    foodOnTile = e;
                }
            }

            if (foodOnTile != null)
            {
                _isEating = true;
                StartCoroutine(StopEating(foodOnTile.GetId()));
            }
            else
            {
                float maxTurningDelta = 15;
                var foodLocation = targetFood.transform.position;
                Quaternion lookAt = Quaternion.LookRotation(foodLocation - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAt, maxTurningDelta);
                transform.position = Vector3.MoveTowards(transform.position, foodLocation, stats.walkSpeed);
            }
        }

        IEnumerator StopEating(int id)
        {
            yield return new WaitForSeconds(2f);
            _hunger = 0;
            _isEating = false;
            EventManager.EventMessage message = new EventManager.EventMessage(id);
            EventManager.TriggerEvent("EntityDied", message);
        }

        /// <summary>
        /// Check if this entity was eaten by other entity by comparing unique ID
        /// </summary>
        /// <param name="message"></param>
        private void Eaten(EventManager.EventMessage message)
        {
            if (_id == message.id)
            {
                Die();
            }
        }
        
        /// <summary>
        /// Trigger event to remove entity if entity died
        /// </summary>

        private void Die()
        {
            _health = 0;
            EventManager.EventMessage message = new EventManager.EventMessage(_id);
            EventManager.TriggerEvent("EntityDied", message);
        }
    }
}
