using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "SO_Sfx_Bank", menuName = "Stealth Particles/SFX Bank")]
public class SfxBank : ScriptableObject
{
    [System.Serializable]
    public class Sfx
    {
        [FormerlySerializedAs("clip")]
        public AudioClip Clip;

        [Range(0f, 1f)]
        [FormerlySerializedAs("volume")]
        public float Volume = 1f;
    }

    [FormerlySerializedAs("tapMove")]
    public Sfx TapMove;

    [FormerlySerializedAs("guardHoldup")]
    public Sfx GuardHoldup;

    [FormerlySerializedAs("powerupPickup")]
    public Sfx PowerupPickup;

    [FormerlySerializedAs("panelDisable")]
    public Sfx PanelDisable;

    [FormerlySerializedAs("lootPickup")]
    public Sfx LootPickup;

    [FormerlySerializedAs("detection")]
    public Sfx Detection;

    [FormerlySerializedAs("successJingle")]
    public Sfx SuccessJingle;

    [FormerlySerializedAs("rankSlam")]
    public Sfx RankSlam;
}
