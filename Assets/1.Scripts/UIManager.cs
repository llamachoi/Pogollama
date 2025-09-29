using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class RunState
{
    // 앱 첫 실행이면 true, 씬 리로딩 이후엔 false
    public static bool IsFirstBoot = true;
}

public class UIManager : MonoBehaviour
{
    public GameObject[] UIPogoTip;
    private Image[] pogoTipOutlines;

    private void Start()
    {
        pogoTipOutlines = new Image[UIPogoTip.Length];
        
        for (int i = 0; i < UIPogoTip.Length; i++)
        {
            UIPogoTip[i].SetActive(false);
            pogoTipOutlines[i] = UIPogoTip[i].transform.Find("Outline").GetComponent<Image>();
        }
    }

    private void Update()
    {
        UpdatePogoTipColor();
    }

    public void UpdatePogoTipColor()
    {
        for (int i = 0; i < ColorManager.Instance.CurrentTotalColors; i++)
        {
            UIPogoTip[i].GetComponent<Image>().color = ColorManager.Instance.Colors[i];
            UIPogoTip[i].SetActive(true);

            pogoTipOutlines[i].enabled = (i == (int)ColorManager.Instance.CurrentColor);
        }
    }

    [Header("Start UI")]
    [SerializeField] private GameObject startPanel;   // Panel_Start
    [SerializeField] private Button startButton;      // Button_Start
    [SerializeField] private Selectable defaultSelected; // 시작 시 포커스(패드/키보드용)

    [Header("Options")]
    [Tooltip("시작 시 커서를 보이게 할지(PC)")]
    [SerializeField] private bool showCursorOnStart = true;

    [Tooltip("시작 시 재생할 BGM 등(선택)")]
    [SerializeField] private AudioSource bgm;

    bool _started;

    void Awake()
    {
        bool showTitle = RunState.IsFirstBoot;

        if (startPanel)
        {
            // 게임 정지 (물리/애니메이션/Update 대부분 멈춤)
            Time.timeScale = 0f;
            _started = false;

            startPanel.SetActive(showTitle);
            
            if (!showTitle)
            {
                Time.timeScale = 1f;
                _started = true;
            }

            RunState.IsFirstBoot = false;
        }

        // 커서 보이기/고정 해제
        if (showCursorOnStart) { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }

        // 패드/키보드 네비게이션 대비 포커스 지정
        if (defaultSelected != null)
        {
            EventSystem.current?.SetSelectedGameObject(null);
            EventSystem.current?.SetSelectedGameObject(defaultSelected.gameObject);
        }

        // 버튼 리스너
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnClickStart);
            startButton.onClick.AddListener(OnClickStart);
        }
    }

    public void OnClickStart()
    {
        if (_started) return;
        _started = true;

        // UI 숨기기
        if (startPanel) startPanel.SetActive(false);

        // 커서 숨김(원하면)
        if (!showCursorOnStart) { Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked; }

        // 게임 재개
        Time.timeScale = 1f;

        // BGM 재생(선택)
        if (bgm && !bgm.isPlaying) bgm.Play();

        AudioManager.Instance.PlaySFX(AudioManager.Instance.ClickSound);
    }
}
