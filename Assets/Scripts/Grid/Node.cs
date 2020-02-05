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
        private int _x, _y, _z;
        private List<Entity> _entities = new List<Entity>(); // All entities currently on this node
        private GameObject _nodeObject; // Reference to game object (quad) in the node

        public Vector3Int GetNodeGridPos()
        {
            return new Vector3Int(_x, _y, _z);
        }

        public Vector3 GetNodeWorldPos()
        {
            return _nodeObject.transform.position;
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
    }
}