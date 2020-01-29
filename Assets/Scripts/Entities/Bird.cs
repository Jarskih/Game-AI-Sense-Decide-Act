using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlatEarth
{

public class Bird : MonoBehaviour
{
    private Quaternion lookDir;

    private Vector3 _targetDir;

    public BoidSettings settings;


    // State
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;
    Vector3 velocity;

    // To update:
    Vector3 acceleration;
    [HideInInspector]
    public Vector3 avgFlockHeading;
    [HideInInspector]
    public Vector3 avgAvoidanceHeading;
    [HideInInspector]
    public Vector3 centreOfFlockmates;
    [HideInInspector]
    public int numPerceivedFlockmates;

    // Cached
    Material material;
    Transform cachedTransform;
    Transform target;

    public void Initialize () {
        cachedTransform = transform;
        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(centreOfFlockmates, 1f);
    }
    
    // Update is called once per frame
    void Update()
    {
        /*
        
        if (birds.Count > 0)
        {
            // TODO flock
        } 
        ;
        if (IsColliding())
        {
            _targetDir = FindDirection();
        }

        lookDir = Quaternion.LookRotation(_targetDir.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(Quaternion.identity,lookDir, 30);

        var newPos = transform.position + transform.forward * 1;
        transform.position = Vector3.MoveTowards(transform.position, newPos, 0.05f);
        */
    }

    public void UpdateBoid () {
        Vector3 acceleration = Vector3.zero;

        if (target != null) {
            Vector3 offsetToTarget = (target.position - position);
            acceleration = SteerTowards (offsetToTarget) * settings.targetWeight;
        }

        if (numPerceivedFlockmates != 0) {
            centreOfFlockmates /= numPerceivedFlockmates;

            Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - position);

            var alignmentForce = SteerTowards (avgFlockHeading) * settings.alignWeight;
            var cohesionForce = SteerTowards (offsetToFlockmatesCentre) * settings.cohesionWeight;
            var separationForce = SteerTowards (avgAvoidanceHeading) * settings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += separationForce;
        }

        if (IsColliding())
        {
            Vector3 collisionAvoidDir = FindDirection();
            Vector3 collisionAvoidForce = SteerTowards (collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;

        Vector3 dir = transform.forward;
        if (speed != 0)
        {
            dir = velocity / speed;
        }
        speed = Mathf.Clamp (speed, settings.minSpeed, settings.maxSpeed);
        velocity = dir * speed;

        position += velocity * Time.deltaTime;
        forward = dir;

        lookDir = Quaternion.LookRotation(centreOfFlockmates, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(Quaternion.identity, lookDir, 5);
        transform.position = position;
        // transform.position = Vector3.MoveTowards(transform.position, position, 1f);
    }
    
    Vector3 SteerTowards (Vector3 vector) {
        Vector3 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude (v, settings.maxSteerForce);
    }

    bool IsColliding()
    {
        Vector3 dir = transform.forward;
        return Physics.Raycast(transform.position, dir, out var hit, settings.collisionAvoidDst);
    }

    Vector3 FindDirection()
    {
        var points = PointsOnSphere(200);
        
        Vector3 bestDir = transform.forward;
        float bestDist = 0;
        foreach (var p in points)
        {
            var direction = p - transform.position;
            if(Physics.Raycast(transform.position,  direction.normalized, out var hit, settings.collisionAvoidDst))
            {
                if (hit.distance > bestDist)
                {
                    bestDist = hit.distance;
                    bestDir = direction;
                }
            }
            else
            {
                return direction;
            }
        }
        return bestDir;
    }

    Vector3[]  PointsOnSphere(int numberOfPoints)
    {
        float phi = (1 + Mathf.Sqrt (5)) / 2;
        float angleIncrement = Mathf.PI * 2 * phi;

        Vector3[] points = new Vector3[numberOfPoints];

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
}
    

}
