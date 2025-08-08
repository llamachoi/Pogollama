using UnityEngine;
using System.Collections;

public class PlatformTrigger : MonoBehaviour
{
    public float baseBounceForce = 3.5f;
    private SpriteRenderer platformRenderer;

    public enum platformType
    {
        Normal,
        Cracked,
        Rainbow
    }

    public enum platformColor
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Indigo,
        Purple,
    }

    public platformColor currentPlatformColor;
    public platformType currentPlatformType;

    private bool hasAddedColor = false;

    [Range(0, 3)]
    public int setCrackCount;
    private int currentCrackCount;
    private int maxCrackCount = 4;

    private void Start()
    {
        platformRenderer = gameObject.GetComponent<SpriteRenderer>();

        switch (currentPlatformType)
        {
            case platformType.Normal:
                platformRenderer.color = ColorManager.colors[(int)currentPlatformColor];
                break;
            case platformType.Cracked:
                currentCrackCount = setCrackCount;
                platformRenderer.color = ColorManager.colors[(int)currentPlatformColor];
                platformRenderer.sprite = PlatformManager.Instance.crackedPlatformSprites[currentCrackCount];
                break;
            case platformType.Rainbow:
                platformRenderer.color = Color.white;
                platformRenderer.sprite = PlatformManager.Instance.rainbowPlatformSprite;
                break;
            default:
                Debug.LogWarning("Unknown platform type: " + currentPlatformType);
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        bool colorMatch = ((int)currentPlatformColor == ColorManager.currentColorIndex);
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        switch (currentPlatformType)
        {
            case platformType.Normal:
                if (!colorMatch) return;
                Bounce(rb);
                break;

            case platformType.Cracked:
                if (!colorMatch) return;
                Bounce(rb);
                HandleCrack();
                break;

            case platformType.Rainbow:
                Bounce(rb);
                HandleRainbow();
                break;
        }
    }

    private void Bounce(Rigidbody2D rb)
    {
        rb.linearVelocity = transform.up * baseBounceForce;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.bounceSound);
    }

    private void HandleCrack()
    {
        currentCrackCount++;

        if (currentCrackCount >= maxCrackCount)
        {
            PlatformManager.Instance.ReactivateAfterDelay(gameObject, 5f);

            AudioManager.Instance.PlaySFX(AudioManager.Instance.destroySound);

            PlatformManager.Instance.ReactivatePlatform(gameObject, 5f);
            gameObject.SetActive(false); //gameObject가 비활성화 될 경우, 현재 스크립트도 비활성화 됩니다. 따라서 매니저를 통해 다시 활성화를 요청합니다.
        }
        else
        {
            platformRenderer.sprite = PlatformManager.Instance.crackedPlatformSprites[currentCrackCount];
            AudioManager.Instance.PlaySFX(AudioManager.Instance.crackSound);
        }
    }

    public void ResetPlatform()
    {
        StartCoroutine(ResetAndCheckVisibility());
    }

    private IEnumerator ResetAndCheckVisibility()
    {
        yield return null; // 한 프레임 대기

        currentCrackCount = setCrackCount;
        platformRenderer.sprite = PlatformManager.Instance.crackedPlatformSprites[currentCrackCount];

        if (platformRenderer.isVisible)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.respawnSound);
        }
    }

    private void HandleRainbow()
    {
        if (hasAddedColor) return;

        if (ColorManager.totalColors < ColorManager.colors.Length)
        {
            ColorManager.totalColors++;
            AudioManager.Instance.PlaySFX(AudioManager.Instance.addColorSound);
            hasAddedColor = true;
        }
    }
}


