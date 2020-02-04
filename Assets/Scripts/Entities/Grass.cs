using System.Collections.Generic;
using UnityEngine;

namespace FlatEarth
{
    public class Grass : Entity
    {
        private Stats _stats;
        public Stats stats => _stats;
        
        [SerializeField] private float _growSpeed = 20f;
        [SerializeField] private float _dieSpeed = 100f;
        [SerializeField] private float _lifetimeAsMature = 60;
        [SerializeField] private float _spreadInterval = 5;

        [SerializeField] private int _id;
        [SerializeField] private State _state;
        [SerializeField] private EntityType type = EntityType.GRASS;
        [SerializeField] private float _lifeTimeCounter = 0;
        [SerializeField] private float _spreadIntervalCounter = 0;
        [SerializeField] private float _health = 1;
        [SerializeField] private bool _mature;

        // Grid
        private Grid _grid;
        [SerializeField] private Node _currentNode;
        
        private enum State
        {
            GROWING,
            SPREADING,
            TRAMPLED,
            DEAD,
        }

        public override void Init(Grid grid)
        {
            _id = gameObject.GetInstanceID();
            
            EventManager.StartListening("GrassEaten", Eaten);
            _grid = grid;
            transform.position = _grid.GetRandomNodePos();
            
            _stats.maxHealth = 100;
            _health = Random.Range(1, 100); // Random health at start
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

        public override void Sense()
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
        public override void Think()
        {
            if (_state == State.DEAD) { return; }
            if (_state == State.TRAMPLED) { return; }
            
            if (_mature)
            {
                _spreadIntervalCounter += Time.deltaTime;

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
        public override void Act()
        {
            switch (_state)
            {
                case State.DEAD:
                    break;
                case State.TRAMPLED:
                    break;
                case State.GROWING:
                {
                    Grow();
                    break;
                }
                case State.SPREADING:
                    Spread();
                    break;
            }
        }

        public override Dictionary<Entity, float> FindFood()
        {
            return null;
        }

        public override void Eat()
        {
        }

        public override void Flee()
        {
        }

        public override void Wander()
        {
        }

        public override void Breed()
        {
        }

        private void Grow()
        {
            if (_mature)
            {
                // Age
                _lifeTimeCounter += Time.deltaTime;
                if (_lifeTimeCounter > _lifetimeAsMature)
                {
                    _health -= Time.deltaTime * _dieSpeed;
                }

                if (_health < 0)
                {
                    Die();
                }
            }
            else
            {
                _health += Time.deltaTime * _growSpeed;
                if (_health > stats.maxHealth)
                {
                    _mature = true;
                }
            }

            var scale = _health * 0.01f;
            scale = Mathf.Clamp(scale, 0, 0.9f);
            transform.localScale = new Vector3(scale, transform.localScale.y, scale);
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
                EventManager.EventMessage message = new EventManager.EventMessage(_id, nodeToSpread, EntityType.GRASS);
                EventManager.TriggerEvent("GrassSpreading", message);
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
            _currentNode.RemoveEntity(this);
            EventManager.EventMessage message = new EventManager.EventMessage(_id);
            EventManager.TriggerEvent("EntityDied", message);
        }
    }
}