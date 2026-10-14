using UnityEngine;

// Goes on every coin prefab. Collider must be a trigger.
public class Coin : MonoBehaviour
{
    public int value = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance.CollectCoin(value)) Destroy(gameObject);
    }
}
