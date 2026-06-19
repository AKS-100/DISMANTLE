using System.Collections;
using UnityEngine;

/// <summary>
/// Attach this to the astro door. When the player walks through the trigger,
/// it calls Door.Close() after a short delay, which fires the door's Close Event
/// (e.g. plays the close animation).
/// </summary>
public class AutoCloseDoor : MonoBehaviour
{
    [Tooltip("How many seconds to wait after the player passes through before closing")]
    public float closeDelay = 0.5f;

    [Tooltip("Tag used to identify the player")]
    public string playerTag = "Player";

    private bool playerHasPassed = false;
    private Door door;

    private void Awake()
    {
        door = GetComponent<Door>();
        if (door == null)
            door = GetComponentInChildren<Door>();
        if (door == null)
            door = GetComponentInParent<Door>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !playerHasPassed)
        {
            playerHasPassed = true;
            StartCoroutine(CloseAfterDelay());
        }
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);

        if (door != null)
        {
            // This calls Door.Close() which fires the closeEvent
            // (which triggers the Closed animation in your Animator)
            door.Close();
            
            // PERMANENTLY LOCK the door so it doesn't reopen if the player 
            // respawns at a checkpoint right next to the door
            door.doorID = 9999;
        }
        else
        {
            // Fallback: directly trigger the animator if Door component not found
            Animator anim = GetComponentInChildren<Animator>();
            if (anim != null)
                anim.SetTrigger("Closed");
        }
    }
}
