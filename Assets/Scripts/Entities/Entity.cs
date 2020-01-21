using UnityEngine;

namespace FlatEarth
{
    public abstract class Entity : MonoBehaviour
    {
        public enum EntityType
        {
            SHEEP,
            WOLF,
            GRASS
        }

        public abstract EntityType GetEntityType();
        public abstract int GetId();
        public abstract void Sense(float deltaTime);
        public abstract void Think(float deltaTime);
        public abstract void Act(float deltaTime);

    }
}