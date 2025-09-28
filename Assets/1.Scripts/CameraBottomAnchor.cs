using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class CameraBottomAnchor : MonoBehaviour
{
    [Header("Width Fit")]
    [Min(0.01f)] public float desiredWorldWidth = 16f; // �׻� ������ ���� ���� '���� ��'
    [Min(0f)] public float extraTopPadding = 0f;       // ���� �� ���̴� ����(���� ����)

    [Header("Optional: Bottom Lock via CameraMovement")]
    public bool syncBottomToMinHeight = false;  // �Ѹ� ȭ�� �Ʒ��� bottomY�� �����ǵ��� CameraMinHeight�� �ڵ� ����
    public float bottomY = 0f;                  // ȭ�� �Ʒ��� ����ų ���� Y
    public CameraMovement cameraMovement;       // �װ� ���� ���� ��ũ��Ʈ ����

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

        // CameraMovement�� ������ ��ȸ�ϰ� ��� ��ġ �ݿ�
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
            // ȭ�� �Ʒ�=bottomY ������ �ǵ���, ī�޶� �߽��� �ּ� Y�� ����
            // (���� �̵��� CameraMovement�� ���)
            cameraMovement.CameraMinHeight = bottomY + _cam.orthographicSize;
        }
    }

    void RecomputeSize(bool force = false)
    {
        float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        if (!force && Mathf.Approximately(aspect, _lastAspect)) return;

        _lastAspect = aspect;

        // �������� ���� -> ���� �ݳ���(orthographicSize) = (������ / ��Ⱦ��) * 0.5 + ������ ����
        float halfHeightFromWidth = (desiredWorldWidth / aspect) * 0.5f;
        float halfPadding = extraTopPadding * 0.5f;

        _cam.orthographicSize = halfHeightFromWidth + halfPadding;
    }
}
