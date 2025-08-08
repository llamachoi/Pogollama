using UnityEngine;

public class Platform : MonoBehaviour
{
    public float baseBounceForce = 3.5f;
    private SpriteRenderer platformRenderer;

    public enum platformColor
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Indigo,
        Purple,
        Rainbow
    }

    public platformColor currentPlatformColor;

    private bool hasAddedColor = false;

    private void Start()
    {
        platformRenderer = gameObject.GetComponent<SpriteRenderer>();

        if (currentPlatformColor == platformColor.Rainbow) return;

        platformRenderer.color = ColorManager.colors[(int)currentPlatformColor];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && (int)currentPlatformColor == ColorManager.currentColorIndex || currentPlatformColor == platformColor.Rainbow)
        {
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            playerRb.linearVelocity = transform.up * baseBounceForce;

            AudioManager.Instance.PlaySFX(AudioManager.Instance.bounceSound);

            if (currentPlatformColor == platformColor.Rainbow && !hasAddedColor && ColorManager.totalColors < ColorManager.colors.Length)
            {
                ColorManager.totalColors++;
                AudioManager.Instance.PlaySFX(AudioManager.Instance.addColorSound);
                hasAddedColor = true;
            }
        }
    }
}
