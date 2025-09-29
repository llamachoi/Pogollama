using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraMovement : MonoBehaviour
{
    [Header("Follow")]
    public Transform Target;
    [Min(0f)] public float FollowSpeed = 5f;

    [Header("Bottom Anchor (����)")]
    [Min(0.01f)] public float desiredWorldWidth = 16f; // �׻� ������ ���� ������
    public float bottomY = 0f;                         // ȭ�� '�Ʒ�'�� ����ų ���� Y (�� ���� ��)

    [Header("Follow Logic")]
    [Tooltip("�÷��̾ ȭ�� �Ʒ����� �̸�ŭ ���� �ö���� �������� '�Ʒ� ����' ����")]
    public float followOffsetFromBottom = 2f; // �÷��̾ bottom���� �󸶳� �ö�;� ī�޶� ���󰡱� �����ϴ���(���� ����)
    [Tooltip("�׻� �Ʒ��� �����Ϸ��� �Ѽ���(�÷��̾ �ö󰡵� �Ʒ��� ���� X)")]
    public bool strictBottomLock = false;     // true�� �׻� �Ʒ� ����(�÷��� �ȷο� �ּ�ȭ)

    [Header("Sky Color")]
    public float SkyMinHeight = 0f;
    public float SkyMaxHeight = 20f;
    public Gradient BackgroundGradient;

    [Header("Pixel Snapping(����)")]
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
        // ���� �� '�Ʒ� ����' ��ġ�� ��� ����
        SetCenterY(BottomAnchorY());
        ApplySky();
    }

    void Update()
    {
        // ��Ⱦ�� ��ȭ ���� �� ���� �þ� ���� �� �Ʒ��� ����
        float aspect = CurrentAspect();
        if (!Mathf.Approximately(aspect, _lastAspect))
        {
            RecomputeSize(force: true);
            // �ػ� �ٲ� '�Ʒ�'�� �״�� ���̰� ��� ����
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
            // ������ �Ʒ� ����(�÷��̾ �ö󰡵� �Ʒ��� ����)
            desiredY = bottomAnchorY;
        }
        else
        {
            // Top-only �ȷο�:
            // �÷��̾ '�Ʒ� + ������' �����̸� �Ʒ� ���� ����,
            // �� ���� �Ѿ�� �׸�ŭ�� ���� (�Ʒ��� ��߱� ����)
            float triggerY = bottomAnchorY + followOffsetFromBottom;
            if (Target.position.y <= triggerY)
            {
                desiredY = bottomAnchorY; // �Ʒ� ����
            }
            else
            {
                // �÷��̾ triggerY ���� �ö� ��ŭ ���󰡵�, �ּҴ� bottomAnchor
                float targetCenterY = Target.position.y - followOffsetFromBottom;
                desiredY = Mathf.Max(targetCenterY, bottomAnchorY);
            }
        }

        // �ε巴�� �̵�(�ӵ� 0�̸� ��� ����)
        float t = (FollowSpeed <= 0f) ? 1f : (FollowSpeed * Time.deltaTime);
        float newY = Mathf.Lerp(centerY, desiredY, t);

        SetCenterY(newY);
        ApplySky();
    }

    // ��������������������������������������������������������������������������������������������������������������������������������������������
    // ũ��/��Ŀ ���

    float CurrentAspect()
    {
        return (float)Screen.width / Mathf.Max(1, Screen.height);
    }

    float BottomAnchorY()
    {
        // ȭ�� �Ʒ��� = centerY - orthoSize
        // �� centerY = bottomY + orthoSize  �� �ǵ��� �����
        return bottomY + _cam.orthographicSize;
    }

    void RecomputeSize(bool force = false)
    {
        float aspect = CurrentAspect();
        if (!force && Mathf.Approximately(aspect, _lastAspect)) return;
        _lastAspect = aspect;

        // ������ ���� �� ���� �ݳ��̴� (������ / ��Ⱦ��)/2
        float halfH = (desiredWorldWidth / Mathf.Max(0.0001f, aspect)) * 0.5f;
        _cam.orthographicSize = Mathf.Max(0.0001f, halfH);
    }

    void SetCenterY(float y)
    {
        Vector3 p = transform.position;
        p.y = y;

        if (snapToPixels && pixelsPerUnit > 0f)
        {
            // ȭ�� �Ʒ����� ���� �ȼ� �׸��忡 �����ǵ��� �߽��� ����
            float unitsPerPixel = 1f / pixelsPerUnit;
            p.y = Mathf.Round(p.y / unitsPerPixel) * unitsPerPixel;
        }

        transform.position = p;
    }

    // ��������������������������������������������������������������������������������������������������������������������������������������������
    // �ϴ� �׶���Ʈ

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
