using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float maxFallSpeed = -10f;
    public float rangeX;
    public float minY = -4f;

    public SpriteRenderer pogoTipRenderer;
    private Rigidbody2D rb;

    public HoldButton leftButton;
    public HoldButton rightButton;

    private void Start()
    {
        pogoTipRenderer.color = ColorManager.Instance.Colors[0];
        rb = GetComponent<Rigidbody2D>();
    }

    public void ChangeColor()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ColorChangeSound);
        pogoTipRenderer.color = ColorManager.Instance.GetPogoColor();
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver) { HandleGameOver(); return; }

        float move = Input.GetAxisRaw("Horizontal"); // 키보드 입력

        // UI 버튼 입력도 합쳐줌
        if (leftButton.isPressing) move = -1f;
        if (rightButton.isPressing) move = 1f;

        Vector3 newPosition = transform.position + Vector3.right * move * moveSpeed * Time.deltaTime;

        if (newPosition.x < -rangeX) newPosition.x = rangeX;
        else if (newPosition.x > rangeX) newPosition.x = -rangeX;

        transform.position = newPosition;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeColor();
        }
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.y < maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxFallSpeed);
        }
    }

    void HandleGameOver()
    {
        CircleCollider2D playerCollider = GetComponent<CircleCollider2D>();

        if (playerCollider.enabled) playerCollider.enabled = false;
        if (transform.position.y < minY && gameObject.activeSelf) gameObject.SetActive(false);
    }

}
