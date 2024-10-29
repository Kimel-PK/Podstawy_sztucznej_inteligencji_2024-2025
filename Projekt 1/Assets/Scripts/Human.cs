using System.Collections.Generic;
using UnityEngine;

public class Human : Entity
{
    [SerializeField] protected List<Entity> collideCandidates = new();
    protected float colliderCheckDistance = 5;

    protected virtual void Update()
    {
        CollideCheck(new List<Entity> (GameManager.Instance.Objects));
    }

    protected virtual void CollideCheck(List<Entity> entity)
    {
        // discard self
        entity.Remove(this);

        int i = Time.frameCount % entity.Count;

        float distance = Vector3.Distance(transform.position, entity[i].Position);
        bool meetsCondition = distance < colliderCheckDistance;

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

        for (int j = collideCandidates.Count - 1; j >= 0; j--)
        {
            if (!collideCandidates[j])
            {
                collideCandidates.RemoveAt(j);
                continue;
            }

            float collisionSize = collideCandidates[j].ColliderRadius + ColliderRadius;
           float distance2 = Vector3.Distance(transform.position, collideCandidates[j].Position);

            if (distance2 <= collisionSize)
                transform.position += (transform.position - collideCandidates[j].Position) * (1 - (distance2 / collisionSize));
        }
    }
}
