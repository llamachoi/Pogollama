using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class CameraBottomAnchor : MonoBehaviour
{
    [Header("Width Fit")]
    [Min(0.01f)] public float desiredWorldWidth = 16f; // 항상 유지할 월드 단위 '가로 폭'
    [Min(0f)] public float extraTopPadding = 0f;       // 위로 더 보이는 여유(월드 단위)

    [Header("Optional: Bottom Lock via CameraMovement")]
    public bool syncBottomToMinHeight = false;  // 켜면 화면 아래가 bottomY로 고정되도록 CameraMinHeight를 자동 조정
    public float bottomY = 0f;                  // 화면 아래가 가리킬 월드 Y
    public CameraMovement cameraMovement;       // 네가 쓰는 기존 스크립트 참조

    Camera _cam;
    float _lastAspect = -1f;


    void OnEnable()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
        RecomputeSize(force: true);
    }

    void OnValidate()
    {
        if (_cam == null) _cam = GetComponent<Camera>();
        if (_cam != null) _cam.orthographic = true;
        RecomputeSize(force: true);
    }

    void Start()
    {
        RecomputeSize(force: true);

        if (syncBottomToMinHeight && cameraMovement != null)
            cameraMovement.CameraMinHeight = bottomY + _cam.orthographicSize;

        // CameraMovement의 보간을 우회하고 즉시 위치 반영
        var p = transform.position;
        float targetY = Mathf.Max(
            cameraMovement != null && cameraMovement.Target ? cameraMovement.Target.position.y : p.y,
            cameraMovement != null ? cameraMovement.CameraMinHeight : (bottomY + _cam.orthographicSize)
        );
        transform.position = new Vector3(p.x, targetY, p.z);
    }

    void Update()
    {
        if (_cam == null) return;

        float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        if (!Mathf.Approximately(aspect, _lastAspect))
        {
            RecomputeSize(force: true);
        }

        if (syncBottomToMinHeight && cameraMovement != null)
        {
            // 화면 아래=bottomY 고정이 되도록, 카메라 중심의 최소 Y를 갱신
            // (실제 이동은 CameraMovement가 담당)
            cameraMovement.CameraMinHeight = bottomY + _cam.orthographicSize;
        }
    }

    void RecomputeSize(bool force = false)
    {
        float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        if (!force && Mathf.Approximately(aspect, _lastAspect)) return;

        _lastAspect = aspect;

        // 가로폭을 고정 -> 세로 반높이(orthographicSize) = (가로폭 / 종횡비) * 0.5 + 여유의 절반
        float halfHeightFromWidth = (desiredWorldWidth / aspect) * 0.5f;
        float halfPadding = extraTopPadding * 0.5f;

        _cam.orthographicSize = halfHeightFromWidth + halfPadding;
    }
}
