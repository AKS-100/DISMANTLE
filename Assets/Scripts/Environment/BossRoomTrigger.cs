using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// When the player walks through this trigger, fires OnPlayerEnter events once.
/// Use this to close a boss room door behind the player.
/// </summary>
public class BossRoomTrigger : MonoBehaviour
{
    [Tooltip("Events to fire the moment the player enters this trigger (e.g. close the door)")]
    public UnityEvent onPlayerEnter = new UnityEvent();

    [Tooltip("Only trigger once")]
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            onPlayerEnter.Invoke();
        }
    }
}
