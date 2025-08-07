using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Game Over");
            GameManager.Instance.GameOver();
        }
    }
}
