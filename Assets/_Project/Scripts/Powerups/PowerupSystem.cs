using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PowerupSystem : MonoBehaviour
{
    private Renderer _cloakRenderer;

    [SerializeField]
    [FormerlySerializedAs("cloakDimFactor")]
    private float _cloakDimFactor = 0.35f;

    private readonly Dictionary<PowerupType, int> _charges = new Dictionary<PowerupType, int>();

    private bool _isCloaked;
    private bool _cloakColorStored;
    private Color _cloakOriginalColor;
    private bool _seenWhileCloakedThisFrame;
    private bool _hasEnteredConeWhileCloaked;

    public bool IsCloaked => _isCloaked;

    private void Awake()
    {
        _cloakRenderer = GetComponentInChildren<Renderer>();
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

        _charges.TryGetValue(type, out int current);
        _charges[type] = current + amount;
    }

    private int GetCharges(PowerupType type)
    {
        _charges.TryGetValue(type, out int current);
        return current;
    }

    public bool Has(PowerupType type)
    {
        return GetCharges(type) > 0;
    }

    public bool TryConsume(PowerupType type)
    {
        _charges.TryGetValue(type, out int current);
        if (current <= 0)
        {
            return false;
        }

        _charges[type] = current - 1;
        return true;
    }

    public void ActivateCloak()
    {
        if (_isCloaked)
        {
            return;
        }

        _isCloaked = true;
        _hasEnteredConeWhileCloaked = false;
        _seenWhileCloakedThisFrame = false;
        ApplyCloakCue();
    }

    public void ReportCloakedSighting()
    {
        _seenWhileCloakedThisFrame = true;
    }

    private void LateUpdate()
    {
        if (!_isCloaked)
        {
            return;
        }

        if (_seenWhileCloakedThisFrame)
        {
            _hasEnteredConeWhileCloaked = true;
        }
        else if (_hasEnteredConeWhileCloaked)
        {
            ClearCloak();
        }

        _seenWhileCloakedThisFrame = false;
    }

    public void ClearCloak()
    {
        if (!_isCloaked)
        {
            return;
        }

        _isCloaked = false;
        RestoreCloakCue();
    }

    private void ApplyCloakCue()
    {
        if (_cloakRenderer == null)
        {
            return;
        }

        Material material = _cloakRenderer.material;
        if (!_cloakColorStored)
        {
            _cloakOriginalColor = material.color;
            _cloakColorStored = true;
        }

        Color dimmed = _cloakOriginalColor * _cloakDimFactor;
        dimmed.a = _cloakOriginalColor.a;
        material.color = dimmed;
    }

    private void RestoreCloakCue()
    {
        if (_cloakRenderer == null || !_cloakColorStored)
        {
            return;
        }

        _cloakRenderer.material.color = _cloakOriginalColor;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state != GameState.Playing)
        {
            ClearCloak();
        }
    }
}
