#region

using Player.Base;
using Player.Common;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

#endregion

namespace Player.Type_3
{
    public class Type_3_Secondary_DarkSlash : Ability
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _slashPrefab;

        [Header("Components")]
        [SerializeField] private AbilityPrefabInitializer _prefabInit;

        [Header("Slash Data")]
        [SerializeField] private AnimationCurve _slashEaseCurve;
        [SerializeField] private float _slashDuration;

        [Header("Post Start Filled")]
        [SerializeField] private SplineContainer _slash;

        private GameObject _slashObject;
        private int _randomSlashIndex;
        private float _currentTime;
        private bool _abilityEnd;

        #region Ability Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => base.AbilityCanStart(playerController) && _currentCooldownDuration <= 0;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            Assert.IsNotNull(_slashObject);

            var percent = _currentTime / _slashDuration;
            var mappedPercent = _slashEaseCurve.Evaluate(percent);
            Vector3 position = _slash.EvaluatePosition(_randomSlashIndex, mappedPercent);
            Vector3 rotation = _slash.EvaluateTangent(_randomSlashIndex, mappedPercent);
            _slashObject.transform.position = position;
            _slashObject.transform.rotation = Quaternion.LookRotation(rotation);

            _currentTime += Time.fixedDeltaTime;
            if (_currentTime >= _slashDuration)
            {
                Destroy(_slashObject);
                _slashObject = null;
                _abilityEnd = true;
                _currentCooldownDuration = _cooldownDuration;
            }
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            _abilityEnd = true;
            _currentTime = 0;
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            var totalSplines = _slash.Splines.Count;
            var randomIndex = Random.Range(0, totalSplines);
            Vector3 spawnPosition = _slash.EvaluatePosition(randomIndex, 0);
            var projectile = Instantiate(_slashPrefab, spawnPosition, Quaternion.identity);

            _slashObject = projectile;
            _randomSlashIndex = randomIndex;
            _currentTime = 0;
            _abilityEnd = false;
        }

        #endregion Ability Functions

        #region Unity Functions

        public override void UnityStartDelegate(BasePlayerController playerController)
        {
            base.UnityStartDelegate(playerController);

            _prefabInit.AbilityPrefabInit();
            _slash = transform.Find("Type_3_NormalPrefab(Clone)/SlashPaths/LeftSlash").GetComponent<SplineContainer>();
        }

        #endregion Unity Functions
    }
}