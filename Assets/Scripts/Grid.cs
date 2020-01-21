using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    class Node {
        int x,y,z;
        private List<Entity> _entities;
    }

    private List<Node> _nodes;
    private int _gridSizeX = 50;
    private int _gridSizeY = 1;
    private int _gridSizeZ = 50;

    private int _nodeSizeX = 2;
    private int _nodeSizeZ = 2;
}
