using System.Collections;
using System.Collections.Generic;
using UnityEditor.CrashReporting;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace FlatEarth
{
    public class EntityManager : MonoBehaviour
    {
        private int _startingWolfs;
        private int _startingSheep;
        private int _startingGrass;

        [SerializeField] private static List<Entity> _entities = new List<Entity>();
        [SerializeField] private static List<Entity> _newEntities = new List<Entity>();
        [SerializeField] private static List<Entity> _removedEntities = new List<Entity>();

    public List<Entity> entities => _entities;

    // Used to organize scene
    private GameObject wolfContainer;
    private GameObject sheepContainer;
    private GameObject grassContainer;
    
    private Grid _grid;

    public void Init(Grid grid, int wolf, int sheep, int grass)
    {
        _startingWolfs = wolf;
        _startingSheep = sheep;
        _startingGrass = grass;

        _grid = grid;
        InitListeners();
        InitEntities();
    }
    
    private void InitListeners()
    {
        EventManager.StartListening("GrassSpreading", GrowGrass);
        EventManager.StartListening("EntityDied", RemoveEntity);
    }

    public void UpdateEntities()
    {
        if (_newEntities.Count > 0)
        {
            foreach (Entity e in _newEntities)
            {
                _entities.Add(e);
            }
            _newEntities.Clear();
        }
            
        if (_removedEntities.Count > 0)
        {
            foreach (Entity e in _removedEntities)
            {
                _entities.Remove(e);
                Destroy(e.gameObject);
            }
            _removedEntities.Clear();
        }
    }
    
    private void InitEntities()
        {
            wolfContainer = new GameObject("WolfContainer");
            for (int i = 0; i < _startingWolfs; i++)
            {
                GameObject e = GameObject.CreatePrimitive(PrimitiveType.Cube);
                e.name = "Wolf " + i;
                e.AddComponent<Wolf>();
                
                e.transform.SetParent(wolfContainer.transform);
                e.transform.localScale = new Vector3(0.25f, 0.7f, 0.8f);
                e.transform.position = _grid.GetRandomNodePos();

                e.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Materials.Wolf);
                _entities.Add(e.GetComponent<Entity>());
                e.GetComponent<Wolf>().Init(_grid);
            }

            sheepContainer = new GameObject("SheepContainer");
            for (int i = 0; i < _startingSheep; i++)
            {
                GameObject e = GameObject.CreatePrimitive(PrimitiveType.Cube);
                e.name = "Sheep " + i;
                e.AddComponent<Sheep>();
                
                e.transform.SetParent(sheepContainer.transform);
                e.transform.localScale = new Vector3(0.25f, 0.5f, 0.5f);
                e.transform.position = _grid.GetRandomNodePos();

                e.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Materials.Sheep);
                _entities.Add(e.GetComponent<Entity>());
                e.GetComponent<Sheep>().Init(_grid);
            }

            grassContainer = new GameObject("GrassContainer");
            for (int i = 0; i < _startingGrass; i++)
            {
                GameObject e = GameObject.CreatePrimitive(PrimitiveType.Cube);
                e.name = "Grass " + i;
                e.AddComponent<Grass>();
                
                e.transform.SetParent(grassContainer.transform);
                e.transform.localScale = new Vector3(0.95f, 0.1f, 0.95f);
                e.transform.position = _grid.GetRandomNodePos();


                e.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Materials.Grass);
                _entities.Add(e.GetComponent<Entity>());
                e.GetComponent<Grass>().Init(_grid);
            }
        }

    private void GrowGrass(EventManager.EventMessage message)
    {
        GameObject e = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var childIndex = _entities.Count + 1;
        e.name = "Grass " + childIndex;
        e.AddComponent<Grass>();

        var t = e.transform;
        t.SetParent(grassContainer.transform);
        t.localScale = new Vector3(0.95f, 0.1f, 0.95f);
        t.position = message.node.GetNodeGridPos();

        e.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Materials.Grass);
        e.GetComponent<Grass>().Init(_grid);
            
        _newEntities.Add(e.GetComponent<Entity>());
    }

    private void RemoveEntity(EventManager.EventMessage message)
    {
        var temp = _entities;
        foreach (var e in temp)
        {
            if (e.GetId() == message.id)
            {
                message.node.RemoveEntity(e);
                _removedEntities.Add(e.GetComponent<Entity>()); ;
                break;
            }
        }
    }
    
    public static List<Entity> FindWolvesAroundNode(Node node, int sensingRadius)
    {
        List<Entity> wolves = new List<Entity>();
        if (node == null) return wolves;
        foreach (var e in _entities)
        {
            if (e.GetEntityType() == Entity.EntityType.WOLF)
            {
                if (Vector3.Distance(e.transform.position, node.GetNodeWorldPos()) < sensingRadius)
                {
                    wolves.Add(e);
                }
            }
        }
        return wolves;
    }

    public static Vector3 GetClosestGrassPos(Node node, int sensingRadius)
    {
        List<Entity> grass = new List<Entity>();
        if (node == null) return Vector3.zero;
        foreach (var e in _entities)
        {
            if (e.GetEntityType() == Entity.EntityType.GRASS)
            {
                // TODO fix magic number
                if (Vector3.Distance(e.transform.position, node.GetNodeWorldPos()) < sensingRadius)
                {
                    grass.Add(e);
                }
            }
        }

        if (grass.Count > 0)
        {
            return grass[Random.Range(0, grass.Count)].transform.position;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public static Vector3 GetClosestSheepPos(Node node, int sensingRadius)
    {
        List<Entity> grass = new List<Entity>();
        if (node == null) return Vector3.zero;
        foreach (var e in _entities)
        {
            if (e.GetEntityType() == Entity.EntityType.SHEEP)
            {
                // TODO fix magic number
                if (Vector3.Distance(e.transform.position, node.GetNodeWorldPos()) < sensingRadius)
                {
                    grass.Add(e);
                }
            }
        }

        if (grass.Count > 0)
        {
            return grass[Random.Range(0, grass.Count)].transform.position;
        }
        else
        {
            return Vector3.zero;
        }
    }
    }
}
