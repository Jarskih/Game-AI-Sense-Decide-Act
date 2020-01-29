using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentState
{
    private Dictionary<string, bool> _states = new Dictionary<string, bool>();

    public bool GetState(string state)
    {
        if (_states.ContainsKey(state))
        {
            return _states[state];
        }
        Debug.LogError("State doesn't exist");
        return false;
    }
    
    public void UpdateState(string state, bool value)
    {
        if (_states.ContainsKey(state))
        {
            _states[state] = value;
        }
        else
        {
            _states.Add(state, value);
        }
    }
}
