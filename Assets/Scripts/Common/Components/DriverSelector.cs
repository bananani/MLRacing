using Common.Identifiers;
using UnityEngine;

namespace Common.Components
{
    [RequireComponent(typeof(Car))]
    public class DriverSelector : MonoBehaviour
    {
        [SerializeField]
        private DriverType _driverType;

        private DriverType _currentDriverType = DriverType.NONE;
        private Driver _currentDriverComponent;

        private void Awake()
        {
            UpdateDriverType();
        }

        private void Update()
        {
            if(_currentDriverType != _driverType)
            {
                UpdateDriverType();
            }
        }

        private void UpdateDriverType()
        {
            Object.Destroy(_currentDriverComponent);
            _currentDriverType = _driverType;

            switch(_currentDriverType)
            {
                case DriverType.NONE:
                    _currentDriverComponent = gameObject.AddComponent<NullDriver>();
                    break;
                case DriverType.PLAYER:
                    _currentDriverComponent = gameObject.AddComponent<DebugPlayer>();
                    break;
                default:
                    break;
            }
        }
    }
}