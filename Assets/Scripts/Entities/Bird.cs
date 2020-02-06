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
    Vector3 velocity;

    // To update:
    Vector3 acceleration;
    [HideInInspector]
    public Vector3 avgHeading;
    [HideInInspector]
    public Vector3 avgAvoidance;
    [HideInInspector]
    public Vector3 centreOfFlock;
    [HideInInspector]
    public int numOfBirds;

    // Cached
    Material material;
    Transform target;
    private Vector3[] _points;

    public void Initialize (FlockingSettings settings)
    {
        _settings = settings;
        
        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
        
        // Calculate all points on the sphere
        _points = PointsOnSphere(_pointsOnSphere);
    }

    public void UpdateBird () {
        Vector3 steeringVector = Vector3.zero;

        if (target != null) {
            Vector3 offsetToTarget = (target.position - transform.position);
            steeringVector = TurnTowards (offsetToTarget) * _settings.targetWeight;
        }

        if (numOfBirds != 0)
        {
            centreOfFlock /= numOfBirds;

            Vector3 offsetToFlockmatesCentre = (centreOfFlock - transform.position);

            var alignmentForce = TurnTowards (avgHeading) * _settings.alignWeight;
            var cohesionForce = TurnTowards (offsetToFlockmatesCentre) * _settings.cohesionWeight;
            var separationForce = TurnTowards (avgAvoidance) *_settings.seperateWeight;

            steeringVector += alignmentForce;
            steeringVector += cohesionForce;
            steeringVector += separationForce;
        }

        if (IsColliding())
        {
            Vector3 collisionAvoidDir = FindDirection();
            Vector3 collisionAvoidForce = TurnTowards (collisionAvoidDir) * _settings.avoidCollisionWeight;
            steeringVector += collisionAvoidForce;
        }


        velocity += steeringVector * Time.deltaTime;
        float speed = velocity.magnitude;

        Vector3 dir = transform.forward;
        dir = velocity / speed;
        speed = Mathf.Clamp (speed, _settings.minSpeed, _settings.maxSpeed);
        velocity = dir * speed;

        transform.position += velocity * Time.deltaTime;
        transform.forward = dir;
    }
    
    Vector3 TurnTowards (Vector3 vector) {
        Vector3 v = vector.normalized * _settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude (v, _settings.maxSteerForce);
    }

    bool IsColliding()
    {
        if (Physics.SphereCast(transform.position, _settings.boundsRadius, transform.forward, out var hit, _settings.collisionAvoidDst))
        {
            return true;
        }
        return false;
    }

    Vector3 FindDirection()
    {
        foreach (var p in _points)
        {
            Vector3 direction = transform.TransformDirection(p);
            Ray ray = new Ray (transform.position, direction);
            if (!Physics.SphereCast (ray, _settings.boundsRadius, _settings.collisionAvoidDst)) {
                return direction;
            }
        }
        return transform.forward;
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
