using UnityEngine;

/// <summary>
/// Triggers a change in background music when the player enters the trigger.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MusicTrigger : MonoBehaviour
{
    public enum MusicTrackType { NormalBGM, BossBGM }

    [Header("Trigger Settings")]
    [Tooltip("Which track to play when the player enters this trigger.")]
    public MusicTrackType trackToPlay = MusicTrackType.BossBGM;
    [Tooltip("Should this trigger deactivate itself after playing the music once?")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private void Start()
    {
        // Ensure the collider is set to be a trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Print warning if placed on UI layer (Layer 5) as it won't collide with 3D player
        if (gameObject.layer == 5)
        {
            Debug.LogWarning($"[MusicTrigger] '{gameObject.name}' is on the UI layer (Layer 5) and inside a Canvas/UI parent. " +
                             "3D colliders cannot interact with UI layer elements. Since the Boss now manages the music automatically, " +
                             "you can safely delete this BossMusicTrigger GameObject.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered) return;

        // Check if the collider belongs to the Player
        if (other.CompareTag("Player"))
        {
            if (MusicManager.instance != null)
            {
                if (trackToPlay == MusicTrackType.BossBGM)
                {
                    MusicManager.instance.PlayBossBGM();
                }
                else
                {
                    MusicManager.instance.PlayNormalBGM();
                }

                hasTriggered = true;
                
                if (triggerOnce)
                {
                    gameObject.SetActive(false); // disable trigger
                }
            }
            else
            {
                Debug.LogWarning("[MusicTrigger] MusicManager instance not found in scene!");
            }
        }
    }
}
