using UnityEngine;

public abstract class Entity : MonoBehaviour {
    
	[field: SerializeField] public float ColliderRadius { get; set; } = 0.5f;
	public Vector3 Position => transform.position;
	
	public float HP {
		get => hp;
		set {
			hp = value;
			if (hp <= 0)
				Destroy(gameObject);
		}
	}
    
	[SerializeField] private float hp = 20f;
}
