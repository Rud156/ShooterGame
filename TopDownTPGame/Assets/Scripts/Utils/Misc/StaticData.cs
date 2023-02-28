#region

using UnityEngine;

#endregion

namespace Utils.Misc
{
    public static class StaticData
    {
        public const int MaxCollidersCheck = 20;
        public const int MaxUltimatePercent = 100;

        // Animations (Since these can be used by any character)
        // Type 1
        public static readonly int Type_1UpperBodyTrigger = Animator.StringToHash("Type_1UpperBodyTrigger");
    }
}