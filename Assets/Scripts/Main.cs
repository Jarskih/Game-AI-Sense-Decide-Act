using System.Collections.Generic;
using UnityEngine;

namespace FlatEarth
{
    public class Main : MonoBehaviour
    {
        private readonly int _gridSizeX = 50;
        private readonly int _gridSizeY = 1;
        private readonly int _gridSizeZ = 50;
        
        private readonly int _startingWolfs = 5;
        private readonly int _startingSheep = 20;
        private readonly int _startingGrass = 5;

        private float timer;
        private float frameTime = 10f;
        
        [SerializeField] private Grid _grid;
        private EntityManager _entityManager;
        private SelectAnimal _selectAnimal;
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

            _selectAnimal = FindObjectOfType<SelectAnimal>();
            _selectAnimal.Init(_grid);
        }

        // Update is called once per frame
        void Update()
        {
            _entityManager.UpdateUI(); // Update total number of entities in the scene
            _selectAnimal.UpdateUI(); // Update UI of the selected animal
            
            foreach (var entity in  _entityManager.entities)
            {
                timer++;
                if(timer > frameTime)
                {
                    timer = 0;
                    entity.Sense();
                    entity.Think();
                }
                entity.Act();
            }
            
            _entityManager.UpdateEntities(); // Add and remove entities to/from list
        }
    }
}