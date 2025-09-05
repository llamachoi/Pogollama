using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public PlatformColor CurrentColor;
    public Color[] Colors;
    [HideInInspector] public int CurrentTotalColors;
    private int startColors = 3;

    public static ColorManager Instance { get; private set; }

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
        CurrentTotalColors = startColors;
    }

    public Color GetPogoColor()
    {
        int nextColorIndex = ((int)CurrentColor + 1) % CurrentTotalColors;
        CurrentColor = (PlatformColor)nextColorIndex;

        AudioManager.Instance.PlaySFX(AudioManager.Instance.ColorChangeSound);

        return Colors[(int)CurrentColor];
    }
}
