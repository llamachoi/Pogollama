using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; // optional

[DisallowMultipleComponent]
public class CullingManager : MonoBehaviour
{
    [System.Serializable]
    public class HeightTier
    {
        public string name;
        public float minY = 0f;
        public float maxY = 100f;
        [Tooltip("�� ���� �������� ����� �����յ�")]
        public GameObject[] prefabs;
        [Tooltip("�� �������� �������� �� ������ Ȱ�� ��������(0�� ���� ����)")]
        public int maxActive = 0;
    }

    [Header("References")]
    public Transform player;          // ������ �� �÷��̾�
    public Camera targetCamera;       // ������� MainCamera
    public Transform container;       // ������ �θ�(��� �� ������Ʈ ��)

    [Header("Spawn Rules")]
    [Tooltip("�÷��̾� X�� �������� �¿� ����")]
    public float horizontalRange = 30f;
    [Tooltip("�÷��̾� ���� �̸�ŭ���� �̸� ����")]
    public float verticalLookAhead = 60f;
    [Tooltip("�� ���ݸ��� �� ���� ���� ��� ����")]
    public float verticalStep = 12f;
    [Tooltip("�� ����(���)���� ������ ����")]
    public int spawnPerStep = 2;
    [Tooltip("������ Z(2D�� -10~-1 ���� ���� ���� ��)")]
    public float spawnZ = 0f;

    [Header("Height Tiers")]
    public HeightTier[] tiers;

    [Header("Culling (ȭ�� �� ��Ȱ��ȭ)")]
    [Tooltip("�����Ӵ� �� ���� ��ġ�� ��������(0=�������� ����)")]
    public int positionsUpdateBudgetPerFrame = 0; // 0�̸� ��� ����
    [Tooltip("Culling �ݰ濡 ���ϴ� ����(������ ��躸�� ��¦ ũ��)")]
    public float cullingRadiusPadding = 0.5f;

    // ��������������������������������������������������������������������������������������������������������������������������������������������
    class Entry
    {
        public GameObject go;
        public Transform tr;
        public Renderer[] renderers;
        public Behaviour[] behavioursToToggle;
        public float radius;
    }

    readonly List<Entry> _entries = new List<Entry>();
    CullingGroup _cg;
    BoundingSphere[] _spheres;
    float _spawnedUpToY;
    int _rollingIndex; // ������ ���ſ� ����κ�

    void Awake()
    {
        if (!targetCamera) targetCamera = Camera.main;
        if (!container) container = transform;
        _spawnedUpToY = (player ? player.position.y : 0f);
    }

    void OnEnable()
    {
        EnsureCullingGroup();
    }

    void OnDisable()
    {
        DisposeCulling();
    }

    void Update()
    {
        if (!player || tiers == null || tiers.Length == 0) return;

        // 1) �������� ���� ����
        float targetTopY = player.position.y + verticalLookAhead;
        while (_spawnedUpToY + verticalStep <= targetTopY)
        {
            _spawnedUpToY += verticalStep;
            SpawnBandAt(_spawnedUpToY);
        }

        // 2) Culling ��ǥ ����(����κ�: CPU �Ƴ���)
        UpdateCullingPositions();

        // 3) �ʹ� �Ʒ��� ������(�÷��̾� �Ʒ� �� ��) ������Ʈ ����(����)
        //    �ʿ� �� �Ʒ� �� �ּ� ���� �� �Ӱ谪 ����
        // DespawnFarBelow(player.position.y - 80f);
    }

    // ��������������������������������������������������������������������������������������������������������������������������������������������
    #region Spawning

    void SpawnBandAt(float y)
    {
        var tier = TierForY(y);
        if (tier == null || tier.prefabs == null || tier.prefabs.Length == 0) return;

        // �ִ� Ȱ�� ����
        if (tier.maxActive > 0 && CountActiveInTier(tier) >= tier.maxActive) return;

        for (int i = 0; i < spawnPerStep; i++)
        {
            var prefab = tier.prefabs[Random.Range(0, tier.prefabs.Length)];
            if (!prefab) continue;

            float x = (player ? player.position.x : 0f) + Random.Range(-horizontalRange, horizontalRange);
            var pos = new Vector3(x, y, spawnZ);

            var go = Instantiate(prefab, pos, Quaternion.identity, container);
            RegisterForCulling(go);
        }
    }

    HeightTier TierForY(float y)
    {
        for (int i = 0; i < tiers.Length; i++)
        {
            if (y >= tiers[i].minY && y <= tiers[i].maxY) return tiers[i];
        }
        return null;
    }

    int CountActiveInTier(HeightTier tier)
    {
        int cnt = 0;
        for (int i = 0; i < _entries.Count; i++)
        {
            var e = _entries[i];
            if (!e.go) continue;
            float y = e.tr.position.y;
            if (y >= tier.minY && y <= tier.maxY) cnt++;
        }
        return cnt;
    }

    #endregion
    // ��������������������������������������������������������������������������������������������������������������������������������������������
    #region CullingGroup

