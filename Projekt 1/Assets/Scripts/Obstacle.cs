using UnityEngine;

public class Obstacle : MonoBehaviour {
    
	public float ColliderRadius { get; private set; } = 0.3f;

	private void OnDrawGizmos() {
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(transform.position, ColliderRadius);
	}
}
