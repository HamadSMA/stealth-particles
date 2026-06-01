using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(ParticleSystem))]
public class PlayerTrailEmitter : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent agent;

    [SerializeField]
    private float speedThreshold = 0.2f;

    private ParticleSystem _system;

    private void Awake()
    {
        _system = GetComponent<ParticleSystem>();

        if (agent == null)
        {
            agent = GetComponentInParent<NavMeshAgent>();
        }
    }

    private void Update()
    {
        if (_system == null)
        {
            return;
        }

        bool moving = agent != null && agent.velocity.magnitude > speedThreshold;
        ParticleSystem.EmissionModule emission = _system.emission;
        emission.enabled = moving;
    }
}
