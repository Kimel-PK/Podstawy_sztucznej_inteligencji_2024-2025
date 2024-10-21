using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {

	public float ColliderRadius { get; private set; } = 0.5f;
	
	[SerializeField] private float speed = 3f;
    
	[SerializeField] private Transform laser;
	[SerializeField] private float gunDamage = 5f;
	
	[SerializeField] private InputActionReference moveAction;
	[SerializeField] private InputActionReference shotAction;
	[SerializeField] private InputActionReference mousePositionAction;
	
	[SerializeField] private List<Transform> hitCandidates = new();
	[SerializeField] private List<Transform> collideCandidates = new();

	private Vector2 moveInput;
	private Vector2 mousePosition;
	private Vector2 lookDir;
	
	private Vector3 hitPos;
	private Transform hitTarget;

	private void OnEnable() {
		moveAction.action.performed += Move;
		moveAction.action.canceled += Move;
		mousePositionAction.action.performed += MousePosition;
		shotAction.action.performed += Shot;
	}

	private void OnDisable() {
		moveAction.action.performed -= Move;
		moveAction.action.canceled -= Move;
		mousePositionAction.action.performed -= MousePosition;
		shotAction.action.performed -= Shot;
	}

	private void Start() {
		GameManager.Instance.Objects.Add(transform);
	}

	private void Update() {
		// move
		transform.position += new Vector3(moveInput.x, moveInput.y) * (speed * Time.deltaTime);
		
		CollideCheck(GameManager.Instance.Objects);
		RayCastCheck(GameManager.Instance.Objects);

		Rotate();
	}

	private void FixedUpdate() {
		AnimateLaser();
	}

	private void OnDestroy() {
		GameManager.Instance.Objects.Remove(transform);
	}

	private void Move(InputAction.CallbackContext context) {
		if (context.performed)
			moveInput = moveAction.action.ReadValue<Vector2>();
		if (context.canceled)
			moveInput = Vector2.zero;
	}

	private void Rotate() {
		Vector2 convertedMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
		lookDir = convertedMousePosition - new Vector2(transform.position.x, transform.position.y);
		float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90;
		transform.rotation = Quaternion.Euler(0, 0, angle);
	}

	private void MousePosition(InputAction.CallbackContext context) {
		if (!context.performed)
			return;

		mousePosition = mousePositionAction.action.ReadValue<Vector2>();
	}

	private void Shot(InputAction.CallbackContext context) {
		if (!context.performed)
			return;
		
		Debug.Log("Player shot");
		Debug.DrawLine(transform.position, hitPos, Color.red, .1f);
		laser.localScale = new Vector3(4f, Vector3.Distance(transform.position, hitPos) - 0.6f, 1f);
		
		if (!hitTarget)
			return;

		Enemy enemy = hitTarget.GetComponent<Enemy>();
		if (enemy)
			enemy.HP -= gunDamage;
	}
	
	private void AnimateLaser() {
		laser.localScale = Vector3.Lerp(laser.localScale, Vector3.zero, .1f);
	}

	private void CollideCheck(List<Transform> elements) {
		// discard self
		elements.Remove(transform);
		
		int i = Time.frameCount % elements.Count;

		float distance = (transform.position - elements[i].position).sqrMagnitude;
		bool meetsCondition = distance < 10;

		if (meetsCondition) {
			if (!collideCandidates.Contains(elements[i])) {
				collideCandidates.Add(elements[i]);
				Debug.DrawRay(transform.position, elements[i].position - transform.position, Color.blue);
			}
		} else
			collideCandidates.Remove(elements[i]);

		foreach (Transform element in collideCandidates) {
			// NOTE is elementRadius necessary? 
			float elementRadius = element.localScale.x / 2;
			float collisionSize = elementRadius + ColliderRadius;
			distance = (transform.position - element.position).sqrMagnitude;

			if (distance < collisionSize)
				transform.position += (transform.position - element.position) * (1 - (distance / collisionSize));
		}
	}

	private void RayCastCheck(List<Transform> elements) {
		Vector3 hitTargetLocal = new(-1, -1, -1);
		float hitTargetRadius = Mathf.Infinity;

		int i = Time.frameCount % elements.Count;
		Vector3 elementLocal = transform.InverseTransformPoint(elements[i].position);

		bool meetsCondition = Mathf.Abs(elementLocal.x) < 10 && elementLocal.y > 0;

		if (meetsCondition) {
			if (!hitCandidates.Contains(elements[i])) {
				hitCandidates.Add(elements[i]);
				Debug.DrawRay(transform.position, elements[i].position - transform.position, Color.yellow);
			}
		} else
			hitCandidates.Remove(elements[i]);

		if (hitTarget) {
			// NOTE is this necessary if we have ColliderRadius?
			hitTargetRadius = hitTarget.localScale.x / 2;
			hitTargetLocal = transform.InverseTransformPoint(hitTarget.position);
		}

		if (hitTarget && (Mathf.Abs(hitTargetLocal.x) > hitTargetRadius || hitTargetLocal.y < 0)) {
			hitTarget = null;
			hitPos = transform.position + transform.up * 100f;
		}
		
		for (int j = hitCandidates.Count - 1; j >= 0; j--) {
			if (!hitCandidates[j]) {
				hitCandidates.RemoveAt(j);
				continue;
			}

			// NOTE is this necessary if we have ColliderRadius?
			float elementRadius = hitCandidates[j].localScale.x / 2;
			elementLocal = transform.InverseTransformPoint(hitCandidates[j].position);
			
			if (Mathf.Abs(elementLocal.x) < elementRadius && elementLocal.y > 0) {
				if (hitTarget) {
					// NOTE is this necessary if we have ColliderRadius?
					hitTargetRadius = hitTarget.localScale.x / 2;
					hitTargetLocal = transform.InverseTransformPoint(hitTarget.position);

					hitPos = transform.position + transform.TransformDirection(0,
						hitTargetLocal.y - Mathf.Sqrt(hitTargetRadius * hitTargetRadius -
						                              hitTargetLocal.x * hitTargetLocal.x), 0);
					Debug.DrawRay(transform.position, hitPos - transform.position,
						new Color(0.1f, 0, 0) * hitTargetLocal.y);

					if (elementLocal.y < hitTargetLocal.y || hitTargetLocal.y < 0 || Mathf.Abs(hitTargetLocal.x) > hitTargetRadius)
						hitTarget = hitCandidates[j];
				} else
					hitTarget = hitCandidates[j];
			}
		}
	}
	
	private void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, ColliderRadius);
	}
}