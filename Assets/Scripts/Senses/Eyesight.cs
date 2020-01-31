using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlatEarth
{
    public class Eyesight : MonoBehaviour
{
    public Dictionary<Entity, float> DetectEntities(Entity.EntityType type, float seeingDistance, float visionAngle)
    {
        var foodNear = EntityManager.FindEntityAround(transform.position, seeingDistance, type);
        var foodInSight = new Dictionary<Entity, float>();
        
        if (foodNear.Count > 0)
        {
            foreach (var food in foodNear)
            {
                var relativePos = food.Key.transform.position - transform.position;
                if (relativePos != Vector3.zero)
                {
                    var rot = Quaternion.LookRotation(relativePos, Vector3.up);
                    if (Quaternion.Angle(Quaternion.identity, rot) < visionAngle)
                    {
                        foodInSight.Add(food.Key, food.Value);
                    }  
                }
            }

            return foodInSight;
        }

        return foodNear;
    }
}
}