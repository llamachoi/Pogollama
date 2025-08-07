using UnityEngine;

public class Platform : MonoBehaviour
{
    public float baseBounceForce = 5f;
    private SpriteRenderer platformRenderer;

    public enum platformColor
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Indigo,
        Purple
    }

    public platformColor currentPlatformColor;

    private void Start()
    {
        platformRenderer = gameObject.GetComponent<SpriteRenderer>();
        platformRenderer.color = ColorManager.colors[(int)currentPlatformColor];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && (int)currentPlatformColor == ColorManager.currentColorIndex)
        {
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            playerRb.linearVelocity = transform.up * baseBounceForce;
        }
    }
}
