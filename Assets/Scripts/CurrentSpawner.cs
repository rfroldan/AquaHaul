using UnityEngine;

[System.Serializable]
public class CurrentEntry
{
    public string name;
    public GameObject prefab;
    [Tooltip("Higher = spawns more often compared to the other entries")]
    public float weight = 1f;
    public float minSpeed = 3f;
    public float maxSpeed = 4f;
    [Tooltip("How many spawn in one go (a row of coins). 1 and 1 = single object")]
    public int groupMin = 1;
    public int groupMax = 1;
    public float groupSpacing = 1.2f;
}

// Spawns coins, fish and bombs just off the RIGHT edge of the screen at random heights.
public class CurrentSpawner : MonoBehaviour
{
    public CurrentEntry[] entries;

    [Header("Timing")]
    public float minInterval = 0.8f;
    public float maxInterval = 1.6f;

    [Header("Placement")]
    public float spawnMargin = 1.5f;     // how far past the right edge things appear
    public float topMargin = 0.8f;       // keep clear of the water line
    public float bottomMargin = 0.8f;    // keep clear of the bottom of the screen

    Transform container;
    Camera cam;
    float timer;

    void Awake() { cam = Camera.main; }

    // Called by GameManager at the start of every dive.
    public void ResetSpawner()
    {
        if (container != null)
        {
            container.gameObject.SetActive(false);
            Destroy(container.gameObject);
        }
        container = new GameObject("CurrentObjects").transform;
        timer = 1f;
    }

    void Update()
    {
        if (container == null) return;
        var gm = GameManager.Instance;
        if (gm == null || gm.State != GameState.Diving) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnOne(gm);
            timer = Random.Range(minInterval, maxInterval);
        }
    }

    void SpawnOne(GameManager gm)
    {
        CurrentEntry e = Pick();
        if (e == null || e.prefab == null) return;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float minY = cam.transform.position.y - halfH + bottomMargin;
        float maxY = gm.surfaceY - topMargin;
        float y = Random.Range(minY, maxY);
        float x = cam.transform.position.x + halfW + spawnMargin;

        int count = Random.Range(e.groupMin, e.groupMax + 1);
        float speed = Random.Range(e.minSpeed, e.maxSpeed);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = new Vector3(x + i * e.groupSpacing, y, 0f);
            var go = Instantiate(e.prefab, pos, Quaternion.identity, container);
            var d = go.GetComponent<CurrentDrifter>();
            if (d != null) d.speed = speed;
        }
    }

    CurrentEntry Pick()
    {
        float total = 0f;
        foreach (var e in entries) total += Mathf.Max(0f, e.weight);
        if (total <= 0f) return null;

        float r = Random.value * total;
        foreach (var e in entries)
        {
            r -= Mathf.Max(0f, e.weight);
            if (r <= 0f) return e;
        }
        return entries[entries.Length - 1];
    }
}
