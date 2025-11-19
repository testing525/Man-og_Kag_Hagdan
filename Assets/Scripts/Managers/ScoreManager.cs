using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text[] scoreTexts;  // Assign 4 TMP_Texts in Inspector

    private PlayerProfile[] playerProfiles; // Will be assigned at runtime

    public void SetPlayerProfiles(PlayerProfile[] profiles)
    {
        playerProfiles = profiles;
        UpdateScoreUI();
    }

    public void UpdateScoreUI()
    {
        if (playerProfiles == null || scoreTexts.Length != playerProfiles.Length)
        {
            Debug.LogWarning("⚠️ ScoreTexts and PlayerProfiles length mismatch!");
            return;
        }

        for (int i = 0; i < playerProfiles.Length; i++)
        {
            if (scoreTexts[i] != null && playerProfiles[i] != null)
            {
                scoreTexts[i].text = $"{playerProfiles[i].points} Points";
            }
        }
    }
}
