using UnityEngine;

public class VisionCone : MonoBehaviour
{
    [SerializeField] private GuardConfig config;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private Transform eyeOrigin;
    [SerializeField] private int meshResolution = 30;
    [SerializeField] private MeshFilter coneMeshFilter;

    private Mesh coneMesh;

    private void Awake()
    {
        if (config == null)
        {
            GuardController controller = GetComponent<GuardController>();
            if (controller != null)
            {
                config = controller.Config;
            }
        }

        if (eyeOrigin == null)
        {
            eyeOrigin = transform;
        }

        if (config == null || eyeOrigin == null || coneMeshFilter == null)
        {
            Debug.LogWarning("VisionCone on '" + name + "' is missing config, eyeOrigin, or coneMeshFilter; disabling.", this);
            enabled = false;
            return;
        }

        coneMesh = new Mesh { name = "VisionCone" };
        coneMeshFilter.mesh = coneMesh;
    }

    public bool ContainsPoint(Vector3 worldPoint, out bool blockedByWall)
    {
        blockedByWall = false;

        Vector3 origin = eyeOrigin.position;
        Vector3 toPoint = worldPoint - origin;
        toPoint.y = 0f;
        float distance = toPoint.magnitude;

        if (distance > config.visionRange)
        {
            return false;
        }

        Vector3 forward = eyeOrigin.forward;
        forward.y = 0f;

        if (Vector3.Angle(forward, toPoint) > config.visionAngle * 0.5f)
        {
            return false;
        }

        if (Physics.Raycast(origin, toPoint.normalized, distance, wallMask))
        {
            blockedByWall = true;
            return false;
        }

        return true;
    }

    private void Update()
    {
        GenerateConeMesh();
    }

    private void GenerateConeMesh()
    {
        Vector3 origin = eyeOrigin.position;
        Vector3 forward = eyeOrigin.forward;
        forward.y = 0f;
        forward.Normalize();

        Transform coneTransform = coneMeshFilter.transform;
        float halfAngle = config.visionAngle * 0.5f;

        Vector3[] vertices = new Vector3[meshResolution + 2];
        int[] triangles = new int[meshResolution * 3];

        Vector3 apexLocal = coneTransform.InverseTransformPoint(origin);
        apexLocal.y = 0f;
        vertices[0] = apexLocal;

        for (int i = 0; i <= meshResolution; i++)
        {
            float angle = -halfAngle + config.visionAngle * (i / (float)meshResolution);
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * forward;

            Vector3 endpoint;
            if (Physics.Raycast(origin, dir, out RaycastHit hit, config.visionRange, wallMask))
            {
                endpoint = hit.point;
            }
            else
            {
                endpoint = origin + dir * config.visionRange;
            }

            Vector3 local = coneTransform.InverseTransformPoint(endpoint);
            local.y = 0f;
            vertices[i + 1] = local;
        }

        for (int i = 0; i < meshResolution; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        coneMesh.Clear();
        coneMesh.vertices = vertices;
        coneMesh.triangles = triangles;
        coneMesh.RecalculateNormals();
        coneMesh.RecalculateBounds();
    }
}
