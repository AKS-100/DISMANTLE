using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Dynamically controls URP Post-Processing Volume effects at runtime:
///   - Vignette darkens as player health drops (danger warning)
///   - Chromatic Aberration spikes briefly on damage
///   - Lens Distortion pulses on boss impacts
///
/// SETUP IN UNITY EDITOR:
///   1. In your scene hierarchy, create: GameObject > Volume (or right-click > Volume > Global Volume).
///   2. In the Volume component, click "New" to create a new Volume Profile.
///   3. In the Volume Profile inspector, click "Add Override" and add:
///       - Vignette         (tick "Active" + tick the "Intensity" override checkbox)
///       - Chromatic Aberration (tick "Active" + tick the "Intensity" override checkbox)
///       - Lens Distortion  (tick "Active" + tick the "Intensity" and "Scale" override checkboxes)
///   4. Assign this Volume GameObject to the "postProcessVolume" field of this script.
///   5. Leave all other fields at defaults; they are tuned in-inspector.
///
/// NOTE: This script requires URP with post-processing enabled in the active renderer.
///       Go to your URP Renderer asset and enable "Post Processing".
/// </summary>
public class DynamicPostProcessing : MonoBehaviour
{
    public static DynamicPostProcessing instance;

    [Header("Volume Reference")]
    [Tooltip("The Global Volume in your scene that holds post-processing overrides.")]
    public Volume postProcessVolume;

    [Header("Vignette — Low Health Warning")]
    [Tooltip("Vignette intensity at full health.")]
    public float vignetteHealthyIntensity = 0.22f;

    [Tooltip("Vignette intensity at critical health (1 HP).")]
    public float vignetteDangerIntensity = 0.65f;

    [Tooltip("How quickly vignette lerps to the target value.")]
    public float vignetteTransitionSpeed = 2f;

    [Header("Chromatic Aberration — Damage Hit")]
    [Tooltip("Peak chromatic aberration intensity spike on damage.")]
    public float chromaticHitIntensity = 0.8f;

    [Tooltip("How quickly chromatic aberration recovers after a hit.")]
    public float chromaticRecoverySpeed = 5f;

    [Header("Lens Distortion — Boss Impact")]
    [Tooltip("Peak lens distortion value during a boss slam pulse.")]
    public float distortionPeakIntensity = -0.35f;

    [Tooltip("Speed of the distortion pulse recovery.")]
    public float distortionRecoverySpeed = 6f;

    // Internal URP effect references
    private Vignette       vignette;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;

    // Runtime targets
    private float targetVignetteIntensity;
    private float currentChromaticIntensity = 0f;
    private float currentDistortionIntensity = 0f;

    // Player health reference
    private Health playerHealth;
    private int lastKnownHealth;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
            return;
        }

        if (postProcessVolume == null)
        {
            Debug.LogWarning("[DynamicPostProcessing] No Volume assigned. Please assign a Global Volume in the Inspector.");
            return;
        }

        // Grab profile overrides
        postProcessVolume.profile.TryGet(out vignette);
        postProcessVolume.profile.TryGet(out chromaticAberration);
        postProcessVolume.profile.TryGet(out lensDistortion);

        if (vignette == null)
            Debug.LogWarning("[DynamicPostProcessing] No Vignette override found in the Volume Profile. Add it via 'Add Override'.");
        if (chromaticAberration == null)
            Debug.LogWarning("[DynamicPostProcessing] No Chromatic Aberration override found in the Volume Profile.");
        if (lensDistortion == null)
            Debug.LogWarning("[DynamicPostProcessing] No Lens Distortion override found in the Volume Profile.");
    }

    private void Start()
    {
        // Find player health
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            playerHealth = GameManager.instance.player.GetComponent<Health>();
        }

        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerController>()?.GetComponent<Health>();
        }

        if (playerHealth != null)
        {
            lastKnownHealth = playerHealth.currentHealth;
        }

        targetVignetteIntensity = vignetteHealthyIntensity;
    }

    private void Update()
    {
        if (playerHealth != null)
        {
            UpdateVignette();
            DetectDamage();
        }

        RecoverChromatic();
        RecoverDistortion();
    }

    // ── Vignette — Health-Based Danger Feeling ──────────────────────────────
    private void UpdateVignette()
    {
        if (vignette == null) return;

        // Normalise health 0..1
        float healthRatio = (float)playerHealth.currentHealth / Mathf.Max(playerHealth.maximumHealth, 1);

        // As health drops, vignette increases
        targetVignetteIntensity = Mathf.Lerp(vignetteDangerIntensity, vignetteHealthyIntensity, healthRatio);

        vignette.intensity.value = Mathf.Lerp(
            vignette.intensity.value,
            targetVignetteIntensity,
            Time.deltaTime * vignetteTransitionSpeed
        );
    }

    // ── Chromatic Aberration — Damage Spike ─────────────────────────────────
    private void DetectDamage()
    {
        if (playerHealth.currentHealth < lastKnownHealth)
        {
            TriggerDamageEffect();
        }
        lastKnownHealth = playerHealth.currentHealth;
    }

    /// <summary>
    /// Spikes chromatic aberration. Called automatically on damage, or call manually for any big hit.
    /// </summary>
    public void TriggerDamageEffect()
    {
        currentChromaticIntensity = chromaticHitIntensity;
    }

    private void RecoverChromatic()
    {
        if (chromaticAberration == null) return;

        currentChromaticIntensity = Mathf.MoveTowards(
            currentChromaticIntensity, 0f, Time.deltaTime * chromaticRecoverySpeed);

        chromaticAberration.intensity.value = currentChromaticIntensity;
    }

    // ── Lens Distortion — Boss Slam Pulse ────────────────────────────────────
    /// <summary>
    /// Call this from a boss attack script to create a screen distortion pulse.
    /// Example: DynamicPostProcessing.instance.TriggerBossImpact();
    /// </summary>
    public void TriggerBossImpact()
    {
        currentDistortionIntensity = distortionPeakIntensity;
    }

    private void RecoverDistortion()
    {
        if (lensDistortion == null) return;

        currentDistortionIntensity = Mathf.MoveTowards(
            currentDistortionIntensity, 0f, Time.deltaTime * distortionRecoverySpeed);

        lensDistortion.intensity.value = currentDistortionIntensity;
    }
}
