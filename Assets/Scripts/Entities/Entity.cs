using System.Collections.Generic;
using UnityEngine;

namespace FlatEarth
{
    public abstract class Entity : MonoBehaviour, IGoap
    {
        [SerializeField] protected float _maxHunger = 100;
        [SerializeField] protected float _hunger = 0;

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
        public abstract Entity FindFood();
        
        // GOAP

        public abstract HashSet<KeyValuePair<string, object>> getWorldState();

        public abstract HashSet<KeyValuePair<string, object>> createGoalState();

        public void planFailed(HashSet<KeyValuePair<string, object>> failedGoal)
        {
            // TODO handle failed plans
        }

        public void planFound(HashSet<KeyValuePair<string, object>> goal, Queue<GoapAction> actions)
        {
            // Yay we found a plan for our goal
            Debug.Log ("<color=green>Plan found</color> "+GoapAgent.prettyPrint(actions));
        }

        public void actionsFinished()
        {
            // Everything is done, we completed our actions for this gool. Hooray!
            Debug.Log ("<color=blue>Actions completed</color>");
        }

        public void planAborted(GoapAction aborter)
        {
            // An action bailed out of the plan. State has been reset to plan again.
            // Take note of what happened and make sure if you run the same goal again
            // that it can succeed.
            Debug.Log ("<color=red>Plan Aborted</color> "+GoapAgent.prettyPrint(aborter));
        }

        public abstract bool moveAgent(GoapAction nextAction);

        public abstract Vector3 GetWanderPos();

        public void RemoveHunger()
        {
            _hunger = 0;
        }
    }
}