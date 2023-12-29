#region

using UnityEngine;

#endregion

namespace Utils.Materials
{
    public class Type_4_TurretMaterialSwitcher : BaseMaterialSwitcher
    {
        [Header("Turret Materials")]
        [SerializeField] private Material _turretPlacedMaterial;
        [SerializeField] private Material _canPlaceMaterial;
        [SerializeField] private Material _cannotPlaceMaterial;

        [Header("Turret Parts")]
        [SerializeField] private MeshRenderer _turretTop;
        [SerializeField] private MeshRenderer _turretBottom;

        #region Material Functions

        public override void SwitchMaterial(int materialIndex)
        {
            switch (materialIndex)
            {
                case 1:
                    {
                        _turretTop.material = _turretPlacedMaterial;
                        _turretBottom.material = _turretPlacedMaterial;
                    }
                    break;

                case 2:
                    {
                        _turretTop.material = _canPlaceMaterial;
                        _turretBottom.material = _canPlaceMaterial;
                    }
                    break;

                case 3:
                    {
                        _turretTop.material = _cannotPlaceMaterial;
                        _turretBottom.material = _cannotPlaceMaterial;
                    }
                    break;
            }
        }

        #endregion Material Functions
    }
}