using NUnit.Framework.Internal;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
     public GameObject  ObstaclePrefab;
     public float  ObstacleNumber  = 10;
     public float Range = 10;

    void Awake()
    {
        for (int i = 0; i < ObstacleNumber; i++)
        {
           GameObject obj= Instantiate(ObstaclePrefab, new Vector3(Random.Range(Range,-Range), Random.Range(Range,-Range), 0), Quaternion.identity);
            obj.transform.SetParent(transform);
            Obstacle obstacle = obj.GetComponent<Obstacle>();
           if (obstacle != null)
           {
                obstacle.ColliderRadius = Random.Range(0.5f, 1f);
           }
   
        }


        
    }

}
