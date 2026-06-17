using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Full Boss AI with multiple phases and attacks.
/// ANIMATOR PARAMETERS REQUIRED:
///   isGrounded  (Bool)   — false = Levitate Idle, true = grounded
///   isRunning   (Bool)   — true = Running
///   attackIndex (Int)    — 0=none, 1=Slash, 2=Punch, 3=Combo
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class BossEnemy : Enemy
{
    [Header("Boss – Detection")]
    public float detectionRange = 18f;

    [Header("Boss – Combat")]
    public float attackRange    = 5f;
    public float chaseRange     = 20f;

    [Header("Boss – Movement")]
    public float chaseSpeed     = 5f;
    public float stopDistance   = 2.5f;

    [Header("Boss – Landing")]
    public float landingDelay   = 1.25f;

    [Header("Boss – Attack Timings")]
    public float slashDuration  = 1.2f;
    public float punchDuration  = 1.0f;
    public float comboDuration  = 1.8f;
    public float attackCooldown = 2f;

    [Header("Boss – Hand2 Punch Collider")]
    [Tooltip("Drag the hand2 bone's Collider here. Must be a Trigger.")]
    public Collider hand2Collider;
    public int punchDamage = 2;

    [Header("Boss – Animator Parameter Names")]
    public string isGroundedParam  = "isGrounded";
    public string isRunningParam   = "isRunning";
    public string attackIndexParam = "attackIndex";

    // ── Internal State ─────────────────────────────────────
    private enum BossPhase { Levitating, Landing, Grounded }
    private enum GroundedState { Idle, Chasing, Attacking }

    private BossPhase     phase        = BossPhase.Levitating;
    private GroundedState groundState  = GroundedState.Idle;

    private NavMeshAgent agent;
    private bool attackOnCooldown = false;
    private int  lastAttackIndex  = 0;
    private bool landingStarted   = false;   // ← FIX: prevent multiple coroutine starts

    // ── Setup ──────────────────────────────────────────────
    protected override void Setup()
    {
        base.Setup();
        canMove = false;

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed           = chaseSpeed;
            agent.stoppingDistance = stopDistance;
            agent.updateRotation  = false;
            agent.isStopped       = true;
        }

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

        // Start in levitating state
        SetAnimBool(isGroundedParam,  false);
        SetAnimBool(isRunningParam,   false);
        SetAnimInt (attackIndexParam, 0);
    }

    // ── Movement (called every FixedUpdate by base class) ──
    protected override void HandleMovement()
    {
        if (target == null) return;

        float dist = Vector3.Distance(transform.position, target.position);

        switch (phase)
        {
            // ─── LEVITATING ───────────────────────────────
            case BossPhase.Levitating:
                StopAgent();
                FaceTarget();
                // Only start landing ONCE
                if (!landingStarted && dist <= detectionRange)
                {
                    landingStarted = true;
                    StartCoroutine(LandRoutine());
                }
                break;

            // ─── LANDING ─────────────────────────────────
            case BossPhase.Landing:
                StopAgent();
                FaceTarget();
                break;

            // ─── GROUNDED ────────────────────────────────
            case BossPhase.Grounded:
                // Escaped completely → return to sky
                if (dist > chaseRange)
                {
                    ReturnToLevitating();
                    break;
                }

                // Don't move while attacking
                if (groundState == GroundedState.Attacking)
                {
                    StopAgent();
                    FaceTarget();
                    break;
                }

                if (dist <= attackRange)
                {
                    // In attack range → stop and prepare to attack
                    StopAgent();
                    FaceTarget();
                    SetAnimBool(isRunningParam, false);
                    groundState = GroundedState.Idle;
                }
                else
                {
                    // Player fled → chase
                    ChasePlayer();
                }
                break;
        }
    }

    // ── Actions (called every FixedUpdate by base class) ───
    protected override void HandleActions()
    {
        if (target == null)                            return;
        if (phase != BossPhase.Grounded)               return;
        if (groundState == GroundedState.Attacking)    return;
        if (attackOnCooldown)                          return;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= attackRange)
        {
            StartCoroutine(PerformRandomAttack());
        }
    }

    // ── Phase Routines ──────────────────────────────────────
    private IEnumerator LandRoutine()
    {
        phase = BossPhase.Landing;
        SetAnimBool(isGroundedParam, true);            // triggers jump anim
        yield return new WaitForSeconds(landingDelay); // wait for jump to finish
        phase      = BossPhase.Grounded;
        groundState = GroundedState.Idle;
    }

    private void ReturnToLevitating()
    {
        StopAllCoroutines();
        phase            = BossPhase.Levitating;
        groundState      = GroundedState.Idle;
        attackOnCooldown = false;
        isAttacking      = false;
        landingStarted   = false;   // allow landing again next time

        StopAgent();
        SetAnimBool(isGroundedParam,  false);
        SetAnimBool(isRunningParam,   false);
        SetAnimInt (attackIndexParam, 0);

        if (hand2Collider != null) hand2Collider.enabled = false;
    }

    // ── Movement Helpers ────────────────────────────────────
    private void ChasePlayer()
    {
        if (agent == null || target == null) return;
        groundState = GroundedState.Chasing;
        SetAnimBool(isRunningParam, true);
        agent.isStopped = false;
        agent.SetDestination(target.position);
        FaceTarget();
    }

    private void StopAgent()
    {
        if (agent == null) return;
        agent.isStopped = true;
        agent.velocity  = Vector3.zero;
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

    // ── Attack Routines ──────────────────────────────────────
    private IEnumerator PerformRandomAttack()
    {
        attackOnCooldown = true;
        groundState      = GroundedState.Attacking;
        isAttacking      = true;

        // Pick attack 1-3, avoid repeating
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

        // Reset
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
