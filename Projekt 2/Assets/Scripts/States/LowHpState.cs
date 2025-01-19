using UnityEngine;

public class LowHpState : AgentState {
	
	public LowHpState(Agent agent) : base(agent) { }

	public override void Update() {
		if (Agent.path.Count == 0) {
			OnEnter();
			return;
		}

		Agent closestEnemy = Agent.GetClosestEnemy();
		if (closestEnemy)
			Agent.rotationTarget = closestEnemy.transform.position;
		else
			Agent.rotationTarget = Agent.path[0].Position / 1000f;
	}

	public override void OnEnter() {
		Agent.path.Clear();

		if (MedKit.All.Count == 0)
			return;
		
		MedKit closestMedKit = MedKit.All[0];
		float closestDistance = Vector3.Distance(Agent.transform.position, closestMedKit.transform.position); 
		
		foreach (MedKit medKit in MedKit.All) {
			float distance = Vector3.Distance(Agent.transform.position, medKit.transform.position);
			if (distance >= closestDistance)
				continue;

			closestMedKit = medKit;
			closestDistance = distance;
		}
		
		NavGraph.NavGraphNode startingNode = NavGraph.Instance.GetClosestNode(Agent.transform.position);
		NavGraph.NavGraphNode endNode = NavGraph.Instance.GetClosestNode(closestMedKit.transform.position);
		Agent.path = AStar.Find(startingNode, endNode);
	}

	public override void OnExit() { }
}
