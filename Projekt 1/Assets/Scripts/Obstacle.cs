using UnityEngine;

public class Obstacle : Entity {

	protected override void Start()
	{
		base.Start();
		GameManager.Instance.Obstacles.Add(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GameManager.Instance.Obstacles.Remove(this);
	}

	public void SetRadius(float radius)
	{
		ColliderRadius = radius;
		transform.localScale = Vector3.one * radius * 2;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(transform.position, ColliderRadius);
	}
}