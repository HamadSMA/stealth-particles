using UnityEngine;

[CreateAssetMenu(fileName = "SfxBank", menuName = "Stealth Particles/SFX Bank")]
public class SfxBank : ScriptableObject
{
    [System.Serializable]
    public class Sfx
    {
        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume = 1f;
    }

    public Sfx tapMove;
    public Sfx guardHoldup;
    public Sfx powerupPickup;
    public Sfx panelDisable;
    public Sfx lootPickup;
    public Sfx detection;
    public Sfx successJingle;
    public Sfx rankSlam;
}
