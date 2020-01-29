using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlatEarth
{
public class WanderAction : GoapAction
{
    private Entity _target;
    private float _counter;
    private double _actionDuration;

    public WanderAction()
    {
        addEffect("foundFood", true);
        cost = 1;
    }
    
    protected override void reset()
    {
        targetPos = Vector3.zero;
    }

    public override bool isDone()
    {
        return targetPos != Vector3.zero;
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        var food = agent.GetComponent<Entity>().FindFood();

        if (food != null)
        {
            target = food.gameObject;
            return true;
        }
        return false;
    }

    public override bool perform(GameObject agent)
    {
        return true;
    }

    public override bool requiresInRange()
    {
        return true;
    }
}
}