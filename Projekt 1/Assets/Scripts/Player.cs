using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Human {
	
	[SerializeField] private float speed = 3f;
    
	[SerializeField] private Transform laser;
	[SerializeField] private float gunDamage = 5f;
	
	[SerializeField] private InputActionReference moveAction;
	[SerializeField] private InputActionReference shotAction;
	[SerializeField] private InputActionReference mousePositionAction;
	
	[SerializeField] private List<Entity> hitCandidates = new();
	

	private Vector2 moveInput;
	private Vector2 mousePosition;
	private Vector2 lookDir;
	
	private Vector3 hitPos;
	private Entity hitTarget;

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

	protected override void Start() {
		base.Start();
		GameManager.Instance.Player = this;
	}

	protected override void Update() {
		// move
		transform.position += new Vector3(moveInput.x, moveInput.y) * (speed * Time.deltaTime);
		
		base.Update();
		RayCastCheck(GameManager.Instance.Objects);

		Rotate();
	}

	private void FixedUpdate() {
		AnimateLaser();
	}

	protected override void OnDestroy() {
		base.OnDestroy();
		GameManager.Instance.Player = null;
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

	

	private void RayCastCheck(List<Entity> entities) {
		Vector3 hitTargetLocal = new(-1, -1, -1);

		int i = Time.frameCount % entities.Count;
		Vector3 elementLocal = transform.InverseTransformPoint(entities[i].Position);

		bool meetsCondition = Mathf.Abs(elementLocal.x) < 10 && elementLocal.y > 0;

		if (meetsCondition) {
			if (!hitCandidates.Contains(entities[i])) {
				hitCandidates.Add(entities[i]);
				Debug.DrawRay(transform.position, entities[i].Position - transform.position, Color.yellow);
			}
		} else
			hitCandidates.Remove(entities[i]);

		if (hitTarget)
			hitTargetLocal = transform.InverseTransformPoint(hitTarget.Position);

		if (hitTarget && (Mathf.Abs(hitTargetLocal.x) > entities[i].ColliderRadius || hitTargetLocal.y < 0)) {
			hitTarget = null;
			hitPos = transform.position + transform.up * 100f;
		}
		
		for (int j = hitCandidates.Count - 1; j >= 0; j--) {
			if (!hitCandidates[j]) {
				hitCandidates.RemoveAt(j);
				continue;
			}
			
			elementLocal = transform.InverseTransformPoint(hitCandidates[j].Position);
			
			if (Mathf.Abs(elementLocal.x) < hitCandidates[j].ColliderRadius && elementLocal.y > 0) {
				if (hitTarget) {
					hitTargetLocal = transform.InverseTransformPoint(hitTarget.Position);

					hitPos = transform.position + transform.TransformDirection(0,
						hitTargetLocal.y - Mathf.Sqrt(hitTarget.ColliderRadius * hitTarget.ColliderRadius -
						                              hitTargetLocal.x * hitTargetLocal.x), 0);
					Debug.DrawRay(transform.position, hitPos - transform.position,
						new Color(0.1f, 0, 0) * hitTargetLocal.y);

					if (elementLocal.y < hitTargetLocal.y || hitTargetLocal.y < 0 || Mathf.Abs(hitTargetLocal.x) > hitTarget.ColliderRadius)
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