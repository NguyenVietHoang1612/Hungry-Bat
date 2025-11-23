using UnityEngine;
using UnityEngine.UIElements;

public static class UIToWorldHelper
{
    public static Vector3 UIElementToWorld(VisualElement ve, float zOffset)
    {
        Vector2 panelPos = ve.worldBound.center;

        IPanel panel = ve.panel;
        if (panel == null) return Vector3.zero;


        float scale = panel.scaledPixelsPerPoint;
        Vector2 screenPos = panelPos * scale;

        return Camera.main.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane + zOffset)
        );
    }
}
