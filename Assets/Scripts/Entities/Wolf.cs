using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;

namespace FlatEarth
{
    public class Wolf : Entity
    {
        // Components
        private Stats _stats;
        public Stats stats => _stats;
        
        private Hearing _hearing;
        private Eyesight _eyeSight;
        
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

        [SerializeField] private Entity _foodInSight;
        [SerializeField] private Entity _foodInMemory;
        [SerializeField] private static float _memoryTime = 10f;

        private WaitForSeconds _wait = new WaitForSeconds(_memoryTime);
        public void Init(Grid grid)
        {
            _id = gameObject.GetInstanceID();
            _grid = grid;
            transform.position = _grid.GetRandomNodePos();
            _currentNode = _grid.GetNodeCenterFromWorldPos(transform.position);
            _oldNode = _currentNode;
            
            // Senses
            _hearing = gameObject.AddComponent<Hearing>();
            _eyeSight = gameObject.AddComponent<Eyesight>();

            // Add actions this entity can perform and assign priorities for actions
            _availableActions.Add(new WanderAction(2));
            _availableActions.Add(new EatAction(1));
            
            // init stats
            _stats.hungerLimit = 70;
            _stats.walkSpeed = 0.01f;
            _stats.runSpeed = 0.05f;
            _stats.slowTurnSpeed = 1;
            _stats.fastTurnSpeed = 180;
            _stats.maxHealth = 100;
            _stats.hearingDistance = 3;
            _stats.visionAngle = 90;
            _stats.visionDistance = 5;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position,_stats.hearingDistance);
            
            Gizmos.color = Color.red;
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
            _currentState.UpdateState("isAfraid", false);
            _currentState.UpdateState("sawFood", _foodInSight != null);
            
            _currentNode = _grid.GetNodeCenterFromWorldPos(transform.position);
            
            if (_currentNode != null && _currentNode != _oldNode)
            {
                _currentNode.AddEntity(this);
                _oldNode.RemoveEntity(this);
                _oldNode = _currentNode;
            }
            
            // Try to see food
            _foodNear = _eyeSight.DetectEntities(EntityType.SHEEP, stats.visionDistance, stats.visionAngle);

            // If that fails try listening for food
            if (_foodNear?.Count == 0)
            {
                _foodNear = _hearing.DetectEntities(EntityType.SHEEP, stats.hearingDistance);
            }

            // Pick closest as preferred food
            if (_foodNear.Count > 0)
            {
                _foodInSight = _foodNear.OrderByDescending(x => x.Value).ToList().Last().Key;
            }

            if (_foodInMemory == null)
            {
                _foodInMemory = _foodInSight;
                StartCoroutine(ForgetFood());
            }
        }

        IEnumerator ForgetFood()
        {
            yield return _wait;
            _foodInMemory = null;
        }

        public override void Think()
        {
            // Get actions entity can do
           _currentAction = FindBestAction(_currentState);

           if (_currentAction == null)
           {
               Debug.LogError("No action available");
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
        }


        public override void Wander()
        {
            _targetPos = GetWanderPos();
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, stats.walkSpeed);
        }

        public override void Eat()
        {
            Entity targetFood = null;
            
            if (_foodInSight == null && _foodInMemory == null)
            {
                Wander();
                return;
            }

            targetFood = _foodInSight == null ? _foodInMemory : _foodInSight;
            
            if (Vector3.Distance(transform.position, _foodInSight.transform.position) < 0.5f)
            {
                _hunger = 0;
                EventManager.EventMessage message = new EventManager.EventMessage(_foodInSight.GetId());
                EventManager.TriggerEvent("EntityDied", message);
            }
            else
            {
                float maxTurningDelta = 15;
                var foodLocation = targetFood.transform.position;
                Quaternion lookAt = Quaternion.LookRotation(foodLocation - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookAt, maxTurningDelta);
                transform.position = Vector3.MoveTowards(transform.position, foodLocation, stats.runSpeed);
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
            float maxTurningDelta; // in degrees
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
    }
}