using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Agent : MonoBehaviour {

    public Action OnEnemyKilled = delegate { };
    public Action OnDied = delegate { };

    public static List<Agent> All = new();

    [field: SerializeField] public string AgentName { get; set; }

    private int kills;
    public int Kills {
        get => kills;
        set {
            kills = value;
            OnEnemyKilled?.Invoke();
        }
    }

    private int deads;
    public int Deads {
        get => deads;
        set {
            deads = value;
            OnDied?.Invoke();
        }
    }

    [field: SerializeField] public float MaxHp { get; set; } = 100f;
    [field: SerializeField] public float Hp { get; set; } = 100f;
    [field: SerializeField] public float MaxArmor { get; set; } = 50f;
    [field: SerializeField] public float Armor { get; set; } = 50f;
    [field: SerializeField] public int MaxAmmo { get; set; } = 20;
    [field: SerializeField] public int Ammo { get; set; } = 20;
    [field: SerializeField] public float Damage { get; set; } = 10f;
    [field: SerializeField] public float AccuracyPercent { get; set; } = .75f;
    [field: SerializeField] public float ShotDelay { get; set; } = 1f;

    public GameObject model;

    [SerializeField] private float speed = 1f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float visionAngle = 45f;
    public List<Agent> agentsInVision = new();

    [SerializeField] private RectTransform hpBar;
    [SerializeField] private RectTransform armorBar;
    [SerializeField] private RectTransform ammoBar;
    
    public List<INode> path = new();
    public Vector3 rotationTarget;

    private StateMachine stateMachine;

    [SerializeField] private GameObject fireMuzzle;
    private float fireAnimTimer;
    
    private bool SeeSomeone => agentsInVision.Count > 0;
    private bool IsDead => Hp <= 0;

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
        stateMachine.AddTransition(() => IsDead, deadState);
        
        stateMachine.AddTransition(() => !IsDead && ((!SeeSomeone && Hp < MaxHp) || (SeeSomeone && Hp < 30 && Armor <= 0)) && MedKit.IsAnyOnMap, lowHpState);
        stateMachine.AddTransition(() => !IsDead && ((!SeeSomeone && Ammo < MaxAmmo) || (SeeSomeone && Ammo <= 0)) && AmmoKit.IsAnyOnMap, lowAmmoState);
        stateMachine.AddTransition(() => !IsDead && !SeeSomeone && Armor < MaxArmor && ArmorKit.IsAnyOnMap, lowArmorState);
        
        // the lowest priority transition
        stateMachine.AddTransition(() => !IsDead && SeeSomeone, attackingState);
        stateMachine.AddTransition(() => !IsDead, wanderState);
        
        stateMachine.currentState = wanderState;
    }

    private void Update() {
        stateMachine.Update();
        Move();
        Rotate();
        CheckVision();

        hpBar.sizeDelta = new Vector2(Hp / MaxHp * 0.5f, 0f);
        armorBar.sizeDelta = new Vector2(Armor / MaxArmor * 0.5f, 0f);
        ammoBar.sizeDelta = new Vector2(Ammo / (float)MaxAmmo * 0.5f, 0f);

        if (fireAnimTimer > 0f) {
            fireMuzzle.SetActive(true);
            fireAnimTimer -= Time.deltaTime;
            return;
        }
        
        fireMuzzle.SetActive(false);
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
        float currentZAngle = model.transform.eulerAngles.z; // Get the current Z angle
        float newZAngle = Mathf.MoveTowardsAngle(currentZAngle, targetAngle, rotationSpeed * Time.deltaTime);

        // Apply the rotation, keeping only the Z axis rotated
        model.transform.rotation = Quaternion.Euler(0, 0, newZAngle);
    }

    private void CheckVision() {
        agentsInVision.Clear();

        foreach (Agent enemy in All) {
            // cant see self
            if (this == enemy)
                continue;
            
            // cant see dead
            if (enemy.IsDead)
                continue;
            
            // cant see if vision blocked
            if (LineIntersectionChecker.IsLineBlocked(transform.position, enemy.transform.position))
                continue;
            
            // cant see if not rotated
            if (!LineIntersectionChecker.IsVectorRotatedTowards(model.transform.up, enemy.transform.position - transform.position, visionAngle))
                continue;
            
            agentsInVision.Add(enemy);
        }
    }

    public Agent GetClosestEnemy() {
        Agent closestEnemy = null;
        foreach (Agent enemy in agentsInVision) {
            if (!closestEnemy || Vector3.Distance(transform.position, enemy.transform.position) <
                Vector3.Distance(transform.position, closestEnemy.transform.position))
                closestEnemy = enemy;
        }

        return closestEnemy;
    }

    public void FireAnimation() {
        fireAnimTimer = .1f;
    }

    private void OnDrawGizmos() {
        for (int i = 0; i < path.Count - 1; i++)
            Debug.DrawLine(path[i].Position / 1000f, path[i + 1].Position / 1000f, Color.red);
        
        // Set the Gizmo color
        Gizmos.color = Color.yellow;

        // Get the forward direction (2D equivalent)
        Vector2 forward = model.transform.up;

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
