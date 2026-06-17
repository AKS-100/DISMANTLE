using System.Collections;
using UnityEngine;

/// <summary>
/// Full Boss AI — NO NavMesh required. Uses Rigidbody movement (wall collisions work).
/// ANIMATOR PARAMETERS REQUIRED:
///   isGrounded  (Bool)   — false = Levitate Idle, true = grounded
///   isRunning   (Bool)   — true = Running
///   attackIndex (Int)    — 0=none, 1=Slash, 2=Punch, 3=Combo
/// </summary>
public class BossEnemy : Enemy
{
    // Rigidbody cached reference
    private Rigidbody rb;

    [Header("Boss – Detection")]
    public float detectionRange = 18f;

    [Header("Boss – Combat")]
    public float attackRange    = 8f;
    public float chaseRange     = 22f;

    [Header("Boss – Movement")]
    public float chaseSpeed     = 4f;

    [Header("Boss – Landing")]
    public float landingDelay   = 1.25f;

    [Header("Boss – Attack Timings")]
    public float slashDuration  = 1.2f;
    public float punchDuration  = 1.0f;
    public float comboDuration  = 1.8f;
    public float attackCooldown = 2f;

    [Header("Boss – Hand2 Punch Collider")]
    [Tooltip("Drag the hand2 bone Collider here. Must be a Trigger.")]
    public Collider hand2Collider;
    public int punchDamage = 2;

    [Header("Boss – Animator Parameter Names")]
    public string isGroundedParam  = "isGrounded";
    public string isRunningParam   = "isRunning";
    public string attackIndexParam = "attackIndex";

    // ── Internal State ──────────────────────────────────────
    private enum BossPhase { Levitating, Landing, Grounded }
    private enum GroundedState { Idle, Chasing, Attacking }

    private BossPhase     phase       = BossPhase.Levitating;
    private GroundedState groundState = GroundedState.Idle;

    private bool attackOnCooldown = false;
    private int  lastAttackIndex  = 0;
    private bool landingStarted   = false;

    // ── Setup ───────────────────────────────────────────────
    protected override void Setup()
    {
        base.Setup();
        canMove = false; // we handle movement ourselves

        // Cache Rigidbody — add one if missing
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.Log("[BossEnemy] Added Rigidbody automatically.");
        }
        rb.freezeRotation = true;   // stop physics from tipping boss over
        rb.useGravity     = false;  // animation/root motion handles Y
        rb.isKinematic    = true;   // we drive movement, collisions still work

        // Auto-configure hand2 collider for punch damage
        if (hand2Collider != null)
        {
            Damage d = hand2Collider.GetComponent<Damage>();
            if (d == null) d = hand2Collider.gameObject.AddComponent<Damage>();
            d.teamId                   = 1;
            d.damageAmount             = punchDamage;
            d.destroyAfterDamage       = false;
            d.dealDamageOnTriggerEnter = true;
            hand2Collider.enabled      = false;
        }

        SetAnimBool(isGroundedParam,  false);
        SetAnimBool(isRunningParam,   false);
        SetAnimInt (attackIndexParam, 0);
    }

    // ── Movement ────────────────────────────────────────────
    protected override void HandleMovement()
    {
        if (target == null) return;

        float dist = Vector3.Distance(transform.position, target.position);

        switch (phase)
        {
            case BossPhase.Levitating:
                FaceTarget();
                if (!landingStarted && dist <= detectionRange)
                {
                    landingStarted = true;
                    StartCoroutine(LandRoutine());
                }
                break;

            case BossPhase.Landing:
                // wait for coroutine
                break;

            case BossPhase.Grounded:
                // Player ran too far → go back to sky
                if (dist > chaseRange)
                {
                    ReturnToLevitating();
                    break;
                }

                if (groundState == GroundedState.Attacking)
                {
                    FaceTarget();
                    break;
                }

                if (dist <= attackRange)
                {
                    // In range → stop chasing, prepare to attack
                    SetAnimBool(isRunningParam, false);
                    groundState = GroundedState.Idle;
                    FaceTarget();
                }
                else
                {
                    // Chase the player directly
                    MoveTowardPlayer();
                }
                break;
        }
    }

    // ── Actions ─────────────────────────────────────────────
    protected override void HandleActions()
    {
        if (target == null)                         return;
        if (phase != BossPhase.Grounded)            return;
        if (groundState == GroundedState.Attacking) return;
        if (attackOnCooldown)                       return;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= attackRange)
        {
            StartCoroutine(PerformRandomAttack());
        }
    }

    // ── Phase Transitions ───────────────────────────────────
    private IEnumerator LandRoutine()
    {
        phase = BossPhase.Landing;
        SetAnimBool(isGroundedParam, true);
        yield return new WaitForSeconds(landingDelay);
        phase       = BossPhase.Grounded;
        groundState = GroundedState.Idle;
    }

    private void ReturnToLevitating()
    {
        StopAllCoroutines();
        phase            = BossPhase.Levitating;
        groundState      = GroundedState.Idle;
        attackOnCooldown = false;
        isAttacking      = false;
        landingStarted   = false;

        SetAnimBool(isGroundedParam,  false);
        SetAnimBool(isRunningParam,   false);
        SetAnimInt (attackIndexParam, 0);

        if (hand2Collider != null) hand2Collider.enabled = false;
    }

    // ── Movement Helper ─────────────────────────────────────
    private void MoveTowardPlayer()
    {
        if (target == null) return;

        groundState = GroundedState.Chasing;
        SetAnimBool(isRunningParam, true);

        // Move toward player on XZ plane using Rigidbody (respects colliders/walls)
        Vector3 direction = (target.position - transform.position);
        direction.y = 0f;
        direction.Normalize();

        Vector3 newPos = rb != null
            ? rb.position + direction * chaseSpeed * Time.fixedDeltaTime
            : transform.position + direction * chaseSpeed * Time.fixedDeltaTime;

        if (rb != null)
            rb.MovePosition(newPos);
        else
            transform.position = newPos;

        FaceTarget();
    }

    private void FaceTarget()
    {
        if (target == null) return;
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 8f
            );
        }
    }

    // ── Attack Routines ─────────────────────────────────────
    private IEnumerator PerformRandomAttack()
    {
        attackOnCooldown = true;
        groundState      = GroundedState.Attacking;
        isAttacking      = true;

        int index;
        do { index = Random.Range(1, 4); } while (index == lastAttackIndex);
        lastAttackIndex = index;

        SetAnimInt(attackIndexParam, index);

        float duration = index == 1 ? slashDuration
                       : index == 2 ? punchDuration
                       :              comboDuration;

        if (index == 2)
            StartCoroutine(EnablePunchCollider(duration));

        yield return new WaitForSeconds(duration);

        SetAnimInt(attackIndexParam, 0);
        isAttacking = false;
        groundState = GroundedState.Idle;

        yield return new WaitForSeconds(attackCooldown);
        attackOnCooldown = false;
    }

    private IEnumerator EnablePunchCollider(float attackDuration)
    {
        yield return new WaitForSeconds(attackDuration * 0.3f);
        if (hand2Collider != null) hand2Collider.enabled = true;

        yield return new WaitForSeconds(attackDuration * 0.4f);
        if (hand2Collider != null) hand2Collider.enabled = false;
    }

    // ── Animator Helpers ────────────────────────────────────
    private void SetAnimBool(string param, bool value)
    {
        if (animator != null) animator.SetBool(param, value);
    }

    private void SetAnimInt(string param, int value)
    {
        if (animator != null) animator.SetInteger(param, value);
    }
}
