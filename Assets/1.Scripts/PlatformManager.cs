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

    //�ڷ�ƾ��, StartCoroutine�� ���� ����Ǹ�, ������ ��ü ��ũ��Ʈ�� Ȱ��ȭ�Ǿ� �ִ� ���� ����˴ϴ�.
    public void ReactivatePlatform(GameObject platform, float delay)
    {
        StartCoroutine(ReactivateAfterDelay(platform, delay));
    }

    public IEnumerator ReactivateAfterDelay(GameObject platform, float delay)
    {
        yield return new WaitForSeconds(delay);

        platform.SetActive(true);
        platform.GetComponent<PlatformTrigger>().ResetPlatform();
    }
}
