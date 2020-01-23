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
        [SerializeField] private float _maxHunger = 100;
        [SerializeField] private float _hungerLimit = 30;
        [SerializeField] private float _hunger = 0;
        [SerializeField] private Vector3 _targetPos;
        private float _walkSpeed = 0.05f;
        private float _runSpeed = 0.1f;
 
        // urges
        private bool _isInDanger;
        [SerializeField] private Vector3 _foodLocation;

        [SerializeField] private Dictionary<Entity, float> _grassNear;
        [SerializeField] private Dictionary<Entity, float> _wolvesNear;

        public void Init(Grid grid)
        {
            _id = gameObject.GetInstanceID();
            EventManager.StartListening("SheepEaten", Eaten);
            _grid = grid;
            _currentNode = _grid.GetNodeCenterFromWorldPos(transform.position);
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
            _wolvesNear = EntityManager.FindEntityAround(transform.position, _sensingRadius, EntityType.WOLF);
            _grassNear = EntityManager.FindEntityAround(transform.position, _sensingRadius, EntityType.GRASS);
        }

        public override void Think()
        {
            // Default to wandering
            _state = State.WANDERING;

            // Priorities 
            // 1. Run from danger
            if(_wolvesNear.Count > 0)
            {
                // TODO find flee direction
                _state = State.FLEEING;
                return;
            }
            // 2. Look for food
            if (_hunger > _hungerLimit)
            {
                
                // Sort the dictionary to get the highest priority item
                var ordered = _grassNear.OrderByDescending(x => x.Value).ToList();
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

        private void Flee()
        {
          Quaternion lookAt = Quaternion.identity;
          float maxTurningDelta = 10;
          var direction = Vector3.zero;

          foreach (var wolf in _wolvesNear)
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
    }
}
