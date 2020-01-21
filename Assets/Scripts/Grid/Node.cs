using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlatEarth
{
    public class Node
    {
        public Node(int pX, int pY, int pZ, GameObject go)
        {
            _x = pX;
            _y = pY;
            _z = pZ;
            nodeObject = go;
        }
        private readonly int _x, _y, _z;
        private List<Entity> _entities = new List<Entity>();
        private GameObject nodeObject;

        public Vector3Int GetNodePos()
        {
            return new Vector3Int(_x, _y, _z);
        }

        public void AddEntity(Entity entity)
        {
            _entities.Add(entity);
        }

        public List<Entity> GetEntities()
        {
            return _entities; 
        }
    }
}