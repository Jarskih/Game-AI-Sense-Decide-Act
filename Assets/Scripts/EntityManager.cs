using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

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
        
        [SerializeField] private static List<Entity> _grassList = new List<Entity>();
        [SerializeField] private static List<Entity> _sheepList = new List<Entity>();
        [SerializeField] private static List<Entity> _wolfList = new List<Entity>();

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
                AddEntity(e);
            }
            _newEntities.Clear();
        }
            
        if (_removedEntities.Count > 0)
        {
            foreach (Entity e in _removedEntities)
            {
                RemoveEntity(e);
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
                AddEntity(e.GetComponent<Entity>());
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
                AddEntity(e.GetComponent<Entity>());
                e.GetComponent<Sheep>().Init(_grid);
            }

            grassContainer = new GameObject("GrassContainer");
            for (int i = 0; i < _startingGrass; i++)
            {
                GameObject e = GameObject.CreatePrimitive(PrimitiveType.Cube);
                e.name = "Grass " + i;
                e.AddComponent<Grass>();
                
                e.transform.SetParent(grassContainer.transform);
                e.transform.localScale = new Vector3(0.0f, 0.1f, 0.0f);
                e.transform.position = _grid.GetRandomNodePos();


                e.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Materials.Grass);
                AddEntity(e.GetComponent<Entity>());
                e.GetComponent<Grass>().Init(_grid);
            }
        }

    private void GrowGrass(EventManager.EventMessage message)
    {
        var entities = message.node.GetEntities();

        foreach (var entity in entities)
        {
            if (entity.GetEntityType() == Entity.EntityType.GRASS)
            {
                // Already has grass
                return;
            }
        }
        
        GameObject e = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var childIndex = _entities.Count + 1;
        e.name = "Grass " + childIndex;
        e.AddComponent<Grass>();

        var t = e.transform;
        t.SetParent(grassContainer.transform);
        t.localScale = new Vector3(0.0f, 0.1f, 0.0f);
        t.position = message.node.GetNodeGridPos();

        e.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Materials.Grass);
        e.GetComponent<Grass>().Init(_grid);
        
        message.node.AddEntity(e.GetComponent<Entity>());
            
        _newEntities.Add(e.GetComponent<Entity>());
    }

    private void RemoveEntity(EventManager.EventMessage message)
    {
        var temp = _entities;
        foreach (var e in temp)
        {
            if (e.GetId() == message.id)
            {
                Debug.Log("Entity died");
                _removedEntities.Add(e.GetComponent<Entity>()); ;
                break;
            }
        }
    }

    private void RemoveEntity(Entity e)
    {
        _entities.Remove(e);
        if (e.GetEntityType() == Entity.EntityType.GRASS)
        {
            _grassList.Remove(e);
        }
        else if(e.GetEntityType() == Entity.EntityType.SHEEP)
        {
            _sheepList.Remove(e);
        }
        else if(e.GetEntityType() == Entity.EntityType.WOLF)
        {
            _wolfList.Remove(e);
        }
        Destroy(e.gameObject);
    }

    private void AddEntity(Entity e)
    {
        _entities.Add(e);
        if (e.GetEntityType() == Entity.EntityType.GRASS)
        {
            _grassList.Add(e);
        }
        else if(e.GetEntityType() == Entity.EntityType.SHEEP)
        {
            _sheepList.Add(e);
        }
        else if(e.GetEntityType() == Entity.EntityType.WOLF)
        {
            _wolfList.Add(e);
        }
    }
    
    public static Dictionary<Entity, float> FindWolvesAroundNode(Node node, float sensingRadius)
    {
        Dictionary<Entity, float> wolves = new Dictionary<Entity, float>();
        if (node == null) return wolves;
        foreach (var e in _wolfList)
        {
            float dist = Vector3.Distance(e.transform.position, node.GetNodeWorldPos());
            if (dist < sensingRadius)
            {
                 wolves.Add(e,dist);
             }
        }
        return wolves;
    }
    
    public static Dictionary<Entity, float> FindGrassAroundNode(Node node, float sensingRadius)
    {
        Dictionary<Entity, float> grassList = new Dictionary<Entity, float>();
        if (node == null) return grassList;
        foreach (var e in _grassList)
        {
            float dist = Vector3.Distance(e.transform.position, node.GetNodeWorldPos());
            if (dist < sensingRadius)
            {
                grassList.Add(e,dist);
            }
        }
        return grassList;
    }
    
    public static Dictionary<Entity, float> FindEntityAround(Vector3 pos, float sensingRadius, Entity.EntityType type)
    {
        Dictionary<Entity, float> animals = new Dictionary<Entity, float>();
        
        foreach (var e in _entities)
        {
            if (e.GetEntityType() == type)
            {         
                float dist = Vector3.Distance(e.transform.position, pos);
                if (dist < sensingRadius)
                {
                    animals.Add(e,dist);
                }
            }
        }
        return animals;
    }
    }
}
