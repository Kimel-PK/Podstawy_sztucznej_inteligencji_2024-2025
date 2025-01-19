using TMPro;
using UnityEngine;

public class UI : MonoBehaviour {
    
    [SerializeField] private TMP_Text redText;
    [SerializeField] private TMP_Text greenText;
    [SerializeField] private TMP_Text blueText;
    [SerializeField] private TMP_Text yellowText;
    
    void Start()
    {
        foreach (Agent agent in Agent.All) {
            agent.OnEnemyKilled += UpdateText;
            agent.OnDied += UpdateText;
        }
    }

    private void OnDestroy() {
        foreach (Agent agent in Agent.All) {
            agent.OnEnemyKilled -= UpdateText;
            agent.OnDied -= UpdateText;
        }
    }

    private void UpdateText() {
        foreach (Agent agent in Agent.All) {
            switch (agent.AgentName) {
                case "Red":
                    redText.text = $"Red     K: {agent.Kills} D: {agent.Deads}";
                    break;
                case "Green":
                    greenText.text = $"Green  K: {agent.Kills} D: {agent.Deads}";
                    break;
                case "Blue":
                    blueText.text = $"Blue     K: {agent.Kills} D: {agent.Deads}";
                    break;
                case "Yellow":
                    yellowText.text = $"Yellow  K: {agent.Kills} D: {agent.Deads}";
                    break;
            }
        }
    }
}
