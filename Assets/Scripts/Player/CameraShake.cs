using System.Collections;
using UnityEngine;

/// <summary>
/// Singleton camera shake controller.
/// Call CameraShake.instance.Shake(intensity, duration) from any script to trigger a shake.
/// Attach this to the same GameObject as the Camera (the player's eye/camera object).
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    [Header("Shake Settings")]
    [Tooltip("Default shake intensity if none is specified.")]
    public float defaultIntensity = 0.15f;

    [Tooltip("Default shake duration in seconds.")]
    public float defaultDuration  = 0.2f;

    [Tooltip("How quickly the shake decays to zero (higher = snappier return).")]
    public float dampingSpeed = 8f;

    // The origin position of the camera before any shake offset
    private Vector3 originalLocalPosition;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        originalLocalPosition = transform.localPosition;
    }

    /// <summary>
    /// Trigger a camera shake with custom intensity and duration.
    /// </summary>
    /// <param name="intensity">Max displacement in units (e.g. 0.1 – 0.5)</param>
    /// <param name="duration">How long the shake lasts in seconds</param>
    public void Shake(float intensity, float duration)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ShakeRoutine(intensity, duration));
    }

    /// <summary>
    /// Trigger a camera shake with the default settings.
    /// </summary>
    public void Shake()
    {
        Shake(defaultIntensity, defaultDuration);
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Fade intensity over the duration for a natural decay feel
            float currentIntensity = Mathf.Lerp(intensity, 0f, elapsed / duration);

            // Random offset in local space
            float offsetX = Random.Range(-1f, 1f) * currentIntensity;
            float offsetY = Random.Range(-1f, 1f) * currentIntensity;

            transform.localPosition = originalLocalPosition + new Vector3(offsetX, offsetY, 0f);

            yield return null;
        }

        // Smoothly restore original position
        while (Vector3.Distance(transform.localPosition, originalLocalPosition) > 0.001f)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                originalLocalPosition,
                Time.deltaTime * dampingSpeed
            );
            yield return null;
        }

        transform.localPosition = originalLocalPosition;
        shakeCoroutine = null;
    }
}
