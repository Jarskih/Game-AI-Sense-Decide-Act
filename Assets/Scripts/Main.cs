using System.Collections.Generic;
using UnityEngine;

namespace FlatEarth
{
    public class Main : MonoBehaviour
    {
        private readonly int _gridSizeX = 50;
        private readonly int _gridSizeY = 1;
        private readonly int _gridSizeZ = 50;
        
        private readonly int _startingWolfs = 10;
        private readonly int _startingSheep = 50;
        private readonly int _startingGrass = 50;

        private int frame = 10;
        
        [SerializeField] private Grid _grid;
        private EntityManager _entityManager;
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
            
        }

        // Update is called once per frame
        void Update()
        {
            foreach (var entity in _entityManager.entities)
            {
                entity.Sense();
                frame++;
                if (frame > 60)
                {
                    entity.Think();
                }

                entity.Act();
            }

            _entityManager.UpdateEntities();
        }
    }
}