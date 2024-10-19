using UnityEngine;

public class Enemy : MonoBehaviour {
    
    public float ColliderRadius { get; private set; } = 0.3f;
    
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ColliderRadius);
    }
}
