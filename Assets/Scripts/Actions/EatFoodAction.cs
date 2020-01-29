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
        private float _actionDuration = 0.2f; // seconds
        private Entity _targetFood;

        public EatFoodAction()
        {
            addPrecondition("isHungry", true);
            addPrecondition("foundFood", true);
            addEffect("foundFood", false);
            addEffect("isHungry",false);
            cost = 1;
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