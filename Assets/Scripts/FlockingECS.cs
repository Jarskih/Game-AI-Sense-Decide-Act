using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace FlatEarth
{
    /// <summary>
    /// Optimized version of flocking using unity Job system.
    /// Makes it possible to spawn 10 times more birds but sometimes birds do not detect walls properly.
    /// </summary>
    
    public class FlockingECS : MonoBehaviour
    {
        private FlockingSettings _settings;
        private List<Bird> _birds = new List<Bird>();
        private int numberOfBirds = 50;
        private float spawnRadius = 5;
        private int _pointsOnSphere = 300;

        private NativeArray<float3> positions;
        private NativeArray<float3> avgFlockHeading;
        private NativeArray<float3> centreOfFlock;
        private NativeArray<float3> avgAvoidanceHeading;
        private NativeArray<int> numFlockmates;
        private NativeList<JobHandle> jobHandles;
        
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
            }
        }

        void Update()
        {
            // Allocate arrays of information sent to a job
            jobHandles = new NativeList<JobHandle>(Allocator.TempJob);
            positions = new NativeArray<float3>(_birds.Count, Allocator.TempJob);
            avgFlockHeading = new NativeArray<float3>(_birds.Count, Allocator.TempJob);
            centreOfFlock = new NativeArray<float3>(_birds.Count, Allocator.TempJob);
            avgAvoidanceHeading = new NativeArray<float3>(_birds.Count, Allocator.TempJob);
            numFlockmates = new NativeArray<int>(_birds.Count, Allocator.TempJob);
            for(var b = 0; b < _birds.Count; b++)
            {
                positions[b] = _birds[b].transform.position;
            }

            for (int i = 0; i < _birds.Count; i++)
            {
                // Create job with the necessary information for calculating flocking behavior
                TestJob test = new TestJob
                {
                    numberOfBirds = _birds.Count,
                    perceptionRadius = 2.5f,
                    avoidanceRadius = 1,
                    positions = positions,
                    avgFlockHeadingWrite = avgFlockHeading,
                    centreOfFlockWrite = centreOfFlock,
                    avgAvoidanceHeadingWrite = avgAvoidanceHeading,
                    numFlockmatesWrite = numFlockmates,
                };

                // Schedule job to be executed
                JobHandle jobHandle = test.Schedule(_birds.Count, 50);
                jobHandles.Add(jobHandle);
            }

            // Complete all jobs
            JobHandle.CompleteAll(jobHandles);
            
            // Update bird properties
            for (int i = 0; i < _birds.Count; i++)
            {
                _birds[i].avgHeading = avgFlockHeading[i];
                _birds[i].centreOfFlock = centreOfFlock[i];
                _birds[i].avgAvoidance = avgAvoidanceHeading[i];
                _birds[i].numOfBirds = numFlockmates[i];
                _birds[i].UpdateBird();
            }

            // Dispose arrays
            jobHandles.Dispose();
            positions.Dispose();
            avgFlockHeading.Dispose();
            centreOfFlock.Dispose();
            avgAvoidanceHeading.Dispose();
            numFlockmates.Dispose();
        }

        
        /// <summary>
        /// Burst compiled job for finding clock direction
        /// </summary>
        [BurstCompile]
        private struct TestJob : IJobParallelFor
        {
            // Updated
            [NativeDisableContainerSafetyRestriction] public NativeArray<float3> avgFlockHeadingWrite;
            [NativeDisableContainerSafetyRestriction] public NativeArray<float3> centreOfFlockWrite;
            [NativeDisableContainerSafetyRestriction] public NativeArray<float3> avgAvoidanceHeadingWrite;
            [NativeDisableContainerSafetyRestriction] public NativeArray<int> numFlockmatesWrite;
            // Static
            [ReadOnly] public int numberOfBirds;
            [ReadOnly] public NativeArray<float3> positions;
            [ReadOnly] public float perceptionRadius;
            [ReadOnly] public float avoidanceRadius;

            public void Execute(int index)
            {
                for (int i = 0; i < numberOfBirds; i++)
                {
                    if (i == index)
                    {
                        continue;
                    }

                    float3 offset = positions[i] - positions[index];
                    float sqrDst = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                    if (sqrDst < perceptionRadius * avoidanceRadius)
                    {
                        avgFlockHeadingWrite[index] += positions[i];
                        centreOfFlockWrite[index] += positions[i];
                        numFlockmatesWrite[index] += 1;

                        if (sqrDst < avoidanceRadius * avoidanceRadius)
                        {
                            avgAvoidanceHeadingWrite[index] -= offset / sqrDst;
                        }
                    }
                }
            }
        }
    }
}