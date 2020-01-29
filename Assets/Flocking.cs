using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlatEarth
{
    public class Flocking : MonoBehaviour
{
    [SerializeField] private List<Bird> _birds;
    private int numberOfBirds = 50;
    private float spawnRadius = 2;

        // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numberOfBirds; i++)
        {
            Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * spawnRadius;
            GameObject b = Instantiate(Resources.Load<GameObject>("Prefabs/Bird"), pos, Quaternion.identity);
            b.transform.forward = UnityEngine.Random.insideUnitSphere;
            b.GetComponent<Bird>().settings = Resources.Load<BoidSettings>("Data/Settings");
            b.GetComponent<Bird>().Initialize();
            _birds.Add(b.GetComponent<Bird>());

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

                if (sqrDst < bird.settings.perceptionRadius * bird.settings.avoidanceRadius)
                {
                    avgFlockHeading  += birdInFlock.transform.forward;
                    centreOfFlock += birdInFlock.transform.position;
                    numFlockmates += 1;
                    
                    if (sqrDst < bird.settings.avoidanceRadius * bird.settings.avoidanceRadius)
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