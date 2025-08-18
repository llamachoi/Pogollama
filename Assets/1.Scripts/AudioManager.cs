using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource BgmSource;
    public AudioSource SfxSource;

    [Header("BGM Clips")]
    public AudioClip BgmSound;

    [Header("SFX Clips")]
    public AudioClip BounceSound;
    public AudioClip CrackSound;
    public AudioClip DestroySound;
    public AudioClip AddColorSound;
    public AudioClip ColorChangeSound;
    public AudioClip RespawnSound;
    public AudioClip GroundSound;
    public AudioClip GameOverSound;
    public AudioClip TimeOverSound;
    public AudioClip FinalBounceSound;
    public AudioClip GameClearSound;

    static private int[] playedIDs = new int[5];
    static private float[] lastPlayedTimes = new float[5];
    static private int playedCount = 0;
    public float SkipCoolTime = 0.05f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayBGM();
    }

    public void PlayBGM()
    {
        if (BgmSource != null && BgmSound != null)
        {
            BgmSource.clip = BgmSound;
            BgmSource.loop = true;
            BgmSource.Play();
        }
    }

    public void StopBGM()
    {
        if (BgmSource != null)
        {
            BgmSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (SfxSource == null || clip == null) return;

        int id = clip.GetInstanceID();
        float now = Time.unscaledTime;

        for (int i = 0; i < playedCount; i++)
        {
            if (playedIDs[i] == id)
            {
                if (now - lastPlayedTimes[i] < SkipCoolTime) return;

                lastPlayedTimes[i] = now;
                SfxSource.PlayOneShot(clip);
                return;
            }
        }

        if (playedCount < playedIDs.Length)
        {
            playedIDs[playedCount] = id;
            lastPlayedTimes[playedCount] = now;
            playedCount++;
        }

        SfxSource.PlayOneShot(clip);
    }
}
