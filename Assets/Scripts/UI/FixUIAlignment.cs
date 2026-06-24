using UnityEngine;
using TMPro;

/// <summary>
/// Automatically corrects UI button positioning, anchors, pivots, 
/// and text alignments to ensure they remain perfectly centered at all resolutions.
/// </summary>
[ExecuteAlways]
public class FixUIAlignment : MonoBehaviour
{
    private void Start()
    {
        FixLayout();
    }

    private void LateUpdate()
    {
        FixLayout();
    }

    public void FixLayout()
    {
        // Force Vertical Layout Group properties on this object if it has one
        UnityEngine.UI.VerticalLayoutGroup verticalGroup = GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        if (verticalGroup != null)
        {
            verticalGroup.childControlWidth = true;
            verticalGroup.childForceExpandWidth = true;
            verticalGroup.childAlignment = TextAnchor.UpperCenter;
        }

        // Grab all child RectTransforms
        RectTransform[] children = GetComponentsInChildren<RectTransform>(true);
        foreach (RectTransform child in children)
        {
            if (child == transform) continue;

            // Target buttons in the hierarchy (e.g. Level1Button, Level2Button, BackButton)
            if (child.name.Contains("Button") || child.name.Contains("Butt"))
            {
                // Force button RectTransform pivot to be centered
                child.pivot = new Vector2(0.5f, 0.5f);

                // If not controlled by a vertical layout group, force center anchors and position
                if (verticalGroup == null || child.parent != transform)
                {
                    child.anchorMin = new Vector2(0.5f, 0.5f);
                    child.anchorMax = new Vector2(0.5f, 0.5f);

                    Vector3 pos = child.anchoredPosition3D;
                    if (Mathf.Abs(pos.x) > 0.001f)
                    {
                        pos.x = 0f;
                        child.anchoredPosition3D = pos;
                    }
                }

                // Correct the TMPro Text child component
                TextMeshProUGUI text = child.GetComponentInChildren<TextMeshProUGUI>(true);
                if (text != null)
                {
                    RectTransform textRect = text.rectTransform;
                    
                    // Force text bounding box to stretch and fill the button area completely
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.anchoredPosition = Vector2.zero;
                    textRect.sizeDelta = Vector2.zero;
                    textRect.pivot = new Vector2(0.5f, 0.5f);

                    // Force the font alignment to Center (both horizontal and vertical)
                    text.alignment = TextAlignmentOptions.Center;
                }
            }
        }
    }
}
