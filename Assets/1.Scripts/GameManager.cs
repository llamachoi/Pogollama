using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public BoxCollider2D groundCollider;

    [HideInInspector]
    public bool isGameOver = false;

    public TextMeshProUGUI timeText;
    private float timeElapsed = 900f;

    bool gamePaused = false;

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
        if (Input.GetKeyDown(KeyCode.R) && isGameOver)
        {
            RestartGame();
        }

        UpdateTimeUI();
    }

    public void GameOver()
    {
        AudioManager.Instance.StopBGM();
        StartCoroutine(WaitForGameOver());
    }

    IEnumerator WaitForGameOver()
    {
        yield return new WaitForSeconds(0.5f);
        gamePaused = true;
        isGameOver = true;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.gameOverSound);

        gameOverUI.SetActive(true);
    }

    public void RestartGame()
    {
        gamePaused = false;
        isGameOver = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateTimeUI()
    {
        if (gamePaused) return;
        
        if (timeElapsed <= 0f)
        {
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
