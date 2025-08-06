using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float fallSpeed = 5f;

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Game Over");
            Time.timeScale = 0;
        }
    }

    private void OnBecameInvisible()
    {
        // 오브젝트가 화면 밖으로 나가면 비활성화
        gameObject.SetActive(false);
    }
}
