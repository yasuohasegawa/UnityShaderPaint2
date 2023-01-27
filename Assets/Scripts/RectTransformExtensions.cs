using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnchorPresets
{
    TopLeft,
    TopCenter,
    TopRight,

    MiddleLeft,
    MiddleCenter,
    MiddleRight,

    BottomLeft,
    BottonCenter,
    BottomRight,
    BottomStretch,

    VerticalStretchLeft,
    VerticalStretchRight,
    VerticalStretchCenter,

    HorizontalStretchTop,
    HorizontalStretchMiddle,
    HorizontalStretchBottom,

    StretchAll
}

public static class RectTransformExtensions
{
    public static AnchorPresets GetAnchorPresets(this RectTransform source)
    {
        var apresets = AnchorPresets.TopLeft;
        if (source.anchorMin.x == 0 && source.anchorMin.y == 1 && source.anchorMax.x == 0 && source.anchorMax.y == 1)
        {
            apresets = AnchorPresets.TopLeft;
        } else if(source.anchorMin.x == 0.5f && source.anchorMin.y == 1 && source.anchorMax.x == 0.5f && source.anchorMax.y == 1)
        {
            apresets = AnchorPresets.TopCenter;
        }
        else if (source.anchorMin.x == 1 && source.anchorMin.y == 1 && source.anchorMax.x == 1 && source.anchorMax.y == 1)
        {
            apresets = AnchorPresets.TopRight;
        }
        else if (source.anchorMin.x == 0 && source.anchorMin.y == 0.5f && source.anchorMax.x == 0 && source.anchorMax.y == 0.5f)
        {
            apresets = AnchorPresets.MiddleLeft;
        }
        else if (source.anchorMin.x == 0.5f && source.anchorMin.y == 0.5f && source.anchorMax.x == 0.5f && source.anchorMax.y == 0.5f)
        {
            apresets = AnchorPresets.MiddleCenter;
        }
        else if (source.anchorMin.x == 1 && source.anchorMin.y == 0.5f && source.anchorMax.x == 1 && source.anchorMax.y == 0.5f)
        {
            apresets = AnchorPresets.MiddleRight;
        }
        else if (source.anchorMin.x == 0 && source.anchorMin.y == 0 && source.anchorMax.x == 0 && source.anchorMax.y == 0)
        {
            apresets = AnchorPresets.BottomLeft;
        }
        else if (source.anchorMin.x == 0.5f && source.anchorMin.y == 0 && source.anchorMax.x == 0.5f && source.anchorMax.y == 0)
        {
            apresets = AnchorPresets.BottonCenter;
        }
        else if (source.anchorMin.x == 1 && source.anchorMin.y == 0 && source.anchorMax.x == 1 && source.anchorMax.y == 0)
        {
            apresets = AnchorPresets.BottomRight;
        }
        else if (source.anchorMin.x == 0 && source.anchorMin.y == 1 && source.anchorMax.x == 1 && source.anchorMax.y == 1)
        {
            apresets = AnchorPresets.HorizontalStretchTop;
        }
        else if (source.anchorMin.x == 0 && source.anchorMin.y == 0.5f && source.anchorMax.x == 1 && source.anchorMax.y == 0.5f)
        {
            apresets = AnchorPresets.HorizontalStretchMiddle;
        }
        else if (source.anchorMin.x == 0 && source.anchorMin.y == 0 && source.anchorMax.x == 1 && source.anchorMax.y == 0)
        {
            apresets = AnchorPresets.HorizontalStretchBottom;
        }
        else if (source.anchorMin.x == 0 && source.anchorMin.y == 0 && source.anchorMax.x == 0 && source.anchorMax.y == 1)
        {
            apresets = AnchorPresets.VerticalStretchLeft;
        }
        else if (source.anchorMin.x == 0.5f && source.anchorMin.y == 0 && source.anchorMax.x == 0.5f && source.anchorMax.y == 1)
        {
            apresets = AnchorPresets.VerticalStretchCenter;
        }
        else if (source.anchorMin.x == 1 && source.anchorMin.y == 0 && source.anchorMax.x == 1 && source.anchorMax.y == 1)
        {
            apresets = AnchorPresets.VerticalStretchRight;
        }
        else if (source.anchorMin.x == 0 && source.anchorMin.y == 0 && source.anchorMax.x == 1 && source.anchorMax.y == 1)
        {
            apresets = AnchorPresets.StretchAll;
        }

        return apresets;
    }
}
