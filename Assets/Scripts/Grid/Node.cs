using System.Collections.Generic;
using UnityEngine;

namespace FlatEarth
{
    [System.Serializable]
    public class Node
    {
        public Node(int pX, int pY, int pZ, GameObject go)
        {
            _x = pX;
            _y = pY;
            _z = pZ;
            _nodeObject = go; 
        }
       [SerializeField] private int _x, _y, _z;
       [SerializeField] private List<Entity> _entities = new List<Entity>(); // All entities currently on this node
       [SerializeField] private GameObject _nodeObject; // Reference to game object (quad) in the node

        public Vector3Int GetNodeGridPos()
        {
            return new Vector3Int(_x, _y, _z);
        }

        public Vector3 GetNodeWorldPos()
        {
            return _nodeObject.transform.position;
        }
        
        public bool CanAddEntity(Entity.EntityType type)
        {
            foreach (var e in _entities)
            {
                if (e.GetEntityType() == type)
                {
                    return false;
                }
            }
            return true;
        }

        public void AddEntity(Entity entity)
        {
            if (!_entities.Contains(entity))
            {
                _entities.Add(entity);   
            }
        }

        public void RemoveEntity(Entity entity)
        {
            if (_entities.Contains(entity))
            {
                _entities.Remove(entity);
            }
        }

        public List<Entity> GetEntities()
        {
            return _entities;
        }

        public Vector3 GetNodeSize()
        {
            return new Vector3(_x, _y, _z);
        }

        public bool HasEntity(Entity.EntityType type)
        {
            foreach (var entity in _entities)
            {
                if (entity.GetEntityType() == type)
                {
                    return true;
                }
            }
            return false;
        }
    }
}