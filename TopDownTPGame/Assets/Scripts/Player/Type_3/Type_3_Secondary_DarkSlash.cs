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

        #region Unity Functions

        public override void UnityStartDelegate(BasePlayerController playerController)
        {
            base.UnityStartDelegate(playerController);

            _prefabInit.AbilityPrefabInit();
            _slash = transform.Find("Type_3_NormalPrefab(Clone)/SlashPaths/Slash").GetComponent<SplineContainer>();
        }

        #endregion Unity Functions

        public override bool AbilityCanStart(BasePlayerController playerController) => true;

        public override bool AbilityNeedsToEnd(BasePlayerController playerController) => _abilityEnd;

        public override void AbilityUpdate(BasePlayerController playerController)
        {
            Assert.IsNotNull(_slashObject);

            float percent = _currentTime / _slashDuration;
            float mappedPercent = _slashEaseCurve.Evaluate(percent);
            Vector3 position = _slash.EvaluatePosition(_randomSlashIndex, mappedPercent);
            _slashObject.transform.position = position;

            _currentTime += Time.fixedDeltaTime;
            if (_currentTime >= _slashDuration)
            {
                Destroy(_slashObject);
                _slashObject = null;
                _abilityEnd = true;
            }
        }

        public override void EndAbility(BasePlayerController playerController)
        {
            _abilityEnd = true;
            _currentTime = 0;
        }

        public override void StartAbility(BasePlayerController playerController)
        {
            int totalSplines = _slash.Splines.Count;
            int randomIndex = Random.Range(0, totalSplines);
            Vector3 spawnPosition = _slash.EvaluatePosition(randomIndex, 0);
            GameObject projectile = Instantiate(_slashPrefab, spawnPosition, Quaternion.identity);

            _slashObject = projectile;
            _randomSlashIndex = randomIndex;
            _currentTime = 0;
            _abilityEnd = false;
        }
    }
}