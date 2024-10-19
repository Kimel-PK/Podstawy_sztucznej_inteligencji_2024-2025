using UnityEngine;

public class Obstacle : MonoBehaviour {
    
	public float ColliderRadius = 0.5f;
	void Start()
	{
        transform.localScale = Vector3.one * ColliderRadius * 2;
    }

        private void OnDrawGizmos() {
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(transform.position, ColliderRadius);
		
	}
}
