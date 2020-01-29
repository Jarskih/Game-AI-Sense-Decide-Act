using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FlatEarth;
using UnityEngine;

namespace FlatEarth
{
public class WanderAction : GoapAction
{
    private Vector3 _wanderTarget = Vector3.zero;
    private Entity _target;
    private float _counter;
    private double _actionDuration;

    public WanderAction()
    {
        addPrecondition("isHungry", false);
        addEffect("eat", true);
        cost = 3;
    }
    
    protected override void reset()
    {
        _wanderTarget = Vector3.zero;
        targetPos = Vector3.zero;
    }

    public override bool isDone()
    {
        return _target != null;
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        Vector3 pos = agent.GetComponent<Entity>().GetWanderPos();

        if (pos != Vector3.zero)
        {
            targetPos = pos;
            _wanderTarget = pos;
            return true;
        }

        return false;
    }

    public override bool perform(GameObject agent)
    {
        Dictionary<Entity, float> entityNear = agent.GetComponent<Entity>().FindFood();
        if (entityNear != null)
        {
            // Sort the dictionary to get the highest priority item
            var ordered = entityNear.OrderByDescending(x => x.Value).ToList();
            if (ordered.Count > 0)
            {
                var entity = ordered[0].Key;
                _target = entity;
            }
        }
        return true;
    }

    public override bool requiresInRange()
    {
        return true;
    }
}
}