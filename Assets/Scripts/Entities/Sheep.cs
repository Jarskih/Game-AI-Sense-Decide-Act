using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        [SerializeField] private float _health = 1;
        [SerializeField] private float _hunger = 0;
        [SerializeField] private float _fear = 0;
 
        private readonly EntityType type = EntityType.SHEEP;
        private int _id;
        private Grid _grid;
        
        private Node _currentNode;
        private Node _oldNode;
        
        // Fleeing
        private float _maxFear = 50;
        private float _fearLimit = 5;
        private float _covardice = 10;
        
        // Hunger
        private float _maxHunger = 100;
        private float _hungerSpeed = 2;
        private float _starveSpeed = 10;
        private float _growSpeed = 2;
        private bool _isEating;

        // Breeding
        private float _healthReductionAfterBreeding = 50;

        // Wandering
        private readonly int[] _wanderAngles = {-15, -10, 5, 0, 0, 5, 10, 15};
        
        // Threat
        private Dictionary<Entity, float> _threatNear = new Dictionary<Entity, float>();
        [SerializeField] private List<Entity> _threatList = new List<Entity>();
        
        // Food
        private Dictionary<Entity, float> _foodNear = new Dictionary<Entity, float>();
        [SerializeField] private Entity _foodInSight;
        [SerializeField] private Entity _foodInMemory;
        private static float _memoryTime = 5f;
        private WaitForSeconds _wait = new WaitForSeconds(_memoryTime);

        public override void Init(Grid grid)
        { 
            EventManager.StartListening("SheepEaten", Eaten);

            _id = gameObject.GetInstanceID();
            _grid = grid;
            transform.position = _grid.GetRandomNodePos();
            _currentNode = _grid.GetNodeFromWorldPos(transform.position);
            _oldNode = _currentNode;
            
            // Senses
            _eyeSight = gameObject.AddComponent<Eyesight>();
            _hearing = gameObject.AddComponent<Hearing>();

            // Add actions this entity can perform and assign priorities for actions (Higher is more important)
            _availableActions.Add(new WanderAction(1));
            _availableActions.Add(new BreedAction(2));
            _availableActions.Add(new EatAction(3));
            _availableActions.Add(new FleeAction(4));

            
            // Init stats
            _stats.hungerLimit = 50;
            _stats.walkSpeed = 0.02f;
            _stats.runSpeed = 0.07f;
            _stats.slowTurnSpeed = 1;
            _stats.fastTurnSpeed = 180;
            _stats.maxHealth = 100;
            _stats.hearingDistance = 5;
            _stats.visionAngle = 90;
            _stats.visionDistance = 10;
            _stats.breedingLimit = 90;
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
            _currentState.UpdateState("isHungry", _hunger > stats.hungerLimit);
            _currentState.UpdateState("isAfraid", _fear > _fearLimit);
            _currentState.UpdateState("sawFood", _foodInSight != null);
            _currentState.UpdateState("isEating", _isEating);
            _currentState.UpdateState("isMature", _health > stats.breedingLimit);

            _currentNode = _grid.GetNodeFromWorldPos(transform.position);
            
            if (_currentNode != null && _currentNode != _oldNode)
            {
                _currentNode.AddEntity(this);
                _oldNode.RemoveEntity(this);
                _oldNode = _currentNode;
            }
            
            // Listen for wolves
            _threatNear = _hearing.DetectEntities(EntityType.WOLF, stats.hearingDistance);
            _threatList = _threatNear.Keys.ToList();
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
                StartCoroutine(ForgetFood()); // forget where the food was
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
            _lastAction = _currentAction;
            _currentAction = FindBestAction(_currentState);

            if (_currentAction == null)
            {
                Debug.LogError("No action available");
                Debug.LogError("isHungry: " + _currentState.GetState("isHungry").ToString());
                Debug.LogError("isAfraid: " + _currentState.GetState("isAfraid").ToString());
                Debug.LogError("sawFood: " + _currentState.GetState("sawFood").ToString());
                Debug.LogError("isEating: " + _currentState.GetState("isEating").ToString());
                Debug.LogError("isMature: " + _currentState.GetState("isMature").ToString());
            }
        }

        public override void Act()
        {
            // Increment counters
            _hunger += Time.deltaTime * _hungerSpeed;
            
            // If starving lose health otherwise gain health
            if (_hunger > _maxHunger)
            {
                _health -= Time.deltaTime * _starveSpeed;
            }
            else
            {
                _health += Time.deltaTime * _growSpeed;
            }
            
            // Increment fear if wolves on sight
            if (_threatList.Count == 0)
            {
                _fear -= Time.deltaTime * _covardice;
            }
            else
            {
                _fear += Time.deltaTime * _covardice;
            }

            _fear = Mathf.Clamp(_fear, 0, _maxFear);

            // Starved to death
            if (_health <= 0)
            {
                Die();
            }

            _health = Mathf.Min(_health, stats.maxHealth);
            
            var scale = _health * 0.01f * Vector3.one;
            var clampedScale = Mathf.Max(scale.x, 0.5f);
            transform.localScale = new Vector3(clampedScale,clampedScale,clampedScale);

            if (_currentAction != null && _currentAction.CanDoAction(_currentState))
            {
                _currentAction?.Act(this);
            }
        }

        public override Dictionary<Entity, float> FindFood()
        {
            return _foodNear;
        }
       
        public override void Flee()
        {
           Quaternion lookAt = Quaternion.identity;
          float maxTurningDelta = 0;
          var direction = Vector3.zero;

          foreach (var wolf in _threatNear)
          {
              direction += transform.position - wolf.Key.transform.position;
          }

          var t = transform;
          _targetPos = t.position + t.forward * 2;
          
          if (_grid.IsOutsideGrid(_targetPos))
          { 
              // We hit the end of the grid. Turn around
              var angle = 25;
              lookAt = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + angle, transform.rotation.eulerAngles.z);
              maxTurningDelta = 5;
              transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAt, maxTurningDelta);
          }
          else
          {
              lookAt = Quaternion.LookRotation(direction.normalized);
              maxTurningDelta = 5;
                        
              transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAt, maxTurningDelta);
              transform.position = Vector3.MoveTowards(transform.position, _targetPos, stats.runSpeed);
          }
        }


        public override void Wander()
        {
            _targetPos = GetWanderPos(_grid, _stats, _wanderAngles);
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, stats.walkSpeed);
        }

        public override void Breed()
        {
            var neighbors = _grid.GetNeighboringNodes(_currentNode);
            foreach (var node in neighbors)
            {
                if (!_grid.HasEntityOnNode(node, EntityType.SHEEP))
                {
                    // Found node to breed new sheep to
                    var message = new EventManager.EventMessage(this, node, EntityType.SHEEP);
                    EventManager.TriggerEvent("EntityAdded", message);
                    _health -= _healthReductionAfterBreeding;
                    _currentState.UpdateState("isMature", false);
                    return;
                }
            }
        }

        public override void Grow()
        {
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
                StartCoroutine(StopEating(foodOnTile));
                EventManager.EventMessage message = new EventManager.EventMessage(foodOnTile, _currentNode, EntityType.GRASS);
                EventManager.TriggerEvent("EntityDied", message);
            }
            else
            {
                float maxTurningDelta = 15;
                var foodLocation = targetFood.transform.position;
                if (foodLocation - transform.position != Vector3.zero)
                {
                    if(foodLocation - transform.position != Vector3.zero)
                    {
                        Quaternion lookAt = Quaternion.LookRotation(foodLocation - transform.position);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAt, maxTurningDelta);
                    }
                }
                transform.position = Vector3.MoveTowards(transform.position, foodLocation, stats.walkSpeed);
            }
        }

        IEnumerator StopEating(Entity food)
        {
            yield return new WaitForSeconds(2f);
            _hunger = 0;
            _isEating = false;
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
            _currentNode.RemoveEntity(this);
            EventManager.EventMessage message = new EventManager.EventMessage(this, _currentNode, EntityType.SHEEP);
            EventManager.TriggerEvent("EntityDied", message);
        }
    }
}
