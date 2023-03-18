#region

using Effects;
using Player.Base;
using Player.Common;
using UnityEngine;

#endregion

namespace Player.Type_3
{
    public class Type_3_Ultimate_DarkPulseParanoiaEffect : Ability
    {
        [Header("Components")]
        [SerializeField] private DestroyParticleEffectSlowlyEmission _destroyParticleEffect;

        [Header("Paranoia Data")]
        [SerializeField] private float _paranoiaDuration;

        private float _currentTimeLeft;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _currentTimeLeft <= 0;

        public override void StartAbility(BasePlayerController playerController) => _currentTimeLeft = _paranoiaDuration;

        public override void AbilityUpdate(BasePlayerController playerController) => _currentTimeLeft -= Time.fixedDeltaTime;

        public override void EndAbility(BasePlayerController playerController)
        {
            _destroyParticleEffect.DestroyEffect();
            Destroy(gameObject);
        }

        #endregion Ability Functions
    }
}