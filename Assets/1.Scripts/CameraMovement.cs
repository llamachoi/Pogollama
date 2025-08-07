using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public float followSpeed = 5f;
    public float minY = 0f;

    void LateUpdate()
    {
        Vector3 cameraPos = transform.position;
        float targetY = Mathf.Max(target.position.y, minY);
        cameraPos.y = Mathf.Lerp(cameraPos.y, targetY, followSpeed * Time.deltaTime);
        transform.position = cameraPos;
    }
}
