using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LeaderboardUploader : MonoBehaviour
{
    public IEnumerator UploadTime(string playerName, float playTime)
    {
        WWWForm form = new WWWForm();
        form.AddField("playerName", playerName);
        form.AddField("playTime", playTime.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/save_score.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
                Debug.Log("Score uploaded: " + www.downloadHandler.text);
            else
                Debug.LogError("Upload failed: " + www.error);
        }
    }
}
