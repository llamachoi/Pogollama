using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public BoxCollider2D groundCollider;

    [HideInInspector] public bool isGameOver = false;
    private bool canReset = false;

    public TextMeshProUGUI timeText;
    public float timeElapsed = 900f;

    public GameObject gameOverUI;

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
        gameOverUI.SetActive(false);
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
        if (!isGameOver) StartCoroutine(WaitForGameOver());
    }

    IEnumerator WaitForGameOver()
    {
        isGameOver = true;
        AudioManager.Instance.StopBGM();

        yield return new WaitForSeconds(0.5f);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.gameOverSound);

        canReset = true;
        gameOverUI.SetActive(true);
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateTimeUI()
    {
        if (isGameOver) return;

        if (timeElapsed <= 0f)
        {
            timeElapsed = 0f;
            timeText.text = "00:00.00";

            GameOver();
            return;
        }

        timeElapsed -= Time.deltaTime;

        int minutes = (int)(timeElapsed / 60f);
        int seconds = (int)(timeElapsed % 60f);
        int centiseconds = (int)((timeElapsed - Mathf.Floor(timeElapsed)) * 100f);
        timeText.text = $"{minutes:00}:{seconds:00}.{centiseconds:00}";
    }
}
