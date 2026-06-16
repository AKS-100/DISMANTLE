using System.Collections;
using UnityEngine;

/// <summary>
/// Boss enemy AI script.
/// Phase 1: Boss levitates (Levitate Idle animation plays).
/// Phase 2: When the player enters attackRange, the boss lands and 
///           performs the Sword_Regular_Combo melee attack animation.
/// </summary>
public class BossEnemy : Enemy
{
    [Header("Boss Phase Settings")]
    [Tooltip("Distance at which the boss drops down and begins the sword combo.")]
    public float attackRange = 6f;

    [Tooltip("Name of the Bool parameter in the Animator that controls grounded/attacking state." +
             " Must match exactly what you set in the Animator window.")]
    public string isGroundedBoolName = "isGrounded";

    [Tooltip("How long (seconds) to wait after landing before starting the sword combo. " +
             "Use this to let a landing animation finish if you have one.")]
    public float landingDelay = 0.5f;

    // ── internal state ──────────────────────────────────────────────
    private enum BossPhase { Levitating, Landing, Attacking }
    private BossPhase currentPhase = BossPhase.Levitating;

    private bool isGroundedParamValid = false;

    // ── Unity lifecycle ─────────────────────────────────────────────

    protected override void Setup()
    {
        base.Setup();

        // Validate the animator parameter we need
        if (animator != null)
        {
            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                if (p.name == isGroundedBoolName && p.type == AnimatorControllerParameterType.Bool)
                {
                    isGroundedParamValid = true;
                    break;
                }
            }

            if (!isGroundedParamValid)
            {
                Debug.LogWarning($"[BossEnemy] Animator parameter '{isGroundedBoolName}' not found! " +
                                 "Make sure the Bool parameter name in the Animator matches exactly.");
            }

            // Boss always starts levitating
            SetGroundedAnimParam(false);
        }
    }

    // ── core loop overrides ──────────────────────────────────────────

    protected override void HandleMovement()
    {
        // Boss does not walk — it only changes phase.
        canMove = false;

        // Always face the player while levitating or attacking
        if (target != null && currentPhase != BossPhase.Landing)
        {
            Vector3 dir = target.position - transform.position;
            dir.y = 0f;
            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(dir),
                    Time.deltaTime * 5f
                );
            }
        }
    }

    protected override void HandleActions()
    {
        if (target == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        switch (currentPhase)
        {
            case BossPhase.Levitating:
                // Wait until player is close enough, then land and attack
                if (distanceToPlayer <= attackRange)
                {
                    StartCoroutine(LandAndAttack());
                }
                break;

            case BossPhase.Attacking:
                // Let base TryToAttack handle damage logic
                TryToAttack();

                // If the player escapes range, go back to levitating
                if (distanceToPlayer > attackRange)
                {
                    ReturnToLevitating();
                }
                break;

            // Landing phase is managed by the coroutine
            case BossPhase.Landing:
                break;
        }
    }

    // ── phase transitions ────────────────────────────────────────────

    /// <summary>
    /// Coroutine: boss drops to the ground, waits for landing animation, then attacks.
    /// </summary>
    private IEnumerator LandAndAttack()
    {
        currentPhase = BossPhase.Landing;

        // Tell animator to leave Levitate Idle
        SetGroundedAnimParam(true);

        // Wait for the landing animation (or just a short delay)
        yield return new WaitForSeconds(landingDelay);

        currentPhase = BossPhase.Attacking;
        isAttacking = true;
    }

    /// <summary>
    /// Returns the boss to the levitating phase.
    /// </summary>
    private void ReturnToLevitating()
    {
        StopAllCoroutines();
        currentPhase = BossPhase.Levitating;
        isAttacking = false;
        SetGroundedAnimParam(false);
    }

    // ── animator helper ──────────────────────────────────────────────

    private void SetGroundedAnimParam(bool value)
    {
        if (animator != null && isGroundedParamValid)
        {
            animator.SetBool(isGroundedBoolName, value);
        }
    }
}
