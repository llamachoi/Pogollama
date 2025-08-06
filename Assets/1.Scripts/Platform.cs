using UnityEngine;

public class Platform : MonoBehaviour
{
    public float baseBounceForce = 10f;
    public float reactiveMultiplier = 1.0f; // 떨어지는 속도의 반작용 비율
    public SpriteRenderer platformRenderer;

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
        platformRenderer.color = ColorManager.Instance.colors[(int)currentPlatformColor];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && (int)currentPlatformColor == (int)PlayerController.currentColor)
        {
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            Vector2 currentVelocity = playerRb.linearVelocity;

            // 하강 속도 계산 (플랫폼 방향과 반대 방향의 속도)
            float fallSpeed = -Vector2.Dot(currentVelocity, transform.up);

            if (fallSpeed > 0f) // 실제로 플랫폼 방향으로 충돌했을 때만
            {
                float totalBounce = baseBounceForce + (fallSpeed * reactiveMultiplier);
                playerRb.linearVelocity = transform.up * totalBounce;
                playerRb.angularVelocity = 0f;
            }
        }
    }
}
