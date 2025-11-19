using UnityEngine;

public class TestAI : MonoBehaviour
{
    void Start()
    {
        if (LearningManager.Instance != null)
        {
            // Example state
            GameState state = new GameState
            {
                playerTile = 25,
                round = 2
            };

            LearningManager.Instance.WriteStateForPython(state);


            // Write state to JSON
            LearningManager.Instance.WriteStateForPython(state);

            Debug.Log("✅ State sent to Python.");
        }
        else
        {
            Debug.LogError("❌ LearningManager.Instance is null!");
        }
    }
}
