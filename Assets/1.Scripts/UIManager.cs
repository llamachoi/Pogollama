using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class RunState
{
    // �� ù �����̸� true, �� ���ε� ���Ŀ� false
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
    [SerializeField] private Selectable defaultSelected; // ���� �� ��Ŀ��(�е�/Ű�����)

    [Header("Options")]
    [Tooltip("���� �� Ŀ���� ���̰� ����(PC)")]
    [SerializeField] private bool showCursorOnStart = true;

    [Tooltip("���� �� ����� BGM ��(����)")]
    [SerializeField] private AudioSource bgm;

    bool _started;

    void Awake()
    {
        bool showTitle = RunState.IsFirstBoot;

        if (startPanel)
        {
            // ���� ���� (����/�ִϸ��̼�/Update ��κ� ����)
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

        // Ŀ�� ���̱�/���� ����
        if (showCursorOnStart) { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }

        // �е�/Ű���� �׺���̼� ��� ��Ŀ�� ����
        if (defaultSelected != null)
        {
            EventSystem.current?.SetSelectedGameObject(null);
            EventSystem.current?.SetSelectedGameObject(defaultSelected.gameObject);
        }

        // ��ư ������
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

        // UI �����
        if (startPanel) startPanel.SetActive(false);

        // Ŀ�� ����(���ϸ�)
        if (!showCursorOnStart) { Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked; }

        // ���� �簳
        Time.timeScale = 1f;

        // BGM ���(����)
        if (bgm && !bgm.isPlaying) bgm.Play();

        AudioManager.Instance.PlaySFX(AudioManager.Instance.ClickSound);
    }
}
