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
        [Tooltip("이 높이 구간에서 사용할 프리팹들")]
        public GameObject[] prefabs;
        [Tooltip("이 구간에서 프리팹을 몇 개까지 활성 유지할지(0은 제한 없음)")]
        public int maxActive = 0;
    }

    [Header("References")]
    public Transform player;          // 기준이 될 플레이어
    public Camera targetCamera;       // 비었으면 MainCamera
    public Transform container;       // 생성물 부모(비면 이 오브젝트 밑)

    [Header("Spawn Rules")]
    [Tooltip("플레이어 X를 기준으로 좌우 범위")]
    public float horizontalRange = 30f;
    [Tooltip("플레이어 위로 이만큼까지 미리 생성")]
    public float verticalLookAhead = 60f;
    [Tooltip("이 간격마다 한 번씩 생성 밴드 진행")]
    public float verticalStep = 12f;
    [Tooltip("각 스텝(밴드)마다 생성할 개수")]
    public int spawnPerStep = 2;
    [Tooltip("생성물 Z(2D면 -10~-1 같은 뒤쪽 고정 값)")]
    public float spawnZ = 0f;

    [Header("Height Tiers")]
    public HeightTier[] tiers;

    [Header("Culling (화면 밖 비활성화)")]
    [Tooltip("프레임당 몇 개의 위치만 갱신할지(0=매프레임 전부)")]
    public int positionsUpdateBudgetPerFrame = 0; // 0이면 모두 갱신
    [Tooltip("Culling 반경에 더하는 여유(렌더러 경계보다 살짝 크게)")]
    public float cullingRadiusPadding = 0.5f;

    // ──────────────────────────────────────────────────────────────────────
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
    int _rollingIndex; // 포지션 갱신용 라운드로빈

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

        // 1) 위쪽으로 스폰 진행
        float targetTopY = player.position.y + verticalLookAhead;
        while (_spawnedUpToY + verticalStep <= targetTopY)
        {
            _spawnedUpToY += verticalStep;
            SpawnBandAt(_spawnedUpToY);
        }

        // 2) Culling 좌표 갱신(라운드로빈: CPU 아끼기)
        UpdateCullingPositions();

        // 3) 너무 아래로 내려간(플레이어 아래 먼 곳) 오브젝트 정리(선택)
        //    필요 시 아래 줄 주석 해제 및 임계값 조정
        // DespawnFarBelow(player.position.y - 80f);
    }

    // ──────────────────────────────────────────────────────────────────────
    #region Spawning

    void SpawnBandAt(float y)
    {
        var tier = TierForY(y);
        if (tier == null || tier.prefabs == null || tier.prefabs.Length == 0) return;

        // 최대 활성 제한
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
    // ──────────────────────────────────────────────────────────────────────
    #region CullingGroup

    void EnsureCullingGroup()
    {
        if (_cg != null) return;
        _cg = new CullingGroup();
        _cg.onStateChanged += OnStateChanged;
        _cg.targetCamera = targetCamera;
        RebuildCullingSpheres(); // 초기화
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

        // 경계로 반경 계산(모든 렌더러 합산)
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

        // 렌더러 외 나머지 Behaviour들을 토글 대상으로(Transform/이 스크립트 제외)
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

        // 초기에는 화면 밖일 수 있으니, 안전하게 한 번 상태 반영
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
        // 간단한 현재 가시성 추정(뷰포트)
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
    // ──────────────────────────────────────────────────────────────────────
    #region Optional Despawn

    void DespawnFarBelow(float thresholdY)
    {
        // thresholdY 보다 훨씬 아래의 항목을 파괴(또는 풀에 반납)
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
