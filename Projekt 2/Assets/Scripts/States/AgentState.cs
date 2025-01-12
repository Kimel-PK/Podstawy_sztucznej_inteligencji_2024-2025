using UnityEngine;

public abstract class AgentState {
    protected readonly Agent Agent;

    public AgentState(Agent agent) {
        Agent = agent;
    }

    public abstract void Update();

    public abstract void OnEnter();

    public abstract void OnExit();
}
