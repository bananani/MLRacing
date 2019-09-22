using Common.Components.CarParts;
using UnityEngine;

namespace Common.Components
{
    [RequireComponent(typeof(Camera))]
    public class FollowTarget : MonoBehaviour
    {
        [SerializeField]
        private Car _followTarget;
        [SerializeField]
        private bool _rotateCamera;

        private Camera _camera;
        private bool _cameraStatic;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Update()
        {
            if(_followTarget == null)
            {
                if(_cameraStatic)
                {
                    return;
                }

                _camera.transform.position = new Vector3(0f, 0f, -200f);
                _camera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                _cameraStatic = true;
                return;
            }

            _cameraStatic = false;
            _camera.transform.position = new Vector3(_followTarget.CurrentPosition.x, _followTarget.CurrentPosition.y, -20f);
            _camera.transform.rotation = Quaternion.Euler(0f, 0f, _rotateCamera ? _followTarget.CurrentVelocityDirection : 0f);
        }
    }
}