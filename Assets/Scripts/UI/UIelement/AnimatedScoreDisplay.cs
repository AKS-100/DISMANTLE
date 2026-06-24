using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Replaces the static score display with a smooth count-up animation.
/// Attach this to the same GameObject as your existing ScoreDisplay.
/// 
/// SETUP:
///   1. Add this script to your Score display GameObject (alongside ScoreDisplay).
///   2. Assign the same Text field as ScoreDisplay uses.
///   3. Optionally configure countUpSpeed and scorePrefix in the Inspector.
/// </summary>
public class AnimatedScoreDisplay : UIelement
{
    [Header("References")]
    [Tooltip("The Text component to display the animated score in.")]
    public Text displayText;

    [Header("Animation Settings")]
    [Tooltip("How fast the score counts up (units per second). Higher = faster.")]
    public float countUpSpeed = 150f;

    [Tooltip("Prefix shown before the score number.")]
    public string scorePrefix = "SCORE: ";

    [Header("Pulse on Score Change")]
    [Tooltip("Should the score text pulse/scale when it changes?")]
    public bool pulseOnChange = true;

    [Tooltip("Scale multiplier during the pulse.")]
    public float pulseScale = 1.25f;

    [Tooltip("Speed of the pulse animation.")]
    public float pulseSpeed = 10f;

    // Internal displayed value that lags behind the real score
    private float displayedScore = 0f;

    // Track real target score
    private int targetScore = 0;

    // For pulse animation
    private Vector3 originalScale;
    private Coroutine pulseCoroutine;

    private void Start()
    {
        originalScale = transform.localScale;

        if (GameManager.instance != null)
        {
            displayedScore = GameManager.score;
            targetScore    = GameManager.score;
        }
    }

    private void Update()
    {
        if (GameManager.instance == null) return;

        int newTarget = GameManager.score;

        // If the real score changed, trigger a pulse
        if (newTarget != targetScore && pulseOnChange)
        {
            targetScore = newTarget;
            TriggerPulse();
        }
        else
        {
            targetScore = newTarget;
        }

        // Smoothly count up toward the target
        if (Mathf.Abs(displayedScore - targetScore) > 0.5f)
        {
            displayedScore = Mathf.MoveTowards(displayedScore, targetScore, countUpSpeed * Time.deltaTime);
        }
        else
        {
            displayedScore = targetScore;
        }

        // Update display text
        if (displayText != null)
        {
            displayText.text = scorePrefix + Mathf.RoundToInt(displayedScore).ToString();
        }
    }

    private void TriggerPulse()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }
        pulseCoroutine = StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        // Scale up
        Vector3 bigScale = originalScale * pulseScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * pulseSpeed;
            transform.localScale = Vector3.Lerp(originalScale, bigScale, t);
            yield return null;
        }

        // Scale back down
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * pulseSpeed;
            transform.localScale = Vector3.Lerp(bigScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
        pulseCoroutine = null;
    }

    /// <summary>
    /// Called by the UIManager to update all UIElements.
    /// We override here so our animated display is also refreshed.
    /// </summary>
    public override void UpdateUI()
    {
        base.UpdateUI();
        // The actual display logic runs in Update() — nothing to force here
    }
}
