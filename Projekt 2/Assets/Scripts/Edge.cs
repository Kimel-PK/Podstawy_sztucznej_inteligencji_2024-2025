using System;
using UnityEngine;

[Serializable]
public class Edge {
    [field: SerializeField] public Vector3 Start { get; set; }
    [field: SerializeField] public Vector3 End { get; set; }
}