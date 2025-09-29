using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldButton : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("눌림이 시작될 때 1회 호출")]
    public UnityEvent OnPressStart;
    [Tooltip("눌림이 끝날 때 1회 호출")]
    public UnityEvent OnPressEnd;
    [Tooltip("눌리는 매 프레임 호출 (길게 누르는 동안 반복 실행)")]
    public UnityEvent OnPressing;

    [Header("Click (단일 클릭)")]
    public UnityEvent OnClick;
    [Min(0f)] public float clickMaxDuration = 0.3f; // 이 시간 이하면 '클릭'으로 판정
    public bool requireDownOnThisForClick = true;   // Down이 버튼 위에서 시작해야 클릭 인정

    [Header("Slide-In & Exit Behavior")]
    [Tooltip("손가락을 누른 채로 슬라이드해 들어와도 Press 시작하지 않음(클릭만 허용)")]
    public bool ignoreSlideInForPress = false;  // C 버튼에 true
    [Tooltip("포인터가 버튼 밖으로 나가도 Press 유지(Up 때만 종료)")]
    public bool endPressOnExit = true;          // A/B에 false면 C 위로 지나가도 유지됨

    [Header("Visual")]
    public Image buttonImage;
    public Color pressedTint = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color normalTint = Color.white;

    // 상태
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

        // 슬라이드-인 무시 옵션 적용
        if (!isPressing && isPointerOver && anyDown && !ignoreSlideInForPress)
            StartPress();

        // 밖으로 나가면 종료할지 여부(endPressOnExit) 반영
        bool shouldEnd =
            isPressing &&
            (
                !anyDown ||                             // 손을 뗀 경우
                (endPressOnExit && !isPointerOver)      // 밖으로 나갔고, 나가면 종료 옵션이 켜진 경우
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

    // ── 이벤트 시스템 콜백 ────────────────────────────────
    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerOver = true;
        _downStartedHere = true;
        _downTime = Time.unscaledTime;
        StartPress(); // Down은 항상 Press 시작(클릭 가능하도록)
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        EndPress();

        // 클릭 판정
        bool okStart = !requireDownOnThisForClick || _downStartedHere;
        bool okOver = isPointerOver; // Up 시점에 버튼 위
        bool okTime = (Time.unscaledTime - _downTime) <= clickMaxDuration;

        if (okStart && okOver && okTime)
            OnClick?.Invoke();

        _downStartedHere = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;

        bool anyDown = Input.GetMouseButton(0) || Input.touchCount > 0;
        // ignoreSlideInForPress가 false일 때만 슬라이드-인으로 Press 시작
        if (!ignoreSlideInForPress && anyDown && !isPressing)
            StartPress();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        // endPressOnExit이 true일 때만 즉시 종료(아니면 유지)
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
