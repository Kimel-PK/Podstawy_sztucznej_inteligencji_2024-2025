using UnityEngine;

public class Obstacle : Entity {

	protected override void Start() {
		base.Start();
		transform.localScale = Vector3.one * ColliderRadius * 2;
		GameManager.Instance.Obstacles.Add(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GameManager.Instance.Obstacles.Remove(this);
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(transform.position, ColliderRadius);
	}
}