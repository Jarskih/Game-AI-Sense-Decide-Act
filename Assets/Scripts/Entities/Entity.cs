using System.Collections;
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
        protected Action _lastAction;

        protected Vector3 _targetPos;
        public Vector3 targetPos => _targetPos;

        protected List<Action> _availableActions = new List<Action>();

        public abstract EntityType GetEntityType();
        public abstract void Init(Grid grid);
        public abstract int GetId();
        public abstract void Sense();
        public abstract void Think();
        public abstract void Act();
        public abstract Dictionary<Entity, float> FindFood();
        public abstract void Eat();
        public abstract void Flee();
        public abstract void Wander();
        public abstract void Breed();
        public abstract void Grow();

        protected Action FindBestAction(CurrentState currentState)
        {
            Dictionary<Action, float> actions = new Dictionary<Action, float>();
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
        
        protected Vector3 GetWanderPos(Grid _grid, Stats stats, int[] _wanderAngles)
        {
            int angle = 0;
            float maxTurningDelta = 1; // in degrees
            var nextPos = transform.position + transform.forward * 2;
            if (_grid.IsOutsideGrid(nextPos))
            { 
                // We hit the end of the grid. Turn around
                angle = 25;
                maxTurningDelta = stats.fastTurnSpeed;
                
                Quaternion rotateDir = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + angle, transform.rotation.eulerAngles.z);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateDir, maxTurningDelta);

                return transform.position;
            }
            else
            {
                // Randomly turn left or right or continue straight
                angle = _wanderAngles[Random.Range(0, _wanderAngles.Length)];
                maxTurningDelta = stats.slowTurnSpeed;
                
                Quaternion rotateDir = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + angle, transform.rotation.eulerAngles.z);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateDir, maxTurningDelta);

                return transform.position + transform.forward * 1;
            }
        }
    }
}