using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public class PortraitFit : MonoBehaviour
{
    const float TARGET = 9f / 16f;   // Portrait 9:16

    RectTransform rt;           // GameRoot
    RectTransform parentRT;     // SafeArea (parent)
    Vector2 lastParentSize;

    void OnEnable()
    {
        rt = transform as RectTransform;
        parentRT = transform.parent as RectTransform;  // <<< key: we compute from SafeArea
        Apply();
    }

    void OnTransformParentChanged()  // in case it gets re-parented in the hierarchy
    {
        parentRT = transform.parent as RectTransform;
        Apply();
    }

    void OnRectTransformDimensionsChange()  // editor and runtime SafeArea changes
    {
        Apply();
    }

    void Update()  // fallback for WebGL/editor
    {
        if (!parentRT) return;
        Vector2 parentSize = parentRT.rect.size;
        if (parentSize != lastParentSize)
            Apply();
    }

    void Apply()
    {
        if (!rt || !parentRT) return;

        Vector2 parentSize = parentRT.rect.size;
        if (parentSize.x <= 0f || parentSize.y <= 0f) return;

        lastParentSize = parentSize;

        float pw = parentSize.x;
        float ph = parentSize.y;
        float current = pw / ph;

        float boxW, boxH;
        if (current > TARGET)
        {
            // Parent is relatively wider → height limits size
            boxH = ph;
            boxW = ph * TARGET;
        }
        else
        {
            // Parent is relatively narrower → width limits size
            boxW = pw;
            boxH = pw / TARGET;
        }

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); // middle-center
        rt.pivot = new Vector2(0.5f, 0.5f);

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, boxW);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   boxH);
        rt.anchoredPosition = Vector2.zero; // centered
    }
}
