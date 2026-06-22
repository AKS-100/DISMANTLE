using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Provides smooth micro-animations (scale and color transitions) to main menu buttons on hover and click.
/// </summary>
[RequireComponent(typeof(Button))]
public class MenuButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Animation")]
    public float hoverScale = 1.08f;
    public float clickScale = 0.95f;
    public float animationSpeed = 10f;

    [Header("Color Animation")]
    public bool changeTextColor = true;
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1f, 0.2f, 0.2f); // Vibrant red

    [Header("Audio (Optional)")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private Vector3 targetScale = Vector3.one;
    private TextMeshProUGUI buttonText;
    private Color targetColor;

    private void Awake()
    {
        targetScale = transform.localScale;
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        
        if (buttonText != null)
        {
            normalColor = buttonText.color;
        }
        targetColor = normalColor;
    }

    private void Update()
    {
        // Smoothly interpolate scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);

        // Smoothly interpolate text color if enabled
        if (changeTextColor && buttonText != null)
        {
            buttonText.color = Color.Lerp(buttonText.color, targetColor, Time.deltaTime * animationSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = new Vector3(hoverScale, hoverScale, 1f);
        if (changeTextColor) targetColor = hoverColor;

        PlaySound(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = Vector3.one;
        if (changeTextColor) targetColor = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = new Vector3(clickScale, clickScale, 1f);
        PlaySound(clickSound);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = new Vector3(hoverScale, hoverScale, 1f);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        
        // Play clip at camera position or using a temporary AudioSource
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 0.5f);
    }
}
