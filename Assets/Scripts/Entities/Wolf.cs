﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        
        private float _lifeTimeCounter;
        private double _lifetime = 240;
        
        // UI 
        private Sprite _eatSprite;
        private Sprite _breedSprite;
        private Sprite _wanderSprite;
        
        // internal state
        private float _maxHunger = 100;
        [SerializeField] protected float _hunger = 0;
        [SerializeField] private float _health = 1;
        [SerializeField] private bool _isEating;
        
        private readonly EntityType type = EntityType.WOLF;
        private int _id;
        private WorldGrid _worldGrid;

        [SerializeField] private Node _currentNode;
        [SerializeField] private Node _oldNode;
       
        // Hunger
        [SerializeField] private float _hungerSpeed = 1;
        [SerializeField] private float _starveSpeed = 1;
        [SerializeField] private float _recoverSpeed = 1;
        [SerializeField] private Dictionary<Entity, float> _foodNear = new Dictionary<Entity, float>();
        
        // Breeding
        private float _healthReductionAfterBreeding = 70;
        
        // Wandering
        private readonly int[] _wanderAngles = {-15, -10, 5, 0, 0, 5, 10, 15};

        [SerializeField] private Entity _foodInSight;
        [SerializeField] private Entity _foodInMemory;
        [SerializeField] private static float _memoryTime = 10f;

        private WaitForSeconds _wait = new WaitForSeconds(_memoryTime);

        public override void Init(WorldGrid worldGrid)
        {
            _id = gameObject.GetInstanceID();
            _worldGrid = worldGrid;
            _currentNode = _worldGrid.GetNodeFromWorldPos(transform.position);
            _oldNode = _currentNode;
            
            // Senses
            _hearing = gameObject.AddComponent<Hearing>();
            _eyeSight = gameObject.AddComponent<Eyesight>();

            // Add actions this entity can perform and assign priorities for actions (Higher is more important)
            _availableActions.Add(new WanderAction(1));
            _availableActions.Add(new BreedAction(2));
            _availableActions.Add(new EatAction(3));

            // UI sprites
            _eatSprite = Resources.Load<Sprite>("Sprites/Sheep");
            _breedSprite = Resources.Load<Sprite>("Sprites/Heart");
            _wanderSprite = Resources.Load<Sprite>("Sprites/Smile");
            
            // init stats
            _stats.hungerLimit = 70;
            _stats.walkSpeed = 0.02f;
            _stats.runSpeed = 0.1f;
            _stats.slowTurnSpeed = 1;
            _stats.fastTurnSpeed = 180;
            _stats.maxHealth = 100;
            _stats.hearingDistance = 5;
            _stats.visionAngle = 90;
            _stats.visionDistance = 10;
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
            _currentState.UpdateState("isMature", _health > 90);
            
            _oldNode = _currentNode;
            _currentNode = _worldGrid.GetNodeFromWorldPos(transform.position);
            if (_currentNode != null && _currentNode != _oldNode)
            {
                _currentNode.AddEntity(this);
                _oldNode.RemoveEntity(this);
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
            // Died of old age
            _lifeTimeCounter += Time.deltaTime;
            if (_lifeTimeCounter > _lifetime)
            {
                Die();
            }

            // Starved to death
            if (_health <= 0)
            {
                Die();
                return;
            }
            _health = Mathf.Min(_health, stats.maxHealth);
            
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
            
            var scale = _health * 0.01f * Vector3.one;
            var clampedScale = Mathf.Max(scale.x, 0.3f);
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

        public override void Wander()
        {
            _stateSprite = _wanderSprite;
            _targetPos = GetWanderPos(_worldGrid, _stats, _wanderAngles);
            transform.position = Vector3.MoveTowards(transform.position, _targetPos, stats.walkSpeed);
        }

        public override void Breed()
        {
            _stateSprite = _breedSprite;
            var neighbors = _worldGrid.GetNeighboringNodes(_currentNode);
            foreach (var node in neighbors)
            {
                if (!_worldGrid.HasEntityOnNode(node, EntityType.WOLF))
                {
                    // Found node to breed to
                    var message = new EventManager.EventMessage(this, node, EntityType.WOLF);
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

        public override void TakeDamage(int damage)
        {
            _health -= damage;
            if (_health <= 0)
            {
                Die();
            }
        }


        public override void Eat()
        {
            _stateSprite = _eatSprite;
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

            targetFood = _foodInSight == null ? _foodInMemory : _foodInSight;

            if (Vector3.Distance(transform.position, targetFood.transform.position) < 0.5f)
            {
                _isEating = true;
                StartCoroutine(StopEating(_foodInSight.GetId()));
                targetFood.TakeDamage(100);
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
        
        IEnumerator StopEating(int foodId)
        {
            yield return new WaitForSeconds(2f);
            _hunger = 0;
            _isEating = false;
        }

        private void Die()
        {
            _health = 0;
            _currentNode.RemoveEntity(this);
            EventManager.EventMessage message = new EventManager.EventMessage(this, _currentNode, EntityType.WOLF);
            EventManager.TriggerEvent("EntityDied", message);
        }
        
        
        public override void Flee()
        {
        }
    }
}