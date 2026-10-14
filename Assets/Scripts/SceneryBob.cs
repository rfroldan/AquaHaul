using UnityEngine;

// Gentle floating motion for the boat and water-surface line. Pure scenery.
public class SceneryBob : MonoBehaviour
{
    public float amplitude = 0.08f;
    public float speed = 1f;
    public float rockDegrees = 1.5f;
    public float phaseOffset = 0f;

    Vector3 startPos;

    void Start() { startPos = transform.position; }

    void Update()
    {
        float t = Time.time * speed + phaseOffset;
        transform.position = startPos + new Vector3(0f, Mathf.Sin(t) * amplitude, 0f);
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.8f) * rockDegrees);
    }
}
