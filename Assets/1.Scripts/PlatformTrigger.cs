using UnityEngine;
using System;
using System.Collections;

public class PlatformTrigger : MonoBehaviour
{
    public float BaseBounceForce = 3.5f;
 
    private SpriteRenderer PlatformRenderer;
    public Sprite[] CrackedPlatformSprites;

    public PlatformColor CurrentPlatformColor;
    public PlatformType CurrentPlatformType;

    private bool hasAddedColor = false;

    [Range(0, 3)]
    public int SetCrackCount;

    [HideInInspector] public int CurrentCrackCount;
    private int maxCrackCount = 4;
    public float respawnTime = 5;

    public float FinishAscendSpeed = 5f;
    public float FinishDuration = 5f;
    private bool isFinishing = false;

    private void Start()
    {
        //CrackedPlatform 타입 플랫폼의 경우 데이터 초기화
        if (CurrentPlatformType == PlatformType.Cracked)
        {
            CurrentCrackCount = SetCrackCount;
            PlatformRenderer = gameObject.GetComponent<SpriteRenderer>();
            PlatformRenderer.sprite = CrackedPlatformSprites[CurrentCrackCount];
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            bool colorMatch = (CurrentPlatformColor == ColorManager.Instance.CurrentColor);
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
            StartCoroutine(ActivatePlatformAfterSeconds(respawnTime));
        }
        else
        {
            PlatformRenderer.sprite = CrackedPlatformSprites[CurrentCrackCount];
            AudioManager.Instance.PlaySFX(AudioManager.Instance.CrackSound);
        }
    }

    private void HandleRainbow()
    {
        if (hasAddedColor) return;

        if (ColorManager.Instance.CurrentTotalColors < ColorManager.Instance.Colors.Length)
        {
            ColorManager.Instance.CurrentTotalColors++;
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

    private void ActivatePlatform()
    {
        bool isActive = PlatformRenderer.enabled;

        PlatformRenderer.enabled = !isActive;
        gameObject.GetComponent<BoxCollider2D>().enabled = !isActive;

        if (!isActive) // 꺼져있던 걸 다시 켰을 때
        {
            CurrentCrackCount = SetCrackCount;
            PlatformRenderer.sprite = CrackedPlatformSprites[CurrentCrackCount];

            AudioManager.Instance.PlaySFX(AudioManager.Instance.RespawnSound);
        }
    }

    IEnumerator ActivatePlatformAfterSeconds(float respawnTime)
    {
        ActivatePlatform();
        yield return new WaitForSeconds(respawnTime);
        ActivatePlatform();
    }
}