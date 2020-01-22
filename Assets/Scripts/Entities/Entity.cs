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
        public abstract void Sense();
        public abstract void Think();
        public abstract void Act();

    }
}