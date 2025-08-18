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
        // static 필드는 Scene이 로드되도 초기화되지 않으므로, Start에서 초기화합니다.
        totalColors = startColors;
        currentColorIndex = 0;
    } 
}
