using System.Collections.Generic;
using UnityEngine;

public class VisionCone : MonoBehaviour
{
    [SerializeField] private GuardConfig config;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask floorMask;
    [SerializeField] private Transform eyeOrigin;
    [SerializeField] private int meshResolution = 30;
    [SerializeField] private MeshFilter coneMeshFilter;
    [SerializeField] private int edgeResolveIterations = 6;
    [SerializeField] private float edgeDistanceThreshold = 0.5f;

    private Mesh coneMesh;
    private bool _hasBounds;
    private float _minX;
    private float _maxX;
    private float _minZ;
    private float _maxZ;
    private readonly List<Vector3> _viewPoints = new List<Vector3>();
    private readonly List<Vector3> _vertices = new List<Vector3>();
    private readonly List<int> _triangles = new List<int>();

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
        coneMesh.MarkDynamic();
        coneMeshFilter.mesh = coneMesh;

        CacheFloorBounds();
    }

    private void CacheFloorBounds()
    {
        _hasBounds = false;

        LayerMask mask = floorMask.value != 0 ? floorMask : LayerMask.GetMask("Walkable");
        if (mask.value == 0)
        {
            return;
        }

        if (Physics.Raycast(eyeOrigin.position + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 50f, mask))
        {
            Bounds bounds = hit.collider.bounds;
            _minX = bounds.min.x;
            _maxX = bounds.max.x;
            _minZ = bounds.min.z;
            _maxZ = bounds.max.z;
            _hasBounds = true;
        }
    }

    private float DistanceToFloorEdge(Vector3 origin, Vector3 dir, float range)
    {
        float limit = range;

        if (dir.x > 1e-5f)
        {
            limit = Mathf.Min(limit, (_maxX - origin.x) / dir.x);
        }
        else if (dir.x < -1e-5f)
        {
            limit = Mathf.Min(limit, (_minX - origin.x) / dir.x);
        }

        if (dir.z > 1e-5f)
        {
            limit = Mathf.Min(limit, (_maxZ - origin.z) / dir.z);
        }
        else if (dir.z < -1e-5f)
        {
            limit = Mathf.Min(limit, (_minZ - origin.z) / dir.z);
        }

        return Mathf.Max(0f, limit);
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

    private struct ViewCast
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;
    }

    private ViewCast Cast(Vector3 origin, Vector3 forward, float angle, float range)
    {
        Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * forward;
        ViewCast result;
        result.angle = angle;

        if (_hasBounds)
        {
            range = DistanceToFloorEdge(origin, dir, range);
        }

        if (Physics.Raycast(origin, dir, out RaycastHit hit, range, wallMask))
        {
            result.hit = true;
            result.point = hit.point;
            result.distance = hit.distance;
        }
        else
        {
            result.hit = false;
            result.point = origin + dir * range;
            result.distance = range;
        }

        return result;
    }

    private void ResolveEdge(Vector3 origin, Vector3 forward, float range, ViewCast reference, ViewCast other, out Vector3 referencePoint, out Vector3 otherPoint)
    {
        float referenceAngle = reference.angle;
        float otherAngle = other.angle;
        referencePoint = reference.point;
        otherPoint = other.point;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float midAngle = (referenceAngle + otherAngle) * 0.5f;
            ViewCast mid = Cast(origin, forward, midAngle, range);
            bool distanceJump = Mathf.Abs(reference.distance - mid.distance) > edgeDistanceThreshold;

            if (mid.hit == reference.hit && !distanceJump)
            {
                referenceAngle = midAngle;
                referencePoint = mid.point;
            }
            else
            {
                otherAngle = midAngle;
                otherPoint = mid.point;
            }
        }
    }

    private void GenerateConeMesh()
    {
        Vector3 origin = eyeOrigin.position;
        Vector3 forward = eyeOrigin.forward;
        forward.y = 0f;
        forward.Normalize();

        float halfAngle = config.visionAngle * 0.5f;
        float range = config.visionRange;

        _viewPoints.Clear();
        ViewCast previous = default;
        bool hasPrevious = false;

        for (int i = 0; i <= meshResolution; i++)
        {
            float angle = -halfAngle + config.visionAngle * (i / (float)meshResolution);
            ViewCast current = Cast(origin, forward, angle, range);

            if (hasPrevious)
            {
                bool distanceJump = Mathf.Abs(previous.distance - current.distance) > edgeDistanceThreshold;
                if (previous.hit != current.hit || (previous.hit && current.hit && distanceJump))
                {
                    ResolveEdge(origin, forward, range, previous, current, out Vector3 previousSide, out Vector3 currentSide);
                    _viewPoints.Add(previousSide);
                    _viewPoints.Add(currentSide);
                }
            }

            _viewPoints.Add(current.point);
            previous = current;
            hasPrevious = true;
        }

        Transform coneTransform = coneMeshFilter.transform;
        _vertices.Clear();
        _triangles.Clear();

        Vector3 apexLocal = coneTransform.InverseTransformPoint(origin);
        apexLocal.y = 0f;
        _vertices.Add(apexLocal);

        for (int i = 0; i < _viewPoints.Count; i++)
        {
            Vector3 local = coneTransform.InverseTransformPoint(_viewPoints[i]);
            local.y = 0f;
            _vertices.Add(local);

            if (i < _viewPoints.Count - 1)
            {
                _triangles.Add(0);
                _triangles.Add(i + 1);
                _triangles.Add(i + 2);
            }
        }

        coneMesh.Clear();
        coneMesh.SetVertices(_vertices);
        coneMesh.SetTriangles(_triangles, 0);
        coneMesh.RecalculateBounds();
    }
}
