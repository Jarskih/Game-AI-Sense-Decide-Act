using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public enum EntityType
    {
        SHEEP,
        WOLF,
        GRASS
    }

    public abstract void Sense();
    public abstract void Think();
    public abstract void Act();
}
