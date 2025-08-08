using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource bgmSource;
    public AudioSource sfxSource;

    public AudioClip bgmSound;
    public AudioClip bounceSound;
    public AudioClip addColorSound;
    public AudioClip colorChangeSound;

    private AudioClip lastPlayedClip;
    private float lastPlayedTime;
    public float sameClipCooldown;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null && clip == null) { Debug.Log("Audio Source or Clip is Missing"); return; }
        if (clip == lastPlayedClip && Time.time - lastPlayedTime < sameClipCooldown) return; // 동일 효과음 겹침 방지

        sfxSource.PlayOneShot(clip);

        lastPlayedClip = clip;
        lastPlayedTime = Time.time;
    }
}
