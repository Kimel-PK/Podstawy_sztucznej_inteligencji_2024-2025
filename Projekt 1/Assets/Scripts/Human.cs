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

        float distance = (transform.position - entity[i].Position).sqrMagnitude;
        bool meetsCondition = distance < 10;

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
            float collisionSize = entity[i].ColliderRadius + ColliderRadius;
            distance = (transform.position - candidate.Position).sqrMagnitude;

            if (distance < collisionSize)
                transform.position += (transform.position - candidate.Position) * (1 - (distance / collisionSize));
        }
    }
}
