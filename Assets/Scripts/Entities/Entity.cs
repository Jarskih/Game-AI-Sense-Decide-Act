using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
        protected CurrentState _currentState = new CurrentState();
        protected Action _currentAction;

        protected Vector3 _targetPos;
        public Vector3 targetPos => _targetPos;

        protected List<Action> _availableActions = new List<Action>();

        public abstract EntityType GetEntityType();
        public abstract int GetId();
        public abstract void Sense();
        public abstract void Think();
        public abstract void Act();
        public abstract Dictionary<Entity, float> FindFood();
        public abstract void Eat();
        public abstract void Flee();
        public abstract void Wander();
        protected Action FindBestAction(CurrentState currentState)
        {
            Dictionary<Action, int> actions = new Dictionary<Action, int>();
            foreach (var action in _availableActions)
            {
                if (action.CanDoAction(currentState))
                {
                    actions.Add(action, action.priority);
                }
            }
            var sorted = actions.OrderByDescending(x => x.Value).ToList();
            if (sorted.Count > 0)
            {
                return sorted[0].Key;
            }
            return null;
        }
    }
}