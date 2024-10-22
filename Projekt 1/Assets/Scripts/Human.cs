using System.Collections.Generic;
using UnityEngine;

public class Human : Entity
{

    [SerializeField] private List<Entity> collideCandidates = new();


    protected virtual void Update()
    {
        CollideCheck(new List<Entity> (GameManager.Instance.Objects));
    }

    private void CollideCheck(List<Entity> entity)
    {
        // discard self
        entity.Remove(this);

        int i = Time.frameCount % entity.Count;

        float distance = Vector3.Distance(transform.position, entity[i].Position);
        bool meetsCondition = distance < 5;

        if (meetsCondition)
        {
            if (!collideCandidates.Contains(entity[i]))
            {
                collideCandidates.Add(entity[i]);
                Debug.DrawRay(transform.position, entity[i].Position - transform.position, Color.blue);
            }
        }
        else
            collideCandidates.Remove(entity[i]);

        foreach (Entity candidate in collideCandidates)
        {
           float collisionSize = candidate.ColliderRadius + ColliderRadius;
           float distance2 = Vector3.Distance(transform.position, candidate.Position);

            if (distance2 <= collisionSize)
                transform.position += (transform.position - candidate.Position) * (1 - (distance2 / collisionSize));
        }
    }
}
