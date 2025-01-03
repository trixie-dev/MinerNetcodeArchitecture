using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    private State currentState;
    private Dictionary<System.Type, State> states = new Dictionary<System.Type, State>();

    public void Update()
    {
        currentState?.Update();
    }

    public void FixedUpdate()
    {
        currentState?.FixedUpdate();
    }

    public void AddState(State state)
    {
        states[state.GetType()] = state;
    }

    public void SetState<T>() where T : State
    {
        var type = typeof(T);

        if (currentState?.GetType() == type) return;

        if (states.TryGetValue(type, out State newState))
        {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }
    }
}