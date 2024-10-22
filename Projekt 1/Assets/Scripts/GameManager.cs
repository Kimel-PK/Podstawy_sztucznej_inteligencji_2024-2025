using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static GameManager Instance { get; private set; }
	
	[SerializeField] public List<Entity> Objects = new();
	public Player Player { get; set; }

	private void Awake() {
		if (!Instance) {
			Instance = this;
		} else {
			Destroy(gameObject);
		}
		DontDestroyOnLoad(gameObject);
	}
}
