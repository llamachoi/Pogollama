using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public static int currentColorIndex;
    [SerializeField] private Color[] colorPalette;
    public static Color[] colors;
    public static ColorManager Instance { get; private set; }
    public static int totalColors;
    public int startColors = 3;

    private void Awake()
    {
        colors = colorPalette;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // static �ʵ�� Scene�� �ε�ǵ� �ʱ�ȭ���� �����Ƿ�, Start���� �ʱ�ȭ�մϴ�.
        totalColors = startColors;
        currentColorIndex = 0;
    } 
}
