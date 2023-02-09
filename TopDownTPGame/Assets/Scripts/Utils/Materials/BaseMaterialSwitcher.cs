using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils.Materials
{
    public abstract class BaseMaterialSwitcher : MonoBehaviour
    {
        #region Material Functions

        public abstract void SwitchMaterial(int materialIndex);

        #endregion Material Functions
    }
}