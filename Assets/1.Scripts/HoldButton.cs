using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldButton : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("������ ���۵� �� 1ȸ ȣ��")]
    public UnityEvent OnPressStart;
    [Tooltip("������ ���� �� 1ȸ ȣ��")]
    public UnityEvent OnPressEnd;
    [Tooltip("������ �� ������ ȣ�� (��� ������ ���� �ݺ� ����)")]
    public UnityEvent OnPressing;

    [Header("Click (���� Ŭ��)")]
    public UnityEvent OnClick;
    [Min(0f)] public float clickMaxDuration = 0.3f; // �� �ð� ���ϸ� 'Ŭ��'���� ����
    public bool requireDownOnThisForClick = true;   // Down�� ��ư ������ �����ؾ� Ŭ�� ����

    [Header("Slide-In & Exit Behavior")]
    [Tooltip("�հ����� ���� ä�� �����̵��� ���͵� Press �������� ����(Ŭ���� ���)")]
    public bool ignoreSlideInForPress = false;  // C ��ư�� true
    [Tooltip("�����Ͱ� ��ư ������ ������ Press ����(Up ���� ����)")]
    public bool endPressOnExit = true;          // A/B�� false�� C ���� �������� ������

    [Header("Visual")]
    public Image buttonImage;
    public Color pressedTint = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color normalTint = Color.white;

    // ����
    public bool isPointerOver;
    public bool isPressing;

    bool _downStartedHere;
    float _downTime;

    void Awake()
    {
        if (buttonImage == null) buttonImage = GetComponent<Image>();
        if (buttonImage != null) buttonImage.color = normalTint;
    }

    void Update()
    {
        bool anyDown = Input.GetMouseButton(0) || Input.touchCount > 0;

        // �����̵�-�� ���� �ɼ� ����
        if (!isPressing && isPointerOver && anyDown && !ignoreSlideInForPress)
            StartPress();

        // ������ ������ �������� ����(endPressOnExit) �ݿ�
        bool shouldEnd =
            isPressing &&
            (
                !anyDown ||                             // ���� �� ���
                (endPressOnExit && !isPointerOver)      // ������ ������, ������ ���� �ɼ��� ���� ���
            );

        if (shouldEnd) EndPress();

        if (isPressing)
        {
            if (buttonImage) buttonImage.color = pressedTint;
            OnPressing?.Invoke();
        }
        else
        {
            if (buttonImage) buttonImage.color = normalTint;
        }
    }

    void StartPress()
    {
        if (isPressing) return;
        isPressing = true;
        OnPressStart?.Invoke();
    }

    void EndPress()
    {
        if (!isPressing) return;
        isPressing = false;
        OnPressEnd?.Invoke();
    }

    // ���� �̺�Ʈ �ý��� �ݹ� ����������������������������������������������������������������
    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerOver = true;
        _downStartedHere = true;
        _downTime = Time.unscaledTime;
        StartPress(); // Down�� �׻� Press ����(Ŭ�� �����ϵ���)
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        EndPress();

        // Ŭ�� ����
        bool okStart = !requireDownOnThisForClick || _downStartedHere;
        bool okOver = isPointerOver; // Up ������ ��ư ��
        bool okTime = (Time.unscaledTime - _downTime) <= clickMaxDuration;

        if (okStart && okOver && okTime)
            OnClick?.Invoke();

        _downStartedHere = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;

        bool anyDown = Input.GetMouseButton(0) || Input.touchCount > 0;
        // ignoreSlideInForPress�� false�� ���� �����̵�-������ Press ����
        if (!ignoreSlideInForPress && anyDown && !isPressing)
            StartPress();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        // endPressOnExit�� true�� ���� ��� ����(�ƴϸ� ����)
        if (isPressing && endPressOnExit) EndPress();
    }

    void OnDisable()
    {
        isPressing = false;
        isPointerOver = false;
        _downStartedHere = false;
        if (buttonImage) buttonImage.color = normalTint;
    }
}
