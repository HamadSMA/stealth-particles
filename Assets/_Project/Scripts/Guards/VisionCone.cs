using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(GuardController))]
public class VisionCone : MonoBehaviour
{
    private GuardConfig _config;

    [SerializeField]
    [FormerlySerializedAs("wallMask")]
    private LayerMask _wallMask;

    [SerializeField]
    [FormerlySerializedAs("floorMask")]
    private LayerMask _floorMask;

    [SerializeField]
    [FormerlySerializedAs("eyeOrigin")]
    private Transform _eyeOrigin;

    [SerializeField]
    [FormerlySerializedAs("meshResolution")]
    private int _meshResolution = 30;

    [SerializeField]
    [FormerlySerializedAs("coneMeshFilter")]
    private MeshFilter _coneMeshFilter;

    [SerializeField]
    [FormerlySerializedAs("edgeResolveIterations")]
    private int _edgeResolveIterations = 6;

    [SerializeField]
    [FormerlySerializedAs("edgeDistanceThreshold")]
    private float _edgeDistanceThreshold = 0.5f;

    private Mesh _coneMesh;
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
        GuardController controller = GetComponent<GuardController>();

        _config = controller.Config;

        if (_eyeOrigin == null)
        {
            _eyeOrigin = transform;
        }

        if (_config == null || _eyeOrigin == null || _coneMeshFilter == null)
        {
            Debug.LogWarning(
                "VisionCone on '"
                    + name
                    + "' is missing config, eye origin, or cone mesh filter; disabling.",
                this
            );
            enabled = false;
            return;
        }

        _coneMesh = new Mesh { name = "VisionCone" };
        _coneMesh.MarkDynamic();
        _coneMeshFilter.mesh = _coneMesh;

        CacheFloorBounds();
    }

    private void CacheFloorBounds()
    {
        _hasBounds = false;

        LayerMask mask = _floorMask.value != 0 ? _floorMask : LayerMask.GetMask("Walkable");
        if (mask.value == 0)
        {
            return;
        }

        if (
            Physics.Raycast(
                _eyeOrigin.position + Vector3.up * 5f,
                Vector3.down,
                out RaycastHit hit,
                50f,
                mask
            )
        )
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

        Vector3 origin = _eyeOrigin.position;
        Vector3 toPoint = worldPoint - origin;
        toPoint.y = 0f;
        float distance = toPoint.magnitude;

        if (distance > _config.VisionRange)
        {
            return false;
        }

        Vector3 forward = _eyeOrigin.forward;
        forward.y = 0f;

        if (Vector3.Angle(forward, toPoint) > _config.VisionAngle * 0.5f)
        {
            return false;
        }

        if (Physics.Raycast(origin, toPoint.normalized, distance, _wallMask))
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
        public bool Hit;
        public Vector3 Point;
        public float Distance;
        public float Angle;
    }

    private ViewCast Cast(Vector3 origin, Vector3 forward, float angle, float range)
    {
        Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * forward;
        ViewCast result;
        result.Angle = angle;

        if (_hasBounds)
        {
            range = DistanceToFloorEdge(origin, dir, range);
        }

        if (Physics.Raycast(origin, dir, out RaycastHit hit, range, _wallMask))
        {
            result.Hit = true;
            result.Point = hit.point;
            result.Distance = hit.distance;
        }
        else
        {
            result.Hit = false;
            result.Point = origin + dir * range;
            result.Distance = range;
        }

        return result;
    }

    private void ResolveEdge(
        Vector3 origin,
        Vector3 forward,
        float range,
        ViewCast reference,
        ViewCast other,
        out Vector3 referencePoint,
        out Vector3 otherPoint
    )
    {
        float referenceAngle = reference.Angle;
        float otherAngle = other.Angle;
        referencePoint = reference.Point;
        otherPoint = other.Point;

        for (int i = 0; i < _edgeResolveIterations; i++)
        {
            float midAngle = (referenceAngle + otherAngle) * 0.5f;
            ViewCast mid = Cast(origin, forward, midAngle, range);
            bool distanceJump =
                Mathf.Abs(reference.Distance - mid.Distance) > _edgeDistanceThreshold;

            if (mid.Hit == reference.Hit && !distanceJump)
            {
                referenceAngle = midAngle;
                referencePoint = mid.Point;
            }
            else
            {
                otherAngle = midAngle;
                otherPoint = mid.Point;
            }
        }
    }

    private void GenerateConeMesh()
    {
        Vector3 origin = _eyeOrigin.position;
        Vector3 forward = _eyeOrigin.forward;
        forward.y = 0f;
        forward.Normalize();

        float halfAngle = _config.VisionAngle * 0.5f;
        float range = _config.VisionRange;

        _viewPoints.Clear();
        ViewCast previous = default;
        bool hasPrevious = false;

        for (int i = 0; i <= _meshResolution; i++)
        {
            float angle = -halfAngle + _config.VisionAngle * (i / (float)_meshResolution);
            ViewCast current = Cast(origin, forward, angle, range);

            if (hasPrevious)
            {
                bool distanceJump =
                    Mathf.Abs(previous.Distance - current.Distance) > _edgeDistanceThreshold;
                if (previous.Hit != current.Hit || (previous.Hit && current.Hit && distanceJump))
                {
                    ResolveEdge(
                        origin,
                        forward,
                        range,
                        previous,
                        current,
                        out Vector3 previousSide,
                        out Vector3 currentSide
                    );
                    _viewPoints.Add(previousSide);
                    _viewPoints.Add(currentSide);
                }
            }

            _viewPoints.Add(current.Point);
            previous = current;
            hasPrevious = true;
        }

        Transform coneTransform = _coneMeshFilter.transform;
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

        _coneMesh.Clear();
        _coneMesh.SetVertices(_vertices);
        _coneMesh.SetTriangles(_triangles, 0);
        _coneMesh.RecalculateBounds();
    }
}
