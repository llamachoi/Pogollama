using UnityEngine;
using System.Collections;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager Instance { get; private set; }
    public Sprite normalPlatformSprite;
    public Sprite rainbowPlatformSprite;
    public Sprite[] crackedPlatformSprites;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    //코루틴은, StartCoroutine을 통해 실행되며, 실행한 주체 스크립트가 활성화되어 있는 동안 실행됩니다. 
    public void ReactivatePlatform(GameObject platform, float delay)
    {
        StartCoroutine(ReactivateAfterDelay(platform, delay));
    }

    private IEnumerator ReactivateAfterDelay(GameObject platform, float delay)
    {
        yield return new WaitForSeconds(delay);

        platform.SetActive(true);

        PlatformTrigger platformTrigger = platform.GetComponent<PlatformTrigger>();

        yield return null; // 한 프레임 대기

        platformTrigger.currentCrackCount = platformTrigger.setCrackCount;
        platformTrigger.platformRenderer.sprite = crackedPlatformSprites[platformTrigger.currentCrackCount];

        if (platformTrigger.platformRenderer.isVisible)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.respawnSound);
        }
    }
}
