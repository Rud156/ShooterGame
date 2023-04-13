#region

using System.Runtime.CompilerServices;
using UnityEngine;

#endregion

namespace Utils.Misc
{
    public static class ExtensionFunctions
    {
        private const float FloatTolerance = 0.1f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyEqual(float a, float b) => Mathf.Abs(a - b) <= FloatTolerance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyEqual(float a, float b, float tolerance) => Mathf.Abs(a - b) <= tolerance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Map(float from, float fromMin, float fromMax, float toMin, float toMax)
        {
            if (IsNearlyEqual(fromMin, fromMax))
                return toMax;

            var fromAbs = from - fromMin;
            var fromMaxAbs = fromMax - fromMin;

            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;

            return to;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float To360Angle(float angle)
        {
            while (angle < 0.0f)
                angle += 360.0f;
            while (angle >= 360.0f)
                angle -= 360.0f;

            return angle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleDifference(float a, float b) => 180 - Mathf.Abs(Mathf.Abs(a - b) - 180);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsVectorAnglesNearlyEqual(Vector3 a, Vector3 b, float tolerance)
        {
            var xDiff = Mathf.Abs(AngleDifference(To360Angle(a.x), To360Angle(b.x)));
            var yDiff = Mathf.Abs(AngleDifference(To360Angle(a.y), To360Angle(b.y)));
            var zDiff = Mathf.Abs(AngleDifference(To360Angle(a.z), To360Angle(b.z)));

            var value = xDiff <= tolerance && yDiff <= tolerance && zDiff <= tolerance;
            if (!value)
            {
                Debug.LogWarning("X: " + xDiff + ", Y: " + yDiff + ", Z: " + zDiff);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetRandomPointInsideCollider(BoxCollider boxCollider)
        {
            var extents = boxCollider.size / 2f;
            Vector3 point = new(
                Random.Range(-extents.x, extents.x),
                Random.Range(-extents.y, extents.y),
                Random.Range(-extents.z, extents.z)
            );

            return boxCollider.transform.TransformPoint(point);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 AverageColorFromTexture(Texture2D tex)
        {
            var texColors = tex.GetPixels32();
            var total = texColors.Length;

            float r = 0;
            float g = 0;
            float b = 0;

            for (var i = 0; i < total; i++)
            {
                r += texColors[i].r;
                g += texColors[i].g;
                b += texColors[i].b;
            }

            return new Color32((byte)(r / total), (byte)(g / total), (byte)(b / total), 255);
        }
    }
}