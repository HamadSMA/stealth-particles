using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

[RequireComponent(typeof(ParticleSystem))]
public class PlayerTrailEmitter : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("agent")]
    private NavMeshAgent _agent;

    [SerializeField]
    [FormerlySerializedAs("speedThreshold")]
    private float _speedThreshold = 0.2f;

    private ParticleSystem _system;

    private void Awake()
    {
        _system = GetComponent<ParticleSystem>();

        if (_agent == null)
        {
            _agent = GetComponentInParent<NavMeshAgent>();
        }
    }

    private void Update()
    {
        if (_system == null)
        {
            return;
        }

        bool moving = _agent != null && _agent.velocity.magnitude > _speedThreshold;
        ParticleSystem.EmissionModule emission = _system.emission;
        emission.enabled = moving;
    }
}
