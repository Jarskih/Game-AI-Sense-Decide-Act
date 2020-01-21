using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.Events;

namespace FlatEarth
{
    public class Main : MonoBehaviour
    {
        private readonly int _gridSizeX = 50;
        private readonly int _gridSizeY = 1;
        private readonly int _gridSizeZ = 50;

        private readonly int _startingWolfs = 10;
        private readonly int _startingSheeps = 20;
        private readonly int _startingGrass = 30;

        [SerializeField] private Grid _grid;
        [SerializeField] private List<Entity> _entities = new List<Entity>();

        // Used to organize scene
        private GameObject wolfContainer;
        private GameObject sheepContainer;
        private GameObject grassContainer;
        
        
        // Start is called before the first frame update
        void Start()
        {
            GameObject eventManager = new GameObject("EventManager");
            eventManager.AddComponent<EventManager>();

            _grid = new Grid();
            if (!_grid.Init(_gridSizeX, _gridSizeY, _gridSizeZ))
            {
                Debug.LogError("Error creating grid");
            }

            InitEntities();
            InitListeners();
        }

        // Update is called once per frame
        void Update()
        {
            foreach (var entity in _entities)
            {
                entity.Sense(Time.deltaTime);
            }

            foreach (var entity in _entities)
            {
                entity.Think(Time.deltaTime);
            }

            foreach (var entity in _entities)
            {
                entity.Act(Time.deltaTime);
            }
        }
        
        public static void GetFreeNode()
        {
            
        }

        private void InitEntities()
        {
            wolfContainer = new GameObject("WolfContainer");
            for (int i = 0; i < _startingWolfs; i++)
            {
                GameObject e = GameObject.CreatePrimitive(PrimitiveType.Cube);
                e.name = "Wolf " + i;
                e.AddComponent<Wolf>();

                var t = e.transform;
                t.SetParent(wolfContainer.transform);
                t.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                t.position = _grid.GetRandomNodePos();

                e.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Materials.Wolf);
                _entities.Add(e.GetComponent<Entity>());
                e.GetComponent<Wolf>().Init(_grid);
            }

            sheepContainer = new GameObject("SheepContainer");
            for (int i = 0; i < _startingSheeps; i++)
            {
                GameObject e = GameObject.CreatePrimitive(PrimitiveType.Cube);
                e.name = "Sheep " + i;
                e.AddComponent<Sheep>();

                var t = e.transform;
                t.SetParent(sheepContainer.transform);
                t.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                t.position = _grid.GetRandomNodePos();

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

                var t = e.transform;
                t.SetParent(grassContainer.transform);
                t.localScale = new Vector3(0.95f, 0.1f, 0.95f);
                t.position = _grid.GetRandomNodePos();


                e.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Materials.Grass);
                _entities.Add(e.GetComponent<Entity>());
                e.GetComponent<Grass>().Init(_grid);
            }
        }
        
        private void InitListeners()
        {
        EventManager.StartListening("GrassSpreading", GrowGrass);
        EventManager.StartListening("GrassDied", RemoveGrass);
        }

        private void GrowGrass(EventManager.EventMessage message)
        {
            GameObject e = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var childIndex = grassContainer.transform.childCount + 1;
            e.name = "Grass " + childIndex;
            e.AddComponent<Grass>();

            var t = e.transform;
            t.SetParent(grassContainer.transform);
            t.localScale = new Vector3(0.95f, 0.1f, 0.95f);
            t.position = message.node.GetNodePos();

            e.GetComponent<MeshRenderer>().material = Resources.Load<Material>(Materials.Grass);
            _entities.Add(e.GetComponent<Entity>());
            e.GetComponent<Grass>().Init(_grid);
        }

        private void RemoveGrass(EventManager.EventMessage message)
        {
            var temp = _entities;
            foreach (var e in temp)
            {
                if (e.GetId() == message.id)
                {
                    _entities.Remove(e);
                    message.node.RemoveEntity(e);
                    GameObject.Destroy(e.gameObject);
                    break;
                }
            }
        }
    }
}