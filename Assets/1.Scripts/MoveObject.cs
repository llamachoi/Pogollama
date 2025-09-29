using UnityEngine;

[RequireComponent(typeof(Renderer))] // ���ü� �ݹ� ����
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

    // ��������������������������������������������������������������������������������������
    [Header("Offscreen Optimization")]
    [Tooltip("ȭ�� ���� �� ������/�ùķ��̼��� �Ͻ�����")]
    public bool pauseSimulationWhenOffscreen = true;
    [Tooltip("ȭ�� ���� �� ���� �߰� ������Ʈ(Animator ��)")]
    public Behaviour[] extraDisableWhenOffscreen;

    private float verticalPingPongTimer = 0f;
    private float initialLocalY; // Store the initial local Y position

    private float adjustedHorizontalSpeed;
    private float adjustedVerticalSpeed;

    private float currentVerticalPosition;

    // ���ü�/�ùķ��̼� Ÿ�̹�
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

        // �ùķ��̼� �ð� ����
        _lastSimTime = Time.time;

        // �ʱ� ���ü� ���� �� ���� ����
        TryInitialVisibilitySync();
    }

    void Update()
    {
        // ȭ�鿡 ������ �ʰ�, ������ũ�� �Ͻ������� ���� ������ ��� ����
        if (!_isVisible && pauseSimulationWhenOffscreen)
        {
            _lastSimTime = Time.time; // ���� �� ū dt ����
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

    // ��������������������������������������������������������������������������������������
    // Visibility callbacks (Renderer �ʿ�)

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

    // �߰� ������Ʈ ���(�� ��ũ��Ʈ/Renderer�� ����)
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

    // �ʱ� ���ü� ����(�� ���� �����ӿ� Invisible �ݹ��� �ٷ� �� �� �� �����Ƿ�)
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
