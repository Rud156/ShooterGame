#region

using UnityEngine;

#endregion

namespace Utils.Misc
{
    public static class StaticData
    {
        public const int MaxCollidersCheck = 20;
        public const int MaxUltimatePercent = 100;

        // Player Animations (Since these can be used by any character)
        // Type 1
        public static readonly int Type_1_Primary = Animator.StringToHash("Type_1UpperBodyAttack");
        public static readonly int Type_1_Secondary = Animator.StringToHash("Type_1UpperBodyAttack");
        public static readonly int Type_1_Tertiary = Animator.StringToHash("Type_1FullBodyTertiary");
        public static readonly int Type_1_TertiaryHorizontal = Animator.StringToHash("Type_1FullBodyTertiaryHorizontal");
        public static readonly int Type_1_TertiaryVertical = Animator.StringToHash("Type_1FullBodyTertiaryVertical");
        public static readonly int Type_1_Ultimate = Animator.StringToHash("Type_1UpperBodyUltimate");
    }
}