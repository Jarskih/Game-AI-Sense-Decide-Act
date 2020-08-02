using System.Collections.Generic;
using UnityEngine;

namespace FlatEarth
{
    public class Grass : Entity
    {
        private Stats _stats;
        public Stats stats => _stats;
        
        private float _growSpeed = 20f;
        private float _dieSpeed = 100f;
        private float _lifetime = 60;
        private float _spreadInterval = 5;

        private int _id;
        private EntityType type = EntityType.GRASS;
        [SerializeField] private float _lifeTimeCounter = 0;
        [SerializeField] private float _spreadIntervalCounter = 0;
        [SerializeField] private float _health = 1;
        [SerializeField] private bool _mature;

        // Grid
        private WorldGrid _worldGrid;
        [SerializeField] private Node _currentNode;
        public override void Init(WorldGrid worldGrid)
        {
            _id = gameObject.GetInstanceID();
            
            _worldGrid = worldGrid;

            _stats.maxHealth = 100;
            _health = Random.Range(1, 100); // Random health at start

            _stats.breedingLimit = 90;
            
            _currentNode = _worldGrid.GetNodeFromWorldPos(transform.position);

            // Add actions this entity can perform and assign priorities for actions (Higher is more important)
            _availableActions.Add(new GrowAction(1));
            _availableActions.Add(new SpreadAction(2));
            _availableActions.Add(new TrampledAction(3));

            _currentAction = _availableActions[0];
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
            if (_currentNode == null)
            {
                Debug.LogError("Cant find current node");
            }
            
            _currentState.UpdateState("isHealthy", _health > _stats.breedingLimit);
            _currentState.UpdateState("isTrampled", false);
            _currentState.UpdateState("isReadyToSpread", _spreadIntervalCounter > _spreadInterval);
 
            if (_spreadIntervalCounter > _spreadInterval)
            {
                _spreadIntervalCounter = 0;
            }

            foreach (var entity in _worldGrid.GetEntitiesOnNode(_currentNode))
            {
                if (entity == null) continue;
                
                if (entity.GetEntityType() != EntityType.GRASS)
                {
                    _currentState.UpdateState("isTrampled", true);
                }
            }
        }

        // Decide: On each decide tick the grass should choose if it will try to spread to an adjacent square or grow, depending on its internal state and sensing information.
        public override void Think()
        {
            _currentAction = FindBestAction(_currentState);
        }

        /*
         Act: On each act tick grass will depending on what it decided either: 
         Grow to a maximum health value, if not being trampled or eaten up, after it reaches its maximum value it stays mature for a while then starts to wither and die, when reaching zero health it is replaced with dirt.
         Try to spread to another square if mature and not being eaten.
         */
        public override void Act()
        {
            _spreadIntervalCounter += Time.deltaTime;

            _currentAction.Act(this);

            var scale = _health * 0.01f;
            scale = Mathf.Clamp(scale, 0, 0.9f);
            transform.localScale = new Vector3(scale, transform.localScale.y, scale);
        }

        public override void Grow()
        {
            if (_mature)
            {
                // Age
                _lifeTimeCounter += Time.deltaTime;
                if (_lifeTimeCounter > _lifetime)
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
        }

        public override void TakeDamage(int damage)
        {
            _health -= damage;
            if (_health <= 0)
            {
                Die();
            }
        }

        public override void Breed()
        {
            var nodes = _worldGrid.GetNeighboringNodes(_currentNode);
            var validNodes = new List<Node>();
            foreach (var node in nodes)
            {
                if (node.HasEntity(EntityType.GRASS))
                {
                    continue;
                }
                validNodes.Add(node);
            }

            if (validNodes.Count > 0)
            {
                var nodeToSpread = validNodes[UnityEngine.Random.Range(0, validNodes.Count)];
                EventManager.EventMessage message = new EventManager.EventMessage(this, nodeToSpread, EntityType.GRASS);
                EventManager.TriggerEvent("EntityAdded", message);
            }
        }

        private void Die()
        {
            _health = 0;
            _currentNode.RemoveEntity(this);
            EventManager.EventMessage message = new EventManager.EventMessage(this, _currentNode, EntityType.GRASS);
            EventManager.TriggerEvent("EntityDied", message);
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
    }
}