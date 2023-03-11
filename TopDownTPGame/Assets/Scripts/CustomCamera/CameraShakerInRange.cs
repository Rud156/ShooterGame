#region

using UnityEngine;

#endregion

namespace CustomCamera
{
    [CreateAssetMenu(fileName = "CameraShakeData", menuName = "CustomCamera/Shake In Range", order = 0)]
    public class CameraShakerInRange : ScriptableObject
    {
        public float duration;
        public float range;

        [Header("Amplitude")]
        public float minAmplitude;
        public float maxAmplitude;

        [Header("Frequency")]
        public float minFrequency;
        public float maxFrequency;
    }
}