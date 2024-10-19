using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Rendering.FilterWindow;

public class Player : MonoBehaviour {

    public List<GameObject> Elements = new List<GameObject>();
    public float ColliderRadius { get; private set; } = 0.5f;
    public GameObject laser;
    public GameObject ObjectSpawner;
    public Vector3 HitPos;
    



    [SerializeField] private float speed = 3f;
	[SerializeField] private InputActionReference moveAction;
	[SerializeField] private InputActionReference shotAction;
	[SerializeField] private InputActionReference mousePositionAction;
    [SerializeField] private List<GameObject> hitcandidates = new List<GameObject>();
    [SerializeField] private List<GameObject> colidecandidates = new List<GameObject>();


    private Vector2 moveInput;
	private Vector2 mousePosition;
    private Vector2 lookDir;
    [SerializeField] private GameObject Hittarget;
    private Transform Playerspace;



    void Start()
    {
        foreach (Transform child in ObjectSpawner.transform)
        {
            Elements.Add(child.gameObject);
        }
    }

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
        Playerspace = transform;

        transform.position += new Vector3(moveInput.x, moveInput.y) * (speed * Time.deltaTime);
        ColideCheck(Elements);
        RayCastCheck(Elements);

        Vector2 convertedMousePosition;
        convertedMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        lookDir = convertedMousePosition - new Vector2(transform.position.x, transform.position.y);
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90;
        transform.rotation = Quaternion.Euler(0, 0, angle);

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
        Vector2 convertedMousePosition;
        convertedMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        //laser.SetActive(true);

        if (Hittarget != null)
        {
            Debug.Log("Player shot");
            Debug.DrawLine(transform.position, HitPos, Color.red, .1f);
            laser.transform.localScale = new Vector3(1, Playerspace.InverseTransformPoint(HitPos).y-0.5f, 1);
        }
        else laser.transform.localScale = new Vector3(1, 0, 1);

    }

	private void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, ColliderRadius);
	}

    private void ColideCheck(List<GameObject> elements)
    {
        float distance;
        float ColisionSize;
        float elementradious;
        int i;
        i = Time.frameCount % elements.Count;

        distance = (transform.position - elements[i].transform.position).sqrMagnitude;

        bool meetsCondition = distance < 10;

        if (meetsCondition)
        {
            if (!colidecandidates.Contains(elements[i]))
            {
                colidecandidates.Add(elements[i]);
                Debug.DrawRay(transform.position, elements[i].transform.position - transform.position, Color.blue);
            }
        }
        else
        {
            colidecandidates.Remove(elements[i]);
        }

        foreach (GameObject element in colidecandidates)
        {

            elementradious = element.transform.localScale.x / 2;
            ColisionSize = elementradious + ColliderRadius;
            distance = (transform.position - element.transform.position).sqrMagnitude;


            if (distance < ColisionSize)
            {
                transform.position += (transform.position - element.transform.position) * (1 - (distance / ColisionSize));
            }
        }
    }

    private void RayCastCheck(List<GameObject> elements)
    {
        Vector3 elementLocal;
        Vector3 HittargetLocal = new Vector3(-1, -1, -1);
        float Hittargetradious = Mathf.Infinity;

        int i;
        i = Time.frameCount % elements.Count;
        float elementradious;
        elementLocal = Playerspace.InverseTransformPoint(elements[i].transform.position);

        bool meetsCondition = Mathf.Abs(elementLocal.x) < 10 && elementLocal.y > 0;

        if (meetsCondition)
        {
            if (!hitcandidates.Contains(elements[i]))
            {
                hitcandidates.Add(elements[i]);
                Debug.DrawRay(transform.position, elements[i].transform.position - transform.position, Color.yellow);
            }
        }
        else
        {
            hitcandidates.Remove(elements[i]);
        }
        if (Hittarget != null)
        {
            Hittargetradious = Hittarget.transform.localScale.x / 2;
            HittargetLocal = Playerspace.InverseTransformPoint(Hittarget.transform.position);
        }
            if (Hittarget != null && (Mathf.Abs(HittargetLocal.x) > Hittargetradious || HittargetLocal.y < 0))
        {
            Hittarget = null;
            HitPos = Vector3.zero;
        }

        foreach (GameObject element in hitcandidates)
        {

            

            elementradious = element.transform.localScale.x / 2;
            elementLocal = Playerspace.InverseTransformPoint(element.transform.position);

            Debug.Log(HittargetLocal);
            if (Mathf.Abs(elementLocal.x) < elementradious && elementLocal.y > 0)
            {
                if (Hittarget != null)
                {
                    Hittargetradious = Hittarget.transform.localScale.x / 2;
                    HittargetLocal = Playerspace.InverseTransformPoint(Hittarget.transform.position);

                    HitPos = transform.position + Playerspace.TransformDirection(0, HittargetLocal.y - Mathf.Sqrt((Hittargetradious * Hittargetradious) - (HittargetLocal.x * HittargetLocal.x)), 0);
                    Debug.DrawRay(transform.position, HitPos - transform.position, new Color(0.1f, 0, 0) * HittargetLocal.y);

                    if ((elementLocal.y < HittargetLocal.y || HittargetLocal.y < 0) || Mathf.Abs(HittargetLocal.x) > Hittargetradious)
                        Hittarget = element;
                }
                else Hittarget = element;
                
            }


        }

    }

}