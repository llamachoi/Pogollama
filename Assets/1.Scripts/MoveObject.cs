using UnityEngine;

[RequireComponent(typeof(Renderer))] // 가시성 콜백 보장
public class MoveObject : MonoBehaviour
{
    [Header("Horizontal Movement Settings")]
    public float horizontalSpeed = 5f; // Base horizontal movement speed
    public Vector2 horizontalSpeedRange = new Vector2(-1f, 1f); // Random range for horizontal speed adjustment
    public float horizontalMin = -10f; // Horizontal minimum position
    public float horizontalMax = 10f; // Horizontal maximum position

    [Header("Vertical Movement Settings")]
    public float verticalSpeed = 2f; // Base vertical movement speed
    public Vector2 verticalSpeedRange = new Vector2(-0.5f, 0.5f); // Random range for vertical speed adjustment
    public float verticalMin = -1f; // Vertical minimum position
    public float verticalMax = 1f; // Vertical maximum position

    // ───────────────────────────────────────────
    [Header("Offscreen Optimization")]
    [Tooltip("화면 밖일 때 움직임/시뮬레이션을 일시정지")]
    public bool pauseSimulationWhenOffscreen = true;
    [Tooltip("화면 밖일 때 꺼둘 추가 컴포넌트(Animator 등)")]
    public Behaviour[] extraDisableWhenOffscreen;

    private float verticalPingPongTimer = 0f;
    private float initialLocalY; // Store the initial local Y position

    private float adjustedHorizontalSpeed;
    private float adjustedVerticalSpeed;

    private float currentVerticalPosition;

    // 가시성/시뮬레이션 타이밍
    private bool _isVisible = false;
    private float _lastSimTime;

    void Start()
    {
        // Store the initial local Y position for relative vertical movement
        initialLocalY = transform.localPosition.y;

        // Apply random speed adjustments
        adjustedHorizontalSpeed = horizontalSpeed + Random.Range(horizontalSpeedRange.x, horizontalSpeedRange.y);
        adjustedVerticalSpeed = verticalSpeed + Random.Range(verticalSpeedRange.x, verticalSpeedRange.y);

        // Initialize current vertical position
        currentVerticalPosition = initialLocalY;

        // 시뮬레이션 시간 기준
        _lastSimTime = Time.time;

        // 초기 가시성 추정 및 상태 적용
        TryInitialVisibilitySync();
    }

    void Update()
    {
        // 화면에 보이지 않고, 오프스크린 일시정지가 켜져 있으면 계산 생략
        if (!_isVisible && pauseSimulationWhenOffscreen)
        {
            _lastSimTime = Time.time; // 복귀 시 큰 dt 방지
            return;
        }

        float dt = Time.time - _lastSimTime;
        if (dt <= 0f) return;

        MoveHorizontalWithVertical(dt);

        _lastSimTime = Time.time;
    }

    private void MoveHorizontalWithVertical(float dt)
    {
        // Horizontal movement (Global)
        Vector3 newPosition = transform.position;
        newPosition.x += adjustedHorizontalSpeed * dt;

        if (newPosition.x > horizontalMax)
        {
            newPosition.x = horizontalMin;
        }

        transform.position = newPosition;

        // Vertical PingPong movement with Lerp (Local, relative to initial position)
        verticalPingPongTimer += dt * adjustedVerticalSpeed;
        float targetVerticalOffset = Mathf.PingPong(verticalPingPongTimer, verticalMax - verticalMin) + verticalMin;
        currentVerticalPosition = Mathf.Lerp(currentVerticalPosition, initialLocalY + targetVerticalOffset, dt * verticalSpeed);

        Vector3 localPosition = transform.localPosition;
        localPosition.y = currentVerticalPosition;
        transform.localPosition = localPosition;
    }

    // ───────────────────────────────────────────
    // Visibility callbacks (Renderer 필요)

    void OnBecameVisible()
    {
        _isVisible = true;
        ToggleExtras(enableExtras: true);
    }

    void OnBecameInvisible()
    {
        _isVisible = false;
        ToggleExtras(enableExtras: false);
    }

    // 추가 컴포넌트 토글(이 스크립트/Renderer는 유지)
    void ToggleExtras(bool enableExtras)
    {
        if (extraDisableWhenOffscreen == null) return;
        for (int i = 0; i < extraDisableWhenOffscreen.Length; i++)
        {
            var beh = extraDisableWhenOffscreen[i];
            if (!beh) continue;
            beh.enabled = enableExtras;
        }
    }

    // 초기 가시성 추정(씬 시작 프레임에 Invisible 콜백이 바로 안 올 수 있으므로)
    void TryInitialVisibilitySync()
    {
        Camera cam = Camera.main;
        if (!cam) return;

        Vector3 v = cam.WorldToViewportPoint(transform.position);
        bool visibleNow = (v.z > 0f && v.x > -0.05f && v.x < 1.05f && v.y > -0.05f && v.y < 1.05f);

        _isVisible = visibleNow;
        ToggleExtras(enableExtras: visibleNow);
    }
}
