using System.Collections.Generic;
using UnityEngine;

namespace Grid
{
    public class Node
    {
        public Node(int pX, int pY, int pZ)
        {
            _x = pX;
            _y = pY;
            _z = pZ;
        }

        private readonly int _x, _y, _z;
        private List<Entity> _entities;
    }

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
                        Node n = new Node(x, y, z);
                        InitGridObjects(x, y, z, quadContainer.transform);
                        _nodes[x, y, z] = n;
                    }
                }
            }
        }

        public Node GetNodeCenterFromWorldPos(float pX, float pY, float pZ)
        {
            return _nodes[
                Mathf.CeilToInt(pX) + _nodeSizeX/2,
                Mathf.CeilToInt(pY) + _nodeSizeY/2,
                Mathf.CeilToInt(pZ) + _nodeSizeZ/2
            ];
        }

        public Vector3Int GetRandomNodePos()
        {
            int x = Random.Range(0, _gridSizeX);
            int y = Random.Range(0, _gridSizeY);
            int z = Random.Range(0, _gridSizeZ);
            return new Vector3Int(x,y,z);
        }
        
        private void InitGridObjects(int pX, int pY, int pZ, Transform parent)
        {
            Vector3 pos = new Vector3(pX * _nodeSizeX, pY * _nodeSizeY, pZ * _nodeSizeZ);
            GameObject quad  = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.SetParent(parent.transform);
            quad.transform.position = pos;
            quad.transform.rotation = Quaternion.Euler(90, 0, 0);
            quad.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        }
    }
}
