using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Cinematic title animator — float, glow pulse, flicker, sway, and entrance.
/// Robust version: uses OnEnable so it survives UIManager page switching.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TitleAnimator : MonoBehaviour
{
    // ── Entrance ─────────────────────────────────────────────────────────────
    [Header("Entrance Slide-In")]
    [Tooltip("Slide up + fade in when the page becomes active.")]
    public bool  playEntrance         = true;
    [Tooltip("Start Y offset below origin (pixels).")]
    public float entranceSlideDistance = 55f;
    [Tooltip("Duration of the entrance (seconds).")]
    public float entranceDuration      = 0.65f;
    [Tooltip("Seconds to wait before starting entrance.")]
    public float entranceDelay         = 0.05f;

    // ── Float ─────────────────────────────────────────────────────────────────
    [Header("Floating Breathe")]
    public bool  enableFloat    = true;
    public float floatAmplitude = 6f;
    public float floatPeriod    = 2.2f;

    // ── Glow Pulse ────────────────────────────────────────────────────────────
    [Header("Glow Colour Pulse")]
    public bool  enableGlowPulse = true;
    public Color baseColour      = new Color(0.85f, 0.06f, 0.06f, 1f);
    public Color glowColour      = new Color(1f, 0.30f, 0.10f, 1f);
    public float glowPeriod      = 2.8f;

    // ── Flicker ───────────────────────────────────────────────────────────────
    [Header("Horror Flicker")]
    public bool  enableFlicker        = true;
    public float flickerInterval      = 4f;
    public float flickerVariance      = 2f;
    [Range(2, 8)]
    public int   flickerCount         = 4;
    public float flickerPulseDuration = 0.045f;
    [Range(0f, 1f)]
    public float flickerMinAlpha      = 0.05f;

    // ── Sway ──────────────────────────────────────────────────────────────────
    [Header("Rotation Sway")]
    public bool  enableSway  = true;
    public float swayAngle   = 0.7f;
    public float swayPeriod  = 1.8f;

    // ── Internals ─────────────────────────────────────────────────────────────
    private TextMeshProUGUI tmp;
    private RectTransform   rt;
    private Vector2         originPos;

    private bool      ready          = false; // true once entrance done
    private float     timeReady      = 0f;    // Time.time when ready was set
    private float     flickerTimer   = 0f;
    private float     nextFlicker    = 0f;
    private Coroutine entranceCoroutine;
    private Coroutine flickerCoroutine;

    private void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        rt  = GetComponent<RectTransform>();
    }

    // OnEnable fires every time the page is activated — perfect for UIManager
    private void OnEnable()
    {
        if (tmp == null || rt == null) return;

        ready        = false;
        flickerTimer = 0f;
        nextFlicker  = flickerInterval + Random.Range(-flickerVariance, flickerVariance);

        if (entranceCoroutine != null)
        {
            StopCoroutine(entranceCoroutine);
            entranceCoroutine = null;
        }

        // Hide immediately so there's no flash of text at wrong position
        SetAlpha(0f);

        // Start the setup coroutine which waits a frame for layout to settle
        entranceCoroutine = StartCoroutine(InitRoutine());
    }

    // Wait one frame so Unity's layout system finishes positioning the element,
    // then record the real origin and begin the entrance (or just show it).
    private IEnumerator InitRoutine()
    {
        // Wait for end of frame — layout is guaranteed to be complete after this
        yield return new WaitForEndOfFrame();

        // NOW capture the true resting position
        originPos = rt.anchoredPosition;

        if (playEntrance)
        {
            // Place below origin, invisible — ready for slide-in
            rt.anchoredPosition = originPos + new Vector2(0f, -entranceSlideDistance);
            SetAlpha(0f);
            entranceCoroutine = StartCoroutine(EntranceRoutine());
        }
        else
        {
            rt.anchoredPosition = originPos;
            tmp.color = baseColour;
            MarkReady();
            entranceCoroutine = null;
        }
    }

    private void OnDisable()
    {
        // Reset so entrance plays cleanly next time the page is shown
        ready = false;
        if (entranceCoroutine != null)
        {
            StopCoroutine(entranceCoroutine);
            entranceCoroutine = null;
        }
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }
    }

    private void Update()
    {
        if (!ready || tmp == null) return;

        float t = Time.time - timeReady; // time since entrance finished

        // ── Float ────────────────────────────────────────────────────────────
        float yOff = enableFloat
            ? Mathf.Sin(t * (Mathf.PI * 2f / floatPeriod)) * floatAmplitude
            : 0f;
        rt.anchoredPosition = originPos + new Vector2(0f, yOff);

        // ── Glow Pulse ───────────────────────────────────────────────────────
        if (enableGlowPulse)
        {
            float p  = (Mathf.Sin(t * (Mathf.PI * 2f / glowPeriod)) + 1f) * 0.5f;
            Color c  = Color.Lerp(baseColour, glowColour, p);
            c.a      = tmp.color.a; // preserve flicker alpha
            tmp.color = c;
        }

        // ── Sway ─────────────────────────────────────────────────────────────
        if (enableSway)
        {
            float angle = Mathf.Sin(t * (Mathf.PI * 2f / swayPeriod)) * swayAngle;
            rt.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        // ── Flicker ───────────────────────────────────────────────────────────
        if (enableFlicker)
        {
            flickerTimer += Time.deltaTime;
            if (flickerTimer >= nextFlicker)
            {
                flickerTimer = 0f;
                nextFlicker  = Mathf.Max(
                    flickerInterval + Random.Range(-flickerVariance, flickerVariance),
                    0.5f
                );
                if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
                flickerCoroutine = StartCoroutine(FlickerRoutine());
            }
        }
    }

    // ── Entrance coroutine ───────────────────────────────────────────────────
    private IEnumerator EntranceRoutine()
    {
        // Safety: ensure we're visible before doing anything
        // (handles edge case where this fires before first frame)
        yield return null;

        if (entranceDelay > 0f)
            yield return new WaitForSeconds(entranceDelay);

        Vector2 startPos = originPos + new Vector2(0f, -entranceSlideDistance);
        float elapsed = 0f;

        while (elapsed < entranceDuration)
        {
            elapsed += Time.deltaTime;
            float rawT = Mathf.Clamp01(elapsed / entranceDuration);

            // EaseOutBack: overshoots slightly then settles
            float easedT = EaseOutBack(rawT);

            rt.anchoredPosition = Vector2.LerpUnclamped(startPos, originPos, easedT);

            // Fade in over first 60% of duration
            float alpha = Mathf.Clamp01(rawT / 0.6f);
            SetAlpha(alpha);

            yield return null;
        }

        // Guarantee final state
        rt.anchoredPosition = originPos;
        SetAlpha(1f);
        tmp.color = baseColour;

        MarkReady();
        entranceCoroutine = null;
    }

    private void MarkReady()
    {
        ready     = true;
        timeReady = Time.time;
    }

    // ── Flicker coroutine ────────────────────────────────────────────────────
    private IEnumerator FlickerRoutine()
    {
        for (int i = 0; i < flickerCount; i++)
        {
            SetAlpha(flickerMinAlpha);
            yield return new WaitForSeconds(flickerPulseDuration);
            SetAlpha(1f);
            yield return new WaitForSeconds(flickerPulseDuration * Random.Range(0.5f, 1.8f));
        }
        SetAlpha(1f);
        flickerCoroutine = null;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private void SetAlpha(float a)
    {
        if (tmp == null) return;
        Color c = tmp.color;
        c.a = a;
        tmp.color = c;
    }

    // EaseOutBack — slight overshoot
    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
