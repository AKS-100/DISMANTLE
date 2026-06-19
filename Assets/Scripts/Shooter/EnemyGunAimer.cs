using UnityEngine;

/// <summary>
/// Attach this to the Gun child object on an enemy.
/// It silently rotates ONLY the fire location transform toward the target
/// every frame, so bullets fly toward the player without moving the gun model.
/// </summary>
public class EnemyGunAimer : MonoBehaviour
{
    [Tooltip("The transform that bullets spawn from (the Fire Location on the Gun)")]
    public Transform fireLocation;

    [Tooltip("The player or target to aim at")]
    public Transform aimTarget;

    [Tooltip("Offset applied to the target position (e.g. aim at chest height instead of feet)")]
    public Vector3 targetOffset = new Vector3(0, 1f, 0);

    private void Update()
    {
        if (fireLocation == null || aimTarget == null)
            return;

        // Point the fire location directly at the player's position (including Y height)
        Vector3 targetPos = aimTarget.position + targetOffset;
        Vector3 directionToTarget = (targetPos - fireLocation.position).normalized;

        if (directionToTarget != Vector3.zero)
        {
            fireLocation.rotation = Quaternion.LookRotation(directionToTarget);
        }
    }
}
