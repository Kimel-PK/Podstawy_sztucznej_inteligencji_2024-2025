using UnityEngine;

public class DeadState : AgentState {
    private const float RespawnDelay = 4f;
    private float timer;
    
    public DeadState(Agent agent) : base(agent) { }

    public override void Update() {
        if (timer > 0) {
            timer -= Time.deltaTime;
            return;
        }

        Agent.Hp = 100;
        Agent.Armor = 100;
        Agent.Ammo = 20;
        
        NavGraph.NavGraphNode randomNode = NavGraph.Instance.GetRandomNode();
        Agent.transform.position = randomNode.Position / 1000f;
    }

    public override void OnEnter() {
        timer = RespawnDelay;
        Agent.path.Clear();
        Agent.rotationTarget = Vector3.zero;
        Agent.model.SetActive(false);
    }

    public override void OnExit() {
        Agent.model.SetActive(true);
    }

    public override string ToString() => $"Respawns in {timer}";
}
