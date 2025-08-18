using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public BoxCollider2D GroundCollider;

    [HideInInspector] public bool IsGameOver = false;
    private bool canReset = false;

    public TextMeshProUGUI TimeText;
    public float TimeElapsed = 900f;

    public GameObject GameOverUI;
    public GameObject GameClearUI;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI BestScoreText;

    private float currentScore;
    private float bestScore;

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
        GameOverUI.SetActive(false);
        GameClearUI.SetActive(false);

        bestScore = PlayerPrefs.GetFloat("BestScore", 0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && canReset)
        {
            RestartGame();
        }

        UpdateTimeUI();
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
        AudioManager.Instance.PlaySFX(AudioManager.Instance.GameOverSound);

        canReset = true;
        GameOverUI.SetActive(true);
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateTimeUI()
    {
        if (IsGameOver) return;

        if (TimeElapsed <= 0f)
        {
            TimeElapsed = 0f;
            TimeText.text = "00:00.00";

            AudioManager.Instance.PlaySFX(AudioManager.Instance.TimeOverSound);

            GameOver();
            return;
        }

        TimeElapsed -= Time.deltaTime;

        int minutes = (int)(TimeElapsed / 60f);
        int seconds = (int)(TimeElapsed % 60f);
        int centiseconds = (int)((TimeElapsed - Mathf.Floor(TimeElapsed)) * 100f);
        TimeText.text = $"{minutes:00}:{seconds:00}.{centiseconds:00}";
    }

    public void GameClear()
    {
        if (!IsGameOver) StartCoroutine(WaitForGameClear());
    }

    IEnumerator WaitForGameClear()
    {
        IsGameOver = true;

        yield return new WaitForSeconds(1f);

        Camera.main.GetComponent<CameraMovement>().enabled = false;

        AudioManager.Instance.StopBGM();

        yield return new WaitForSeconds(0.5f);
        
        AudioManager.Instance.PlaySFX(AudioManager.Instance.GameClearSound);

        canReset = true;

        ScoreText.text = TimeText.text;

        currentScore = TimeElapsed;

        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            BestScoreText.text = "NEW RECORD!";
            PlayerPrefs.SetFloat("BestScore", bestScore);
            PlayerPrefs.Save();
        }
        else
        {
            int minutes = (int)(bestScore / 60f);
            int seconds = (int)(bestScore % 60f);
            int centiseconds = (int)((bestScore - Mathf.Floor(bestScore)) * 100f);

            BestScoreText.text = $"BEST SCORE {minutes:00}:{seconds:00}.{centiseconds:00}";
        }

        GameClearUI.SetActive(true);
    }
}
