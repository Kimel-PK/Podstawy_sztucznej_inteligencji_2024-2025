using UnityEngine;

public class AttackingState : AgentState {
	
	private float timer;
    
	public AttackingState(Agent agent) : base(agent) { }

	public override void Update() {

		Agent closestEnemy = Agent.GetClosestEnemy();

		if (closestEnemy) {
			Agent.rotationTarget = closestEnemy.transform.position;
			Shot(closestEnemy);
		}

		if (Agent.path.Count > 0)
			return;
		
		NavGraph.NavGraphNode startingNode = NavGraph.Instance.GetClosestNode(Agent.transform.position);
		NavGraph.NavGraphNode endNode = NavGraph.Instance.GetClosestNode(Agent.transform.position + (Vector3)Random.insideUnitCircle * 1f);
		Agent.path = AStar.Find(startingNode, endNode);
	}

	private void Shot(Agent enemy) {
		if (timer >= 0f) {
			timer -= Time.deltaTime;
			return;
		}
		
		if (Agent.Ammo <= 0)
			return;
		
		if (!LineIntersectionChecker.IsVectorRotatedTowards(Agent.model.transform.up, enemy.transform.position - Agent.transform.position, 3f))
			return;

		if (Random.Range(0f, 1f) < Agent.AccuracyPercent) {
			if (enemy.Armor > 0)
				enemy.Armor -= Agent.Damage;
			else
				enemy.Hp -= Agent.Damage + Random.Range(-Agent.Damage * .2f, Agent.Damage * .2f);
			
		}

		Agent.Ammo--;
		Agent.FireAnimation();
		
		if (enemy.Hp <= 0)
			Agent.Kills++;

		timer = Agent.ShotDelay;
	}

	public override void OnEnter() {
		Agent.path.Clear();
		timer = Agent.ShotDelay;
	}

	public override void OnExit() { }
}
