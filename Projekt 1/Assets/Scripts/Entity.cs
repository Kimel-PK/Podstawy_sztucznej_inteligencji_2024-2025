using UnityEngine;
using UnityEngine.UI;

public abstract class Entity : MonoBehaviour {
    
	public float HP {
		get => hp;
		set {
			hp = value;
			if (hpBar)
				hpBar.value = hp / maxHP;
			if (hp <= 0)
				Destroy(gameObject);
		}
	}
	
	public Vector3 Position => transform.position;
	
	[field: SerializeField] public float ColliderRadius { get; set; } = 0.5f;

	[SerializeField] protected Transform entitySprite;
    
	[SerializeField] private Slider hpBar;
	[SerializeField] private float hp = 20f;

	private float maxHP;

	protected virtual void Start() {
		maxHP = hp;
		if (hpBar)
			hpBar.value = 1f;
		GameManager.Instance.Objects.Add(this);
	}

	protected virtual void OnDestroy() {
		GameManager.Instance.Objects.Remove(this);
	}
}