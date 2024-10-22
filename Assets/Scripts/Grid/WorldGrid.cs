﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlatEarth
{
    public class WorldGrid
    {
        // Visual representation of grid tile
        private Node[,,] _nodes;
        private int _gridSizeX;
        private int _gridSizeZ;
        private int _gridSizeY = 1;

        private int _nodeSizeX = 1;
        private int _nodeSizeZ = 1;
        private int _nodeSizeY = 1;
        public int sizeX => _gridSizeX;
        public int sizeZ => _gridSizeZ;

        public bool Init(int pX, int pY, int pZ)
        {
            if (pX <= 0 && pY <= 0 && pZ <= 0)
            {
                Debug.LogError("Grid size has to be positive numbers");
                return false;
            }

            _gridSizeX = pX;
            _gridSizeZ = pZ;

            _nodes = new Node[pX,pY,pZ];
            
            CreateGrid();

            return true;
        }

        private void CreateGrid()
        {
            GameObject quadContainer = new GameObject("Quads");
            int y = 0;
            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int z = 0; z < _gridSizeZ; z++)
                {
                    var quad = InitGridObject(x, 1, z, quadContainer.transform);
                    Node n = new Node(x, 1, z, quad);
                    _nodes[x, y, z] = n;
                }
            }
        }
        /// <summary>
        /// Returns a node from world position
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>Node</returns>
        public Node GetNodeFromWorldPos(Vector3 pos)
        {
            if (!IsValidPos(pos))
            {
                return null;
            }

            var x = Mathf.RoundToInt(pos.x / _nodeSizeX);
            var z = Mathf.RoundToInt(pos.z / _nodeSizeZ);
            
            return IsValidPos(x,0,z) ? _nodes[x, 0, z] : null;
        }

        public Vector3 GetRandomNodePos()
        {
            int x = UnityEngine.Random.Range(0, _gridSizeX);
            int y = UnityEngine.Random.Range(0, _gridSizeY);
            int z = UnityEngine.Random.Range(0, _gridSizeZ);
            return GetWorldPosFromNode(new Vector3(x,y,z));
        }
        
        public List<Entity> GetEntitiesOnNode(Node n)
        {
            if (n == null)
            {
                Debug.LogError("Invalid Node");
                var e = new List<Entity>();
                return e;
            }
            return n.GetEntities();
        }
        
        public List<Node> GetNeighboringNodes(Node currentNode)
        {
            List<Node> neighbors = new List<Node>();
            int y = currentNode.GetNodeGridPos().y;
            for (int x = currentNode.GetNodeGridPos().x - 1; x <= currentNode.GetNodeGridPos().x + 1; x++)
            {
                for (int z = currentNode.GetNodeGridPos().z - 1; z <= currentNode.GetNodeGridPos().z + 1; z++)
                {
                    if (x == currentNode.GetNodeGridPos().x && z == currentNode.GetNodeGridPos().z)
                    {
                        continue;
                    }

                    if (IsValidPos(x, y, z))
                    {
                        neighbors.Add(_nodes[x, y, z]);
                    }
                }
            }
            return neighbors;
        }
        public Vector3 GetWorldPosFromNode(Vector3 pos)
        {
            return new Vector3(pos.x*_nodeSizeX, pos.y, pos.z*_nodeSizeZ);
        }
        
        public bool IsOutsideGrid(Vector3 pos)
        {
            if (pos.x < 0 || pos.y < 0 || pos.z < 0)
            {
                return true;
            }

            if (pos.x > _gridSizeX * _nodeSizeX - 1 || pos.z > _gridSizeZ * _nodeSizeZ-1)
            {
                return true;
            }

            return false;
        }

        public bool HasEntityOnNode(Node node, Entity.EntityType type)
        {
            foreach (var entity in GetEntitiesOnNode(node))
            {
                if (entity.GetEntityType() == type)
                {
                    return true;
                }
            }
            return false;
        }
        
        private GameObject InitGridObject(int pX, int pY, int pZ, Transform parent)
        {
            Vector3 pos = new Vector3(pX * _nodeSizeX, pY * _nodeSizeY, pZ * _nodeSizeZ);
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Materials.Sand);

            quad.transform.SetParent(parent.transform);
            quad.transform.position = pos;
            quad.transform.rotation = Quaternion.Euler(90, 0, 0);
            quad.transform.localScale = new Vector3(0.98f*_nodeSizeX, 0.98f*_nodeSizeY, 0.98f*_nodeSizeZ);
            return quad;
        }
        private bool IsValidPos(int pX, int pY, int pZ)
        {
            if (pX < 0 || pY < 0 || pZ < 0)
            {
                return false;
            }
            if (pX > _nodeSizeX*_gridSizeX - 1 || pY >  _nodeSizeY*_gridSizeY-1 || pZ >  _nodeSizeZ*_gridSizeZ-1)
            {
                return false;
            }
            return true;
        }
        
        private bool IsValidPos(Vector3 pos)
        {
            return IsValidPos((int)pos.x, (int)pos.y, (int)pos.z);
        }
       
    }
}
