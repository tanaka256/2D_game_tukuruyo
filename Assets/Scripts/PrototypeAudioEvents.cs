using UnityEngine;

/// <summary>
/// Audio clips for gameplay events. Clips are assigned in the prefab Inspector.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public sealed class PrototypeAudioEvents : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private AudioClip playerShot;
    [SerializeField] private AudioClip chargeStart;
    [SerializeField] private AudioClip chargedShot;
    [SerializeField] private AudioClip missileShot;
    [SerializeField] private AudioClip playerHit;
    [SerializeField] private AudioClip playerJump;
    [SerializeField] private AudioClip playerLand;

    [Header("Enemy")]
    [SerializeField] private AudioClip enemyShot;
    [SerializeField] private AudioClip enemyHit;
    [SerializeField] private AudioClip enemyDeath;
    [SerializeField] private AudioClip guardBlock;

    [Header("Levels")]
    [Range(0f, 1f)]
    [SerializeField] private float effectsVolume = 0.8f;

    private AudioSource audioSource;
    private AudioSource chargeAudioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;

        chargeAudioSource = gameObject.AddComponent<AudioSource>();
        chargeAudioSource.playOnAwake = false;
        chargeAudioSource.loop = true;
        chargeAudioSource.spatialBlend = 0f;
        chargeAudioSource.volume = effectsVolume;
    }

    public void PlayPlayerShot() => Play(playerShot);
    public void PlayChargeStart()
    {
        if (chargeStart == null)
        {
            return;
        }

        if (chargeAudioSource == null)
        {
            return;
        }

        chargeAudioSource.clip = chargeStart;
        chargeAudioSource.volume = effectsVolume;
        chargeAudioSource.Play();
    }

    public void StopChargeStart()
    {
        if (chargeAudioSource != null && chargeAudioSource.isPlaying)
        {
            chargeAudioSource.Stop();
        }
    }
    public void PlayChargedShot() => Play(chargedShot);
    public void PlayMissileShot() => Play(missileShot);
    public void PlayEnemyShot() => Play(enemyShot);
    public void PlayPlayerHit() => Play(playerHit);
    public void PlayEnemyHit() => Play(enemyHit);
    public void PlayGuardBlock() => Play(guardBlock);
    public void PlayPlayerJump() => Play(playerJump);
    public void PlayPlayerLand() => Play(playerLand);

    public void PlayDeath()
    {
        AudioClip clip = GetComponent<PrototypeHealth>()?.Faction == PrototypeFaction.Player
            ? null
            : enemyDeath;
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, effectsVolume);
        }
    }

    private void Play(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        audioSource.PlayOneShot(clip, effectsVolume);
    }
}
