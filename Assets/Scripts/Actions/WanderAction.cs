using System.Collections;
using System.Collections.Generic;
using FlatEarth;
using UnityEngine;

namespace FlatEarth
{
public class WanderAction : GoapAction
{
    private Vector3 _wanderTarget = Vector3.zero;
    private float _counter;
    private double _actionDuration;

    public WanderAction()
    {
        addPrecondition("isIdle", true);
    }
    
    protected override void reset()
    {
        _wanderTarget = Vector3.zero;
        targetPos = Vector3.zero;
    }

    public override bool isDone()
    {
        return true;
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        Vector3 pos = agent.GetComponent<Entity>().GetWanderPos();

        if (pos != Vector3.zero)
        {
            targetPos = pos;
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