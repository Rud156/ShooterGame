#region

using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Common;

#endregion

namespace Player.Type_2
{
    public class Type_2_Ultimate_WaterBubbleFrozen : Ability
    {
        [Header("Frozen Data")]
        [SerializeField] private float _frozenDuration;

        private float _currentTimeLeft;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _currentTimeLeft <= 0;

        public override void StartAbility(BasePlayerController playerController) => _currentTimeLeft = _frozenDuration;

        public override void AbilityUpdate(BasePlayerController playerController) => _currentTimeLeft -= GlobalStaticData.FixedUpdateTime;

        public override void EndAbility(BasePlayerController playerController) => Destroy(gameObject);

        #endregion Ability Functions

        #region Specific Data

        public override Vector3 GetMovementData() => Vector3.zero;

        #endregion Specific Data
    }
}