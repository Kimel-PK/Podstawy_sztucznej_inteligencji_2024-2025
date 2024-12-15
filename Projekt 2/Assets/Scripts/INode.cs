using System.Collections.Generic;
using UnityEngine;

public interface INode {
	public Vector2 Position { get; }
	public List<INode> Neighbours { get; }
}