#region

using Ability_Scripts.Projectiles;
using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Misc;

#endregion

namespace Player.Type_4
{
    public class Type_4_Tertiary_Satchel : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _satchelPrefab;

        [Header("Components")]
        [SerializeField] private AbilityPrefabInitializer _prefabInit;
        [SerializeField] private Transform _cameraHolder;

        [Header("Satchel Data")]
        [SerializeField] private float _satchelAffectRadius;
        [SerializeField] private float _satchelVelocity;
        [SerializeField] private float _airControlMultiplier;
        [SerializeField] private float _minForceDuration;
        [SerializeField] private float _maxForceDuration;
        [SerializeField] private float _satchelGravityMultiplier;

        [Header("Post Start Filled")]
        [SerializeField] private Transform _orbitShootPoint;
        [SerializeField] private Transform _staticShootPoint;

        private SatchelNade _satchelObject;
        private bool _abilityEnd;

        private Vector3 _computedVelocity;
        private Vector3 _direction;
        private float _satchelSpawnedDuration;

        #region Unity Functions

        protected override void Start()
        {
            base.Start();

            _prefabInit.AbilityPrefabInit();
            _orbitShootPoint = transform.Find("CameraHolder/Type_4_CameraPrefab(Clone)/BelowShootPoint");
            _staticShootPoint = transform.Find("Type_4_NormalPrefab(Clone)/BelowShootPoint");
        }

        #endregion Unity Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            if (_satchelSpawnedDuration <= 0)
            {
                InitialSatchelActivation(playerController);
            }
            else
            {
                UpdateSatchelMovement(playerController);
            }
        }

        public override void EndAbility(BasePlayerController playerController) => _abilityEnd = true;

        public override void StartAbility(BasePlayerController playerController)
        {
            _abilityEnd = false;
            _satchelSpawnedDuration = 0;
            _computedVelocity = Vector3.zero;
        }

        public override Vector3 GetMovementData() => _computedVelocity;

        #region Ability Updates

        private void InitialSatchelActivation(BasePlayerController playerController)
        {
            if (_satchelObject == null)
            {
                var direction = _cameraHolder.forward;

                var shootPosition = playerController.IsGrounded ? _staticShootPoint.position : _orbitShootPoint.position;
                var satchel = Instantiate(_satchelPrefab, shootPosition, Quaternion.identity);
                var satchelNade = satchel.GetComponent<SatchelNade>();
                satchelNade.LaunchProjectile(direction);

                _satchelObject = satchelNade;
                _abilityEnd = true;
            }
            else
            {
                var distance = Vector3.Distance(transform.position, _satchelObject.transform.position);
                DebugExtension.DebugWireSphere(_satchelObject.transform.position, _satchelAffectRadius, duration: 10);

                if (distance > _satchelAffectRadius)
                {
                    _abilityEnd = true;
                }
                else
                {
                    var mappedDuration = ExtensionFunctions.Map(distance, 0, _satchelAffectRadius, _maxForceDuration, _minForceDuration);
                    var direction = transform.position - _satchelObject.transform.position;

                    _satchelSpawnedDuration = mappedDuration;
                    _direction = direction.normalized;
                    _computedVelocity = Vector3.zero;
                }

                _satchelObject.ProjectileDestroy();
                _satchelObject = null;
            }
        }

        private void UpdateSatchelMovement(BasePlayerController playerController)
        {
            _satchelSpawnedDuration -= Time.fixedDeltaTime;

            var coreInput = playerController.GetCoreMoveInput();
            var forward = _cameraHolder.forward;
            var right = _cameraHolder.right;

            var satchelMovement = forward * coreInput.y + right * coreInput.x;
            satchelMovement.y = 0;
            satchelMovement = _airControlMultiplier * _satchelVelocity * satchelMovement.normalized;

            _computedVelocity = _direction * _satchelVelocity;
            _computedVelocity.x += satchelMovement.x;
            _computedVelocity.z += satchelMovement.z;
            _computedVelocity.y += Physics.gravity.y * _satchelGravityMultiplier;

            if (_satchelSpawnedDuration < 0)
            {
                _abilityEnd = true;
            }
        }

        #endregion Ability Updates
    }
}