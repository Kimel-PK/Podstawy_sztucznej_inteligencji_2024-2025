using UnityEngine;

public class Obstacle : Entity {

	protected override void Start() {
		base.Start();
		transform.localScale = Vector3.one * ColliderRadius * 2;
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(transform.position, ColliderRadius);
	}
}