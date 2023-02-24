#region

using Player.Base;
using Player.Common;
using UnityEngine;

#endregion

namespace Player.Type_5
{
    public class Type_5_Ultimate_EngineerShield : Ability
    {
        [Header("Shield Data")]
        [SerializeField] private float _shieldDuration;

        private float _currentTimeLeft;

        #region Unity Functions

        private void OnTriggerEnter(Collider other)
        {
            // TODO: Handle this using OwnerID.
            // Every Object in the game should know who shot/created it so use OwnerId
            // Then when objects pass through filter using OwnerId and don't destroy the required GameObjects
            Destroy(other.gameObject);
        }

        #endregion Unity Functions

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _currentTimeLeft <= 0;

        public override void StartAbility(BasePlayerController playerController) => _currentTimeLeft = _shieldDuration;

        public override void AbilityUpdate(BasePlayerController playerController) => _currentTimeLeft -= Time.fixedDeltaTime;

        public override void EndAbility(BasePlayerController playerController) => Destroy(gameObject);

        #endregion Ability Functions
    }
}