using UnityEngine;
using System.Collections;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager Instance { get; private set; }
    public Sprite RainbowPlatformSprite;
    public Sprite[] CrackedPlatformSprites;

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

    //�ڷ�ƾ��, StartCoroutine�� ���� ����Ǹ�, ������ ��ü ��ũ��Ʈ�� Ȱ��ȭ�Ǿ� �ִ� ���� ����˴ϴ�. 
    public void ReactivatePlatform(GameObject platform, float delay)
    {
        StartCoroutine(ReactivateAfterDelay(platform, delay));
    }

    private IEnumerator ReactivateAfterDelay(GameObject platform, float delay)
    {
        yield return new WaitForSeconds(delay);

        platform.SetActive(true);

        PlatformTrigger platformTrigger = platform.GetComponent<PlatformTrigger>();

        yield return null; // �� ������ ���

        platformTrigger.CurrentCrackCount = platformTrigger.SetCrackCount;
        platformTrigger.PlatformRenderer.sprite = CrackedPlatformSprites[platformTrigger.CurrentCrackCount];

        if (platformTrigger.PlatformRenderer.isVisible)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.RespawnSound);
        }
    }
}
