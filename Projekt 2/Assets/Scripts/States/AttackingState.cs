using UnityEngine;

public class AttackingState : AgentState {

	private float shotDelay = 2f;
	private float timer;
    
	public AttackingState(Agent agent) : base(agent) { }

	public override void Update() {
		// TODO Agent.rotationTarget = attacked enemy

		Shot();
		
		if (Agent.path.Count > 0)
			return;
		
		// TODO strafe
		/*
		NavGraph.NavGraphNode startingNode = NavGraph.Instance.GetClosestNode(Agent.transform.position);
		NavGraph.NavGraphNode endNode = NavGraph.Instance.GetClosestNode(???);
		Agent.path = AStar.Find(startingNode, endNode);
		*/
	}

	private void Shot() {
		if (timer >= 0f) {
			timer -= Time.deltaTime;
			return;
		}
		
		// TODO if is not rotated towards attacked enemy return
		
		// TODO shot enemy
		// enemy.Hp -= Agent.gunDamage;

		timer = shotDelay;
	}

	public override void OnEnter() {
		timer = shotDelay;
	}

	public override void OnExit() { }
}
