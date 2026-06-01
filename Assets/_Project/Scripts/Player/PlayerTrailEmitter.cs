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
    private ParticleSystem.EmissionModule _emission;

    private void Awake()
    {
        _system = GetComponent<ParticleSystem>();
        _emission = _system.emission;

        if (agent == null)
        {
            agent = GetComponentInParent<NavMeshAgent>();
        }
    }

    private void Update()
    {
        bool moving = agent != null && agent.velocity.magnitude > speedThreshold;
        _emission.enabled = moving;
    }
}
