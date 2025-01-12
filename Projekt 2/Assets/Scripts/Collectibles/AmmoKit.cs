using System.Collections.Generic;

public class AmmoKit : Collectible {
    
    public static readonly List<AmmoKit> All = new ();
    public static bool IsAnyOnMap => All.Count > 0;
    
    private void Awake() {
        All.Add(this);
    }
    
    protected override void OnCollect(Agent agent) {
        agent.Ammo = 20;
        All.Remove(this);
    }
}
