using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float maxFallSpeed = -10f;
    public float rangeX;
    public float minY = -4f;

    public SpriteRenderer pogoTipRenderer;
    private Rigidbody2D rb;

    private void Start()
    {
        pogoTipRenderer.color = ColorManager.Instance.Colors[0];
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver) { HandleGameOver(); return; }

        float move = Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position + Vector3.right * move;

        if (newPosition.x < -rangeX) newPosition.x = rangeX;
        else if (newPosition.x > rangeX) newPosition.x = -rangeX;

        transform.position = newPosition;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            pogoTipRenderer.color = ColorManager.Instance.GetPogoColor();
        }
    }

    void FixedUpdate()
    {
        // Y속도가 maxFallSpeed보다 작아지면(더 빨라지면) 제한
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
