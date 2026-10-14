using UnityEngine;

// Goes on coins, fish and bombs. The "current" carries them from the right of the screen
// to the left, and they delete themselves once they are off the left edge.
public class CurrentDrifter : MonoBehaviour
{
    [Tooltip("Units per second to the LEFT (the spawner overrides this per spawn)")]
    public float speed = 3f;

    [Header("Optional motion")]
    public float bobHeight = 0f;     // gentle up/down wobble
    public float bobSpeed = 2f;
    public float spinSpeed = 0f;     // degrees per second (nice for bombs)

    [Header("Sprite")]
    [Tooltip("Tick if your art faces RIGHT, so it is flipped to face the way it travels")]
    public bool flipSprite = false;

    [Header("Sound")]
    public AudioClip enterSound;     // plays once when it appears on screen (fish splash, bomb fuse)

    public float destroyMargin = 2f; // how far past the left edge before deleting

    Camera cam;
    float baseY, phase;
    bool soundPlayed;

    void Start()
    {
        cam = Camera.main;
        baseY = transform.position.y;
        phase = Random.value * 6.28f;
        if (flipSprite)
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr) sr.flipX = true;
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        Vector3 p = transform.position;
        p.x -= speed * dt;
        if (bobHeight > 0f) p.y = baseY + Mathf.Sin(Time.time * bobSpeed + phase) * bobHeight;
        transform.position = p;

        if (spinSpeed != 0f) transform.Rotate(0f, 0f, spinSpeed * dt);

        float halfW = cam.orthographicSize * cam.aspect;
        float left = cam.transform.position.x - halfW;
        float right = cam.transform.position.x + halfW;

        if (!soundPlayed && enterSound && p.x < right - 0.5f)
        {
            soundPlayed = true;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayOneShotClip(enterSound, 0.7f);
        }

        if (p.x < left - destroyMargin) Destroy(gameObject);
    }
}
