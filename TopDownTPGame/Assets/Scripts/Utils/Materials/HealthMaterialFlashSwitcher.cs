#region

using System;
using System.Collections;
using System.Collections.Generic;
using HealthSystem;
using UnityEngine;

#endregion

namespace Utils.Materials
{
    public class HealthMaterialFlashSwitcher : BaseMaterialSwitcher
    {
        [Header("Components")]
        [SerializeField] private HealthAndDamage _healthAndDamage;

        [Header("Flash Data")]
        [SerializeField] private Material _flashMaterial;
        [SerializeField] private int _flashCount;
        [SerializeField] private float _flashOnDuration;
        [SerializeField] private float _flashOffDuration;

        [Header("Materials")]
        [SerializeField] private List<MaterialData> _materialData;

        private bool _flashActive;

        #region Unity Functions

        private void Start()
        {
            _healthAndDamage.OnDamageTaken += HandleDamageTaken;
            _healthAndDamage.OnHealthDecayed += HandleHealthDecayed;
        }

        private void OnDestroy()
        {
            _healthAndDamage.OnDamageTaken -= HandleDamageTaken;
            _healthAndDamage.OnHealthDecayed -= HandleHealthDecayed;
        }

        #endregion Unity Functions

        #region Utils

        private void HandleDamageTaken(int damageTaken, int startingHealth, int finalHealth) => SwitchMaterial(1);

        private void HandleHealthDecayed(int startHealth, int decayedHealth, float decayDuration) => SwitchMaterial(1);

        public override void SwitchMaterial(int materialIndex)
        {
            if (materialIndex == 1 && !_flashActive)
            {
                StartCoroutine(GameObjectMaterialFlasher());
            }
        }

        private IEnumerator GameObjectMaterialFlasher()
        {
            _flashActive = true;
            for (var i = 0; i < _flashCount; i++)
            {
                foreach (var materialData in _materialData)
                {
                    for (var matIndex = 0; matIndex < materialData.gameObjectMaterials.Count; matIndex++)
                    {
                        materialData.gameObjectRenderer.material = _flashMaterial;
                    }
                }

                yield return new WaitForSeconds(_flashOnDuration);

                foreach (var materialData in _materialData)
                {
                    materialData.gameObjectRenderer.materials = materialData.gameObjectMaterials.ToArray();
                }

                yield return new WaitForSeconds(_flashOffDuration);
            }

            _flashActive = false;
        }

        #endregion Utils

        #region Structs

        [Serializable]
        public struct MaterialData
        {
            public Renderer gameObjectRenderer;
            public List<Material> gameObjectMaterials;
        }

        #endregion Structs
    }
}