using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlatEarth
{
    public class Hearing : MonoBehaviour
    {
        private Dictionary<Entity, float> _foodNear = new Dictionary<Entity, float>();
        
        /// <summary>
        /// Detects entities around game object inside sensing radius
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sensingRadius"></param>
        /// <returns>Dictionary<Entity, float></returns>
        public Dictionary<Entity, float> DetectEntities(Entity.EntityType type, float sensingRadius)
        {
            _foodNear = EntityManager.FindEntityAround(transform.position, sensingRadius, type);
            return _foodNear;
        }
    }
}