    void EnsureCullingGroup()
    {
        if (_cg != null) return;
        _cg = new CullingGroup();
        _cg.onStateChanged += OnStateChanged;
        _cg.targetCamera = targetCamera;
        RebuildCullingSpheres(); // �ʱ�ȭ
    }

    void DisposeCulling()
    {
        if (_cg != null)
        {
            _cg.onStateChanged -= OnStateChanged;
            _cg.Dispose();
            _cg = null;
        }
        _spheres = null;
        _entries.Clear();
    }

    void RegisterForCulling(GameObject go)
    {
        var e = new Entry();
        e.go = go;
        e.tr = go.transform;
        e.renderers = go.GetComponentsInChildren<Renderer>(true);

        // ���� �ݰ� ���(��� ������ �ջ�)
        Bounds? merged = null;
        for (int i = 0; i < e.renderers.Length; i++)
        {
            var r = e.renderers[i];
            if (!r) continue;
            if (merged == null) merged = r.bounds;
            else merged = Encapsulate(merged.Value, r.bounds);
        }
        float radius = 1f;
        if (merged != null)
        {
            var b = merged.Value.extents;
            radius = Mathf.Max(b.x, b.y, b.z) + cullingRadiusPadding;
        }
        e.radius = Mathf.Max(0.1f, radius);

        // ������ �� ������ Behaviour���� ��� �������(Transform/�� ��ũ��Ʈ ����)
        List<Behaviour> toggles = new List<Behaviour>();
        var behaviours = go.GetComponentsInChildren<Behaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            var beh = behaviours[i];
            if (!beh) continue;
            if (beh is Renderer) continue;
            if (beh is CullingManager) continue;
            toggles.Add(beh);
        }
        e.behavioursToToggle = toggles.ToArray();

        _entries.Add(e);
        RebuildCullingSpheres();

        // �ʱ⿡�� ȭ�� ���� �� ������, �����ϰ� �� �� ���� �ݿ�
        SetVisible(e, IsVisibleNow(e));
    }

    void RebuildCullingSpheres()
    {
        if (_cg == null) return;

        int n = _entries.Count;
        _spheres = new BoundingSphere[n];
        for (int i = 0; i < n; i++)
        {
            var e = _entries[i];
            if (e == null || e.tr == null) continue;
            _spheres[i] = new BoundingSphere(e.tr.position, e.radius);
        }
        _cg.SetBoundingSpheres(_spheres);
        _cg.SetBoundingSphereCount(n);
    }

    void UpdateCullingPositions()
    {
        if (_cg == null || _spheres == null) return;

        int n = _entries.Count;
        if (n == 0) return;

        int budget = positionsUpdateBudgetPerFrame <= 0 ? n : Mathf.Min(positionsUpdateBudgetPerFrame, n);
        for (int i = 0; i < budget; i++)
        {
            if (_rollingIndex >= n) _rollingIndex = 0;
            var e = _entries[_rollingIndex];
            if (e != null && e.tr != null)
                _spheres[_rollingIndex].position = e.tr.position;
            _rollingIndex++;
        }
    }

    void OnStateChanged(CullingGroupEvent ev)
    {
        if (ev.index < 0 || ev.index >= _entries.Count) return;
        var e = _entries[ev.index];
        if (e == null) return;

        SetVisible(e, ev.isVisible);
    }

    void SetVisible(Entry e, bool visible)
    {
        if (e.renderers != null)
        {
            for (int i = 0; i < e.renderers.Length; i++)
                if (e.renderers[i]) e.renderers[i].enabled = visible;
        }
        if (e.behavioursToToggle != null)
        {
            for (int i = 0; i < e.behavioursToToggle.Length; i++)
                if (e.behavioursToToggle[i]) e.behavioursToToggle[i].enabled = visible;
        }
    }

    bool IsVisibleNow(Entry e)
    {
        if (!targetCamera || e == null || e.tr == null) return false;
        // ������ ���� ���ü� ����(����Ʈ)
        Vector3 v = targetCamera.WorldToViewportPoint(e.tr.position);
        return v.z > 0f && v.x > -0.05f && v.x < 1.05f && v.y > -0.05f && v.y < 1.05f;
    }

    static Bounds Encapsulate(Bounds a, Bounds b)
    {
        a.Encapsulate(b.min);
        a.Encapsulate(b.max);
        return a;
    }

    #endregion
    // ��������������������������������������������������������������������������������������������������������������������������������������������
    #region Optional Despawn

    void DespawnFarBelow(float thresholdY)
    {
        // thresholdY ���� �ξ� �Ʒ��� �׸��� �ı�(�Ǵ� Ǯ�� �ݳ�)
        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            var e = _entries[i];
            if (!e.go) { _entries.RemoveAt(i); continue; }
            if (e.tr.position.y < thresholdY)
            {
                Destroy(e.go);
                _entries.RemoveAt(i);
            }
        }
        RebuildCullingSpheres();
    }

    #endregion
}
