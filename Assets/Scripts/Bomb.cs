using UnityEngine;

// Explodes on contact and kills the player instantly, no matter how many lives are left.
public class Bomb : MonoBehaviour
{
    public GameObject explosionPrefab;       // particle effect
    [Tooltip("If ON, the player passes through bombs while dodging (Space)")]
    public bool ignoreWhenDodging = true;
    public float deathDelay = 1f;            // seconds before the game-over panel shows

    bool exploded;

    void OnTriggerStay2D(Collider2D other)
    {
        if (exploded || !other.CompareTag("Player")) return;
        var gm = GameManager.Instance;
        if (gm == null || gm.State != GameState.Diving) return;
        if (ignoreWhenDodging && gm.player.IsDodging) return;

        exploded = true;
        if (explosionPrefab) Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        AudioManager.Play(Sfx.Explosion);
        gm.KillPlayer("You hit a bomb!", deathDelay, true);
        Destroy(gameObject);
    }
}
