using FlatEarth;
using UnityEngine;
using Unity.Entities;
public class FlockingSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Transform t1, ref FlockingComponent bird) =>
        {
            var birdTemp = bird;
            Vector3 avgFlockHeading = Vector3.zero;
            Vector3 centreOfFlock = Vector3.zero;
            int numFlockmates = 0;
            Vector3 avgAvoidanceHeading = Vector3.zero;
            
            Entities.ForEach((Transform t2, ref FlockingComponent birdInFlock) =>
            {
                if (birdInFlock.id != birdTemp.id)
                {
                    Vector3 offset = t2.position - t1.position;
                    float sqrDst = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                    if (sqrDst < birdTemp.perceptionRadius * birdTemp.avoidanceRadius)
                    {
                        avgFlockHeading += t2.forward;
                        centreOfFlock += t2.position;
                        numFlockmates += 1;

                        if (sqrDst < birdTemp.avoidanceRadius * birdTemp.avoidanceRadius)
                        {
                            avgAvoidanceHeading -= offset / sqrDst;
                        }
                    }
                }
            });

            bird.avgFlockHeading = avgFlockHeading;
            bird.centreOfFlockmates = centreOfFlock;
            bird.avgAvoidanceHeading = avgAvoidanceHeading;
            bird.numPerceivedFlockmates = numFlockmates;
            
            Vector3 acceleration = Vector3.zero;

            if (bird.numPerceivedFlockmates != 0)
            {
                bird.centreOfFlockmates /= bird.numPerceivedFlockmates;

                var centreOfFlockmates = (Vector3)bird.centreOfFlockmates;
                
                Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - t1.position);

                var alignmentForce = SteerTowards (bird.avgFlockHeading, bird) * bird.alignWeight;
                var cohesionForce = SteerTowards (offsetToFlockmatesCentre, bird) * bird.cohesionWeight;
                var separationForce = SteerTowards (bird.avgAvoidanceHeading, bird) *bird.seperateWeight;

                acceleration += alignmentForce;
                acceleration += cohesionForce;
                acceleration += separationForce;
            }

            if (IsColliding(t1, ref bird))
            {
                Vector3 collisionAvoidDir = FindDirection(ref bird, t1);
                Vector3 collisionAvoidForce = SteerTowards (bird.collisionAvoidDir, bird) * bird.avoidCollisionWeight;
                acceleration += collisionAvoidForce;
            }

            var velocity = (Vector3) bird.velocity;
            velocity += acceleration * Time.deltaTime;
            float speed = velocity.magnitude;

            Vector3 dir = t1.forward;
            dir = bird.velocity / speed;
            speed = Mathf.Clamp (speed, bird.minSpeed, bird.maxSpeed);
            bird.velocity = dir * speed;

            bird.position += bird.velocity * Time.deltaTime;
            bird.forward = dir;
            t1.position = bird.position;
            t1.forward = dir;
        });
    }
    
    Vector3 SteerTowards (Vector3 vector, FlockingComponent bird)
    {
        var velocity = (Vector3) bird.velocity;
        Vector3 v = vector.normalized * bird.maxSpeed - velocity;
        return Vector3.ClampMagnitude (v, bird.maxSteerForce);
    }
    
    bool IsColliding(Transform t, ref FlockingComponent bird)
    {
        Vector3 dir = t.forward;
        if (Physics.SphereCast(bird.position, bird.boundsRadius, t.forward, out var hit, bird.collisionAvoidDst))
        {
            return true;
        }
        return false;
    }
    
    Vector3 FindDirection(ref FlockingComponent bird, Transform t)
    {
        foreach (var p in bird.points)
        {
            Vector3 direction = t.TransformDirection(p);
            Ray ray = new Ray (bird.position, direction);
            if (!Physics.SphereCast (ray, bird.boundsRadius, bird.collisionAvoidDst)) {
                return direction;
            }
        }
        return bird.forward;
    }
}
