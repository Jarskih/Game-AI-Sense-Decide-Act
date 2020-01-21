using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor.U2D;
using UnityEngine;

public class Main : MonoBehaviour
{
    private readonly int _gridSizeX = 50;
    private readonly int _gridSizeY = 1;
    private readonly int _gridSizeZ = 50;

    private readonly int _startingWolfs = 10;
    private readonly int _startingSheeps = 20;
    private readonly int _startingGrass = 30;
    
    [SerializeField] private Grid.Grid _grid;
    private List<Entity> _entities = new List<Entity>();
    
    // Start is called before the first frame update
    void Start()
    {
        _grid = new Grid.Grid();
        if (!_grid.Init(_gridSizeX, _gridSizeY, _gridSizeZ))
        {
            Debug.LogError("Error creating grid");
        }

        InitEntities();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var entity in _entities)
        {
            entity.Sense();
        }

        foreach (var entity in _entities)
        {
            entity.Think();
        }

        foreach (var entity in _entities)
        {
            entity.Act();
        }
    }

    private void InitEntities()
    {
        GameObject wolfContainer = new GameObject("WolfContainer");
        for (int i = 0; i < _startingWolfs; i++)
        {
            GameObject wolf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wolf.name = "Wolf " + i;
            wolf.AddComponent<Entities.Wolf>();
            wolf.transform.SetParent(wolfContainer.transform);
            wolf.transform.position = _grid.GetRandomNodePos();
        }

        GameObject sheepContainer = new GameObject("SheepContainer");
        for (int i = 0; i < _startingSheeps; i++)
        {
            GameObject sheep = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sheep.name = "Sheep " + i;
            sheep.AddComponent<Entities.Sheep>();
            sheep.transform.SetParent(sheepContainer.transform);
            sheep.transform.position = _grid.GetRandomNodePos();
        }

        GameObject grassContainer = new GameObject("GrassContainer");
        for (int i = 0; i < _startingGrass; i++)
        {
            GameObject grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grass.name = "Grass " + i;
            grass.AddComponent<Entities.Grass>();
            grass.transform.SetParent(grassContainer.transform);
            grass.transform.position = _grid.GetRandomNodePos();
        }
    }
}
