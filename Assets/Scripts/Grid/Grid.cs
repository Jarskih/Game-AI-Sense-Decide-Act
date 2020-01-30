using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace FlatEarth
{
    public class Grid
    {
        // Visual representation of grid tile
        private List<GameObject> _gridObjects = new List<GameObject>();
        private Node[,,] _nodes;
        private int _gridSizeX;
        private int _gridSizeY;
        private int _gridSizeZ;

        private int _nodeSizeX = 1;
        private int _nodeSizeY = 2;
        private int _nodeSizeZ = 1;

        public bool Init(int pX, int pY, int pZ)
        {
            if (pX <= 0 && pY <= 0 && pZ <= 0)
            {
                Debug.LogError("Grid size parameters has to be positive integers");
                return false;
            }

            _gridSizeX = pX;
            _gridSizeY = pY;
            _gridSizeZ = pZ;

            _nodes = new Node[pX,pY,pZ];
            
            CreateGrid();

            return true;
        }

        private void CreateGrid()
        {
            GameObject quadContainer = new GameObject("Quads");
            for (int y = 0; y < _gridSizeY; y++)
            {
                for (int x = 0; x < _gridSizeX; x++)
                {
                    for (int z = 0; z < _gridSizeZ; z++)
                    {
                        var quad = InitGridObject(x, y, z, quadContainer.transform);
                        Node n = new Node(x, y, z, quad);
                        _nodes[x, y, z] = n;
                    }
                }
            }
        }
        /// <summary>
        /// Returns a node from world position
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>Node</returns>
        public Node GetNodeCenterFromWorldPos(Vector3 pos)
        {
            if (!IsValidPos(pos))
            {
                return null;
            }

            var x = Mathf.FloorToInt(pos.x);
            var z = Mathf.FloorToInt(pos.z);
            
            if(IsValidPos(x,0,z))
            {
                return _nodes[x, 0, z];
            }
            else
            {
                return null;
                Debug.LogError("Not valid world pos");
            }
        }

        public Vector3Int GetRandomNodePos()
        {
            int x = Random.Range(0, _gridSizeX);
            int y = Random.Range(0, _gridSizeY);
            int z = Random.Range(0, _gridSizeZ);
            return new Vector3Int(x,y,z);
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
            return new Vector3(pos.x*_nodeSizeX, pos.y*_nodeSizeY, pos.z*_nodeSizeZ);
        }
        
        public bool IsOutsideGrid(Vector3 pos)
        {
            if (pos.x < 0 || pos.y < 0 || pos.z < 0)
            {
                return true;
            }

            if (pos.x > _gridSizeX * _nodeSizeX - 1 || pos.y > _gridSizeY * _nodeSizeY-1 || pos.z > _gridSizeZ * _nodeSizeZ-1)
            {
                return true;
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
            quad.transform.localScale = new Vector3(0.98f, 0.98f, 0.98f);
            return quad;
        }
        private bool IsValidPos(int pX, int pY, int pZ)
        {
            if (pX < 0 || pY < 0 || pZ < 0)
            {
                return false;
            }
            if (pX > _gridSizeX - 1 || pY > _gridSizeY-1 || pZ > _gridSizeZ-1)
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
