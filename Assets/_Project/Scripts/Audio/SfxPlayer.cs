using UnityEngine;

public class SfxPlayer : MonoBehaviour
{
    private static SfxPlayer _instance;

    private SfxBank _bank;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null)
        {
            return;
        }

        GameObject go = new GameObject("SfxPlayer");
        go.AddComponent<SfxPlayer>();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        SfxBank[] banks = Resources.LoadAll<SfxBank>(string.Empty);
        _bank = banks.Length > 0 ? banks[0] : null;
    }

    private void OnEnable()
    {
        GameEvents.OnGuardNeutralized += HandleGuardNeutralized;
        GameEvents.OnPowerupCollected += HandlePowerupCollected;
        GameEvents.OnPanelDisabled += HandlePanelDisabled;
        GameEvents.OnLootCollected += HandleLootCollected;
    }

    private void OnDisable()
    {
        GameEvents.OnGuardNeutralized -= HandleGuardNeutralized;
        GameEvents.OnPowerupCollected -= HandlePowerupCollected;
        GameEvents.OnPanelDisabled -= HandlePanelDisabled;
        GameEvents.OnLootCollected -= HandleLootCollected;
    }

    private void HandleGuardNeutralized()
    {
        Play(_bank != null ? _bank.GuardHoldup : null);
    }

    private void HandlePowerupCollected()
    {
        Play(_bank != null ? _bank.PowerupPickup : null);
    }

    private void HandlePanelDisabled()
    {
        Play(_bank != null ? _bank.PanelDisable : null);
    }

    private void HandleLootCollected(int collected, int total)
    {
        Play(_bank != null ? _bank.LootPickup : null);
    }

    private void Play(SfxBank.Sfx sfx)
    {
        if (sfx == null || sfx.Clip == null || AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlaySfx(sfx.Clip, sfx.Volume);
    }
}
