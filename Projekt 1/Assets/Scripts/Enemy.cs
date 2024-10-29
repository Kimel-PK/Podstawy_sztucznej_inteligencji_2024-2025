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

    private Vector3 target;
    private Vector3 wanderOffset;
    
    private float obstacleX;
    private int nearEnemiesCount;
    private Obstacle frontObstacle;
    private float courageTimeout;
    private bool courage;

    protected override void Update()
    {
        base.Update();
        if (Time.frameCount % Random.Range(60, 100) == 0)
            wanderOffset = new Vector3(Random.Range(-2.5f, 2.5f), Random.Range(-2.5f, 2.5f), 0);

        if (courageTimeout > 0f)
            courageTimeout -= Time.deltaTime;
        if (Time.frameCount % Random.Range(20, 200) == 0)
            courageTimeout = Random.Range(4f, 8f);
        
        if (Vector3.Distance(GameManager.Instance.Player.Position, Position) < playerProximityAnger)
            angry = true;
        
        target = angry ? Attack() : Flee() + wanderOffset;
        Move();
        Rotate();
    }

    private void Move()
    {
        Vector3 direction = target - transform.position;
        Avoidance();
        direction = transform.InverseTransformDirection(direction) + new Vector3(obstacleX, 0, 0);
        direction = transform.TransformDirection(direction);
        transform.position += direction.normalized * ((angry ? speed * 1.3f : speed) * Time.deltaTime);
    }

    private Vector3 Flee() {
        Player player = GameManager.Instance.Player;

        Obstacle closestObstacle = GameManager.Instance.Obstacles[0];
        for (int i = 1; i < GameManager.Instance.Obstacles.Count; i++)
        {
            if (Vector3.Distance(Position, GameManager.Instance.Obstacles[i].Position) < Vector3.Distance(Position, closestObstacle.Position))
                closestObstacle = GameManager.Instance.Obstacles[i];
        }
        return closestObstacle.Position + (closestObstacle.Position - player.Position).normalized * (closestObstacle.ColliderRadius + 2f);
    }

    private Vector3 Attack ()
    {
        return GameManager.Instance.Player.transform.position;
    }

    private void Rotate()
    {
        Vector2 lookDir = new Vector2 (target.x, target.y) - new Vector2(transform.position.x, transform.position.y);
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90;
        Quaternion newRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.Lerp(transform.rotation,newRotation,(Time.deltaTime*5)%1);
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

    private void OnDrawGizmosSelected() {
        if (!Application.isPlaying)
            return;
        Gizmos.color = angry ? Color.red : Color.blue;
        Gizmos.DrawWireSphere(transform.position, ColliderRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Flee(), .5f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Flee() + wanderOffset, .5f);
    }
}