using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    public static Spawner Instance { get; private set; }

    public GameObject obstaclePrefab;
    public float spawnInterval = 1f;
    public float spawnRangeX = 8.5f;
    public int poolSize = 10;

    private List<GameObject> pool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        pool = new List<GameObject>();
        InitializePool();
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(obstaclePrefab);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }

        // 풀 초과 시 새로 생성 (선택 사항)
        GameObject extra = Instantiate(obstaclePrefab);
        extra.SetActive(false);
        pool.Add(extra);
        return extra;
    }

    public void SpawnFromPool()
    {
        GameObject obj = GetPooledObject();
        if (obj != null)
        {
            float randomX = Random.Range(-spawnRangeX, spawnRangeX);
            Vector3 spawnPos = new Vector3(randomX, transform.position.y, 0f);
            obj.transform.position = spawnPos;
            obj.SetActive(true);
        }
    }

    private void Start()
    {
        InvokeRepeating(nameof(SpawnFromPool), 1f, spawnInterval);
    }
}
