using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Class which derives from the Enemy base class. Handles ground enemy movement
/// </summary>
public class GroundEnemy : Enemy
{
    [Header("Ground Enemy Settings")]
    [Tooltip("The nav mesh agent used to move this enemy")]
    public NavMeshAgent agent = null;
    [Tooltip("The distance at which to stop before reaching the objective")]
    public float stopDistance = 2.0f;
    [Tooltip("Whether this enemy can stop if it is within it's stop distance but does not have line of sight to it's target")]
    public bool lineOfSightToStop = true;
    [Tooltip("Whether this enemy should always face the player, or face in the direction it is moving")]
    public bool alwaysFacePlayer = true;

    [Header("Physics Hover Settings (used when NavMesh Agent is removed)")]
    [Tooltip("Height above the ground this enemy hovers at when using physics movement")]
    public float hoverHeight = 3.0f;
    [Tooltip("How strongly the hover corrects the height — higher = snappier")]
    public float hoverStrength = 8.0f;
    [Tooltip("How much the hover dampens vertical movement to prevent bouncing")]
    public float hoverDamping = 4.0f;

    /// <summary>
    /// Description:
    /// Overrides base function setup, sets up nav mesh agent reference
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    protected override void Setup()
    {
        base.Setup();
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        if (agent != null)
        {
            agent.updateRotation = !alwaysFacePlayer;
        }
    }

    /// <summary>
    /// Description:
    /// Makes the enemy nav agent go to a specific location and will continue to try to reach that location until
    /// the specified time has passed
    /// Input:
    /// Vector3 target, float timeToSpend
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="target">The target to travel to</param>
    /// <param name="timeToSpend">The time to spend trying to get to that target</param>
    public void GoToTarget(Vector3 target, float timeToSpend)
    {
        if (agent != null)
        {
            agent.SetDestination(target);
        }
        travelingToSpecificTarget = true;
        timeToStopTrying = Time.time + timeToSpend;
    }

    bool travelingToSpecificTarget = false;
    float timeToStopTrying = 0;

    /// <summary>
    /// Description:
    /// Handles movement for this enemy
    /// Rotates the enemy to face it's target and sets navmesh destination
    /// Inputs:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    protected override void HandleMovement()
    {
        if (enemyRigidbody != null)
        {
            enemyRigidbody.velocity = new Vector3(0, enemyRigidbody.velocity.y, 0);
            enemyRigidbody.angularVelocity = Vector3.zero;
        }

        // Always face player when no NavMesh agent (physics-based movement)
        bool shouldFaceTarget = ShouldMove() || alwaysFacePlayer || (agent == null && target != null);
        if (shouldFaceTarget && target != null)
        {
            Quaternion desiredRotation = CalculateDesiredRotation();
            if (enemyRigidbody != null)
            {
                // Use MoveRotation so physics constraints are respected properly
                enemyRigidbody.MoveRotation(desiredRotation);
            }
            else
            {
                transform.rotation = desiredRotation;
            }
        }
        
        if (travelingToSpecificTarget)
        {
            if (Time.time >= timeToStopTrying || (agent != null && NavMeshAgentDestinationReached()))
            {
                travelingToSpecificTarget = false;
                if (agent != null) agent.SetDestination(target.position);
            }
        }
        else if (ShouldMove())
        {
            if (agent != null) 
            {
                agent.SetDestination(target.position);
            }
            else 
            {
                // --- FLYING / HOVER MOVEMENT (no NavMesh) ---
                // This mode makes the enemy hover at a fixed height above the ground
                // and fly directly toward the player, clearing stairs and walls.

                Vector3 desiredDir = (target.position - transform.position);
                desiredDir.y = 0;
                desiredDir.Normalize();

                if (enemyRigidbody != null)
                {
                    // --- Hover height correction ---
                    // Raycast straight down to find the ground below
                    RaycastHit groundHit;
                    int enemyLayer = gameObject.layer;
                    int groundMask = ~(1 << enemyLayer);
                    float currentGroundY = transform.position.y; // fallback
                    if (Physics.Raycast(transform.position, Vector3.down, out groundHit, 20f, groundMask))
                    {
                        currentGroundY = groundHit.point.y;
                    }

                    float targetY = currentGroundY + hoverHeight;
                    float yError = targetY - transform.position.y;
                    // Spring-damper for smooth hover
                    float yVelocity = enemyRigidbody.velocity.y;
                    float hoverForce = (yError * hoverStrength) - (yVelocity * hoverDamping);

                    // Cancel gravity effect and apply our own hover
                    hoverForce += Mathf.Abs(Physics.gravity.y); // counteract gravity

                    // --- Horizontal movement toward player ---
                    Vector3 horizontalVelocity = desiredDir * moveSpeed;

                    enemyRigidbody.velocity = new Vector3(
                        horizontalVelocity.x,
                        enemyRigidbody.velocity.y + hoverForce * Time.fixedDeltaTime,
                        horizontalVelocity.z
                    );
                }
                else
                {
                    transform.position += desiredDir * moveSpeed * Time.deltaTime;
                }
            }
        }
        else if (agent != null)
        {
            if (agent.isOnNavMesh) agent.SetDestination(transform.position);
        }
    }

