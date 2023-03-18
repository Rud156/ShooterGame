﻿#region

using UnityEngine;

#endregion

namespace Player.Common
{
    public static class PlayerStaticData
    {
        public const int MaxCollidersCheck = 100;
        public const int MaxUltimatePercent = 100;

        // Player Animations (Since these can be used by any character)
        // Type 1
        public static readonly int Type_1_Primary = Animator.StringToHash("Type_1UpperBodyAttack");
        public static readonly int Type_1_Secondary = Animator.StringToHash("Type_1UpperBodyAttack");
        public static readonly int Type_1_Tertiary = Animator.StringToHash("Type_1FullBodyTertiary");
        public static readonly int Type_1_TertiaryHorizontal = Animator.StringToHash("Type_1FullBodyTertiaryHorizontal");
        public static readonly int Type_1_TertiaryVertical = Animator.StringToHash("Type_1FullBodyTertiaryVertical");
        public static readonly int Type_1_Ultimate = Animator.StringToHash("Type_1UpperBodyUltimate");

        // Type 2
        public static readonly int Type_2_Primary = Animator.StringToHash("Type_2UpperBodyAttack");
        public static readonly int Type_2_PrimaryFront = Animator.StringToHash("Type_2UpperBodyFrontAttack");
        public static readonly int Type_2_Secondary = Animator.StringToHash("Type_2UpperBodySecondary");
        public static readonly int Type_2_Tertiary = Animator.StringToHash("Type_2FullBodyTertiary");
        public static readonly int Type_2_Ultimate = Animator.StringToHash("Type_2UpperBodyUltimate");

        //Type 3
        public static readonly int Type_3_Primary = Animator.StringToHash("Type_3UpperBodyPrimary");
        public static readonly int Type_3_Secondary = Animator.StringToHash("Type_3UpperBodySecondary");
        public static readonly int Type_3_Tertiary = Animator.StringToHash("Type_3FullBodyTertiary");
        public static readonly int Type_3_TertiaryHorizontal = Animator.StringToHash("Type_3FullBodyTertiaryHorizontal");
        public static readonly int Type_3_TertiaryVertical = Animator.StringToHash("Type_3FullBodyTertiaryVertical");
        public static readonly int Type_3_Ultimate = Animator.StringToHash("Type_3UpperBodyUltimate");
    }
}