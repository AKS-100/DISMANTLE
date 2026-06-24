using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dynamic crosshair that expands on shoot, shrinks when still, and turns red when aimed at an enemy.
///
/// SETUP IN UNITY EDITOR:
///   1. Create a new empty child under your HUD Canvas — name it "Crosshair".
///   2. Set its RectTransform anchors to center (0.5, 0.5) and position to (0, 0).
///   3. Create 4 child Image objects — name them "Top", "Bottom", "Left", "Right".
///   4. For each, add a UI Image component (use a small white 4x16 pixel sprite or the default UI sprite).
///       - Top:    Width=4, Height=16, Position=(0, 12, 0)
///       - Bottom: Width=4, Height=16, Position=(0,-12, 0)
///       - Left:   Width=16, Height=4, Position=(-12, 0, 0)
///       - Right:  Width=16, Height=4, Position=( 12, 0, 0)
///   5. Assign the 4 Image references and the player camera in the Inspector.
///
/// The crosshair will:
///   - Expand when the player moves or shoots
///   - Shrink back to normal when still
///   - Turn the hostileColor when the center raycast hits an enemy collider
/// </summary>
public class DynamicCrosshair : MonoBehaviour
{
    [Header("Crosshair Arms — assign all 4")]
    public RectTransform armTop;
    public RectTransform armBottom;
    public RectTransform armLeft;
    public RectTransform armRight;

    [Header("Spread Settings")]
    [Tooltip("Gap between center and each arm at rest (pixels).")]
    public float restSpread    = 10f;

    [Tooltip("Max additional spread when shooting or moving.")]
    public float maxSpread     = 30f;

    [Tooltip("How quickly the crosshair expands (higher = snappier).")]
    public float expandSpeed   = 25f;

    [Tooltip("How quickly the crosshair contracts back to rest.")]
    public float contractSpeed = 8f;

    [Header("Color Settings")]
    public Color neutralColor  = Color.white;
    public Color hostileColor  = new Color(1f, 0.15f, 0.15f, 1f);  // bright red

    [Header("Enemy Detection")]
    [Tooltip("The player camera used for the center raycast.")]
    public Camera playerCamera;

    [Tooltip("Max distance to detect an enemy.")]
    public float enemyDetectRange = 50f;

    [Tooltip("Layer mask for enemies (set to your Enemy layer).")]
    public LayerMask enemyLayers = ~0; // default: all layers

    [Tooltip("Tag used for enemy GameObjects.")]
    public string enemyTag = "Enemy";

    // Current spread value (lerped each frame)
    private float currentSpread;

    // Target spread this frame
    private float targetSpread;

    // Whether the player recently fired
    private float shootSpreadTimer = 0f;

    [Tooltip("How many seconds after shooting until spread begins contracting.")]
    public float spreadDecayDelay = 0.1f;

    private Image[] allArms;

    private void Start()
    {
        currentSpread = restSpread;
        targetSpread  = restSpread;

        allArms = new Image[]
        {
            armTop?.GetComponent<Image>(),
            armBottom?.GetComponent<Image>(),
            armLeft?.GetComponent<Image>(),
            armRight?.GetComponent<Image>()
        };

        // Auto-find camera if not assigned
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        ApplySpread(currentSpread);
    }

    private void Update()
    {
        HandleSpread();
        HandleEnemyDetection();
    }

    // ── Spread ───────────────────────────────────────────────────────────────
    private void HandleSpread()
    {
        // Check if player is moving
        bool isMoving = false;
        if (GameManager.instance?.player != null)
        {
            Vector3 vel = Vector3.zero;
            CharacterController cc = GameManager.instance.player.GetComponent<CharacterController>();
            if (cc != null) vel = cc.velocity;
            isMoving = vel.magnitude > 0.5f;
        }

        // Decay shoot timer
        if (shootSpreadTimer > 0f)
        {
            shootSpreadTimer -= Time.deltaTime;
        }

        bool spreading = isMoving || shootSpreadTimer > 0f;

        targetSpread = spreading ? maxSpread : restSpread;

        float lerpSpeed = spreading ? expandSpeed : contractSpeed;
        currentSpread = Mathf.Lerp(currentSpread, targetSpread, Time.deltaTime * lerpSpeed);

        ApplySpread(currentSpread);
    }

    private void ApplySpread(float spread)
    {
        if (armTop    != null) armTop.anchoredPosition    = new Vector2(0f,  spread);
        if (armBottom != null) armBottom.anchoredPosition = new Vector2(0f, -spread);
        if (armLeft   != null) armLeft.anchoredPosition   = new Vector2(-spread, 0f);
        if (armRight  != null) armRight.anchoredPosition  = new Vector2( spread, 0f);
    }

    // ── Enemy Detection ───────────────────────────────────────────────────────
    private void HandleEnemyDetection()
    {
        if (playerCamera == null) return;

        bool aimingAtEnemy = false;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, enemyDetectRange, enemyLayers))
        {
            if (hit.collider.CompareTag(enemyTag))
            {
                aimingAtEnemy = true;
            }
        }

        Color targetColor = aimingAtEnemy ? hostileColor : neutralColor;
        foreach (Image arm in allArms)
        {
            if (arm != null)
            {
                arm.color = Color.Lerp(arm.color, targetColor, Time.deltaTime * 12f);
            }
        }
    }

    /// <summary>
    /// Call this when the player fires to expand the crosshair.
    /// Hook this up to your Gun script's fire event or call it manually.
    /// </summary>
    public void OnPlayerShoot()
    {
        shootSpreadTimer = spreadDecayDelay + 0.15f;
    }

    /// <summary>
    /// Singleton-style static access for easy calling from Gun scripts.
    /// Example: DynamicCrosshair.TriggerShootSpread();
    /// </summary>
    public static DynamicCrosshair instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
    }

    public static void TriggerShootSpread()
    {
        if (instance != null)
        {
            instance.OnPlayerShoot();
        }
    }
}
