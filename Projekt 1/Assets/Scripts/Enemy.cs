using System;
using UnityEngine;

public class Enemy : Entity {

    

    private void OnDestroy() {
        GameManager.Instance.Objects.Remove(this);
    }

    private void Start() {
        GameManager.Instance.Objects.Add(this);
    }
    
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ColliderRadius);
    }
}
