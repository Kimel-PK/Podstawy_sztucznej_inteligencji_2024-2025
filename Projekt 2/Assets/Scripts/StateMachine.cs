using System;
using System.Collections.Generic;

public class StateMachine {
    public AgentState currentState;

    private readonly List<Transition> transitions = new();

    private bool isOn;

    public void Start() {
        currentState.OnEnter();
        isOn = true;
    }

    public void Update() {
        if (!isOn)
            return;
        
        foreach (Transition transition in transitions) {
            if (!transition.EvaluateCondition())
                continue;

            if (currentState == transition.newState) {
                currentState?.Update();
                return;
            }

            currentState?.OnExit();
            currentState = transition.newState;
            currentState?.OnEnter();
            return;
        }
    }

    public void AddTransition(Func<bool> condition, AgentState newState) {
        transitions.Add(new Transition(condition, newState));
    }

    private class Transition {
        private readonly Func<bool> condition;
        public readonly AgentState newState;

        public Transition(Func<bool> condition, AgentState newState) {
            this.condition = condition;
            this.newState = newState;
        }

        public bool EvaluateCondition() => condition.Invoke();
    }
}