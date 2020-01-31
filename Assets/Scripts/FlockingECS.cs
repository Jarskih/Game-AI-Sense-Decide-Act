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
    public class FlockingECS : MonoBehaviour
    {
        public FlockingSettings settings;
        [SerializeField] private List<Bird> _birds = new List<Bird>();
        private int numberOfBirds = 50;
        private float spawnRadius = 5;
        private int _pointsOnSphere = 300;

        [SerializeField] private GameObject srcGameObject;
        
        private NativeArray<float3> positions;
        private NativeArray<float3> avgFlockHeading;
        private NativeArray<float3> centreOfFlock;
        private NativeArray<float3> avgAvoidanceHeading;
        private NativeArray<int> numFlockmates;
        private NativeList<JobHandle> jobHandles;

        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < numberOfBirds; i++)
            {
                Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * spawnRadius;
                GameObject b = Instantiate(Resources.Load<GameObject>("Prefabs/Bird"), pos,
                    Quaternion.identity);
                b.transform.forward = UnityEngine.Random.insideUnitSphere;
                b.GetComponent<Bird>().Initialize(settings);
                _birds.Add(b.GetComponent<Bird>());
            }
            

        }

        void Update()
        {
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

                JobHandle jobHandle = test.Schedule(_birds.Count, 50);
                jobHandles.Add(jobHandle);
            }

            JobHandle.CompleteAll(jobHandles);

            for (int i = 0; i < _birds.Count; i++)
            {
                _birds[i].avgFlockHeading = avgFlockHeading[i];
                _birds[i].centreOfFlockmates = centreOfFlock[i];
                _birds[i].avgAvoidanceHeading = avgAvoidanceHeading[i];
                _birds[i].numPerceivedFlockmates = numFlockmates[i];
                _birds[i].UpdateBoid();
            }

            jobHandles.Dispose();
            positions.Dispose();
            avgFlockHeading.Dispose();
            centreOfFlock.Dispose();
            avgAvoidanceHeading.Dispose();
            numFlockmates.Dispose();
        }

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