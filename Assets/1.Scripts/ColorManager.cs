using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public int CurrentColorIndex;
    public Color[] Colors;
    public static ColorManager Instance { get; private set; }
    [HideInInspector] public int TotalColors;
    [SerializeField] private int startColors = 3;

    private void Awake()
    {
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
        TotalColors = startColors;
    }
}
