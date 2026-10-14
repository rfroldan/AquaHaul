using UnityEngine;

public enum Sfx { Splash, Pickup, Upgrade, Warning, Hurt, HitWall, Dodge, Click, Explosion }

// One per scene. Game scene: fill everything.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource ambientSource;
    public AudioSource sfxSource;

    [Header("Looping")]
    public AudioClip music;
    public AudioClip ambient;

    [Header("Sound Effects")]
    public AudioClip splash;
    public AudioClip pickup;
    public AudioClip upgrade;
    public AudioClip oxygenWarning;
    public AudioClip hurt;
    public AudioClip hitWall;
    public AudioClip dodge;
    public AudioClip click;
    public AudioClip explosion;

    void Awake() { Instance = this; }
    void OnDestroy() { if (Instance == this) Instance = null; }

    void Start()
    {
        StartLoop(musicSource, music);
        StartLoop(ambientSource, ambient);
    }

    void StartLoop(AudioSource src, AudioClip clip)
    {
        if (!src || !clip) return;
        src.clip = clip;
        src.loop = true;
        src.Play();
    }

    public static void Play(Sfx s)
    {
        if (Instance == null) return;
        Instance.PlayOneShotClip(Instance.Get(s));
    }

    public void PlayOneShotClip(AudioClip clip, float volume = 1f)
    {
        if (clip && sfxSource) sfxSource.PlayOneShot(clip, volume);
    }

    AudioClip Get(Sfx s)
    {
        switch (s)
        {
            case Sfx.Splash: return splash;
            case Sfx.Pickup: return pickup;
            case Sfx.Upgrade: return upgrade;
            case Sfx.Warning: return oxygenWarning;
            case Sfx.Hurt: return hurt;
            case Sfx.HitWall: return hitWall;
            case Sfx.Dodge: return dodge;
            case Sfx.Explosion: return explosion;
            default: return click;
        }
    }
}
