using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float destroyDelay = 0.5f;
    public int tilesToPushBack = 8;
    public PlayerProfile owner;

    private void Start()
    {
        Destroy(gameObject, destroyDelay);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerMarker marker = collision.GetComponent<PlayerMarker>();
        if (!marker) return;

        PlayerProfile target = marker.playerProfile;

        GameManager gm = FindAnyObjectByType<GameManager>();
        GameManager_Bots gmBots = FindAnyObjectByType<GameManager_Bots>();

        int index = -1;

        if (gm != null)
            index = System.Array.IndexOf(gm.playerProfiles, target);
        else if (gmBots != null)
            index = System.Array.IndexOf(gmBots.playerProfiles, target);

        if (index < 0) return;

        // Shield block
        if (target.ownedItems.Contains("Shield"))
        {
            Debug.Log($"🛡 {target.playerName}'s shield auto-blocked the bomb!");
            target.ownedItems.Remove("Shield");
            return;
        }

        // Pushback amount
        int moveBack = tilesToPushBack;
        if (target == owner)
            moveBack -= 6;

        // ⭐ Register the effect (but don't handle turn flow)
        //if (gm != null)
            //gm.RegisterBombPushback(index, moveBack);
        if (gmBots != null)
            gmBots.RegisterBombPushback(index, moveBack);
    }


}
