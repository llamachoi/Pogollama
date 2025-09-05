using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    public PlatformData PlatformData;

    private GameObject[] platformPrefabs;
    private GameObject rainbowPlatformPrefab;

    [Header("Local Spawn Range (로컬 좌표)")]
    public Vector2 xRangeLocal = new Vector2(-2f, 2f);
    public Vector2 yRangeLocal = new Vector2(0f, 15f);

    [Header("Spacing / Snap")]
    public float xStep = 0.5f;
    public float minYGap = 2;
    public float maxYGap = 4;
    public float yStep = 0.5f;

    [Header("Horizontal Gap")]
    [Tooltip("플랫폼 간 수평 최소 간격 (X축 기준)")]
    public float minXDistance = 1.0f;

    [Header("Utilities")]
    public bool clearChildrenBeforeGenerate = true;

    private bool hasGenerated;
    private List<Vector2> spawnedPositions = new List<Vector2>(); // 생성 좌표 기록

    private void Awake()
    {
        platformPrefabs = PlatformData.SpawnPrefabs.ToArray();
        rainbowPlatformPrefab = PlatformData.RainbowSpawnPrefabs;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasGenerated)
        {
            GeneratePlatforms();
            hasGenerated = true;
        }
    }

    public void GeneratePlatforms()
    {
        if (rainbowPlatformPrefab == null)
        {
            Debug.LogWarning("[PlatformSpawner] rainbowPlatformPrefab이 비어 있습니다.");
            return;
        }

        if (clearChildrenBeforeGenerate)
            ClearChildren();

        spawnedPositions.Clear();

        float yMin = yRangeLocal.x;
        float yMax = yRangeLocal.y;
        float currentY = Snap(yMin, yStep);

        while (true)
        {
            float gap = Random.Range(minYGap, maxYGap);  // max 포함
            float nextY = currentY + gap;
            if (nextY >= yMax) break;

            float spawnY = Snap(nextY, yStep);
            float spawnX = PickXWithDistance();

            SpawnOne(RandomPick(platformPrefabs), new Vector2(spawnX, spawnY));
            spawnedPositions.Add(new Vector2(spawnX, spawnY));

            currentY = spawnY;
        }

        // 마지막: 최대 높이에 레인보우 고정 생성
        float rainbowX = PickXWithDistance();
        float rainbowY = Snap(yMax, yStep);
        SpawnOne(rainbowPlatformPrefab, new Vector2(0, rainbowY));
        spawnedPositions.Add(new Vector2(rainbowX, rainbowY));
    }

    private float PickXWithDistance()
    {
        int maxTries = 30;
        for (int i = 0; i < maxTries; i++)
        {
            float candidate = Snap(Random.Range(xRangeLocal.x, xRangeLocal.y), xStep);
            bool valid = true;

            foreach (var pos in spawnedPositions)
            {
                if (Mathf.Abs(candidate - pos.x) < minXDistance)
                {
                    valid = false;
                    break;
                }
            }

            if (valid) return candidate;
        }
        // 실패 시 그냥 랜덤값 반환 (무한루프 방지)
        return Snap(Random.Range(xRangeLocal.x, xRangeLocal.y), xStep);
    }

    private void SpawnOne(GameObject prefab, Vector2 localPos)
    {
        if (prefab == null) return;
        var go = Instantiate(prefab, this.transform);
        go.transform.localPosition = new Vector3(localPos.x, localPos.y, 0f);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
    }

    private float Snap(float value, float step)
    {
        if (step <= 0f) return value;
        return Mathf.Round(value / step) * step;
    }

    private GameObject RandomPick(GameObject[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        int idx = Random.Range(0, arr.Length);
        return arr[idx];
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(transform.GetChild(i).gameObject);
            else
                Destroy(transform.GetChild(i).gameObject);
#else
            Destroy(transform.GetChild(i).gameObject);
#endif
        }
    }
}
