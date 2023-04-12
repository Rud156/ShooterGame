#region

using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Common;

#endregion

namespace Player.Type_5
{
    public class Type_5_Secondary_Stun : Ability
    {
        [Header("Stun Data")]
        [SerializeField] private float _stunDuration;

        private float _currentTimeLeft;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _currentTimeLeft <= 0;

        public override void StartAbility(BasePlayerController playerController) => _currentTimeLeft = _stunDuration;

        public override void AbilityUpdate(BasePlayerController playerController) => _currentTimeLeft -= GlobalStaticData.FixedUpdateTime;

        public override void EndAbility(BasePlayerController playerController) => Destroy(gameObject);

        #endregion Ability Functions

        #region Specific Data

        public override Vector3 GetMovementData() => Vector3.zero;

        #endregion Specific Data
    }
}