using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform Target;
    public float FollowSpeed = 5f;
    public float CameraMinHeight = 0f;

    public float SkyMinHeight = 0f;
    public float SkyMaxHeight = 20f;

    public Gradient BackgroundGradient;

    void LateUpdate()
    {
        Vector3 cameraPos = transform.position;
        float targetY = Mathf.Max(Target.position.y, CameraMinHeight);
        cameraPos.y = Mathf.Lerp(cameraPos.y, targetY, FollowSpeed * Time.deltaTime);
        transform.position = cameraPos;
        ChangeSkyColor();
    }

    void ChangeSkyColor()
    {
        float height = transform.position.y;

        float t = Mathf.InverseLerp(SkyMinHeight, SkyMaxHeight, height);
        GetComponent<Camera>().backgroundColor = BackgroundGradient.Evaluate(t);
    }
}

