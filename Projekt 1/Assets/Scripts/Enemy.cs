using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : Human {

    [SerializeField] private float speed = 1f;
    [SerializeField] private bool angry;
    [SerializeField] private float offset = 2f;

    private Vector3 target;
    private Vector3 wanderOffset;
    private readonly List<Obstacle> potentialObstacles = new();
    private float obstacleX;
    private bool invoked;
    private Entity frontObstacle;
    
    protected override void Update()
    {
        if (invoked == false) AddObjects();
        base.Update();
        if (Time.frameCount % Random.Range(60, 100) == 0)
            wanderOffset = new Vector3(Random.Range(-2.5f, 2.5f), Random.Range(-2.5f, 2.5f), 0);
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
        transform.position += direction.normalized * (speed * Time.deltaTime);
    }

    private Vector3 Flee() {
        Player player = GameManager.Instance.Player;
        
        Obstacle closestObstacle = potentialObstacles[0];
        for (int i = 1; i < potentialObstacles.Count; i++)
        {
            if (Vector3.Distance(Position, potentialObstacles[i].Position) < Vector3.Distance(Position, closestObstacle.Position))
                closestObstacle = potentialObstacles[i];
        }
        return closestObstacle.Position + ((closestObstacle.Position - player.Position).normalized * (closestObstacle.ColliderRadius + 2f));
    }

    private Vector3 Attack ()
    {
        return GameManager.Instance.Player.transform.position;
    }
    
    private void AddObjects()
    {
        foreach (Entity entity in GameManager.Instance.Objects)
        {
            if (entity is not Obstacle obstacle)
                continue;

            potentialObstacles.Add(obstacle);
        }
        frontObstacle = potentialObstacles[0];
        invoked = true;
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
        Vector3 frontObstacleLocal = new(100, 100, 100);

        for (int i = 1; i < potentialObstacles.Count; i++)
        {
            Vector3 obstacleLocal = transform.InverseTransformPoint(potentialObstacles[i].Position);
         
            if (obstacleLocal.y < 5 && obstacleLocal.y > 0 && Mathf.Abs(obstacleLocal.x) < potentialObstacles[i].ColliderRadius + ColliderRadius+ offset && frontObstacleLocal.y > obstacleLocal.y)
            {
                frontObstacle = potentialObstacles[i];
                frontObstacleLocal = transform.InverseTransformPoint(frontObstacle.Position);
            }
            if (frontObstacle)
            {
                Debug.DrawLine(transform.position, frontObstacle.Position, Color.blue);
                obstacleX = Mathf.Sign(frontObstacleLocal.x) * (Mathf.Abs( frontObstacleLocal.x)-( frontObstacle.ColliderRadius+ ColliderRadius+ offset)) ;
            }
            if ((frontObstacleLocal.y > 5 || frontObstacleLocal.y < 0 || Mathf.Abs(frontObstacleLocal.x) > frontObstacle.ColliderRadius+ ColliderRadius+ offset) && frontObstacle)
            {
                frontObstacle = null;
                obstacleX = 0;
            }
        }
    }
    
    private void OnDrawGizmosSelected() {
        if (!Application.isPlaying)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ColliderRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Flee(), .5f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Flee() + wanderOffset, .5f);
    }
}