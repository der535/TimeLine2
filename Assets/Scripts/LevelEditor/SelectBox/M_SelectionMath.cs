using UnityEngine;

public static class M_SelectionMath
{
    public static bool IsInside(RectTransform target, Bounds selectionBounds)
    {
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Bounds targetBounds = new Bounds(corners[0], Vector3.zero);
        targetBounds.Encapsulate(corners[2]);
        return selectionBounds.Intersects(targetBounds);
    }
}