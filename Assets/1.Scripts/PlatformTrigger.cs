using UnityEngine;
using System;

public class PlatformTrigger : MonoBehaviour
{
    public float BaseBounceForce = 3.5f;
 
    public SpriteRenderer PlatformRenderer;
    public PlatformColor CurrentPlatformColor;
    public PlatformType CurrentPlatformType;

    private bool hasAddedColor = false;

    [Range(0, 3)]
    public int SetCrackCount;
    public int CurrentCrackCount;
    private int maxCrackCount = 4;

    public float FinishAscendSpeed = 5f;
    public float FinishDuration = 5f;
    private bool isFinishing = false;

    private void Start()
    {
        PlatformRenderer = gameObject.GetComponent<SpriteRenderer>();

        switch (CurrentPlatformType)
        {
            case PlatformType.Normal:
                PlatformRenderer.color = ColorManager.Instance.Colors[(int)CurrentPlatformColor];
                break;
            case PlatformType.Cracked:
                CurrentCrackCount = SetCrackCount;
                PlatformRenderer.color = ColorManager.Instance.Colors[(int)CurrentPlatformColor];
                PlatformRenderer.sprite = PlatformManager.Instance.CrackedPlatformSprites[CurrentCrackCount];
                break;
            case PlatformType.Rainbow:
                PlatformRenderer.color = Color.white;
                PlatformRenderer.sprite = PlatformManager.Instance.RainbowPlatformSprite;
                break;
            case PlatformType.Finish:
                break;
            default:
                Debug.LogWarning("Unknown platform type: " + CurrentPlatformType);
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            bool colorMatch = ((int)CurrentPlatformColor == ColorManager.Instance.CurrentColorIndex);
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

            switch (CurrentPlatformType)
            {
                case PlatformType.Normal:
                    if (!colorMatch) return;
                    Bounce(rb);
                    break;
                case PlatformType.Cracked:
                    if (!colorMatch) return;
                    Bounce(rb);
                    HandleCrack();
                    break;
                case PlatformType.Rainbow:
                    Bounce(rb);
                    HandleRainbow();
                    break;
                case PlatformType.Finish:
                    HandleFinish(rb);
                    break;
            }
        }
    }

    private void Bounce(Rigidbody2D rb)
    {
        rb.linearVelocity = transform.up * BaseBounceForce;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.BounceSound);
    }

    private void HandleCrack()
    {
        CurrentCrackCount++;

        if (CurrentCrackCount >= maxCrackCount)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.DestroySound);

            PlatformManager.Instance.ReactivatePlatform(gameObject, 5f); //gameObject가 비활성화 될 경우, 현재 스크립트도 비활성화 됩니다. 따라서 매니저를 통해 다시 활성화를 요청합니다.

            gameObject.SetActive(false);
        }
        else
        {
            PlatformRenderer.sprite = PlatformManager.Instance.CrackedPlatformSprites[CurrentCrackCount];
            AudioManager.Instance.PlaySFX(AudioManager.Instance.CrackSound);
        }
    }

    private void HandleRainbow()
    {
        if (hasAddedColor) return;

        if (ColorManager.Instance.TotalColors < ColorManager.Instance.Colors.Length)
        {
            ColorManager.Instance.TotalColors++;
            AudioManager.Instance.PlaySFX(AudioManager.Instance.AddColorSound);
            hasAddedColor = true;
        }
    }

    private void HandleFinish(Rigidbody2D rb)
    {
        if (isFinishing) return;
        isFinishing = true;

        AudioManager.Instance.PlaySFX(AudioManager.Instance.FinalBounceSound);

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.up * BaseBounceForce;

        GameManager.Instance.GameClear();
    }
}