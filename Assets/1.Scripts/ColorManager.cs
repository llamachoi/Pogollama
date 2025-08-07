using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public static int currentColorIndex = 0;
    
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
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        totalColors = startColors;
    }
}
