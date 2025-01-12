using UnityEngine;

public class LowAmmoState : AgentState {
	
	public LowAmmoState(Agent agent) : base(agent) { }

	public override void Update() {
		if (Agent.path.Count == 0) {
			OnEnter();
			return;
		}

		Agent.rotationTarget = Agent.path[0].Position / 1000f;
	}

	public override void OnEnter() {
		Agent.path.Clear();

		if (AmmoKit.All.Count == 0)
			return;
		
		AmmoKit closestAmmoKit = AmmoKit.All[0];
		float closestDistance = Vector3.Distance(Agent.transform.position, closestAmmoKit.transform.position); 
		
		foreach (AmmoKit ammoKit in AmmoKit.All) {
			float distance = Vector3.Distance(Agent.transform.position, ammoKit.transform.position);
			if (distance >= closestDistance)
				continue;

			closestAmmoKit = ammoKit;
			closestDistance = distance;
		}
		
		NavGraph.NavGraphNode startingNode = NavGraph.Instance.GetClosestNode(Agent.transform.position);
		NavGraph.NavGraphNode endNode = NavGraph.Instance.GetClosestNode(closestAmmoKit.transform.position);
		Agent.path = AStar.Find(startingNode, endNode);
	}

	public override void OnExit() { }
}
