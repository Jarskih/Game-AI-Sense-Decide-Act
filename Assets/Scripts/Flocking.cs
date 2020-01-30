using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace FlatEarth
{
    public class Flocking : MonoBehaviour
    { 
        private FlockingSettings _settings;
        private List<Bird> _birds = new List<Bird>();
        private int numberOfBirds = 100;
        private float spawnRadius = 5;

        // Start is called before the first frame update
    void Start()
    {
        _settings = Resources.Load<FlockingSettings>("Data/FlockingSettings");
        
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

    // Update is called once per frame
    void Update()
    {
        foreach (var bird in _birds)
        {
            Vector3 avgFlockHeading = Vector3.zero;
            Vector3 centreOfFlock = Vector3.zero;
            int numFlockmates = 0;
            Vector3 avgAvoidanceHeading = Vector3.zero;
            foreach (var birdInFlock in _birds)
            {
                if (birdInFlock == bird)
                {
                    continue;
                }
                Vector3 offset = birdInFlock.transform.position - bird.transform.position;
                float sqrDst = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                if (sqrDst < _settings.perceptionRadius * _settings.avoidanceRadius)
                {
                    avgFlockHeading  += birdInFlock.transform.forward;
                    centreOfFlock += birdInFlock.transform.position;
                    numFlockmates += 1;
                    
                    if (sqrDst < _settings.avoidanceRadius * _settings.avoidanceRadius)
                    {
                        avgAvoidanceHeading -= offset / sqrDst;
                    }
                }
            }

            bird.avgFlockHeading = avgFlockHeading;
            bird.centreOfFlockmates = centreOfFlock;
            bird.avgAvoidanceHeading = avgAvoidanceHeading;
            bird.numPerceivedFlockmates = numFlockmates;
            
            bird.UpdateBoid();
        } 
    }
}

}