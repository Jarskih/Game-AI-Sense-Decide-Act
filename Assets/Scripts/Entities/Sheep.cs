using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FlatEarth
{
    public class Sheep : Entity
    {
        private readonly EntityType type = EntityType.SHEEP;
        [SerializeField] private float _health = 100;
        [SerializeField] private float _hunger = 0;
        [SerializeField] private float _maxHunger = 100;
        [SerializeField] private float _eatLimit = 30;
        [SerializeField] private int _id;
        [SerializeField] private State _state;
        [SerializeField] private Node _currentNode;
        private Grid _grid;
        [SerializeField] private bool _hasGrass;
        [SerializeField] private float _hungerSpeed = 2;
        [SerializeField] private float _starveSpeed = 5;
        [SerializeField] private float _recoverSpeed;
        [SerializeField] private float _maxDistanceDelta = 1;
        private Node _wanderTarget;
        private Vector3 targetPos;

        private enum State
        {
            DEAD,
            EATING,
            FLEEING,
            WANDERING,
            BREEDING,
        }

        public void Init(Grid grid)
        {
            _id = gameObject.GetInstanceID();
            EventManager.StartListening("SheepEaten", Eaten);
            _grid = grid;
            _currentNode = _grid.GetNodeCenterFromWorldPos(transform.position);
            _currentNode.AddEntity(this);
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
            _hasGrass = false;
            // Find grass
            var entities = _grid.GetEntitiesOnNode(_currentNode);
            if (entities.Count > 0)
            {
                foreach (var e in entities)
                {
                    if (e.GetEntityType() == EntityType.GRASS)
                    {
                        _hasGrass = true;
                    }
                }
            }

            //TODO find wolves
        }

        public override void Think(float deltaTime)
        {
            // Default to wandering
            _state = State.WANDERING;
            
            _hunger += deltaTime * _hungerSpeed;
            if (_hunger > _maxHunger)
            {
                _health -= deltaTime * _starveSpeed;
            }
            else
            {
                _health += deltaTime * _recoverSpeed;
            }
            
            if (_hasGrass)
            {
                if (_hunger > _eatLimit)
                {
                    _state = State.EATING;
                }
            }

            if (_health < 0)
            {
                Die();
            }
        }

        public override void Act(float deltaTime)
        {
            switch (_state) 
            {
                case State.DEAD:
                    break;
                case State.EATING:
                    Eat();
                    break;
                case State.WANDERING:
                    Wander();
                    break;
            }
        }

        private void Wander()
        {
            if (_wanderTarget == null)
            {
                var neighboringNodes = _grid.GetNeighboringNodes(_currentNode);
                _wanderTarget = neighboringNodes[Random.Range(0, neighboringNodes.Count)];
            }

            // new wonder target
            if (Vector3.Distance(targetPos, transform.position) < 0.1f)
            {
                var neighboringNodes = _grid.GetNeighboringNodes(_currentNode);
                _wanderTarget = neighboringNodes[Random.Range(0, neighboringNodes.Count)];
            }
            
            targetPos = _grid.GetWorldPosFromNode(_wanderTarget.GetNodePos());
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 0.1f);
        }

        private void Eat()
        {
            _hunger = 0;
            EventManager.EventMessage message = new EventManager.EventMessage(_currentNode, _id);
            EventManager.TriggerEvent("GrassEaten", message);
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
