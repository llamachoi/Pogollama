using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource bgmSource;
    public AudioSource sfxSource;

    public AudioClip bgmSound;
    public AudioClip bounceSound;
    public AudioClip crackSound;
    public AudioClip destroySound;
    public AudioClip addColorSound;
    public AudioClip colorChangeSound;
    public AudioClip respawnSound;
    public AudioClip groundSound;
    public AudioClip gameOverSound;
    public AudioClip timeOverSound;

    static private int[] playedIDs = new int[5];
    static private float[] lastPlayedTimes = new float[5];
    static private int playedCount = 0;
    public float skipCoolTime = 0.05f;

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
        if (bgmSource != null && bgmSound != null)
        {
            bgmSource.clip = bgmSound;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }


    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;

        int id = clip.GetInstanceID();
        float now = Time.unscaledTime;

        for (int i = 0; i < playedCount; i++)
        {
            if (playedIDs[i] == id)
            {
                if (now - lastPlayedTimes[i] < skipCoolTime) return;

                lastPlayedTimes[i] = now;
                sfxSource.PlayOneShot(clip);
                return;
            }
        }

        if (playedCount < playedIDs.Length)
        {
            playedIDs[playedCount] = id;
            lastPlayedTimes[playedCount] = now;
            playedCount++;
        }

        sfxSource.PlayOneShot(clip);
    }
}
