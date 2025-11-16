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

        GameManager gm = FindAnyObjectByType<GameManager>();
        PlayerProfile target = marker.playerProfile;
        int index = System.Array.IndexOf(gm.playerProfiles, target);

        if (index < 0) return;

        // 🛡 AUTO SHIELD CHECK (inventory)
        if (target.ownedItems.Contains("Shield"))
        {
            Debug.Log($"🛡 {target.playerName}'s shield auto-blocked the bomb!");
            target.ownedItems.Remove("Shield");  // consume the shield
            return; // ❗ Do NOT apply pushback
        }

        int moveBack = tilesToPushBack;

        // 💣 Owner penalty
        if (target == owner)
            moveBack -= 6;

        gm.StartCoroutine(gm.MovePlayerBackwards(index, moveBack));
    }
}
