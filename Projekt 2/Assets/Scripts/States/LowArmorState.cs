using UnityEngine;

public class LowArmorState : AgentState {
	public LowArmorState(Agent agent) : base(agent) { }

	public override void Update() {
		if (Agent.path.Count == 0) {
			OnEnter();
			return;
		}

		Agent.rotationTarget = Agent.path[0].Position / 1000f;
	}

	public override void OnEnter() {
		Agent.path.Clear();

		if (ArmorKit.All.Count == 0)
			return;
		
		ArmorKit closestArmorKit = ArmorKit.All[0];
		float closestDistance = Vector3.Distance(Agent.transform.position, closestArmorKit.transform.position); 
		
		foreach (ArmorKit armorKit in ArmorKit.All) {
			float distance = Vector3.Distance(Agent.transform.position, armorKit.transform.position);
			if (distance >= closestDistance)
				continue;

			closestArmorKit = armorKit;
			closestDistance = distance;
		}
		
		NavGraph.NavGraphNode startingNode = NavGraph.Instance.GetClosestNode(Agent.transform.position);
		NavGraph.NavGraphNode endNode = NavGraph.Instance.GetClosestNode(closestArmorKit.transform.position);
		Agent.path = AStar.Find(startingNode, endNode);
	}

	public override void OnExit() { }
}
