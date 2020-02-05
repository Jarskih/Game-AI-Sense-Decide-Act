using System.Collections.Generic;
using UnityEngine;

namespace FlatEarth
{
    /// <summary>
    /// Flocking behaviour
    /// </summary>
    
    public class Flocking : MonoBehaviour
    { 
        private FlockingSettings _settings;
        private List<Bird> _birds = new List<Bird>();
        private int numberOfBirds = 50;
        private float spawnRadius = 5;
        
        void Start()
    {
        // Load flocking settings
        _settings = Resources.Load<FlockingSettings>("Data/FlockingSettings");
        
        // Create birds
        for (int i = 0; i < numberOfBirds; i++)
        {
            Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * spawnRadius;
            GameObject b = Instantiate(Resources.Load<GameObject>("Prefabs/Bird"), pos, Quaternion.identity);
            b.transform.forward = UnityEngine.Random.insideUnitSphere;
            b.GetComponent<Bird>().Initialize(_settings);
            _birds.Add(b.GetComponent<Bird>());
            b.transform.SetParent(transform);
        }
    }
        
     void Update()
    {
        // Calculate heading for each bird in scene
        foreach (var bird in _birds)
        {
            // Reset variables
            Vector3 avgHeading = Vector3.zero;
            Vector3 centreOfFlock = Vector3.zero;
            int numOfBirds = 0;
            Vector3 avgAvoidance = Vector3.zero;
            
            // Loop through every bird in the flock and add vectors to the bird's heading
            foreach (var birdInFlock in _birds)
            {
                if (birdInFlock == bird)
                {
                    continue;
                }
                Vector3 offset = birdInFlock.transform.position - bird.transform.position;
                float squaredDist = offset.sqrMagnitude;

                if (squaredDist < _settings.perceptionRadius * _settings.avoidanceRadius)
                {
                    avgHeading  += birdInFlock.transform.forward;
                    centreOfFlock += birdInFlock.transform.position;
                    numOfBirds += 1;
                    
                    if (squaredDist < _settings.avoidanceRadius * _settings.avoidanceRadius)
                    {
                        avgAvoidance -= offset / squaredDist;
                    }
                }
            }
            
            // Assign variables to the bird
            bird.avgHeading = avgHeading;
            bird.centreOfFlock = centreOfFlock;
            bird.avgAvoidance = avgAvoidance;
            bird.numOfBirds = numOfBirds;
            
            // Update heading
            bird.UpdateBird();
        } 
    }
    }
}