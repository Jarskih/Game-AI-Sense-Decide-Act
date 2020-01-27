using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlatEarth
{
    public class EatFoodAction : GoapAction
    {
        private bool _ateFood = false;
        private float _counter = 0;
        private float _actionDuration = 2; // seconds
        private Entity _targetFood;

        public EatFoodAction()
        {
            addPrecondition("isHungry", true);
            addEffect("isHungry",false);
            cost = 2;
        }

        protected override void reset()
        {
            _ateFood = false;
            _counter = 0;
        }

        public override bool isDone()
        {
            return _ateFood;
        }

        public override bool checkProceduralPrecondition(GameObject agent)
        {
            Dictionary<Entity, float> entityNear = agent.GetComponent<Entity>().FindFood();
            if (entityNear != null)
            {
                // Sort the dictionary to get the highest priority item
                var ordered = entityNear.OrderByDescending(x => x.Value).ToList();
                if (ordered.Count > 0)
                {
                    var entity = ordered[0].Key;
                    base.target = entity.gameObject;
                    _targetFood = entity;
                    return true;
                }
            }
            return false;
        }

        public override bool perform(GameObject agent)
        {
            _counter += Time.deltaTime;

            if (_counter > _actionDuration) {
                _ateFood = true;
                agent.GetComponent<Entity>().RemoveHunger();
                EventManager.EventMessage message = new EventManager.EventMessage(_targetFood.GetId());
                EventManager.TriggerEvent("EntityDied", message);
            }
            return true;
        }

        public override bool requiresInRange()
        {
            return true;
        }
    }
}