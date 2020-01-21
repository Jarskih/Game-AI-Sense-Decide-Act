using System.Collections.Generic;
using UnityEngine;

namespace FlatEarth
{
    public class Grass : Entity
    {
        [SerializeField] private float _growSpeed = 20f;
        [SerializeField] private float _lifetimeAsMature = 30;
        [SerializeField] private float _spreadInterval = 5;
        [SerializeField] private float _fullHealth = 100;
        
        [SerializeField] private int _id;
        [SerializeField] private State _state;
        private EntityType type = EntityType.GRASS;
        [SerializeField] private float _lifeTimeCounter = 0;
        [SerializeField] private float _spreadIntervalCounter = 0;
        [SerializeField] private float _health = 1;
        [SerializeField] private bool _mature;

        // Grid
        private Grid _grid;
        private Node _currentNode;
        
        private enum State
        {
            GROWING,
            SPREADING,
            TRAMPLED,
            DEAD,
        }

        public void Init(Grid grid)
        {
            _id = gameObject.GetInstanceID();
            EventManager.StartListening("GrassEaten", Eaten);
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

        //Sense: On each sense tick the grass should find out if it is being eaten or trampled wolves or sheep.

        public override void Sense(float deltaTime)
        {
            if (_state == State.DEAD)
            {
                return;
            }
           
            // Default to growing
            _state = State.GROWING;
            
            _currentNode = _grid.GetNodeCenterFromWorldPos(transform.position);

            if (_currentNode == null)
            {
                Debug.LogError("Cant find current node");
            }
            
            foreach (var entity in _grid.GetEntitiesOnNode(_currentNode))
            {
                if (entity == null) continue;
                
                if (entity.GetEntityType() != EntityType.GRASS)
                {
                    _state = State.TRAMPLED;
                }
            }
        }

        // Decide: On each decide tick the grass should choose if it will try to spread to an adjacent square or grow, depending on its internal state and sensing information.
        public override void Think(float deltaTime)
        {
            if (_state == State.DEAD) { return; }
            if (_state == State.TRAMPLED) { return; }
            
            if (_mature)
            {
                _spreadIntervalCounter += deltaTime;

                if (_spreadIntervalCounter > _spreadInterval)
                {
                    _spreadIntervalCounter = 0;
                    _state = State.SPREADING;
                }
                else
                {
                    _state = State.GROWING;
                }
            }
            else
            {
                _state = State.GROWING;
            }
        }

        /*
         Act: On each act tick grass will depending on what it decided either: 
         Grow to a maximum health value, if not being trampled or eaten up, after it reaches its maximum value it stays mature for a while then starts to wither and die, when reaching zero health it is replaced with dirt.
         Try to spread to another square if mature and not being eaten.
         */
        public override void Act(float deltaTime)
        {
            switch (_state)
            {
                case State.DEAD:
                case State.TRAMPLED:
                    return;
                case State.GROWING:
                {
                    Grow(deltaTime);
                    break;
                }
                case State.SPREADING:
                    Spread();
                    break;
            }
        }

        private void Grow(float deltaTime)
        {
            if (_mature)
            {
                // Age
                _lifeTimeCounter += deltaTime;
                if (_lifeTimeCounter > _lifetimeAsMature)
                {
                    _health -= deltaTime * _growSpeed;
                }

                if (_health < 0)
                {
                    Die();
                }
            }
            else
            {
                _health += deltaTime * _growSpeed;
                if (_health > _fullHealth)
                {
                    _mature = true;
                }
            } 
        }

        private void Spread()
        {
            var nodes = _grid.GetNeighboringNodes(_currentNode);
            var validNodes = new List<Node>();
            foreach (var node in nodes)
            {
                if (node.GetEntities().Count == 0)
                {
                    validNodes.Add(node);
                    continue;
                }

                foreach (var entity in node.GetEntities())
                {
                    if (entity == null)
                    {
                        validNodes.Add(node);
                        break;
                    }

                    if (entity.GetEntityType() != EntityType.GRASS)
                    {
                        validNodes.Add(node);
                        break;
                    }
                }
            }

            if (validNodes.Count > 0)
            {
                var nodeToSpread = validNodes[UnityEngine.Random.Range(0, validNodes.Count)];
                EventManager.EventMessage message = new EventManager.EventMessage(nodeToSpread, _id);
                EventManager.TriggerEvent("GrassSpreading", message);
            }
        }

        private void Eaten(EventManager.EventMessage message)
        {
            if (_currentNode == message.node)
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