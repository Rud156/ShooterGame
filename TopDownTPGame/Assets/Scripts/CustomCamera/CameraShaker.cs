#region

using UnityEngine;

#endregion

namespace CustomCamera
{
    [CreateAssetMenu(fileName = "CameraShakeData", menuName = "CustomCamera/Shake", order = 0)]
    public class CameraShaker : ScriptableObject
    {
        public float frequency;
        public float amplitude;
        public float duration;
    }
}