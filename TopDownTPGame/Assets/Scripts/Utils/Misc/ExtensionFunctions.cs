#region

using UnityEngine;

#endregion

namespace Utils.Misc
{
    public static class ExtensionFunctions
    {
        private const float FloatTolerance = 0.1f;

        public static bool IsNearlyEqual(float a, float b) => Mathf.Abs(a - b) <= FloatTolerance;

        public static bool IsNearlyEqual(float a, float b, float tolerance) => Mathf.Abs(a - b) <= tolerance;

        public static float Map(float from, float fromMin, float fromMax, float toMin, float toMax)
        {
            var fromAbs = from - fromMin;
            var fromMaxAbs = fromMax - fromMin;

            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;

            return to;
        }

        public static float To360Angle(float angle)
        {
            while (angle < 0.0f)
                angle += 360.0f;
            while (angle >= 360.0f)
                angle -= 360.0f;

            return angle;
        }

        public static float AngleDifference(float a, float b) => 180 - Mathf.Abs(Mathf.Abs(a - b) - 180);

        public static bool IsVectorAnglesNearlyEqual(Vector3 a, Vector3 b, float tolerance)
        {
            var xDiff = Mathf.Abs(AngleDifference(To360Angle(a.x), To360Angle(b.x)));
            var yDiff = Mathf.Abs(AngleDifference(To360Angle(a.y), To360Angle(b.y)));
            var zDiff = Mathf.Abs(AngleDifference(To360Angle(a.z), To360Angle(b.z)));

            bool value = xDiff <= tolerance && yDiff <= tolerance && zDiff <= tolerance;
            if (!value)
            {
                Debug.LogWarning("X: " + xDiff + ", Y: " + yDiff + ", Z: " + zDiff);
            }

            return value;
        }

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
    }
}