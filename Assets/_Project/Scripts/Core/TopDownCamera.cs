using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float cameraHeight = 18f;
    [SerializeField] private Vector3 followOffset = Vector3.zero;
    [SerializeField] private float smoothTime = 0.25f;
    [SerializeField] private float cameraAngle = 90f;

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + Vector3.up * cameraHeight + followOffset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        transform.rotation = Quaternion.Euler(cameraAngle, 0f, 0f);
    }

    public void ApplyLevelConfig(float height, Vector3 offset)
    {
        cameraHeight = height;
        followOffset = offset;
    }
}