    /// <summary>
    /// Description:
    /// Checks to see if the nav mesh agent has reached its destination or not
    /// Input:
    /// none
    /// Returns:
    /// bool
    /// </summary>
    /// <returns>bool: Whether or not the agent has reached its destination</returns>
    bool NavMeshAgentDestinationReached()
    {
        if (agent == null) return true;

        // Check if we've reached the destination
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Description:
    /// Small function which determines whether or not this enemy should move
    /// Input: 
    /// none
    /// Return: 
    /// bool
    /// </summary>
    /// <returns>bool: Whether or not this enemy should move</returns>
    bool ShouldMove()
    {
        // If move while attack is set to true, we can move while attacking. Otherwise we just need to check isAttacking for
        // whether or not we move
        bool attackMove = moveWhileAttacking == true || isAttacking == false;

        bool hasLineOfSight = true;

        if (lineOfSightToMove)
        {
            hasLineOfSight = HasLineOfSight();
        }

        if (target != null && canMove && attackMove && hasLineOfSight && (target.position - transform.position).magnitude < maximumMoveRange)
        {
            if ((target.position - transform.position).magnitude > stopDistance)
            {
                isMoving = true;
                return true;
            }
            else if (lineOfSightToStop && !HasLineOfSight())
            {
                isMoving = false;
                return false;
            }
        }

        if (isAttacking)
        {
            isMoving = false;
            isIdle = false;
        }
        else
        {
            isIdle = true;
        }
        return false;
    }

    /// <summary>
    /// Description:
    /// Calculates the movement that this enemy will make this frame.
    /// Input: 
    /// none
    /// Return:
    /// Vector3
    /// </summary>
    /// <returns>Vector3: The desired movement of this enemy</returns>
    protected override Vector3 CalculateDesiredMovement()
    {
        if (agent != null)
        {
            return agent.desiredVelocity * Time.deltaTime;
        }
        return base.CalculateDesiredMovement();
    }

    /// <summary>
    /// Description:
    /// Caclulates the desired rotation of this enemy this frame
    /// Input: 
    /// none
    /// Return: 
    /// Quaternion
    /// </summary>
    /// <returns>Quaternion: The desired rotation of the enemy</returns>
    protected override Quaternion CalculateDesiredRotation()
    {
        if (target != null)
        {
            if (alwaysFacePlayer || agent == null)
            {
                Vector3 lookDirection = target.position - transform.position;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    Quaternion result = Quaternion.LookRotation(lookDirection, transform.up);
                    return result;
                }
            }
        }
        return base.CalculateDesiredRotation();
    }
}
