using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using UnityEngine;

namespace FlatEarth
{
    public class EntityManager : MonoBehaviour
    {
        private int _startingWolfs;
        private int _startingSheep;
        private int _startingGrass;

        private static List<Entity> _entities = new List<Entity>();
        private static List<Entity> _newEntities = new List<Entity>();
        private static List<Entity> _removedEntities = new List<Entity>();
        
        private static List<Entity> _grassList = new List<Entity>();
        private static List<Entity> _sheepList = new List<Entity>();
        private static List<Entity> _wolfList = new List<Entity>();

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
        InitEntities();
    }
    
    private void OnEnable()
    {
        EventManager.StartListening("EntityDied", RemoveEntity);
        EventManager.StartListening("EntityAdded", AddEntity);
        EventManager.StartListening("GrassGrowing", GrowGrass);
    }

    private void OnDisable()
    {
        EventManager.StopListening("EntityDied", RemoveEntity);
        EventManager.StopListening("EntityAdded", AddEntity);
        EventManager.StopListening("GrassGrowing", GrowGrass);
    }

    public void UpdateEntities()
    {
        if (_newEntities.Count > 0)
        {
            foreach (Entity e in _newEntities)
            {
                AddEntityToList(e);
            }
            _newEntities.Clear();
        }
            
        if (_removedEntities.Count > 0)
        {
            foreach (Entity e in _removedEntities)
            {
                RemoveEntityToList(e);
            }
            _removedEntities.Clear();
        }

        if (_grassList.Count > _grid.sizeX * _grid.sizeZ)
        {
            Debug.LogError("Too many grass entities");
        }
    }

    private void AddEntity(EventManager.EventMessage message)
    {
        Entity e = null;
        switch (message.type)
        {
            case Entity.EntityType.GRASS:
                e = CreateEntityGrass(message.node);
                break;
            case Entity.EntityType.SHEEP:
                e = CreateEntitySheep(message.node);
                break;
            case Entity.EntityType.WOLF:
                e = CreateEntityWolf(message.node);
                break;
            default:
                Debug.LogError("No entity type available: " + message.type);
                break;
        }

        if (e != null)
        {
            e.GetComponent<Entity>().Init(_grid);
            e.transform.position = message.node.GetNodeGridPos();
            
            message.node.AddEntity(e);
            
            _newEntities.Add(e.GetComponent<Entity>());
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
        t.position = message.node.GetNodeWorldPos();

        e.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Materials.Grass);
        e.GetComponent<Grass>().Init(_grid);
        
        message.node.AddEntity(e.GetComponent<Entity>());
            
        _newEntities.Add(e.GetComponent<Entity>());
    }

    private Entity CreateEntityGrass(Node targetNode)
    {
                
        GameObject e = GameObject.CreatePrimitive(PrimitiveType.Cube);
        e.name = "Grass " + _grassList.Count;
        e.AddComponent<Grass>();
                
        e.transform.SetParent(grassContainer.transform);
        e.transform.localScale = new Vector3(0.0f, 0.1f, 0.0f);

        e.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Materials.Grass);
        return e.GetComponent<Entity>();
    }
    
    private Entity CreateEntitySheep(Node targetNode)
    {
                
        GameObject e = Instantiate(Resources.Load<GameObject>("Prefabs/Sheep"));
        e.name = "Sheep " + _sheepList.Count;
        e.AddComponent<Sheep>();
                
        e.transform.SetParent(sheepContainer.transform);

        return e.GetComponent<Entity>();
    }

    
    private Entity CreateEntityWolf(Node targetNode)
    {
        GameObject e = Instantiate(Resources.Load<GameObject>("Prefabs/Wolf"));
        e.name = "Wolf " + _wolfList.Count;
        e.AddComponent<Wolf>();
        
        e.transform.SetParent(wolfContainer.transform);

        return e.GetComponent<Entity>();
    }


    private void InitEntities()
        {
            wolfContainer = new GameObject("WolfContainer");
            for (int i = 0; i < _startingWolfs; i++)
            {
                GameObject e = Instantiate(Resources.Load<GameObject>("Prefabs/Wolf"));
                e.name = "Wolf " + i;
                e.AddComponent<Wolf>();
                e.transform.SetParent(wolfContainer.transform);

                AddEntityToList(e.GetComponent<Entity>());
                e.GetComponent<Wolf>().Init(_grid);
            }

            sheepContainer = new GameObject("SheepContainer");
            for (int i = 0; i < _startingSheep; i++)
            {
                GameObject e = Instantiate(Resources.Load<GameObject>("Prefabs/Sheep"));
                e.name = "Sheep " + i;
                e.AddComponent<Sheep>();
                
                e.transform.SetParent(sheepContainer.transform);

                AddEntityToList(e.GetComponent<Entity>());
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

                e.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Materials.Grass);
                AddEntityToList(e.GetComponent<Entity>());
                e.GetComponent<Grass>().Init(_grid);
            }
        }

    private void RemoveEntity(EventManager.EventMessage message)
    {
        var temp = _entities;
        foreach (var e in temp)
        {
            if (e.GetId() == message.id)
            {
              //  Debug.Log("Entity died");
                _removedEntities.Add(e.GetComponent<Entity>()); ;
                break;
            }
        }
    }

    private void RemoveEntityToList(Entity e)
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
        StartCoroutine(DestroyEntity(e));
    }

    IEnumerator DestroyEntity(Entity e)
    {
        yield return new WaitForEndOfFrame();
        Destroy(e.gameObject);
    }

    private void AddEntityToList(Entity e)
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
