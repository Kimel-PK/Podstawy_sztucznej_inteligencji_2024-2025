using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : Human {

    [SerializeField] private float speed = 1f;
    [SerializeField] private bool angry;
    [SerializeField] private float offset = 2f;
    [SerializeField] private int scatterThreshold = 1;
    [SerializeField] private int formGroupThreshold = 4;
    [SerializeField] private float playerProximityAnger = 5f;
    
    [SerializeField] private float minCourageDelay = 6f;
    [SerializeField] private float maxCourageDelay = 12f;
    [SerializeField] private float minCourageDuration = 4f;
    [SerializeField] private float maxCourageDuration = 10f;
    
    [SerializeField] private float attackDelay = 1f;
    [SerializeField] private float damage = 2f;

    private Vector3 target;
    private Vector3 fleeTarget;
    private Vector3 wanderOffset;
    
    private float obstacleX;
    private int nearEnemiesCount;
    private Obstacle frontObstacle;

    private float courageDelayTimer;
    private float courageTimer;
    private Obstacle courageObstacle;

    private float attackDelayTimer;

    protected override void Update()
    {
        if (!GameManager.Instance.Player)
            return;
        
        base.Update();
        if (Time.frameCount % Random.Range(60, 100) == 0)
            wanderOffset = new Vector3(Random.Range(-2.5f, 2.5f), Random.Range(-2.5f, 2.5f), 0);

        Courage();

        if (Vector3.Distance(GameManager.Instance.Player.Position, Position) < playerProximityAnger)
            angry = true;

        fleeTarget = GetFleeTarget();
        
        target = angry ? GetPlayerPosition() : fleeTarget + wanderOffset;
        Move();
        Rotate();

        DealDamage();
    }

    private void Move()
    {
        Vector3 direction = target - transform.position;
        Avoidance();
        direction = transform.InverseTransformDirection(direction) + new Vector3(obstacleX, 0, 0);
        direction = transform.TransformDirection(direction);
        transform.position += direction.normalized * ((angry ? speed * 1.3f : speed) * Time.deltaTime);
    }

    private Vector3 GetFleeTarget() {
        Player player = GameManager.Instance.Player;

        // if courage mode, just go to picked obstacle no matter what
        if (courageObstacle)
            return courageObstacle.Position + (courageObstacle.Position - player.Position).normalized * (courageObstacle.ColliderRadius + 2f);
        
        // sort obstacles by distance to enemy
        List<Obstacle> obstaclesSorted = GameManager.Instance.Obstacles.OrderBy(obstacle => Vector3.Distance(obstacle.Position, Position)).ToList();
        
        // return best hiding spot
        return obstaclesSorted[0].Position + (obstaclesSorted[0].Position - player.Position).normalized * (obstaclesSorted[0].ColliderRadius + 2f);
    }

    private Vector3 GetPlayerPosition ()
    {
        return GameManager.Instance.Player.Position;
    }

    private void Courage()
    {
        if (courageObstacle)
        {
            if (courageTimer > 0f)
                courageTimer -= Time.deltaTime;
            else
            {
                courageObstacle = null;
                courageDelayTimer = Random.Range(minCourageDelay, maxCourageDelay);
            }
        }
        else
        {
            if (courageDelayTimer > 0f)
                courageDelayTimer -= Time.deltaTime;
            else
            {
                List<Obstacle> obstaclesSorted = GameManager.Instance.Obstacles.OrderBy(obstacle => Vector3.Distance(obstacle.Position, Position)).ToList();
                obstaclesSorted.RemoveAt(0);
                
                courageObstacle = GetRandomElementWithSquareProbability(ref obstaclesSorted);
                
                courageTimer = Random.Range(minCourageDuration, maxCourageDuration);
            }
        }
    }

    private void Rotate()
    {
        Vector2 lookDir = new Vector2(target.x, target.y) - new Vector2(entitySprite.position.x, entitySprite.position.y);
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90;
        Quaternion newRotation = Quaternion.Euler(0, 0, angle);
        entitySprite.rotation = Quaternion.Lerp(entitySprite.rotation, newRotation, (Time.deltaTime * 5) % 1);
    }

    private void Avoidance()
    {
        if (GameManager.Instance.Obstacles.Count == 0)
            return;
        
        Vector3 frontObstacleLocal = new(100, 100, 100);

        for (int i = 1; i < GameManager.Instance.Obstacles.Count; i++)
        {
            Vector3 obstacleLocal = transform.InverseTransformPoint(GameManager.Instance.Obstacles[i].Position);
         
            if (obstacleLocal.y < 5 && obstacleLocal.y > 0 && Mathf.Abs(obstacleLocal.x) < GameManager.Instance.Obstacles[i].ColliderRadius + ColliderRadius+ offset && frontObstacleLocal.y > obstacleLocal.y)
            {
                frontObstacle = GameManager.Instance.Obstacles[i];
                frontObstacleLocal = transform.InverseTransformPoint(frontObstacle.Position);
            }
            if (frontObstacle)
            {
                Debug.DrawLine(transform.position, frontObstacle.Position, Color.blue);
                obstacleX = Mathf.Sign(frontObstacleLocal.x) * (Mathf.Abs( frontObstacleLocal.x)-( frontObstacle.ColliderRadius+ ColliderRadius+ offset)) ;
            }
            if (frontObstacle && (frontObstacleLocal.y > 5 || frontObstacleLocal.y < 0 || Mathf.Abs(frontObstacleLocal.x) > frontObstacle.ColliderRadius+ ColliderRadius+ offset))
            {
                frontObstacle = null;
                obstacleX = 0;
            }
        }
    }

    protected override void CollideCheck(List<Entity> entity)
    {
        colliderCheckDistance = angry ? 7.5f : 5f;
        base.CollideCheck(entity);
        nearEnemiesCount = collideCandidates.Count(e => e is Enemy);
        if (angry && nearEnemiesCount <= scatterThreshold)
            angry = false;
        else if (!angry && nearEnemiesCount >= formGroupThreshold)
            angry = true;
    }

    private void DealDamage()
    {
        if (attackDelayTimer > 0f)
        {
            attackDelayTimer -= Time.deltaTime;
            return;
        }
        
        float distanceFromPlayer = Vector3.Distance(GameManager.Instance.Player.Position, Position) - GameManager.Instance.Player.ColliderRadius - ColliderRadius;
        if (distanceFromPlayer > .1f)
            return;
        
        GameManager.Instance.Player.HP -= damage;
        attackDelayTimer = attackDelay;
    }
    
    private Obstacle GetRandomElementWithSquareProbability(ref List<Obstacle> list)
    {
        // Total number of elements
        int n = list.Count;

        // Create a list to store cumulative probabilities
        float[] cumulativeProbabilities = new float[n];
        float totalProbability = 0;

        // Calculate the probability as (1 - (i / (n-1))^2)
        for (int i = 0; i < n; i++)
        {
            float probability = 1f - Mathf.Pow((float)i / (n - 1), 2);
            totalProbability += probability;
            cumulativeProbabilities[i] = totalProbability;
        }

        // Generate a random number between 0 and totalProbability
        float randomValue = Random.Range(0f, totalProbability);

        // Select the element based on the random value
        for (int i = 0; i < n; i++)
        {
            if (randomValue <= cumulativeProbabilities[i])
                return list[i];
        }

        // In case of rounding errors, return the last element
        return list[^1];
    }

    private void OnDrawGizmosSelected() {
        if (!Application.isPlaying)
            return;
        Gizmos.color = angry ? Color.red : Color.blue;
        Gizmos.DrawWireSphere(transform.position, ColliderRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(fleeTarget, .5f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(fleeTarget + wanderOffset, .5f);
    }
}