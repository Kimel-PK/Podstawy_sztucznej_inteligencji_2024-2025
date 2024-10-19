using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {

	public float ColliderRadius { get; private set; } = 0.3f;

	[SerializeField] private float speed = 3f;
	
	[SerializeField] private InputActionReference moveAction;
	[SerializeField] private InputActionReference shotAction;
	[SerializeField] private InputActionReference mousePositionAction;

	private Vector2 moveInput;
	private Vector2 mousePosition;
	
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

	private void Update() {
		transform.position += new Vector3(moveInput.x, moveInput.y) * (speed * Time.deltaTime);
	}

	private void Move(InputAction.CallbackContext context) {
		if (context.performed)
			moveInput = moveAction.action.ReadValue<Vector2>();
		if (context.canceled)
			moveInput = Vector2.zero;
	}
	
	private void MousePosition(InputAction.CallbackContext context) {
		if (!context.performed)
			return;

		mousePosition = mousePositionAction.action.ReadValue<Vector2>();
	}

	private void Shot(InputAction.CallbackContext context) {
		if (!context.performed)
			return;

		Vector2 convertedMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
		
		Debug.Log("Player shot");
		Debug.DrawLine(transform.position, new Vector3(convertedMousePosition.x, convertedMousePosition.y, 0f), Color.red, .1f);
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, ColliderRadius);
	}
}