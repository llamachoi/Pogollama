using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [HideInInspector] public bool IsGameOver = false;
    private bool canReset = false;

    public TextMeshProUGUI TimeText;
    public float TimeElapsed = 900f;

    public GameObject GameStartUI;
    public GameObject GameClearUI;
    public TextMeshProUGUI FinalHeightText;

    public Slider EnergySlider;
    private float TotalEnergy = 100f;
    public float EnergyConsumptionRate = 5f;
    [Range (0, 100)] public float EnergyRechargeRate = 50f;

    public TextMeshProUGUI HeightText;
    public TextMeshProUGUI BestHeightText;
    
    private int currentHeight;
    private int bestHeight;

    private GameObject player;

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
        GameClearUI.SetActive(false);

        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && canReset)
        {
            RestartGame();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            PlayerPrefs.DeleteAll();
        }

        UpdateEnergyUI();
        UpdateHeight();
    }

    public void GameOver()
    {
        if (!IsGameOver) StartCoroutine(WaitForGameOver());
    }

    IEnumerator WaitForGameOver()
    {
        IsGameOver = true;
        AudioManager.Instance.StopBGM();

        yield return new WaitForSeconds(0.5f);

        canReset = true;

        if (currentHeight > bestHeight)
        {
            bestHeight = currentHeight;
        }

        FinalHeightText.text = $"{bestHeight:0}m";
        AudioManager.Instance.PlaySFX(AudioManager.Instance.GameClearSound);
        GameClearUI.SetActive(true);
    }

    public void RestartGame()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ClickSound);
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateEnergyUI()
    {
        if (IsGameOver) return;
        TotalEnergy -= EnergyConsumptionRate * Time.deltaTime;
        TotalEnergy = Mathf.Clamp(TotalEnergy, 0f, 100f);
        EnergySlider.value = TotalEnergy / 100f;

        if (TotalEnergy <= 0f)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.TimeOverSound);
            GameOver();
        }
    }

    public void RechargeEnergy()
    {
        if (IsGameOver) return;
        // 남아있는 에너지의 50% 만큼 충전
        TotalEnergy += EnergyRechargeRate;
        TotalEnergy = Mathf.Clamp(TotalEnergy, 0f, 100f);
        EnergySlider.value = TotalEnergy / 100f;
    }

    private void UpdateHeight()
    {
        if (IsGameOver) return;
        currentHeight = (int)Mathf.Max(player.transform.position.y, 0);
        currentHeight = Mathf.Min(9999, currentHeight);
        HeightText.text = $"{currentHeight:0}m";

        if (currentHeight >= bestHeight)
        {
            bestHeight = currentHeight;
            bestHeight = Mathf.Min(9999, bestHeight);

            BestHeightText.text = $"BEST:{bestHeight:0}m";
        }
    }
}
