using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static GameManager Instance { get; private set; }
	public Player Player { get; set; }

	[field: SerializeField] public List<Entity> Objects { get; set; } = new();

	private void Awake() {
		if (!Instance) {
			Instance = this;
		} else {
			Destroy(gameObject);
		}
		DontDestroyOnLoad(gameObject);
	}
}
