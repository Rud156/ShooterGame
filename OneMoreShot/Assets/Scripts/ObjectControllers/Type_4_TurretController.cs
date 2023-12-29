using Player.Misc;
using System;
using UnityEngine;

namespace ObjectControllers
{
    public class Type_4_TurretController : MonoBehaviour
    {
        private static readonly int AlphaClip = Shader.PropertyToID("_AlphaClip");
        private const float OneSecond = 1;

        [Header("Components")]
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private Collider _turretCollider;
        [SerializeField] private Transform _turretTop;
        [SerializeField] private GameObject _turretRingParticle;

        [Header("Laser")]
        [SerializeField] private GameObject _laserLineGameObject;
        [SerializeField] private GameObject _laserStartEnd;
        [SerializeField] private GameObject _laserTargetEnd;
        [SerializeField] private LineRenderer _laserLine;

        [Header("Turret Meshes")]
        [SerializeField] private Renderer _turretTopMesh;
        [SerializeField] private Renderer _turretBottomMesh;

        [Header("Turret State Data")]
        [SerializeField] private float _activationAlphaChangeRate;
        [SerializeField] private float _destroyAlphaChangeRate;

        [Header("Turret Targeting Data")]
        [SerializeField] private float _targetingRadius;
        [SerializeField] private LayerMask _targetingMask;
        [SerializeField] private float _stayOnTargetMinDuration;
        [SerializeField][Range(0, 1)] private float _targetLowestHealthRatio;

        [Header("Shooting Data")]
        [SerializeField] private float _windupTime;
        [SerializeField] private int _damagePerSec;

        [Header("Debug")]
        [SerializeField] private bool _debugIsActive;
        [SerializeField] private float _debugDisplayDuration;

        private Collider[] _hitColliders = new Collider[PlayerStaticData.MaxCollidersCheck];

        [SerializeField] private TurretState _turretState;
        [SerializeField] private TurretTargetingState _turretTargetingState;

        private float _floatData1;
        private float _floatData2;

        private Material _turretTopMaterial;
        private Material _turretBottomMaterial;
        private Transform _currentTarget;

        #region Unity Functions

        private void Start()
        {
            SetTurretState(TurretState.InActive);
            SetTurretTargetingState(TurretTargetingState.None);
        }

        private void Update()
        {
            _turretCollider.enabled = false;

            switch (_turretState)
            {
                case TurretState.InActive:
                    break;

                case TurretState.Activating:
                    UpdateActivatingState();
                    break;

                case TurretState.Idle:
                    UpdateIdleState();
                    break;

                case TurretState.WindUp:
                    UpdateWindUpState();
                    break;

                case TurretState.Targeting:
                    UpdateTargetingState();
                    break;

                case TurretState.Destroy:
                    UpdateDestroyState();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            _turretCollider.enabled = true;
        }

        #endregion Unity Functions

        #region Turret State

        #region State Updates

        private void UpdateActivatingState()
        {
        }

        private void UpdateIdleState()
        {
        }

        private void UpdateWindUpState()
        {
        }

        private void UpdateTargetingState()
        {
        }

        private void UpdateDestroyState()
        {
        }

        #endregion State Updates

        private void SetTurretState(TurretState turretState)
        {
            _turretState = turretState;
            if (_laserStartEnd != null) // Since this function is also called Indirectly when the Player gets destroyed it causes a Unity Error
            {
                _laserStartEnd.SetActive(turretState is TurretState.WindUp or TurretState.Targeting);
            }
        }

        private void SetTurretTargetingState(TurretTargetingState turretTargetingState)
        {
            _turretTargetingState = turretTargetingState;

            if (_laserLineGameObject != null && _laserTargetEnd != null)
            {
                if (turretTargetingState == TurretTargetingState.TrackAndDamageTarget)
                {
                    _laserLineGameObject.SetActive(true);
                    _laserTargetEnd.SetActive(true);
                }
                else
                {
                    _laserLineGameObject.SetActive(false);
                    _laserTargetEnd.SetActive(false);
                }
            }
        }

        #endregion Turret State

        #region External Functions

        public void ActivateTurret()
        {
            _floatData1 = 1;
            _turretRingParticle.SetActive(true);
            _turretTopMaterial = _turretTopMesh.material;
            _turretBottomMaterial = _turretBottomMesh.material;

            SetTurretState(TurretState.Activating);
            SetTurretTargetingState(TurretTargetingState.None);
        }

        public void DestroyTurret()
        {
            _floatData1 = 0;
            SetTurretState(TurretState.Destroy);
            SetTurretTargetingState(TurretTargetingState.None);
        }

        #endregion External Functions

        #region Enums

        private enum TurretState
        {
            InActive,
            Activating,
            Idle,
            WindUp,
            Targeting,
            Destroy,
        }

        private enum TurretTargetingState
        {
            None,
            FindTarget,
            TrackAndDamageTarget,
        }

        #endregion Enums
    }
}