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
    public FlockingSettings _settings;
    private int _pointsOnSphere = 300;

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
    private Vector3[] _points;

    public void Initialize (FlockingSettings settings)
    {
        _settings = settings;
        cachedTransform = transform;
        
        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
        
        // Calculate all points on the sphere
        _points = PointsOnSphere(_pointsOnSphere);
    }
    
    private void OnDrawGizmos()
    {
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(centreOfFlockmates, 1f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, avgAvoidanceHeading);
        
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, avgFlockHeading);
    }

    public void UpdateBoid () {
        Vector3 acceleration = Vector3.zero;

        if (target != null) {
            Vector3 offsetToTarget = (target.position - position);
            acceleration = SteerTowards (offsetToTarget) * _settings.targetWeight;
        }

        if (numPerceivedFlockmates != 0)
        {
            centreOfFlockmates /= numPerceivedFlockmates;

            Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - position);

            var alignmentForce = SteerTowards (avgFlockHeading) * _settings.alignWeight;
            var cohesionForce = SteerTowards (offsetToFlockmatesCentre) * _settings.cohesionWeight;
            var separationForce = SteerTowards (avgAvoidanceHeading) *_settings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += separationForce;
        }

        if (IsColliding())
        {
            Vector3 collisionAvoidDir = FindDirection();
            Vector3 collisionAvoidForce = SteerTowards (collisionAvoidDir) * _settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }


        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;

        Vector3 dir = transform.forward;
        dir = velocity / speed;
        speed = Mathf.Clamp (speed, _settings.minSpeed, _settings.maxSpeed);
        velocity = dir * speed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;
    }
    
    Vector3 SteerTowards (Vector3 vector) {
        Vector3 v = vector.normalized * _settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude (v, _settings.maxSteerForce);
    }

    bool IsColliding()
    {
        Vector3 dir = transform.forward;
        if (Physics.SphereCast(position, _settings.boundsRadius, forward, out var hit, _settings.collisionAvoidDst,
            _settings.obstacleMask))
        {
            return true;
        }
        return false;
    }

    Vector3 FindDirection()
    {
        foreach (var p in _points)
        {
            Vector3 direction = cachedTransform.TransformDirection(p);
            Ray ray = new Ray (position, direction);
            if (!Physics.SphereCast (ray, _settings.boundsRadius, _settings.collisionAvoidDst, _settings.obstacleMask)) {
                return direction;
            }
        }
        return forward;
    }

    /// <summary>
    /// Calculates points on a sphere using golden spiral method https://stackoverflow.com/questions/9600801/evenly-distributing-n-points-on-a-sphere/44164075#44164075
    /// </summary>
    /// <param name="numberOfPoints"></param>
    /// <returns>Vector3[]</returns>
    Vector3[]  PointsOnSphere(int numberOfPoints)
    {
        float phi = (1 + Mathf.Sqrt (5)) / 2;
        float angleIncrement = Mathf.PI * 2 * phi;

        _points = new Vector3[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = (float) i / numberOfPoints;
            float inclination = Mathf.Acos (1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin (inclination) * Mathf.Cos (azimuth);
            float y = Mathf.Sin (inclination) * Mathf.Sin (azimuth);
            float z = Mathf.Cos (inclination);
            _points[i] = new Vector3 (x, y, z);
        }
        return _points;
    }
}
    

}
