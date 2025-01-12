using UnityEngine;

public abstract class Collectible : MonoBehaviour {

	private void Update() {
		foreach (Agent agent in Agent.All) {
			if (Vector3.Distance(agent.transform.position, transform.position) > 0.3f)
				continue;

			OnCollect(agent);
			Destroy(gameObject);
		}
	}

	protected abstract void OnCollect(Agent agent);
}
