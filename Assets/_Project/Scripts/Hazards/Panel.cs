using UnityEngine;

public class Panel : MonoBehaviour
{
    [SerializeField]
    private Laser linkedLaser;

    [SerializeField]
    private Renderer panelRenderer;

    [SerializeField]
    private Material armedMaterial;

    [SerializeField]
    private Material usedMaterial;

    [SerializeField]
    private float activationRange = 3.5f;

    private bool isPlaying;
    private bool isUsed;

    public bool IsUsed => isUsed;

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Start()
    {
        if (!isUsed && panelRenderer != null && armedMaterial != null)
        {
            panelRenderer.sharedMaterial = armedMaterial;
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        isPlaying = state == GameState.Playing;
    }

    public bool TryDisable(Vector3 playerWorldPos)
    {
        if (!isPlaying || isUsed)
        {
            return false;
        }

        if (Vector3.Distance(playerWorldPos, transform.position) > activationRange)
        {
            return false;
        }

        if (linkedLaser != null)
        {
            linkedLaser.SetActive(false);
        }

        isUsed = true;

        if (panelRenderer != null && usedMaterial != null)
        {
            panelRenderer.sharedMaterial = usedMaterial;
        }

        return true;
    }
}
