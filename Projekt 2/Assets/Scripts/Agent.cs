using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Agent : MonoBehaviour {

    public static List<Agent> All = new();
    
    [field: SerializeField] public float Hp { get; set; } = 100f;
    [field: SerializeField] public float Armor { get; set; } = 100f;
    [field: SerializeField] public int Ammo { get; set; } = 20;
    
    public GameObject model;

    [SerializeField] private float speed = 1f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float visionAngle = 45f;
    [SerializeField] private float gunDamage = 15f;
    [SerializeField] private List<Agent> agentsInVision = new();
    
    public List<INode> path = new();
    public Vector3 rotationTarget;

    private StateMachine stateMachine;
    
    private bool SeeSomeone => agentsInVision.Count > 0;
    private bool IsDead => Hp == 0;

    private void Awake() {
        All.Add(this);
    }

    private void OnEnable() {
        NavGraph.OnNavGraphBuild += NavGraph_OnNavGraphBuild;
    }

    private void OnDisable() {
        NavGraph.OnNavGraphBuild -= NavGraph_OnNavGraphBuild;
    }

    private void Start() {

        WanderState wanderState = new (this);
        AttackingState attackingState = new(this);
        LowHpState lowHpState = new(this);
        LowArmorState lowArmorState = new(this);
        LowAmmoState lowAmmoState = new(this);
        DeadState deadState = new(this);
        
        stateMachine = new StateMachine();
        // the highest priority transition
        stateMachine.AddTransition(() => Hp <= 0, deadState);
        stateMachine.AddTransition(() => 
            !IsDead &&
            (Hp >= 100 || !MedKit.IsAnyOnMap) &&
            (Armor >= 100 || !ArmorKit.IsAnyOnMap) &&
            (Ammo >= 20 || !AmmoKit.IsAnyOnMap) && 
            !SeeSomeone, wanderState);
        
        stateMachine.AddTransition(() => !IsDead && Hp < 100 && MedKit.IsAnyOnMap, lowHpState);
        stateMachine.AddTransition(() => !IsDead && Ammo < 20 && AmmoKit.IsAnyOnMap, lowAmmoState);
        stateMachine.AddTransition(() => !IsDead && Armor < 100 && ArmorKit.IsAnyOnMap, lowArmorState);
        
        // the lowest priority transition
        stateMachine.AddTransition(() => !IsDead && Hp >= 100 && Armor >= 100 && Ammo >= 20 && SeeSomeone, attackingState);
        
        stateMachine.currentState = wanderState;
    }

    private void Update() {
        stateMachine.Update();
        Move();
        Rotate();
        CheckVision();
    }

    private void NavGraph_OnNavGraphBuild() {
        stateMachine.Start();
    }

    private void Move() {
        if (path.Count == 0)
            return;
        
        transform.position = Vector3.MoveTowards(transform.position, path[0].Position / 1000f, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, path[0].Position / 1000f) <= 0.01f)
            path.RemoveAt(0);
    }

    private void Rotate() {
        // Calculate the direction to the target in the 2D plane (XY plane)
        Vector3 directionToTarget = rotationTarget - transform.position;

        // Ignore Z-axis by projecting onto the XY plane
        directionToTarget.z = 0;

        // Check if the direction vector is valid (not zero)
        if (directionToTarget == Vector3.zero)
            return;

        // Calculate the target angle in degrees on the Z-axis
        float targetAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f;

        // Smoothly rotate towards the target angle
        float currentZAngle = transform.eulerAngles.z; // Get the current Z angle
        float newZAngle = Mathf.MoveTowardsAngle(currentZAngle, targetAngle, rotationSpeed * Time.deltaTime);

        // Apply the rotation, keeping only the Z axis rotated
        transform.rotation = Quaternion.Euler(0, 0, newZAngle);
    }

    private void CheckVision() {
        agentsInVision.Clear();
        
        // TODO
        // add every agent that can be shot to agentsInVision list
    }

    private void OnDrawGizmos() {
        for (int i = 0; i < path.Count - 1; i++)
            Debug.DrawLine(path[i].Position / 1000f, path[i + 1].Position / 1000f, Color.red);
        
        // Set the Gizmo color
        Gizmos.color = Color.yellow;

        // Get the forward direction (2D equivalent)
        Vector2 forward = transform.up;

        // Calculate the two boundary directions of the cone
        Quaternion leftRotation = Quaternion.AngleAxis(-visionAngle, -Vector3.forward);
        Quaternion rightRotation = Quaternion.AngleAxis(visionAngle, -Vector3.forward);

        Vector3 leftBoundary = leftRotation * forward * 20f;
        Vector3 rightBoundary = rightRotation * forward * 20f;

        // Draw the two boundary lines of the cone
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        
        // current state text next to agent
        Gizmos.color = Color.white;
        if (stateMachine is { currentState: not null })
            Handles.Label(transform.position, stateMachine.currentState.ToString());
    }
}
