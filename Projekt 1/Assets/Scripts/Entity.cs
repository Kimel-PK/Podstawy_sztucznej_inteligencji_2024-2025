using UnityEngine;

public abstract class Entity : MonoBehaviour {
    
	public float HP {
		get => hp;
		set {
			hp = value;
			if (hp <= 0)
				Destroy(gameObject);
		}
	}
	
	public Vector3 Position => transform.position;
	
	[field: SerializeField] public float ColliderRadius { get; set; } = 0.5f;
    
	[SerializeField] private float hp = 20f;

	protected virtual void Start() {
		GameManager.Instance.Objects.Add(this);
	}

	protected virtual void OnDestroy() {
		GameManager.Instance.Objects.Remove(this);
	}
}