using UnityEngine;

public class WanderState : AgentState {

    public WanderState(Agent agent) : base(agent) {}

    public override void Update()
    {
        if (Agent.path.Count > 0) {
            Agent.rotationTarget = Agent.path[0].Position / 1000f;
            return;
        }
        
        NavGraph.NavGraphNode startingNode = NavGraph.Instance.GetClosestNode(Agent.transform.position);
        NavGraph.NavGraphNode endNode = NavGraph.Instance.GetRandomNode();
        Agent.path = AStar.Find(startingNode, endNode);
    }

    public override void OnEnter() { }

    public override void OnExit() { }
}
