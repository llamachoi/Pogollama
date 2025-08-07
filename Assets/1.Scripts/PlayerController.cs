using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rangeX;
    public SpriteRenderer pogoTipRenderer;

    private void Start()
    {
        pogoTipRenderer.color = ColorManager.colors[0];
    }

    void Update()
    {
        float move = Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position + Vector3.right * move;

        if (newPosition.x < -rangeX)
        {
            newPosition.x = rangeX;
        }
        else if (newPosition.x > rangeX)
        {
            newPosition.x = -rangeX;
        }

        transform.position = newPosition;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangePogoColor();
        }
    }

    void ChangePogoColor()
    {
        int nextColorIndex = (ColorManager.currentColorIndex + 1) % ColorManager.totalColors;
        ColorManager.currentColorIndex = nextColorIndex;
        pogoTipRenderer.color = ColorManager.colors[ColorManager.currentColorIndex];
    }
}
