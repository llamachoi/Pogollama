using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public float followSpeed = 5f;
    public float minY = 0f;

    public float minHeight = 0f;
    public float maxHeight = 20f;

    public Gradient backgroundGradient;

    void LateUpdate()
    {
        Vector3 cameraPos = transform.position;
        float targetY = Mathf.Max(target.position.y, minY);
        cameraPos.y = Mathf.Lerp(cameraPos.y, targetY, followSpeed * Time.deltaTime);
        transform.position = cameraPos;
        ChangeSkyColor();
    }

    void ChangeSkyColor()
    {
        float height = transform.position.y;

        float t = Mathf.InverseLerp(minHeight, maxHeight, height);
        GetComponent<Camera>().backgroundColor = backgroundGradient.Evaluate(t);
    }
}

