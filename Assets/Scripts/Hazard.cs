using UnityEngine;

// Goes on any enemy/obstacle that should hurt the player on touch.
// Works with trigger colliders for enemies.
public class Hazard : MonoBehaviour
{
    void OnTriggerStay2D(Collider2D other) { TryHurt(other); }
    void OnCollisionStay2D(Collision2D c) { TryHurt(c.collider); }

    void TryHurt(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        GameManager.Instance.DamagePlayer(transform.position);
    }
}
