using AbilityScripts.Projectiles;
using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Misc;

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

        private Transform _shootPoint;

        private SatchelNade _satchelObject;
        private bool _abilityEnd;

        private Vector3 _computedVelocity;
        private Vector3 _direction;
        private float _duration;

        #region Unity Functions

        private void Start()
        {
            _prefabInit.AbilityPrefabInit();
            _shootPoint = transform.Find("Type_4_Prefab(Clone)/BelowShootPoint");
        }

        #endregion

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            if (_duration <= 0)
            {
                InitialSatchelActivation();
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
            _duration = 0;
            _computedVelocity = Vector3.zero;
        }

        public override Vector3 GetMovementData() => _computedVelocity;

        #region Ability Updates

        private void InitialSatchelActivation()
        {
            if (_satchelObject == null)
            {
                Vector3 direction = _cameraHolder.forward;

                GameObject satchel = Instantiate(_satchelPrefab, _shootPoint.position, Quaternion.identity);
                SatchelNade satchelNade = satchel.GetComponent<SatchelNade>();
                satchelNade.LaunchProjectile(direction);

                _satchelObject = satchelNade;
                _abilityEnd = true;
            }
            else
            {
                float distance = Vector3.Distance(transform.position, _satchelObject.transform.position);
                DebugExtension.DebugWireSphere(_satchelObject.transform.position, _satchelAffectRadius, duration: 10);

                if (distance > _satchelAffectRadius)
                {
                    _abilityEnd = true;
                }
                else
                {
                    float mappedDuration = ExtensionFunctions.Map(distance, 0, _satchelAffectRadius, _maxForceDuration, _minForceDuration);
                    Vector3 direction = transform.position - _satchelObject.transform.position;

                    _duration = mappedDuration;
                    _direction = direction.normalized;
                    _computedVelocity = Vector3.zero;
                }

                _satchelObject.ProjectileDestroy();
                _satchelObject = null;
            }
        }

        private void UpdateSatchelMovement(BasePlayerController playerController)
        {
            _duration -= Time.fixedDeltaTime;

            Vector2 coreInput = playerController.GetCoreMoveInput();
            Vector3 forward = _cameraHolder.forward;
            Vector3 right = _cameraHolder.right;

            Vector3 satchelMovement = forward * coreInput.y + right * coreInput.x;
            satchelMovement.y = 0;
            satchelMovement = _airControlMultiplier * _satchelVelocity * satchelMovement.normalized;

            _computedVelocity = _direction * _satchelVelocity;
            _computedVelocity.x += satchelMovement.x;
            _computedVelocity.z += satchelMovement.z;
            _computedVelocity.y += Physics.gravity.y * _satchelGravityMultiplier;

            if (_duration < 0)
            {
                _abilityEnd = true;
            }
        }

        #endregion Ability Updates
    }
}