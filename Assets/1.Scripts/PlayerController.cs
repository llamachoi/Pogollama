using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rangeX;
    public SpriteRenderer pogoTipRenderer;
    [HideInInspector]
    public enum pogoColor
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Indigo,
        Purple
    }

    public static pogoColor currentColor;

    private void Start()
    {
        currentColor = pogoColor.Red;
        pogoTipRenderer.color = ColorManager.Instance.colors[(int)currentColor];
    }

    void Update()
    {
        float move = Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position + Vector3.right * move;
        //newPosition.x = Mathf.Clamp(newPosition.x, -rangeX, rangeX);

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
        int totalColors = System.Enum.GetValues(typeof(pogoColor)).Length;
        int nextColorIndex = ((int)currentColor + 1) % totalColors;
        currentColor = (pogoColor)nextColorIndex;
        pogoTipRenderer.color = ColorManager.Instance.colors[(int)currentColor];
    }
}
