using System;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = System.Random;

namespace FlatEarth
{
    public class Grass : Entity
    {
        [SerializeField] private float _growSpeed = 10f;
        [SerializeField] private float _lifetimeAsMature = 30;
        [SerializeField] private float _growInterval = 5;
        [SerializeField] private float _fullHealth = 100;
        
        [SerializeField] private int _id;
        [SerializeField] private State _state;
        private EntityType type = EntityType.GRASS;
        [SerializeField] private float _lifeTimeCounter = 0;
        [SerializeField] private float _growIntervalCounter = 0;
        [SerializeField] private float _health = 1;

        // Grid
        private Grid _grid;
        private Node _currentNode;
        
        private enum State
        {
            GROWING,
            MATURE,
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
                    
                }
            }
        }

        // Decide: On each decide tick the grass should choose if it will try to spread to an adjacent square or grow, depending on its internal state and sensing information.
        public override void Think(float deltaTime)
        {
            if (_state == State.DEAD)
            {
                return;
            }
        }

        /*
         Act: On each act tick grass will depending on what it decided either: 
         Grow if not being trampled or eaten up to a maximum health value, after it reaches its maximum value it stays mature for a while then starts to wither and die, when reaching zero health it is replaced with dirt.
         Try to spread to another square if mature and not being eaten.
         */
        public override void Act(float deltaTime)
        {
            switch (_state)
            {
                case State.DEAD:
                    return;
                case State.GROWING:
                    _health += deltaTime * _growSpeed;
                    if (_health > _fullHealth)
                    {
                        _state = State.MATURE;
                    }
                    break;
                case State.MATURE:
                {
                    _lifeTimeCounter += deltaTime;
                    if (_lifeTimeCounter > _lifetimeAsMature)
                    {
                        _health -= deltaTime * _growSpeed;
                    
                        if (_health <= 0)
                        {
                            Die();
                        }

                        _growIntervalCounter += deltaTime;

                        if (_growIntervalCounter > _growInterval)
                        {
                            _growInterval = 0;
                            Spread();
                        }
                    }
                    break;
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

            var nodeToSpread = validNodes[UnityEngine.Random.Range(0, validNodes.Count - 1)];
            EventManager.EventMessage message = new EventManager.EventMessage(nodeToSpread, _id);
            EventManager.TriggerEvent("GrassSpreading", message);

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
            EventManager.TriggerEvent("GrassDied", message);
        }
    }
}