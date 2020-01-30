using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FlatEarth
{
    public class FlockingECS : MonoBehaviour
    { 
        public FlockingSettings settings;
    [SerializeField] private List<Bird> _birds;
    private int numberOfBirds = 100;
    private float spawnRadius = 5;
    private int _pointsOnSphere = 300;

    [SerializeField] private GameObject srcGameObject;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numberOfBirds; i++)
        {
            Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * spawnRadius;
            GameObject b = EntityManager.Instantiate(Resources.Load<GameObject>("Prefabs/Bird"), transform.position, Quaternion.identity);
            b.transform.forward = UnityEngine.Random.insideUnitSphere;
            b.GetComponent<Bird>().Initialize(settings);
            _birds.Add(b.GetComponent<Bird>());
        }

        Unity.Entities.EntityManager entityManager = Unity.Entities.World.Active.EntityManager;

        int id = 0;
        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        foreach (var e in entityManager.GetAllEntities())
        {
            id++;
            entityManager.AddComponent<FlockingComponent>(e);
            entityManager.AddComponent<Bird>(e);
            entityManager.SetComponentData(e, new FlockingComponent
            {
                id = id,
                perceptionRadius = settings.perceptionRadius,
                avoidanceRadius = settings.avoidanceRadius,
                numPerceivedFlockmates = 0,
                velocity = transform.forward * startSpeed,
                points = PointsOnSphere(_pointsOnSphere)
            });
        }
    }
    
    /// <summary>
    /// Calculates points on a sphere using golden spiral method https://stackoverflow.com/questions/9600801/evenly-distributing-n-points-on-a-sphere/44164075#44164075
    /// </summary>
    /// <param name="numberOfPoints"></param>
    /// <returns>Vector3[]</returns>
    float3[]  PointsOnSphere(int numberOfPoints)
    {
        float phi = (1 + Mathf.Sqrt (5)) / 2;
        float angleIncrement = Mathf.PI * 2 * phi;

        var points = new float3[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = (float) i / numberOfPoints;
            float inclination = Mathf.Acos (1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin (inclination) * Mathf.Cos (azimuth);
            float y = Mathf.Sin (inclination) * Mathf.Sin (azimuth);
            float z = Mathf.Cos (inclination);
            points[i] = new Vector3 (x, y, z);
        }
        return points;
    }

    // Update is called once per frame
   
    /*
    void Update()
    {
        foreach (var bird in _birds)
        {
            bird.UpdateBoid();
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

                if (sqrDst < settings.perceptionRadius * settings.avoidanceRadius)
                {
                    avgFlockHeading  += birdInFlock.forward;
                    centreOfFlock += birdInFlock.position;
                    numFlockmates += 1;
                    
                    if (sqrDst < settings.avoidanceRadius * settings.avoidanceRadius)
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
*/
}
}