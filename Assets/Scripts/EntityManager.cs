using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlatEarth
{
    public class EntityManager : MonoBehaviour
    {
        private IntVariable _numberOfGrass;
        private IntVariable _numberOfSheep;
        private IntVariable _numberOfWolves;
        
        private int _startingWolfs;
        private int _startingSheep;
        private int _startingGrass;

        private static List<Entity> _entities = new List<Entity>();
        private static List<Entity> _grassList = new List<Entity>();
        private static List<Entity> _sheepList = new List<Entity>();
        private static List<Entity> _wolfList = new List<Entity>();

        // Keep track of index for naming entities
        private int _grassIndex = 0;
        private int _sheepIndex = 0;
        private int _wolfIndex = 0;
        
        public List<Entity> entities => _entities;

        private static List<Entity> _addedEntities = new List<Entity>();
        private static List<Entity> _removedEntities = new List<Entity>();

    // Used to organize scene
    private GameObject _wolfContainer;
    private GameObject _sheepContainer;
    private GameObject _grassContainer;
    
    private WorldGrid _worldGrid;

    public void Init(WorldGrid worldGrid, int wolf, int sheep, int grass)
    {
        _startingWolfs = wolf;
        _startingSheep = sheep;
        _startingGrass = grass;

        _numberOfGrass = Resources.Load<IntVariable>("Data/NumberOfGrass");
        _numberOfSheep = Resources.Load<IntVariable>("Data/NumberOfSheep");
        _numberOfWolves = Resources.Load<IntVariable>("Data/NumberOfWolves");

        _worldGrid = worldGrid;
        InitEntities();
    }
    
    private void OnEnable()
    {
        EventManager.StartListening("EntityDied", RemoveEntity);
        EventManager.StartListening("EntityAdded", AddEntity);
    }

    private void OnDisable()
    {
        EventManager.StopListening("EntityDied", RemoveEntity);
        EventManager.StopListening("EntityAdded", AddEntity);
    }

    private void CreateEntity(Entity.EntityType entity, Node targetNode)
    {
        Entity e = null;
        switch (entity)
        {
            case Entity.EntityType.GRASS:
                e = CreateEntityGrass(targetNode);
                break;
            case Entity.EntityType.SHEEP:
                e = CreateEntitySheep(targetNode);
                break;
            case Entity.EntityType.WOLF:
                e = CreateEntityWolf(targetNode);
                break;
            default:
                Debug.LogError("No entity type available: " + entity);
                break;
        }
        
        e.transform.position = targetNode.GetNodeWorldPos();
        e.GetComponent<Entity>().Init(_worldGrid);
        targetNode.AddEntity(e);
        _addedEntities.Add(e);
    }

    private void AddEntity(EventManager.EventMessage message)
    {
        if (message.node.CanAddEntity(message.type))
        {
            CreateEntity(message.type, message.node);
        }
        else
        {
            Debug.LogError("Cant spawn here");
        }
    }
    private void InitEntities()
    {
        _wolfContainer = new GameObject("WolfContainer");
        for (int i = 0; i < _startingWolfs; i++)
        {
            GameObject e = Instantiate(Resources.Load<GameObject>("Prefabs/Wolf"));
            e.name = "Wolf " + _wolfIndex;
            e.AddComponent<Wolf>();
            e.transform.SetParent(_wolfContainer.transform);

            AddEntityToList(e.GetComponent<Entity>());
            e.GetComponent<Wolf>().Init(_worldGrid);
            _wolfIndex++;
        }

        _sheepContainer = new GameObject("SheepContainer");
        for (int i = 0; i < _startingSheep; i++)
        {
            GameObject e = Instantiate(Resources.Load<GameObject>("Prefabs/Sheep"));
            e.name = "Sheep " + _sheepIndex;
            e.AddComponent<Sheep>();
                
            e.transform.SetParent(_sheepContainer.transform);

            AddEntityToList(e.GetComponent<Entity>());
            e.GetComponent<Sheep>().Init(_worldGrid);
            _sheepIndex++;
        }

        _grassContainer = new GameObject("GrassContainer");
        for (int i = 0; i < _startingGrass; i++)
        {
            GameObject e = GameObject.CreatePrimitive(PrimitiveType.Cube);
            e.name = "Grass " + _grassIndex;
            e.AddComponent<Grass>();
                
            e.transform.SetParent(_grassContainer.transform);
            e.transform.localScale = new Vector3(0.0f, 0.1f, 0.0f);
            e.transform.position = _worldGrid.GetRandomNodePos();

            e.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Materials.Grass);
            AddEntityToList(e.GetComponent<Entity>());
            e.GetComponent<Grass>().Init(_worldGrid);
            _grassIndex++;
        }
    }
    
    private Entity CreateEntityGrass(Node targetNode)
    {
                
        GameObject e = GameObject.CreatePrimitive(PrimitiveType.Cube);
        e.name = "Grass " + _grassIndex;
        _grassIndex++;
        e.AddComponent<Grass>();
                
        e.transform.SetParent(_grassContainer.transform);
        e.transform.localScale = new Vector3(0.0f, 0.1f, 0.0f);

        e.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Materials.Grass);
        return e.GetComponent<Entity>();
    }
    
    private Entity CreateEntitySheep(Node targetNode)
    {
                
        GameObject e = Instantiate(Resources.Load<GameObject>("Prefabs/Sheep"), _sheepContainer.transform, true);
        e.name = "Sheep " + _sheepIndex;
        _sheepIndex++;
        e.AddComponent<Sheep>();

        return e.GetComponent<Entity>();
    }

    
    private Entity CreateEntityWolf(Node targetNode)
    {
        GameObject e = Instantiate(Resources.Load<GameObject>("Prefabs/Wolf"), _wolfContainer.transform, true);
        e.name = "Wolf " + _wolfIndex;
        _wolfIndex++;
        e.AddComponent<Wolf>();

        return e.GetComponent<Entity>();
    }
    private void RemoveEntity(EventManager.EventMessage message)
    {
        if (message.entity == null)
        {
            Debug.LogError("entity was null");
            return;
        }
        _removedEntities.Add(message.entity);
    }

    private void RemoveEntityFromList(Entity entity)
    {
        _entities.Remove(entity);
        if (entity.GetEntityType() == Entity.EntityType.GRASS)
        {
            _grassList.Remove(entity);
        }
        else if(entity.GetEntityType() == Entity.EntityType.SHEEP)
        {
            _sheepList.Remove(entity);
        }
        else if(entity.GetEntityType() == Entity.EntityType.WOLF)
        {
            _wolfList.Remove(entity);
        }
        Destroy(entity.gameObject);
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

    public void UpdateEntities()
    {
        foreach (var entity in _addedEntities)
        {
            AddEntityToList(entity);
        }
        _addedEntities.Clear();

        foreach (var entity in _removedEntities)
        {
            RemoveEntityFromList(entity);
        }
        _removedEntities.Clear();
    }

    public void UpdateUI()
    {
        _numberOfGrass.Value = _grassList.Count;
        _numberOfSheep.Value = _sheepList.Count;
        _numberOfWolves.Value = _wolfList.Count;
    }
    }
}
