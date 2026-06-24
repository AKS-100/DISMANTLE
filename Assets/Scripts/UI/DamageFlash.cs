using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plays a full-screen blood-red vignette flash when the player takes damage.
/// 
/// SETUP: 
///   1. Add a UI Canvas (Screen Space – Overlay) named "DamageFlashCanvas" to the scene.
///   2. Add a UI Image as a child — set it to stretch full screen (anchors: 0,0 to 1,1).
///   3. Set the Image color to (1, 0, 0, 0) — fully transparent red.
///   4. Assign both the Image and the player's Health component to this script in the Inspector.
///
/// This script auto-subscribes by polling the player's Health in Start().
/// No manual event wiring in the Editor is required.
/// </summary>
public class DamageFlash : MonoBehaviour
{
    public static DamageFlash instance;

    [Header("Flash Settings")]
    [Tooltip("The full-screen overlay Image used for the flash effect.")]
    public Image flashImage;

    [Tooltip("The color of the flash. Red with 0 alpha is recommended.")]
    public Color flashColor = new Color(1f, 0f, 0f, 0.45f);

    [Tooltip("How quickly the flash fades out (higher = faster).")]
    public float fadeSpeed = 4f;

    [Tooltip("How quickly the flash appears (higher = more instant).")]
    public float flashInSpeed = 20f;

    // Cached reference to player health for damage polling
    private Health playerHealth;
    private int lastKnownHealth;
    private bool isFlashing = false;
    private Coroutine flashCoroutine;

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
    }

    private void Start()
    {
        // Auto-find the player health component
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

        // Ensure the flash starts fully transparent
        if (flashImage != null)
        {
            Color startColor = flashColor;
            startColor.a = 0f;
            flashImage.color = startColor;
        }
    }

    private void Update()
    {
        if (playerHealth == null) return;

        // Poll for damage — if health dropped, trigger the flash
        if (playerHealth.currentHealth < lastKnownHealth)
        {
            TriggerFlash();
        }

        lastKnownHealth = playerHealth.currentHealth;
    }

    /// <summary>
    /// Trigger the damage flash effect. Call this from any script to show the flash.
    /// </summary>
    public void TriggerFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashRoutine());

        // Also shake the camera for extra impact
        if (CameraShake.instance != null)
        {
            CameraShake.instance.Shake(0.2f, 0.25f);
        }
    }

    private IEnumerator FlashRoutine()
    {
        if (flashImage == null) yield break;

        isFlashing = true;

        // Flash IN instantly
        flashImage.color = flashColor;

        // Fade OUT smoothly
        float alpha = flashColor.a;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            Color c = flashColor;
            c.a = Mathf.Max(alpha, 0f);
            flashImage.color = c;
            yield return null;
        }

        flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
        isFlashing = false;
        flashCoroutine = null;
    }
}
