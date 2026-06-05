using System.Collections.Generic;
using UnityEngine;

public class PowerupSystem : MonoBehaviour
{
    [SerializeField]
    private Renderer cloakRenderer;

    [SerializeField]
    private float cloakDimFactor = 0.35f;

    private readonly Dictionary<PowerupType, int> charges = new Dictionary<PowerupType, int>();

    private bool isCloaked;
    private bool cloakColorStored;
    private Color cloakOriginalColor;
    private bool seenWhileCloakedThisFrame;
    private bool hasEnteredConeWhileCloaked;

    public bool IsCloaked => isCloaked;

    private void Awake()
    {
        if (cloakRenderer == null)
        {
            cloakRenderer = GetComponentInChildren<Renderer>();
        }
    }

    private void OnEnable()
    {
        GameEvents.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        ClearCloak();
    }

    public void Grant(PowerupType type, int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }

        charges.TryGetValue(type, out int current);
        charges[type] = current + amount;
    }

    public int GetCharges(PowerupType type)
    {
        charges.TryGetValue(type, out int current);
        return current;
    }

    public bool Has(PowerupType type)
    {
        return GetCharges(type) > 0;
    }

    public bool TryConsume(PowerupType type)
    {
        charges.TryGetValue(type, out int current);
        if (current <= 0)
        {
            return false;
        }

        charges[type] = current - 1;
        return true;
    }

    public void ActivateCloak()
    {
        if (isCloaked)
        {
            return;
        }

        isCloaked = true;
        hasEnteredConeWhileCloaked = false;
        seenWhileCloakedThisFrame = false;
        ApplyCloakCue();
    }

    public void ReportCloakedSighting()
    {
        seenWhileCloakedThisFrame = true;
    }

    private void LateUpdate()
    {
        if (!isCloaked)
        {
            return;
        }

        if (seenWhileCloakedThisFrame)
        {
            hasEnteredConeWhileCloaked = true;
        }
        else if (hasEnteredConeWhileCloaked)
        {
            ClearCloak();
        }

        seenWhileCloakedThisFrame = false;
    }

    public void ClearCloak()
    {
        if (!isCloaked)
        {
            return;
        }

        isCloaked = false;
        RestoreCloakCue();
    }

    private void ApplyCloakCue()
    {
        if (cloakRenderer == null)
        {
            return;
        }

        Material material = cloakRenderer.material;
        if (!cloakColorStored)
        {
            cloakOriginalColor = material.color;
            cloakColorStored = true;
        }

        Color dimmed = cloakOriginalColor * cloakDimFactor;
        dimmed.a = cloakOriginalColor.a;
        material.color = dimmed;
    }

    private void RestoreCloakCue()
    {
        if (cloakRenderer == null || !cloakColorStored)
        {
            return;
        }

        cloakRenderer.material.color = cloakOriginalColor;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state != GameState.Playing)
        {
            ClearCloak();
        }
    }
}
