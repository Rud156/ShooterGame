using Player.Base;
using Player.Common;
using UnityEngine;
using Utils.Input;

namespace Player.Type_2
{
    public class Type_2_Tertiary_WaterMovement : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _waterMovementPrefab;

        [Header("Spawn Data")]
        [SerializeField] private GameObject _playerMainMesh;
        [SerializeField] private Transform _spawnParent;
        [SerializeField] private Vector3 _spawnOffset;
        [SerializeField] private float _rbModifiedHeight;
        [SerializeField] private float _rbModifiedYOffset;

        [Header("Water Data")]
        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _abilityDuration;

        private float _currentDuration;
        private Vector3 _computedVelocity;
        private GameObject _customMeshEffect;

        private Vector3 _originalCenterOffset;
        private float _originalHeight;

        // This is added since the Ability Update is called in the same frame that the Ability is Started
        // So to handle the key a second time a delay needed to be added for 1 frame
        private bool _abilityUpdatedOnce;

        public override bool AbilityCanStart(BasePlayerController playerController) => playerController.IsGrounded;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _currentDuration <= 0;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            var forward = transform.forward;
            var right = transform.right;

            var coreInput = playerController.GetCoreMoveInput();
            var movement = forward * coreInput.y + right * coreInput.x;
            movement = _movementSpeed * movement.normalized;
            movement.y = 0;

            _computedVelocity = movement;
            _currentDuration -= Time.fixedDeltaTime;

            var key = playerController.GetTertiaryAbilityKey();
            if (key.KeyPressedThisFrame && _abilityUpdatedOnce)
            {
                _currentDuration = 0;
            }

            _abilityUpdatedOnce = true;
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            var characterController = playerController.GetComponent<CharacterController>();
            characterController.height = _originalHeight;
            characterController.center = _originalCenterOffset;

            _playerMainMesh.SetActive(true);
            Destroy(_customMeshEffect);
            _customMeshEffect = null;
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            _currentDuration = _abilityDuration;
            _abilityUpdatedOnce = false;

            var characterController = playerController.GetComponent<CharacterController>();
            _originalHeight = characterController.height;
            _originalCenterOffset = characterController.center;

            characterController.height = _rbModifiedHeight;
            characterController.center = new Vector3(0, _rbModifiedYOffset, 0);

            _playerMainMesh.SetActive(false);
            var customMeshEffect = Instantiate(_waterMovementPrefab, transform.position, Quaternion.identity);
            customMeshEffect.transform.SetParent(_spawnParent);
            customMeshEffect.transform.localPosition = customMeshEffect.transform.localPosition + _spawnOffset;
            _customMeshEffect = customMeshEffect;
        }

        public override Vector3 GetMovementData() => _computedVelocity;
    }
}