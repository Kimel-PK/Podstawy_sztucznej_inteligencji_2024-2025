using System.Collections.Generic;

public class MedKit : Collectible {
	
	public static readonly List<MedKit> All = new ();
	public static bool IsAnyOnMap => All.Count > 0;

	private void Awake() {
		All.Add(this);
	}

	protected override void OnCollect(Agent agent) {
		agent.Hp = 100f;
		All.Remove(this);
	}
}
