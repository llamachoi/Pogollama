using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraMovement : MonoBehaviour
{
    [Header("Follow")]
    public Transform Target;
    [Min(0f)] public float FollowSpeed = 5f;

    [Header("Bottom Anchor (고정)")]
    [Min(0.01f)] public float desiredWorldWidth = 16f; // 항상 유지할 월드 가로폭
    public float bottomY = 0f;                         // 화면 '아래'가 가리킬 월드 Y (땅 높이 등)

    [Header("Follow Logic")]
    [Tooltip("플레이어가 화면 아래에서 이만큼 위로 올라오기 전까지는 '아래 고정' 유지")]
    public float followOffsetFromBottom = 2f; // 플레이어가 bottom에서 얼마나 올라와야 카메라가 따라가기 시작하는지(월드 단위)
    [Tooltip("항상 아래를 고정하려면 켜세요(플레이어가 올라가도 아래는 고정 X)")]
    public bool strictBottomLock = false;     // true면 항상 아래 고정(플레이 팔로우 최소화)

    [Header("Sky Color")]
    public float SkyMinHeight = 0f;
    public float SkyMaxHeight = 20f;
    public Gradient BackgroundGradient;

    [Header("Pixel Snapping(선택)")]
    public bool snapToPixels = false;
    public float pixelsPerUnit = 64f;

    Camera _cam;
    float _lastAspect = -1f;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
    }

    void Start()
    {
        RecomputeSize(force: true);
        // 시작 시 '아래 고정' 위치로 즉시 정렬
        SetCenterY(BottomAnchorY());
        ApplySky();
    }

    void Update()
    {
        // 종횡비 변화 감지 → 세로 시야 재계산 → 아래선 유지
        float aspect = CurrentAspect();
        if (!Mathf.Approximately(aspect, _lastAspect))
        {
            RecomputeSize(force: true);
            // 해상도 바뀌어도 '아래'는 그대로 보이게 즉시 보정
            SetCenterY(Mathf.Max(transform.position.y, BottomAnchorY()));
        }
    }

    void LateUpdate()
    {
        float centerY = transform.position.y;
        float bottomAnchorY = BottomAnchorY();

        float desiredY;

        if (strictBottomLock || Target == null)
        {
            // 언제나 아래 고정(플레이어가 올라가도 아래선 유지)
            desiredY = bottomAnchorY;
        }
        else
        {
            // Top-only 팔로우:
            // 플레이어가 '아래 + 오프셋' 이하이면 아래 고정 유지,
            // 그 선을 넘어서면 그만큼만 따라감 (아래가 들뜨기 시작)
            float triggerY = bottomAnchorY + followOffsetFromBottom;
            if (Target.position.y <= triggerY)
            {
                desiredY = bottomAnchorY; // 아래 고정
            }
            else
            {
                // 플레이어가 triggerY 위로 올라간 만큼 따라가되, 최소는 bottomAnchor
                float targetCenterY = Target.position.y - followOffsetFromBottom;
                desiredY = Mathf.Max(targetCenterY, bottomAnchorY);
            }
        }

        // 부드럽게 이동(속도 0이면 즉시 스냅)
        float t = (FollowSpeed <= 0f) ? 1f : (FollowSpeed * Time.deltaTime);
        float newY = Mathf.Lerp(centerY, desiredY, t);

        SetCenterY(newY);
        ApplySky();
    }

    // ──────────────────────────────────────────────────────────────────────
    // 크기/앵커 계산

    float CurrentAspect()
    {
        return (float)Screen.width / Mathf.Max(1, Screen.height);
    }

    float BottomAnchorY()
    {
        // 화면 아래선 = centerY - orthoSize
        // → centerY = bottomY + orthoSize  가 되도록 맞춘다
        return bottomY + _cam.orthographicSize;
    }

    void RecomputeSize(bool force = false)
    {
        float aspect = CurrentAspect();
        if (!force && Mathf.Approximately(aspect, _lastAspect)) return;
        _lastAspect = aspect;

        // 가로폭 고정 → 세로 반높이는 (가로폭 / 종횡비)/2
        float halfH = (desiredWorldWidth / Mathf.Max(0.0001f, aspect)) * 0.5f;
        _cam.orthographicSize = Mathf.Max(0.0001f, halfH);
    }

    void SetCenterY(float y)
    {
        Vector3 p = transform.position;
        p.y = y;

        if (snapToPixels && pixelsPerUnit > 0f)
        {
            // 화면 아래선이 월드 픽셀 그리드에 스냅되도록 중심을 스냅
            float unitsPerPixel = 1f / pixelsPerUnit;
            p.y = Mathf.Round(p.y / unitsPerPixel) * unitsPerPixel;
        }

        transform.position = p;
    }

    // ──────────────────────────────────────────────────────────────────────
    // 하늘 그라디언트

    void ApplySky()
    {
        if (BackgroundGradient == null) return;
        float t = Mathf.InverseLerp(SkyMinHeight, SkyMaxHeight, transform.position.y);
        _cam.backgroundColor = BackgroundGradient.Evaluate(t);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (desiredWorldWidth < 0.01f) desiredWorldWidth = 0.01f;
        if (_cam == null) _cam = GetComponent<Camera>();
        if (_cam != null && _cam.orthographic)
        {
            RecomputeSize(force: true);
            SetCenterY(BottomAnchorY());
        }
    }
#endif
}
