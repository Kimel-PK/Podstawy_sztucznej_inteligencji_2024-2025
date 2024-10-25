using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : Human {

    [SerializeField] private float speed = 1f;
    [SerializeField] private bool angry;
    [SerializeField] private float Offset = 2f;

    private Vector3 target;
    private Vector3 wanderOffset;
    private List<Obstacle> potentialObstacles = new();
    private float ObstacleX;
    private bool invoked=false;
    private Entity FrontOstacle;
    

    private void OnDestroy() {
        GameManager.Instance.Objects.Remove(this);
    }

    private void Start() {
        GameManager.Instance.Objects.Add(this);

    }
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
        Vector3 direction = (target - transform.position);
        Avoidance();
        direction= transform.InverseTransformDirection(direction)+new Vector3( ObstacleX,0,0);
        direction= transform.TransformDirection(direction);
        transform.position += direction.normalized * speed * Time.deltaTime;
    }

    private Vector3 Flee() {
        Player player = GameManager.Instance.Player;
 

        Obstacle closestobstacle = potentialObstacles[0];
        for (int i = 1; i < potentialObstacles.Count; i++)
        {
            if (Vector3.Distance(Position, potentialObstacles[i].Position) < Vector3.Distance(Position, closestobstacle.Position))
                closestobstacle = potentialObstacles[i];
        }
        return closestobstacle.Position + ((closestobstacle.Position - player.Position).normalized * (closestobstacle.ColliderRadius + 2f));
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
        FrontOstacle = potentialObstacles[0];
        invoked = true;
    }

    private void Rotate()
    {
        Quaternion newrotation;
        Vector2 lookDir = new Vector2 (target.x, target.y) - new Vector2(transform.position.x, transform.position.y);
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90;
        newrotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.Lerp(transform.rotation,newrotation,(Time.deltaTime*5)%1);
    }
    private void Avoidance()
    {
        Vector3 FrontOstacleLocal = new Vector3(100, 100, 100);

        for (int i = 1; i < potentialObstacles.Count; i++)
        {
            Vector3 ObstacleLocal = transform.InverseTransformPoint(potentialObstacles[i].Position);
         
                if (ObstacleLocal.y < 5 && ObstacleLocal.y > 0 && Mathf.Abs(ObstacleLocal.x) < potentialObstacles[i].ColliderRadius + ColliderRadius+ Offset && FrontOstacleLocal.y > ObstacleLocal.y)
            {

                FrontOstacle = potentialObstacles[i];
                FrontOstacleLocal = transform.InverseTransformPoint(FrontOstacle.Position);

            }
            if (FrontOstacle!=null)
            {
                Debug.DrawLine(transform.position, FrontOstacle.Position, Color.blue);
                ObstacleX = Mathf.Sign(FrontOstacleLocal.x) * (Mathf.Abs( FrontOstacleLocal.x)-( FrontOstacle.ColliderRadius+ ColliderRadius+ Offset)) ;
            }
            if ((FrontOstacleLocal.y > 5 || FrontOstacleLocal.y < 0 || Mathf.Abs(FrontOstacleLocal.x) > FrontOstacle.ColliderRadius+ ColliderRadius+ Offset) && FrontOstacle)
            {
                FrontOstacle = null;
                ObstacleX = 0;
            }
           
        }

    }
   
  
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ColliderRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Flee(), .5f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Flee() + wanderOffset, .5f);
    }
}
