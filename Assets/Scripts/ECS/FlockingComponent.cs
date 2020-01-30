using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public partial struct FlockingComponent : IComponentData
{
    public int id;
    
    public float minSpeed;
    public float maxSpeed;
    public float perceptionRadius;
    public float avoidanceRadius;
    public float maxSteerForce;

    public float alignWeight;
    public float cohesionWeight;
    public float seperateWeight;
    
    public float boundsRadius;
    public float avoidCollisionWeight;
    public float collisionAvoidDst;
    
    
    public float3 velocity;
    public float3 position;
    public float3 forward;
    public float3[] points;
    public int numPerceivedFlockmates;
    public float3 centreOfFlockmates;
    public float3 avgFlockHeading;
    public float3 offsetToFlockmatesCentre;
    public float3 avgAvoidanceHeading;
    public float3 collisionAvoidDir;
}
