using System.Collections.Generic;
using UnityEngine;

namespace FlatEarth
{
    public class Main : MonoBehaviour
    {
        private readonly int _gridSizeX = 50;
        private readonly int _gridSizeY = 1;
        private readonly int _gridSizeZ = 50;
        
        private readonly int _startingWolfs = 2;
        private readonly int _startingSheep = 10;
        private readonly int _startingGrass = 10;

        private float timer;
        private float frameTime = 0.2f;
        
        private Grid _grid;
        private EntityManager _entityManager;
        private CreateUIElement _createUiElement;
        void Start()
        {
            gameObject.AddComponent<EventManager>();

            // Create grid
            _grid = new Grid();
            if (!_grid.Init(_gridSizeX, _gridSizeY, _gridSizeZ))
            {
                Debug.LogError("Error creating grid");
            }
            
            // Create entities
            _entityManager = gameObject.AddComponent<EntityManager>();
            _entityManager.Init(_grid, _startingWolfs, _startingSheep, _startingGrass);

            _createUiElement = FindObjectOfType<CreateUIElement>();
            _createUiElement.StartListeningForEvents();

        }

        // Update is called once per frame
        void Update()
        {
            foreach (var entity in _entityManager.entities)
            {
                timer += Time.deltaTime;
                if(timer > frameTime)
                {
                    timer = 0;
                    entity.Sense();
                    entity.Think();
                }

                entity.Act();
            }

            _entityManager.UpdateEntities();
        }
    }
}