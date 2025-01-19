using System.Collections.Generic;

public class ArmorKit : Collectible {
	
	public static readonly List<ArmorKit> All = new ();
	public static bool IsAnyOnMap => All.Count > 0;
	
	private void Awake() {
		All.Add(this);
	}
	
	protected override void OnCollect(Agent agent) {
		agent.Armor = agent.MaxArmor;
		All.Remove(this);
	}
}
