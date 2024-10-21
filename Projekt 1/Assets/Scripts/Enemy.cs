using System;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public float HP {
        get => hp;
        set {
            hp = value;
            if (hp <= 0)
                Destroy(gameObject);
        }
    }

    [field: SerializeField] public float ColliderRadius { get; private set; } = 0.3f;
    
    [SerializeField] private float hp = 20f;

    private void OnDestroy() {
        GameManager.Instance.Objects.Remove(transform);
    }

    private void Start() {
        GameManager.Instance.Objects.Add(transform);
    }
    
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ColliderRadius);
    }
}
