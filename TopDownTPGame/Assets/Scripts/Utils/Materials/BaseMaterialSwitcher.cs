#region

using UnityEngine;

#endregion

namespace Utils.Materials
{
    public abstract class BaseMaterialSwitcher : MonoBehaviour
    {
        #region Material Functions

        public abstract void SwitchMaterial(int materialIndex);

        #endregion Material Functions
    }
}